using System.Collections.Generic;
using System.Linq;
using Bollywell.Hydra.Messaging.MessageIds;
using Bollywell.Hydra.Messaging.Storage;

namespace Bollywell.Hydra.Messaging.MessageFetchers
{
    public class MultiFetcher<TMessage> : IMessageFetcher<TMessage> where TMessage : TransportMessage
    {
        private readonly IMessageFetcher<TMessage>[] _fetchers;

        public MultiFetcher(params IMessageFetcher<TMessage>[] fetchers)
        {
            _fetchers = fetchers;
        }

        public IEnumerable<TMessage> MessagesAfterIdUpToSeq(IStore store, IMessageId startId, long lastSeq)
        {
            return _fetchers.AsParallel().Select(f => f.MessagesAfterIdUpToSeq(store, startId, lastSeq)).Merge();
        }

        public IEnumerable<TMessage> MessagesInSet(IStore store, IEnumerable<IMessageId> messageIds)
        {
            return _fetchers.AsParallel().Select(f => f.MessagesInSet(store, messageIds)).Merge();
        }
    }
}
