using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

namespace BgLight
{
    public static class WallpaperRenderer
    {
        private const int PanelMargin = 40;   // distance panneau <-> bord écran
        private const int PanelPadding = 20;   // distance texte <-> bord panneau
        private const int LineSpacing = 6;    // espace vertical supplémentaire entre lignes

        public static void Render(SystemInfoData data, AppConfig config, int width, int height)
        {
            var lines = data.ToLines();

            using (var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb))
            using (var graphics = Graphics.FromImage(bitmap))
            using (var font = new Font(config.FontName, config.FontSize, FontStyle.Regular, GraphicsUnit.Point))
            using (var textBrush = new SolidBrush(Color.FromArgb(240, 240, 240)))
            using (var panelBrush = new SolidBrush(Color.FromArgb(140, 0, 0, 0)))
            {
                graphics.Clear(config.BgColor);
                graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                // Mesure du bloc de texte
                float lineHeight = font.GetHeight(graphics) + LineSpacing;
                float textWidth = MeasureMaxWidth(graphics, lines, font);
                float textHeight = lineHeight * lines.Count;

                float panelWidth = textWidth + PanelPadding * 2;
                float panelHeight = textHeight + PanelPadding * 2;

                PointF panelOrigin = PanelOrigin(config.Position, width, height, panelWidth, panelHeight);

                graphics.FillRectangle(panelBrush, panelOrigin.X, panelOrigin.Y, panelWidth, panelHeight);

                float x = panelOrigin.X + PanelPadding;
                float y = panelOrigin.Y + PanelPadding;
                foreach (var line in lines)
                {
                    graphics.DrawString(line, font, textBrush, x, y);
                    y += lineHeight;
                }

                var dir = Path.GetDirectoryName(config.OutputPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                bitmap.Save(config.OutputPath, ImageFormat.Bmp);
            }
        }

        private static float MeasureMaxWidth(Graphics graphics, IList<string> lines, Font font)
        {
            float max = 1f;
            foreach (var line in lines)
            {
                var size = graphics.MeasureString(string.IsNullOrEmpty(line) ? " " : line, font);
                if (size.Width > max) max = size.Width;
            }
            return max;
        }

        private static PointF PanelOrigin(PanelPosition pos, int width, int height, float pw, float ph)
        {
            float x, y;
            switch (pos)
            {
                case PanelPosition.TopRight:
                    x = width - PanelMargin - pw; y = PanelMargin;
                    break;
                case PanelPosition.BottomLeft:
                    x = PanelMargin; y = height - PanelMargin - ph;
                    break;
                case PanelPosition.BottomRight:
                    x = width - PanelMargin - pw; y = height - PanelMargin - ph;
                    break;
                case PanelPosition.TopLeft:
                default:
                    x = PanelMargin; y = PanelMargin;
                    break;
            }

            // Ne jamais démarrer hors écran (grande police / petite résolution).
            return new PointF(Math.Max(0f, x), Math.Max(0f, y));
        }
    }
}
