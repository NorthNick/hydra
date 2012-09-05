using System;
using Bollywell.Hydra.Messaging.Config;
using Bollywell.Hydra.Messaging.MessageFetchers;
using Bollywell.Hydra.Messaging.MessageIds;
using Bollywell.Hydra.Messaging.Pollers;

namespace Bollywell.Hydra.Messaging
{
    public class HydraService : IHydraService
    {
        private readonly IConfigProvider _configProvider;

        public HydraService(IConfigProvider configProvider)
        {
            _configProvider = configProvider;
        }

        #region Implementation of IHydraService

        public IPoller<TMessage> GetPoller<TMessage>(IMessageFetcher<TMessage> messageFetcher, IMessageId startId = null, long bufferDelayMs = 0) where TMessage : TransportMessage
        {
            return new Poller<TMessage>(_configProvider, messageFetcher, startId, bufferDelayMs);
        }

        public IMessageId Send<TMessage>(TMessage message) where TMessage : TransportMessage
        {
            // TODO: use the "get database and server together" call suggested in Poller
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
