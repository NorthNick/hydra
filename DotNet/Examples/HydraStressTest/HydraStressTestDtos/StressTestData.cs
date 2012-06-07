using System;
using System.Runtime.Serialization;

namespace HydraStressTestDtos
{
    [DataContract(Namespace = "")]
    public class StressTestData
    {
        [DataMember] public string Sender { get; set; }
        [DataMember] public string Username { get; set; }
        [DataMember] public string Domain { get; set; }
        [DataMember] public DateTime Timestamp { get; set; }
        [DataMember] public long Seq { get; set; }
        [DataMember] public string Data { get; set; }
    }
}
