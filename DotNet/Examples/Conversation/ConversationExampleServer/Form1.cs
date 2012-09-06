using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows.Forms;
using Bollywell.Hydra.Conversations;
using Bollywell.Hydra.ConversationExampleDto;
using Bollywell.Hydra.Messaging;
using Bollywell.Hydra.Messaging.Config;
using Bollywell.Hydra.Messaging.Listeners;

namespace Bollywell.Hydra.ConversationExampleServer
{
    public partial class Form1 : Form
    {
        private const string MyName = "AppendServer";
        private readonly HashSet<AppendServer> _servers = new HashSet<AppendServer>();

        public Form1()
        {
            InitializeComponent();
            string pollSetting = ConfigurationManager.AppSettings["PollIntervalMs"];
            int? pollIntervalMs = pollSetting == null ? (int?) null : int.Parse(pollSetting);
            var servers = ConfigurationManager.AppSettings["HydraServers"].Split(',').Select(s => s.Trim());
            var port = int.Parse(ConfigurationManager.AppSettings["Port"]);
            var hydraService = new HydraService(new RoundRobinConfigProvider(servers, ConfigurationManager.AppSettings["Database"], port), new ListenerOptions { PollIntervalMs = pollIntervalMs });

            new Switchboard<ConversationDto>(hydraService, MyName).Subscribe(OnNext);
        }

        private void OnNext(Conversation<ConversationDto> conversation)
        {
            _servers.Add(new AppendServer(conversation));
        }
    }
}
