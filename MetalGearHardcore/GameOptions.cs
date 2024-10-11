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
        public string GameLocation;
        public bool BleedingKills;
        public bool Permadeath;
        public bool DisablePausing;
        public bool DisableQuickReload;
        public bool ExtendGuardStatuses;
        public bool PermanentDamage;

        public GameOptions(IniData iniData)
        {
            GameLocation = iniData["Global"].GetKeyData("GameLocation").Value;
            if (GameLocation.Contains("\""))
            {
                GameLocation = GameLocation.Replace("\"", "");
            }
            BleedingKills = bool.Parse(iniData["Global"].GetKeyData("BleedingKills").Value);
            Permadeath = bool.Parse(iniData["Global"].GetKeyData("Permadeath").Value);
            DisablePausing = bool.Parse(iniData["Global"].GetKeyData("DisablePausing").Value);
            DisableQuickReload = bool.Parse(iniData["Global"].GetKeyData("DisableQuickReload").Value);
            ExtendGuardStatuses = bool.Parse(iniData["Global"].GetKeyData("ExtendGuardStatuses").Value);
            PermanentDamage = bool.Parse(iniData["Global"].GetKeyData("PermanentDamage").Value);
        }
    }
}
