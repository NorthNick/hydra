namespace Bollywell.Hydra.ConversationExampleClient
{
    partial class AppendClient
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
            this.HandleLbl = new System.Windows.Forms.Label();
            this.SuffixLbl = new System.Windows.Forms.Label();
            this.ResponseLbl = new System.Windows.Forms.Label();
            this.RequestBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.RequestBtn = new System.Windows.Forms.Button();
            this.EndBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // HandleLbl
            // 
            this.HandleLbl.AutoSize = true;
            this.HandleLbl.Location = new System.Drawing.Point(4, 4);
            this.HandleLbl.Name = "HandleLbl";
            this.HandleLbl.Size = new System.Drawing.Size(44, 13);
            this.HandleLbl.TabIndex = 0;
            this.HandleLbl.Text = "Handle:";
            // 
            // SuffixLbl
            // 
            this.SuffixLbl.AutoSize = true;
            this.SuffixLbl.Location = new System.Drawing.Point(4, 21);
            this.SuffixLbl.Name = "SuffixLbl";
            this.SuffixLbl.Size = new System.Drawing.Size(36, 13);
            this.SuffixLbl.TabIndex = 1;
            this.SuffixLbl.Text = "Suffix:";
            // 
            // ResponseLbl
            // 
            this.ResponseLbl.AutoSize = true;
            this.ResponseLbl.Location = new System.Drawing.Point(4, 38);
            this.ResponseLbl.Name = "ResponseLbl";
            this.ResponseLbl.Size = new System.Drawing.Size(76, 13);
            this.ResponseLbl.TabIndex = 2;
            this.ResponseLbl.Text = "Last response;";
            // 
            // RequestBox
            // 
            this.RequestBox.Location = new System.Drawing.Point(63, 56);
            this.RequestBox.Name = "RequestBox";
            this.RequestBox.Size = new System.Drawing.Size(139, 20);
            this.RequestBox.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 59);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Request: ";
            // 
            // RequestBtn
            // 
            this.RequestBtn.Location = new System.Drawing.Point(29, 91);
            this.RequestBtn.Name = "RequestBtn";
            this.RequestBtn.Size = new System.Drawing.Size(75, 23);
            this.RequestBtn.TabIndex = 5;
            this.RequestBtn.Text = "Request";
            this.RequestBtn.UseVisualStyleBackColor = true;
            this.RequestBtn.Click += new System.EventHandler(this.RequestBtn_Click);
            // 
            // EndBtn
            // 
            this.EndBtn.Location = new System.Drawing.Point(121, 91);
            this.EndBtn.Name = "EndBtn";
            this.EndBtn.Size = new System.Drawing.Size(75, 23);
            this.EndBtn.TabIndex = 6;
            this.EndBtn.Text = "End";
            this.EndBtn.UseVisualStyleBackColor = true;
            this.EndBtn.Click += new System.EventHandler(this.EndBtn_Click);
            // 
            // AppendClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.EndBtn);
            this.Controls.Add(this.RequestBtn);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.RequestBox);
            this.Controls.Add(this.ResponseLbl);
            this.Controls.Add(this.SuffixLbl);
            this.Controls.Add(this.HandleLbl);
            this.Name = "AppendClient";
            this.Size = new System.Drawing.Size(222, 128);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label HandleLbl;
        private System.Windows.Forms.Label SuffixLbl;
        private System.Windows.Forms.Label ResponseLbl;
        private System.Windows.Forms.TextBox RequestBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button RequestBtn;
        private System.Windows.Forms.Button EndBtn;
    }
}
