using YAVD.ConsoleApp.Actions;
using YAVD.ConsoleApp.Menus;
using YAVD.Core.Helpers;

var dbCheck = SystemValidator.CheckDatabase();
if (!dbCheck.IsSuccess)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(dbCheck.Message);
    Console.ResetColor();
    ChannelActions.WaitForKey();
    return;
}

var ffmpegCheck = SystemValidator.CheckFFmpeg();
if (!ffmpegCheck.IsSuccess)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine(ffmpegCheck.Message);
    Console.ResetColor();
    ChannelActions.WaitForKey();
    return;
}

while (true)
{
    Console.Clear();
    Console.WriteLine("=== YAVD: Youtube Audio Video Downloader ===");
    Console.WriteLine("1. Direkt Video/Ses İndir");
    Console.WriteLine("2. Kanallar");
    Console.WriteLine("0. Çıkış");
    Console.Write("\nSeçiminiz: ");

    string choice = Console.ReadLine();

    if (choice == "1") await MenuManager.DirectDownloadMenu();
    else if (choice == "2") await MenuManager.ChannelsMenu();
    else if (choice == "0") return;
}