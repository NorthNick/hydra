using System;
using System.Windows.Forms;
using Bollywell.Hydra.ConversationExampleDto;

namespace Bollywell.Hydra.ConversationExampleClient
{
    public partial class AppendClientUi : UserControl
    {
        private AppendClient _appendClient;

        public AppendClientUi()
        {
            InitializeComponent();
        }

        public void Init(AppendClient appendClient)
        {
            _appendClient = appendClient;
            _appendClient.Message += OnMessage;

            HandleLbl.Text += " " + appendClient.Handle;
            SuffixLbl.Text += " " + appendClient.Suffix;
        }

        private void OnMessage(ConversationDto message)
        {
            this.Invoke(new Action(() => ResponseLbl.Text = string.Format("Last response: {0}, {1}", message.MessageType, message.Data)));
        }

        private void RequestBtn_Click(object sender, EventArgs e)
        {
            _appendClient.Request(RequestBox.Text);
        }

        private void EndBtn_Click(object sender, EventArgs e)
        {
            _appendClient.End();
        }
    }
}
