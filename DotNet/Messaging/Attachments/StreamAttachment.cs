using System.IO;
using System.Net.Http;

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

        #region Overrides of Attachment

        public override int DataLength() { return (int)Data.Length; }

        internal override HttpContent ToHttpContent() { return new StreamContent(Data); }

        #endregion
    }
}
