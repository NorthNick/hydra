using System.Net.Http;
using Shastra.Hydra.Messaging.MessageIds;

namespace Shastra.Hydra.Messaging.Attachments
{
    public class Attachment
    {
        public string ContentType { get; private set; }
        public IMessageId MessageId { get; set; }
        public string Name { get; private set; }

        // This base type is only for use internally, hence the constructor restriction
        internal Attachment(string name, string contentType, IMessageId messageId = null)
        {
            Name = name;
            ContentType = contentType;
            MessageId = messageId;
        }

        public virtual int DataLength() { return 0; }

        internal virtual HttpContent ToHttpContent() { return null; }
    }
}
