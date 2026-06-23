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
            var rows = data.Rows();

            using (var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb))
            using (var graphics = Graphics.FromImage(bitmap))
            using (var font = new Font(config.FontName, config.FontSize, FontStyle.Regular, GraphicsUnit.Point))
            using (var textBrush = new SolidBrush(Color.FromArgb(240, 240, 240)))
            using (var panelBrush = new SolidBrush(Color.FromArgb(140, 0, 0, 0)))
            {
                graphics.Clear(config.BgColor);
                graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                // Mesure du bloc de texte (2 colonnes : label et value)
                float lineHeight = font.GetHeight(graphics) + LineSpacing;
                var labelValueSizes = MeasureColumns(graphics, rows, font);
                float labelWidth = labelValueSizes.Item1;
                float valueWidth = labelValueSizes.Item2;
                float columnGap = 20f; // espace entre colonnes
                float textWidth = labelWidth + columnGap + valueWidth;
                float textHeight = lineHeight * rows.Count;

                float panelWidth = textWidth + PanelPadding * 2;
                float panelHeight = textHeight + PanelPadding * 2;

                PointF panelOrigin = PanelOrigin(config.Position, width, height, panelWidth, panelHeight);

                graphics.FillRectangle(panelBrush, panelOrigin.X, panelOrigin.Y, panelWidth, panelHeight);

                float x = panelOrigin.X + PanelPadding;
                float y = panelOrigin.Y + PanelPadding;
                foreach (var row in rows)
                {
                    graphics.DrawString(row.Label + ":", font, textBrush, x, y);
                    graphics.DrawString(row.Value, font, textBrush, x + labelWidth + columnGap, y);
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

        private static (float, float) MeasureColumns(Graphics graphics, IList<(string Label, string Value)> rows, Font font)
        {
            float maxLabelWidth = 1f;
            float maxValueWidth = 1f;
            foreach (var row in rows)
            {
                var labelSize = graphics.MeasureString(row.Label + ":", font);
                var valueSize = graphics.MeasureString(row.Value, font);
                if (labelSize.Width > maxLabelWidth) maxLabelWidth = labelSize.Width;
                if (valueSize.Width > maxValueWidth) maxValueWidth = valueSize.Width;
            }
            return (maxLabelWidth, maxValueWidth);
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
