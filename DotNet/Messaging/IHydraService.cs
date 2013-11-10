using System.Threading.Tasks;
using Shastra.Hydra.Messaging.Attachments;
using Shastra.Hydra.Messaging.Listeners;
using Shastra.Hydra.Messaging.MessageFetchers;
using Shastra.Hydra.Messaging.MessageIds;

namespace Shastra.Hydra.Messaging
{
    public interface IHydraService
    {
        IListener<TMessage> GetListener<TMessage>(IMessageFetcher<TMessage> messageFetcher, IMessageId startId = null, ListenerOptions listenerOptions = null) where TMessage : TransportMessage;
        Task<IMessageId> SendAsync<TMessage>(TMessage message) where TMessage : TransportMessage;
        Task<AttachmentContent> GetAttachmentAsync(Attachment attachment);
        string ServerName { get; }
    }
}