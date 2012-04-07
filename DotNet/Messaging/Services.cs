using System;
using Bollywell.Hydra.Messaging.Config;

namespace Bollywell.Hydra.Messaging
{
    /// <summary>
    /// Manages knowledge on the location of the Message Centre to poll. When OPS.Cloud is running it should update its
    /// info periodically and change the config if necessary.
    /// </summary>
    public class Services
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
    }
}
