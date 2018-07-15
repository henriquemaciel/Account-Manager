namespace DraconiusGoGUI.UI.PluginUI.ShuffleADS
{
    partial class ShuffleADS_ImportForm
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
            if (disposing && (components != null))
            {
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
            this.APILabel = new System.Windows.Forms.Label();
            this.APITextBox = new System.Windows.Forms.TextBox();
            this.AmountLabel = new System.Windows.Forms.Label();
            this.AmountTextBox = new System.Windows.Forms.TextBox();
            this.ImportButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // APILabel
            // 
            this.APILabel.AutoSize = true;
            this.APILabel.Location = new System.Drawing.Point(49, 26);
            this.APILabel.Name = "APILabel";
            this.APILabel.Size = new System.Drawing.Size(37, 17);
            this.APILabel.TabIndex = 3;
            this.APILabel.Text = "API: ";
            this.APILabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // APITextBox
            // 
            this.APITextBox.Location = new System.Drawing.Point(96, 22);
            this.APITextBox.Margin = new System.Windows.Forms.Padding(4);
            this.APITextBox.Name = "APITextBox";
            this.APITextBox.Size = new System.Drawing.Size(256, 22);
            this.APITextBox.TabIndex = 4;
            // 
            // AmountLabel
            // 
            this.AmountLabel.AutoSize = true;
            this.AmountLabel.Location = new System.Drawing.Point(96, 65);
            this.AmountLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.AmountLabel.Name = "AmountLabel";
            this.AmountLabel.Size = new System.Drawing.Size(119, 17);
            this.AmountLabel.TabIndex = 5;
            this.AmountLabel.Text = "Amount to Import:";
            // 
            // AmountTextBox
            // 
            this.AmountTextBox.Location = new System.Drawing.Point(224, 62);
            this.AmountTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.AmountTextBox.Name = "AmountTextBox";
            this.AmountTextBox.Size = new System.Drawing.Size(83, 22);
            this.AmountTextBox.TabIndex = 6;
            this.AmountTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.AmountTextBox_KeyPress);
            // 
            // ImportButton
            // 
            this.ImportButton.Location = new System.Drawing.Point(152, 101);
            this.ImportButton.Margin = new System.Windows.Forms.Padding(4);
            this.ImportButton.Name = "ImportButton";
            this.ImportButton.Size = new System.Drawing.Size(100, 28);
            this.ImportButton.TabIndex = 7;
            this.ImportButton.Text = "Import";
            this.ImportButton.UseVisualStyleBackColor = true;
            this.ImportButton.Click += new System.EventHandler(this.ImportButton_Click);
            // 
            // ShuffleADS_ImportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(403, 145);
            this.Controls.Add(this.ImportButton);
            this.Controls.Add(this.AmountTextBox);
            this.Controls.Add(this.AmountLabel);
            this.Controls.Add(this.APITextBox);
            this.Controls.Add(this.APILabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "ShuffleADS_ImportForm";
            this.Text = "Import Accounts from ShuffleADS";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label APILabel;
        private System.Windows.Forms.TextBox APITextBox;
        private System.Windows.Forms.Label AmountLabel;
        private System.Windows.Forms.TextBox AmountTextBox;
        private System.Windows.Forms.Button ImportButton;
    }
}