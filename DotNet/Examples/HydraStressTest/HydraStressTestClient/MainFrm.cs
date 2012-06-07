using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Bollywell.Hydra.Messaging;
using Bollywell.Hydra.Messaging.Config;
using Bollywell.Hydra.Messaging.MessageFetchers;
using Bollywell.Hydra.Messaging.Pollers;
using Bollywell.Hydra.Messaging.Serializers;
using Bollywell.Hydra.PubSubByType;
using HydraStressTestDtos;

namespace Bollywell.Hydra.HydraStressTestClient
{
    public partial class MainFrm : Form
    {
        private int _sendIntervalMs = 3000;
        private int _maxDataLength = 10000;
        private int _maxSendBatchSize = 10;

        private readonly string _domain = Environment.UserDomainName;
        private readonly string _username = Environment.UserName;
        private readonly string _myName = Dns.GetHostName() + ":" + Process.GetCurrentProcess().Id.ToString();
        private Subscriber<StressTestData> _subscriber;
        private IDisposable _recvSubscription;
        private long _recvCount;
        private readonly Dictionary<string, long> _clientSeq = new Dictionary<string, long>();

        private readonly HydraService _hydraService;
        private IObservable<long> _sendObservable;
        private IDisposable _sendSubscription;
        private long _sendCount;
        private readonly Random _rnd = new Random();
        private readonly Publisher<StressTestData> _stressSender;
        private long _errorCount;
        private readonly Publisher<StressTestError> _errorSender;
        private readonly IPoller<HydraMessage> _poller;
        private readonly ISerializer<StressTestControl> _serializer = new HydraDataContractSerializer<StressTestControl>();
        private IDisposable _controlSubscription;
        private bool _listening, _sending;
        private int _heartbeatIntervalMs;
        private IObservable<long> _heartbeatObservable;
        private IDisposable _heartbeatSubscription;
        private Tuple<string, int>[] _dataList;

        public MainFrm()
        {
            InitializeComponent();
            InitialiseData();

            WebRequest.DefaultWebProxy = null;
            var pinger = new Pinger(ConfigurationManager.AppSettings["HydraServers"].Split(',').Select(s => s.Trim()));
            pinger.RefreshSync();
            var servers = pinger.ServerInfo.Where(si => si.IsReachable).Select(si => si.Address);

            string pollSetting = ConfigurationManager.AppSettings["PollIntervalMs"];
            int? pollIntervalMs = pollSetting == null ? (int?)null : int.Parse(pollSetting);
            _hydraService = new HydraService(new RoundRobinConfigProvider(servers, ConfigurationManager.AppSettings["Database"], pollIntervalMs));
            _stressSender = new Publisher<StressTestData>(_hydraService);
            _errorSender = new Publisher<StressTestError>(_hydraService);
            _poller = _hydraService.GetPoller(new HydraByTopicByDestinationMessageFetcher(typeof(StressTestControl).FullName, _myName));
            _controlSubscription = _poller.Select(hydraMessage => _serializer.Deserialize(hydraMessage.Data)).ObserveOn(SynchronizationContext.Current).Subscribe(OnControlRecv);
            _heartbeatIntervalMs = int.Parse(ConfigurationManager.AppSettings["HeartbeatIntervalMs"]);
            _heartbeatObservable = Observable.Interval(TimeSpan.FromMilliseconds(_heartbeatIntervalMs), Scheduler.ThreadPool);
            _heartbeatSubscription = _heartbeatObservable.Subscribe(OnHeartbeat);
        }

        #region Listening

        private void SetListening(bool newState)
        {
            if (newState == _listening) return;
            if (newState) {
                _subscriber = new Subscriber<StressTestData>(_hydraService);
                _recvSubscription = _subscriber.ObserveOn(SynchronizationContext.Current).Subscribe(OnRecv);
                ListenBtn.Text = "Stop listening";
            } else {
                _recvSubscription.Dispose();
                _subscriber.Dispose();
                ListenBtn.Text = "Listen";
            }
            _listening = newState;
        }

        private void ListenBtn_Click(object sender, EventArgs e)
        {
            SetListening(!_listening);
            SendSettings();
        }

        private void OnRecv(StressTestData message)
        {
            _recvCount++;
            MessageCountLbl.Text = string.Format("Messages received: {0}", _recvCount);
            ServerLbl.Text = string.Format("Server: {0}", _hydraService.ServerName);
            long seqId;
            if (_clientSeq.TryGetValue(message.Sender, out seqId) && message.Seq != seqId + 1) {
                try {
                    _errorCount++;
                    ErrorCountLbl.Text = string.Format("Error count: {0}", _errorCount);
                    _errorSender.Send(new StressTestError {
                        Sender = message.Sender, Receiver = _myName, ExpectedSeq = seqId + 1, ReceivedSeq = message.Seq,
                        SendTime = message.Timestamp, ReceiveTime = DateTime.UtcNow, DataLength = message.Data.Length
                    });
                } catch (Exception) {
                    // Ignore errors
                }
            }
            _clientSeq[message.Sender] = message.Seq;
        }

        #endregion

        #region Sending

        private void SetSending(bool newState)
        {
            if (newState == _sending) return;
            if (newState) {
                _sendObservable = Observable.Interval(TimeSpan.FromMilliseconds(_sendIntervalMs), Scheduler.ThreadPool);
                _sendSubscription = _sendObservable.Subscribe(OnSend);
                SendBtn.Text = "Stop sending";
            } else {
                _sendSubscription.Dispose();
                _sendObservable = null;
                SendBtn.Text = "Send";
            }
            _sending = newState;
        }

        private void SendBtn_Click(object sender, EventArgs e)
        {
            SetSending(!_sending);
            SendSettings();
        }

        private void OnSend(long seq)
        {
            // Send up to MaxSendBatchSize messages
            try {
                for (int i = 0; i < _rnd.Next(_maxSendBatchSize + 1); i++) {
                    string data = CreateStringOfLength(_rnd.Next(_maxDataLength + 1));
                    _stressSender.Send(new StressTestData {Sender = _myName, Domain = _domain, Username = _username,
                                                           Seq = _sendCount + 1, Data = data, Timestamp = DateTime.UtcNow});
                    _sendCount++;
                }
            } catch (Exception) {
                // Ignore errors
            }
        }

        #endregion

        #region Control

        private void OnControlRecv(StressTestControl message)
        {
            _maxDataLength = message.SendMaxDataLength;
            _maxSendBatchSize = message.SendBatchSize;
            _sendIntervalMs = message.SendIntervalMs;
            SetSending(message.Send);
            SetListening(message.Listen);
            _subscriber.BufferDelayMs = message.BufferDelayMs;
        }

        private void OnHeartbeat(long seq)
        {
            SendSettings();
        }

        private void SendSettings()
        {
            try {
                var reply = new StressTestControl {
                    Listen = _listening, Send = _sending, SendBatchSize = _maxSendBatchSize, SendIntervalMs = _sendIntervalMs,
                    SendMaxDataLength = _maxDataLength, BufferDelayMs = (_subscriber == null) ? 0 : _subscriber.BufferDelayMs
                };
                _hydraService.Send(new HydraMessage { Source = _myName, Destination = "StressTestConsole", Topic = typeof(StressTestControl).FullName, Data = _serializer.Serialize(reply) });
            } catch (Exception) {
                // Ignore errors
            }
        }

        #endregion

        #region Data

        private void InitialiseData()
        {
            ResourceSet resources = MessageData.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            _dataList = resources.Cast<DictionaryEntry>().Select(entry => Tuple.Create((string)entry.Value, ((string)entry.Value).Length)).OrderByDescending(tuple => tuple.Item2).ToArray();
        }

        private string CreateStringOfLength(int length)
        {
            int dataIndex = 0, remainingLength = length;
            var res = new StringBuilder();
            while (remainingLength > 0) {
                while (_dataList[dataIndex].Item2 > remainingLength && dataIndex < _dataList.GetUpperBound(0)) { dataIndex++; }
                res.Append(_dataList[dataIndex].Item1);
                remainingLength -= _dataList[dataIndex].Item2;
            }
            return res.ToString();
        }

        #endregion
    }
}
