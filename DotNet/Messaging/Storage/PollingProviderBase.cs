using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Bollywell.Hydra.Messaging.Storage
{
    public abstract class PollingProviderBase : IProvider, IDisposable
    {
        private string _hydraServer;
        protected readonly ServerDistance<ServerDistanceInfo> Distances;
        private readonly object _serverErrorLock = new object();
        protected readonly Dictionary<string, IStore> StoreDict;
        // Set after initialisation to save having to get _initLock every time.
        protected bool Initialised { get; set; }
        private readonly ManualResetEvent _initLock = new ManualResetEvent(false);

        #region Properties

        /// <summary>
        /// The currently nearest Hydra server.
        /// </summary>
        public string HydraServer
        {
            get { return _hydraServer; } 
            set
            {
                if (!Initialised) FinishedInitialisation();
                _hydraServer = value;
            }
        }

        public bool IsOffline { get; protected set; }

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
        /// <param name="port">Port number of the messaging database. Defaults to 5984</param>
        protected PollingProviderBase(string hydraServer, string database = null, int? port = null) 
            : this(new List<string> {hydraServer}, database, port) {}

        /// <summary>
        /// Initialise messaging. Must be called before any attempt to send or listen.
        /// </summary>
        /// <param name="hydraServers">Hydra servers to communicate with</param>
        /// <param name="database">Name of the messaging database. Defaults to "hydra"</param>
        /// <param name="port">Port number of the messaging database. Defaults to 5984</param>
        protected PollingProviderBase(IEnumerable<string> hydraServers, string database = null, int? port = null)
             : this(hydraServers.Select(s => new CouchDbStore(s, s, database, port))) {}

        /// <summary>
        /// Initialise messaging. Must be called before any attempt to send or listen.
        /// </summary>
        /// <param name="stores">Hydra stores to communicate with</param>
        protected PollingProviderBase(IEnumerable<IStore> stores)
        {
            if (stores == null || !stores.Any()) throw new ArgumentException("At least one store must be supplied", "stores");
            StoreDict = stores.ToDictionary(store => store.Name);
            Distances = new ServerDistance<ServerDistanceInfo>(StoreDict.Keys, MeasureDistance, InitDistance);
            // TODO: this won't work because the FinishedInitialisation event may be raised before this handler is bound
            Distances.FinishedInitialisation += serverDistance => FinishedInitialisation();
            Distances.Subscribe(OnDistanceInfo);
        }

        /// <summary>
        /// Fetches the current store to use for polling.
        /// </summary>
        /// <param name="waitForInitialisation">Whether to block until the IProvider is fully initialised</param>
        /// <returns></returns>
        public IStore GetStore(bool waitForInitialisation)
        {
            if (waitForInitialisation && !Initialised) {
                // Pause until _initLock is set by LockedInitDistance
                // TODO: set timeout here and set _initialised afterwards regardless
                _initLock.WaitOne();
            }
            return HydraServer == null ? null : StoreDict[HydraServer];
        }

        /// <summary>
        /// Called by listener when it cannot contact a server.
        /// </summary>
        /// <param name="server">The server that could not be contacted</param>
        public void ServerError(string server)
        {
            // The lock ensures that multiple threads do not attempt to reset at the same time.
            // We could use double-checked locking, but even Jon Skeet is doubtful about it, and there would not be much performance gain.
            lock (_serverErrorLock) {
                if (server == HydraServer) {
                    // The current server has gone offline. Inform Distances.
                    Distances.OnDistanceInfo(new ServerDistanceInfo { Name = server, Distance = long.MaxValue, IsReachable = false });
                }
            }
        }

        protected virtual void OnDistanceInfo(ServerDistanceInfo sdi) { }

        #region Measure distance

        protected ServerDistanceInfo MeasureDistance(string server)
        {
            return StoreDict[server].MeasureDistance();
        }

        protected void FinishedInitialisation()
        {
            if (!Initialised) {
                Initialised = true;
                _initLock.Set();
            }
        }

        // InitDistance will be run asynchronously by ServerDistance but should itself be synchronous.
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
                _initLock.Dispose();
            }
            // free native resources if there are any.
        }

        #endregion

    }
}