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
            /*
            var yt = YouTubeMethods.GetYouTubeChannelFromHandle("TheNatureReveals");
            //var yt = YouTubeMethods.GetYouTubeChannelFromVideoUrl("https://www.youtube.com/shorts/5YCMBPS1R7Y?feature=share");
            
            if (yt != null)
            {
                Console.WriteLine("Channel Id : {0}", yt.Id);
                Console.WriteLine("Channel Title : {0}", yt.Title);
                Console.WriteLine("Channel Description : {0}", yt.Description);
                Console.WriteLine("Channel LastVideoDate : {0}", yt.LastVideoDate);
                
                if (DatabaseMethods.InsertOrReplaceYouTubeChannel(yt))
                    Console.WriteLine("Kayıt edildi");
            }
            */

            
            List<YouTubeChannelModel> knl = DatabaseMethods.GetYouTubeChannels("UCTlPoI5U7OJjd0795o7nThw");

            foreach (var item in knl)
            {
                Console.WriteLine("Channel Id : {0}", item.Id);
                Console.WriteLine("Channel Title : {0}", item.Title);
                //Console.WriteLine("Channel Description : {0}", item.Description);
                Console.WriteLine("Channel LastVideoDate : {0}", item.LastVideoDate);

                var arm = YouTubeMethods.CheckYouTubeChannelVideosWithDate(item.Id, item.LastVideoDate);
                Console.WriteLine("xxx : {0}", arm.LastVideoDate);
            }
            

            Console.WriteLine("Bitti");
            Console.ReadLine();
        }
    }
}
