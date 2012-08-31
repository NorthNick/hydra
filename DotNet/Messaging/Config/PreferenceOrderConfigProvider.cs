using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Bollywell.Hydra.Messaging.Config
{
    /// <summary>
    /// Use the first server in the preference list that is responding. Switch if a earlier one in the list becomes available.
    /// </summary>
    public class PreferenceOrderConfigProvider : PollingConfigProviderBase
    {
        private readonly List<string> _servers;
        private readonly Dictionary<string, int> _serverIndices = new Dictionary<string, int>();
        private IDisposable _preferenceSubscription;
        private int _preferredIndex;

        /// <summary>
        /// Initialise messaging. Must be called before any attempt to send or listen.
        /// </summary>
        /// <param name="hydraServers">Hydra servers to communicate with, in decreasing order of preference</param>
        /// <param name="database">Name of the messaging database. Defaults to "hydra"</param>
        /// <param name="port">Port number of the messaging database. defaults to 5984</param>
        /// <param name="pollIntervalMs">Optional polling interval of the database, in milliseconds</param>
        public PreferenceOrderConfigProvider(IEnumerable<string> hydraServers, string database = DefaultDatabase, int port = DefaultPort, int? pollIntervalMs = null)
            : this(hydraServers.Select(s => new CouchDbStore(s, s, database, port)), pollIntervalMs) {}

        /// <summary>
        /// Initialise messaging. Must be called before any attempt to send or listen.
        /// </summary>
        /// <param name="stores">Hydra stores to communicate with</param>
        /// <param name="pollIntervalMs">Optional polling interval of the database, in milliseconds</param>
        public PreferenceOrderConfigProvider(IEnumerable<IStore> stores, int? pollIntervalMs = null) : base(stores, pollIntervalMs)
        {
            _servers = stores.Select(s => s.Name).ToList();
            Enumerable.Range(0, _servers.Count).ToList().ForEach(index => _serverIndices[_servers[index]] = index);
            Start();
        }

        protected override void PreStop()
        {
            if (_preferenceSubscription != null) {
                _preferenceSubscription.Dispose();
                _preferenceSubscription = null;
            }
        }

        protected override void PostStart()
        {
            _preferredIndex = _servers.Count;
            _preferenceSubscription = Distances.Where(IsNewPreferred).Subscribe(sdi => HydraServer = sdi.Address);
        }

        private bool IsNewPreferred(ServerDistanceInfo sdi)
        {
            // Note that this returns false if  the current preferred server becomes unreachable. It will only return true when the new preferred server turns up.
            // A server is preferred if it is earlier in _servers than the current server, it is reachable, and all servers before it in _servers are unreachable.
            var sdiIndex = _serverIndices[sdi.Address];
            if (sdiIndex < _preferredIndex && sdi.IsReachable 
                && _servers.Take(sdiIndex - 1).All(s => Distances.ServerInfo.ContainsKey(s) && !Distances.ServerInfo[s].IsReachable)) {
                _preferredIndex = sdiIndex;
                return true;
            }
            return false;
        }
    }
}
