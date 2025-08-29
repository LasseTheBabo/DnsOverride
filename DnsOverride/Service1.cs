using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace DnsOverride
{
    public partial class Service1 : ServiceBase
    {
        private Timer timer;
        string hosts = Path.Combine(Environment.SystemDirectory, @"drivers\etc\hosts");
        string url = "https://raw.githubusercontent.com/LasseTheBabo/files/master/hosts";

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Log("DnsOverride gestartet.");

            timer = new Timer(60 * 1000);
            timer.Elapsed += async (sender, e) => await SetDns();

            timer.AutoReset = true;
            timer.Start();

            Task.Run(() => SetDns());
        }

        protected override void OnStop()
        {
            Log("DnsOverride gestoppt.");
        }

        public async Task SetDns()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string content = await client.GetStringAsync(url);
                    File.WriteAllText(hosts, content);
                }
                catch { }
            }

            try
            {
                string[] dns = { "1.1.1.3" };

                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();

                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"])
                    {
                        ManagementBaseObject newDNS =
                            mo.GetMethodParameters("SetDNSServerSearchOrder");

                        newDNS["DNSServerSearchOrder"] = dns;

                        ManagementBaseObject setDNS =
                            mo.InvokeMethod("SetDNSServerSearchOrder", newDNS, null);

                        Log("DNS geändert für Adapter: " + mo["Description"]);
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Fehler beim Setzen des DNS: " + ex.Message);
            }
        }

        internal static void Log(string message)
        {
            try
            {
                if (!EventLog.SourceExists("DnsOverride"))
                {
                    EventLog.CreateEventSource("DnsOverride", "Application");
                }
                EventLog.WriteEntry("DnsOverride", message);
            }
            catch
            {

            }
        }
    }
}
