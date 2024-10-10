using MetalGearHardcore;

namespace Launcher
{
    public partial class GameLauncher : Form
    {
        /// <summary>
        /// Just a very inconsequential launcher so that users know they're actually running the mod.
        /// </summary>
        public GameLauncher()
        {
            InitializeComponent();
            Task.Run(DelayGameStart);
        }

        private void DelayGameStart()
        {
            Thread.Sleep(10000);
            Invoke(new MethodInvoker(Hide));
            MGS2Hardcore.Main_Thread();
            Invoke(new MethodInvoker(Close));
        }
    }
}
