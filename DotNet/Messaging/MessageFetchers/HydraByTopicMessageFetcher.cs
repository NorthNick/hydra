using Shastra.Hydra.Messaging.MessageIds;
using Shastra.Hydra.Messaging.Storage;

namespace Shastra.Hydra.Messaging.MessageFetchers
{
    public class HydraByTopicMessageFetcher : MessageFetcherBase<HydraMessage>
    {
        private readonly string _topic;
        private readonly IKeyOptions _endKey;

        public HydraByTopicMessageFetcher(string topic)
        {
            _topic = topic;
            _endKey = new KeyOptions(_topic, KeyOptions.MaxValue);
        }

        protected override string ViewName { get { return "broadcastMessages"; } }

        // The broadcastMessages view is indexed on [topic, id]
        protected override IKeyOptions MessageKey(IMessageId id) { return new KeyOptions(_topic, id.ToDocId()); }
        protected override IKeyOptions EndKey() { return _endKey; }

    }
}
