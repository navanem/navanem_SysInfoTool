using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BgLight
{
    internal static class Program
    {
        [STAThread]
        internal static int Main(string[] args)
        {
            var config = AppConfig.Parse(args);
            var logger = new Logger(config.LogPath);

            try
            {
                var data = SystemInfoCollector.Collect();

                var virtualBounds = SystemInformation.VirtualScreen;
                if (virtualBounds.Width <= 0 || virtualBounds.Height <= 0)
                {
                    virtualBounds = new Rectangle(0, 0, 1920, 1080);
                }

                var monitors = new List<Rectangle>();
                foreach (var screen in Screen.AllScreens)
                {
                    monitors.Add(screen.Bounds);
                }
                if (monitors.Count == 0)
                {
                    monitors.Add(virtualBounds);
                }

                WallpaperRenderer.Render(data, config, virtualBounds, monitors);
                WallpaperSetter.Apply(config.OutputPath);

                return 0;
            }
            catch (Exception ex)
            {
                logger.Error("Failed to update the wallpaper.", ex);
                return 1;
            }
        }
    }
}
