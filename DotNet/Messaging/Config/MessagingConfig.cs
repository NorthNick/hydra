
namespace Bollywell.Hydra.Messaging.Config
{
    class MessagingConfig : IMessagingConfig
    {
        public string Database { get; internal set; }
        public string HydraServer { get; internal set; }
        public int? PollIntervalMs { get; internal set; }

        public MessagingConfig(string database, string hydraServer, int? pollIntervalMs)
        {
            PollIntervalMs = pollIntervalMs;
            Database = database;
            HydraServer = hydraServer;
        }
    }
}
