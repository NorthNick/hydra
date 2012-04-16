using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reactive.Linq;
using System.Windows.Forms;
using Bollywell.Hydra.ConversationExampleDto;
using Bollywell.Hydra.Messaging;
using Bollywell.Hydra.Messaging.Config;
using Bollywell.Hydra.Messaging.MessageFetchers;
using Bollywell.Hydra.Messaging.Pollers;
using Bollywell.Hydra.Messaging.Serializers;

namespace Bollywell.Hydra.ConversationExampleServer
{
    public partial class Form1 : Form
    {
        private const string MyName = "AppendServer";
        private readonly IPoller<HydraMessage> _poller;
        private readonly static HydraDataContractSerializer<ConversationDto> Serializer = new HydraDataContractSerializer<ConversationDto>();
        private readonly IObservable<Tuple<ConversationDto, HydraMessage>> _messageSource;
        private readonly HashSet<AppendServer> _servers = new HashSet<AppendServer>();

        public Form1()
        {
            InitializeComponent();
            string pollSetting = ConfigurationManager.AppSettings["PollIntervalMs"];
            int? pollIntervalMs = pollSetting == null ? (int?) null : int.Parse(pollSetting);
            Services.DbConfigProvider = new AppDbConfigProvider(ConfigurationManager.AppSettings["HydraServer"], ConfigurationManager.AppSettings["Database"], pollIntervalMs);

            // Poll for messages with topic "AppendConversation", and destination "AppendServer"
            _poller = new Poller<HydraMessage>(new HydraByTopicByDestinationMessageFetcher(ConversationDto.ConversationTopic, MyName));
            // Deserialise the content of incoming message to ConversationDto objects, and keep the original message
            _messageSource = _poller.Select(hydraMessage => Tuple.Create(Serializer.Deserialize(hydraMessage.Data), hydraMessage));
            // Send Init messages to the OnInit method to create a new AppendServer
            _messageSource.Where(messagePair => messagePair.Item1.MessageType == MessageTypes.Init).Subscribe(OnInit);
        }

        private void OnInit(Tuple<ConversationDto, HydraMessage> messagePair)
        {
            _servers.Add(new AppendServer(messagePair.Item2.Handle, messagePair.Item2.Source, messagePair.Item1.Data, 
                _messageSource.Where(pair => pair.Item2.Handle == messagePair.Item2.Handle).Select(pair => pair.Item1), Send));
        }

        private static void Send(ConversationDto message, string handle, string destination)
        {
            var hydraMessage = new HydraMessage {Topic = ConversationDto.ConversationTopic, Source = MyName, Destination = destination,
                                                 Handle = handle, Data = Serializer.Serialize(message)};
            hydraMessage.Send();
        }
    }
}
