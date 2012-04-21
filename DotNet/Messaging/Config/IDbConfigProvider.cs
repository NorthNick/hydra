
namespace Bollywell.Hydra.Messaging.Config
{
    public interface IDbConfigProvider
    {
        IMessagingConfig GetConfig();
        /// <summary>
        /// Attempt to switch to another server.
        /// </summary>
        /// <returns>True if another server was found. False otherwise.</returns>
        bool SwitchServer();
    }
}
