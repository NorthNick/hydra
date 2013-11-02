using System.Collections.Generic;
using Shastra.Hydra.Messaging.Attachments;

namespace Shastra.Hydra.Messaging
{
    public class AugmentedMessage<TMessage>
    {
        public TMessage Message { get; set; }
        public IEnumerable<Attachment> Attachments { get; set; }

        public AugmentedMessage(TMessage message, IEnumerable<Attachment> attachments)
        {
            Message = message;
            Attachments = attachments;
        }
    }
}
