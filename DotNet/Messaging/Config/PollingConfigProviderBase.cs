using System;
using System.Collections.Generic;
using System.Linq;

namespace Bollywell.Hydra.Messaging.Config
{
    public abstract class PollingConfigProviderBase : IConfigProvider, IDisposable
    {
        protected const string DefaultDatabase = "hydra";
        protected const int DefaultPort = 5984;

        protected readonly ServerDistance<ServerDistanceInfo> Distances;
        private readonly object _lock = new object();
        private readonly Dictionary<string, IStore> _storeDict;

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
             : this(hydraServers.Select(s => new CouchDbStore(s, s, database, port)), pollIntervalMs) {}

        /// <summary>
        /// Initialise messaging. Must be called before any attempt to send or listen.
        /// </summary>
        /// <param name="stores">Hydra stores to communicate with</param>
        /// <param name="pollIntervalMs">Optional polling interval of the database, in milliseconds</param>
        protected PollingConfigProviderBase(IEnumerable<IStore> stores, int? pollIntervalMs = null)
        {
            if (stores == null || !stores.Any()) throw new ArgumentException("At least one store must be supplied", "stores");
            _storeDict = stores.ToDictionary(store => store.Name);
            PollIntervalMs = pollIntervalMs;
            Distances = new ServerDistance<ServerDistanceInfo>(stores.Select(store => store.Name), MeasureDistance, InitDistance);
        }

        /// <summary>
        /// Fetches the current store to use for polling.
        /// </summary>
        public IStore GetStore()
        {
            return _storeDict[HydraServer];
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
            return _storeDict[server].MeasureDistance();
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