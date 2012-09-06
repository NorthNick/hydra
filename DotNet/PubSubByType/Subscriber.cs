using System;
using System.Reactive.Linq;
using Bollywell.Hydra.Messaging;
using Bollywell.Hydra.Messaging.Listeners;
using Bollywell.Hydra.Messaging.MessageFetchers;
using Bollywell.Hydra.Messaging.Serializers;

namespace Bollywell.Hydra.PubSubByType
{
    /// <summary>
    /// Class for subscribing to Hydra messages of a given type. To be used in conjunction with the Publisher class.
    /// </summary>
    /// <typeparam name="TSub">The type of messages to subscribe to.</typeparam>
    public class Subscriber<TSub> : IObservable<TSub>, IDisposable
    {
        private readonly IListener<HydraMessage> _listener;
        private readonly IObservable<TSub> _messageSource;
        private readonly ISerializer<TSub> _serializer;

        public event Action<object, TSub> MessageInQueue;

        public long BufferDelayMs { get { return _listener.BufferDelayMs; } set { _listener.BufferDelayMs = value; } }
        public long PollIntervalMs { get { return _listener.PollIntervalMs; } set { _listener.PollIntervalMs = value; } }

        /// <summary>
        /// Subscribe to all published messages of type TSub.
        /// </summary>
        /// <param name="hydraService">The Hydra service to use for sending messages.</param>
        /// <param name="serializer">The serializer to use for TSub objects. Defaults to HydraDataContractSerializer.</param>
        /// <remarks>The serializer must match the one used by the message publisher.</remarks>
        public Subscriber(IHydraService hydraService, ISerializer<TSub> serializer = null) : this(hydraService, null, serializer) {}

        /// <summary>
        /// Subscribe to all messages of type TSub sent to a specific destination.
        /// </summary>
        /// <param name="hydraService">The Hydra service to use for sending messages.</param>
        /// <param name="thisParty">The message destination (i.e. this app).</param>
        /// <param name="serializer">The serializer to use for TSub objects. Defaults to HydraDataContractSerializer.</param>
        /// <remarks>The serializer must match the one used by the message publisher. If thisParty is null, then the subscription will be to all published TSub messages
        /// regardless of destination.</remarks>
        public Subscriber(IHydraService hydraService, string thisParty, ISerializer<TSub> serializer = null)
        {
            _serializer = serializer ?? new HydraDataContractSerializer<TSub>();
            if (thisParty == null) {
                _listener = hydraService.GetListener(new HydraByTopicMessageFetcher(typeof(TSub).FullName));
            } else {
                _listener = hydraService.GetListener(new HydraByTopicByDestinationMessageFetcher(typeof (TSub).FullName, thisParty));
            }
            _messageSource = _listener.Select(hydraMessage => _serializer.Deserialize(hydraMessage.Data));
            _messageSource.Subscribe(MessageSourceOnNext);
        }


        private void MessageSourceOnNext(TSub message)
        {
            if (MessageInQueue != null) {
                MessageInQueue(this, message);
            }
        }

        #region Implementation of IObservable<out TSub>

        public IDisposable Subscribe(IObserver<TSub> observer)
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
