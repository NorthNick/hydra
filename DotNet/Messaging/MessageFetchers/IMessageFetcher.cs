using System.Collections.Generic;
using Bollywell.Hydra.Messaging.MessageIds;
using Bollywell.Hydra.Messaging.Storage;

namespace Bollywell.Hydra.Messaging.MessageFetchers
{
    public interface IMessageFetcher<out TMessage> where TMessage : TransportMessage
    {
        // Messages with MessageId > startId and with SeqId <= lastSeq
        IEnumerable<TMessage> MessagesAfterIdUpToSeq(IStore store, IMessageId startId, long lastSeq);
        IEnumerable<TMessage> MessagesInSet(IStore store, IEnumerable<IMessageId> messageIds);
    }
}
