namespace Bollywell.Messaging.PubSubByTypeExample
{
    partial class Form1
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.SendBtn = new System.Windows.Forms.Button();
            this.DateBox = new System.Windows.Forms.TextBox();
            this.LongBox = new System.Windows.Forms.TextBox();
            this.StringBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SerialiseComboBox = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.DateBox);
            this.groupBox1.Controls.Add(this.LongBox);
            this.groupBox1.Controls.Add(this.StringBox);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(182, 135);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Message";
            // 
            // SendBtn
            // 
            this.SendBtn.Location = new System.Drawing.Point(63, 201);
            this.SendBtn.Name = "SendBtn";
            this.SendBtn.Size = new System.Drawing.Size(75, 23);
            this.SendBtn.TabIndex = 5;
            this.SendBtn.Text = "Send";
            this.SendBtn.UseVisualStyleBackColor = true;
            this.SendBtn.Click += new System.EventHandler(this.SendBtn_Click);
            // 
            // DateBox
            // 
            this.DateBox.Location = new System.Drawing.Point(50, 69);
            this.DateBox.Name = "DateBox";
            this.DateBox.Size = new System.Drawing.Size(100, 20);
            this.DateBox.TabIndex = 3;
            // 
            // LongBox
            // 
            this.LongBox.Location = new System.Drawing.Point(50, 43);
            this.LongBox.Name = "LongBox";
            this.LongBox.Size = new System.Drawing.Size(100, 20);
            this.LongBox.TabIndex = 2;
            // 
            // StringBox
            // 
            this.StringBox.Location = new System.Drawing.Point(50, 17);
            this.StringBox.Name = "StringBox";
            this.StringBox.Size = new System.Drawing.Size(100, 20);
            this.StringBox.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 46);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(34, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Long:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(33, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Date:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "String:";
            // 
            // SerialiseComboBox
            // 
            this.SerialiseComboBox.FormattingEnabled = true;
            this.SerialiseComboBox.Items.AddRange(new object[] {
            "DataContract",
            "JSON"});
            this.SerialiseComboBox.Location = new System.Drawing.Point(89, 155);
            this.SerialiseComboBox.Name = "SerialiseComboBox";
            this.SerialiseComboBox.Size = new System.Drawing.Size(106, 21);
            this.SerialiseComboBox.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(20, 158);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Serialise as:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(213, 257);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.SerialiseComboBox);
            this.Controls.Add(this.SendBtn);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form1";
            this.Text = "PubSubByType";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox DateBox;
        private System.Windows.Forms.TextBox LongBox;
        private System.Windows.Forms.TextBox StringBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button SendBtn;
        private System.Windows.Forms.ComboBox SerialiseComboBox;
        private System.Windows.Forms.Label label4;
    }
}

