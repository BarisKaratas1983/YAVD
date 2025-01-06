using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using YAVD.Core.Base;
using YAVD.Core.Helpers;
using YAVD.Core.Methods;
using YAVD.Core.Models;

namespace YAVD.ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            YAVDBase yavd = new YAVDBase();
            yavd.ApiKeys.GetApiKeys();

            if (yavd.ApiKeys.ApiKeys != null)
            {
                foreach (var item in yavd.ApiKeys.ApiKeys)
                {
                    Console.WriteLine(item.ApiKey);
                }
            }

            Console.WriteLine("Bitti");
            Console.ReadLine();
        }
    }
}