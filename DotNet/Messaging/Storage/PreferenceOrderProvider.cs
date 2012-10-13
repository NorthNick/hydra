using System.Collections.Generic;
using System.Linq;

namespace Bollywell.Hydra.Messaging.Storage
{
    /// <summary>
    /// Use the first server in the preference list that is responding. Switch if a earlier one in the list becomes available.
    /// </summary>
    public class PreferenceOrderProvider : PollingProviderBase
    {
        private readonly List<string> _servers;
        private readonly Dictionary<string, int> _serverIndices = new Dictionary<string, int>();
        private int _preferredIndex;

        /// <summary>
        /// Initialise messaging. Must be called before any attempt to send or listen.
        /// </summary>
        /// <param name="hydraServers">Hydra servers to communicate with, in decreasing order of preference</param>
        /// <param name="database">Name of the messaging database. Defaults to "hydra"</param>
        /// <param name="port">Port number of the messaging database. Defaults to 5984</param>
        public PreferenceOrderProvider(IEnumerable<string> hydraServers, string database = DefaultDatabase, int port = DefaultPort)
            : this(hydraServers.Select(s => new CouchDbStore(string.Format("{0}:{1}:{2}", s, port, database), s, database, port))) { }

        /// <summary>
        /// Initialise messaging. Must be called before any attempt to send or listen.
        /// </summary>
        /// <param name="stores">Hydra stores to communicate with</param>
        public PreferenceOrderProvider(IEnumerable<IStore> stores) : base(stores)
        {
            _servers = stores.Select(s => s.Name).ToList();
            Enumerable.Range(0, _servers.Count).ToList().ForEach(index => _serverIndices[_servers[index]] = index);
            _preferredIndex = _servers.Count;
        }

        protected override void OnDistanceInfo(ServerDistanceInfo sdi)
        {
            var sdiIndex = _serverIndices[sdi.Name];
            if (sdi.IsReachable && sdiIndex < _preferredIndex
                && _servers.Take(sdiIndex - 1).All(s => Distances.ServerInfo.ContainsKey(s) && !Distances.ServerInfo[s].IsReachable)) {
                // A server is preferred if it is earlier in _servers than the current server, it is reachable, and all servers before it in _servers are unreachable.
                _preferredIndex = sdiIndex;
                HydraServer = sdi.Name;
            } else if (!sdi.IsReachable && sdiIndex == _preferredIndex) {
                // The current server is no longer responding - replace with the first reachable one before which all others are unreachable, or else null
                var candidate = _servers.SkipWhile(s => Distances.ServerInfo.ContainsKey(s) && !Distances.ServerInfo[s].IsReachable).FirstOrDefault();
                HydraServer = candidate != null && Distances.ServerInfo.ContainsKey(candidate) ? candidate : null;
                _preferredIndex = HydraServer == null ? _servers.Count : _serverIndices[HydraServer];
            }
        }

    }
}
