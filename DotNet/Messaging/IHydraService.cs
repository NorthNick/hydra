using Bollywell.Hydra.Messaging.Listeners;
using Bollywell.Hydra.Messaging.MessageFetchers;
using Bollywell.Hydra.Messaging.MessageIds;

namespace Bollywell.Hydra.Messaging
{
    public interface IHydraService
    {
        IListener<TMessage> GetListener<TMessage>(IMessageFetcher<TMessage> messageFetcher, IMessageId startId = null, ListenerOptions listenerOptions = null) where TMessage : TransportMessage;
        IMessageId Send<TMessage>(TMessage message) where TMessage : TransportMessage;
        string ServerName { get; }
    }
}