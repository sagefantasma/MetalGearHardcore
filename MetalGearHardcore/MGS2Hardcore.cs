using SimplifiedMemoryManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MetalGearHardcore
{
    public static class MGS2Hardcore
    {
        #region Internals
        static Process mgs2Process;
        private static readonly IntPtr CurrentStagePtr = new IntPtr(0x00948340);
        private const int CurrentStageOffset = 0x2C;
        private static readonly IntPtr CurrentCharacterPtr = new IntPtr(0x00948340);
        private const int CurrentCharacterOffset = 0x1C;
        private static int SnakeHealth = 200;
        private static int RaidenHealth = 200;
        private static readonly IntPtr CurrentHealthPtr = new IntPtr(0x017DE780);
        private const int CurrentHealthOffset = 0x8D2;
        private static readonly IntPtr CurrentAmmoPtr = new IntPtr(0x0153FC10);
        private const int CurrentAmmoOffset = 0x0;
        private static readonly IntPtr CurrentWeaponPtr = new IntPtr(0x00948340);
        private const int CurrentWeaponOffset = 0x104;
        private static readonly IntPtr CurrentMagazinePtr = new IntPtr(0x0); //TODO: need real value
        private const int CurrentMagazineOffset = 0x0; //TODO: need real value
        private const int CurrentMagazineHardcode = 0x16E894C;
        private static readonly IntPtr PauseButtonLocation = new IntPtr(0x6A29E);
        private const int PauseButtonLength = 5;
        private static readonly IntPtr ItemPauseLocation = new IntPtr(0x1EED79);
        private const int ItemPauseLength = 6;
        private static readonly IntPtr WeaponPauseLocation = new IntPtr(0x1F0877);
        private const int WeaponPauseLength = 6;
        private static readonly IntPtr ReloadMagLocation = new IntPtr(0x551082);
        private const int ReloadMagLength = 7;
        private static readonly IntPtr WeaponSwitchReload1Location = new IntPtr(0x4B04C8);
        private const int WeaponSwitchReload1Length = 6;
        private static readonly IntPtr WeaponSwitchReload2Location = new IntPtr(0x53B17C);
        private const int WeaponSwitchReload2Length = 6;
        private static readonly byte[] ReloadMagBytes = new byte[] { 0x44, 0x89, 0x05, 0xC3, 0x78, 0x19, 0x01 };
        private static bool ReloadDisabled = false;
        private static readonly List<int> WeaponsWithMags = new List<int> { 1, 2, 3, 4, 5, 15, 18, 19 };
        private static Dictionary<int, int> WeaponsWithMagsDict = new Dictionary<int, int>();
        private static bool Permadeath = true;
        private static bool Permadamage = true;
        private static bool DeathByBleeding = false;
        private static readonly IntPtr MaxAlertTimer = new IntPtr(0x016C8E10);
        private const int MaxAlertTimerOffset = 0x33C;
        private static readonly IntPtr MaxEvasionTimerPtr1 = new IntPtr(0x153F9D0);
        private const int MaxEvasionTimerPtr2 = 0x20;
        private const int MaxEvasionTimerOffset = 0x54;
        private static readonly IntPtr MaxCautionTimer = new IntPtr(0x17B2CC0);
        private const int MaxCautionTimerOffset = 0xD24;
        private static readonly IntPtr ContinueCountPtr = new IntPtr(0x948340);
        private const int ContinueCountOffset = 0x132;
        private const int GameTimerOffset = 0x10;
        private static int CurrentGameTime;
        private static CancellationTokenSource tokenSource = new CancellationTokenSource();
        /*
        private const int SetAlertTimerLocation = 0x199D10; //B9 00040000 -- mov ecx, 00000400
        private const int SetEvasionTimerLocation = 0x136C1F; //B9 B0040000 -- mov ecx, 000004B0
        private const int SetCautionTimerLocation = 0x137616; //B9 100E0000 -- mov ecx, 00000E10
        private const int SetCautionTimerLocation2 = 0x1375AE; //B9 100E0000 -- mov ecx, 00000E10*/
        //seems the game is "hardcoded" to set the caution timer to 3600, but one of the functions
        //called along the way while setting this hardcode IGNORES the parameter passed to it and sets the 
        //value to 3600 within that function itself. I think a literal potato coded this part.
        //and just setting the max timer like this is seemingly causing the game to crash. cool.
        private const string CompatibleGameVersion = "2.0.0.0";


        const int DLL_PROCESS_ATTACH = 0;
        const int DLL_THREAD_ATTACH = 1;
        const int DLL_THREAD_DETACH = 2;
        const int DLL_PROCESS_DETACH = 3;

        private static Process GetProcess()
        {
            return Process.GetProcessesByName("METAL GEAR SOLID2").FirstOrDefault();
        }

        private static byte[] GetCurrentStage()
        {
            try
            {
                lock (mgs2Process)
                {
                    using (SimpleProcessProxy proxy = new SimpleProcessProxy(mgs2Process))
                    {
                        return GetCurrentStage(proxy);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to get current stage");
                return null;
            }
        }

        private static byte[] GetCurrentStage(SimpleProcessProxy proxy)
        {
            try
            {
                IntPtr currentStageLocation = proxy.FollowPointer(CurrentStagePtr, false);
                currentStageLocation = IntPtr.Add(currentStageLocation, CurrentStageOffset);
                return proxy.GetMemoryFromPointer(currentStageLocation, 4);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to get current stage");
                return null;
            }
        }

        private static SupportedCharacter GetCurrentChara()
        {
            lock (mgs2Process)
            {
                using (SimpleProcessProxy spp = new SimpleProcessProxy(mgs2Process))
                {
                    IntPtr characterLocation = IntPtr.Add(spp.FollowPointer(CurrentCharacterPtr, false), CurrentCharacterOffset);
                    byte[] characterBytes = spp.GetMemoryFromPointer(characterLocation, 10);

                    string character = Encoding.UTF8.GetString(characterBytes).Trim();


                    if (character.Contains("r_tnk"))
                    {
                        return SupportedCharacter.Snake;
                    }
                    else if (character.Contains("r_plt"))
                    {
                        return SupportedCharacter.Raiden;
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

        private static string GetGameFilePath(string path)
        {
            List<string> filesInDirectory = Directory.GetFiles(Environment.CurrentDirectory).ToList();
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            string filePath = filesInDirectory.Find(file => file.Contains("METAL GEAR SOLID2.exe"));
            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("MGS2 Hardcore must be located in the same directory as the game. Please move MGS2 Hardcore and it's files to " +
                    "the same directory as 'METAL GEAR SOLID2.exe'");
                return null;
            }
            return filePath;
        }

        private static short GetContinueCount(SimpleProcessProxy spp)
        {
            IntPtr continuesLocation = spp.FollowPointer(ContinueCountPtr, false);
            continuesLocation = IntPtr.Add(continuesLocation, ContinueCountOffset);
            byte[] currentContinues = spp.GetMemoryFromPointer(continuesLocation, 2);
            return BitConverter.ToInt16(currentContinues, 0);
        }
        #endregion

        public static void Main_Thread()
        {
            MGS2GameOptions gameOptions = MGS2IniHandler.ParseIniFile();
            
            string gameLocation = gameOptions.GameLocation;
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(gameLocation);
            if (versionInfo.ProductVersion != CompatibleGameVersion)
            {
                MessageBox.Show("MGS2 Hardcore is only compatible with the Master Collection MGS2, version 2.0.0.0 on Steam");
                return;
            }
            FileInfo fileInfo = new FileInfo(gameLocation);            
            ProcessStartInfo mgs2StartInfo = new ProcessStartInfo(gameLocation)
            {
                WorkingDirectory = fileInfo.DirectoryName,
                UseShellExecute = false,
                CreateNoWindow = false,
            };
            Process.Start(mgs2StartInfo);
            while (mgs2Process == null)
            {
                mgs2Process = GetProcess();
            }
            Console.WriteLine("Found MGS2!");
            SupportedCharacter character = SupportedCharacter.Unknown;
            while (character != SupportedCharacter.Snake && character != SupportedCharacter.Raiden && !mgs2Process.HasExited)
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
            //Task gameTimerTask = Task.Factory.StartNew(MonitorGameTimer);

            Permadeath = gameOptions.Permadeath;
            Permadamage = gameOptions.PermanentDamage;
            DeathByBleeding = gameOptions.BleedingKills;

            if (Permadamage || Permadeath || DeathByBleeding)
                MonitorHP(tokenSource.Token);
            if (gameOptions.ExtendGuardStatuses)
                ExtendGuardStatuses(tokenSource.Token);
            if (gameOptions.DisablePausing)
                DisablePauses();
            if (gameOptions.DisableQuickReload)
                DisableQuickReload(tokenSource.Token);
            while (!mgs2Process.HasExited)
            {

            }
            tokenSource.Cancel();
        }

        #region Permanent Damage/Permanent Death/Death by Bleeding
        private static void MonitorHP(CancellationToken token)
        {
            //Console.WriteLine("Making damage permanent...");
            //monitor current HP
            Task.Factory.StartNew(MonitorCurrentHealth, token);
        }

        private static void CapRationsAtZero()
        {
            int maxRationOffset = 242;
            while (true)
            {
                try
                {
                    lock (mgs2Process)
                    {
                        using (SimpleProcessProxy proxy = new SimpleProcessProxy(mgs2Process))
                        {
                            IntPtr ammoOffset = proxy.FollowPointer(IntPtr.Add(CurrentAmmoPtr, CurrentAmmoOffset), false);
                            proxy.SetMemoryAtPointer(IntPtr.Add(ammoOffset, maxRationOffset), BitConverter.GetBytes(0));
                        }
                    }
                }
                catch (Exception e)
                {
                }
            }
        }

        private static void MonitorCurrentHealth()
        {
            while (true)
            {
                try
                {
                    SupportedCharacter currentCharacter = GetCurrentChara();
                    lock (mgs2Process)
                    {
                        using (SimpleProcessProxy proxy = new SimpleProcessProxy(mgs2Process))
                        {
                            IntPtr healthLocation = proxy.FollowPointer(CurrentHealthPtr, false);
                            byte[] currentHealth = proxy.GetMemoryFromPointer(IntPtr.Add(healthLocation, CurrentHealthOffset), 2);
                            short currentHealthInt = BitConverter.ToInt16(currentHealth, 0);
                            if (DeathByBleeding)
                            {
                                //this is a workaround for now
                                //TODO: is there a boolean that we can find that controls bleeding?
                                if (currentHealthInt == 1)
                                {
                                    //wait 5 seconds, then kill the shit out of the player.
                                    Thread.Sleep(5000);
                                    proxy.SetMemoryAtPointer(IntPtr.Add(healthLocation, CurrentHealthOffset), BitConverter.GetBytes(0));
                                }
                            }
                            if (Permadeath)
                            {
                                if (GetContinueCount(proxy) > 0)
                                {
                                    proxy.SetMemoryAtPointer(IntPtr.Add(healthLocation, CurrentHealthOffset), BitConverter.GetBytes(0));
                                }
                            }

                            if (currentCharacter == SupportedCharacter.Snake)
                            {
                                if(SnakeHealth == 0)
                                {
                                    proxy.SetMemoryAtPointer(IntPtr.Add(healthLocation, CurrentHealthOffset), BitConverter.GetBytes(0));
                                    continue;
                                }
                                if (currentHealthInt <= SnakeHealth)
                                {
                                    SnakeHealth = currentHealthInt;
                                }
                                else
                                {
                                    if (Permadamage)
                                    {
                                        //if HP goes up, reset back down to last known value
                                        proxy.SetMemoryAtPointer(IntPtr.Add(healthLocation, CurrentHealthOffset), BitConverter.GetBytes(SnakeHealth));
                                    }
                                }
                            }
                            else
                            {
                                if (RaidenHealth == 0)
                                {
                                    proxy.SetMemoryAtPointer(IntPtr.Add(healthLocation, CurrentHealthOffset), BitConverter.GetBytes(0));
                                    continue;
                                }
                                if (currentHealthInt <= RaidenHealth)
                                {
                                    RaidenHealth = currentHealthInt;
                                }
                                else
                                {
                                    //if HP goes up, reset back down to last known value
                                    if (Permadamage)
                                    {
                                        proxy.SetMemoryAtPointer(IntPtr.Add(healthLocation, CurrentHealthOffset), BitConverter.GetBytes(RaidenHealth));
                                    }
                                }
                            }
                        }
                    }
                    Thread.Sleep(200);
                }
                catch(Exception e)
                {
                    //Console.WriteLine("Could not get current health");
                    if(e is NotImplementedException)
                    {
                        Console.WriteLine("Player doesn't seem to be playing as story mode Snake/Raiden, setting health to max");
                        SnakeHealth = 200;
                        RaidenHealth = 200;
                    }
                }
            }
            
        }
        #endregion

        #region Extend Guard Statuses(buggy)
        private static void ExtendGuardStatuses(CancellationToken token)
        {
            Task.Factory.StartNew(ForceAlertTimer, token);
            Task.Factory.StartNew(ForceEvasionTimer, token);
            Task.Factory.StartNew(PersistCautionTimer, token);
        }

        private static void ForceAlertTimer()
        {
            bool recentlyForced = false;
            while (true)
            {
                try
                {
                    lock (mgs2Process)
                    {
                        using (SimpleProcessProxy spp = new SimpleProcessProxy(mgs2Process))
                        {
                            IntPtr alertTimer = IntPtr.Add(spp.FollowPointer(MaxAlertTimer, false), MaxAlertTimerOffset);
                            short currentAlertTimer = BitConverter.ToInt16(spp.GetMemoryFromPointer(alertTimer, 2), 0);
                            if (currentAlertTimer == 0)
                            {
                                continue;
                            }
                            if (900 < currentAlertTimer && currentAlertTimer < 1024 && !recentlyForced)
                            {
                                spp.SetMemoryAtPointer(alertTimer, BitConverter.GetBytes(3072));
                                recentlyForced = true;
                            }
                            else if(currentAlertTimer < 900)
                            {
                                recentlyForced = false;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    //Console.WriteLine($"Failed to force alert timer: {e}");
                }
                finally { }
            }
        }

        private static void ForceEvasionTimer()
        {
            bool recentlyForced = false;
            while (true)
            {
                try
                {
                    lock (mgs2Process)
                    {
                        using (SimpleProcessProxy spp = new SimpleProcessProxy(mgs2Process))
                        {
                            IntPtr evasionTimer = spp.FollowPointer(MaxEvasionTimerPtr1, false);
                            evasionTimer = new IntPtr(BitConverter.ToInt64(spp.GetMemoryFromPointer(IntPtr.Add(evasionTimer, MaxEvasionTimerPtr2), 8), 0));
                            evasionTimer = IntPtr.Add(evasionTimer, MaxEvasionTimerOffset);

                            short currentCautionTimer = BitConverter.ToInt16(spp.GetMemoryFromPointer(evasionTimer, 2), 0);
                            if(currentCautionTimer == 0)
                            {
                                continue;
                            }
                            if (1100 < currentCautionTimer && currentCautionTimer < 1200 && !recentlyForced)
                            {
                                spp.SetMemoryAtPointer(evasionTimer, BitConverter.GetBytes(3600));
                                recentlyForced = true;
                            }
                            else if (currentCautionTimer < 1100)
                            {
                                recentlyForced = false;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    //Console.WriteLine($"Failed to force evasion timer: {e}");
                }
                finally { }
            }
        }

        private static void PersistCautionTimer()
        {
            bool recentlyForced = false;
            while (true)
            {
                try
                {
                    lock (mgs2Process)
                    {
                        using (SimpleProcessProxy spp = new SimpleProcessProxy(mgs2Process))
                        {
                            IntPtr cautionTimer = IntPtr.Add(spp.FollowPointer(MaxCautionTimer, false), MaxCautionTimerOffset);
                            short currentCautionTimer = BitConverter.ToInt16(spp.GetMemoryFromPointer(cautionTimer, 2), 0);
                            if(currentCautionTimer == 0)
                            {
                                continue;
                            }
                            if (3500 < currentCautionTimer && currentCautionTimer < 3600 && !recentlyForced)
                            {
                                spp.SetMemoryAtPointer(cautionTimer, BitConverter.GetBytes(10800));
                                recentlyForced = true;
                            }
                            else if(currentCautionTimer < 3500)
                            {
                                recentlyForced = false;
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    //Console.WriteLine($"Failed to force caution timer: {e}");
                }
                finally { }
            }
        }
        #endregion

        #region Disable Pausing
        private static void DisablePauses()
        {
            //just disable all 3 sources of pausing
            try
            {
                lock (mgs2Process)
                {
                    using (SimpleProcessProxy spp = new SimpleProcessProxy(mgs2Process))
                    {
                        spp.ModifyProcessOffset(PauseButtonLocation, NopArray(PauseButtonLength), true);
                        spp.ModifyProcessOffset(ItemPauseLocation, NopArray(ItemPauseLength), true);
                        spp.ModifyProcessOffset(WeaponPauseLocation, NopArray(WeaponPauseLength), true);
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine($"Something went wrong with disabling pausing: {e}");
            }
            //could disable codec pausing too, but that can cause crashes iirc
        }
        #endregion

        #region Disable Quick Reload
        private static void DisableQuickReload(CancellationToken token)
        {
            /*
            int lastGameTime = CurrentGameTime;
            while(lastGameTime == CurrentGameTime && lastGameTime != 0)
            {
                //wait until the game is loaded and timer is going up
            }
            try
            {
                lock (mgs2Process)
                {
                    using (SimpleProcessProxy spp = new SimpleProcessProxy(mgs2Process))
                    {
                        //these are causing crashes now. sonuva. seems like the functions themselves are busted
                        Thread.Sleep(2000);
                        spp.ModifyProcessOffset(new IntPtr(WeaponSwitchReload1Location), NopArray(WeaponSwitchReload1Length), true);
                        spp.ModifyProcessOffset(new IntPtr(WeaponSwitchReload2Location), NopArray(WeaponSwitchReload2Length), true);
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine($"Failed to disable weapon switch reload: {e}");
            }*/
            Task.Factory.StartNew(MonitorMagazines, token);
        }

        private static void EnableReload(bool enable, SimpleProcessProxy spp)
        {
            if (!enable)
            {
                if (!ReloadDisabled)
                {
                    //Console.WriteLine("Reloading now disabled");
                    spp.ModifyProcessOffset(ReloadMagLocation, NopArray(ReloadMagLength), true);
                    ReloadDisabled = true;
                }
            }
            else
            {
                if (ReloadDisabled)
                {
                    //Console.WriteLine("Reloading now enabled");
                    spp.ModifyProcessOffset(ReloadMagLocation, ReloadMagBytes, true);
                    ReloadDisabled = false;
                }
            }
        }

        private static void MonitorMagazines()
        {
            //TODO: so now, the only thing that is undesirable is if you equip a weapon with no clip(setting in-memory mag count to 0)
            //and then equip a weapon with a clip, that weapon will always have its mag count set to 0. need to figure out some logic
            //to make that work better-er, but the rest of it is working fairly well.
            //ammo counts are accidentally shared between snake & raiden for m9 (low priority)
            //might be cool to add a "manual reload" functionality, if possible. would probably be a huge pain to add, though
            short lastEquippedWeapon = 0;
            byte[] lastStage = new byte[4];
            Console.WriteLine("Monitoring magazines");
            while (true)
            {
                /*
                 * If you have no weapon equipped, your current mag will represent the last known mag size of the last equipped weapon
                 * If you have a weapon with mag size equipped, your current mag will represent the actual current mag size
                 * If you have a weapon with NO mag size equipped, your current mag will always be 0.
                 */
                try
                {
                    byte[] currentStage = GetCurrentStage();
                    lock (mgs2Process)
                    {
                        using (SimpleProcessProxy spp = new SimpleProcessProxy(mgs2Process))
                        {
                            
                            //Disable reloading between zones
                            if(!lastStage.SequenceEqual(currentStage))
                            {
                                lastStage = currentStage;
                                //Console.WriteLine("Current stage is not matching last stage");
                                EnableReload(false, spp);
                                Thread.Sleep(2000); //wait for 2 seconds to avoid filling empty mag on zone transition
                                continue;
                            }
                            IntPtr currentWeaponLocation = spp.FollowPointer(CurrentWeaponPtr, false);
                            currentWeaponLocation = IntPtr.Add(currentWeaponLocation, CurrentWeaponOffset);
                            byte[] equippedWeapon = spp.GetMemoryFromPointer(currentWeaponLocation, 2);
                            short equippedWeaponInt = BitConverter.ToInt16(equippedWeapon, 0);
                            //Check to see if this is a magged weapon - if not, just note it and disable reloading
                            if (!WeaponsWithMags.Contains(equippedWeaponInt))
                            {
                                //Console.WriteLine("Currently equipped weapon has no mag");
                                EnableReload(false, spp);
                                lastEquippedWeapon = equippedWeaponInt;
                                continue;
                            }

                            //Check to see if this is a new weapon equipped
                            if(lastEquippedWeapon != equippedWeaponInt) 
                            {
                                //And check to see if the new equipped weapon has a mag
                                if (WeaponsWithMagsDict.ContainsKey(equippedWeaponInt))
                                {
                                    Console.WriteLine("New weapon with mag equipped");
                                    Thread.Sleep(50); //very tiny sleep to avoid our mag set from being overridden by the game's mag set
                                    spp.ModifyProcessOffset(new IntPtr(CurrentMagazineHardcode), WeaponsWithMagsDict[equippedWeaponInt], true);
                                    lastEquippedWeapon = equippedWeaponInt;
                                    Thread.Sleep(150);
                                    continue;
                                }
                            }

                            //Enable reloading if we have a magged weapon equipped and it is not a newly equipped item, and we're not in a new zone
                            EnableReload(true, spp);

                            byte[] bulletsInMag = spp.ReadProcessOffset(new IntPtr(CurrentMagazineHardcode), 2);
                            short bulletsInMagInt = BitConverter.ToInt16(bulletsInMag, 0);
                            //Check to see if we have already equipped this item at any point in this game instance
                            if (WeaponsWithMagsDict.ContainsKey(equippedWeaponInt))
                            {
                                //if the gun currently has more bullets than we were last aware of it having AND we were last aware of the gun having 0 bullets
                                //or if the gun has fewer bullets than we were last aware of
                                if ((bulletsInMagInt > WeaponsWithMagsDict[equippedWeaponInt] && WeaponsWithMagsDict[equippedWeaponInt] == 0) ||
                                    bulletsInMagInt < WeaponsWithMagsDict[equippedWeaponInt])
                                {
                                    //update our known count of bullets
                                    //Console.WriteLine("Updating mag count");
                                    WeaponsWithMagsDict[equippedWeaponInt] = bulletsInMagInt;
                                }
                                //otherwise, set the bullets in mag count to last known count
                                else
                                {
                                    //Console.WriteLine("Setting mag count");
                                    spp.ModifyProcessOffset(new IntPtr(CurrentMagazineHardcode), WeaponsWithMagsDict[equippedWeaponInt], true);
                                }
                            }
                            //otherwise, start tracking this new weapon
                            else
                            {
                                //Console.WriteLine("Adding new gun to dict");
                                WeaponsWithMagsDict.Add(equippedWeaponInt, bulletsInMagInt);
                            }
                            lastEquippedWeapon = equippedWeaponInt;
                        }
                    }
                }
                catch
                {

                }
                finally
                {
                    
                }
            }
        }
        #endregion
    }
}
