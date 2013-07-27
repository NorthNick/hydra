using System;

namespace Shastra.Hydra.Tests.Mocks
{
    internal class DocInfo
    {
        public string Destination { get; private set; }
        public string DocId { get; private set; }
        public DateTime Timestamp { get; private set; }
        public string Topic { get; private set; }

        public DocInfo(string docId, string topic, string destination, DateTime timestamp)
        {
            DocId = docId;
            Topic = topic;
            Destination = destination;
            Timestamp = timestamp;
        }
    }
}