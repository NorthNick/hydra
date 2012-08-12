using System.Collections.Generic;
using Bollywell.Hydra.Messaging.MessageIds;

namespace Bollywell.Hydra.Messaging.MessageFetchers
{
    public interface IMessageFetcher<out TMessage> where TMessage : TransportMessage
    {
        IEnumerable<TMessage> MessagesAfterIdBeforeSeq(IStore store, IMessageId startId, long lastSeq);
        IEnumerable<TMessage> MessagesInSet(IStore store, IEnumerable<IMessageId> messageIds);
    }
}
