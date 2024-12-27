using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAVD.Core.Helpers;
using YAVD.Core.Methods;
using YAVD.Core.Models;

namespace YAVD.ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {

            var knl = YouTubeMethods.GetYouTubeChannelFromVideoUrl("https://www.youtube.com/watch?v=KcwYwTP5i00");           

            List<YouTubeVideoModel> vid = YouTubeMethods.GetYouTubeVideos(knl.Id, null);

            Console.WriteLine("Bitti");
            Console.ReadLine();
        }
    }
}
