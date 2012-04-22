using System.Collections.Generic;
using LoveSeat;

namespace Bollywell.Hydra.Messaging.MessageFetchers
{
    public interface IMessageFetcher<TMessage> where TMessage : TransportMessage
    {
        IEnumerable<TMessage> MessagesAfterIdBeforeSeq(CouchDatabase db, IMessageId startId, long lastSeq);
        IEnumerable<TMessage> MessagesInSet(CouchDatabase db, IEnumerable<IMessageId> messageIds);
    }
}
