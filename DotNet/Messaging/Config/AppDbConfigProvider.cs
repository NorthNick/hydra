using System;
using System.Collections.Generic;
using System.Linq;

namespace Bollywell.Hydra.Messaging.Config
{
    public class AppDbConfigProvider : IDbConfigProvider
    {
        private static MessagingConfig _theConfig;
        private static List<string> _servers;
        private static int _serverIndex;

        /// <summary>
        /// Initialise messaging. Must be called before any attempt to send or listen.
        /// </summary>
        /// <param name="hydraServer">Hydra server to communicate with</param>
        /// <param name="database">Name of the messaging database</param>
        /// <param name="pollIntervalMs">Optional polling interval of the database, in milliseconds</param>
        public AppDbConfigProvider(string hydraServer, string database, int? pollIntervalMs = null)
        {
            _servers = new List<string> { hydraServer };
            Update(hydraServer, database, pollIntervalMs);
        }

        /// <summary>
        /// Initialise messaging. Must be called before any attempt to send or listen.
        /// </summary>
        /// <param name="hydraServers">Hydra servers to communicate with</param>
        /// <param name="database">Name of the messaging database</param>
        /// <param name="pollIntervalMs">Optional polling interval of the database, in milliseconds</param>
        public AppDbConfigProvider(IEnumerable<string> hydraServers, string database, int? pollIntervalMs = null)
        {
            if (hydraServers == null || !hydraServers.Any()) throw new ArgumentException("At least one server must be supplied", "hydraServers");
            _servers = new List<string>(hydraServers);
            _serverIndex = 0;
            Update(_servers[0], database, pollIntervalMs);
        }

        public IMessagingConfig GetConfig()
        {
            return _theConfig;
        }

        public bool SwitchServer()
        {
            // TODO: Make this intelligent e.g. test the next server for responsiveness.
            _serverIndex = (_serverIndex + 1) % _servers.Count;
            Update(_servers[0], _theConfig.Database, _theConfig.PollIntervalMs);
            return true;
        }

        /// <summary>
        /// Initialise messaging. Must be called before any attempt to send or listen.
        /// </summary>
        /// <param name="hydraServer">Hydra server to communicate with</param>
        /// <param name="database">Name of the messaging database</param>
        /// <param name="pollIntervalMs">Optional polling interval of the database, in milliseconds</param>
        public void Update(string hydraServer, string database, int? pollIntervalMs = null)
        {
            _theConfig = new MessagingConfig(database, hydraServer, pollIntervalMs);
        }
    }
}
