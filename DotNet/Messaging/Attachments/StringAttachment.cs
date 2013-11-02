
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
    }
}
