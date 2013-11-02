using System;
using System.Runtime.Serialization;
using Shastra.Hydra.Messaging.Attachments;
using Shastra.Hydra.Messaging.Listeners;
using Shastra.Hydra.Messaging.MessageFetchers;
using Shastra.Hydra.Messaging.MessageIds;
using Shastra.Hydra.Messaging.Storage;

namespace Shastra.Hydra.Messaging
{
    public class StdHydraService : IHydraService
    {
        private readonly IProvider _provider;

        public ListenerOptions DefaultListenerOptions { get; set; }

        public StdHydraService(IProvider provider, ListenerOptions defaultListenerOptions = null)
        {
            _provider = provider;
            DefaultListenerOptions = defaultListenerOptions;
        }

        #region Implementation of IHydraService

        public IListener<TMessage> GetListener<TMessage>(IMessageFetcher<TMessage> messageFetcher, IMessageId startId = null, ListenerOptions listenerOptions = null) where TMessage : TransportMessage
        {
            return new StdListener<TMessage>(_provider, messageFetcher, startId, listenerOptions ?? DefaultListenerOptions);
        }

        public IMessageId Send<TMessage>(TMessage message) where TMessage : TransportMessage
        {
            return TryStoreMethod(store => store.SaveDoc(message.ToJson(), message.Attachments));
        }

        public AttachmentContent GetAttachment(Attachment attachment)
        {
            return TryStoreMethod(store => store.GetAttachment(attachment));
        }

        public string ServerName { get { return _provider.HydraServer; } }

        #endregion

        private T TryStoreMethod<T>(Func<IStore, T> method)
        {
            var store = _provider.GetStore(true);
            while (store != null) {
                try {
                    return method(store);
                } catch (SerializationException) {
                    // Rethrow error without invoking server error
                    throw;
                } catch (Exception) {
                    // Swallow error and mark server as offline
                    _provider.ServerError(store.Name);
                }
                store = _provider.GetStore(true);
            }
            throw new Exception("StdHydraService.TryStoreMethod: Error - all servers offline.");
        }

    }
}
