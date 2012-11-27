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
            this.openTableFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.plikToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wczytajWydajnościToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wczytajPdgToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wyjścieToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.oProgramieToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.button1 = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
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
            // openTableFileDialog
            // 
            this.openTableFileDialog.DefaultExt = "dmp";
            this.openTableFileDialog.Filter = "Pliki tabel (*.dat)|*.dat";
            this.openTableFileDialog.Title = "Wybierz plik danych aparatu";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.plikToolStripMenuItem,
            this.oProgramieToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(284, 24);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // plikToolStripMenuItem
            // 
            this.plikToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.wczytajWydajnościToolStripMenuItem,
            this.wczytajPdgToolStripMenuItem,
            this.wyjścieToolStripMenuItem});
            this.plikToolStripMenuItem.Name = "plikToolStripMenuItem";
            this.plikToolStripMenuItem.Size = new System.Drawing.Size(38, 20);
            this.plikToolStripMenuItem.Text = "Plik";
            // 
            // wczytajWydajnościToolStripMenuItem
            // 
            this.wczytajWydajnościToolStripMenuItem.Name = "wczytajWydajnościToolStripMenuItem";
            this.wczytajWydajnościToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.wczytajWydajnościToolStripMenuItem.Text = "Wczytaj wydajności";
            this.wczytajWydajnościToolStripMenuItem.Click += new System.EventHandler(this.wczytajWydajnościToolStripMenuItem_Click);
            // 
            // wczytajPdgToolStripMenuItem
            // 
            this.wczytajPdgToolStripMenuItem.Name = "wczytajPdgToolStripMenuItem";
            this.wczytajPdgToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.wczytajPdgToolStripMenuItem.Text = "Wczytaj pdg";
            this.wczytajPdgToolStripMenuItem.Click += new System.EventHandler(this.wczytajPdgToolStripMenuItem_Click);
            // 
            // wyjścieToolStripMenuItem
            // 
            this.wyjścieToolStripMenuItem.Name = "wyjścieToolStripMenuItem";
            this.wyjścieToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.wyjścieToolStripMenuItem.Text = "Wyjście";
            this.wyjścieToolStripMenuItem.Click += new System.EventHandler(this.wyjścieToolStripMenuItem_Click);
            // 
            // oProgramieToolStripMenuItem
            // 
            this.oProgramieToolStripMenuItem.Name = "oProgramieToolStripMenuItem";
            this.oProgramieToolStripMenuItem.Size = new System.Drawing.Size(86, 20);
            this.oProgramieToolStripMenuItem.Text = "O programie";
            this.oProgramieToolStripMenuItem.Click += new System.EventHandler(this.oProgramieToolStripMenuItem_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(106, 281);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "DOSE Load";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.testbutton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 341);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.countMUButton);
            this.Controls.Add(this.logTextbox);
            this.Controls.Add(this.fileNameTextbox);
            this.Controls.Add(this.openRTPlanButton);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MUCalc";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Button openRTPlanButton;
        private System.Windows.Forms.TextBox fileNameTextbox;
        private System.Windows.Forms.TextBox logTextbox;
        private System.Windows.Forms.Button countMUButton;
        private System.Windows.Forms.OpenFileDialog openTableFileDialog;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem plikToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wczytajWydajnościToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wczytajPdgToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wyjścieToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem oProgramieToolStripMenuItem;
        private System.Windows.Forms.Button button1;
    }
}

