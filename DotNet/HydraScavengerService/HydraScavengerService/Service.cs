using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.ServiceProcess;
using System.Timers;
using Bollywell.Hydra.Messaging;
using Bollywell.Hydra.Messaging.Config;
using LoveSeat;

namespace HydraScavengerService
{
    public partial class Service : ServiceBase
    {
        private const string EventSource = "Hydra Scavenger Service";
        private const string EventLogName = "Application";

        private TimeSpan _cleanupTime;
        private int _expiryDays;
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

            _cleanupTime = DateTime.Parse(ConfigurationManager.AppSettings["DailyCleanupTime"]).TimeOfDay;
            _expiryDays = int.Parse(ConfigurationManager.AppSettings["MessageExpiryDays"]);
            Services.DbConfigProvider = new AppDbConfigProvider(ConfigurationManager.AppSettings["HydraServer"], ConfigurationManager.AppSettings["Database"]);

            _timer = new Timer();
            _timer.Elapsed += TimerOnElapsed;
            _timer.Interval = GetExpiryInterval();
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
                // NDN 7/4/12 - removed Compact, as CouchDb 1.2 automates it.
                var results = new List<string> { ScavengeMessages() };

                EventLogger.WriteEntry(string.Format(EventSource + " daily run.{0}{1}", Environment.NewLine, string.Join(Environment.NewLine, results)));

            }
            catch (Exception ex) {
                EventLogger.WriteEntry(string.Format(EventSource + " daily run error: {0}", ex), EventLogEntryType.Error);
            }

            _timer.Interval = GetExpiryInterval();
            _timer.Enabled = true;

        }

        private double GetExpiryInterval()
        {
            var expiryInterval = DateTime.Now.Date.Add(_cleanupTime) - DateTime.Now;
            // If expiry is less than 1 minute in the future, move forward by a day
            if (expiryInterval < new TimeSpan(0, 1, 0)) expiryInterval += new TimeSpan(1, 0, 0, 0);

            return expiryInterval.TotalMilliseconds;
        }

        #region Maintenance methods

        private string ScavengeMessages()
        {
            try {
                // Scavenge messages
                var messagingConfig = Services.GetConfig();
                var options = new ViewOptions();
                options.StartKey.Add(TransportMessage.MessageIdForDate(new DateTime(1970, 1, 1)).ToDocId());
                options.EndKey.Add(TransportMessage.MessageIdForDate(DateTime.Now.AddDays(-_expiryDays)).ToDocId());
                var db = new CouchClient(messagingConfig.HydraServer, 5984, null, null, false, AuthenticationType.Basic).GetDatabase(messagingConfig.Database);
                int count = 0;
                // NOTE: This code deletes the docs one at a time. It would probably be more efficient to use the _bulk_docs interface and supply JSON of the form
                // { "docs": [{"_id": "xxx", "_rev": "abc", "_delete": true}, {"_id": "yyy", "_rev": "def", "_delete": true}, ...] }
                // See http://www.couchbase.org/sites/default/files/uploads/all/documentation/couchbase-api-db.html#couchbase-api-db_db-bulk-docs_post
                // Loveseat now has access to _bulk_docs so really should do this...
                foreach (var row in db.GetAllDocuments(options).Rows) {
                    db.DeleteDocument((string) row["id"], (string) row["value"]["rev"]);
                    count++;
                }
                return string.Format("Scavenged {0} messages", count);
            }
            catch (Exception ex) {
                return string.Format("Scavenging error: {0}", ex);
            }
        }

        #endregion
    }
}
