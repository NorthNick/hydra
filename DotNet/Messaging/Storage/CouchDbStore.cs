using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;
using Shastra.Hydra.Messaging.Attachments;
using Shastra.Hydra.Messaging.MessageIds;

namespace Shastra.Hydra.Messaging.Storage
{
    internal class CouchDbStore : IStore
    {
        private const string DefaultDatabase = "hydra";
        private const int DefaultPort = 5984;
        private const string DesignDoc = "hydra";
        private readonly string _database;
        private readonly int _port;
        private readonly CouchDbClient _client;
        private readonly string _url;

        public string Name { get; private set; }

        public CouchDbStore(string server, string database = null, int? port = null) : this("", server, database, port)
        {
            Name = NameFromServerDetails(server, _database, _port);
        }

        public CouchDbStore(string name, string server, string database = null, int? port = null)
        {
            _database = database ?? DefaultDatabase;
            _port = port.HasValue ? port.Value : DefaultPort;
            Name = name;
            // This URL checks both that the server is up, and that the view index is up to date
            _url = string.Format("http://{0}:{1}/{2}/_design/{3}/_view/broadcastMessages?limit=0", server, _port, _database, DesignDoc);
            _client = new CouchDbClient(server, _port, _database);
        }

        public IEnumerable<IMessageId> GetChanges(IMessageId startId, long sinceSeq, out long lastSeq)
        {
            // Get changes after sinceSeq, throw out non-messages e.g. design doc updates, and drop messages at or before _startId

            var changes = _client.GetDoc(string.Format("_changes?since={0}", sinceSeq));
            // Changes are returned as 
            // {"results":[{"seq":28312,"id":"04b8dbf49b5d2603","changes":[{"rev":"1-ea426b58321d93c39a3486cc4d55abe2"}]},
            //             ...
            //             {"seq":28313,"id":"_design/mce","changes":[{"rev":"9-4d4ec5b438064ab0d602f2ed2ea9ac34"}]}
            //            ],
            //  "last_seq":28313}
            lastSeq = (long) changes["last_seq"];
            return changes["results"].Select(jObj => (string) jObj["id"]).Where(MessageIdManager.IsMessageId).Select(MessageIdManager.Create)
                    .OrderBy(mId => mId).SkipWhile(mId => mId.CompareTo(startId) <= 0);
        }

        public long GetLastSeq()
        {
            // Getting the empty document returns database info as:
            // {"db_name":"hydra","doc_count":499245,"doc_del_count":273331,"update_seq":1045940,"purge_seq":0,"compact_running":false,"disk_size":604704891,"data_size":372365869,
            //  "instance_start_time":"1332323453803000","disk_format_version":6,"committed_update_seq":1045940}
            return (long)_client.GetDoc("")["update_seq"];
        }

        public async Task<IMessageId> SaveDocAsync(JObject json, IEnumerable<Attachment> attachments = null)
        {
            // TODO: deal with the case where posting fails but raises a CouchDb {error:xxx, reason:xxx} object and not an exception.
            var jobj = await _client.SaveDocAsync(json, attachments).ConfigureAwait(false);
            return MessageIdManager.Create((string)jobj["id"]);
        }

        public IEnumerable<JToken> GetDocs(string viewName, IViewOptions options)
        {
            return _client.View(viewName, options, DesignDoc);
        }

        public async Task<AttachmentContent> GetAttachmentAsync(Attachment attachment)
        {
            var content = await _client.GetDocContentsAsync(string.Format("{0}/{1}", attachment.MessageId.ToDocId(), HttpUtility.UrlEncode(attachment.Name))).ConfigureAwait(false);
            return new AttachmentContent(content);
        }

        public ServerDistanceInfo MeasureDistance()
        {
            bool responseOk = false;
            long elapsed = 0;
            try {
                var timer = Stopwatch.StartNew();
                using (HttpWebResponse response = (HttpWebResponse) WebRequest.Create(_url).GetResponse()) {
                    elapsed = timer.ElapsedMilliseconds;
                    responseOk = response.StatusCode == HttpStatusCode.OK;
                }
            } catch (Exception) {
                // Swallow errors
            }
            return new ServerDistanceInfo { Name = Name, Distance = responseOk ? elapsed : long.MaxValue, IsReachable = responseOk };
        }

        public static string NameFromServerDetails(string server, string database, int port)
        {
            return string.Format("{0}:{1}:{2}", server, port, database);
        }
    }
}
