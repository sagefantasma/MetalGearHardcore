using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IniParser.Parser;
using IniParser.Model;
using IniParser;

namespace MetalGearHardcore
{
    public static class IniHandler
    {
        public static string IniFile = "MGS2HardcoreConfig.ini";

        public static GameOptions ParseIniFile()
        {
            FileIniDataParser iniParser = new FileIniDataParser();

            IniData parsedData = iniParser.ReadFile(IniFile);

            return new GameOptions(parsedData);
        }

        public static void UpdateIniFile(GameOptions gameOptions)
        {
            FileIniDataParser iniParser = new FileIniDataParser();

            IniData iniData = iniParser.ReadFile(IniFile);
            iniData["Global"]["GameLocation"] = gameOptions.GameLocation;
            iniData["Global"]["BleedingKills"] = gameOptions.BleedingKills.ToString();
            iniData["Global"]["Permadeath"] = gameOptions.Permadeath.ToString();
            iniData["Global"]["DisablePausing"] = gameOptions.DisablePausing.ToString();
            iniData["Global"]["DisableQuickReload"] = gameOptions.DisableQuickReload.ToString();
            iniData["Global"]["ExtendGuardStatuses"] = gameOptions.ExtendGuardStatuses.ToString();
            iniData["Global"]["PermanentDamage"] = gameOptions.PermanentDamage.ToString();

            iniParser.WriteFile(IniFile, iniData);
        }

        /*
         * GameLocation = "C:\Program Files (x86)\Steam\steamapps\common\MGS2\METAL GEAR SOLID2.exe"

; Feel free to toggle any of the options below on(true) or off(false) to customize your hardcore experience to your liking.
; If you get down to 1hp while bleeding, you will bleed out and die.
BleedingKills = true

; When your life reaches zero, the game is over. There are no continues, my friend.
Permadeath = false

; Disables the pause button, and the pausing that happens when you open the Item or Weapon menus. For game stability reasons, Codec pause still works.
DisablePausing = true

; No more weapon switching to reload, you must manually reload.
DisableQuickReload = true

; When you've been found by guards, this will extend the amount of time they're in Alert, Evasion, and Caution modes.
ExtendGuardStatuses = true

; Rations are a waste of good food. Defeating bosses will not replenish your health. Crouch/prone will not heal at low HP.
PermanentDamage = true
        */
    }
}
