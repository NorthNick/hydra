using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Bollywell.Hydra.Messaging.Config;
using Bollywell.Hydra.Messaging.MessageFetchers;
using Bollywell.Hydra.Messaging.MessageIds;

namespace Bollywell.Hydra.Messaging.Pollers
{
    public class Poller<TMessage> : IPoller<TMessage> where TMessage : TransportMessage
    {
        private const int DefaultTimerInterval = 10000;
        private readonly Timer _timer;
        private readonly IConfigProvider _configProvider;
        private readonly IMessageFetcher<TMessage> _messageFetcher;
        private readonly Subject<TMessage> _subject = new Subject<TMessage>();
        private List<TMessage> _messageBuffer = new List<TMessage>();
        private long _lastSeq;
        private IMessageId _startId;
        private string _server;
        private IStore _store;
        private bool _disposed = false;

        public long BufferDelayMs { get; set; }

        /// <summary>
        /// The last Id raised to clients. While processing a message, this will be the Id of that message.
        /// </summary>
        public IMessageId LastId { get; private set; }

        /// <summary>
        /// Construct a Poller and start it polling.
        /// </summary>
        /// <param name="configProvider"> </param>
        /// <param name="messageFetcher">IMessageFetcher with which to poll.</param>
        /// <param name="startId">Only fetch messages with higher id than startId. Defaults to the id corresponding to now.</param>
        /// <param name="bufferDelayMs">Buffer messages for this many ms to allow late arriving messages to be sorted into order. Defaults to 0.</param>
        /// <remarks>The polling interval is taken from Service.GetConfig().PollIntervalMs and is dynamic: changes take effect after the next poll.</remarks>
        public Poller(IConfigProvider configProvider, IMessageFetcher<TMessage> messageFetcher, IMessageId startId = null, long bufferDelayMs = 0)
        {
            _configProvider = configProvider;
            _messageFetcher = messageFetcher;
            BufferDelayMs = bufferDelayMs;
            LastId = startId ?? MessageIdManager.Create(DateTime.UtcNow);
            // Set timer to fire just once
            _timer = new Timer(TimerOnElapsed, null, _configProvider.PollIntervalMs ?? DefaultTimerInterval, Timeout.Infinite);
        }

        #region Polling

        private void TimerOnElapsed(object state)
        {
            if (_disposed) return;

            // Disable the timer while polling. Poll on a background thread (as that's where the timer fires), then re-enable the timer when done.
            // Do it all inside a try..finally so that the timer gets restarted no matter what happens.
            try {
                Poll();
            } catch (Exception) {
                // TODO: detect what sort of error this was
                _configProvider.ServerError(_server);
            } finally {
                _timer.Change(_configProvider.PollIntervalMs ?? DefaultTimerInterval, Timeout.Infinite);
            }
        }

        private void Poll()
        {
            var server = _configProvider.HydraServer;
            if (server != _server) {
                // The server has changed, so reinitialise. As _server is initially null, this will be also be called on the very first poll.
                // TODO: There is a slim chance of the server changing after the call above, and before the GetDb call below, which would be a problem.
                // The solution would be to have a single IConfigProvider call that returns both the server and db (or maybe a version number and db).
                _server = server;
                _store = _configProvider.GetStore();
                _startId = LastId;
                // Populate _messageBuffer from _startId
                _lastSeq = _store.GetLastSeq();
                // Fetch messages from _startId, ignoring ones after _lastSeq.
                _messageBuffer = _messageFetcher.MessagesAfterIdUpToSeq(_store, _startId, _lastSeq).ToList();
            } else {
                // Get changes from _lastSeq. Fetch messages in the change set and put in _messageBuffer.
                var changes = _store.GetChanges(_startId, _lastSeq, out _lastSeq).ToList();
                if (changes.Any()) _messageBuffer = new List<IEnumerable<TMessage>> {_messageBuffer, _messageFetcher.MessagesInSet(_store, changes)}.Merge().ToList();
            }

            var delayedId = MessageIdManager.Create(DateTime.UtcNow.AddMilliseconds(-BufferDelayMs));
            var newMessages = _messageBuffer.TakeWhile(m => m.MessageId.CompareTo(delayedId) <= 0).ToList();
            foreach (var message in newMessages) {
                // Track the last message processed in case we swap servers.
                LastId = message.MessageId;
                OnMessageInQueue(message);
            }
            _messageBuffer = _messageBuffer.Skip(newMessages.Count).ToList();
        }

        #endregion

        #region MessageInQueue event

        public event Action<object, TMessage> MessageInQueue;

        protected virtual void OnMessageInQueue(TMessage message)
        {
            try {
                _subject.OnNext(message);
                if (MessageInQueue != null) MessageInQueue(this, message);
            } catch (Exception) {
                // Swallow errors so that an error processing one message does not stop others from being processed.
                // TODO: log error
            }
        }

        #endregion

        # region Implementation of IObservable<TMessage>

        public IDisposable Subscribe(IObserver<TMessage> observer)
        {
            return _subject.Subscribe(observer);
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
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _subject.OnCompleted();
                _subject.Dispose();
                _disposed = true;
            }
            // free native resources if there are any.
        }

        #endregion
    }
}
