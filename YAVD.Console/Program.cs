using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAVD.Core.Methods;
using YAVD.Core.Models;

namespace YAVD.ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var youTubeChannel = YouTubeMethods.GetYouTubeChannel("muratgamsiz");
            
            Console.WriteLine(youTubeChannel.Description);
            Console.ReadLine();
        }
    }
}
