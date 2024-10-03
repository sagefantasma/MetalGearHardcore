using SimplifiedMemoryManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MetalGearHardcore
{
    public class MGS2Hardcore
    {
        Process mgs2Process;
        private static int CurrentHealth;
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
        private readonly byte[] ReloadMagBytes = new byte[] { 0x44, 0x89, 0x05, 0xC3, 0x78, 0x19, 0x01 };
        private bool ReloadDisabled = false;

        public MGS2Hardcore()
        {
            mgs2Process = GetProcess();
            //PermanentDamage();
            //DisableContinues();
            //ExtendGuardStatuses();
            DisablePauses();
            DisableQuickReload();
            while (true)
            {

            }
        }

        private Process GetProcess()
        {
            return Process.GetProcessesByName("METAL GEAR SOLID2").FirstOrDefault();
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

        #region Permanent Damage(NEEDS VERIFICATION)
        private void PermanentDamage()
        {
            //TODO: needs verification
            //set max rations to 0
            int rationOffset = 146;
            lock (mgs2Process)
            {
                using (SimpleProcessProxy proxy = new SimpleProcessProxy(mgs2Process))
                {
                    IntPtr ammoOffset = proxy.FollowPointer(new IntPtr(CurrentAmmoPtr + CurrentAmmoOffset), false);
                    proxy.SetMemoryAtPointer(IntPtr.Add(ammoOffset, rationOffset), BitConverter.GetBytes(0));
                }
            }

            //monitor current HP
            Task.Factory.StartNew(MonitorCurrentHealth);
        }

        private void MonitorCurrentHealth()
        {
            //TODO: needs verification
            lock (mgs2Process)
            {
                using (SimpleProcessProxy proxy = new SimpleProcessProxy(mgs2Process))
                {
                    IntPtr healthLocation = proxy.FollowPointer(new IntPtr(CurrentHealthPtr), false);
                    byte[] currentHealth = proxy.GetMemoryFromPointer(IntPtr.Add(healthLocation, CurrentHealthOffset), 2);
                    short currentHealthInt = BitConverter.ToInt16(currentHealth, 0);
                    if (currentHealthInt <= CurrentHealth)
                    {
                        CurrentHealth = currentHealthInt;
                    }
                    else
                    {
                        //if HP goes up, reset back down to last known value
                        proxy.SetMemoryAtPointer(IntPtr.Add(healthLocation, CurrentHealthOffset), BitConverter.GetBytes(CurrentHealth));
                    }
                }
            }
            
        }
        #endregion

        #region Disable Continues(INCOMPLETE)
        private void DisableContinues()
        {
            //needs additional research
            //can we just make it so the continue button doesnt appear?
        }

        #endregion

        #region Extend Guard Statuses(INCOMPLETE)
        private void ExtendGuardStatuses()
        {
            //needs additional research
            //need to extend Alert
            //need to extend Evasion
            //need to extend Caution
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

        #region Disable Quick Reload
        private void DisableQuickReload()
        {
            Task.Factory.StartNew(MonitorMagazines);
        }

        private void MonitorMagazines()
        {
            while (true)
            {
                lock (mgs2Process)
                {
                    using (SimpleProcessProxy spp = new SimpleProcessProxy(mgs2Process))
                    {
                        IntPtr currentWeaponLocation = spp.FollowPointer(new IntPtr(CurrentWeaponPtr), false);
                        currentWeaponLocation = IntPtr.Add(currentWeaponLocation, CurrentWeaponOffset);
                        byte[] equippedWeapon = spp.GetMemoryFromPointer(currentWeaponLocation, 2);
                        short equippedWeaponInt = BitConverter.ToInt16(equippedWeapon, 0);
                        if (equippedWeaponInt == 0)
                        {
                            //disable reloading while having no weapon equipped to avoid being able to "reverse" quick reload
                            if (!ReloadDisabled)
                            {
                                spp.ModifyProcessOffset(new IntPtr(ReloadMagLocation), NopArray(ReloadMagLength), true);
                                ReloadDisabled = true;
                                Thread.Sleep(250); //this small sleep is set to prevent spamming from working to "reverse" quick reload
                                continue;
                            }
                        }
                        
                        byte[] bulletsInMag = spp.ReadProcessOffset(new IntPtr(CurrentMagazineHardcode), 2);
                        short bulletsInMagInt = BitConverter.ToInt16(bulletsInMag, 0);

                        if(equippedWeaponInt != 0)
                        {
                            if (bulletsInMagInt == 0)
                            {
                                //enable reloading
                                if (ReloadDisabled)
                                {
                                    spp.ModifyProcessOffset(new IntPtr(ReloadMagLocation), ReloadMagBytes, true);
                                    ReloadDisabled = false;
                                }
                            }
                            else
                            {
                                //disable reloading
                                if (!ReloadDisabled)
                                {
                                    spp.ModifyProcessOffset(new IntPtr(ReloadMagLocation), NopArray(ReloadMagLength), true);
                                    ReloadDisabled = true;
                                }
                            }
                        }
                        
                    }
                }
                Thread.Sleep(50);
            }
        }
        #endregion
    }
}
