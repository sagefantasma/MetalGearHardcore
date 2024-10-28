using MetalGearHardcore;
using System.Diagnostics;

namespace Launcher
{
    public partial class MGS2Launcher : Form
    {
        MGS2GameOptions currentGameOptions;


        public MGS2Launcher()
        {
            InitializeComponent();
            //Task.Run(DelayGameStart);
            currentGameOptions = MGS2IniHandler.ParseIniFile();
            SetupUIElements();
        }

        private void SetOption(Button gameOption, bool enable)
        {
            int indexOfColon = gameOption.Text.IndexOf(':') + 1;
            if (enable)
            {
                gameOption.BackColor = Color.LawnGreen;
                gameOption.Text = gameOption.Text.Substring(0, indexOfColon) + " ON";
            }
            else
            {
                gameOption.BackColor = Color.Red;
                gameOption.Text = gameOption.Text.Substring(0, indexOfColon) + " OFF";
            }
            UpdateCurrentGameOptions(gameOption, enable);
        }

        private void UpdateCurrentGameOptions(Button button, bool enable)
        {
            if (button.Text.ToLower().Contains("bleeding"))
            {
                currentGameOptions.BleedingKills = enable;
            }
            else if (button.Text.ToLower().Contains("death"))
            {
                currentGameOptions.Permadeath = enable;
            }
            else if (button.Text.ToLower().Contains("pausing"))
            {
                currentGameOptions.DisablePausing = enable;
            }
            else if (button.Text.ToLower().Contains("reload"))
            {
                currentGameOptions.DisableQuickReload = enable;
            }
            else if (button.Text.ToLower().Contains("status"))
            {
                currentGameOptions.ExtendGuardStatuses = enable;
            }
            else if (button.Text.ToLower().Contains("damage"))
            {
                currentGameOptions.PermanentDamage = enable;
            }
        }

        private void ToggleOption(Button gameOption)
        {
            bool enable = false;

            if (gameOption.Text.Contains("OFF"))
            {
                enable = true;
            }

            SetOption(gameOption, enable);
        }

        private void SetupUIElements()
        {
            if (!File.Exists(currentGameOptions.GameLocation))
            {
                launchGameBtn.Enabled = false;
            }
            if (currentGameOptions.BleedingKills)
            {
                SetOption(bleedingKillsBtn, true);
            }
            else
            {
                SetOption(bleedingKillsBtn, false);
            }
            if (currentGameOptions.DisablePausing)
            {
                SetOption(pausingBtn, true);
            }
            else
            {
                SetOption(pausingBtn, false);
            }
            if (currentGameOptions.DisableQuickReload)
            {
                SetOption(quickReloadBtn, true);
            }
            else
            {
                SetOption(quickReloadBtn, false);
            }
            if (currentGameOptions.ExtendGuardStatuses)
            {
                SetOption(guardStatusesBtn, true);
            }
            else
            {
                SetOption(guardStatusesBtn, false);
            }
            if (currentGameOptions.Permadeath)
            {
                SetOption(permadeathBtn, true);
            }
            else
            {
                SetOption(permadeathBtn, false);
            }
            if (currentGameOptions.PermanentDamage)
            {
                SetOption(permanentDamageBtn, true);
            }
            else
            {
                SetOption(permanentDamageBtn, false);
            }
        }

        private void StartGame()
        {
            Invoke(new MethodInvoker(Hide));
            MGS2IniHandler.UpdateIniFile(currentGameOptions);
            MGS2Hardcore.Main_Thread();
            Invoke(new MethodInvoker(Close));
        }

        private void BooleanButton_Click(object sender, EventArgs e)
        {
            ToggleOption(sender as Button);
        }

        private void quitBtn_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void locateExeBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = currentGameOptions.GameLocation;
            dlg.Multiselect = false;
            dlg.Title = "Please find and select 'METAL GEAR SOLID2.exe'";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                currentGameOptions.GameLocation = dlg.FileName;
                if (File.Exists(currentGameOptions.GameLocation))
                {
                    launchGameBtn.Enabled = true;
                }
            }
        }

        private void launchGameBtn_Click(object sender, EventArgs e)
        {
            StartGame();
        }

        private void bleedingKillsBtn_MouseHover(object sender, EventArgs e)
        {
            ToolTip toolTip = new ToolTip();
            toolTip.Show("If you get down to 1 HP while bleeding, you will bleed out and die.", sender as IWin32Window);
        }

        private void permadeathBtn_MouseHover(object sender, EventArgs e)
        {
            ToolTip toolTip = new ToolTip();
            toolTip.Show("When your life reaches zero, the game is over. There are no continues, my friend.", sender as IWin32Window);
        }

        private void pausingBtn_MouseHover(object sender, EventArgs e)
        {
            ToolTip toolTip = new ToolTip();
            toolTip.Show("Disables the pause button, and the pausing that happens when you open the Item or Weapon menus. For game stability reasons, Codec pause still works.", sender as IWin32Window);
        }

        private void quickReloadBtn_MouseHover(object sender, EventArgs e)
        {
            ToolTip toolTip = new ToolTip();
            toolTip.Show("No more weapon switching to reload, you must manually reload.", sender as IWin32Window);
        }

        private void guardStatusesBtn_MouseHover(object sender, EventArgs e)
        {
            ToolTip toolTip = new ToolTip();
            toolTip.Show("When you've been found by guards, this will extend the amount of time they're in Alert, Evasion, and Caution modes.", sender as IWin32Window);
        }

        private void permanentDamageBtn_MouseHover(object sender, EventArgs e)
        {
            ToolTip toolTip = new ToolTip();
            toolTip.Show("Rations are a waste of good food. Defeating bosses will not replenish your health. Crouch/prone will not heal at low HP.", sender as IWin32Window);
        }

        private void donationBtn_MouseHover(object sender, EventArgs e)
        {
            ToolTip toolTip = new ToolTip();
            toolTip.Show("Like METAL GEAR HARDCORE? Consider donating to my Ko-fi to fund this and future projects!", sender as IWin32Window);
        }

        private void donationBtn_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://ko-fi.com/sagefantasma") { UseShellExecute = true });
        }
    }
}
