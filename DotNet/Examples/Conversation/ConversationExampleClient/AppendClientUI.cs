using System;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Forms;
using Shastra.Hydra.ConversationExampleDto;
using Shastra.Hydra.Conversations;
using Shastra.Hydra.Messaging;

namespace Shastra.Hydra.ConversationExampleClient
{
    public partial class AppendClientUi : UserControl
    {
        private Conversation<ConversationDto> _conversation;
        private IDisposable _subscription;

        public AppendClientUi()
        {
            InitializeComponent();
        }

        public void Init(Conversation<ConversationDto> conversation, string suffix)
        {
            HandleLbl.Text += " " + conversation.Handle;
            SuffixLbl.Text += " " + suffix;

            _conversation = conversation;
            _subscription = _conversation.SkipErrors().ObserveOn(SynchronizationContext.Current).Subscribe(OnNext);
            _conversation.SendAsync(new ConversationDto { MessageType = MessageTypes.Init, Data = suffix });
        }

        private void OnNext(AugmentedMessage<ConversationDto> message)
        {
            ResponseLbl.Text = string.Format("Last response: {0}, {1}", message.Message.MessageType, message.Message.Data);
        }

        private void RequestBtn_Click(object sender, EventArgs e)
        {
            _conversation.SendAsync(new ConversationDto { MessageType = MessageTypes.Request, Data = RequestBox.Text });
        }

        private void EndBtn_Click(object sender, EventArgs e)
        {
            _conversation.SendAsync(new ConversationDto { MessageType = MessageTypes.End });
            _subscription.Dispose();
            _conversation.Dispose();
        }
    }
}
