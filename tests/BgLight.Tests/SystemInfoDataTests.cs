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
        public void ToLines_returns_one_labelled_line_per_field()
        {
            var d = new SystemInfoData
            {
                ComputerName = "PC01",
                User = "DOM\\bob",
                IPv4 = "10.0.0.5",
                OsVersion = "Windows 11 (22631)",
                RamTotal = "64.0 Go",
                RamUsed = "15.8 Go",
                DiskTotal = "930.0 Go",
                DiskFree = "400.0 Go",
                DomainOrWorkgroup = "DOM",
                GeneratedAt = "2026-06-22 10:00:00"
            };

            var lines = d.ToLines();

            Assert.Equal(9, lines.Count);
            Assert.Contains(lines, l => l.Contains("PC01"));
            Assert.Contains(lines, l => l.Contains("10.0.0.5"));
            Assert.Contains(lines, l => l.Contains("15.8 Go") && l.Contains("64.0 Go"));
            Assert.Contains(lines, l => l.Contains("2026-06-22 10:00:00"));
        }
    }
}
