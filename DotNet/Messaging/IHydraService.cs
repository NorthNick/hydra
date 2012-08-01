using Bollywell.Hydra.Messaging.MessageFetchers;
using Bollywell.Hydra.Messaging.MessageIds;
using Bollywell.Hydra.Messaging.Pollers;

namespace Bollywell.Hydra.Messaging
{
    public interface IHydraService
    {
        IPoller<TMessage> GetPoller<TMessage>(IMessageFetcher<TMessage> messageFetcher, IMessageId startId = null, long bufferDelayMs = 0) where TMessage : TransportMessage;
        void Send<TMessage>(TMessage message) where TMessage : TransportMessage;
        string ServerName { get; }
    }
}