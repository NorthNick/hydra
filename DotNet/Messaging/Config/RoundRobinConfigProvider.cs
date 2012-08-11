using System;
using System.Collections.Generic;
using System.Linq;
using LoveSeat;
using LoveSeat.Interfaces;

namespace Bollywell.Hydra.Messaging.Config
{
    /// <summary>
    /// Fail over between servers on a round robin basis.
    /// </summary>
    public class RoundRobinConfigProvider : IConfigProvider
    {
        private const string DefaultDatabase = "hydra";
        private const int DefaultPort = 5984;

        private static List<string> _servers;
        private static int _serverIndex;
        private readonly string _database;
        private readonly int _port;

        public string HydraServer { get; private set; }
        public int? PollIntervalMs { get; private set; }

        /// <summary>
        /// Initialise messaging. Must be called before any attempt to send or listen.
        /// </summary>
        /// <param name="hydraServer">Hydra server to communicate with</param>
        /// <param name="database">Name of the messaging database. Defaults to "hydra"</param>
        /// <param name="port">Port number of the messaging database. defaults to 5984</param>
        /// <param name="pollIntervalMs">Optional polling interval of the database, in milliseconds</param>
        public RoundRobinConfigProvider(string hydraServer, string database = DefaultDatabase, int port = DefaultPort, int? pollIntervalMs = null) 
            : this(new List<string> {hydraServer}, database, port, pollIntervalMs) {}

        /// <summary>
        /// Initialise messaging. Must be called before any attempt to send or listen.
        /// </summary>
        /// <param name="hydraServers">Hydra servers to communicate with</param>
        /// <param name="database">Name of the messaging database. Defaults to "hydra"</param>
        /// <param name="port">Port number of the messaging database. defaults to 5984</param>
        /// <param name="pollIntervalMs">Optional polling interval of the database, in milliseconds</param>
        public RoundRobinConfigProvider(IEnumerable<string> hydraServers, string database = DefaultDatabase, int port = DefaultPort, int? pollIntervalMs = null)
        {
            if (hydraServers == null || !hydraServers.Any()) throw new ArgumentException("At least one server must be supplied", "hydraServers");
            _servers = new List<string>(hydraServers);
            _database = database;
            _port = port;
            HydraServer = _servers[0];
            PollIntervalMs = pollIntervalMs;
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
            // Very simplistic check that switches if the problematic server is the current one.
            if (server == _servers[_serverIndex]) {
                // TODO: Make this intelligent e.g. test the next server for responsiveness.
                _serverIndex = (_serverIndex + 1) % _servers.Count;
                HydraServer = _servers[_serverIndex];
            }
        }

    }
}
