
namespace Bollywell.Hydra.Messaging.Storage
{
    public interface IServerDistanceInfo
    {
        string Address { get; set; }
        bool IsReachable { get; set; }
        long Distance { get; set; }
    }

    public class ServerDistanceInfo : IServerDistanceInfo
    {
        public string Address { get; set; }
        public bool IsReachable { get; set; }
        public long Distance { get; set; }
    }
}
