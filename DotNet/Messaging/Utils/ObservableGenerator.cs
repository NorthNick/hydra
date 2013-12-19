using System;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;

namespace Shastra.Hydra.Messaging.Utils
{
    class ObservableGenerator<T> : IObservable<T>, IDisposable
    {
        private readonly long _repeat;
        private readonly Func<T> _valueGenerator;
        private readonly IScheduler _scheduler;
        private readonly Subject<T> _subject = new Subject<T>();
        private readonly object _startLock = new object();
        private bool _started = false;
        private bool _disposed = false;

        public ObservableGenerator(long repeat, Func<T> valueGenerator, IScheduler scheduler)
        {
            _repeat = repeat;
            _valueGenerator = valueGenerator;
            _scheduler = scheduler;
        }

        #region Implementation of IObservable<out T>

        public IDisposable Subscribe(IObserver<T> observer)
        {
            lock (_startLock) {
                if (!_started) {
                    _scheduler.Schedule(TimeSpan.FromMilliseconds(_repeat), NextVal);
                    _started = true;
                }
            }
            return _subject.Subscribe(observer);
        }

        private void NextVal()
        {
            if (!_disposed) {
                _subject.OnNext(_valueGenerator());
                _scheduler.Schedule(TimeSpan.FromMilliseconds(_repeat), NextVal);
            }
        }

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) {
                // free managed resources
                _subject.OnCompleted();
                _subject.Dispose();
                _disposed = true;
            }
            // free native resources if there are any.
        }

        #endregion
    }
}
