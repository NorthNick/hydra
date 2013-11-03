
using System.Net.Http;

namespace Shastra.Hydra.Messaging.Attachments
{
    public class StringAttachment : Attachment
    {
        private const string TextContentType = "text/plain";

        public string Data { get; private set; }

        public StringAttachment(string name, string data) : base(name, TextContentType)
        {
            Data = data;
        }

        #region Overrides of Attachment

        public override int DataLength() { return Data.Length; }

        public override HttpContent ToHttpContent() { return new StringContent(Data); }

        #endregion
    }
}
