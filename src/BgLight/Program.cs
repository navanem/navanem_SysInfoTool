using System;
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

                var bounds = Screen.PrimaryScreen != null
                    ? Screen.PrimaryScreen.Bounds
                    : new System.Drawing.Rectangle(0, 0, 1920, 1080);

                int width = bounds.Width > 0 ? bounds.Width : 1920;
                int height = bounds.Height > 0 ? bounds.Height : 1080;

                WallpaperRenderer.Render(data, config, width, height);
                WallpaperSetter.Apply(config.OutputPath);

                return 0;
            }
            catch (Exception ex)
            {
                logger.Error("Echec de la mise a jour du fond d'ecran.", ex);
                return 1;
            }
        }
    }
}
