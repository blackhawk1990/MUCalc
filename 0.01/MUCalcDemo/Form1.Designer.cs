namespace MUCalcDemo
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
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.openRTPlanButton = new System.Windows.Forms.Button();
            this.fileNameTextbox = new System.Windows.Forms.TextBox();
            this.logTextbox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "dmp";
            this.openFileDialog.Filter = "RT Plan (*.dcm)|*.dcm";
            this.openFileDialog.Title = "Wybierz RT Plan";
            // 
            // openRTPlanButton
            // 
            this.openRTPlanButton.Location = new System.Drawing.Point(108, 27);
            this.openRTPlanButton.Name = "openRTPlanButton";
            this.openRTPlanButton.Size = new System.Drawing.Size(75, 23);
            this.openRTPlanButton.TabIndex = 0;
            this.openRTPlanButton.Text = "Wczytaj";
            this.openRTPlanButton.UseVisualStyleBackColor = true;
            this.openRTPlanButton.Click += new System.EventHandler(this.openRTPlanButton_Click);
            // 
            // fileNameTextbox
            // 
            this.fileNameTextbox.Location = new System.Drawing.Point(12, 67);
            this.fileNameTextbox.Name = "fileNameTextbox";
            this.fileNameTextbox.Size = new System.Drawing.Size(260, 20);
            this.fileNameTextbox.TabIndex = 1;
            // 
            // logTextbox
            // 
            this.logTextbox.Location = new System.Drawing.Point(12, 93);
            this.logTextbox.Multiline = true;
            this.logTextbox.Name = "logTextbox";
            this.logTextbox.Size = new System.Drawing.Size(260, 157);
            this.logTextbox.TabIndex = 2;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.logTextbox);
            this.Controls.Add(this.fileNameTextbox);
            this.Controls.Add(this.openRTPlanButton);
            this.Name = "Form1";
            this.Text = "MUCalcDemo";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Button openRTPlanButton;
        private System.Windows.Forms.TextBox fileNameTextbox;
        private System.Windows.Forms.TextBox logTextbox;
    }
}

