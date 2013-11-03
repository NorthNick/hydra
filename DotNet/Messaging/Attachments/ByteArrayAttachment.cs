using System.Net.Http;

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

        #region Overrides of Attachment

        public override int DataLength() { return Data.Length; }

        public override HttpContent ToHttpContent() { return new ByteArrayContent(Data); }

        #endregion
    }
}
