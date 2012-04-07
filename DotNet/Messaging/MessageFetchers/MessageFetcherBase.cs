using System;
using System.Collections.Generic;
using System.Linq;
using LoveSeat;
using Newtonsoft.Json.Linq;

namespace Bollywell.Hydra.Messaging.MessageFetchers
{
    public abstract class MessageFetcherBase<TMessage> : IMessageFetcher<TMessage> where TMessage : TransportMessage
    {
        private static readonly List<TMessage> EmptyList = new List<TMessage>();
        private readonly IMessageId _startId;
        private bool _firstPoll = true;
        private long _lastSeq = 0;

        protected MessageFetcherBase(IMessageId startId)
        {
            _startId = startId;
        }

        // All messages between fromId and toId inclusive (or CouchValue.MaxValue if toId is absent).
        // If any rows are returned by the view query, call UpdateLastSeq with the returned rows and lastSeq.
        protected abstract IEnumerable<JToken> AllMessagesFrom(IMessageId fromId, IMessageId toId = null);

        public List<TMessage> AllNewMessages()
        {
            List<TMessage> messages;
            if (_firstPoll) {
                _firstPoll = false;
                _lastSeq = GetLastSeq();
                var rows = AllMessagesFrom(_startId);
                if (rows.Any()) { _lastSeq = Math.Max(_lastSeq, rows.Max(row => (long)row["value"])); }
                messages = rows.Select(TransportMessage.Hydrate<TMessage>).ToList();
            } else {
                long lastSeq;
                var changeList = GetChanges(_lastSeq, _startId, out lastSeq);
                _lastSeq = lastSeq;

                if (!changeList.Any()) { return EmptyList; }
                // Fetch messages >= max(minId, _startId) and <= maxId and return the ones in the changes list
                messages = AllMessagesFrom(changeList.Min, changeList.Max).Select(TransportMessage.Hydrate<TMessage>).Where(m => changeList.Contains(m.MessageId)).ToList();
            }

            return messages;

        }

        protected CouchDatabase GetDb()
        {
            var config = Services.GetConfig();
            return new CouchClient(config.HydraServer, 5984, null, null, false, AuthenticationType.Basic).GetDatabase(config.Database);
        }

        private SortedSet<IMessageId> GetChanges(long sinceSeq, IMessageId startId, out long lastSeq)
        {
            // Get changes after sinceSeq, with ids after startId; throw out non-messages e.g. design doc updates; and sort into minId..maxId

            // Loveseat doesn't have a _changes call, so it has to be done like this.
            var changes = GetDb().GetDocument(string.Format("_changes?since={0}", sinceSeq));
            // Changes are returned as 
            // {"results":[{"seq":28312,"id":"04b8dbf49b5d2603","changes":[{"rev":"1-ea426b58321d93c39a3486cc4d55abe2"}]},
            //             ...
            //             {"seq":28313,"id":"_design/mce","changes":[{"rev":"9-4d4ec5b438064ab0d602f2ed2ea9ac34"}]}
            //            ],
            //  "last_seq":28313}
            lastSeq = (long) changes["last_seq"];
            return new SortedSet<IMessageId>(changes["results"].Select(jObj => (string)jObj["id"]).Where(LongMessageId.IsMessageId).Select(id => new LongMessageId(id)).OrderBy(mId => mId)
                .SkipWhile(mId => mId.CompareTo(startId) <= 0));
        }

        private long GetLastSeq()
        {
            // Getting the empty document returns database info as:
            // {"db_name":"hydra","doc_count":499245,"doc_del_count":273331,"update_seq":1045940,"purge_seq":0,"compact_running":false,"disk_size":604704891,"data_size":372365869,
            //  "instance_start_time":"1332323453803000","disk_format_version":6,"committed_update_seq":1045940}
            return (long) GetDb().GetDocument("")["update_seq"];
        }

    }
}
