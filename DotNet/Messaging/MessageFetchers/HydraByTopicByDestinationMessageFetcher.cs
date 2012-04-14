using LoveSeat;
using LoveSeat.Interfaces;

namespace Bollywell.Hydra.Messaging.MessageFetchers
{
    public class HydraByTopicByDestinationMessageFetcher : MessageFetcherBase<HydraMessage>
    {
        private readonly string _topic;
        private readonly string _destination;

        /// <summary>
        /// Initialise HydraByTopicByDestinationMessageFetcher
        /// </summary>
        /// <param name="topic">The topic for which to fetch messages</param>
        /// <param name="destination">The destination for which to fetch messages</param>
        public HydraByTopicByDestinationMessageFetcher(string topic, string destination)
        {
            _topic = topic;
            _destination = destination;
        }

        protected override string DesignDoc { get { return "hydra"; } }
        protected override string ViewName { get { return "directedMessages"; } }

        protected override IKeyOptions EndKey() { return new KeyOptions(_topic, _destination, CouchValue.MaxValue); }
        protected override IKeyOptions MessageKey(IMessageId id) { return new KeyOptions(_topic, _destination, id.ToDocId()); }

    }
}
