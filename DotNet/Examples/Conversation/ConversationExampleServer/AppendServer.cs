using System;
using Shastra.Hydra.ConversationExampleDto;
using Shastra.Hydra.Conversations;
using Shastra.Hydra.Messaging;

namespace Shastra.Hydra.ConversationExampleServer
{
    class AppendServer
    {
        private readonly Conversation<ConversationDto> _conversation;
        private readonly IDisposable _subscription;
        private string _suffix;

        public AppendServer(Conversation<ConversationDto> conversation)
        {
            _conversation = conversation;
            _subscription = conversation.SkipErrors().Subscribe(OnNext);
        }

        private void OnNext(ConversationDto message)
        {
            // Ignore invalid messages
            switch (message.MessageType) {
                    case MessageTypes.Init:
                    _suffix = message.Data;
                    _conversation.Send(new ConversationDto { MessageType = MessageTypes.Ack });
                    break;
                case MessageTypes.Request:
                    _conversation.Send(new ConversationDto { MessageType = MessageTypes.Response, Data = message.Data + _suffix });
                    break;
                case MessageTypes.End:
                    _subscription.Dispose();
                    _conversation.Dispose();
                    break;
            }
        }

    }
}
