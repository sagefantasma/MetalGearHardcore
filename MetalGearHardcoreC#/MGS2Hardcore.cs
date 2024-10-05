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
    public class MGS2Hardcore
    {
        #region Internals
        Process mgs2Process;
        private const int CurrentStagePtr = 0x00948340;
        private const int CurrentStageOffset = 0x2C;
        private const int CurrentCharacterPtr = 0x00948340;
        private const int CurrentCharacterOffset = 0x1C;
        private static int SnakeHealth = 200;
        private static int RaidenHealth = 200;
        private const int CurrentHealthPtr = 0x017DE780;
        private const int CurrentHealthOffset = 0x8D2;
        private const int CurrentAmmoPtr = 0x0153FC10;
        private const int CurrentAmmoOffset = 0x0;
        private const int CurrentWeaponPtr = 0x00948340;
        private const int CurrentWeaponOffset = 0x104;
        private const int CurrentMagazinePtr = 0x0; //TODO: need real value
        private const int CurrentMagazineOffset = 0x0; //TODO: need real value
        private const int CurrentMagazineHardcode = 0x16E894C;
        private const int PauseButtonLocation = 0x6A29E;
        private const int PauseButtonLength = 5;
        private const int ItemPauseLocation = 0x1EED79;
        private const int ItemPauseLength = 6;
        private const int WeaponPauseLocation = 0x1F0877;
        private const int WeaponPauseLength = 6;
        private const int ReloadMagLocation = 0x551082;
        private const int ReloadMagLength = 7;
        private const int WeaponSwitchReload1Location = 0x4B04C8;
        private const int WeaponSwitchReload1Length = 6;
        private const int WeaponSwitchReload2Location = 0x53B17C;
        private const int WeaponSwitchReload2Length = 6;
        private readonly byte[] ReloadMagBytes = new byte[] { 0x44, 0x89, 0x05, 0xC3, 0x78, 0x19, 0x01 };
        private bool ReloadDisabled = false;
        private readonly List<int> WeaponsWithMags = new List<int> { 1, 2, 3, 4, 5, 15, 18, 19 };
        private Dictionary<int, int> WeaponsWithMagsDict = new Dictionary<int, int>();
        private bool Permadeath = true;
        private bool DeathByBleeding = false;
        private const int MaxAlertTimer = 0x016C8E10;
        private const int MaxAlertTimerOffset = 0x33C;
        private const int MaxEvasionTimerPtr1 = 0x153F9D0;
        private const int MaxEvasionTimerPtr2 = 0x20;
        private const int MaxEvasionTimerOffset = 0x54;
        private const int MaxCautionTimer = 0x17B2CC0;
        private const int MaxCautionTimerOffset = 0xD24;
        private const int GameTimerOffset = 0x10;
        private int CurrentGameTime;
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

        private Process GetProcess()
        {
            return Process.GetProcessesByName("METAL GEAR SOLID2").FirstOrDefault();
        }

        private byte[] GetCurrentStage()
        {
            try
            {
                lock (mgs2Process)
                {
                    using (SimpleProcessProxy proxy = new SimpleProcessProxy(mgs2Process))
                    {
                        IntPtr currentStageLocation = proxy.FollowPointer(new IntPtr(CurrentStagePtr), false);
                        currentStageLocation = IntPtr.Add(currentStageLocation, CurrentStageOffset);
                        return proxy.GetMemoryFromPointer(currentStageLocation, 4);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to get current stage");
                return null;
            }
        }

        private SupportedCharacter GetCurrentChara()
        {
            lock (mgs2Process)
            {
                using (SimpleProcessProxy spp = new SimpleProcessProxy(mgs2Process))
                {
                    IntPtr characterLocation = IntPtr.Add(spp.FollowPointer(new IntPtr(CurrentCharacterPtr), false), CurrentCharacterOffset);
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

        private byte[] NopArray(int num)
        {
            byte[] array = new byte[num];
            for (int i = 0; i < num; i++)
            {
                array[i] = 0x90;
            }
            return array;
        }
        #endregion

        public MGS2Hardcore()
        {
            GameOptions gameOptions = IniHandler.ParseIniFile();
            List<string> filesInDirectory = Directory.GetFiles(Environment.CurrentDirectory).ToList();

            string filePath = filesInDirectory.Find(file => file.Contains("METAL GEAR SOLID2.exe"));
            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("MGS2 Hardcore must be located in the same directory as the game. Please move MGS2 Hardcore and it's files to " +
                    "the same directory as 'METAL GEAR SOLID2.exe'");
                return;
            }
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(filePath);
            if(versionInfo.ProductVersion != CompatibleGameVersion)
            {
                MessageBox.Show("MGS2 Hardcore is only compatible with the Master Collection MGS2, version 2.0.0.0 on Steam");
                return;
            }
            Process.Start(filePath);
            while (mgs2Process == null)
            {
                mgs2Process = GetProcess();
            }
            Console.WriteLine("Found MGS2!");
            SupportedCharacter character = SupportedCharacter.Unknown;
            while (character != SupportedCharacter.Snake && character != SupportedCharacter.Raiden)
            {
                try
                {
                    character = GetCurrentChara();
                }
                catch { }
            }
            Console.WriteLine($"Current character found: {character}");
            //Task gameTimerTask = Task.Factory.StartNew(MonitorGameTimer);
            
            if(gameOptions.BleedingKills)
                BleedingKills();
            if (gameOptions.PermanentDamage)
                PermanentDamage();
            if (!gameOptions.Permadeath)
                DisablePermadeath();
            if (gameOptions.ExtendGuardStatuses)
                ExtendGuardStatuses();
            if (gameOptions.DisablePausing)
                DisablePauses();
            if (gameOptions.DisableQuickReload)
                DisableQuickReload();
            while (true)
            {

            }
        }

        private void MonitorGameTimer()
        {
            while (true)
            {
                string currentStage = Encoding.UTF8.GetString(GetCurrentStage());
                lock (mgs2Process)
                {
                    using (SimpleProcessProxy spp = new SimpleProcessProxy(mgs2Process))
                    {
                        if (currentStage.StartsWith("w"))
                        {
                            IntPtr currentStageLocation = spp.FollowPointer(new IntPtr(CurrentStagePtr), false);
                            currentStageLocation = IntPtr.Add(currentStageLocation, CurrentStageOffset);
                            long cslLong = currentStageLocation.ToInt64();
                            CurrentGameTime = BitConverter.ToInt32(spp.GetMemoryFromPointer(new IntPtr(cslLong + GameTimerOffset), 4), 0);
                        }
                    }
                }
                Thread.Sleep(1000);
            }
        }

        #region Bleeding Kills(NEEDS VERIFICATION)
        private void BleedingKills()
        {
            //As a workaround...
            Console.WriteLine("Allowing bleeding to kill");
            DeathByBleeding = true;
            /*
            //i've found where the bleeding damage is determined, but i haven't figure out yet how to FORCE it to kill...
            //turning the je to jmp _somehow_ does not result in the damage actually being dealt.
            try
            {
                throw new NotImplementedException();
            }
            catch
            {

            }
            */
        }
        #endregion

        #region Permanent Damage(NEEDS VERIFICATION)
        private void PermanentDamage()
        {
            //TODO: needs verification
            Console.WriteLine("Making damage permanent...");
            /*
             * I guess we could make this a separate option or something? But unnecessary for now(and never works atm, needs to be in a loop to work)
            int maxRationOffset = 242;
            try
            {
                lock (mgs2Process)
                {
                    using (SimpleProcessProxy proxy = new SimpleProcessProxy(mgs2Process))
                    {
                        IntPtr ammoOffset = proxy.FollowPointer(new IntPtr(CurrentAmmoPtr + CurrentAmmoOffset), false);
                        proxy.SetMemoryAtPointer(IntPtr.Add(ammoOffset, maxRationOffset), BitConverter.GetBytes(0));
                    }
                }
                Console.WriteLine("Max rations set to 0");
            }
            catch(Exception e) 
            {
                Console.WriteLine($"Something went wrong when making damage permanent: {e}");
            }
            */
            //monitor current HP
            Task.Factory.StartNew(MonitorCurrentHealth);
        }

        private void MonitorCurrentHealth()
        {
            //TODO: works great, but we need separate checks for raiden & snake... it doesnt make sense to do tanker-plant
            //and have Raiden start with less health because Snake took damage on the tanker...
            while (true)
            {
                try
                {
                    SupportedCharacter currentCharacter = GetCurrentChara();
                    lock (mgs2Process)
                    {
                        using (SimpleProcessProxy proxy = new SimpleProcessProxy(mgs2Process))
                        {
                            IntPtr healthLocation = proxy.FollowPointer(new IntPtr(CurrentHealthPtr), false);
                            byte[] currentHealth = proxy.GetMemoryFromPointer(IntPtr.Add(healthLocation, CurrentHealthOffset), 2);
                            short currentHealthInt = BitConverter.ToInt16(currentHealth, 0);
                            if (DeathByBleeding)
                            {
                                //this is a workaround for now
                                if (currentHealthInt == 1)
                                {
                                    //wait 5 seconds, then kill the shit out of the player.
                                    Thread.Sleep(5000);
                                    proxy.SetMemoryAtPointer(IntPtr.Add(healthLocation, CurrentHealthOffset), BitConverter.GetBytes(0));
                                }
                            }
                            if (!Permadeath)
                            {
                                SnakeHealth = 200;
                                RaidenHealth = 200;
                            }
                            if (currentCharacter == SupportedCharacter.Snake)
                            {
                                if (currentHealthInt <= SnakeHealth)
                                {
                                    SnakeHealth = currentHealthInt;
                                }
                                else
                                {
                                    //if HP goes up, reset back down to last known value
                                    proxy.SetMemoryAtPointer(IntPtr.Add(healthLocation, CurrentHealthOffset), BitConverter.GetBytes(SnakeHealth));
                                }
                            }
                            else
                            {
                                if (currentHealthInt <= RaidenHealth)
                                {
                                    RaidenHealth = currentHealthInt;
                                }
                                else
                                {
                                    //if HP goes up, reset back down to last known value
                                    proxy.SetMemoryAtPointer(IntPtr.Add(healthLocation, CurrentHealthOffset), BitConverter.GetBytes(RaidenHealth));
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

        #region Disable Permadeath(NEEDS VERIFICATION)
        private void DisablePermadeath()
        {
            //Maybe we make this dependent on continue count, instead? hmmj
            Console.WriteLine("Disabling permadeath");
            Permadeath = false;
        }

        #endregion

        #region Extend Guard Statuses(NEEDS VERIFICATION)
        private void ExtendGuardStatuses()
        {
            Task.Factory.StartNew(ForceAlertTimer);
            Task.Factory.StartNew(ForceEvasionTimer);
            Task.Factory.StartNew(PersistCautionTimer);
        }

        private void ForceAlertTimer()
        {
            while (true)
            {
                try
                {
                    lock (mgs2Process)
                    {
                        using (SimpleProcessProxy spp = new SimpleProcessProxy(mgs2Process))
                        {
                            IntPtr alertTimer = IntPtr.Add(spp.FollowPointer(new IntPtr(MaxAlertTimer), false), MaxAlertTimerOffset);
                            short currentAlertTimer = BitConverter.ToInt16(spp.GetMemoryFromPointer(alertTimer, 2), 0);
                            if (currentAlertTimer == 1024)
                                spp.SetMemoryAtPointer(alertTimer, BitConverter.GetBytes(3072));
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to force alert timer: {e}");
                }
                finally { }
            }
        }

        private void ForceEvasionTimer()
        {
            while (true)
            {
                try
                {
                    lock (mgs2Process)
                    {
                        using (SimpleProcessProxy spp = new SimpleProcessProxy(mgs2Process))
                        {
                            IntPtr evasionTimer = spp.FollowPointer(new IntPtr(MaxEvasionTimerPtr1), false);
                            evasionTimer = spp.FollowPointer(IntPtr.Add(evasionTimer, MaxEvasionTimerPtr2), false);
                            evasionTimer = IntPtr.Add(evasionTimer, MaxEvasionTimerOffset);
                            /*evasionTimer = new IntPtr(evasionTimer.ToInt64() - mgs2Process.MainModule.BaseAddress.ToInt64());
                            evasionTimer = IntPtr.Add(evasionTimer, MaxEvasionTimerPtr2);
                            evasionTimer = spp.FollowPointer(evasionTimer, false);*/

                            short currentCautionTimer = BitConverter.ToInt16(spp.GetMemoryFromPointer(evasionTimer, 2), 0);
                            if (currentCautionTimer == 1200)
                                spp.SetMemoryAtPointer(evasionTimer, BitConverter.GetBytes(3600));
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to force evasion timer: {e}");
                }
                finally { }
            }
        }

        private void PersistCautionTimer()
        {
            while (true)
            {
                try
                {
                    lock (mgs2Process)
                    {
                        using (SimpleProcessProxy spp = new SimpleProcessProxy(mgs2Process))
                        {
                            IntPtr cautionTimer = IntPtr.Add(spp.FollowPointer(new IntPtr(MaxCautionTimer), false), MaxCautionTimerOffset);
                            short currentCautionTimer = BitConverter.ToInt16(spp.GetMemoryFromPointer(cautionTimer, 2), 0);
                            if (currentCautionTimer == 3600)
                                spp.SetMemoryAtPointer(cautionTimer, BitConverter.GetBytes(10800));
                        }
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine($"Failed to force caution timer: {e}");
                }
                finally { }
            }
        }
        #endregion

        #region Disable Pausing
        private void DisablePauses()
        {
            //just disable all 3 sources of pausing
            try
            {
                lock (mgs2Process)
                {
                    using (SimpleProcessProxy spp = new SimpleProcessProxy(mgs2Process))
                    {
                        spp.ModifyProcessOffset(new IntPtr(PauseButtonLocation), NopArray(PauseButtonLength), true);
                        spp.ModifyProcessOffset(new IntPtr(ItemPauseLocation), NopArray(ItemPauseLength), true);
                        spp.ModifyProcessOffset(new IntPtr(WeaponPauseLocation), NopArray(WeaponPauseLength), true);
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

        #region Disable Quick Reload(BUGGY)
        private void DisableQuickReload()
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
            Task.Factory.StartNew(MonitorMagazines);
        }

        private void MonitorMagazines()
        {
            //TODO: so now, the only thing that is undesirable is if you equip a weapon with no clip(setting in-memory mag count to 0)
            //and then equip a weapon with a clip, that weapon will always have its mag count set to 0. need to figure out some logic
            //to make that work better-er, but the rest of it is working fairly well.
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
                                Console.WriteLine("Current stage is not matching last stage");
                                if (!ReloadDisabled)
                                {
                                    Console.WriteLine("Reloading now disabled");
                                    spp.ModifyProcessOffset(new IntPtr(ReloadMagLocation), NopArray(ReloadMagLength), true);
                                    ReloadDisabled = true;
                                }
                                Thread.Sleep(200);
                                continue;
                            }
                            IntPtr currentWeaponLocation = spp.FollowPointer(new IntPtr(CurrentWeaponPtr), false);
                            currentWeaponLocation = IntPtr.Add(currentWeaponLocation, CurrentWeaponOffset);
                            byte[] equippedWeapon = spp.GetMemoryFromPointer(currentWeaponLocation, 2);
                            short equippedWeaponInt = BitConverter.ToInt16(equippedWeapon, 0);
                            //Check to see if this is a magged weapon - if not, just note it and disable reloading
                            if (!WeaponsWithMags.Contains(equippedWeaponInt))
                            {
                                Console.WriteLine("Currently equipped weapon has no mag");
                                if (!ReloadDisabled)
                                {
                                    Console.WriteLine("Reloading now disabled");
                                    spp.ModifyProcessOffset(new IntPtr(ReloadMagLocation), NopArray(ReloadMagLength), true);
                                    ReloadDisabled = true;
                                }
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
                                    spp.ModifyProcessOffset(new IntPtr(CurrentMagazineHardcode), WeaponsWithMagsDict[equippedWeaponInt], true);
                                    lastEquippedWeapon = equippedWeaponInt;
                                    Thread.Sleep(150);
                                    continue;
                                }
                            }

                            //Enable reloading if we have a magged weapon equipped and it is not a newly equipped item, and we're not in a new zone
                            if (ReloadDisabled)
                            {
                                Console.WriteLine("Reloading now enabled");
                                spp.ModifyProcessOffset(new IntPtr(ReloadMagLocation), ReloadMagBytes, true);
                                ReloadDisabled = false;
                            }

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
                                    Console.WriteLine("Updating mag count");
                                    WeaponsWithMagsDict[equippedWeaponInt] = bulletsInMagInt;
                                }
                                //otherwise, set the bullets in mag count to last known count
                                else
                                {
                                    Console.WriteLine("Setting mag count");
                                    spp.ModifyProcessOffset(new IntPtr(CurrentMagazineHardcode), WeaponsWithMagsDict[equippedWeaponInt], true);
                                }
                            }
                            //otherwise, start tracking this new weapon
                            else
                            {
                                Console.WriteLine("Adding new gun to dict");
                                WeaponsWithMagsDict.Add(equippedWeaponInt, bulletsInMagInt);
                            }
                            lastEquippedWeapon = equippedWeaponInt;
                            Thread.Sleep(50);
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
