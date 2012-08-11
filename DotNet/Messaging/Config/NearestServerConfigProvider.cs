using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Bollywell.Hydra.Messaging.Config
{
    /// <summary>
    /// Use the Hydra server with the smallest response time. Switch if another one is more than Tolerance milliseconds faster.
    /// </summary>
    public class NearestServerConfigProvider : PollingConfigProviderBase
    {
        private string _closestServer;
        private IDisposable _closestSubscription;

        /// <summary>
        /// Only change servers if the distance is more than Tolerance milliseconds less than the distance to the current server.
        /// </summary>
        public long Tolerance = 50;

        /// <summary>
        /// Initialise messaging. Must be called before any attempt to send or listen.
        /// </summary>
        /// <param name="hydraServer">Hydra server to communicate with</param>
        /// <param name="database">Name of the messaging database. Defaults to "hydra"</param>
        /// <param name="port">Port number of the messaging database. defaults to 5984</param>
        /// <param name="pollIntervalMs">Optional polling interval of the database, in milliseconds</param>
        public NearestServerConfigProvider(string hydraServer, string database = DefaultDatabase, int port = DefaultPort, int? pollIntervalMs = null) 
            : this(new List<string> {hydraServer}, database, port, pollIntervalMs) {}

        /// <summary>
        /// Initialise messaging. Must be called before any attempt to send or listen.
        /// </summary>
        /// <param name="hydraServers">Hydra servers to communicate with</param>
        /// <param name="database">Name of the messaging database. Defaults to "hydra"</param>
        /// <param name="port">Port number of the messaging database. defaults to 5984</param>
        /// <param name="pollIntervalMs">Optional polling interval of the database, in milliseconds</param>
        public NearestServerConfigProvider(IEnumerable<string> hydraServers, string database = DefaultDatabase, int port = DefaultPort, int? pollIntervalMs = null)
            :base(hydraServers, database, port, pollIntervalMs)
        {
            Start();
        }

        protected override void PreStop()
        {
            if (_closestSubscription != null) {
                _closestSubscription.Dispose();
                _closestSubscription = null;
            }
            _closestServer = null;
        }

        protected override void PostStart()
        {
            // Block until the first server response
            HydraServer = Distances.First(IsNewClosest).Address;
            _closestSubscription = Distances.Where(IsNewClosest).Subscribe(sdi => HydraServer = sdi.Address);
        }

        private bool IsNewClosest(ServerDistanceInfo sdi)
        {
            if (sdi.IsReachable && (_closestServer == null || (_closestServer != sdi.Address && sdi.Distance < Distances.ServerInfo[_closestServer].Distance - Tolerance))) {
                _closestServer = sdi.Address;
                return true;
            }
            return false;
        }

        #region Measure distance

        protected override void InitDistance(IEnumerable<string> servers)
        {
            // Do a one-off poll of everything and discard the results, but only wait 1.5 seconds in case of very slow responses. For some reason the first connection to
            // a CouchDb server on localhost takes one second for the TCP connect phase, so this gets over that initial slow poll.
            Task.WaitAll(servers.Select(server => Task.Factory.StartNew(() => { MeasureDistance(server); })).ToArray(), 1500);
        }

        #endregion

    }
}
