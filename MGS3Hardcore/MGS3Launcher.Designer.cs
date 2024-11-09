namespace MGS3Hardcore
{
    partial class MGS3Launcher
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MGS3Launcher));
            locateExeBtn = new Button();
            launchGameBtn = new Button();
            quitBtn = new Button();
            permanentDamageBtn = new Button();
            guardStatusesBtn = new Button();
            quickReloadBtn = new Button();
            pausingBtn = new Button();
            permadeathBtn = new Button();
            doubleDamageBtn = new Button();
            donationBtn = new Button();
            warningLabel = new Label();
            SuspendLayout();
            // 
            // locateExeBtn
            // 
            locateExeBtn.Location = new Point(520, 381);
            locateExeBtn.Name = "locateExeBtn";
            locateExeBtn.Size = new Size(131, 57);
            locateExeBtn.TabIndex = 18;
            locateExeBtn.Text = "Locate Game Executable";
            locateExeBtn.UseVisualStyleBackColor = true;
            locateExeBtn.Click += locateExeBtn_Click;
            // 
            // launchGameBtn
            // 
            launchGameBtn.BackColor = SystemColors.ActiveCaption;
            launchGameBtn.Location = new Point(657, 381);
            launchGameBtn.Name = "launchGameBtn";
            launchGameBtn.Size = new Size(131, 57);
            launchGameBtn.TabIndex = 17;
            launchGameBtn.Text = "Launch MGS3";
            launchGameBtn.UseVisualStyleBackColor = false;
            launchGameBtn.Click += launchGameBtn_Click;
            // 
            // quitBtn
            // 
            quitBtn.BackColor = Color.IndianRed;
            quitBtn.Location = new Point(657, 12);
            quitBtn.Name = "quitBtn";
            quitBtn.Size = new Size(131, 57);
            quitBtn.TabIndex = 16;
            quitBtn.Text = "Quit";
            quitBtn.UseVisualStyleBackColor = false;
            quitBtn.Click += quitBtn_Click;
            // 
            // permanentDamageBtn
            // 
            permanentDamageBtn.BackColor = Color.LawnGreen;
            permanentDamageBtn.Location = new Point(149, 381);
            permanentDamageBtn.Name = "permanentDamageBtn";
            permanentDamageBtn.Size = new Size(131, 57);
            permanentDamageBtn.TabIndex = 15;
            permanentDamageBtn.Text = "Permanent Damage: ON";
            permanentDamageBtn.UseVisualStyleBackColor = false;
            permanentDamageBtn.Click += BooleanButton_Click;
            permanentDamageBtn.MouseHover += permanentDamageBtn_MouseHover;
            // 
            // guardStatusesBtn
            // 
            guardStatusesBtn.BackColor = Color.LawnGreen;
            guardStatusesBtn.Location = new Point(149, 318);
            guardStatusesBtn.Name = "guardStatusesBtn";
            guardStatusesBtn.Size = new Size(131, 57);
            guardStatusesBtn.TabIndex = 14;
            guardStatusesBtn.Text = "Extend Guard Statuses: ON";
            guardStatusesBtn.UseVisualStyleBackColor = false;
            guardStatusesBtn.Click += BooleanButton_Click;
            guardStatusesBtn.MouseHover += guardStatusesBtn_MouseHover;
            // 
            // quickReloadBtn
            // 
            quickReloadBtn.BackColor = Color.LawnGreen;
            quickReloadBtn.Location = new Point(12, 381);
            quickReloadBtn.Name = "quickReloadBtn";
            quickReloadBtn.Size = new Size(131, 57);
            quickReloadBtn.TabIndex = 13;
            quickReloadBtn.Text = "Disable Quick Reload: ON";
            quickReloadBtn.UseVisualStyleBackColor = false;
            quickReloadBtn.Click += BooleanButton_Click;
            quickReloadBtn.MouseHover += quickReloadBtn_MouseHover;
            // 
            // pausingBtn
            // 
            pausingBtn.BackColor = Color.LawnGreen;
            pausingBtn.Location = new Point(12, 318);
            pausingBtn.Name = "pausingBtn";
            pausingBtn.Size = new Size(131, 57);
            pausingBtn.TabIndex = 12;
            pausingBtn.Text = "Disable Pausing: ON";
            pausingBtn.UseVisualStyleBackColor = false;
            pausingBtn.Click += BooleanButton_Click;
            pausingBtn.MouseHover += pausingBtn_MouseHover;
            // 
            // permadeathBtn
            // 
            permadeathBtn.BackColor = Color.Red;
            permadeathBtn.Location = new Point(12, 255);
            permadeathBtn.Name = "permadeathBtn";
            permadeathBtn.Size = new Size(131, 57);
            permadeathBtn.TabIndex = 11;
            permadeathBtn.Text = "Permadeath: OFF";
            permadeathBtn.UseVisualStyleBackColor = false;
            permadeathBtn.Click += BooleanButton_Click;
            permadeathBtn.MouseHover += permadeathBtn_MouseHover;
            // 
            // doubleDamageBtn
            // 
            doubleDamageBtn.BackColor = Color.LawnGreen;
            doubleDamageBtn.Location = new Point(12, 192);
            doubleDamageBtn.Name = "doubleDamageBtn";
            doubleDamageBtn.Size = new Size(131, 57);
            doubleDamageBtn.TabIndex = 10;
            doubleDamageBtn.Text = "Double Damage: ON";
            doubleDamageBtn.UseVisualStyleBackColor = false;
            doubleDamageBtn.Click += BooleanButton_Click;
            doubleDamageBtn.MouseHover += doubleDamageBtn_MouseHover;
            // 
            // donationBtn
            // 
            donationBtn.BackgroundImage = (Image)resources.GetObject("donationBtn.BackgroundImage");
            donationBtn.BackgroundImageLayout = ImageLayout.Stretch;
            donationBtn.Location = new Point(12, 12);
            donationBtn.Name = "donationBtn";
            donationBtn.Size = new Size(40, 32);
            donationBtn.TabIndex = 19;
            donationBtn.UseVisualStyleBackColor = true;
            donationBtn.Click += donationBtn_Click;
            donationBtn.MouseHover += donationBtn_MouseHover;
            // 
            // warningLabel
            // 
            warningLabel.AutoSize = true;
            warningLabel.BackColor = Color.OrangeRed;
            warningLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            warningLabel.Location = new Point(501, 436);
            warningLabel.Name = "warningLabel";
            warningLabel.Size = new Size(299, 15);
            warningLabel.TabIndex = 20;
            warningLabel.Text = "WARNING: Pausing is disabled; this can cause crashes";
            // 
            // MGS3Launcher
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(800, 450);
            ControlBox = false;
            Controls.Add(donationBtn);
            Controls.Add(locateExeBtn);
            Controls.Add(launchGameBtn);
            Controls.Add(quitBtn);
            Controls.Add(permanentDamageBtn);
            Controls.Add(guardStatusesBtn);
            Controls.Add(quickReloadBtn);
            Controls.Add(pausingBtn);
            Controls.Add(permadeathBtn);
            Controls.Add(doubleDamageBtn);
            Controls.Add(warningLabel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "MGS3Launcher";
            ShowIcon = false;
            ShowInTaskbar = false;
            TopMost = true;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button locateExeBtn;
        private Button launchGameBtn;
        private Button quitBtn;
        private Button permanentDamageBtn;
        private Button guardStatusesBtn;
        private Button quickReloadBtn;
        private Button pausingBtn;
        private Button permadeathBtn;
        private Button doubleDamageBtn;
        private Button donationBtn;
        private Label warningLabel;
    }
}
