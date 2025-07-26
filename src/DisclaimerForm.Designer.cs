namespace SenDev.XafSame
{
    partial class DisclaimerForm
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
            acknowledgeButton = new System.Windows.Forms.Button();
            exitButton = new System.Windows.Forms.Button();
            disclaimerTextBox = new System.Windows.Forms.RichTextBox();
            SuspendLayout();
            // 
            // acknowledgeButton
            // 
            acknowledgeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            acknowledgeButton.Location = new System.Drawing.Point(559, 241);
            acknowledgeButton.Margin = new System.Windows.Forms.Padding(6);
            acknowledgeButton.Name = "acknowledgeButton";
            acknowledgeButton.Size = new System.Drawing.Size(186, 64);
            acknowledgeButton.TabIndex = 1;
            acknowledgeButton.Text = "Acknowledge";
            acknowledgeButton.UseVisualStyleBackColor = true;
            // 
            // exitButton
            // 
            exitButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            exitButton.Location = new System.Drawing.Point(766, 241);
            exitButton.Margin = new System.Windows.Forms.Padding(6);
            exitButton.Name = "exitButton";
            exitButton.Size = new System.Drawing.Size(186, 64);
            exitButton.TabIndex = 2;
            exitButton.Text = "Exit";
            exitButton.UseVisualStyleBackColor = true;
            // 
            // disclaimerTextBox
            // 
            disclaimerTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            disclaimerTextBox.Location = new System.Drawing.Point(24, 15);
            disclaimerTextBox.Name = "disclaimerTextBox";
            disclaimerTextBox.ReadOnly = true;
            disclaimerTextBox.Size = new System.Drawing.Size(931, 206);
            disclaimerTextBox.TabIndex = 3;
            disclaimerTextBox.Text = "";
            // 
            // DisclaimerForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(977, 332);
            Controls.Add(disclaimerTextBox);
            Controls.Add(exitButton);
            Controls.Add(acknowledgeButton);
            Margin = new System.Windows.Forms.Padding(6);
            Name = "DisclaimerForm";
            Padding = new System.Windows.Forms.Padding(19, 21, 19, 21);
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "SenDev.XafSame: Disclaimer";
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Button acknowledgeButton;
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.RichTextBox disclaimerTextBox;
    }
}