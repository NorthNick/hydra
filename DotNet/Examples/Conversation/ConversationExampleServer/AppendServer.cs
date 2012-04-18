using Bollywell.Hydra.Conversation;
using Bollywell.Hydra.ConversationExampleDto;

namespace Bollywell.Hydra.ConversationExampleServer
{
    class AppendServer : ConversationBase<ConversationDto>
    {
        private string _suffix;

        public override void OnMessage(ConversationDto message)
        {
            // Ignore invalid messages
            switch (message.MessageType) {
                    case MessageTypes.Init:
                    _suffix = message.Data;
                    Send(new ConversationDto { MessageType = MessageTypes.Ack });
                    break;
                case MessageTypes.Request:
                    Send(new ConversationDto { MessageType = MessageTypes.Response, Data = message.Data + _suffix });
                    break;
                case MessageTypes.End:
                    Done();
                    break;
            }
        }

    }
}
