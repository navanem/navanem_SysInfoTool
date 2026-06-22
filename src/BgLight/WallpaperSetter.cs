using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace BgLight
{
    public static class WallpaperSetter
    {
        private const int SPI_SETDESKWALLPAPER = 0x0014;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public static void Apply(string imagePath)
        {
            // Style "Remplir" (Fill)
            using (var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", writable: true))
            {
                if (key != null)
                {
                    key.SetValue("WallpaperStyle", "10"); // 10 = Fill
                    key.SetValue("TileWallpaper", "0");
                }
            }

            int result = SystemParametersInfo(
                SPI_SETDESKWALLPAPER,
                0,
                imagePath,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);

            if (result == 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(),
                    "SystemParametersInfo(SPI_SETDESKWALLPAPER) a echoue.");
            }
        }
    }
}
