using System;
using YAVDCore;


namespace YAVDConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            YAVD y = new YAVD();

            /*
            if (y.ApiKey.Items.Count > 0)
            {
                string videoLink = @"https://www.youtube.com/watch?v=1pEZAo_imwc";

                if (y.Channels.Methods.SaveChannelFromVideoLink(videoLink, y.ApiKey.Items[0]))
                    y.Channels.RefreshChannels();
            }
            */
            Console.WriteLine("Kanal Listesi {0}", y.Channels.Items.Count);
            foreach (var i in y.Channels.Items)
            {
                Console.WriteLine(i.Title);
                i.CheckVideos();
            }

            Console.WriteLine("");

            Console.WriteLine("Bitti");
            Console.ReadLine();
        }
    }
}