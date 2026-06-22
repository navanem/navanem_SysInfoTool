using System;
using System.IO;
using Xunit;

namespace BgLight.Tests
{
    public class LoggerTests
    {
        [Fact]
        public void Error_creates_file_and_writes_message()
        {
            var dir = Path.Combine(Path.GetTempPath(), "BgLightTest_" + Guid.NewGuid().ToString("N"));
            var logPath = Path.Combine(dir, "log.txt");
            try
            {
                var logger = new Logger(logPath);
                logger.Error("boom", new InvalidOperationException("details"));

                Assert.True(File.Exists(logPath));
                var content = File.ReadAllText(logPath);
                Assert.Contains("boom", content);
                Assert.Contains("details", content);
            }
            finally
            {
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }
        }

        [Fact]
        public void Error_appends_without_throwing_on_invalid_path()
        {
            var logger = new Logger("Z:\\does\\not\\exist\\nope\\log.txt");
            // ne doit pas lever
            logger.Error("ignored");
        }
    }
}
