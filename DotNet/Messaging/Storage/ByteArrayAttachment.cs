
namespace Shastra.Hydra.Messaging.Storage
{
    public class ByteArrayAttachment : Attachment
    {
        private const string DefaultContentType = "application/octet-stream";

        public byte[] Data { get; private set; }
        public string ContentType { get; private set; }

        public ByteArrayAttachment(string name, byte[] data, string contentType = DefaultContentType) : base(name)
        {
            Data = data;
            ContentType = contentType;
        }    
    }
}
