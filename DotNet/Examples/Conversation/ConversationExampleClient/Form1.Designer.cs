namespace Shastra.Hydra.ConversationExampleClient
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
            this.NewBtn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuffixBox = new System.Windows.Forms.TextBox();
            this.ClientPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // NewBtn
            // 
            this.NewBtn.Location = new System.Drawing.Point(197, 10);
            this.NewBtn.Name = "NewBtn";
            this.NewBtn.Size = new System.Drawing.Size(75, 23);
            this.NewBtn.TabIndex = 1;
            this.NewBtn.Text = "New Client";
            this.NewBtn.UseVisualStyleBackColor = true;
            this.NewBtn.Click += new System.EventHandler(this.NewBtn_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Suffix:";
            // 
            // SuffixBox
            // 
            this.SuffixBox.Location = new System.Drawing.Point(54, 12);
            this.SuffixBox.Name = "SuffixBox";
            this.SuffixBox.Size = new System.Drawing.Size(137, 20);
            this.SuffixBox.TabIndex = 3;
            // 
            // ClientPanel
            // 
            this.ClientPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ClientPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.ClientPanel.Location = new System.Drawing.Point(12, 53);
            this.ClientPanel.Name = "ClientPanel";
            this.ClientPanel.Size = new System.Drawing.Size(260, 450);
            this.ClientPanel.TabIndex = 4;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 515);
            this.Controls.Add(this.ClientPanel);
            this.Controls.Add(this.SuffixBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.NewBtn);
            this.Name = "Form1";
            this.Text = "Append Client";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button NewBtn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox SuffixBox;
        private System.Windows.Forms.FlowLayoutPanel ClientPanel;
    }
}

