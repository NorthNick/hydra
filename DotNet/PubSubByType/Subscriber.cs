using System;
using System.Reactive.Linq;
using Bollywell.Hydra.Messaging;
using Bollywell.Hydra.Messaging.MessageFetchers;
using Bollywell.Hydra.Messaging.Pollers;
using Bollywell.Hydra.Messaging.Serializers;

namespace Bollywell.Hydra.PubSubByType
{
    public class Subscriber<TSub> : IObservable<TSub>, IDisposable
    {
        private readonly IPoller<HydraMessage> _poller;
        private readonly IObservable<TSub> _messageSource;
        private readonly ISerializer<TSub> _serializer;

        public event Action<object, TSub> MessageInQueue;

        public Subscriber(ISerializer<TSub> serializer = null)
        {
            _serializer = serializer ?? new HydraDataContractSerializer<TSub>();
            _poller = new Poller<HydraMessage>(new HydraByTopicMessageFetcher(typeof (TSub).FullName));
            _messageSource = _poller.Select(hydraMessage => _serializer.Deserialize(hydraMessage.Data));
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
                _poller.Dispose();
            }
            // free native resources if there are any.
        }

        #endregion

    }
}
