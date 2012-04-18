using System;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Forms;
using Bollywell.Hydra.ConversationExampleDto;
using Bollywell.Hydra.Conversations;

namespace Bollywell.Hydra.ConversationExampleClient
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
            _subscription = _conversation.ObserveOn(SynchronizationContext.Current).Subscribe(OnNext);
            _conversation.Send(new ConversationDto { MessageType = MessageTypes.Init, Data = suffix });
        }

        private void OnNext(ConversationDto message)
        {
            ResponseLbl.Text = string.Format("Last response: {0}, {1}", message.MessageType, message.Data);
        }

        private void RequestBtn_Click(object sender, EventArgs e)
        {
            _conversation.Send(new ConversationDto { MessageType = MessageTypes.Request, Data = RequestBox.Text });
        }

        private void EndBtn_Click(object sender, EventArgs e)
        {
            _conversation.Send(new ConversationDto { MessageType = MessageTypes.End });
            _subscription.Dispose();
            _conversation.Dispose();
        }
    }
}
