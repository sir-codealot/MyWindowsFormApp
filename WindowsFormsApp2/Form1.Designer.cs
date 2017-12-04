namespace WindowsFormsApp2
{
    partial class mainWindow
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(mainWindow));
            this.SourceLabel = new System.Windows.Forms.Label();
            this.SourceTextBox = new System.Windows.Forms.TextBox();
            this.DestTextBox = new System.Windows.Forms.TextBox();
            this.DestLabel = new System.Windows.Forms.Label();
            this.MachineLabel = new System.Windows.Forms.Label();
            this.SourceBtn = new System.Windows.Forms.Button();
            this.DestBtn = new System.Windows.Forms.Button();
            this.MakeBtn = new System.Windows.Forms.Button();
            this.QuitBtn = new System.Windows.Forms.Button();
            this.AboutBtn = new System.Windows.Forms.Button();
            this.RadioBtn1 = new System.Windows.Forms.RadioButton();
            this.RadioBtn2 = new System.Windows.Forms.RadioButton();
            this.RadioBtn4 = new System.Windows.Forms.RadioButton();
            this.RadioBtn3 = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // SourceLabel
            // 
            this.SourceLabel.AutoSize = true;
            this.SourceLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
            this.SourceLabel.Location = new System.Drawing.Point(12, 36);
            this.SourceLabel.Name = "SourceLabel";
            this.SourceLabel.Size = new System.Drawing.Size(109, 16);
            this.SourceLabel.TabIndex = 0;
            this.SourceLabel.Text = "Quell-Programm:";
            // 
            // SourceTextBox
            // 
            this.SourceTextBox.Location = new System.Drawing.Point(15, 55);
            this.SourceTextBox.Name = "SourceTextBox";
            this.SourceTextBox.Size = new System.Drawing.Size(320, 20);
            this.SourceTextBox.TabIndex = 1;
            // 
            // DestTextBox
            // 
            this.DestTextBox.Location = new System.Drawing.Point(15, 126);
            this.DestTextBox.Name = "DestTextBox";
            this.DestTextBox.Size = new System.Drawing.Size(320, 20);
            this.DestTextBox.TabIndex = 3;
            // 
            // DestLabel
            // 
            this.DestLabel.AutoSize = true;
            this.DestLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
            this.DestLabel.Location = new System.Drawing.Point(12, 107);
            this.DestLabel.Name = "DestLabel";
            this.DestLabel.Size = new System.Drawing.Size(69, 16);
            this.DestLabel.TabIndex = 2;
            this.DestLabel.Text = "Ziel-Datei:";
            // 
            // MachineLabel
            // 
            this.MachineLabel.AutoSize = true;
            this.MachineLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
            this.MachineLabel.Location = new System.Drawing.Point(12, 169);
            this.MachineLabel.Name = "MachineLabel";
            this.MachineLabel.Size = new System.Drawing.Size(69, 16);
            this.MachineLabel.TabIndex = 4;
            this.MachineLabel.Text = "Maschine:";
            // 
            // SourceBtn
            // 
            this.SourceBtn.Location = new System.Drawing.Point(364, 53);
            this.SourceBtn.Name = "SourceBtn";
            this.SourceBtn.Size = new System.Drawing.Size(75, 23);
            this.SourceBtn.TabIndex = 5;
            this.SourceBtn.Text = "Suchen...";
            this.SourceBtn.UseVisualStyleBackColor = true;
            this.SourceBtn.Click += new System.EventHandler(this.SourceBtn_Click);
            // 
            // DestBtn
            // 
            this.DestBtn.Location = new System.Drawing.Point(364, 124);
            this.DestBtn.Name = "DestBtn";
            this.DestBtn.Size = new System.Drawing.Size(75, 23);
            this.DestBtn.TabIndex = 6;
            this.DestBtn.Text = "Suchen...";
            this.DestBtn.UseVisualStyleBackColor = true;
            this.DestBtn.Click += new System.EventHandler(this.DestBtn_Click);
            // 
            // MakeBtn
            // 
            this.MakeBtn.Location = new System.Drawing.Point(283, 270);
            this.MakeBtn.Name = "MakeBtn";
            this.MakeBtn.Size = new System.Drawing.Size(75, 23);
            this.MakeBtn.TabIndex = 7;
            this.MakeBtn.Text = "Erstellen";
            this.MakeBtn.UseVisualStyleBackColor = true;
            this.MakeBtn.Click += new System.EventHandler(this.MakeBtn_Click);
            // 
            // QuitBtn
            // 
            this.QuitBtn.Location = new System.Drawing.Point(364, 270);
            this.QuitBtn.Name = "QuitBtn";
            this.QuitBtn.Size = new System.Drawing.Size(75, 23);
            this.QuitBtn.TabIndex = 8;
            this.QuitBtn.Text = "Beenden";
            this.QuitBtn.UseVisualStyleBackColor = true;
            this.QuitBtn.Click += new System.EventHandler(this.QuitBtn_Click);
            // 
            // AboutBtn
            // 
            this.AboutBtn.Location = new System.Drawing.Point(12, 270);
            this.AboutBtn.Name = "AboutBtn";
            this.AboutBtn.Size = new System.Drawing.Size(23, 23);
            this.AboutBtn.TabIndex = 9;
            this.AboutBtn.Text = "?";
            this.AboutBtn.UseVisualStyleBackColor = true;
            this.AboutBtn.Click += new System.EventHandler(this.AboutBtn_Click);
            // 
            // RadioBtn1
            // 
            this.RadioBtn1.AutoSize = true;
            this.RadioBtn1.Checked = true;
            this.RadioBtn1.Location = new System.Drawing.Point(15, 188);
            this.RadioBtn1.Name = "RadioBtn1";
            this.RadioBtn1.Size = new System.Drawing.Size(67, 17);
            this.RadioBtn1.TabIndex = 10;
            this.RadioBtn1.TabStop = true;
            this.RadioBtn1.Text = "DMC100";
            this.RadioBtn1.UseVisualStyleBackColor = true;
            // 
            // RadioBtn2
            // 
            this.RadioBtn2.AutoSize = true;
            this.RadioBtn2.Location = new System.Drawing.Point(15, 213);
            this.RadioBtn2.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            this.RadioBtn2.Name = "RadioBtn2";
            this.RadioBtn2.Size = new System.Drawing.Size(84, 17);
            this.RadioBtn2.TabIndex = 11;
            this.RadioBtn2.Text = "UniPort6000";
            this.RadioBtn2.UseVisualStyleBackColor = true;
            // 
            // RadioBtn4
            // 
            this.RadioBtn4.AutoSize = true;
            this.RadioBtn4.Location = new System.Drawing.Point(131, 213);
            this.RadioBtn4.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            this.RadioBtn4.Name = "RadioBtn4";
            this.RadioBtn4.Size = new System.Drawing.Size(53, 17);
            this.RadioBtn4.TabIndex = 13;
            this.RadioBtn4.Text = "Union";
            this.RadioBtn4.UseVisualStyleBackColor = true;
            // 
            // RadioBtn3
            // 
            this.RadioBtn3.AutoSize = true;
            this.RadioBtn3.Location = new System.Drawing.Point(131, 188);
            this.RadioBtn3.Name = "RadioBtn3";
            this.RadioBtn3.Size = new System.Drawing.Size(54, 17);
            this.RadioBtn3.TabIndex = 12;
            this.RadioBtn3.Text = "Forest";
            this.RadioBtn3.UseVisualStyleBackColor = true;
            // 
            // mainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 307);
            this.Controls.Add(this.SourceTextBox);
            this.Controls.Add(this.RadioBtn4);
            this.Controls.Add(this.RadioBtn3);
            this.Controls.Add(this.RadioBtn2);
            this.Controls.Add(this.RadioBtn1);
            this.Controls.Add(this.AboutBtn);
            this.Controls.Add(this.QuitBtn);
            this.Controls.Add(this.MakeBtn);
            this.Controls.Add(this.DestBtn);
            this.Controls.Add(this.SourceBtn);
            this.Controls.Add(this.MachineLabel);
            this.Controls.Add(this.DestTextBox);
            this.Controls.Add(this.DestLabel);
            this.Controls.Add(this.SourceLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "mainWindow";
            this.Text = "Einrichteblatt erstellen";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label SourceLabel;
        private System.Windows.Forms.TextBox SourceTextBox;
        private System.Windows.Forms.TextBox DestTextBox;
        private System.Windows.Forms.Label DestLabel;
        private System.Windows.Forms.Label MachineLabel;
        private System.Windows.Forms.Button SourceBtn;
        private System.Windows.Forms.Button DestBtn;
        private System.Windows.Forms.Button MakeBtn;
        private System.Windows.Forms.Button QuitBtn;
        private System.Windows.Forms.Button AboutBtn;
        private System.Windows.Forms.RadioButton RadioBtn1;
        private System.Windows.Forms.RadioButton RadioBtn2;
        private System.Windows.Forms.RadioButton RadioBtn4;
        private System.Windows.Forms.RadioButton RadioBtn3;
    }
}

