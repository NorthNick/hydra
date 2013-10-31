using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Timers;
using Shastra.Hydra.Messaging.MessageIds;
using Newtonsoft.Json.Linq;
using Shastra.Hydra.Messaging.Storage;

namespace HydraScavengerService
{
    public partial class Service : ServiceBase
    {
        private const string EventSource = "Hydra Scavenger Service";
        private const string EventLogName = "Application";

        private Timer _timer;

        public Service()
        {
            InitializeComponent();
            if (!EventLog.SourceExists(EventSource)) {
                EventLog.CreateEventSource(EventSource, EventLogName);
            }
            EventLogger.Source = EventSource;
            EventLogger.Log = EventLogName;
        }

        #region Service events

        protected override void OnStart(string[] args)
        {
            EventLogger.WriteEntry(EventSource + " starting", EventLogEntryType.Information);

            _timer = new Timer();
            _timer.Elapsed += TimerOnElapsed;
            _timer.Interval = int.Parse(ConfigurationManager.AppSettings["PollIntervalSeconds"]) * 1000;
            _timer.Enabled = true;

            EventLogger.WriteEntry(EventSource + " started", EventLogEntryType.Information);
        }

        protected override void OnStop()
        {
            _timer.Enabled = false;
            EventLogger.WriteEntry(EventSource + " stopped", EventLogEntryType.Information);
        }

        #endregion

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _timer.Enabled = false;
            try {
                var deleteBatchSize = int.Parse(ConfigurationManager.AppSettings["DeleteBatchSize"]);
                ScavengeByDate(deleteBatchSize);
                ScavengeByNumber(deleteBatchSize);
            }
            catch (Exception ex) {
                EventLogger.WriteEntry(string.Format(EventSource + " error: {0}", ex), EventLogEntryType.Error);
            } finally {
                _timer.Interval = int.Parse(ConfigurationManager.AppSettings["PollIntervalSeconds"]) * 1000;
                _timer.Enabled = true;
            }

        }

        #region Maintenance methods

        private void ScavengeByDate(int deleteBatchSize)
        {
            if (ConfigurationManager.AppSettings["MessageExpiryDays"] == null) return;
            
            var expiryDays = double.Parse(ConfigurationManager.AppSettings["MessageExpiryDays"]);
            var options = new ViewOptions {
                StartKey = new KeyOptions(MessageIdManager.Create(new DateTime(1970, 1, 1)).ToDocId()), 
                EndKey = new KeyOptions(MessageIdManager.Create(DateTime.UtcNow.AddDays(-expiryDays)).ToDocId()), 
                Limit = deleteBatchSize
            };
            var db = new CouchDbClient(ConfigurationManager.AppSettings["HydraServer"], int.Parse(ConfigurationManager.AppSettings["Port"]), ConfigurationManager.AppSettings["Database"]);
            var rows = db.GetDoc("_all_docs", options)["rows"];
            while (rows.Any()) {
                DeleteDocs(rows, db);
                rows = db.GetDoc("_all_docs", options)["rows"];
            }
        }

        private void ScavengeByNumber(int deleteBatchSize)
        {
            if (ConfigurationManager.AppSettings["MaxDocsInDatabase"] == null) return;

            var maxDocs = long.Parse(ConfigurationManager.AppSettings["MaxDocsInDatabase"]);
            var options = new ViewOptions {
                StartKey = new KeyOptions(MessageIdManager.Create(new DateTime(1970, 1, 1)).ToDocId()), 
                EndKey = new KeyOptions(MessageIdManager.Create(DateTime.UtcNow).ToDocId())
            };
            var db = new CouchDbClient(ConfigurationManager.AppSettings["HydraServer"], int.Parse(ConfigurationManager.AppSettings["Port"]), ConfigurationManager.AppSettings["Database"]);
            // Getting the empty document returns general database info
            var deleteCount = db.GetDoc("").Value<long>("doc_count") - maxDocs;
            while (deleteCount > 0) {
                options.Limit = (int) Math.Min(deleteCount, deleteBatchSize);
                var rows = db.GetDoc("_all_docs")["rows"];
                if (!rows.Any()) break;
                DeleteDocs(rows, db);
                deleteCount -= deleteBatchSize;
            }
        }

        private static void DeleteDocs(IEnumerable<JToken> rows, CouchDbClient db)
        {
            // Use the _bulk_docs interface to delete messages, supplying JSON of the form
            // { "docs": [{"_id": "xxx", "_rev": "abc", "_deleted": true}, {"_id": "yyy", "_rev": "def", "_deleted": true}, ...] }
            var docs = new JArray(rows.Select(row => BulkDeleteDoc(row.Value<string>("id"), row["value"].Value<string>("rev"))).ToList());
            db.DeleteDocuments(docs);
        }

        private static readonly JProperty DeletedProperty = new JProperty("_deleted", true);

        private static JObject BulkDeleteDoc(string id, string rev)
        {
            return new JObject(new JProperty("_id", id), new JProperty("_rev", rev), DeletedProperty);
        }

        #endregion
    }
}
