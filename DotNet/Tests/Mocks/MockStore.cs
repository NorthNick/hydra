using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Concurrency;
using System.Web;
using Newtonsoft.Json.Linq;
using Shastra.Hydra.Messaging.Attachments;
using Shastra.Hydra.Messaging.MessageIds;
using Shastra.Hydra.Messaging.Storage;

namespace Shastra.Hydra.Tests.Mocks
{
    internal class MockStore : IStore
    {
        private readonly string _suffix;
        private readonly List<DocInfo> _docInfos = new List<DocInfo>();
        private readonly Dictionary<string, JToken> _docs = new Dictionary<string, JToken>();
        private readonly Dictionary<string, Dictionary<string, AttachmentContent>> _attachments = new Dictionary<string, Dictionary<string, AttachmentContent>>();
        private readonly IScheduler _scheduler;
        private readonly object _lock = new object();
        private long _lastMicroseconds = 0;

        public string Name { get; private set; }

        public MockStore(string name, string suffix = null, IScheduler scheduler = null)
        {
            Name = name;
            _suffix = suffix ?? "";
            _scheduler = scheduler ?? Scheduler.CurrentThread;
        }

        public IEnumerable<IMessageId> GetChanges(IMessageId startId, long sinceSeq, out long lastSeq)
        {
            // Get changes after sinceSeq, and drop messages at or before _startId
            Replicate();
            lastSeq = GetLastSeq();
            return Enumerable.Range((int) sinceSeq + 1, (int) (lastSeq - sinceSeq)).Select(seqId => MessageIdManager.Create(_docInfos[seqId].DocId)).
                OrderBy(mId => mId).SkipWhile(mId => mId.CompareTo(startId) <= 0);
        }

        public long GetLastSeq()
        {
            // SeqId is the index of the last element of _docs
            return _docs.Count - 1;
        }

        // Attachments are currently ignored
        public IMessageId SaveDoc(JObject json, IEnumerable<Attachment> attachments = null)
        {
            Replicate();

            DateTime idDate;
            string docId;
            // Allow the default DocId to be overridden in a TestHydraMessage
            if (json["idDate"] != null) {
                idDate = DateTime.Parse((string) json["idDate"]);
                // This bypasses the uniqueness check, so don't specify the same date twice.
                docId = ((long) (idDate.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds) * 1000).ToString("x14") + _suffix;
            } else {
                idDate = _scheduler.Now.UtcDateTime;
                docId = CreateDocId(idDate);
            }
            JToken stored = new JObject(new JProperty("id", docId), new JProperty("doc", json));
            var docInfo = new DocInfo(docId, (string) json["topic"], (string) json["destination"], idDate);
            // Lock out other SaveDoc calls so that we definitely get the right list length
            lock (_lock) {
                stored["value"] = _docInfos.Count;
                _docInfos.Add(docInfo);
            }
            _docs[docId] = stored;
            SaveAttachments(docId, json, attachments);
            return MessageIdManager.Create(docId);
        }

        public AttachmentContent GetAttachment(Attachment attachment)
        {
            return _attachments[attachment.MessageId.ToDocId()][attachment.Name];
        }

        public ServerDistanceInfo MeasureDistance()
        {
            return new ServerDistanceInfo { Name = Name, Distance = 10, IsReachable = true };
        }

        public IEnumerable<JToken> GetDocs(string viewName, IViewOptions options)
        {
            // We are only interested in viewNames "broadcastMessages" and "directedMessages"
            // options always has IncludeDocs=true and either:
            //   Keys = list of keys to get, each all [topic, couchId] or all [topic, destination, couchId] respectively. Or
            //   StartKey = [topic, couchId] or [topic, destination, couchId] and EndKey = [topic, maxvalue] or [topic, destination, maxvalue] respectively

            // Extract topic and possibly destination from here
            JArray filterArray;
            string startId = null;
            HashSet<string> keySet = null;
            if (options.StartKey != null) {
                filterArray = JArray.Parse(HttpUtility.UrlDecode(options.StartKey.ToString()));
                startId = (string) filterArray.Last;
            } else {
                var keyArray = options.Keys.Select(key => JArray.Parse(HttpUtility.UrlDecode(key.ToString()))).ToList();
                // Empty array of keys for some reason
                if (!options.Keys.Any()) return Enumerable.Empty<JToken>();
                filterArray = keyArray[0];
                keySet = new HashSet<string>(keyArray.Select(key => (string) key.Last));
            }
            string topic = (string) filterArray[0];

            // Find the docs matching topic and possibly destination
            IEnumerable<DocInfo> filteredDocInfos;
            if (viewName == "broadcastMessages") {
                filteredDocInfos = _docInfos.Where(di => di.Topic == topic && di.Destination == null);
            } else if (viewName == "directedMessages") {
                string destination = (string) filterArray[1];
                filteredDocInfos = _docInfos.Where(di => di.Topic == topic && di.Destination == destination);
            } else {
                throw new Exception(string.Format("MockStore.GetDocs: View name {0} is not implemented.", viewName));
            }

            // Return either those on or after startId, or in keySet
            if (startId != null) {
                filteredDocInfos = filteredDocInfos.Where(di => String.Compare(di.DocId, startId, StringComparison.InvariantCulture) >= 0);
            } else {
                filteredDocInfos = filteredDocInfos.Where(di => keySet.Contains(di.DocId));
            }
            return filteredDocInfos.OrderBy(di => di.DocId).Select(di => _docs[di.DocId]);
        }

        private void Replicate()
        {
            // For now do nothing
        }

        /// <summary>
        /// Create a docid based on the date, incorporating the suffix, and ensuring monotonicity and uniqueness.
        /// </summary>
        /// <param name="date">The UTC date</param>
        /// <returns>A unique docid, higher in the sort order than any previous one</returns>
        private string CreateDocId(DateTime date)
        {
            long microseconds = (long) (date.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds) * 1000;
            _lastMicroseconds = (microseconds > _lastMicroseconds) ? microseconds : _lastMicroseconds + 1;
            return _lastMicroseconds.ToString("x14") + _suffix;
        }

        private void SaveAttachments(string docId, JObject json, IEnumerable<Attachment> attachments)
        {
            if (attachments == null || !attachments.Any()) return;

            AddAttachmentStubs(json, attachments);
            _attachments[docId] = attachments.ToDictionary(att => att.Name, att => new AttachmentContent(att.ToHttpContent()));
        }

        private static void AddAttachmentStubs(JObject json, IEnumerable<Attachment> attachments)
        {
            // The attachments are turned into an _attachments property on the JSON. The value is an object having one property
            // per attachment, whose name is the attachment name and whose value is as in JsonAttachment below.
            var jsonParts = new JObject();
            foreach (var attachment in attachments) {
                jsonParts.Add(new JProperty(attachment.Name, JsonAttachment(attachment.ContentType, attachment.DataLength())));
            }
            json.Add(new JProperty("_attachments", jsonParts));
        }

        private static JObject JsonAttachment(string contentType, long length)
        {
            return new JObject(
                new JProperty("follows", true),
                new JProperty("content_type", contentType),
                new JProperty("length", length));
        }
    }
}
