using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bollywell.Hydra.Messaging.Storage
{
    /// <summary>
    /// Use the Hydra server with the smallest response time. Switch if another one is more than Tolerance milliseconds faster.
    /// </summary>
    public class NearestServerProvider : PollingProviderBase
    {

        /// <summary>
        /// Only change servers if the distance is more than Tolerance milliseconds less than the distance to the current server.
        /// </summary>
        public long Tolerance = 50;

        /// <summary>
        /// Initialise messaging. Must be called before any attempt to send or listen.
        /// </summary>
        /// <param name="hydraServer">Hydra server to communicate with</param>
        /// <param name="database">Name of the messaging database. Defaults to "hydra"</param>
        /// <param name="port">Port number of the messaging database. Defaults to 5984</param>
        public NearestServerProvider(string hydraServer, string database = null, int? port = null) 
            : this(new List<string> {hydraServer}, database, port) {}

        /// <summary>
        /// Initialise messaging. Must be called before any attempt to send or listen.
        /// </summary>
        /// <param name="hydraServers">Hydra servers to communicate with</param>
        /// <param name="database">Name of the messaging database. Defaults to "hydra"</param>
        /// <param name="port">Port number of the messaging database. Defaults to 5984</param>
        public NearestServerProvider(IEnumerable<string> hydraServers, string database = null, int? port = null)
            : this(hydraServers.Select(s => new CouchDbStore(s, database, port))) {}

        /// <summary>
        /// Initialise messaging. Must be called before any attempt to send or listen.
        /// </summary>
        /// <param name="stores">Hydra stores to communicate with</param>
        public NearestServerProvider(IEnumerable<IStore> stores)
            : base(stores) {}

        protected override void OnDistanceInfo(ServerDistanceInfo sdi)
        {
            if (sdi.IsReachable && (HydraServer == null || (HydraServer != sdi.Name && sdi.Distance < Distances.ServerInfo[HydraServer].Distance - Tolerance))) {
                // There is a better server than the current one
                HydraServer = sdi.Name;
            } else if (!sdi.IsReachable && HydraServer == sdi.Name) {
                // The current server is no longer responding - replace with the nearest reachable one, or null if none are reachable
                HydraServer = Distances.ServerInfo.Values.Where(sdi1 => sdi1.IsReachable).OrderBy(sdi1 => sdi1.Distance).Select(sdi1 => sdi1.Name).FirstOrDefault();
            } else if (!Initialised && !sdi.IsReachable && Distances.ServerInfo.Count == StoreDict.Count) {
                // All servers are unreachable so initialisation is done
                FinishedInitialisation();
            }
        }

        #region Measure distance

        protected override void InitDistance(IEnumerable<string> servers)
        {
            // Do a one-off poll of everything and discard the results, but only wait 1.5 seconds in case of very slow responses. For some reason the first connection to
            // a CouchDb server on localhost takes one second for the TCP connect phase, so this gets over that initial slow poll.
            // TODO: convert to Rx - see http://cm-bloggers.blogspot.co.uk/2011/02/reactive-extensions.html
            Task.WaitAll(servers.Select(server => Task.Factory.StartNew(() => { MeasureDistance(server); })).ToArray(), 1500);
        }

        #endregion

    }
}
