using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using LoveSeat;
using LoveSeat.Interfaces;

namespace Bollywell.Hydra.Messaging.Config
{
    public abstract class PollingConfigProviderBase : IConfigProvider, IDisposable
    {
        protected const string DefaultDatabase = "hydra";
        protected const int DefaultPort = 5984;

        private readonly string _database;
        private readonly int _port;
        protected readonly ServerDistance<ServerDistanceInfo> Distances;
        private readonly object _lock = new object();

        #region Properties

        /// <summary>
        /// The currently nearest Hydra server.
        /// </summary>
        public string HydraServer { get; protected set; }

        /// <summary>
        /// Interval at which to poll for new messages, in milliseconds.
        /// </summary>
        public int? PollIntervalMs { get; private set; }

        /// <summary>
        /// The interval at which distance to Hydra servers is measured, in milliseonds.
        /// </summary>
        public double DistanceIntervalMs
        {
            get { return Distances.Interval; }
            set { Distances.Interval = value; }
        }

        #endregion

        /// <summary>
        /// Initialise messaging. Must be called before any attempt to send or listen.
        /// </summary>
        /// <param name="hydraServer">Hydra server to communicate with</param>
        /// <param name="database">Name of the messaging database. Defaults to "hydra"</param>
        /// <param name="port">Port number of the messaging database. defaults to 5984</param>
        /// <param name="pollIntervalMs">Optional polling interval of the database, in milliseconds</param>
        protected PollingConfigProviderBase(string hydraServer, string database = DefaultDatabase, int port = DefaultPort, int? pollIntervalMs = null) 
            : this(new List<string> {hydraServer}, database, port, pollIntervalMs) {}

        /// <summary>
        /// Initialise messaging. Must be called before any attempt to send or listen.
        /// </summary>
        /// <param name="hydraServers">Hydra servers to communicate with</param>
        /// <param name="database">Name of the messaging database. Defaults to "hydra"</param>
        /// <param name="port">Port number of the messaging database. defaults to 5984</param>
        /// <param name="pollIntervalMs">Optional polling interval of the database, in milliseconds</param>
        protected PollingConfigProviderBase(IEnumerable<string> hydraServers, string database = DefaultDatabase, int port = DefaultPort, int? pollIntervalMs = null)
        {
            if (hydraServers == null || !hydraServers.Any()) throw new ArgumentException("At least one server must be supplied", "hydraServers");
            _database = database;
            _port = port;
            PollIntervalMs = pollIntervalMs;
            Distances = new ServerDistance<ServerDistanceInfo>(hydraServers, MeasureDistance, InitDistance);
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
                    PreStop();
                    Distances.Stop();
                    Start();
                }
            }
        }

        protected void Start()
        {
            Distances.Start();
            PostStart();
        }

        protected virtual void PreStop() { }
        protected virtual void PostStart() { }

        #region Measure distance

        protected ServerDistanceInfo MeasureDistance(string server)
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

        protected virtual void InitDistance(IEnumerable<string> servers) {}

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
                Distances.Dispose();
            }
            // free native resources if there are any.
        }

        #endregion

    }
}