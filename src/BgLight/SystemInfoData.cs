using System.Collections.Generic;

namespace BgLight
{
    public class SystemInfoData
    {
        public string ComputerName { get; set; } = "N/A";
        public string User { get; set; } = "N/A";
        public string Cpu { get; set; } = "N/A";
        public string SerialNumber { get; set; } = "N/A";
        public string IPv4 { get; set; } = "N/A";
        public string OsVersion { get; set; } = "N/A";
        public string RamTotal { get; set; } = "N/A";
        public string RamUsed { get; set; } = "N/A";
        public string DiskTotal { get; set; } = "N/A";
        public string DiskFree { get; set; } = "N/A";
        public string DomainOrWorkgroup { get; set; } = "N/A";
        public string GeneratedAt { get; set; } = "N/A";

        public string Title
        {
            get { return ComputerName; }
        }

        public IList<(string Label, string Value)> Rows()
        {
            return new List<(string Label, string Value)>
            {
                ("User", User),
                ("Processor", Cpu),
                ("Serial No.", SerialNumber),
                ("IPv4", IPv4),
                ("OS", OsVersion),
                ("RAM", RamUsed + " / " + RamTotal),
                ("Disk (C:)", DiskFree + " free / " + DiskTotal),
                ("Domain", DomainOrWorkgroup),
                ("Generated", GeneratedAt)
            };
        }
    }
}
