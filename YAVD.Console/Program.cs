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
            var knl = YouTubeMethods.GetYouTubeChannelFromVideoUrl("https://www.youtube.com/watch?v=T4eMk7uhlhQ");
            DatabaseMethods.InsertOrReplaceYouTubeChannel(knl);

            var vid = YouTubeMethods.GetYouTubeVideos(knl.Id, null);
            DatabaseMethods.InsertOrReplaceYouTubeVideo(vid);

            var kanallar = DatabaseMethods.GetYouTubeChannels();

            foreach (var kanal in kanallar)
            {
                Console.WriteLine("Kanal : {0}", kanal.Title);
                var videolar = DatabaseMethods.GetYouTubeVideos(kanal.Id);

                foreach (var video in videolar)
                {
                    Console.WriteLine("Video : {0} (https:" + "//www.youtube.com/watch?v={1})", video.Title, video.Id);
                }
            }

            Console.WriteLine("Bitti");
            Console.ReadLine();
        }
    }
}