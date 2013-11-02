using Shastra.Hydra.Messaging.Attachments;
using Shastra.Hydra.Messaging.Listeners;
using Shastra.Hydra.Messaging.MessageFetchers;
using Shastra.Hydra.Messaging.MessageIds;

namespace Shastra.Hydra.Messaging
{
    public interface IHydraService
    {
        IListener<TMessage> GetListener<TMessage>(IMessageFetcher<TMessage> messageFetcher, IMessageId startId = null, ListenerOptions listenerOptions = null) where TMessage : TransportMessage;
        IMessageId Send<TMessage>(TMessage message) where TMessage : TransportMessage;
        AttachmentContent GetAttachment(Attachment attachment);
        string ServerName { get; }
    }
}