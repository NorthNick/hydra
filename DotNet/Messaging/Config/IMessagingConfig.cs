
namespace Bollywell.Hydra.Messaging.Config
{
    public interface IMessagingConfig
    {
        string Database { get; }
        string HydraServer { get; }
        int? PollIntervalMs { get; }
    }
}
