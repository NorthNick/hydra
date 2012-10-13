namespace Bollywell.Hydra.Messaging.Storage
{
    public interface IProvider
    {
        IStore GetStore(bool waitForInitialisation);
        string HydraServer { get; }
        bool IsOffline { get; }
        void ServerError(string server);
    }
}
