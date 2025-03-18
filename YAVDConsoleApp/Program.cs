using System;
using YAVDConsoleApp.Methods;
using YAVDCore;

namespace YAVDConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            bool showMenu = true;

            while (showMenu)
            {
                showMenu = Menu.Show();
            }

        }
    }
}