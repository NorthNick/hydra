using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Bollywell.Hydra.Messaging.MessageIds;
using LoveSeat;
using LoveSeat.Interfaces;
using Newtonsoft.Json.Linq;

namespace Bollywell.Hydra.Messaging.Storage
{
    internal class CouchDbStore : IStore
    {
        private const string DefaultDatabase = "hydra";
        private const int DefaultPort = 5984;
        private const string DesignDoc = "hydra";
        private readonly string _database;
        private readonly int _port;
        private readonly IDocumentDatabase _db;

        public string Name { get; private set; }

        public CouchDbStore(string server, string database = null, int? port = null) : this("", server, database, port)
        {
            Name = NameFromServerDetails(server, _database, _port);
        }

        public CouchDbStore(string name, string server, string database = null, int? port = null)
        {
            _database = database ?? DefaultDatabase;
            _port = port.HasValue ? port.Value : DefaultPort;
            Name = name;
            _db = new CouchClient(server, _port, null, null, false, AuthenticationType.Basic).GetDatabase(_database);
        }

        public IEnumerable<IMessageId> GetChanges(IMessageId startId, long sinceSeq, out long lastSeq)
        {
            // Get changes after sinceSeq, throw out non-messages e.g. design doc updates, and drop messages at or before _startId

            // Loveseat doesn't have a _changes call, so it has to be done like this.
            var changes = _db.GetDocument(string.Format("_changes?since={0}", sinceSeq));
            // Changes are returned as 
            // {"results":[{"seq":28312,"id":"04b8dbf49b5d2603","changes":[{"rev":"1-ea426b58321d93c39a3486cc4d55abe2"}]},
            //             ...
            //             {"seq":28313,"id":"_design/mce","changes":[{"rev":"9-4d4ec5b438064ab0d602f2ed2ea9ac34"}]}
            //            ],
            //  "last_seq":28313}
            lastSeq = (long) changes["last_seq"];
            return changes["results"].Select(jObj => (string) jObj["id"]).Where(MessageIdManager.IsMessageId).Select(MessageIdManager.Create)
                    .OrderBy(mId => mId).SkipWhile(mId => mId.CompareTo(startId) <= 0);
        }

        public long GetLastSeq()
        {
            // Getting the empty document returns database info as:
            // {"db_name":"hydra","doc_count":499245,"doc_del_count":273331,"update_seq":1045940,"purge_seq":0,"compact_running":false,"disk_size":604704891,"data_size":372365869,
            //  "instance_start_time":"1332323453803000","disk_format_version":6,"committed_update_seq":1045940}
            return (long) _db.GetDocument("")["update_seq"];
        }

        public IMessageId SaveDoc(string json)
        {
            // TODO: deal with the case where posting fails but raises a CouchDb {error:xxx, reason:xxx} object and not an exception.
            var jobj = _db.CreateDocument(json);
            return MessageIdManager.Create((string) jobj["id"]);
        }

        public IEnumerable<JToken> GetDocs(string viewName, IViewOptions options)
        {
            // TODO: check out whether LoveSeat can be improved to use IViewOptions for the second parameter type
            return _db.View(viewName, (ViewOptions) options, DesignDoc).Rows;
        }

        public ServerDistanceInfo MeasureDistance()
        {
            bool responseOk = false;
            long elapsed = 0;
            try {
                // This URL checks both that the server is up, and that the view index is up to date
                string url = string.Format("http://{0}:{1}/{2}/_design/hydra/_view/broadcastMessages?limit=0", Name, _port, _database);
                var timer = Stopwatch.StartNew();
                using (HttpWebResponse response = (HttpWebResponse) WebRequest.Create(url).GetResponse()) {
                    elapsed = timer.ElapsedMilliseconds;
                    responseOk = response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch (Exception) {
                // Swallow errors
            }
            return new ServerDistanceInfo { Name = Name, Distance = responseOk ? elapsed : long.MaxValue, IsReachable = responseOk };
        }

        public static string NameFromServerDetails(string server, string database, int port)
        {
            return string.Format("{0}:{1}:{2}", server, port, database);
        }
    }
}
