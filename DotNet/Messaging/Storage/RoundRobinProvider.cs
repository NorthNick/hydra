using System;
using System.Collections.Generic;
using System.Linq;

namespace Bollywell.Hydra.Messaging.Storage
{
    /// <summary>
    /// Fail over between servers on a round robin basis.
    /// </summary>
    public class RoundRobinProvider : IProvider
    {
        private const string DefaultDatabase = "hydra";
        private const int DefaultPort = 5984;

        private static int _storeIndex;
        private readonly List<IStore> _stores;

        public string HydraServer { get; private set; }

        public bool IsOffline { get; private set; }

        /// <summary>
        /// Initialise messaging. Must be called before any attempt to send or listen.
        /// </summary>
        /// <param name="hydraServer">Hydra server to communicate with</param>
        /// <param name="database">Name of the messaging database. Defaults to "hydra"</param>
        /// <param name="port">Port number of the messaging database. defaults to 5984</param>
        public RoundRobinProvider(string hydraServer, string database = DefaultDatabase, int port = DefaultPort) 
            : this(new List<string> {hydraServer}, database, port) {}

        /// <summary>
        /// Initialise messaging. Must be called before any attempt to send or listen.
        /// </summary>
        /// <param name="hydraServers">Hydra servers to communicate with</param>
        /// <param name="database">Name of the messaging database. Defaults to "hydra"</param>
        /// <param name="port">Port number of the messaging database. defaults to 5984</param>
        public RoundRobinProvider(IEnumerable<string> hydraServers, string database = DefaultDatabase, int port = DefaultPort)
            : this(hydraServers.Select(s => new CouchDbStore(s, s, database, port))) {}

        /// <summary>
        /// Initialise messaging. Must be called before any attempt to send or listen.
        /// </summary>
        /// <param name="stores">Hydra stores to communicate with</param>
        public RoundRobinProvider(IEnumerable<IStore> stores)
        {
            if (stores == null || !stores.Any()) throw new ArgumentException("At least one store must be supplied", "stores");
            _stores = new List<IStore>(stores);
            HydraServer = _stores[0].Name;
        }

        /// <summary>
        /// Fetches the current store to use for polling.
        /// </summary>
        public IStore GetStore(bool waitForInitialisation)
        {
            return _stores[_storeIndex];
        }

        /// <summary>
        /// Called by listener when it cannot contact a server.
        /// </summary>
        /// <param name="server">The server that could not be contacted</param>
        public void ServerError(string server)
        {
            // Very simplistic check that switches if the problematic server is the current one.
            if (server == _stores[_storeIndex].Name) {
                // TODO: Make this intelligent e.g. test the next server for responsiveness.
                _storeIndex = (_storeIndex + 1) % _stores.Count;
                HydraServer = _stores[_storeIndex].Name;
            }
        }

    }
}
