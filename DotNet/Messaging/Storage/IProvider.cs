namespace Bollywell.Hydra.Messaging.Storage
{
    public interface IProvider
    {
        IStore GetStore();
        string HydraServer { get; }
        void ServerError(string server);
    }
}
