using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAVD.Core;

namespace YAVD.ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MainSettingsModel mainSettings = MainSettingsMethods.LoadSettings();

            Console.WriteLine(mainSettings.YouTubeApiKey);
            Console.ReadLine();
        }
    }
}
