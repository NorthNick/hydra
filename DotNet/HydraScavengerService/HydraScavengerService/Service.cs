using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Timers;
using Bollywell.Hydra.Messaging;
using Bollywell.Hydra.Messaging.Config;
using LoveSeat;
using Newtonsoft.Json.Linq;

namespace HydraScavengerService
{
    public partial class Service : ServiceBase
    {
        private const string EventSource = "Hydra Scavenger Service";
        private const string EventLogName = "Application";

        private int _expiryDays;
        private Timer _timer;
        private int _pollIntervalSeconds;

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

            _pollIntervalSeconds = int.Parse(ConfigurationManager.AppSettings["PollIntervalSeconds"]);
            _expiryDays = int.Parse(ConfigurationManager.AppSettings["MessageExpiryDays"]);
            Services.DbConfigProvider = new AppDbConfigProvider(ConfigurationManager.AppSettings["HydraServer"], ConfigurationManager.AppSettings["Database"]);

            _timer = new Timer();
            _timer.Elapsed += TimerOnElapsed;
            _timer.Interval = _pollIntervalSeconds * 1000;
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
                ScavengeMessages();
            }
            catch (Exception ex) {
                EventLogger.WriteEntry(string.Format(EventSource + " error: {0}", ex), EventLogEntryType.Error);
            } finally {
                _timer.Interval = _pollIntervalSeconds * 1000;
                _timer.Enabled = true;
            }

        }

        #region Maintenance methods

        private void ScavengeMessages()
        {
                var messagingConfig = Services.GetConfig();
                var options = new ViewOptions();
                options.StartKey.Add(TransportMessage.MessageIdForDate(new DateTime(1970, 1, 1)).ToDocId());
                options.EndKey.Add(TransportMessage.MessageIdForDate(DateTime.Now.AddDays(-_expiryDays)).ToDocId());
                var db = new CouchClient(messagingConfig.HydraServer, 5984, null, null, false, AuthenticationType.Basic).GetDatabase(messagingConfig.Database);
                // Use the _bulk_docs interface to delete messages, supplying JSON of the form
                // { "docs": [{"_id": "xxx", "_rev": "abc", "_deleted": true}, {"_id": "yyy", "_rev": "def", "_deleted": true}, ...] }
                var docs = db.GetAllDocuments(options).Rows.Select(row => BulkDeleteDoc(row["id"].Value<string>(), row["value"]["rev"].Value<string>())).ToList();
                db.SaveDocuments(new Documents { Values = docs }, false);
        }

        private static Document BulkDeleteDoc(string id, string rev)
        {
            var jobj = new JObject();
            jobj["_deleted"] = true;
            return new Document(jobj) { Id = id, Rev = rev };
        }

        #endregion
    }
}
