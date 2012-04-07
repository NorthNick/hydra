
namespace Bollywell.Hydra.Messaging.Config
{
    public class AppDbConfigProvider : IDbConfigProvider
    {
        private static MessagingConfig _theConfig;

        /// <summary>
        /// Initialise messaging. Must be called before any attempt to send or listen.
        /// </summary>
        /// <param name="hydraServer">Hydra server to communicate with</param>
        /// <param name="database">Name of the messaging database</param>
        /// <param name="pollIntervalMs">Optional polling interval of the database, in milliseconds</param>
        public AppDbConfigProvider(string hydraServer, string database, int? pollIntervalMs = null)
        {
            Update(hydraServer, database, pollIntervalMs);
        }

        public IMessagingConfig GetConfig()
        {
            return _theConfig;
        }

        /// <summary>
        /// Initialise messaging. Must be called before any attempt to send or listen.
        /// </summary>
        /// <param name="hydraServer">Hydra server to communicate with</param>
        /// <param name="database">Name of the messaging database</param>
        /// <param name="pollIntervalMs">Optional polling interval of the database, in milliseconds</param>
        public void Update(string hydraServer, string database, int? pollIntervalMs = null)
        {
            _theConfig = new MessagingConfig(database, hydraServer, pollIntervalMs);
        }
    }
}
