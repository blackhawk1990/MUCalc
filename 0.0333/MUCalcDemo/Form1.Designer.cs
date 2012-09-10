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
            this.countMUButton = new System.Windows.Forms.Button();
            this.uselessButton = new System.Windows.Forms.Button();
            this.uselessTextbox = new System.Windows.Forms.TextBox();
            this.openTableFileDialog = new System.Windows.Forms.OpenFileDialog();
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
            this.openRTPlanButton.Location = new System.Drawing.Point(42, 28);
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
            this.logTextbox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.logTextbox.Size = new System.Drawing.Size(260, 157);
            this.logTextbox.TabIndex = 2;
            // 
            // countMUButton
            // 
            this.countMUButton.Location = new System.Drawing.Point(168, 28);
            this.countMUButton.Name = "countMUButton";
            this.countMUButton.Size = new System.Drawing.Size(75, 23);
            this.countMUButton.TabIndex = 3;
            this.countMUButton.Text = "Policz MU";
            this.countMUButton.UseVisualStyleBackColor = true;
            this.countMUButton.Click += new System.EventHandler(this.countMUButton_Click);
            // 
            // uselessButton
            // 
            this.uselessButton.Location = new System.Drawing.Point(106, -1);
            this.uselessButton.Name = "uselessButton";
            this.uselessButton.Size = new System.Drawing.Size(75, 23);
            this.uselessButton.TabIndex = 4;
            this.uselessButton.Text = "Useless";
            this.uselessButton.UseVisualStyleBackColor = true;
            this.uselessButton.Click += new System.EventHandler(this.uselessButton_Click);
            // 
            // uselessTextbox
            // 
            this.uselessTextbox.Location = new System.Drawing.Point(187, -1);
            this.uselessTextbox.Name = "uselessTextbox";
            this.uselessTextbox.Size = new System.Drawing.Size(73, 20);
            this.uselessTextbox.TabIndex = 5;
            this.uselessTextbox.TextChanged += new System.EventHandler(this.uselessTextbox_TextChanged);
            // 
            // openTableFileDialog
            // 
            this.openTableFileDialog.DefaultExt = "dmp";
            this.openTableFileDialog.Filter = "Pliki tekstowe (*.txt)|*.txt";
            this.openTableFileDialog.Title = "Wybierz RT Plan";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.uselessTextbox);
            this.Controls.Add(this.uselessButton);
            this.Controls.Add(this.countMUButton);
            this.Controls.Add(this.logTextbox);
            this.Controls.Add(this.fileNameTextbox);
            this.Controls.Add(this.openRTPlanButton);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MUCalc";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Button openRTPlanButton;
        private System.Windows.Forms.TextBox fileNameTextbox;
        private System.Windows.Forms.TextBox logTextbox;
        private System.Windows.Forms.Button countMUButton;
        private System.Windows.Forms.Button uselessButton;
        private System.Windows.Forms.TextBox uselessTextbox;
        private System.Windows.Forms.OpenFileDialog openTableFileDialog;
    }
}

