using System;
using Bollywell.Hydra.Messaging.Listeners;
using Bollywell.Hydra.Messaging.MessageFetchers;
using Bollywell.Hydra.Messaging.MessageIds;
using Bollywell.Hydra.Messaging.Storage;

namespace Bollywell.Hydra.Messaging
{
    public class HydraService : IHydraService
    {
        private readonly IProvider _provider;

        public ListenerOptions DefaultListenerOptions { get; set; }

        public HydraService(IProvider provider, ListenerOptions defaultListenerOptions = null)
        {
            _provider = provider;
            DefaultListenerOptions = defaultListenerOptions;
        }

        #region Implementation of IHydraService

        public IListener<TMessage> GetListener<TMessage>(IMessageFetcher<TMessage> messageFetcher, IMessageId startId = null, ListenerOptions listenerOptions = null) where TMessage : TransportMessage
        {
            return new Listener<TMessage>(_provider, messageFetcher, startId, listenerOptions ?? DefaultListenerOptions);
        }

        public IMessageId Send<TMessage>(TMessage message) where TMessage : TransportMessage
        {
            bool failed = false;
            while (!failed && !_provider.IsOffline) {
                var store = _provider.GetStore(true);
                if (store == null) {
                    failed = true;
                } else {
                    try {
                        return store.SaveDoc(message.ToJson());
                    }
                    catch (Exception) {
                        _provider.ServerError(store.Name);
                    }
                }
            }
            throw new Exception("HydraService.Send: Error sending message - all servers offline.");
        }
        
        #endregion

    }
}
