using SimplifiedMemoryManager;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MetalGearHardcore
{
    public static class MGS3Hardcore
    {
        #region Internals
        static Process mgs3Process;
        private static readonly IntPtr CurrentStagePtr = new IntPtr(0x00ACBE18);
        private const int CurrentStageOffset = 0x24;
        private static readonly IntPtr CurrentCharacterPtr = new IntPtr(0x00ACBE18); 
        private const int CurrentCharacterOffset = 0x14;
        private static ushort SnakeHealth = 1000;
        private static readonly IntPtr CurrentHealthPtr = new IntPtr(0x00ACBE18); 
        private const int CurrentHealthOffset = 0x684;
        private static readonly IntPtr MaxHpPtr = new IntPtr(0x00ACBE18);
        private const int MaxHpOffset = 0x686;
        private static readonly IntPtr CheckpointHealthPtr = new IntPtr(0x00ACBE20);
        private const int CheckpointHealthOffset = 0x684;
        private static readonly SimplePattern QuickMenuPauseAoB = new SimplePattern("F7 D1 21 0D E4 94 C6 01 C3 CC CC CC CC CC CC CC");
        private static byte[] DisableQuickMenuPauseBytes = new byte[] { 0x85, 0x05, 0x30, 0xF9, 0xA8, 0x01 };
        private static readonly SimplePattern FilterPattern = new SimplePattern("00 00 A0 49 00 00 00 00 FF FF FF 7F");
        private static readonly IntPtr QuickReloadLocation = new IntPtr(0x9AB9A);
        private const int QuickReloadLength = 4;
        private static readonly IntPtr XMovementCode = new IntPtr(0xB74E0);
        private static byte[] XMovementBytes = new byte[] { 0xF3, 0x0F, 0x11, 0x5F, 0x10 };
        private static readonly IntPtr ZMovementCode = new IntPtr(0xB74F0);
        private static byte[] ZMovementBytes = new byte[] { 0xF3, 0x0F, 0x11, 0x57, 0x18 };
        private static readonly IntPtr YMovementCode = new IntPtr(0xB74EA);
        private static byte[] YMovementBytes = new byte[] { 0xF3, 0x44, 0x0F, 0x11, 0x47, 0x14 };
        private static bool Permadeath = true;
        private static bool Permadamage = true;
        private static readonly IntPtr MaxAlertTimer1 = new IntPtr(0x01D772E8);
        private const int MaxAlertTimerPtr2 = 0x58;
        private const int MaxAlertTimerOffset = 0x34;
        private static readonly IntPtr MaxEvasionTimerPtr1 = new IntPtr(0x01D772E8);
        private const int MaxEvasionTimerPtr2 = 0x58;
        private const int MaxEvasionTimerOffset = 0x4C;
        private static readonly IntPtr MaxCautionTimerPtr1 = new IntPtr(0x01D772E8);
        private const int MaxCautionTimerPtr2 = 0x58;
        private const int MaxCautionTimerOffset = 0x38;
        private static bool PlayerIsFrozen;
        private static readonly IntPtr ContinuesPtr = new IntPtr(0x00ACBE18);
        private const int ContinuesOffset = 0x34;
        private static short LastKnownContinueCount = short.MaxValue;
        private static readonly IntPtr DifficultyLevelPtr = new IntPtr(0x00ACBE18);
        private const int DifficultyOffset = 0x6;
        private static CancellationTokenSource tokenSource = new CancellationTokenSource();
        private const string CompatibleGameVersion = "2.0.0.0";
        private static bool DisableAllModifiers = true;
        private static MGS3Stage.LocationString currentLocation;
        private static bool DoubleDamage = false;
        private static byte MenuStateByte = 1;
        private static byte[] OriginalQuickMenuBytes;

        private static Process GetProcess()
        {
            return Process.GetProcessesByName("METAL GEAR SOLID3").FirstOrDefault();
        }

        private static MGS3Stage.LocationString GetCurrentStage()
        {
            try
            {
                lock (mgs3Process)
                {
                    using (SimpleProcessProxy proxy = new SimpleProcessProxy(mgs3Process))
                    {
                        IntPtr currentStageLocation = proxy.FollowPointer(CurrentStagePtr, false);
                        currentStageLocation = IntPtr.Add(currentStageLocation, CurrentStageOffset);
                        byte[] stageBytes = proxy.GetMemoryFromPointer(currentStageLocation, 5);
                        string stageString = Encoding.Default.GetString(stageBytes);
                        bool successfulParse = Enum.TryParse(stageString, true, out MGS3Stage.LocationString location);
                        if (successfulParse)
                        {
                            return location;
                        }
                        else
                        {
                            return MGS3Stage.LocationString.InvalidStage;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to get current stage");
                return MGS3Stage.LocationString.InvalidStage;
            }
        }

        private static void MonitorCurrentStage()
        {
            while (true)
            {
                currentLocation = GetCurrentStage();
                if(currentLocation != MGS3Stage.LocationString.InvalidStage)
                {
                    DisableAllModifiers = false;
                }
                else
                {
                    DisableAllModifiers = true;
                }
            }
        }

        private static byte[] GetCurrentDifficulty()
        {
            try
            {
                lock (mgs3Process)
                {
                    using (SimpleProcessProxy proxy = new SimpleProcessProxy(mgs3Process))
                    {
                        IntPtr currentDifficultyLocation = proxy.FollowPointer(DifficultyLevelPtr, false);
                        currentDifficultyLocation = IntPtr.Add(currentDifficultyLocation, DifficultyOffset);

                        return proxy.GetMemoryFromPointer(currentDifficultyLocation, 1);
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Failed to get current difficulty");
                return null;
            }
        }

        private static SupportedCharacter GetCurrentChara()
        {
            lock (mgs3Process)
            {
                using (SimpleProcessProxy spp = new SimpleProcessProxy(mgs3Process))
                {
                    IntPtr characterLocation = IntPtr.Add(spp.FollowPointer(CurrentCharacterPtr, false), CurrentCharacterOffset);
                    byte[] characterBytes = spp.GetMemoryFromPointer(characterLocation, 10);

                    string character = Encoding.UTF8.GetString(characterBytes).Trim();


                    if (character.Contains("r_sna"))
                    {
                        return SupportedCharacter.Snake;
                    }
                    else
                    {
                        throw new NotImplementedException("The current character is not supported by the MGS Hardcore mod.");
                    }
                }
            }
        }

        private static byte[] NopArray(int num)
        {
            byte[] array = new byte[num];
            for (int i = 0; i < num; i++)
            {
                array[i] = 0x90;
            }
            return array;
        }
        #endregion

        public static void Main_Thread()
        {
            MGS3GameOptions gameOptions = MGS3IniHandler.ParseIniFile();

            string gameLocation = gameOptions.GameLocation;
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(gameLocation);
            if (versionInfo.ProductVersion != CompatibleGameVersion)
            {
                MessageBox.Show("MGS3 Hardcore is only compatible with the Master Collection MGS3, version 2.0.0.0 on Steam");
                return;
            }
            FileInfo fileInfo = new FileInfo(gameLocation);
            ProcessStartInfo mgs3StartInfo = new ProcessStartInfo(gameLocation)
            {
                WorkingDirectory = fileInfo.DirectoryName,
                UseShellExecute = false,
                CreateNoWindow = false,
            };
            Process.Start(mgs3StartInfo);
            while (mgs3Process == null)
            {
                mgs3Process = GetProcess();
            }
            Console.WriteLine("Found MGS3!");
            SupportedCharacter character = SupportedCharacter.Unknown;
            while (character != SupportedCharacter.Snake && !mgs3Process.HasExited)
            {
                try
                {
                    character = GetCurrentChara();
                }
                catch
                {
                }
            }
            Console.WriteLine($"Current character found: {character}");
            Task.Factory.StartNew(MonitorCurrentStage, tokenSource.Token);

            Permadamage = gameOptions.PermanentDamage;
            Permadeath = gameOptions.Permadeath;
            DoubleDamage = gameOptions.DoubleDamage;
            if(Permadamage || Permadeath)
                MonitorPlayerHealth(tokenSource.Token);
            if (gameOptions.ExtendGuardStatuses)
                ExtendGuardStatuses(tokenSource.Token);
            if (gameOptions.DisablePausing)
                DisablePauses(tokenSource.Token);
            if (gameOptions.DisableQuickReload)
                DisableQuickReload(tokenSource.Token);
            while (!mgs3Process.HasExited)
            {

            }
            tokenSource.Cancel();
        }

        #region Permanent Damage & Permadeath (NEEDS FINAL CONFIRMATION)
        private static void MonitorPlayerHealth(CancellationToken token)
        {
            Console.WriteLine("Monitoring player health...");
            //monitor current HP
            Task.Factory.StartNew(MonitorCurrentHealth, token);
        }

        private static void MonitorCurrentHealth()
        {
            try
            {
                while (true)
                {
                    if (DisableAllModifiers)
                        continue;
                    try
                    {
                        //SupportedCharacter currentCharacter = GetCurrentChara();
                        lock (mgs3Process)
                        {
                            using (SimpleProcessProxy proxy = new SimpleProcessProxy(mgs3Process))
                            {
                                IntPtr continuesLocation = proxy.FollowPointer(ContinuesPtr, false);
                                continuesLocation = IntPtr.Add(continuesLocation, ContinuesOffset);
                                byte[] currentContinues = proxy.GetMemoryFromPointer(continuesLocation, 2);
                                short currentContinuesParsed = BitConverter.ToInt16(currentContinues, 0);
                                IntPtr healthLocation = proxy.FollowPointer(CurrentHealthPtr, false);
                                healthLocation = IntPtr.Add(healthLocation, CurrentHealthOffset);
                                byte[] currentHealth = proxy.GetMemoryFromPointer(healthLocation, 2);
                                ushort currentHealthInt = BitConverter.ToUInt16(currentHealth, 0);
                                IntPtr maxHealthLocation = proxy.FollowPointer(MaxHpPtr, false);
                                maxHealthLocation = IntPtr.Add(maxHealthLocation, MaxHpOffset);
                                byte[] currentMaxHp = proxy.GetMemoryFromPointer(maxHealthLocation, 2);
                                ushort currentMaxHpShort = BitConverter.ToUInt16(currentMaxHp, 0);
                                IntPtr checkpointHealthLocation = proxy.FollowPointer(CheckpointHealthPtr, false);
                                checkpointHealthLocation = IntPtr.Add(checkpointHealthLocation, CheckpointHealthOffset);
                                byte[] checkpointHealthBytes = proxy.GetMemoryFromPointer(checkpointHealthLocation, 2);
                                ushort checkpointHealth = BitConverter.ToUInt16(checkpointHealthBytes, 0);

                                if (Permadeath)
                                {
                                    if (currentContinuesParsed > LastKnownContinueCount)
                                    {
                                        SnakeHealth = 0;
                                        proxy.SetMemoryAtPointer(healthLocation, BitConverter.GetBytes(0));
                                        continue;
                                    }
                                    else
                                    {
                                        LastKnownContinueCount = currentContinuesParsed;
                                    }
                                }
                                
                                if (currentLocation != MGS3Stage.LocationString.v007a
                                        && currentLocation != MGS3Stage.LocationString.s141a
                                        && currentLocation != MGS3Stage.LocationString.InvalidStage)
                                {
                                    if(SnakeHealth > checkpointHealth)
                                    {
                                        SnakeHealth = checkpointHealth;
                                        continue;
                                    }
                                    if (currentHealthInt <= SnakeHealth)
                                    {
                                        if (DoubleDamage)
                                        {
                                            ushort damageTaken = (ushort)(SnakeHealth - currentHealthInt);
                                            short healthAfterDoubleDamage = (short) (SnakeHealth - (damageTaken * 2));
                                            if(healthAfterDoubleDamage > 0)
                                            {
                                                SnakeHealth = (ushort) healthAfterDoubleDamage;
                                            }
                                            else if(currentHealthInt > 0)
                                            {
                                                proxy.SetMemoryAtPointer(healthLocation, BitConverter.GetBytes(0));
                                                SnakeHealth = checkpointHealth;
                                            }
                                        }
                                        else
                                        {
                                            SnakeHealth = currentHealthInt;
                                        }
                                    }
                                    else
                                    {
                                        //if HP goes up, reset back down to last known value
                                        if (Permadamage)
                                            proxy.SetMemoryAtPointer(healthLocation, BitConverter.GetBytes(SnakeHealth));
                                    }
                                }
                            }
                        }
                        Thread.Sleep(100);
                    }
                    catch (Exception e)
                    {
                        //Console.WriteLine("Could not get current health");
                        if (e is NotImplementedException)
                        {
                            //Console.WriteLine("Player doesn't seem to be playing as story mode Snake, setting health to max");
                            //SnakeHealth = 1000;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                MessageBox.Show($"SOMEHOW WE BROKE OUT OF MONITOR CURRENT HEALTH: {e}");
            }
        }
        #endregion

        #region Double Damage(abandoned in this form because end of virtuous mission and sorrow fight dont play kosher with it)
        private static void ForceHalfMaxHp(CancellationToken token)
        {
            //first pass looked good, it was the only thing working while everything else wasnt KEKW
            Task.Factory.StartNew(HalfMaxHp, token);
        }

        private static void HalfMaxHp()
        {
            //this isnt a "true" half max hp, and in fact is a bit more brutal, but this is easier and honestly makes more sense.
            //i mean, why would snake get MORE hp after virtuous mission???
            //TODO: fix bug where you just fuckin DIE at the end of virtuous mission KEKW
            while (true)
            {
                if (DisableAllModifiers)
                    continue;
                byte currentDifficulty = GetCurrentDifficulty()[0];
                short desiredMaxHp;

                switch(currentDifficulty)
                {
                    case 0x0A: //Very Easy
                        //400 max hp
                        desiredMaxHp = 200;
                        break;
                    case 0x14: //Easy
                        //300 max hp
                        desiredMaxHp = 150;
                        break;
                    case 0x1E: //Normal
                        //200 max hp (wat)
                        desiredMaxHp = 125;
                        break;
                    case 0x28: //Hard
                        //250 max hp (in snake eater)
                        //200 max hp (in virtuous mission)
                        desiredMaxHp = 100;
                        break;
                    case 0x32: //Extreme
                        //130 max hp (at rassvet, snake eater)
                        desiredMaxHp = 75;
                        break;
                    case 0x3C: //Euro Extreme
                        //125 max hp (at start)
                        desiredMaxHp = 60;
                        break;
                    default:
                        continue;
                }

                lock (mgs3Process)
                {
                    try
                    {
                        using (SimpleProcessProxy spp = new SimpleProcessProxy(mgs3Process))
                        {
                            IntPtr maxHpPtr = spp.FollowPointer(MaxHpPtr, false);
                            maxHpPtr = IntPtr.Add(maxHpPtr, MaxHpOffset);
                            short currentMaxHp = BitConverter.ToInt16(spp.GetMemoryFromPointer(maxHpPtr, 2), 0);
                            if (currentMaxHp == desiredMaxHp)
                            {
                                continue;
                            }
                            else
                            {
                                if (currentLocation != MGS3Stage.LocationString.v007a && currentLocation != MGS3Stage.LocationString.s141a)
                                {
                                    spp.SetMemoryAtPointer(maxHpPtr, BitConverter.GetBytes(desiredMaxHp));
                                }
                                else
                                {
                                    //NOTE: trying to account for end of virtuous mission and sorrow fight
                                    spp.SetMemoryAtPointer(maxHpPtr, BitConverter.GetBytes(desiredMaxHp * 2));
                                }
                            }
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }
        #endregion

        #region Extend Guard Statuses
        private static void ExtendGuardStatuses(CancellationToken token)
        {
            Task.Factory.StartNew(ForceAlertTimer, token);
            Task.Factory.StartNew(ForceEvasionTimer, token);
            Task.Factory.StartNew(PersistCautionTimer, token);
        }

        private static void ForceAlertTimer()
        {
            bool recentlyForced = false;
            IntPtr alertTimer = IntPtr.Zero;
            while (true)
            {
                if (DisableAllModifiers)
                    continue;
                try
                {
                    Thread.Sleep(200);
                    lock (mgs3Process)
                    {
                        using (SimpleProcessProxy spp = new SimpleProcessProxy(mgs3Process))
                        {
                            if (alertTimer == IntPtr.Zero)
                            {
                                alertTimer = spp.FollowPointer(MaxAlertTimer1, false);
                                alertTimer = new IntPtr(BitConverter.ToInt64(spp.GetMemoryFromPointer(IntPtr.Add(alertTimer, MaxAlertTimerPtr2), 8), 0));
                                alertTimer = IntPtr.Add(alertTimer, MaxAlertTimerOffset);
                            }
                            int currentAlertTimer = BitConverter.ToInt32(spp.GetMemoryFromPointer(alertTimer, 4), 0);
                            if (currentAlertTimer == 0)
                            {
                                continue;
                            }
                            if (17500 < currentAlertTimer && currentAlertTimer < 17750 && !recentlyForced)
                            {
                                spp.SetMemoryAtPointer(alertTimer, BitConverter.GetBytes(36000));
                                recentlyForced = true;
                            }
                            if(currentAlertTimer < 17000)
                            {
                                recentlyForced = false;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Thread.Sleep(1000);
                    //Console.WriteLine($"Failed to force alert timer: {e}");
                }
                finally { }
            }
        }

        private static void ForceEvasionTimer()
        {
            bool recentlyForced = false;
            IntPtr evasionTimer = IntPtr.Zero;
            while (true)
            {
                if (DisableAllModifiers)
                    continue;
                try
                {
                    Thread.Sleep(200);
                    lock (mgs3Process)
                    {
                        using (SimpleProcessProxy spp = new SimpleProcessProxy(mgs3Process))
                        {
                            if (evasionTimer == IntPtr.Zero)
                            {
                                evasionTimer = spp.FollowPointer(MaxEvasionTimerPtr1, false);
                                evasionTimer = new IntPtr(BitConverter.ToInt64(spp.GetMemoryFromPointer(IntPtr.Add(evasionTimer, MaxEvasionTimerPtr2), 8), 0));
                                evasionTimer = IntPtr.Add(evasionTimer, MaxEvasionTimerOffset);
                            }
                            int currentEvasionTimer = BitConverter.ToInt32(spp.GetMemoryFromPointer(evasionTimer, 4), 0);
                            if (currentEvasionTimer == 0)
                                continue;
                            if (17500 < currentEvasionTimer && currentEvasionTimer < 17750 && !recentlyForced)
                            {
                                spp.SetMemoryAtPointer(evasionTimer, BitConverter.GetBytes(36000));
                                recentlyForced = true;
                            }
                            if (currentEvasionTimer < 17000)
                            {
                                recentlyForced = false;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Thread.Sleep(1000);
                    //Console.WriteLine($"Failed to force evasion timer: {e}");
                }
                finally { }
            }
        }

        private static void PersistCautionTimer()
        {
            bool recentlyForced = false;
            IntPtr cautionTimer = IntPtr.Zero;
            while (true)
            {
                if (DisableAllModifiers)
                    continue;
                try
                {
                    Thread.Sleep(200);
                    lock (mgs3Process)
                    {
                        using (SimpleProcessProxy spp = new SimpleProcessProxy(mgs3Process))
                        {
                            if (cautionTimer == IntPtr.Zero)
                            {
                                cautionTimer = spp.FollowPointer(MaxCautionTimerPtr1, false);
                                cautionTimer = new IntPtr(BitConverter.ToInt64(spp.GetMemoryFromPointer(IntPtr.Add(cautionTimer, MaxCautionTimerPtr2), 8), 0));
                                cautionTimer = IntPtr.Add(cautionTimer, MaxCautionTimerOffset);
                            }
                            int currentCautionTimer = BitConverter.ToInt32(spp.GetMemoryFromPointer(cautionTimer, 4), 0);
                            if (currentCautionTimer == 0)
                                continue;
                            if (17500 < currentCautionTimer && currentCautionTimer < 17750 && !recentlyForced)
                            {
                                spp.SetMemoryAtPointer(cautionTimer, BitConverter.GetBytes(36000));
                                recentlyForced = true;
                            }
                            if (currentCautionTimer < 17000)
                            {
                                recentlyForced = false;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Thread.Sleep(1000);
                    //Console.WriteLine($"Failed to force caution timer: {e}");
                }
                finally { }
            }
        }
        #endregion

        #region Disable Pausing
        private static void DisablePauses(CancellationToken token)
        {
            IntPtr menuWindowStateLocation = new IntPtr();
            IntPtr quickMenuPauseAoBLocation = new IntPtr();
            try
            {
                lock (mgs3Process)
                {
                    using (SimpleProcessProxy spp = new SimpleProcessProxy(mgs3Process))
                    {
                        quickMenuPauseAoBLocation = spp.ScanMemoryForUniquePattern(QuickMenuPauseAoB);
                        quickMenuPauseAoBLocation = IntPtr.Add(quickMenuPauseAoBLocation, 0x3FF);
                        IntPtr filterLocation = spp.ScanMemoryForPattern(FilterPattern).FirstOrDefault();
                        //menuWindowStateLocation = IntPtr.Add(filterLocation, 0x215AC);
                        menuWindowStateLocation = IntPtr.Add(filterLocation, 0x22B6C);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Something went wrong with disabling pausing: {e}");
            }

            Task.Factory.StartNew(() => DisableMovementWhileInMenu(menuWindowStateLocation), token);
            Task.Factory.StartNew(() => EnsureQuickMenuPauseStaysDisabled(quickMenuPauseAoBLocation), token);
        }

        private static void EnsureQuickMenuPauseStaysDisabled(IntPtr quickMenuPauseAoBLocation)
        {
            while (true)
            {
                if (DisableAllModifiers)
                    continue;
                lock (mgs3Process)
                {
                    using (SimpleProcessProxy spp = new SimpleProcessProxy(mgs3Process))
                    {
                        byte[] currentMenuPauseBytes = spp.ReadProcessOffset(quickMenuPauseAoBLocation, 6);
                        if (!DisableQuickMenuPauseBytes.SequenceEqual(currentMenuPauseBytes))
                        {
                            OriginalQuickMenuBytes = currentMenuPauseBytes;
                        }
                        if (MenuStateByte == 1)
                        {
                            spp.ModifyProcessOffset(quickMenuPauseAoBLocation, OriginalQuickMenuBytes, true);
                        }
                        else
                        {
                            spp.ModifyProcessOffset(quickMenuPauseAoBLocation, DisableQuickMenuPauseBytes, true);
                        }
                    }
                }
                Thread.Sleep(500); //changing to check twice a second now to try and avoid real menuing issues
            }
        }

        private static void DisableMovementWhileInMenu(IntPtr menuWindowStateLocation)
        {
            while (true)
            {
                if (DisableAllModifiers)
                    continue;
                Thread.Sleep(100);
                lock (mgs3Process)
                {
                    try
                    {
                        using (SimpleProcessProxy spp = new SimpleProcessProxy(mgs3Process))
                        {
                            MenuStateByte = spp.ReadProcessOffset(menuWindowStateLocation, 1)[0];

                            switch (MenuStateByte)
                            {
                                case 0:
                                case 1:
                                    //either live in-game, or in codec/inventory menu
                                    FreezePlayerToggle(false, spp);
                                    break;
                                case 4:
                                    //in either weapon/item menu
                                    FreezePlayerToggle(true, spp);
                                    break;
                            }
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }

        private static void FreezePlayerToggle(bool freeze, SimpleProcessProxy spp)
        {
            if (freeze)
            {
                if (!PlayerIsFrozen)
                {
                    spp.ModifyProcessOffset(XMovementCode, NopArray(XMovementBytes.Length), true);
                    spp.ModifyProcessOffset(ZMovementCode, NopArray(ZMovementBytes.Length), true);
                    spp.ModifyProcessOffset(YMovementCode, NopArray(YMovementBytes.Length), true);
                    PlayerIsFrozen = true;
                }
            }
            else if (PlayerIsFrozen)
            {
                spp.ModifyProcessOffset(XMovementCode, XMovementBytes, true);
                spp.ModifyProcessOffset(ZMovementCode, ZMovementBytes, true);
                spp.ModifyProcessOffset(YMovementCode, YMovementBytes, true);
                PlayerIsFrozen = false;
            }
        }
        #endregion

        #region Disable Quick Reload
        private static void DisableQuickReload(CancellationToken token)
        {
            Task.Factory.StartNew(MonitorQuickReload, token);
        }

        private static void MonitorQuickReload()
        {
            //been updated, needs retesting
            byte[] disablingQuickReloadBytes = NopArray(QuickReloadLength);
            while (true)
            {
                if (DisableAllModifiers)
                    continue;
                try
                {
                    lock (mgs3Process)
                    {
                        using (SimpleProcessProxy spp = new SimpleProcessProxy(mgs3Process))
                        {
                            byte[] currentStatus = spp.ReadProcessOffset(QuickReloadLocation, QuickReloadLength);
                            if (!currentStatus.SequenceEqual(disablingQuickReloadBytes))
                                spp.ModifyProcessOffset(QuickReloadLocation, disablingQuickReloadBytes, true);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to disable quick reload: {e}");
                }
                Thread.Sleep(60000); //check/perform this only once a minute
            }
        }
        #endregion
    }
}
