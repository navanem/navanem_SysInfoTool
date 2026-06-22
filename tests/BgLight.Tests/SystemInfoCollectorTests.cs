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
    }
}
