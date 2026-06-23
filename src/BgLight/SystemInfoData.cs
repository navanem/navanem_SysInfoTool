using System.Collections.Generic;

namespace BgLight
{
    public class SystemInfoData
    {
        public string ComputerName { get; set; } = "N/A";
        public string User { get; set; } = "N/A";
        public string Manufacturer { get; set; } = "N/A";
        public string Model { get; set; } = "N/A";
        public string Cpu { get; set; } = "N/A";
        public string SerialNumber { get; set; } = "N/A";
        public string AssetTag { get; set; } = "N/A";
        public string IPv4 { get; set; } = "N/A";
        public string OsVersion { get; set; } = "N/A";
        public string Uptime { get; set; } = "N/A";
        public string RamTotal { get; set; } = "N/A";
        public string RamUsed { get; set; } = "N/A";
        public string DiskTotal { get; set; } = "N/A";
        public string DiskFree { get; set; } = "N/A";
        public string DomainOrWorkgroup { get; set; } = "N/A";
        public string Fqdn { get; set; } = "N/A";
        public string Mac { get; set; } = "N/A";
        public string Dhcp { get; set; } = "N/A";
        public string Dns { get; set; } = "N/A";
        public string BitLocker { get; set; } = "N/A";
        public string Activation { get; set; } = "N/A";
        public string Antivirus { get; set; } = "N/A";
        public string Battery { get; set; } = "";
        public string GeneratedAt { get; set; } = "N/A";

        public IList<(string Label, string Value)> Disks { get; } = new List<(string, string)>();

        public string Title
        {
            get { return ComputerName; }
        }

        public IList<(string Title, IList<(string Label, string Value)> Rows)> Sections()
        {
            var system = new List<(string, string)>
            {
                ("User", User),
                ("OS", OsVersion),
                ("Uptime", Uptime),
            };
            if (!string.IsNullOrEmpty(Battery))
            {
                system.Add(("Battery", Battery));
            }
            system.Add(("Generated", GeneratedAt));

            var hardware = new List<(string, string)>
            {
                ("Manufacturer", Manufacturer),
                ("Model", Model),
                ("Processor", Cpu),
                ("Serial No.", SerialNumber),
                ("Asset tag", AssetTag),
                ("RAM", RamUsed + " / " + RamTotal),
            };

            var network = new List<(string, string)>
            {
                ("Domain", DomainOrWorkgroup),
                ("FQDN", Fqdn),
                ("IPv4", IPv4),
                ("MAC", Mac),
                ("DHCP", Dhcp),
                ("DNS", Dns),
            };

            var storage = new List<(string, string)>();
            if (Disks.Count == 0)
            {
                storage.Add(("C:", "N/A"));
            }
            else
            {
                foreach (var d in Disks)
                {
                    storage.Add((d.Label, d.Value));
                }
            }

            var security = new List<(string, string)>
            {
                ("BitLocker", BitLocker),
                ("Activation", Activation),
                ("Antivirus", Antivirus),
            };

            return new List<(string, IList<(string, string)>)>
            {
                ("SYSTEM", system),
                ("HARDWARE", hardware),
                ("NETWORK", network),
                ("STORAGE", storage),
                ("SECURITY", security),
            };
        }
    }
}
