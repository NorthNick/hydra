using System.Collections.Generic;
using LoveSeat;
using Newtonsoft.Json.Linq;

namespace Bollywell.Hydra.Messaging.MessageFetchers
{
    public class HydraByTopicByDestinationMessageFetcher : MessageFetcherBase<HydraMessage>
    {
        private readonly string _topic;
        private readonly string _destination;

        /// <summary>
        /// Initialise HydraByTopicByDestinationMessageFetcher
        /// </summary>
        /// <param name="startId"> </param>
        /// <param name="topic">The topic for which to fetch messages</param>
        /// <param name="destination">The destination for which to fetch messages</param>
        public HydraByTopicByDestinationMessageFetcher(IMessageId startId, string topic, string destination) : base(startId)
        {
            _topic = topic;
            _destination = destination;
        }

        protected override IEnumerable<JToken> AllMessagesFrom(IMessageId fromId, IMessageId toId = null)
        {
            // The mceDirectedMessages view is indexed on [topic, destination, id]
            var options = new ViewOptions { IncludeDocs = true, InclusiveEnd = true };
            options.StartKey.Add(_topic, _destination, fromId.ToDocId());
            options.EndKey.Add(_topic, _destination, toId == null ? CouchValue.MaxValue : toId.ToDocId());
            return GetDb().View("directedMessages", options, "hydra").Rows;
        }

    }
}
