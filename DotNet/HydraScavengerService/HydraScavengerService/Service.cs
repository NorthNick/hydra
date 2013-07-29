using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Timers;
using Shastra.Hydra.Messaging.MessageIds;
using LoveSeat;
using LoveSeat.Interfaces;
using Newtonsoft.Json.Linq;

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
            var options = new ViewOptions();
            options.StartKey.Add(MessageIdManager.Create(new DateTime(1970, 1, 1)).ToDocId());
            options.EndKey.Add(MessageIdManager.Create(DateTime.UtcNow.AddDays(-expiryDays)).ToDocId());
            options.Limit = deleteBatchSize;
            var db = new CouchClient(ConfigurationManager.AppSettings["HydraServer"], int.Parse(ConfigurationManager.AppSettings["Port"]), null, null, false, AuthenticationType.Basic).
                GetDatabase(ConfigurationManager.AppSettings["Database"]);
            var rows = db.GetAllDocuments(options).Rows;
            while (rows.Any()) {
                DeleteDocs(rows, db);
                rows = db.GetAllDocuments(options).Rows;
            }
        }

        private void ScavengeByNumber(int deleteBatchSize)
        {
            if (ConfigurationManager.AppSettings["MaxDocsInDatabase"] == null) return;

            var maxDocs = long.Parse(ConfigurationManager.AppSettings["MaxDocsInDatabase"]);
            var options = new ViewOptions();
            options.StartKey.Add(MessageIdManager.Create(new DateTime(1970, 1, 1)).ToDocId());
            options.EndKey.Add(MessageIdManager.Create(DateTime.UtcNow).ToDocId());
            var db = new CouchClient(ConfigurationManager.AppSettings["HydraServer"], int.Parse(ConfigurationManager.AppSettings["Port"]), null, null, false, AuthenticationType.Basic).
                GetDatabase(ConfigurationManager.AppSettings["Database"]);
            // Getting the empty document returns general database info
            var deleteCount = db.GetDocument("").Value<long>("doc_count") - maxDocs;
            while (deleteCount > 0) {
                options.Limit = (int) Math.Min(deleteCount, deleteBatchSize);
                var rows = db.GetAllDocuments(options).Rows;
                if (!rows.Any()) break;
                DeleteDocs(rows, db);
                deleteCount -= deleteBatchSize;
            }
        }

        private static void DeleteDocs(IEnumerable<JToken> rows, IDocumentDatabase db)
        {
            var docs = rows.Select(row => BulkDeleteDoc(row.Value<string>("id"), row["value"].Value<string>("rev"))).ToList();
            db.SaveDocuments(new Documents { Values = docs }, false);
        }

        private static Document BulkDeleteDoc(string id, string rev)
        {
            // Use the _bulk_docs interface to delete messages, supplying JSON of the form
            // { "docs": [{"_id": "xxx", "_rev": "abc", "_deleted": true}, {"_id": "yyy", "_rev": "def", "_deleted": true}, ...] }
            var jobj = new JObject();
            jobj["_deleted"] = true;
            return new Document(jobj) { Id = id, Rev = rev };
        }

        #endregion
    }
}
