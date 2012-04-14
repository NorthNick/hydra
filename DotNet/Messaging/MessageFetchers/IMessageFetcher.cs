using System.Collections.Generic;

namespace Bollywell.Hydra.Messaging.MessageFetchers
{
    public interface IMessageFetcher<TMessage> where TMessage : TransportMessage
    {
        IEnumerable<TMessage> MessagesFromIdBeforeSeq(IMessageId startId, long lastSeq);
        IEnumerable<TMessage> MessagesInSet(IEnumerable<IMessageId> messageIds);
    }
}
