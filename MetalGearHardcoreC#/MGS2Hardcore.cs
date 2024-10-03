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
        private const string CompatibleGameVersion = "2.0.0.0";

        public MGS2Hardcore()
        {
            GameOptions gameOptions = IniHandler.ParseIniFile();

            string filePath = "C:/Program Files (x86)/Steam/steamapps/common/MGS2/METAL GEAR SOLID2.exe"; //TODO: obviously remove this hardcode
            //string filePath = Path.GetFullPath(gameOptions.GameLocation);
            //TODO: make the auto-launcher work correctly
            /*Process.Start(gameOptions.GameLocation);*/
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(filePath);
            if(versionInfo.ProductVersion != CompatibleGameVersion)
            {
                MessageBox.Show("MGS2 Hardcore is only compatible with the Master Collection MGS2, version 2.0.0.0 on Steam");
                return;
            }
            while (mgs2Process == null)
            {
                mgs2Process = GetProcess();
            }
            if(gameOptions.BleedingKills)
                BleedingKills();
            if (gameOptions.PermanentDamage)
                PermanentDamage();
            if (gameOptions.DisableContinues)
                DisableContinues();
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

        private Process GetProcess()
        {
            return Process.GetProcessesByName("METAL GEAR SOLID2").FirstOrDefault();
        }

        private SupportedCharacter GetCurrentChara()
        {
            lock (mgs2Process)
            {
                using(SimpleProcessProxy spp = new SimpleProcessProxy(mgs2Process))
                {
                    IntPtr characterLocation = IntPtr.Add(spp.FollowPointer(new IntPtr(CurrentCharacterPtr), false), CurrentCharacterOffset);
                    byte[] characterBytes = spp.GetMemoryFromPointer(characterLocation, 10);

                    string character = Encoding.UTF8.GetString(characterBytes).Trim();
                    

                    if (character.Contains("r_tnk"))
                    {
                        return SupportedCharacter.Snake;
                    }
                    if (character.Contains("r_plt"))
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
            for(int i = 0; i < num; i++)
            {
                array[i] = 0x90;
            }
            return array;
        }

        #region Bleeding Kills(INCOMPLETE)
        private void BleedingKills()
        {
            try
            {
                throw new NotImplementedException();
            }
            catch
            {

            }
        }
        #endregion

        #region Permanent Damage(NEEDS VERIFICATION)
        private void PermanentDamage()
        {
            //TODO: needs verification
            int maxRationOffset = 242;
            //this part _might_ need to be in a loop, not sure.
            lock (mgs2Process)
            {
                using (SimpleProcessProxy proxy = new SimpleProcessProxy(mgs2Process))
                {
                    IntPtr ammoOffset = proxy.FollowPointer(new IntPtr(CurrentAmmoPtr + CurrentAmmoOffset), false);
                    proxy.SetMemoryAtPointer(IntPtr.Add(ammoOffset, maxRationOffset), BitConverter.GetBytes(0));
                }
            }

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
                    Thread.Sleep(50);
                }
                catch(Exception e)
                {
                    int a = 2 + 2;
                }
            }
            
        }
        #endregion

        #region Disable Continues(INCOMPLETE)
        private void DisableContinues()
        {
            //needs additional research
            //can we just make it so the continue button doesnt appear?
            try
            {
                throw new NotImplementedException();
            }
            catch
            {

            }
        }

        #endregion

        #region Extend Guard Statuses(INCOMPLETE)
        private void ExtendGuardStatuses()
        {
            //needs additional research
            //need to extend Alert
            //need to extend Evasion
            //need to extend Caution
            try
            {
                throw new NotImplementedException();
            }
            catch
            {
                
            }
        }
        #endregion

        #region Disable Pausing
        private void DisablePauses()
        {
            //just disable all 3 sources of pausing
            lock(mgs2Process)
            {
                using(SimpleProcessProxy spp = new SimpleProcessProxy(mgs2Process))
                {
                    spp.ModifyProcessOffset(new IntPtr(PauseButtonLocation), NopArray(PauseButtonLength), true);
                    spp.ModifyProcessOffset(new IntPtr(ItemPauseLocation), NopArray(ItemPauseLength), true);
                    spp.ModifyProcessOffset(new IntPtr(WeaponPauseLocation), NopArray(WeaponPauseLength), true);
                }
            }
            //could disable codec pausing too, but that can cause crashes iirc
        }
        #endregion

        #region Disable Quick Reload(BUGGY)
        private void DisableQuickReload()
        {
            lock (mgs2Process)
            {
                using (SimpleProcessProxy spp = new SimpleProcessProxy(mgs2Process))
                {
                    spp.ModifyProcessOffset(new IntPtr(WeaponSwitchReload1Location), NopArray(WeaponSwitchReload1Length), true);
                    spp.ModifyProcessOffset(new IntPtr(WeaponSwitchReload2Location), NopArray(WeaponSwitchReload2Length), true);
                }
            }
            Task.Factory.StartNew(MonitorMagazines);
        }

        private void MonitorMagazines()
        {
            //TODO: this is kinda jank, but it is working (seemingly) consistently in all cases except when you
            //load into a new area with an empty gun - no matter what i do it is just auto-reloading the damn thing.
            short lastEquippedWeapon = 0;
            byte[] lastStage = new byte[4];
            while (true)
            {
                /*
                 * If you have no weapon equipped, your current mag will represent the last known mag size of the last equipped weapon
                 * If you have a weapon with mag size equipped, your current mag will represent the actual current mag size
                 * If you have a weapon with NO mag size equipped, your current mag will always be 0.
                 */
                try
                {
                    lock (mgs2Process)
                    {
                        using (SimpleProcessProxy spp = new SimpleProcessProxy(mgs2Process))
                        {
                            IntPtr currentStageLocation = spp.FollowPointer(new IntPtr(CurrentStagePtr), false);
                            currentStageLocation = IntPtr.Add(currentStageLocation, CurrentStageOffset);
                            byte[] currentStage = spp.GetMemoryFromPointer(currentStageLocation, 4);
                            if(!lastStage.SequenceEqual(currentStage))
                            {
                                spp.ModifyProcessOffset(new IntPtr(ReloadMagLocation), NopArray(ReloadMagLength), true);
                                ReloadDisabled = true;
                                lastStage = currentStage;
                                Thread.Sleep(200);
                                continue;
                            }
                            IntPtr currentWeaponLocation = spp.FollowPointer(new IntPtr(CurrentWeaponPtr), false);
                            currentWeaponLocation = IntPtr.Add(currentWeaponLocation, CurrentWeaponOffset);
                            byte[] equippedWeapon = spp.GetMemoryFromPointer(currentWeaponLocation, 2);
                            short equippedWeaponInt = BitConverter.ToInt16(equippedWeapon, 0);
                            if (!WeaponsWithMags.Contains(equippedWeaponInt))
                            {
                                spp.ModifyProcessOffset(new IntPtr(ReloadMagLocation), NopArray(ReloadMagLength), true);
                                ReloadDisabled = true;
                                lastEquippedWeapon = equippedWeaponInt;
                                continue;
                            }

                            if(lastEquippedWeapon != equippedWeaponInt) 
                            {
                                if (WeaponsWithMagsDict.ContainsKey(equippedWeaponInt))
                                {
                                    spp.ModifyProcessOffset(new IntPtr(CurrentMagazineHardcode), WeaponsWithMagsDict[equippedWeaponInt], true);
                                    lastEquippedWeapon = equippedWeaponInt;
                                    Thread.Sleep(150);
                                    continue;
                                }
                            }

                            spp.ModifyProcessOffset(new IntPtr(ReloadMagLocation), ReloadMagBytes, true);
                            ReloadDisabled = false;

                            byte[] bulletsInMag = spp.ReadProcessOffset(new IntPtr(CurrentMagazineHardcode), 2);
                            short bulletsInMagInt = BitConverter.ToInt16(bulletsInMag, 0);
                            if (WeaponsWithMagsDict.ContainsKey(equippedWeaponInt))
                            {
                                if((bulletsInMagInt > WeaponsWithMagsDict[equippedWeaponInt] && WeaponsWithMagsDict[equippedWeaponInt] == 0)||
                                    bulletsInMagInt < WeaponsWithMagsDict[equippedWeaponInt])
                                    WeaponsWithMagsDict[equippedWeaponInt] = bulletsInMagInt;
                                else
                                {
                                    spp.ModifyProcessOffset(new IntPtr(CurrentMagazineHardcode), WeaponsWithMagsDict[equippedWeaponInt], true);
                                }
                            }
                            else
                            {
                                WeaponsWithMagsDict.Add(equippedWeaponInt, bulletsInMagInt);
                            }
                            lastEquippedWeapon = equippedWeaponInt;
                            Thread.Sleep(50);
                        }
                    }
                }
                finally
                {
                    
                }
            }
        }
        #endregion
    }
}
