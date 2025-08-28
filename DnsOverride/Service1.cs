using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace DnsOverride
{
    public partial class Service1 : ServiceBase
    {
        private Timer timer;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Log("DnsOverride gestartet.");

            timer = new Timer(60 * 1000);
            timer.Elapsed += ((sernder, e) => SetDns());
            timer.AutoReset = true;
            timer.Start();

            SetDns();
        }

        protected override void OnStop()
        {
            Log("DnsOverride gestoppt.");
        }

        public void SetDns()
        {
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

        private static void Log(string message)
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
