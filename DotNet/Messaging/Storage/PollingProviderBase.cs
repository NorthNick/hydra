using System;
using System.Collections.Generic;
using System.Linq;

namespace Bollywell.Hydra.Messaging.Storage
{
    public abstract class PollingProviderBase : IProvider, IDisposable
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
        protected PollingProviderBase(string hydraServer, string database = DefaultDatabase, int port = DefaultPort) 
            : this(new List<string> {hydraServer}, database, port) {}

        /// <summary>
        /// Initialise messaging. Must be called before any attempt to send or listen.
        /// </summary>
        /// <param name="hydraServers">Hydra servers to communicate with</param>
        /// <param name="database">Name of the messaging database. Defaults to "hydra"</param>
        /// <param name="port">Port number of the messaging database. defaults to 5984</param>
        protected PollingProviderBase(IEnumerable<string> hydraServers, string database = DefaultDatabase, int port = DefaultPort)
             : this(hydraServers.Select(s => new CouchDbStore(s, s, database, port))) {}

        /// <summary>
        /// Initialise messaging. Must be called before any attempt to send or listen.
        /// </summary>
        /// <param name="stores">Hydra stores to communicate with</param>
        protected PollingProviderBase(IEnumerable<IStore> stores)
        {
            if (stores == null || !stores.Any()) throw new ArgumentException("At least one store must be supplied", "stores");
            _storeDict = stores.ToDictionary(store => store.Name);
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
        /// Called by listener when it cannot contact a server.
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