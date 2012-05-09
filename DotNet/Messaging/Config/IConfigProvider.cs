using LoveSeat.Interfaces;

namespace Bollywell.Hydra.Messaging.Config
{
    public interface IConfigProvider
    {
        IDocumentDatabase GetDb();
        string HydraServer { get; }
        int? PollIntervalMs { get; }
        void ServerError(string server);
    }
}
