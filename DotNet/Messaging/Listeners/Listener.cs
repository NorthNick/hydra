﻿using Bollywell.Hydra.Messaging.MessageFetchers;
using Bollywell.Hydra.Messaging.MessageIds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Bollywell.Hydra.Messaging.Storage;

namespace Bollywell.Hydra.Messaging.Listeners
{
    public class Listener<TMessage> : IListener<TMessage> where TMessage : TransportMessage
    {
        private const long DefaultPollIntervalMs = 1000;
        private const long DefaultBufferDelayMs = 0;
        private readonly IProvider _provider;
        private readonly IMessageFetcher<TMessage> _messageFetcher;
        private readonly Subject<TMessage> _subject = new Subject<TMessage>();
        private List<TMessage> _messageBuffer = new List<TMessage>();
        private long _lastSeq;
        private IMessageId _startId;
        private string _server;
        private IStore _store;
        private bool _disposed = false;
        private readonly IDisposable _messageSub;
        private readonly IScheduler _scheduler;

        public long BufferDelayMs { get; set; }
        public long PollIntervalMs { get; set; }

        /// <summary>
        /// The last Id raised to clients. While processing a message, this will be the Id of that message.
        /// </summary>
        public IMessageId LastId { get; private set; }

        /// <summary>
        /// Construct a Listener and start it polling.
        /// </summary>
        /// <param name="provider"> </param>
        /// <param name="messageFetcher">IMessageFetcher with which to poll.</param>
        /// <param name="startId">Only fetch messages with higher id than startId. Defaults to the id corresponding to now.</param>
        /// <param name="listenerOptions">Default values for Listener options.</param>
        /// <param name="scheduler">Scheduler to use for polling. Defaults to TaskPoolScheduler.Default.</param>
        /// <remarks>The polling interval is taken from Service.GetConfig().PollIntervalMs and is dynamic: changes take effect after the next poll.</remarks>
        internal Listener(IProvider provider, IMessageFetcher<TMessage> messageFetcher, IMessageId startId = null, ListenerOptions listenerOptions = null, IScheduler scheduler = null)
        {
            _provider = provider;
            _messageFetcher = messageFetcher;
            _scheduler = scheduler ?? TaskPoolScheduler.Default;
            BufferDelayMs = listenerOptions == null ? DefaultBufferDelayMs : listenerOptions.BufferDelayMs;
            PollIntervalMs = listenerOptions == null || !listenerOptions.PollIntervalMs.HasValue ? DefaultPollIntervalMs : listenerOptions.PollIntervalMs.Value;
            LastId = startId ?? MessageIdManager.Create(_scheduler.Now.UtcDateTime);
            _messageSub = Observable.Generate(true, _ => true, _ => false, _ => OnElapsed(), _ => TimeSpan.FromMilliseconds(PollIntervalMs), _scheduler).
                          SelectMany(messages => messages).Subscribe(OnMessageInQueue);
        }

        #region Polling

        private readonly IEnumerable<TMessage> _noMessages = Enumerable.Empty<TMessage>();

        private IEnumerable<TMessage> OnElapsed()
        {
            if (_disposed) return _noMessages;

            try {
                return Poll();
            } catch (Exception) {
                // TODO: detect what sort of error this was
                _provider.ServerError(_server);
                return _noMessages;
            }
        }

        private IEnumerable<TMessage> Poll()
        {
            var server = _provider.HydraServer;
            if (server != _server) {
                // The server has changed, so reinitialise. As _server is initially null, this will be also be called on the very first poll.
                // TODO: There is a slim chance of the server changing after the call above, and before the GetStore call below, which would be a problem.
                // The solution would be to have a single IProvider call that returns both the server and store (or maybe a version number and store).
                _server = server;
                _store = _provider.GetStore();
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

            var delayedId = MessageIdManager.Create(_scheduler.Now.UtcDateTime.AddMilliseconds(-BufferDelayMs));
            var newMessages = _messageBuffer.TakeWhile(m => m.MessageId.CompareTo(delayedId) <= 0).ToList();
            _messageBuffer.RemoveRange(0, newMessages.Count);
            return newMessages;
        }

        #endregion

        #region MessageInQueue event

        public event Action<object, TMessage> MessageInQueue;

        protected virtual void OnMessageInQueue(TMessage message)
        {
            try {
                LastId = message.MessageId;
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
                _messageSub.Dispose();
                _subject.OnCompleted();
                _subject.Dispose();
                _disposed = true;
            }
            // free native resources if there are any.
        }

        #endregion
    }
}
