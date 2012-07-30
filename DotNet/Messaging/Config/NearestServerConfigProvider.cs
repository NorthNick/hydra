using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Threading.Tasks;
using LoveSeat;
using LoveSeat.Interfaces;

namespace Bollywell.Hydra.Messaging.Config
{
    public class NearestServerConfigProvider : IConfigProvider
    {
        private const string DefaultDatabase = "hydra";
        private const int DefaultPort = 5984;

        private readonly string _database;
        private readonly int _port;
        private readonly ServerDistance<ServerDistanceInfo> _distances;
        private IDisposable _subscription;
        private readonly object _lock = new object();

        #region Properties

        /// <summary>
        /// The currently nearest Hydra server.
        /// </summary>
        public string HydraServer { get; private set; }

        /// <summary>
        /// Interval at which to poll for new messages, in milliseconds.
        /// </summary>
        public int? PollIntervalMs { get; private set; }

        /// <summary>
        /// The interval at which distance to Hydra servers is measured, in milliseonds.
        /// </summary>
        public double DistanceIntervalMs
        {
            get { return _distances.Interval; }
            set { _distances.Interval = value; }
        }

        #endregion

        /// <summary>
        /// Initialise messaging. Must be called before any attempt to send or listen.
        /// </summary>
        /// <param name="hydraServer">Hydra server to communicate with</param>
        /// <param name="database">Name of the messaging database. Defaults to "hydra"</param>
        /// <param name="port">Port number of the messaging database. defaults to 5984</param>
        /// <param name="pollIntervalMs">Optional polling interval of the database, in milliseconds</param>
        public NearestServerConfigProvider(string hydraServer, string database = DefaultDatabase, int port = DefaultPort, int? pollIntervalMs = null) 
            : this(new List<string> {hydraServer}, database, port, pollIntervalMs) {}

        /// <summary>
        /// Initialise messaging. Must be called before any attempt to send or listen.
        /// </summary>
        /// <param name="hydraServers">Hydra servers to communicate with</param>
        /// <param name="database">Name of the messaging database. Defaults to "hydra"</param>
        /// <param name="port">Port number of the messaging database. defaults to 5984</param>
        /// <param name="pollIntervalMs">Optional polling interval of the database, in milliseconds</param>
        public NearestServerConfigProvider(IEnumerable<string> hydraServers, string database = DefaultDatabase, int port = DefaultPort, int? pollIntervalMs = null)
        {
            if (hydraServers == null || !hydraServers.Any()) throw new ArgumentException("At least one server must be supplied", "hydraServers");
            _database = database;
            _port = port;
            PollIntervalMs = pollIntervalMs;
            _distances = new ServerDistance<ServerDistanceInfo>(hydraServers, MeasureDistance, InitDistance);
            Subscribe();
        }

        /// <summary>
        /// Fetches the current database to use for polling.
        /// </summary>
        public IDocumentDatabase GetDb()
        {
            return new CouchClient(HydraServer, _port, null, null, false, AuthenticationType.Basic).GetDatabase(_database);
        }

        /// <summary>
        /// Called by poller when it cannot contact a server.
        /// </summary>
        /// <param name="server">The server that could not be contacted</param>
        public void ServerError(string server)
        {
            // The lock ensures that multiple threads do not attempt to reset at the same time.
            // We could use double-checked locking, but even Jon Skeet is doubtful about it, and there would not be much performance gain.
            lock (_lock) {
                if (server == HydraServer) {
                    // The current server has gone offline. Restart _distances to force a repoll.
                    _subscription.Dispose();
                    _distances.Stop();
                    Subscribe();
                }
            }
        }

        private void Subscribe()
        {
            _distances.Start();
            // Block until the first server response
            HydraServer = _distances.First().Address;
            _subscription = _distances.Subscribe(sdi => HydraServer = sdi.Address);
        }

        #region Measure distance

        private ServerDistanceInfo MeasureDistance(string server)
        {
            bool responseOk = false;
            long elapsed = 0;
            try {
                // This URL checks both that the server is up, and that the view index is up to date
                string url = string.Format("http://{0}:{1}/{2}/_design/hydra/_view/broadcastMessages?limit=0", server, _port, _database);
                var timer = Stopwatch.StartNew();
                using (HttpWebResponse response = (HttpWebResponse)WebRequest.Create(url).GetResponse()) {
                    elapsed = timer.ElapsedMilliseconds;
                    responseOk = response.StatusCode == HttpStatusCode.OK;
                }
            } catch (Exception) {
                // Swallow errors
            }
            return new ServerDistanceInfo {Address = server, Distance = responseOk ? elapsed : long.MaxValue, IsReachable = responseOk};
        }

        private void InitDistance(IEnumerable<string> servers)
        {
            // Do a one-off poll of everything and discard the results, but only wait 1.5 seconds in case of very slow responses. For some reason the first connection to
            // a CouchDb server on localhost takes one second for the TCP connect phase, so this gets over that initial slow poll.
            Task.WaitAll(servers.Select(server => Task.Factory.StartNew(() => { MeasureDistance(server); })).ToArray(), 1500);
        }

        #endregion
    }
}
