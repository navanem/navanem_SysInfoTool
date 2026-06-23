using System.Linq;
using Xunit;

namespace BgLight.Tests
{
    public class SystemInfoDataTests
    {
        private static SystemInfoData Sample()
        {
            var d = new SystemInfoData
            {
                ComputerName = "PC01",
                User = "DOM\\bob",
                Manufacturer = "Dell Inc.",
                Model = "XPS 15",
                Cpu = "Intel Core i7",
                SerialNumber = "SER123",
                AssetTag = "IT-0042",
                IPv4 = "10.0.0.5",
                OsVersion = "Windows 11",
                Uptime = "3d 04:12",
                RamTotal = "64.0 GB",
                RamUsed = "15.8 GB",
                DomainOrWorkgroup = "DOM",
                Fqdn = "pc01.dom.local",
                Mac = "00:1A:2B:3C:4D:5E",
                Dhcp = "10.0.0.1",
                Dns = "10.0.0.1",
                BitLocker = "On (C:)",
                Activation = "Activated",
                Antivirus = "Defender (enabled, up to date)",
                GeneratedAt = "2026-06-23 10:00:00"
            };
            d.Disks.Add(("C:", "400.0 GB free / 930.0 GB"));
            return d;
        }

        [Fact]
        public void Defaults_are_correct()
        {
            var d = new SystemInfoData();
            Assert.Equal("N/A", d.ComputerName);
            Assert.Equal("N/A", d.Manufacturer);
            Assert.Equal("N/A", d.Model);
            Assert.Equal("N/A", d.AssetTag);
            Assert.Equal("N/A", d.Uptime);
            Assert.Equal("N/A", d.Mac);
            Assert.Equal("N/A", d.Fqdn);
            Assert.Equal("N/A", d.Dhcp);
            Assert.Equal("N/A", d.Dns);
            Assert.Equal("N/A", d.BitLocker);
            Assert.Equal("N/A", d.Activation);
            Assert.Equal("N/A", d.Antivirus);
            Assert.Equal("", d.Battery);
            Assert.Empty(d.Disks);
        }

        [Fact]
        public void Title_is_computer_name()
        {
            Assert.Equal("PC01", Sample().Title);
        }

        [Fact]
        public void Sections_have_expected_titles_in_order()
        {
            var titles = Sample().Sections().Select(s => s.Title).ToArray();
            Assert.Equal(new[] { "SYSTEM", "HARDWARE", "NETWORK", "STORAGE", "SECURITY" }, titles);
        }

        [Fact]
        public void Hardware_section_has_manufacturer_model_and_ram()
        {
            var hw = Sample().Sections().First(s => s.Title == "HARDWARE").Rows;
            Assert.Contains(hw, r => r.Label == "Manufacturer" && r.Value == "Dell Inc.");
            Assert.Contains(hw, r => r.Label == "Model" && r.Value == "XPS 15");
            Assert.Contains(hw, r => r.Label == "Asset tag" && r.Value == "IT-0042");
            Assert.Contains(hw, r => r.Label == "RAM" && r.Value.Contains("15.8 GB") && r.Value.Contains("64.0 GB"));
        }

        [Fact]
        public void Storage_section_uses_disks_list()
        {
            var st = Sample().Sections().First(s => s.Title == "STORAGE").Rows;
            Assert.Contains(st, r => r.Label == "C:" && r.Value.Contains("400.0 GB free"));
        }

        [Fact]
        public void Battery_omitted_when_empty_present_when_set()
        {
            var without = Sample().Sections().First(s => s.Title == "SYSTEM").Rows;
            Assert.DoesNotContain(without, r => r.Label == "Battery");

            var d = Sample();
            d.Battery = "85% (charging)";
            var with = d.Sections().First(s => s.Title == "SYSTEM").Rows;
            Assert.Contains(with, r => r.Label == "Battery" && r.Value == "85% (charging)");
        }
    }
}
