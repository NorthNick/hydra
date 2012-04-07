using System;
using System.Reactive.Subjects;
using System.Threading;
using Bollywell.Hydra.Messaging.MessageFetchers;

namespace Bollywell.Hydra.Messaging.Pollers
{
    public abstract class PollerBase<TMessage> : IPoller<TMessage> where TMessage : TransportMessage
    {
        private const int DefaultTimerInterval = 10000;
        protected IMessageFetcher<TMessage> MessageFetcher;
        protected readonly Timer Timer;
        private bool _disposed = false;
        private readonly Subject<TMessage> _subject = new Subject<TMessage>();

        protected PollerBase(IMessageFetcher<TMessage> messageFetcher)
        {
            MessageFetcher = messageFetcher;
            // Set timer to fire just once
            Timer = new Timer(TimerOnElapsed, null, Services.GetConfig().PollIntervalMs ?? DefaultTimerInterval, Timeout.Infinite);
        }

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

        protected abstract void Poll();

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