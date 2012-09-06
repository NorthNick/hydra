namespace Bollywell.Hydra.Messaging.Config
{
    public interface IConfigProvider
    {
        IStore GetStore();
        string HydraServer { get; }
        void ServerError(string server);
    }
}
