using System.Collections.Generic;

namespace BgLight
{
    public class SystemInfoData
    {
        public string ComputerName { get; set; } = "N/A";
        public string User { get; set; } = "N/A";
        public string IPv4 { get; set; } = "N/A";
        public string OsVersion { get; set; } = "N/A";
        public string RamTotal { get; set; } = "N/A";
        public string RamUsed { get; set; } = "N/A";
        public string DiskTotal { get; set; } = "N/A";
        public string DiskFree { get; set; } = "N/A";
        public string DomainOrWorkgroup { get; set; } = "N/A";
        public string GeneratedAt { get; set; } = "N/A";

        public IList<string> ToLines()
        {
            return new List<string>
            {
                "PC          : " + ComputerName,
                "Utilisateur : " + User,
                "IPv4        : " + IPv4,
                "OS          : " + OsVersion,
                "RAM         : " + RamUsed + " / " + RamTotal,
                "Disque C:   : " + DiskFree + " libre / " + DiskTotal,
                "Domaine     : " + DomainOrWorkgroup,
                "Genere le   : " + GeneratedAt,
                "" // ligne de respiration en bas du panneau
            };
        }
    }
}
