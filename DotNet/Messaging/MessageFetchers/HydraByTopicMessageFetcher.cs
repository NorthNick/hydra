using Bollywell.Hydra.Messaging.MessageIds;
using LoveSeat;
using LoveSeat.Interfaces;

namespace Bollywell.Hydra.Messaging.MessageFetchers
{
    public class HydraByTopicMessageFetcher : MessageFetcherBase<HydraMessage>
    {
        private readonly string _topic;
        private readonly IKeyOptions _endKey;

        public HydraByTopicMessageFetcher(string topic)
        {
            _topic = topic;
            _endKey = new KeyOptions(_topic, CouchValue.MaxValue);
        }

        protected override string ViewName { get { return "broadcastMessages"; } }

        // The broadcastMessages view is indexed on [topic, id]
        protected override IKeyOptions MessageKey(IMessageId id) { return new KeyOptions(_topic, id.ToDocId()); }
        protected override IKeyOptions EndKey() { return _endKey; }

    }
}
