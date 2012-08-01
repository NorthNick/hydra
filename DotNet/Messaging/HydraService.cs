using Bollywell.Hydra.Messaging.Config;
using Bollywell.Hydra.Messaging.MessageFetchers;
using Bollywell.Hydra.Messaging.MessageIds;
using Bollywell.Hydra.Messaging.Pollers;

namespace Bollywell.Hydra.Messaging
{
    public class HydraService : IHydraService
    {
        private readonly IConfigProvider _configProvider;
        private readonly long _bufferDelayMs;

        public HydraService(IConfigProvider configProvider, long bufferDelayMs = 0)
        {
            _configProvider = configProvider;
            _bufferDelayMs = bufferDelayMs;
        }

        #region Implementation of IHydraService

        public IPoller<TMessage> GetPoller<TMessage>(IMessageFetcher<TMessage> messageFetcher, IMessageId startId = null, long bufferDelayMs = 0) where TMessage : TransportMessage
        {
            return new Poller<TMessage>(_configProvider, messageFetcher, startId, bufferDelayMs);
        }

        public void Send<TMessage>(TMessage message) where TMessage : TransportMessage
        {
            message.Send(_configProvider);
        }

        public string ServerName
        {
            get { return _configProvider.HydraServer; }
        }

        #endregion

    }
}
