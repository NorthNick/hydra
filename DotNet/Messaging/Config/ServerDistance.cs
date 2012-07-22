using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Bollywell.Hydra.Messaging.Config
{
    internal class ServerDistance<TServerDistanceInfo> : IObservable<TServerDistanceInfo>, IDisposable where TServerDistanceInfo : class, IServerDistanceInfo
    {
        // Timer interval of 5 mins in milliseconds.
        private const double TimerInterval = 5 * 60 * 1000;

        private readonly List<string> _servers;
        private readonly Subject<TServerDistanceInfo> _subject = new Subject<TServerDistanceInfo>();
        private Dictionary<string, TServerDistanceInfo> _serverInfo = new Dictionary<string,TServerDistanceInfo>();
        private string _closestServer;
        private IDisposable _poller;
        private readonly Func<string, TServerDistanceInfo> _measureDistance;
        private readonly object _lock = new object();
        private bool _allUnreachable = false;

        #region Properties

        /// <summary>
        /// Gets or sets the the interval in milliseconds at which servers are pinged. Default 5 mins.
        /// </summary>
        public double Interval;

        /// <summary>
        /// Only change servers if the distance is more than Tolerance milliseconds less than the distance to the current server.
        /// </summary>
        public long Tolerance = 50;

        /// <summary>
        /// TServerDistanceInfo item for each server being monitored, ordered by increasing ping time.
        /// </summary>
        public List<TServerDistanceInfo> ServerInfo { get { return _serverInfo.Values.OrderBy(sdi => sdi.Distance).ToList(); } }

        #endregion

        #region Constructor

        /// <summary>
        /// Class to maintain ping time information to a collection of servers
        /// </summary>
        /// <param name="servers">Names or string representation of IP addresses of the servers to monitor</param>
        /// <param name="measureDistance">Optional function to measure distance to a server</param>
        /// <param name="init">Optional initialisation function</param>
        public ServerDistance(IEnumerable<string> servers, Func<string, TServerDistanceInfo> measureDistance = null, Action<IEnumerable<string>> init = null)
        {
            Interval = TimerInterval;
            _servers = servers.ToList();
            _measureDistance = measureDistance ?? MeasureDistance;
            (init ?? Init)(_servers);
        }

        #endregion

        #region Methods

        public void Start()
        {
            lock (_lock) {
                _serverInfo = new Dictionary<string, TServerDistanceInfo>();
                _closestServer = null;
            }
            // Poll each server immediately, then with an Interval millisecond gap, each on task pool threads.
            // Note that Generate runs measureDistance immediately, then waits for the timeSelector interval before passing on the result and immediately running measureDistance again. In
            // order to get results quickly timeSelector is zero fist time through, then a longer gap, but this means you get two measureDistances close together initially.
            // We use the TaskPool Scheduler, as the observables don't seem to shut down on disposal if we use NewThread.
            _poller = _servers.Select(server => Observable.Generate(true, _ => true, _ => false, _ => _measureDistance(server),
                                                                    isFirst => isFirst ? TimeSpan.Zero : TimeSpan.FromMilliseconds(Interval), Scheduler.TaskPool))
                              .Merge().Subscribe(OnDistanceInfo);
        }

        public void Stop()
        {
            if (_poller != null) {
                _poller.Dispose();
                _poller = null;
            }
        }

        private void OnDistanceInfo(TServerDistanceInfo sdi)
        {
            // Ensure that multiple threads do not attempt to update _distances at the same time.
            lock (_lock) {
                _serverInfo[sdi.Address] = sdi;
                if (sdi.IsReachable && (_closestServer == null || (_closestServer != sdi.Address && sdi.Distance < _serverInfo[_closestServer].Distance - Tolerance))) {
                    _closestServer = sdi.Address;
                    _allUnreachable = false;
                    _subject.OnNext(sdi);
                } else if (!_allUnreachable && !sdi.IsReachable && _serverInfo.Count == _servers.Count && _serverInfo.All(kvp => !kvp.Value.IsReachable)) {
                    _allUnreachable = true;
                    _subject.OnError(new Exception("All servers unreachable"));
                }
            }
        }

        #endregion

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
            Stop();
        }

        #endregion
    }
}
