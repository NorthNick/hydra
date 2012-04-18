using System;
using Bollywell.Hydra.Conversation;
using Bollywell.Hydra.ConversationExampleDto;

namespace Bollywell.Hydra.ConversationExampleClient
{
    public class AppendClient : ConversationBase<ConversationDto>
    {
        public string Suffix { get; private set; }

        public event Action<ConversationDto> Message;

        public void Init(string suffix)
        {
            Suffix = suffix;
            Send(new ConversationDto {MessageType = MessageTypes.Init, Data = suffix});
        }

        public void Request(string data)
        {
            Send(new ConversationDto {MessageType = MessageTypes.Request, Data = data});
        }

        public void End()
        {
            Send(new ConversationDto {MessageType = MessageTypes.End});
            Done();
        }

        #region Overrides of ConversationBase<ConversationDto>

        public override void OnMessage(ConversationDto message)
        {
            if (Message != null) Message(message);
        }

        #endregion
    }
}
