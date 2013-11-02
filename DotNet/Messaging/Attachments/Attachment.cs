
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
    }
}
