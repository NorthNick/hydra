using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Forms;
using Shastra.Hydra.Messaging;
using Shastra.Hydra.Messaging.MessageFetchers;
using Shastra.Hydra.Messaging.Listeners;
using Shastra.Hydra.Messaging.Serializers;
using Shastra.Hydra.Messaging.Storage;
using Shastra.Hydra.PubSubByType;
using HydraStressTestDtos;

namespace HydraStressTestConsole
{
    public partial class MainFrm : Form
    {
        private readonly Dictionary<string, ClientControl> _clients = new Dictionary<string, ClientControl>();
        private readonly Subscriber<StressTestData> _dataSubscriber;
        private IDisposable _dataSubscription;
        private readonly Subscriber<StressTestError> _errorSubscriber;
        private IDisposable _errorSubscription;
        private static readonly ISerializer<StressTestControl> Serializer = new HydraDataContractSerializer<StressTestControl>();
        private readonly IListener<HydraMessage> _listener;
        private IDisposable _controlSubscription;
        private static Publisher<StressTestControl> _controlPublisher;

        public MainFrm()
        {
            InitializeComponent();

            WebRequest.DefaultWebProxy = null;
            var pinger = new Pinger(ConfigurationManager.AppSettings["HydraServers"].Split(',').Select(s => s.Trim()));
            pinger.RefreshSync();
            var servers = pinger.ServerInfo.Where(si => si.IsReachable).Select(si => si.Address);

            string pollSetting = ConfigurationManager.AppSettings["PollIntervalMs"];
            int? pollIntervalMs = pollSetting == null ? (int?)null : int.Parse(pollSetting);
            var hydraService = new StdHydraService(new NearestServerProvider(servers, ConfigurationManager.AppSettings["Database"], 5984), new ListenerOptions { PollIntervalMs = pollIntervalMs });

            _dataSubscriber = new Subscriber<StressTestData>(hydraService);
            _dataSubscription = _dataSubscriber.ObserveOn(SynchronizationContext.Current).SkipErrors().Subscribe(OnDataRecv);
            _errorSubscriber = new Subscriber<StressTestError>(hydraService);
            _errorSubscription = _errorSubscriber.ObserveOn(SynchronizationContext.Current).SkipErrors().Subscribe(OnErrorRecv);
            _listener = hydraService.GetListener(new HydraByTopicByDestinationMessageFetcher(typeof(StressTestControl).FullName, "StressTestConsole"));
            _controlSubscription = _listener.ObserveOn(SynchronizationContext.Current).Subscribe(OnControlRecv);
            _controlPublisher = new Publisher<StressTestControl>(hydraService) {ThisParty = "StressTestConsole"};
        }

        internal static void Send(StressTestControl message, string destination)
        {
            _controlPublisher.Send(message, destination);
        }

        private void OnDataRecv(StressTestData message)
        {
            GetClientControl(message.Sender).DataMessage(message);
        }

        private void OnErrorRecv(StressTestError message)
        {
            GetClientControl(message.Receiver).ErrorMessage(message);
        }

        private void OnControlRecv(HydraMessage message)
        {
            GetClientControl(message.Source).ControlMessage(Serializer.Deserialize(message.Data));
        }

        private ClientControl GetClientControl(string clientId)
        {
            ClientControl res;
            if (!_clients.TryGetValue(clientId, out res)) {
                _clients.Add(clientId, new ClientControl());
                res = _clients[clientId];
                res.ClientId = clientId;
                ClientPanel.Controls.Add(res);
            }
            return res;
        }
    }
}
