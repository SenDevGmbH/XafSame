namespace SenDev.XafSame
{
    partial class SplashForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.TextBox logTextBox;
        private System.Windows.Forms.Panel containerPanel;

        private void InitializeComponent()
        {
            titleLabel = new System.Windows.Forms.Label();
            logTextBox = new System.Windows.Forms.TextBox();
            containerPanel = new System.Windows.Forms.Panel();
            showExceptionDetailsButton = new System.Windows.Forms.Button();
            exitButton = new System.Windows.Forms.Button();
            containerPanel.SuspendLayout();
            SuspendLayout();
            // 
            // titleLabel
            // 
            titleLabel.AutoSize = true;
            titleLabel.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            titleLabel.ForeColor = System.Drawing.Color.Black;
            titleLabel.Location = new System.Drawing.Point(43, 49);
            titleLabel.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new System.Drawing.Size(474, 51);
            titleLabel.TabIndex = 0;
            titleLabel.Text = "Model Editor is starting...";
            // 
            // logTextBox
            // 
            logTextBox.BackColor = System.Drawing.Color.Gainsboro;
            logTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            logTextBox.Font = new System.Drawing.Font("Consolas", 10F);
            logTextBox.ForeColor = System.Drawing.Color.Black;
            logTextBox.Location = new System.Drawing.Point(43, 147);
            logTextBox.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            logTextBox.Multiline = true;
            logTextBox.Name = "logTextBox";
            logTextBox.ReadOnly = true;
            logTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            logTextBox.Size = new System.Drawing.Size(1035, 540);
            logTextBox.TabIndex = 1;
            // 
            // containerPanel
            // 
            containerPanel.BackColor = System.Drawing.Color.LightSeaGreen;
            containerPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            containerPanel.Controls.Add(showExceptionDetailsButton);
            containerPanel.Controls.Add(exitButton);
            containerPanel.Controls.Add(titleLabel);
            containerPanel.Controls.Add(logTextBox);
            containerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            containerPanel.Location = new System.Drawing.Point(0, 0);
            containerPanel.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            containerPanel.Name = "containerPanel";
            containerPanel.Size = new System.Drawing.Size(1125, 797);
            containerPanel.TabIndex = 0;
            // 
            // showExceptionDetailsButton
            // 
            showExceptionDetailsButton.Location = new System.Drawing.Point(43, 721);
            showExceptionDetailsButton.Name = "showExceptionDetailsButton";
            showExceptionDetailsButton.Size = new System.Drawing.Size(283, 46);
            showExceptionDetailsButton.TabIndex = 3;
            showExceptionDetailsButton.Text = "Show exception details";
            showExceptionDetailsButton.UseVisualStyleBackColor = true;
            showExceptionDetailsButton.Visible = false;
            showExceptionDetailsButton.Click += showExceptionDetailsButton_Click;
            // 
            // exitButton
            // 
            exitButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            exitButton.Location = new System.Drawing.Point(928, 721);
            exitButton.Name = "exitButton";
            exitButton.Size = new System.Drawing.Size(150, 46);
            exitButton.TabIndex = 2;
            exitButton.Text = "Exit";
            exitButton.UseVisualStyleBackColor = true;
            exitButton.Visible = false;
            exitButton.Click += exitButton_Click;
            // 
            // SplashForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(1125, 797);
            Controls.Add(containerPanel);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            Name = "SplashForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            containerPanel.ResumeLayout(false);
            containerPanel.PerformLayout();
            ResumeLayout(false);
        }

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
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.Button showExceptionDetailsButton;
    }
}