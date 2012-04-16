using System;
using Bollywell.Hydra.ConversationExampleDto;

namespace Bollywell.Hydra.ConversationExampleServer
{
    class AppendServer
    {
        private readonly string _handle;
        private readonly string _source;
        private readonly string _suffix;
        private readonly Action<ConversationDto, string, string> _sender;
        private readonly IDisposable _subscription;

        public AppendServer(string handle, string source, string suffix, IObservable<ConversationDto> messageSource, Action<ConversationDto, string, string> sender)
        {
            _handle = handle;
            _source = source;
            _suffix = suffix;
            _sender = sender;
            _subscription = messageSource.Subscribe(OnNext);
            _sender(new ConversationDto {MessageType = MessageTypes.Ack}, _handle, _source);
        }

        private void OnNext(ConversationDto message)
        {
            // Ignore invalid messages
            switch (message.MessageType) {
                case MessageTypes.Request:
                    _sender(new ConversationDto { MessageType = MessageTypes.Response, Data = message.Data + _suffix}, _handle, _source);
                    break;
                case MessageTypes.End:
                    _subscription.Dispose();
                    break;
            }
        }
    }
}
