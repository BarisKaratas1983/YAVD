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

await MainMenu.Show();