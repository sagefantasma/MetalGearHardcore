using IniParser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetalGearHardcore
{
    public class GameOptions
    {
        public bool ModEnabled;
        public bool BleedingKills;
        public bool Permadeath;
        public bool DisablePausing;
        public bool DisableQuickReload;
        public bool ExtendGuardStatuses;
        public bool PermanentDamage;

        public GameOptions(IniData iniData)
        {
            ModEnabled = bool.Parse(iniData.GetKey("DeactivateHardcoreMod"));
            BleedingKills = bool.Parse(iniData.GetKey("BleedingKills"));
            Permadeath = bool.Parse(iniData.GetKey("Permadeath"));
            DisablePausing = bool.Parse(iniData.GetKey("DisablePausing"));
            DisableQuickReload = bool.Parse(iniData.GetKey("DisableQuickReload"));
            ExtendGuardStatuses = bool.Parse(iniData.GetKey("ExtendGuardStatuses"));
            PermanentDamage = bool.Parse(iniData.GetKey("PermanentDamage"));
        }
    }
}
