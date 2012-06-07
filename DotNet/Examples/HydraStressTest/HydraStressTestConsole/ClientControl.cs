using System;
using System.Windows.Forms;
using HydraStressTestDtos;

namespace HydraStressTestConsole
{
    public partial class ClientControl : UserControl
    {
        private long _dataCount, _errorCount;

        private string _clientId;
        public string ClientId { 
            get { return _clientId; } 
            set { _clientId = value; MachineLbl.Text = _clientId; }
        }

        public ClientControl()
        {
            InitializeComponent();
        }

        public void DataMessage(StressTestData message)

        {
            _dataCount++;
            UpdateMessageLabel();
            DomainLbl.Text = message.Domain;
            UserLbl.Text = message.Username;
        }

        public void ErrorMessage(StressTestError message)
        {
            _errorCount++;
            UpdateMessageLabel();
        }

        private void UpdateBtn_Click(object sender, EventArgs e)
        {
            MainFrm.Send(new StressTestControl {Listen = ListenChk.Checked, Send = SendChk.Checked, BufferDelayMs = long.Parse(BufferDelayBox.Text),
                                                SendBatchSize = int.Parse(BatchSizeBox.Text), SendIntervalMs = int.Parse(SendIntervalBox.Text), 
                                                SendMaxDataLength = int.Parse(DataSizeBox.Text)
            }, ClientId);
        }

        public void ControlMessage(StressTestControl message)
        {
            ListenChk.Checked = message.Listen;
            SendChk.Checked = message.Send;
            BufferDelayBox.Text = message.BufferDelayMs.ToString();
            SendIntervalBox.Text = message.SendIntervalMs.ToString();
            BatchSizeBox.Text = message.SendBatchSize.ToString();
            DataSizeBox.Text = message.SendMaxDataLength.ToString();
        }

        private void UpdateMessageLabel()
        {
            MessageCountLbl.Text = string.Format("Messages/errors: {0}/{1}", _dataCount, _errorCount);
        }
    }
}
