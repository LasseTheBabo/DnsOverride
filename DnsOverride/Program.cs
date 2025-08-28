using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using System.ServiceProcess;

namespace DnsOverride
{
    internal static class Program
    {
        private const string ServiceName = "DnsOverride";

        static void Main()
        {
            if (Environment.UserInteractive)
            {
                if (!IsUserAdministrator())
                {
                    RestartAsAdmin();
                    Environment.Exit(0);
                }

                if (!ServiceExists(ServiceName))
                {
                    string exePath = Process.GetCurrentProcess().MainModule.FileName;

                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "sc",
                        Arguments = $"create {ServiceName} binPath= \"{exePath}\" start= auto",
                        Verb = "runas",
                        UseShellExecute = true
                    };

                    Process.Start(psi)?.WaitForExit();

                    Process.Start("sc", $"start {ServiceName}");
                }
            }
            else
            {
                ServiceBase.Run(new Service1());
            }
        }

        private static bool ServiceExists(string serviceName)
        {
            ServiceController[] services = ServiceController.GetServices();
            foreach (var s in services)
            {
                if (s.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static void InstallService()
        {
            
        }

        public static bool IsUserAdministrator()
        {
            try
            {
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        public static void RestartAsAdmin()
        {
            var exeName = Process.GetCurrentProcess().MainModule.FileName;
            var startInfo = new ProcessStartInfo(exeName) { Verb = "runas" };

            try
            {
                Process.Start(startInfo);
            }
            catch (Win32Exception)
            {
                return;
            }

            Environment.Exit(0);
        }
    }
}
