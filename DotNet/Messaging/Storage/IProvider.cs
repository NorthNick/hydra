namespace Shastra.Hydra.Messaging.Storage
{
    public interface IProvider
    {
        IStore GetStore(bool waitForInitialisation);
        string HydraServer { get; }
        void ServerError(string server);
    }
}
