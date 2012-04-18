using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Bollywell.Hydra.Messaging.MessageFetchers;

namespace Bollywell.Hydra.Messaging.Pollers
{
    public class Poller<TMessage> : IPoller<TMessage> where TMessage : TransportMessage
    {
        private const int DefaultTimerInterval = 10000;
        protected IMessageFetcher<TMessage> MessageFetcher;
        protected readonly Timer Timer;
        private bool _disposed = false;
        private readonly Subject<TMessage> _subject = new Subject<TMessage>();

        private List<TMessage> _messageBuffer = new List<TMessage>();
        private readonly IMessageId _startId;
        private bool _firstPoll = true;
        private long _lastSeq;
        private readonly double _bufferDelayMs;

        public Poller(IMessageFetcher<TMessage> messageFetcher, long bufferDelayMs = 0, IMessageId startId = null)
        {
            MessageFetcher = messageFetcher;
            _bufferDelayMs = bufferDelayMs;
            _startId = startId ?? TransportMessage.MessageIdForDate(DateTime.UtcNow);
            // Set timer to fire just once
            Timer = new Timer(TimerOnElapsed, null, Services.GetConfig().PollIntervalMs ?? DefaultTimerInterval, Timeout.Infinite);
        }

        #region Polling

        private void TimerOnElapsed(object state)
        {
            if (_disposed) return;

            // Disable the timer while polling. Poll on a background thread, then reenable the timer when done.
            // Do it all inside a try..finally so that the timer gets restarted no matter what happens.
            try {
                Poll();
            } catch (Exception) {
                // TODO: log error
            } finally {
                Timer.Change(Services.GetConfig().PollIntervalMs ?? DefaultTimerInterval, Timeout.Infinite);
            }
        }

        private void Poll()
        {
            if (_firstPoll) {
                // Populate _messageBuffer from _startId
                _lastSeq = GetLastSeq();
                // Fetch messages from _startId, ignoring ones after _lastSeq.
                _messageBuffer = MessageFetcher.MessagesFromIdBeforeSeq(_startId, _lastSeq).ToList();
                _firstPoll = false;
            } else {
                // Get changes from _lastSeq. Fetch messages in the change set and put in _messageBuffer.
                var changes = GetChanges(_lastSeq, out _lastSeq).ToList();
                if (changes.Any()) _messageBuffer = new List<IEnumerable<TMessage>> {_messageBuffer, MessageFetcher.MessagesInSet(changes)}.Merge().ToList();
            }

            var delayedId = TransportMessage.MessageIdForDate(DateTime.UtcNow.AddMilliseconds(-_bufferDelayMs));
            var newMessages = _messageBuffer.TakeWhile(m => m.MessageId.CompareTo(delayedId) <= 0);
            // Update _messageBuffer before raising message events so that errors in processing them do not prevent the update.
            _messageBuffer = _messageBuffer.SkipWhile(m => m.MessageId.CompareTo(delayedId) <= 0).ToList();
            foreach (var message in newMessages) {
                OnMessageInQueue(message);
            }
        }

        private static IEnumerable<IMessageId> GetChanges(long sinceSeq, out long lastSeq)
        {
            // Get changes after sinceSeq, and throw out non-messages e.g. design doc updates.

            // Loveseat doesn't have a _changes call, so it has to be done like this.
            var changes = Services.GetDb().GetDocument(string.Format("_changes?since={0}", sinceSeq));
            // Changes are returned as 
            // {"results":[{"seq":28312,"id":"04b8dbf49b5d2603","changes":[{"rev":"1-ea426b58321d93c39a3486cc4d55abe2"}]},
            //             ...
            //             {"seq":28313,"id":"_design/mce","changes":[{"rev":"9-4d4ec5b438064ab0d602f2ed2ea9ac34"}]}
            //            ],
            //  "last_seq":28313}
            lastSeq = (long) changes["last_seq"];
            return changes["results"].Select(jObj => (string) jObj["id"]).Where(LongMessageId.IsMessageId).Select(id => new LongMessageId(id)).OrderBy(mId => mId);
        }

        private static long GetLastSeq()
        {
            // Getting the empty document returns database info as:
            // {"db_name":"hydra","doc_count":499245,"doc_del_count":273331,"update_seq":1045940,"purge_seq":0,"compact_running":false,"disk_size":604704891,"data_size":372365869,
            //  "instance_start_time":"1332323453803000","disk_format_version":6,"committed_update_seq":1045940}
            return (long) Services.GetDb().GetDocument("")["update_seq"];
        }

        #endregion

        #region MessageInQueue event

        public event Action<object, TMessage> MessageInQueue;

        protected virtual void OnMessageInQueue(TMessage message)
        {
            _subject.OnNext(message);
            if (MessageInQueue == null) return;
            try {
                MessageInQueue(this, message);
            } catch (Exception) {
                // Swallow errors so that an error processing one message does not stop others from being processed.
                // TODO: log error
            }
        }

        #endregion

        public IDisposable Subscribe(IObserver<TMessage> observer)
        {
            return _subject.Subscribe(observer);
        }

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
                Timer.Change(Timeout.Infinite, Timeout.Infinite);
                _subject.OnCompleted();
                _subject.Dispose();
                _disposed = true;
            }
            // free native resources if there are any.
        }

        #endregion
    }
}
