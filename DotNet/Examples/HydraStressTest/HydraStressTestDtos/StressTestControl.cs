using System.Runtime.Serialization;

namespace HydraStressTestDtos
{
    [DataContract]
    public class StressTestControl
    {
        [DataMember] public bool Listen { get; set; }
        [DataMember] public bool Send { get; set; }
        [DataMember] public int SendIntervalMs { get; set; }
        [DataMember] public int SendBatchSize { get; set; }
        [DataMember] public int SendMaxDataLength { get; set; }
        [DataMember] public long BufferDelayMs { get; set; }
    }
}
