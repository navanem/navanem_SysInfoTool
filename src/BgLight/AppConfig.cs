using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;

namespace BgLight
{
    public class AppConfig
    {
        public string OutputPath { get; private set; } = @"C:\ProgramData\BgLight\wallpaper_info.bmp";
        public float FontSize { get; private set; } = 11f;
        public PanelPosition Position { get; private set; } = PanelPosition.TopRight;
        public Color BgColor { get; private set; } = ColorTranslator.FromHtml("#202020");
        public string FontName { get; private set; } = "Segoe UI";
        public Color AccentColor { get; private set; } = ColorTranslator.FromHtml("#0078D4");

        public string LogPath
        {
            get
            {
                var dir = Path.GetDirectoryName(OutputPath);
                if (string.IsNullOrEmpty(dir))
                {
                    dir = ".";
                }
                return Path.Combine(dir, "log.txt");
            }
        }

        public static AppConfig Parse(string[] args)
        {
            var config = new AppConfig();
            if (args == null)
            {
                return config;
            }

            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var arg in args)
            {
                if (string.IsNullOrEmpty(arg) || !arg.StartsWith("/", System.StringComparison.Ordinal))
                {
                    continue;
                }

                var body = arg.Substring(1);
                var eq = body.IndexOf('=');
                if (eq <= 0)
                {
                    continue;
                }

                var key = body.Substring(0, eq).Trim();
                var value = body.Substring(eq + 1).Trim();
                if (value.Length >= 2 && value[0] == '"' && value[value.Length - 1] == '"')
                    value = value.Substring(1, value.Length - 2);
                map[key] = value;
            }

            string s;

            if (map.TryGetValue("outputPath", out s) && !string.IsNullOrWhiteSpace(s))
            {
                config.OutputPath = s;
            }

            if (map.TryGetValue("fontSize", out s) &&
                float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var size) &&
                size > 0)
            {
                config.FontSize = size;
            }

            if (map.TryGetValue("position", out s) &&
                Enum.TryParse(s, true, out PanelPosition pos))
            {
                config.Position = pos;
            }

            if (map.TryGetValue("bgColor", out s))
            {
                try
                {
                    config.BgColor = ColorTranslator.FromHtml(s);
                }
                catch
                {
                    // garde le défaut
                }
            }

            if (map.TryGetValue("fontName", out s) && !string.IsNullOrWhiteSpace(s))
            {
                config.FontName = s;
            }

            if (map.TryGetValue("accentColor", out s))
            {
                try
                {
                    config.AccentColor = ColorTranslator.FromHtml(s);
                }
                catch
                {
                    // garde le défaut
                }
            }

            return config;
        }
    }
}
