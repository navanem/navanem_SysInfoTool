using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

namespace BgLight
{
    public static class WallpaperRenderer
    {
        private const int PanelMargin = 40;    // distance panel <-> screen edge
        private const int PanelPadding = 24;    // distance content <-> panel edge
        private const int LineSpacing = 8;     // vertical spacing between body lines
        private const int CornerRadius = 12;    // radius of panel rounded corners
        private const int TitleGap = 10;    // space below title (before line)
        private const int AccentHeight = 3;     // accent line thickness
        private const int AccentGap = 14;    // space below line (before body)
        private const int ColumnGap = 28;    // space between label and value columns
        private const int FooterGap = 14;    // space above panel footer

        private const string FooterCredit = "made by navanem.com";

        public static void Render(SystemInfoData data, AppConfig config, int width, int height)
        {
            var rows = new List<(string Label, string Value)>();
            foreach (var section in data.Sections())
            {
                foreach (var r in section.Rows)
                {
                    rows.Add(r);
                }
            }

            using (var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb))
            using (var graphics = Graphics.FromImage(bitmap))
            using (var titleFont = new Font(config.FontName, config.FontSize + 3f, FontStyle.Bold, GraphicsUnit.Point))
            using (var font = new Font(config.FontName, config.FontSize, FontStyle.Regular, GraphicsUnit.Point))
            using (var footerFont = new Font(config.FontName, Math.Max(7f, config.FontSize - 2f), FontStyle.Regular, GraphicsUnit.Point))
            using (var textBrush = new SolidBrush(Color.FromArgb(240, 240, 240)))
            using (var labelBrush = new SolidBrush(Color.FromArgb(170, 170, 170)))
            using (var footerBrush = new SolidBrush(Color.FromArgb(140, 140, 140)))
            using (var panelBrush = new SolidBrush(Color.FromArgb(100, 0, 0, 0)))
            using (var accentBrush = new SolidBrush(config.AccentColor))
            {
                graphics.Clear(config.BgColor);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                float lineHeight = font.GetHeight(graphics) + LineSpacing;
                float titleHeight = titleFont.GetHeight(graphics);

                float labelColWidth = 1f;
                float valueColWidth = 1f;
                foreach (var r in rows)
                {
                    float lw = graphics.MeasureString(string.IsNullOrEmpty(r.Label) ? " " : r.Label, font).Width;
                    if (lw > labelColWidth) labelColWidth = lw;
                    float vw = graphics.MeasureString(string.IsNullOrEmpty(r.Value) ? " " : r.Value, font).Width;
                    if (vw > valueColWidth) valueColWidth = vw;
                }

                string title = string.IsNullOrEmpty(data.Title) ? " " : data.Title;
                float titleWidth = graphics.MeasureString(title, titleFont).Width;
                float bodyWidth = labelColWidth + ColumnGap + valueColWidth;

                string footer = BuildFooter();
                float footerHeight = footerFont.GetHeight(graphics);
                float footerWidth = graphics.MeasureString(footer, footerFont).Width;

                float contentWidth = Math.Max(Math.Max(titleWidth, bodyWidth), footerWidth);

                float contentHeight = titleHeight + TitleGap + AccentHeight + AccentGap
                    + lineHeight * rows.Count + FooterGap + footerHeight;

                float panelWidth = contentWidth + PanelPadding * 2;
                float panelHeight = contentHeight + PanelPadding * 2;

                PointF panelOrigin = PanelOrigin(config.Position, width, height, panelWidth, panelHeight);

                using (var path = RoundedRect(panelOrigin.X, panelOrigin.Y, panelWidth, panelHeight, CornerRadius))
                {
                    graphics.FillPath(panelBrush, path);
                }

                float x = panelOrigin.X + PanelPadding;
                float y = panelOrigin.Y + PanelPadding;

                // Title (PC name)
                graphics.DrawString(title, titleFont, textBrush, x, y);
                y += titleHeight + TitleGap;

                // Accent line below title
                using (var accentPath = RoundedRect(x, y, contentWidth, AccentHeight, AccentHeight / 2f))
                {
                    graphics.FillPath(accentBrush, accentPath);
                }
                y += AccentHeight + AccentGap;

                // Body: two aligned columns
                float valueX = x + labelColWidth + ColumnGap;
                foreach (var r in rows)
                {
                    graphics.DrawString(r.Label, font, labelBrush, x, y);
                    graphics.DrawString(r.Value, font, textBrush, valueX, y);
                    y += lineHeight;
                }

                // Panel footer: credit + version
                y += FooterGap;
                graphics.DrawString(footer, footerFont, footerBrush, x, y);

                var dir = Path.GetDirectoryName(config.OutputPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                bitmap.Save(config.OutputPath, ImageFormat.Bmp);
            }
        }

        private static string BuildFooter()
        {
            var v = typeof(WallpaperRenderer).Assembly.GetName().Version;
            if (v == null)
            {
                return FooterCredit;
            }
            return FooterCredit + "   ·   v" + v.Major + "." + v.Minor + "." + v.Build;
        }

        private static GraphicsPath RoundedRect(float x, float y, float w, float h, float radius)
        {
            float r = Math.Max(0f, Math.Min(radius, Math.Min(w, h) / 2f));
            var path = new GraphicsPath();
            if (r <= 0f)
            {
                path.AddRectangle(new RectangleF(x, y, w, h));
                return path;
            }

            float d = r * 2f;
            path.AddArc(x, y, d, d, 180f, 90f);
            path.AddArc(x + w - d, y, d, d, 270f, 90f);
            path.AddArc(x + w - d, y + h - d, d, d, 0f, 90f);
            path.AddArc(x, y + h - d, d, d, 90f, 90f);
            path.CloseFigure();
            return path;
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

            // Never start off-screen (large font / small resolution).
            return new PointF(Math.Max(0f, x), Math.Max(0f, y));
        }
    }
}
