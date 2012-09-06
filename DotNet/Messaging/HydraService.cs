using System;
using Bollywell.Hydra.Messaging.Config;
using Bollywell.Hydra.Messaging.Listeners;
using Bollywell.Hydra.Messaging.MessageFetchers;
using Bollywell.Hydra.Messaging.MessageIds;

namespace Bollywell.Hydra.Messaging
{
    public class HydraService : IHydraService
    {
        private readonly IConfigProvider _configProvider;
        private readonly ListenerOptions _listenerOptions;

        public HydraService(IConfigProvider configProvider, ListenerOptions listenerOptions = null)
        {
            _configProvider = configProvider;
            _listenerOptions = listenerOptions;
        }

        #region Implementation of IHydraService

        public IListener<TMessage> GetListener<TMessage>(IMessageFetcher<TMessage> messageFetcher, IMessageId startId = null, ListenerOptions listenerOptions = null) where TMessage : TransportMessage
        {
            return new Listener<TMessage>(_configProvider, messageFetcher, startId, listenerOptions ?? _listenerOptions);
        }

        public IMessageId Send<TMessage>(TMessage message) where TMessage : TransportMessage
        {
            // TODO: use the "get database and server together" call suggested in Listener
            try {
                return _configProvider.GetStore().SaveDoc(message.ToJson());
            }
            catch (Exception ex) {
                _configProvider.ServerError(_configProvider.HydraServer);
                throw new Exception("HydraService.Send: error sending message. " + ex.Message, ex);
            }
        }
        
        public string ServerName
        {
            get { return _configProvider.HydraServer; }
        }

        #endregion

    }
}
