using System.Collections.Generic;
using Shastra.Hydra.Messaging.MessageIds;
using Shastra.Hydra.Messaging.Storage;

namespace Shastra.Hydra.Messaging.MessageFetchers
{
    public interface IMessageFetcher<out TMessage> where TMessage : TransportMessage
    {
        // Messages with MessageId > startId and with SeqId <= lastSeq
        IEnumerable<TMessage> MessagesAfterIdUpToSeq(IStore store, IMessageId startId, long lastSeq);
        IEnumerable<TMessage> MessagesInSet(IStore store, IEnumerable<IMessageId> messageIds);
    }
}
