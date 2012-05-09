using System.Collections.Generic;
using LoveSeat.Interfaces;

namespace Bollywell.Hydra.Messaging.MessageFetchers
{
    public interface IMessageFetcher<out TMessage> where TMessage : TransportMessage
    {
        IEnumerable<TMessage> MessagesAfterIdBeforeSeq(IDocumentDatabase db, IMessageId startId, long lastSeq);
        IEnumerable<TMessage> MessagesInSet(IDocumentDatabase db, IEnumerable<IMessageId> messageIds);
    }
}
