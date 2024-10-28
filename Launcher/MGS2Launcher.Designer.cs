namespace Launcher
{
    partial class MGS2Launcher
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MGS2Launcher));
            bleedingKillsBtn = new Button();
            permadeathBtn = new Button();
            pausingBtn = new Button();
            quickReloadBtn = new Button();
            permanentDamageBtn = new Button();
            guardStatusesBtn = new Button();
            quitBtn = new Button();
            launchGameBtn = new Button();
            locateExeBtn = new Button();
            donationBtn = new Button();
            SuspendLayout();
            // 
            // bleedingKillsBtn
            // 
            bleedingKillsBtn.BackColor = Color.LawnGreen;
            bleedingKillsBtn.Location = new Point(12, 192);
            bleedingKillsBtn.Name = "bleedingKillsBtn";
            bleedingKillsBtn.Size = new Size(131, 57);
            bleedingKillsBtn.TabIndex = 0;
            bleedingKillsBtn.Text = "Bleeding Kills: ON";
            bleedingKillsBtn.UseVisualStyleBackColor = false;
            bleedingKillsBtn.Click += BooleanButton_Click;
            bleedingKillsBtn.MouseHover += bleedingKillsBtn_MouseHover;
            // 
            // permadeathBtn
            // 
            permadeathBtn.BackColor = Color.Red;
            permadeathBtn.Location = new Point(12, 255);
            permadeathBtn.Name = "permadeathBtn";
            permadeathBtn.Size = new Size(131, 57);
            permadeathBtn.TabIndex = 2;
            permadeathBtn.Text = "Permadeath: OFF";
            permadeathBtn.UseVisualStyleBackColor = false;
            permadeathBtn.Click += BooleanButton_Click;
            permadeathBtn.MouseHover += permadeathBtn_MouseHover;
            // 
            // pausingBtn
            // 
            pausingBtn.BackColor = Color.LawnGreen;
            pausingBtn.Location = new Point(12, 318);
            pausingBtn.Name = "pausingBtn";
            pausingBtn.Size = new Size(131, 57);
            pausingBtn.TabIndex = 3;
            pausingBtn.Text = "Disable Pausing: ON";
            pausingBtn.UseVisualStyleBackColor = false;
            pausingBtn.Click += BooleanButton_Click;
            pausingBtn.MouseHover += pausingBtn_MouseHover;
            // 
            // quickReloadBtn
            // 
            quickReloadBtn.BackColor = Color.LawnGreen;
            quickReloadBtn.Location = new Point(12, 381);
            quickReloadBtn.Name = "quickReloadBtn";
            quickReloadBtn.Size = new Size(131, 57);
            quickReloadBtn.TabIndex = 4;
            quickReloadBtn.Text = "Disable Quick Reload: ON";
            quickReloadBtn.UseVisualStyleBackColor = false;
            quickReloadBtn.Click += BooleanButton_Click;
            quickReloadBtn.MouseHover += quickReloadBtn_MouseHover;
            // 
            // permanentDamageBtn
            // 
            permanentDamageBtn.BackColor = Color.LawnGreen;
            permanentDamageBtn.Location = new Point(149, 381);
            permanentDamageBtn.Name = "permanentDamageBtn";
            permanentDamageBtn.Size = new Size(131, 57);
            permanentDamageBtn.TabIndex = 6;
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
            guardStatusesBtn.TabIndex = 5;
            guardStatusesBtn.Text = "Extend Guard Statuses: ON";
            guardStatusesBtn.UseVisualStyleBackColor = false;
            guardStatusesBtn.Click += BooleanButton_Click;
            guardStatusesBtn.MouseHover += guardStatusesBtn_MouseHover;
            // 
            // quitBtn
            // 
            quitBtn.BackColor = Color.IndianRed;
            quitBtn.Location = new Point(657, 12);
            quitBtn.Name = "quitBtn";
            quitBtn.Size = new Size(131, 57);
            quitBtn.TabIndex = 7;
            quitBtn.Text = "Quit";
            quitBtn.UseVisualStyleBackColor = false;
            quitBtn.Click += quitBtn_Click;
            // 
            // launchGameBtn
            // 
            launchGameBtn.BackColor = SystemColors.ActiveCaption;
            launchGameBtn.Location = new Point(657, 381);
            launchGameBtn.Name = "launchGameBtn";
            launchGameBtn.Size = new Size(131, 57);
            launchGameBtn.TabIndex = 8;
            launchGameBtn.Text = "Launch MGS2";
            launchGameBtn.UseVisualStyleBackColor = false;
            launchGameBtn.Click += launchGameBtn_Click;
            // 
            // locateExeBtn
            // 
            locateExeBtn.Location = new Point(520, 381);
            locateExeBtn.Name = "locateExeBtn";
            locateExeBtn.Size = new Size(131, 57);
            locateExeBtn.TabIndex = 9;
            locateExeBtn.Text = "Locate Game Executable";
            locateExeBtn.UseVisualStyleBackColor = true;
            locateExeBtn.Click += locateExeBtn_Click;
            // 
            // donationBtn
            // 
            donationBtn.BackgroundImage = (Image)resources.GetObject("donationBtn.BackgroundImage");
            donationBtn.BackgroundImageLayout = ImageLayout.Stretch;
            donationBtn.Location = new Point(12, 12);
            donationBtn.Name = "donationBtn";
            donationBtn.Size = new Size(40, 32);
            donationBtn.TabIndex = 20;
            donationBtn.UseVisualStyleBackColor = true;
            donationBtn.Click += donationBtn_Click;
            donationBtn.MouseHover += donationBtn_MouseHover;
            // 
            // MGS2Launcher
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
            Controls.Add(bleedingKillsBtn);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "MGS2Launcher";
            ShowIcon = false;
            ShowInTaskbar = false;
            TopMost = true;
            ResumeLayout(false);
        }

        #endregion

        private Button bleedingKillsBtn;
        private Button permadeathBtn;
        private Button pausingBtn;
        private Button quickReloadBtn;
        private Button permanentDamageBtn;
        private Button guardStatusesBtn;
        private Button quitBtn;
        private Button launchGameBtn;
        private Button locateExeBtn;
        private Button donationBtn;
    }
}
