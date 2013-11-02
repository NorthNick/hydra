using System;
using System.Reactive;
using System.Reactive.Linq;
using Shastra.Hydra.Messaging;
using Shastra.Hydra.Messaging.Listeners;
using Shastra.Hydra.Messaging.MessageFetchers;
using Shastra.Hydra.Messaging.Serializers;

namespace Shastra.Hydra.PubSubByType
{
    /// <summary>
    /// Class for subscribing to Hydra messages of a given type. To be used in conjunction with the Publisher class.
    /// </summary>
    /// <typeparam name="TSub">The type of messages to subscribe to.</typeparam>
    public class Subscriber<TSub> : IObservable<Notification<AugmentedMessage<TSub>>>, IDisposable
    {
        private readonly IListener<HydraMessage> _listener;
        private readonly IObservable<Notification<AugmentedMessage<TSub>>> _messageSource;
        private readonly ISerializer<TSub> _serializer;

        public event Action<object, Notification<AugmentedMessage<TSub>>> MessageInQueue;

        public long BufferDelayMs { get { return _listener.BufferDelayMs; } set { _listener.BufferDelayMs = value; } }
        public long PollIntervalMs { get { return _listener.PollIntervalMs; } set { _listener.PollIntervalMs = value; } }

        /// <summary>
        /// Subscribe to all published messages of type TSub.
        /// </summary>
        /// <param name="hydraService">The Hydra service to use for sending messages.</param>
        /// <param name="serializer">The serializer to use for TSub objects. Defaults to HydraDataContractSerializer.</param>
        /// <param name="topic">The topic to listen for. Defaults to the full name of TSub.</param>
        /// <remarks>The serializer must match the one used by the message publisher.</remarks>
        public Subscriber(IHydraService hydraService, ISerializer<TSub> serializer = null, string topic = null) : this(hydraService, null, serializer, topic) {}

        /// <summary>
        /// Subscribe to all messages of type TSub sent to a specific destination.
        /// </summary>
        /// <param name="hydraService">The Hydra service to use for sending messages.</param>
        /// <param name="thisParty">The message destination (i.e. this app).</param>
        /// <param name="serializer">The serializer to use for TSub objects. Defaults to HydraDataContractSerializer.</param>
        /// <param name="topic">The topic to listen for. Defaults to the full name of TSub.</param>
        /// <remarks>The serializer must match the one used by the message publisher. If thisParty is null, then the subscription will be to all published TSub messages
        /// regardless of destination.</remarks>
        public Subscriber(IHydraService hydraService, string thisParty, ISerializer<TSub> serializer = null, string topic = null)
        {
            _serializer = serializer ?? new HydraDataContractSerializer<TSub>();
            string messageTopic = topic ?? typeof (TSub).FullName;
            if (thisParty == null) {
                _listener = hydraService.GetListener(new HydraByTopicMessageFetcher(messageTopic));
            } else {
                _listener = hydraService.GetListener(new HydraByTopicByDestinationMessageFetcher(messageTopic, thisParty));
            }
            _messageSource = _listener.Select(MessageNotification);
            _messageSource.Subscribe(MessageSourceOnNext);
        }

        private Notification<AugmentedMessage<TSub>> MessageNotification(HydraMessage message)
        {
            try {
                return Notification.CreateOnNext(new AugmentedMessage<TSub>(_serializer.Deserialize(message.Data), message.Attachments));
            } catch (Exception ex) {
                return Notification.CreateOnError<AugmentedMessage<TSub>>(ex);
            }
        }

        private void MessageSourceOnNext(Notification<AugmentedMessage<TSub>> message)
        {
            if (MessageInQueue != null) {
                MessageInQueue(this, message);
            }
        }

        #region Implementation of IObservable<out Notification<AugmentedMessage<TSub>>>

        public IDisposable Subscribe(IObserver<Notification<AugmentedMessage<TSub>>> observer)
        {
            return _messageSource.Subscribe(observer);
        }

        #endregion

        #region Implementation of IDisposable

        // See http://msdn.microsoft.com/en-us/library/ms244737.aspx

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) {
                // free managed resources
                _listener.Dispose();
            }
            // free native resources if there are any.
        }

        #endregion

    }
}
