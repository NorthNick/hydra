
namespace Bollywell.Hydra.Messaging.Storage
{
    public interface IServerDistanceInfo
    {
        string Name { get; set; }
        bool IsReachable { get; set; }
        long Distance { get; set; }
    }

    public class ServerDistanceInfo : IServerDistanceInfo
    {
        public string Name { get; set; }
        public bool IsReachable { get; set; }
        public long Distance { get; set; }
    }
}
