namespace HydraStressTestConsole
{
    partial class ClientControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.DomainLbl = new System.Windows.Forms.Label();
            this.MachineLbl = new System.Windows.Forms.Label();
            this.UserLbl = new System.Windows.Forms.Label();
            this.MessageCountLbl = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.SendChk = new System.Windows.Forms.CheckBox();
            this.ListenChk = new System.Windows.Forms.CheckBox();
            this.BufferDelayBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.UpdateBtn = new System.Windows.Forms.Button();
            this.SendIntervalBox = new System.Windows.Forms.TextBox();
            this.BatchSizeBox = new System.Windows.Forms.TextBox();
            this.DataSizeBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // DomainLbl
            // 
            this.DomainLbl.AutoSize = true;
            this.DomainLbl.Location = new System.Drawing.Point(6, 16);
            this.DomainLbl.Name = "DomainLbl";
            this.DomainLbl.Size = new System.Drawing.Size(46, 13);
            this.DomainLbl.TabIndex = 0;
            this.DomainLbl.Text = "Domain:";
            // 
            // MachineLbl
            // 
            this.MachineLbl.AutoSize = true;
            this.MachineLbl.Location = new System.Drawing.Point(118, 16);
            this.MachineLbl.Name = "MachineLbl";
            this.MachineLbl.Size = new System.Drawing.Size(51, 13);
            this.MachineLbl.TabIndex = 1;
            this.MachineLbl.Text = "Machine:";
            // 
            // UserLbl
            // 
            this.UserLbl.AutoSize = true;
            this.UserLbl.Location = new System.Drawing.Point(6, 38);
            this.UserLbl.Name = "UserLbl";
            this.UserLbl.Size = new System.Drawing.Size(32, 13);
            this.UserLbl.TabIndex = 2;
            this.UserLbl.Text = "User:";
            // 
            // MessageCountLbl
            // 
            this.MessageCountLbl.AutoSize = true;
            this.MessageCountLbl.Location = new System.Drawing.Point(90, 38);
            this.MessageCountLbl.Name = "MessageCountLbl";
            this.MessageCountLbl.Size = new System.Drawing.Size(58, 13);
            this.MessageCountLbl.TabIndex = 3;
            this.MessageCountLbl.Text = "Messages:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.DomainLbl);
            this.groupBox1.Controls.Add(this.MessageCountLbl);
            this.groupBox1.Controls.Add(this.MachineLbl);
            this.groupBox1.Controls.Add(this.UserLbl);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(249, 62);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Summary";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.DataSizeBox);
            this.groupBox2.Controls.Add(this.BatchSizeBox);
            this.groupBox2.Controls.Add(this.SendIntervalBox);
            this.groupBox2.Controls.Add(this.UpdateBtn);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.BufferDelayBox);
            this.groupBox2.Controls.Add(this.ListenChk);
            this.groupBox2.Controls.Add(this.SendChk);
            this.groupBox2.Location = new System.Drawing.Point(4, 72);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(248, 126);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Control";
            // 
            // SendChk
            // 
            this.SendChk.AutoSize = true;
            this.SendChk.Location = new System.Drawing.Point(8, 20);
            this.SendChk.Name = "SendChk";
            this.SendChk.Size = new System.Drawing.Size(51, 17);
            this.SendChk.TabIndex = 0;
            this.SendChk.Text = "Send";
            this.SendChk.UseVisualStyleBackColor = true;
            // 
            // ListenChk
            // 
            this.ListenChk.AutoSize = true;
            this.ListenChk.Location = new System.Drawing.Point(8, 44);
            this.ListenChk.Name = "ListenChk";
            this.ListenChk.Size = new System.Drawing.Size(54, 17);
            this.ListenChk.TabIndex = 1;
            this.ListenChk.Text = "Listen";
            this.ListenChk.UseVisualStyleBackColor = true;
            // 
            // BufferDelayBox
            // 
            this.BufferDelayBox.Location = new System.Drawing.Point(181, 19);
            this.BufferDelayBox.Name = "BufferDelayBox";
            this.BufferDelayBox.Size = new System.Drawing.Size(61, 20);
            this.BufferDelayBox.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(89, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Buffer delay (ms):";
            // 
            // UpdateBtn
            // 
            this.UpdateBtn.Location = new System.Drawing.Point(166, 95);
            this.UpdateBtn.Name = "UpdateBtn";
            this.UpdateBtn.Size = new System.Drawing.Size(75, 23);
            this.UpdateBtn.TabIndex = 4;
            this.UpdateBtn.Text = "Update";
            this.UpdateBtn.UseVisualStyleBackColor = true;
            this.UpdateBtn.Click += new System.EventHandler(this.UpdateBtn_Click);
            // 
            // SendIntervalBox
            // 
            this.SendIntervalBox.Location = new System.Drawing.Point(181, 44);
            this.SendIntervalBox.Name = "SendIntervalBox";
            this.SendIntervalBox.Size = new System.Drawing.Size(59, 20);
            this.SendIntervalBox.TabIndex = 5;
            // 
            // BatchSizeBox
            // 
            this.BatchSizeBox.Location = new System.Drawing.Point(181, 69);
            this.BatchSizeBox.Name = "BatchSizeBox";
            this.BatchSizeBox.Size = new System.Drawing.Size(59, 20);
            this.BatchSizeBox.TabIndex = 6;
            // 
            // DataSizeBox
            // 
            this.DataSizeBox.Location = new System.Drawing.Point(66, 69);
            this.DataSizeBox.Name = "DataSizeBox";
            this.DataSizeBox.Size = new System.Drawing.Size(49, 20);
            this.DataSizeBox.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(83, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Send interval (ms):";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(118, 72);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Batch size:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 72);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(54, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Data size:";
            // 
            // ClientControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "ClientControl";
            this.Size = new System.Drawing.Size(256, 201);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label DomainLbl;
        private System.Windows.Forms.Label MachineLbl;
        private System.Windows.Forms.Label UserLbl;
        private System.Windows.Forms.Label MessageCountLbl;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox ListenChk;
        private System.Windows.Forms.CheckBox SendChk;
        private System.Windows.Forms.Button UpdateBtn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox BufferDelayBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox DataSizeBox;
        private System.Windows.Forms.TextBox BatchSizeBox;
        private System.Windows.Forms.TextBox SendIntervalBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
    }
}
