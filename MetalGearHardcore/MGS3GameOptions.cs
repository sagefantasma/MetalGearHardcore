using IniParser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetalGearHardcore
{
    public class MGS3GameOptions
    {
        public string GameLocation;
        public bool DoubleDamage;
        public bool Permadeath;
        public bool DisablePausing;
        public bool DisableQuickReload;
        public bool ExtendGuardStatuses;
        public bool PermanentDamage;

        public MGS3GameOptions(IniData iniData)
        {
            GameLocation = iniData["Global"].GetKeyData("GameLocation").Value;
            if (GameLocation.Contains("\""))
            {
                GameLocation = GameLocation.Replace("\"", "");
            }
            DoubleDamage = bool.Parse(iniData["Global"].GetKeyData("DoubleDamage").Value);
            Permadeath = bool.Parse(iniData["Global"].GetKeyData("Permadeath").Value);
            DisablePausing = bool.Parse(iniData["Global"].GetKeyData("DisablePausing").Value);
            DisableQuickReload = bool.Parse(iniData["Global"].GetKeyData("DisableQuickReload").Value);
            ExtendGuardStatuses = bool.Parse(iniData["Global"].GetKeyData("ExtendGuardStatuses").Value);
            PermanentDamage = bool.Parse(iniData["Global"].GetKeyData("PermanentDamage").Value);
        }
    }
}
