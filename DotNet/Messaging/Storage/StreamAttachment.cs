using System.IO;

namespace Shastra.Hydra.Messaging.Storage
{
    class StreamAttachment : Attachment
    {
        private const string DefaultContentType = "application/octet-stream";

        public Stream Data { get; private set; }
        public string ContentType { get; private set; }

        public StreamAttachment(string name, Stream data, string contentType = DefaultContentType) : base(name)
        {
            Data = data;
            ContentType = contentType;
        }
    }
}
