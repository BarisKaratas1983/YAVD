using YAVD.ConsoleApp.Menus;

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