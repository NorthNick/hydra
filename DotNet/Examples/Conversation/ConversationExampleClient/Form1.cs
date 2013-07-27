using System;
using System.Configuration;
using System.Linq;
using System.Windows.Forms;
using Shastra.Hydra.ConversationExampleDto;
using Shastra.Hydra.Conversations;
using Shastra.Hydra.Messaging;
using Shastra.Hydra.Messaging.Listeners;
using Shastra.Hydra.Messaging.Storage;

namespace Shastra.Hydra.ConversationExampleClient
{
    public partial class Form1 : Form
    {
        private const string MyName = "AppendClient";
        private readonly Switchboard<ConversationDto> _switchboard;

        public Form1()
        {
            InitializeComponent();

            string pollSetting = ConfigurationManager.AppSettings["PollIntervalMs"];
            int? pollIntervalMs = pollSetting == null ? (int?) null : int.Parse(pollSetting);
            var servers = ConfigurationManager.AppSettings["HydraServers"].Split(',').Select(s => s.Trim());
            var port = int.Parse(ConfigurationManager.AppSettings["Port"]);
            var hydraService = new StdHydraService(new NearestServerProvider(servers, ConfigurationManager.AppSettings["Database"], port), new ListenerOptions { PollIntervalMs = pollIntervalMs });

            _switchboard = new Switchboard<ConversationDto>(hydraService, MyName);
        }

        private void NewBtn_Click(object sender, EventArgs e)
        {
            var client = _switchboard.NewConversation("AppendServer");
            var clientUi = new AppendClientUi();
            clientUi.Init(client, SuffixBox.Text);
            ClientPanel.Controls.Add(clientUi);
        }

    }
}
