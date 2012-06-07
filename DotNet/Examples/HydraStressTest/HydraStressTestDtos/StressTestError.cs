using System;
using System.Runtime.Serialization;

namespace HydraStressTestDtos
{
    [DataContract(Namespace = "")]
    public class StressTestError
    {
        [DataMember] public string Sender { get; set; }
        [DataMember] public string Receiver { get; set; }
        [DataMember] public long ExpectedSeq { get; set; }
        [DataMember] public long ReceivedSeq { get; set; }
        [DataMember] public DateTime SendTime { get; set; }
        [DataMember] public DateTime ReceiveTime { get; set; }
        [DataMember] public int DataLength { get; set; }
    }
}
