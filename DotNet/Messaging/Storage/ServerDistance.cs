﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Bollywell.Hydra.Messaging.Storage
{
    public class ServerDistance<TServerDistanceInfo> : IObservable<TServerDistanceInfo>, IDisposable where TServerDistanceInfo : class, IServerDistanceInfo
    {
        // Default interval of 20 seconds.
        private const double TimerInterval = 20000;

        private readonly List<string> _servers;
        private readonly Subject<TServerDistanceInfo> _subject = new Subject<TServerDistanceInfo>();
        private IDisposable _poller;
        private readonly Func<string, TServerDistanceInfo> _measureDistance;
        private readonly object _lock = new object();
        private bool _initialisedRaised;

        #region Properties

        /// <summary>
        /// Gets or sets the the interval in milliseconds at which servers are pinged. Default 5 mins.
        /// </summary>
        public double Interval;

        /// <summary>
        /// TServerDistanceInfo item for each server being monitored.
        /// </summary>
        public Dictionary<string, TServerDistanceInfo> ServerInfo { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Class to maintain ping time information to a collection of servers
        /// </summary>
        /// <param name="servers">Names or string representation of IP addresses of the servers to monitor</param>
        /// <param name="measureDistance">Optional function to measure distance to a server</param>
        /// <param name="init">Optional initialisation function, to be run asynchronously before polling starts.</param>
        /// <param name="finishedInitialisationHandler">Optional FinishedInitialisation event handler. Supplying this ensures the event handler is definitely attached before the event is raised.</param>
        public ServerDistance(IEnumerable<string> servers, Func<string, TServerDistanceInfo> measureDistance = null, Action<IEnumerable<string>> init = null, Action<object> finishedInitialisationHandler = null)
        {
            Interval = TimerInterval;
            _servers = servers.ToList();
            _measureDistance = measureDistance ?? MeasureDistance;
            ServerInfo = new Dictionary<string, TServerDistanceInfo>();
            if (finishedInitialisationHandler != null) FinishedInitialisation += finishedInitialisationHandler;
            Task.Factory.StartNew(() => { (init ?? Init)(_servers); Start(); });
        }

        #endregion

        #region Methods

        public void Start()
        {
            lock (_lock) {
                ServerInfo = new Dictionary<string, TServerDistanceInfo>();
            }
            // Poll each server immediately, then with an Interval millisecond gap, each on task pool threads.
            // Note that Generate runs measureDistance immediately, then waits for the timeSelector interval before passing on the result and immediately running measureDistance again. In
            // order to get results quickly timeSelector is zero first time through, then a longer gap, but this means you get two measureDistances close together initially.
            // We use the TaskPool Scheduler, as the observables don't seem to shut down on disposal if we use NewThread.
            _poller = _servers.Select(server => Observable.Generate(true, _ => true, _ => false, _ => _measureDistance(server),
                                                                    isFirst => isFirst ? TimeSpan.Zero : TimeSpan.FromMilliseconds(Interval), TaskPoolScheduler.Default))
                              .Merge().Subscribe(OnDistanceInfo);
        }

        public void Stop()
        {
            if (_poller != null) {
                _poller.Dispose();
                _poller = null;
            }
        }

        public void OnDistanceInfo(TServerDistanceInfo sdi)
        {
            // Ensure that multiple threads do not attempt to update ServerInfo at the same time.
            lock (_lock) {
                ServerInfo[sdi.Name] = sdi;
                // Raise the FinishedInitialisation event just once, when all servers have been polled
                if (!_initialisedRaised && ServerInfo.Count == _servers.Count && FinishedInitialisation != null) {
                    FinishedInitialisation(this);
                    _initialisedRaised = true;
                }
                _subject.OnNext(sdi);
            }
        }

        #endregion

        /// <summary>
        /// Raised when all servers have been polled for the first time.
        /// </summary>
        public event Action<object> FinishedInitialisation;

        protected virtual TServerDistanceInfo MeasureDistance(string server)
        {
            throw new NotImplementedException("MeasureDistance must be overridden or supplied in the constructor");
        }

        protected virtual void Init(IEnumerable<string> servers) {}

        #region Implementation of Interfaces

        public IDisposable Subscribe(IObserver<TServerDistanceInfo> observer)
        {
            return _subject.Subscribe(observer);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) {
                // free managed resources
                Stop();
            }
            // free native resources if there are any.
        }

        #endregion

    }
}
