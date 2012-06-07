namespace Bollywell.Hydra.HydraStressTestClient
{
    partial class MainFrm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainFrm));
            this.ListenBtn = new System.Windows.Forms.Button();
            this.MessageCountLbl = new System.Windows.Forms.Label();
            this.SendBtn = new System.Windows.Forms.Button();
            this.ServerLbl = new System.Windows.Forms.Label();
            this.ErrorCountLbl = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // ListenBtn
            // 
            this.ListenBtn.Location = new System.Drawing.Point(54, 106);
            this.ListenBtn.Name = "ListenBtn";
            this.ListenBtn.Size = new System.Drawing.Size(121, 23);
            this.ListenBtn.TabIndex = 0;
            this.ListenBtn.Text = "Listen";
            this.ListenBtn.UseVisualStyleBackColor = true;
            this.ListenBtn.Click += new System.EventHandler(this.ListenBtn_Click);
            // 
            // MessageCountLbl
            // 
            this.MessageCountLbl.AutoSize = true;
            this.MessageCountLbl.Location = new System.Drawing.Point(31, 36);
            this.MessageCountLbl.Name = "MessageCountLbl";
            this.MessageCountLbl.Size = new System.Drawing.Size(111, 13);
            this.MessageCountLbl.TabIndex = 1;
            this.MessageCountLbl.Text = "Messages received: 0";
            // 
            // SendBtn
            // 
            this.SendBtn.Location = new System.Drawing.Point(54, 154);
            this.SendBtn.Name = "SendBtn";
            this.SendBtn.Size = new System.Drawing.Size(121, 23);
            this.SendBtn.TabIndex = 2;
            this.SendBtn.Text = "Send";
            this.SendBtn.UseVisualStyleBackColor = true;
            this.SendBtn.Click += new System.EventHandler(this.SendBtn_Click);
            // 
            // ServerLbl
            // 
            this.ServerLbl.AutoSize = true;
            this.ServerLbl.Location = new System.Drawing.Point(31, 9);
            this.ServerLbl.Name = "ServerLbl";
            this.ServerLbl.Size = new System.Drawing.Size(41, 13);
            this.ServerLbl.TabIndex = 3;
            this.ServerLbl.Text = "Server:";
            // 
            // ErrorCountLbl
            // 
            this.ErrorCountLbl.AutoSize = true;
            this.ErrorCountLbl.Location = new System.Drawing.Point(31, 63);
            this.ErrorCountLbl.Name = "ErrorCountLbl";
            this.ErrorCountLbl.Size = new System.Drawing.Size(71, 13);
            this.ErrorCountLbl.TabIndex = 4;
            this.ErrorCountLbl.Text = "Error count: 0";
            // 
            // MainFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(229, 221);
            this.Controls.Add(this.ErrorCountLbl);
            this.Controls.Add(this.ServerLbl);
            this.Controls.Add(this.SendBtn);
            this.Controls.Add(this.MessageCountLbl);
            this.Controls.Add(this.ListenBtn);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainFrm";
            this.Text = "Hydra Stress Tester";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ListenBtn;
        private System.Windows.Forms.Label MessageCountLbl;
        private System.Windows.Forms.Button SendBtn;
        private System.Windows.Forms.Label ServerLbl;
        private System.Windows.Forms.Label ErrorCountLbl;
    }
}

