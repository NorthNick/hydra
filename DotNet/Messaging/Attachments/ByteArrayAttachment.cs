
namespace Shastra.Hydra.Messaging.Attachments
{
    public class ByteArrayAttachment : Attachment
    {
        private const string DefaultContentType = "application/octet-stream";

        public byte[] Data { get; private set; }

        public ByteArrayAttachment(string name, byte[] data, string contentType = DefaultContentType) : base(name, contentType)
        {
            Data = data;
        }    
    }
}
