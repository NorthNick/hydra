using System.Collections.Generic;

namespace Bollywell.Hydra.Messaging.MessageFetchers
{
    public interface IMessageFetcher<TMessage> where TMessage : TransportMessage
    {
        List<TMessage> AllNewMessages();
    }
}
