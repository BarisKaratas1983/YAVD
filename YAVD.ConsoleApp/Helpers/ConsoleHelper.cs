using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAVD.ConsoleApp.Helpers
{
    public static class ConsoleHelper
    {
        public static void WaitForKey()
        {
            Console.WriteLine("\nDevam etmek için bir tuşa basın...");
            Console.ReadKey();
        }
    }
}
