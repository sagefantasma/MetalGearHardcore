using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IniParser.Parser;
using IniParser.Model;

namespace MetalGearHardcore
{
    internal class IniHandler
    {
        public IniHandler() 
        {
            
        }

        public static GameOptions ParseIniFile()
        {
            IniDataParser iniParser = new IniDataParser();

            IniData parsedData = iniParser.Parse(File.ReadAllText("MGS2HardcoreConfig.ini"));

            return new GameOptions(parsedData);
        }
    }
}
