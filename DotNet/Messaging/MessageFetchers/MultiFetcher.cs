using System.Collections.Generic;
using System.Linq;
using LoveSeat.Interfaces;

namespace Bollywell.Hydra.Messaging.MessageFetchers
{
    public class MultiFetcher<TMessage> : IMessageFetcher<TMessage> where TMessage : TransportMessage
    {
        private readonly IMessageFetcher<TMessage>[] _fetchers;

        public MultiFetcher(params IMessageFetcher<TMessage>[] fetchers)
        {
            _fetchers = fetchers;
        }

        public IEnumerable<TMessage> MessagesAfterIdBeforeSeq(IDocumentDatabase db, IMessageId startId, long lastSeq)
        {
            return _fetchers.AsParallel().Select(f => f.MessagesAfterIdBeforeSeq(db, startId, lastSeq)).Merge();
        }

        public IEnumerable<TMessage> MessagesInSet(IDocumentDatabase db, IEnumerable<IMessageId> messageIds)
        {
            return _fetchers.AsParallel().Select(f => f.MessagesInSet(db, messageIds)).Merge();
        }
    }
}
