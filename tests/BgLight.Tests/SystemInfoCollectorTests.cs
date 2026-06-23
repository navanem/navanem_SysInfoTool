using System;
using Xunit;

namespace BgLight.Tests
{
    public class SystemInfoCollectorTests
    {
        [Fact]
        public void Collect_never_throws_and_returns_data()
        {
            var data = SystemInfoCollector.Collect();
            Assert.NotNull(data);
        }

        [Fact]
        public void ComputerName_falls_back_to_machine_name()
        {
            var data = SystemInfoCollector.Collect();
            Assert.False(string.IsNullOrWhiteSpace(data.ComputerName));
            Assert.NotEqual("N/A", data.ComputerName);
        }

        [Fact]
        public void GeneratedAt_is_populated()
        {
            var data = SystemInfoCollector.Collect();
            Assert.NotEqual("N/A", data.GeneratedAt);
        }

        [Fact]
        public void User_is_populated()
        {
            var data = SystemInfoCollector.Collect();
            Assert.NotEqual("N/A", data.User);
            Assert.Contains("\\", data.User);
        }

        [Fact]
        public void Cpu_and_serial_are_never_null()
        {
            var data = SystemInfoCollector.Collect();
            Assert.False(string.IsNullOrEmpty(data.Cpu));
            Assert.False(string.IsNullOrEmpty(data.SerialNumber));
        }

        [Fact]
        public void New_fields_are_never_null()
        {
            var d = SystemInfoCollector.Collect();
            Assert.NotNull(d.Manufacturer);
            Assert.NotNull(d.Model);
            Assert.NotNull(d.AssetTag);
            Assert.NotNull(d.Uptime);
            Assert.NotNull(d.Mac);
            Assert.NotNull(d.Fqdn);
            Assert.NotNull(d.Dhcp);
            Assert.NotNull(d.Dns);
            Assert.NotNull(d.BitLocker);
            Assert.NotNull(d.Activation);
            Assert.NotNull(d.Antivirus);
            Assert.NotNull(d.Battery);
            Assert.NotNull(d.Disks);
        }
    }
}
