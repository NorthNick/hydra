using LoveSeat;
using LoveSeat.Interfaces;

namespace Bollywell.Hydra.Messaging.MessageFetchers
{
    public class HydraByTopicMessageFetcher : MessageFetcherBase<HydraMessage>
    {
        private readonly string _topic;

        public HydraByTopicMessageFetcher(string topic)
        {
            _topic = topic;
        }

        protected override string DesignDoc { get { return "hydra"; } }
        protected override string ViewName { get { return "broadcastMessages"; } }

        protected override IKeyOptions EndKey() { return new KeyOptions(_topic, CouchValue.MaxValue); }
        protected override IKeyOptions MessageKey(IMessageId id) { return new KeyOptions(_topic, id.ToDocId()); }

    }
}
