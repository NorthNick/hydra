using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Forms;
using Bollywell.Hydra.ConversationExampleDto;
using Bollywell.Hydra.Messaging;
using Bollywell.Hydra.Messaging.Config;
using Bollywell.Hydra.Messaging.MessageFetchers;
using Bollywell.Hydra.Messaging.Pollers;
using Bollywell.Hydra.Messaging.Serializers;

namespace Bollywell.Hydra.ConversationExampleClient
{
    public partial class Form1 : Form
    {
        private const string MyName = "AppendClient";
        private readonly IPoller<HydraMessage> _poller;
        private readonly static HydraDataContractSerializer<ConversationDto> Serializer = new HydraDataContractSerializer<ConversationDto>();
        private readonly IObservable<Tuple<ConversationDto, HydraMessage>> _messageSource;

        public Form1()
        {
            InitializeComponent();

            string pollSetting = ConfigurationManager.AppSettings["PollIntervalMs"];
            int? pollIntervalMs = pollSetting == null ? (int?) null : int.Parse(pollSetting);
            Services.DbConfigProvider = new AppDbConfigProvider(ConfigurationManager.AppSettings["HydraServer"], ConfigurationManager.AppSettings["Database"], pollIntervalMs);

            // Poll for messages with topic "AppendConversation", and destination "AppendClient"
            _poller = new Poller<HydraMessage>(new HydraByTopicByDestinationMessageFetcher(ConversationDto.ConversationTopic, MyName));
            // Deserialise the content of incoming message to ConversationDto objects, and keep the original message
            _messageSource = _poller.ObserveOn(SynchronizationContext.Current).Select(hydraMessage => Tuple.Create(Serializer.Deserialize(hydraMessage.Data), hydraMessage));
        }

        private static void Send(ConversationDto message, string handle)
        {
            var hydraMessage = new HydraMessage { Topic = ConversationDto.ConversationTopic, Source = MyName, Destination = "AppendServer",
                                                  Handle = handle, Data = Serializer.Serialize(message) };
            hydraMessage.Send();
        }

        private void NewBtn_Click(object sender, EventArgs e)
        {
            string handle = Guid.NewGuid().ToString("N");
            var client = new AppendClient();
            client.Init(SuffixBox.Text, handle, _messageSource.Where(pair => pair.Item2.Handle == handle).Select(pair => pair.Item1), Send);
            ClientPanel.Controls.Add(client);
        }

    }
}
