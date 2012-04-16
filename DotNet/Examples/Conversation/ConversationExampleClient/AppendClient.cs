using System;
using System.Windows.Forms;
using Bollywell.Hydra.ConversationExampleDto;

namespace Bollywell.Hydra.ConversationExampleClient
{
    public partial class AppendClient : UserControl
    {
        private string _suffix;
        private string _handle;
        private Action<ConversationDto, string> _sender;
        private IDisposable _subscription;

        public AppendClient()
        {
            InitializeComponent();
        }

        public void Init(string suffix, string handle, IObservable<ConversationDto> messageSource, Action<ConversationDto, string> sender)
        {
            _suffix = suffix;
            _handle = handle;
            _sender = sender;
            HandleLbl.Text += " " + _handle;
            SuffixLbl.Text += " " + _suffix;
            _subscription = messageSource.Subscribe(OnNext);
            _sender(new ConversationDto { MessageType = MessageTypes.Init, Data = _suffix }, _handle);
        }

        private void OnNext(ConversationDto message)
        {
            ResponseLbl.Text = string.Format("Last response: {0}, {1}", message.MessageType, message.Data);
        }

        private void RequestBtn_Click(object sender, EventArgs e)
        {
            _sender(new ConversationDto { MessageType = MessageTypes.Request, Data = RequestBox.Text }, _handle);
        }

        private void EndBtn_Click(object sender, EventArgs e)
        {
            _sender(new ConversationDto { MessageType = MessageTypes.End }, _handle);
            _subscription.Dispose();
        }
    }
}
