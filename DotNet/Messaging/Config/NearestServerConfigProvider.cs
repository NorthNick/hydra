﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Threading;
using LoveSeat;
using LoveSeat.Interfaces;

namespace Bollywell.Hydra.Messaging.Config
{
    public class NearestServerConfigProvider : IConfigProvider
    {
        private readonly string _database;
        private readonly int _port = 5984;
        private readonly ServerDistance<ServerDistanceInfo> _distances;
        private IDisposable _subscription;

        public string HydraServer { get; private set; }
        public int? PollIntervalMs { get; private set; }

        /// <summary>
        /// Initialise messaging. Must be called before any attempt to send or listen.
        /// </summary>
        /// <param name="hydraServer">Hydra server to communicate with</param>
        /// <param name="database">Name of the messaging database</param>
        /// <param name="pollIntervalMs">Optional polling interval of the database, in milliseconds</param>
        public NearestServerConfigProvider(string hydraServer, string database, int? pollIntervalMs = null) : this(new List<string> {hydraServer}, database, pollIntervalMs) {}

        /// <summary>
        /// Initialise messaging. Must be called before any attempt to send or listen.
        /// </summary>
        /// <param name="hydraServers">Hydra servers to communicate with</param>
        /// <param name="database">Name of the messaging database</param>
        /// <param name="pollIntervalMs">Optional polling interval of the database, in milliseconds</param>
        public NearestServerConfigProvider(IEnumerable<string> hydraServers, string database, int? pollIntervalMs = null)
        {
            if (hydraServers == null || !hydraServers.Any()) throw new ArgumentException("At least one server must be supplied", "hydraServers");
            _database = database;
            PollIntervalMs = pollIntervalMs;
            _distances = new ServerDistance<ServerDistanceInfo>(hydraServers, MeasureDistance);
            Subscribe();
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
            if (server == HydraServer) {
                // The current server has gone offline. Restart _distances to force a repoll.
                _subscription.Dispose();
                _distances.Stop();
                Subscribe();
            }
        }

        private void Subscribe()
        {
            _distances.Start();
            // Block until the first server response
            HydraServer = _distances.First().Address;
            _subscription = _distances.Subscribe(sdi => HydraServer = sdi.Address);
        }

        #region Measure distance

        private ServerDistanceInfo MeasureDistance(string server)
        {
            var timer = Stopwatch.StartNew();
            bool responseOk = false;
            try {
                // This URL checks both that the server is up, and that the view index is up to date
                string url = string.Format("http://{0}:{1}/{2}/_design/hydra/_view/broadcastMessages?limit=0", server, _port, _database);
                HttpWebResponse response = (HttpWebResponse) WebRequest.Create(url).GetResponse();
                responseOk = response.StatusCode == HttpStatusCode.OK;
            } catch (Exception) {
                // Swallow errors
            }
            return new ServerDistanceInfo {Address = server, Distance = responseOk ? timer.ElapsedMilliseconds : long.MaxValue, IsReachable = responseOk};
        }
        #endregion
    }
}
