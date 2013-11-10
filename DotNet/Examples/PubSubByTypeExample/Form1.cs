using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Forms;
using Shastra.Hydra.Messaging;
using Shastra.Hydra.Messaging.Attachments;
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
            _subscriber.ObserveOn(SynchronizationContext.Current).SkipErrors().Subscribe(OnMessage, _ => { });
        }

        private void OnMessage(AugmentedMessage<PstMessage> message)
        {
            MessageBox.Show("Received message: " + message.Message);
            if (message.Attachments != null && message.Attachments.Any()) {
                var dialogue = new SaveFileDialog {Title = "Save attachment"};
                if (dialogue.ShowDialog() == DialogResult.OK) {
                    var stream = _hydraService.GetAttachmentAsync(message.Attachments.First()).Result.ReadAsStreamAsync().Result;
                    using (Stream file = File.OpenWrite(dialogue.FileName)) {
                        stream.CopyTo(file);
                    }
                }
            }
        }

        private void SendBtn_Click(object sender, EventArgs e)
        {
            try {
                string stringField = StringBox.Text;
                long longField = long.Parse(LongBox.Text);
                DateTime dateField = DateTime.Parse(DateBox.Text);
                List<Attachment> attachments = null;
                string filename = AttachmentBox.Text;
                if (!string.IsNullOrEmpty(filename)) {
                    if (!File.Exists(filename)) throw new Exception("File attachment does not exist");
                    var attStream = new FileStream(filename, FileMode.Open);
                    attachments = new List<Attachment> { new StreamAttachment(Path.GetFileName(filename), attStream) };
                }
                _publisher.SendAsync(new PstMessage { StringField = stringField, LongField = longField, DateField = dateField }, attachments);
            } catch (Exception ex) {
                MessageBox.Show("Error sending message: " + ex.Message);
            }
        }

        void Form1_Closing(object sender, CancelEventArgs e)
        {
            _subscriber.Dispose();
        }

        private void FileBtn_Click(object sender, EventArgs e)
        {
            var dialogue = new OpenFileDialog {Title = "Attachment file"};
            if (dialogue.ShowDialog() == DialogResult.OK) {
                AttachmentBox.Text = dialogue.FileName;
            }
        }

    }
}
