using System.Runtime.Serialization;

namespace Bollywell.Hydra.Messaging
{
    [DataContract]
    public class HydraMessage : TransportMessage
    {
        [DataMember] public string Source { get; set; }
        [DataMember] public string Destination { get; set; }
        [DataMember] public string Topic { get; set; }
        [DataMember] public string Subject { get; set; }
        [DataMember] public string Handle { get; set; }
        [DataMember] public string Data { get; set; }
    }
}
