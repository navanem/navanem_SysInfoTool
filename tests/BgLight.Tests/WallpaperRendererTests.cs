using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Xunit;

namespace BgLight.Tests
{
    public class WallpaperRendererTests
    {
        private static AppConfig ConfigWithOutput(string path)
        {
            return AppConfig.Parse(new[] { "/outputPath=" + path });
        }

        [Fact]
        public void Render_creates_bmp_of_requested_size()
        {
            var dir = Path.Combine(Path.GetTempPath(), "BgLightTest_" + Guid.NewGuid().ToString("N"));
            var path = Path.Combine(dir, "wp.bmp");
            try
            {
                var data = new SystemInfoData { ComputerName = "PC01" };
                WallpaperRenderer.Render(data, ConfigWithOutput(path), 800, 600);

                Assert.True(File.Exists(path));
                using (var img = Image.FromFile(path))
                {
                    Assert.Equal(800, img.Width);
                    Assert.Equal(600, img.Height);
                    Assert.Equal(ImageFormat.Bmp, img.RawFormat);
                }
            }
            finally
            {
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }
        }

        [Fact]
        public void Render_creates_missing_output_directory()
        {
            var dir = Path.Combine(Path.GetTempPath(), "BgLightTest_" + Guid.NewGuid().ToString("N"), "deep");
            var path = Path.Combine(dir, "wp.bmp");
            try
            {
                WallpaperRenderer.Render(new SystemInfoData(), ConfigWithOutput(path), 320, 240);
                Assert.True(File.Exists(path));
            }
            finally
            {
                var root = Directory.GetParent(dir).FullName;
                if (Directory.Exists(root)) Directory.Delete(root, true);
            }
        }

        [Fact]
        public void Render_works_for_all_positions()
        {
            foreach (PanelPosition pos in Enum.GetValues(typeof(PanelPosition)))
            {
                var dir = Path.Combine(Path.GetTempPath(), "BgLightTest_" + Guid.NewGuid().ToString("N"));
                var path = Path.Combine(dir, "wp.bmp");
                try
                {
                    var config = AppConfig.Parse(new[] { "/outputPath=" + path, "/position=" + pos });
                    WallpaperRenderer.Render(new SystemInfoData { ComputerName = "PC01" }, config, 640, 480);
                    Assert.True(File.Exists(path));
                }
                finally
                {
                    if (Directory.Exists(dir)) Directory.Delete(dir, true);
                }
            }
        }

        [Fact]
        public void Render_with_full_data_creates_bmp()
        {
            var dir = Path.Combine(Path.GetTempPath(), "BgLightTest_" + Guid.NewGuid().ToString("N"));
            var path = Path.Combine(dir, "wp.bmp");
            try
            {
                var data = new SystemInfoData
                {
                    ComputerName = "DESKTOP-AV12",
                    User = "CORP\\jdupont",
                    Cpu = "Intel Core i7-1185G7",
                    SerialNumber = "5CD1234XYZ",
                    IPv4 = "10.0.2.47"
                };
                WallpaperRenderer.Render(data, ConfigWithOutput(path), 1920, 1080);

                Assert.True(File.Exists(path));
                using (var img = Image.FromFile(path))
                {
                    Assert.Equal(1920, img.Width);
                    Assert.Equal(1080, img.Height);
                }
            }
            finally
            {
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }
        }

        [Fact]
        public void Render_multi_monitor_creates_virtual_size_bmp()
        {
            var dir = Path.Combine(Path.GetTempPath(), "BgLightTest_" + Guid.NewGuid().ToString("N"));
            var path = Path.Combine(dir, "wp.bmp");
            try
            {
                var vbounds = new Rectangle(0, 0, 3840, 1080);
                var monitors = new System.Collections.Generic.List<Rectangle>
                {
                    new Rectangle(0, 0, 1920, 1080),
                    new Rectangle(1920, 0, 1920, 1080)
                };
                WallpaperRenderer.Render(new SystemInfoData { ComputerName = "PC01" },
                    ConfigWithOutput(path), vbounds, monitors);

                Assert.True(File.Exists(path));
                using (var img = Image.FromFile(path))
                {
                    Assert.Equal(3840, img.Width);
                    Assert.Equal(1080, img.Height);
                }
            }
            finally
            {
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }
        }

        [Fact]
        public void Render_with_missing_bgImage_falls_back_to_solid()
        {
            var dir = Path.Combine(Path.GetTempPath(), "BgLightTest_" + Guid.NewGuid().ToString("N"));
            var path = Path.Combine(dir, "wp.bmp");
            try
            {
                var config = AppConfig.Parse(new[] { "/outputPath=" + path, @"/bgImage=C:\does\not\exist.png" });
                WallpaperRenderer.Render(new SystemInfoData { ComputerName = "PC01" }, config, 800, 600);
                Assert.True(File.Exists(path));
            }
            finally
            {
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }
        }

        [Fact]
        public void Render_with_present_bgImage()
        {
            var dir = Path.Combine(Path.GetTempPath(), "BgLightTest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            var imgPath = Path.Combine(dir, "bg.png");
            using (var bg = new Bitmap(100, 100))
            {
                bg.Save(imgPath, ImageFormat.Png);
            }
            var path = Path.Combine(dir, "wp.bmp");
            try
            {
                var config = AppConfig.Parse(new[] { "/outputPath=" + path, "/bgImage=" + imgPath });
                WallpaperRenderer.Render(new SystemInfoData { ComputerName = "PC01" }, config, 800, 600);
                Assert.True(File.Exists(path));
            }
            finally
            {
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }
        }
    }
}
