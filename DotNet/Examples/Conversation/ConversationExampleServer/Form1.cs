using System.Configuration;
using System.Windows.Forms;
using Bollywell.Hydra.Conversation;
using Bollywell.Hydra.ConversationExampleDto;
using Bollywell.Hydra.Messaging;
using Bollywell.Hydra.Messaging.Config;

namespace Bollywell.Hydra.ConversationExampleServer
{
    public partial class Form1 : Form
    {
        private readonly Switchboard<AppendServer, ConversationDto> _switchboard;
        private const string MyName = "AppendServer";

        public Form1()
        {
            InitializeComponent();
            string pollSetting = ConfigurationManager.AppSettings["PollIntervalMs"];
            int? pollIntervalMs = pollSetting == null ? (int?) null : int.Parse(pollSetting);
            Services.DbConfigProvider = new AppDbConfigProvider(ConfigurationManager.AppSettings["HydraServer"], ConfigurationManager.AppSettings["Database"], pollIntervalMs);

            _switchboard = new Switchboard<AppendServer, ConversationDto>(MyName);
        }

    }
}
