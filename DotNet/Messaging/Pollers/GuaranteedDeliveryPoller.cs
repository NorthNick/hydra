using Bollywell.Hydra.Messaging.MessageFetchers;

namespace Bollywell.Hydra.Messaging.Pollers
{
    /// <summary>
    /// Ensure all messages get delivered.
    /// </summary>
    public class GuaranteedDeliveryPoller<TMessage> : PollerBase<TMessage> where TMessage : TransportMessage
    {
        public GuaranteedDeliveryPoller(IMessageFetcher<TMessage> messageFetcher) : base(messageFetcher) {}

        protected override void Poll()
        {
            MessageFetcher.AllNewMessages().ForEach(OnMessageInQueue);
        }

    }
}
