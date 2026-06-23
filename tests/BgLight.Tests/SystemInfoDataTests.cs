using System.Linq;
using Xunit;

namespace BgLight.Tests
{
    public class SystemInfoDataTests
    {
        [Fact]
        public void Defaults_are_NA()
        {
            var d = new SystemInfoData();
            Assert.Equal("N/A", d.ComputerName);
            Assert.Equal("N/A", d.User);
            Assert.Equal("N/A", d.Cpu);
            Assert.Equal("N/A", d.SerialNumber);
            Assert.Equal("N/A", d.IPv4);
            Assert.Equal("N/A", d.OsVersion);
            Assert.Equal("N/A", d.RamTotal);
            Assert.Equal("N/A", d.RamUsed);
            Assert.Equal("N/A", d.DiskTotal);
            Assert.Equal("N/A", d.DiskFree);
            Assert.Equal("N/A", d.DomainOrWorkgroup);
            Assert.Equal("N/A", d.GeneratedAt);
        }

        [Fact]
        public void Title_is_computer_name()
        {
            var d = new SystemInfoData { ComputerName = "PC01" };
            Assert.Equal("PC01", d.Title);
        }

        [Fact]
        public void Rows_returns_ordered_label_value_pairs()
        {
            var d = new SystemInfoData
            {
                ComputerName = "PC01",
                User = "DOM\\bob",
                Cpu = "Intel Core i7-1185G7",
                SerialNumber = "5CD1234XYZ",
                IPv4 = "10.0.0.5",
                OsVersion = "Windows 11 (22631)",
                RamTotal = "64.0 Go",
                RamUsed = "15.8 Go",
                DiskTotal = "930.0 Go",
                DiskFree = "400.0 Go",
                DomainOrWorkgroup = "DOM",
                GeneratedAt = "2026-06-22 10:00:00"
            };

            var rows = d.Rows();

            Assert.Equal(9, rows.Count);
            Assert.Equal("Utilisateur", rows[0].Label);
            Assert.Equal("Processeur", rows[1].Label);
            Assert.Equal("Intel Core i7-1185G7", rows[1].Value);
            Assert.Equal("N° série", rows[2].Label);
            Assert.Equal("5CD1234XYZ", rows[2].Value);
            Assert.Contains(rows, r => r.Label == "IPv4" && r.Value == "10.0.0.5");
            Assert.Contains(rows, r => r.Label == "RAM" && r.Value.Contains("15.8 Go") && r.Value.Contains("64.0 Go"));
            Assert.Contains(rows, r => r.Label == "Disque (C:)" && r.Value.Contains("400.0 Go") && r.Value.Contains("930.0 Go"));
            Assert.Contains(rows, r => r.Label == "Généré le" && r.Value == "2026-06-22 10:00:00");
        }
    }
}
