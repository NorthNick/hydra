using System;
using Bollywell.Hydra.Messaging.Config;
using LoveSeat;

namespace Bollywell.Hydra.Messaging
{
    /// <summary>
    /// Manages knowledge on the location of the Hydra server to poll. In a cloud environment it should update its
    /// info periodically and change the config if necessary.
    /// </summary>
    public static class Services
    {
        public static IDbConfigProvider DbConfigProvider { get; set; }

        static Services()
        {
            // Set defaults for properties by talking to the cloud
        }

        public static IMessagingConfig GetConfig()
        {
            if (DbConfigProvider == null) throw new Exception("Services.DbConfigProvider has not been initialised");
            return DbConfigProvider.GetConfig();
        }

        public static CouchDatabase GetDb()
        {
            var config = GetConfig();
            return new CouchClient(config.HydraServer, 5984, null, null, false, AuthenticationType.Basic).GetDatabase(config.Database);
        }

        public static void ServerError(string server)
        {
            DbConfigProvider.ServerError(server);
        }
    }
}
