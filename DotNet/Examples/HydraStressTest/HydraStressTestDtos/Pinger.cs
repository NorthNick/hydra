using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Timers;

namespace HydraStressTestDtos
{
    public class Pinger
    {

        #region Class variables
        // Timer interval of 5 mins in milliseconds.
        private const double TimerInterval = 5 * 60 * 1000;
        private const long UnreachableTime = 100000;

        private readonly List<string> _servers;
        private readonly Timer _timer;
        private bool _running;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the the interval in milliseconds at which servers are pinged. Default 5 mins.
        /// </summary>
        public double Interval
        {
            get { return _timer.Interval; }
            set { _timer.Interval = value; }
        }

        /// <summary>
        /// PingInfo item for each server being monitored, ordered by increasing ping time.
        /// </summary>
        public List<PingInfo> ServerInfo { get; private set; }
        #endregion

        #region Events
        public delegate void PingerEventHandler(object sender, PingEventArgs pe);

        /// <summary>
        /// Raised after ServerInfo has been refreshed
        /// </summary>
        public event PingerEventHandler Refreshed;
        #endregion

        #region Constructor
        /// <summary>
        /// Class to maintain ping time information to a collection of servers
        /// </summary>
        /// <param name="servers">Names or string representation of IP addresses of the servers to monitor</param>
        public Pinger(IEnumerable<string> servers)
        {
            _servers = servers.ToList();
            ServerInfo = new List<PingInfo>();
            _timer = new Timer { Interval = TimerInterval, Enabled = false };
            _timer.Elapsed += RefreshServerInfoBegin;

        }
        #endregion

        #region Methods

        /// <summary>
        /// Refresh ServerInfo, and return when complete.
        /// </summary>
        public void RefreshSync()
        {
            var task = PingTask();
            task.Wait();
        }

        /// <summary>
        /// Start the pinger running. The first refresh starts immediately.
        /// </summary>
        public void Start()
        {
            if (!_running) {
                _running = true;
                RefreshServerInfoBegin(null, null);
            }
        }

        /// <summary>
        /// Stop the pinger. Any ongoing refreshes will complete and raise the Refreshed event.
        /// </summary>
        public void Stop()
        {
            // Prevent further events from being raised, and prevent timer being re-enabled after any currently-occurring event processing
            _timer.Stop();
            _running = false;
        }

        #endregion

        #region Helper methods

        private void RefreshServerInfoBegin(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();
            PingTask();
        }

        private Task PingTask()
        {
            // Ping the server list on separate threads.
            return Task.Factory.ContinueWhenAll(_servers.Select(server => Task.Factory.StartNew(() => Ping(server))).ToArray(),
                                                completedTasks => RefreshServerInfoEnd(completedTasks.Select(t => t.Result)));
        }

        private void RefreshServerInfoEnd(IEnumerable<PingInfo> pingReplies)
        {
            ServerInfo = pingReplies.OrderBy(reply => reply.RoundtripMilliseconds).ToList();
            OnRefreshed();
            // Be careful not to restart the timer if the Pinger was stopped while refreshing.
            if (_running) _timer.Start();
        }

        private void OnRefreshed()
        {
            if (Refreshed != null) Refreshed(this, new PingEventArgs(DateTime.Now));
        }

        private PingInfo Ping(string server)
        {
            try {
                var reply = new Ping().Send(server);
                bool isReachable = reply.Status == IPStatus.Success;
                return new PingInfo { Address = server, IsReachable = isReachable, RoundtripMilliseconds = isReachable ? reply.RoundtripTime : UnreachableTime };
            } catch (Exception) {
                return new PingInfo { Address = server, IsReachable = false, RoundtripMilliseconds = UnreachableTime };
            }
        }

        #endregion

    }

    public class PingInfo
    {
        public string Address { get; set; }
        public bool IsReachable { get; set; }
        public long RoundtripMilliseconds { get; set; }
    }

    public class PingEventArgs : EventArgs
    {
        public DateTime SignalTime { get; private set; }

        public PingEventArgs(DateTime signalTime)
        {
            SignalTime = signalTime;
        }
    }
}
