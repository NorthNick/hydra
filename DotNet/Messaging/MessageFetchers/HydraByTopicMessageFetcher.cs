using System.Collections.Generic;
using LoveSeat;
using Newtonsoft.Json.Linq;

namespace Bollywell.Hydra.Messaging.MessageFetchers
{
    public class HydraByTopicMessageFetcher : MessageFetcherBase<HydraMessage>
    {
        private readonly string _topic;

        public HydraByTopicMessageFetcher(IMessageId startId, string topic) : base(startId)
        {
            _topic = topic;
        }

        protected override IEnumerable<JToken> AllMessagesFrom(IMessageId fromId, IMessageId toId = null)
        {
            // The mceBroadcastMessages view is indexed on [topic, id]
            var options = new ViewOptions { IncludeDocs = true, InclusiveEnd = true };
            options.StartKey.Add(_topic, fromId.ToDocId());
            options.EndKey.Add(_topic, toId == null ? CouchValue.MaxValue : toId.ToDocId());
            return GetDb().View("broadcastMessages", options, "hydra").Rows;
        }
    }
}
