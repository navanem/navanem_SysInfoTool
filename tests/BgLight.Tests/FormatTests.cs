using System;
using Xunit;

namespace BgLight.Tests
{
    public class FormatTests
    {
        [Fact]
        public void Giga_converts_bytes_to_gigabytes_one_decimal()
        {
            // 64 Go = 64 * 1024^3 octets
            ulong bytes = 64UL * 1024 * 1024 * 1024;
            Assert.Equal("64.0 Go", Format.Giga(bytes));
        }

        [Fact]
        public void Giga_rounds_to_one_decimal()
        {
            ulong bytes = (ulong)(1.58 * 1024 * 1024 * 1024);
            Assert.Equal("1.6 Go", Format.Giga(bytes));
        }

        [Fact]
        public void Join_uses_comma_separator()
        {
            Assert.Equal("10.0.0.1, 10.0.0.2", Format.Join(new[] { "10.0.0.1", "10.0.0.2" }));
        }

        [Fact]
        public void Join_returns_NA_when_empty()
        {
            Assert.Equal("N/A", Format.Join(Array.Empty<string>()));
        }

        [Fact]
        public void Join_filters_empty_and_whitespace_values()
        {
            Assert.Equal("10.0.0.1", Format.Join(new[] { "", "  ", "10.0.0.1" }));
        }
    }
}
