
namespace Bollywell.Hydra.Messaging.Config
{
    public interface IDbConfigProvider
    {
        IMessagingConfig GetConfig();
        void ServerError(string server);
    }
}
