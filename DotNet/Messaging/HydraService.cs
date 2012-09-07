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
        private readonly ListenerOptions _listenerOptions;

        public HydraService(IProvider provider, ListenerOptions listenerOptions = null)
        {
            _provider = provider;
            _listenerOptions = listenerOptions;
        }

        #region Implementation of IHydraService

        public IListener<TMessage> GetListener<TMessage>(IMessageFetcher<TMessage> messageFetcher, IMessageId startId = null, ListenerOptions listenerOptions = null) where TMessage : TransportMessage
        {
            return new Listener<TMessage>(_provider, messageFetcher, startId, listenerOptions ?? _listenerOptions);
        }

        public IMessageId Send<TMessage>(TMessage message) where TMessage : TransportMessage
        {
            // TODO: use the "get database and server together" call suggested in Listener
            try {
                return _provider.GetStore().SaveDoc(message.ToJson());
            }
            catch (Exception ex) {
                _provider.ServerError(_provider.HydraServer);
                throw new Exception("HydraService.Send: error sending message. " + ex.Message, ex);
            }
        }
        
        public string ServerName
        {
            get { return _provider.HydraServer; }
        }

        #endregion

    }
}
