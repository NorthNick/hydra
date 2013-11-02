using System.IO;

namespace Shastra.Hydra.Messaging.Attachments
{
    public class StreamAttachment : Attachment
    {
        private const string DefaultContentType = "application/octet-stream";

        public Stream Data { get; private set; }

        public StreamAttachment(string name, Stream data, string contentType = DefaultContentType) : base(name, contentType)
        {
            Data = data;
        }
    }
}
