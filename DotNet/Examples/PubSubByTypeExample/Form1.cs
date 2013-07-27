using System;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Forms;
using Shastra.Hydra.Messaging;
using Shastra.Hydra.Messaging.Listeners;
using Shastra.Hydra.Messaging.Serializers;
using Shastra.Hydra.Messaging.Storage;
using Shastra.Hydra.PubSubByType;

namespace Shastra.Messaging.PubSubByTypeExample
{
    public partial class Form1 : Form
    {
        private Publisher<PstMessage> _publisher;
        private Subscriber<PstMessage> _subscriber;
        private readonly StdHydraService _hydraService;

        public Form1()
        {
            InitializeComponent();
            this.Closing += Form1_Closing;

            string pollSetting = ConfigurationManager.AppSettings["PollIntervalMs"];
            int? pollIntervalMs = pollSetting == null ? (int?) null : int.Parse(pollSetting);
            var servers = ConfigurationManager.AppSettings["HydraServers"].Split(',').Select(s => s.Trim());
            var port = int.Parse(ConfigurationManager.AppSettings["Port"]);
            _hydraService = new StdHydraService(new NearestServerProvider(servers, ConfigurationManager.AppSettings["Database"], port), new ListenerOptions {PollIntervalMs = pollIntervalMs});

            SerialiseComboBox.SelectedIndexChanged += SerialiseComboBox_SelectedIndexChanged;
            SerialiseComboBox.SelectedIndex = 0;
        }

        void SerialiseComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_subscriber != null) _subscriber.Dispose();

            ISerializer<PstMessage> serializer;
            if (SerialiseComboBox.Text == "DataContract") {
                serializer = new HydraDataContractSerializer<PstMessage>();
            } else {
                serializer = new HydraJsonSerializer<PstMessage>();
            }
            _publisher = new Publisher<PstMessage>(_hydraService, serializer);
            _subscriber = new Subscriber<PstMessage>(_hydraService, serializer);
            _subscriber.ObserveOn(SynchronizationContext.Current).Subscribe(OnMessage, _ => { });
        }

        private void OnMessage(PstMessage message)
        {
            MessageBox.Show("Received message: " + message);
        }

        private void SendBtn_Click(object sender, EventArgs e)
        {
            try {
                string stringField = StringBox.Text;
                long longField = long.Parse(LongBox.Text);
                DateTime dateField = DateTime.Parse(DateBox.Text);
                _publisher.Send(new PstMessage { StringField = stringField, LongField = longField, DateField = dateField });
            } catch (Exception ex) {
                MessageBox.Show("Error sending message: " + ex.Message);
            }
        }

        void Form1_Closing(object sender, CancelEventArgs e)
        {
            _subscriber.Dispose();
        }

    }
}
