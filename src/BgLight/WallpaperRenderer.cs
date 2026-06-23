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
        private const int PanelMargin = 40;
        private const int PanelPadding = 24;
        private const int LineSpacing = 7;
        private const int CornerRadius = 14;
        private const int TitleGap = 10;
        private const int AccentHeight = 3;
        private const int AccentGap = 14;
        private const int ColumnGap = 28;
        private const int SectionTitleGap = 4;
        private const int SectionGap = 12;
        private const int FooterGap = 14;
        private const int ShadowOffset = 6;

        private const string FooterCredit = "made by navanem.com";

        public static void Render(SystemInfoData data, AppConfig config, int width, int height)
        {
            Render(data, config, new Rectangle(0, 0, width, height),
                new List<Rectangle> { new Rectangle(0, 0, width, height) });
        }

        public static void Render(SystemInfoData data, AppConfig config, Rectangle virtualBounds, IList<Rectangle> monitors)
        {
            int w = Math.Max(1, virtualBounds.Width);
            int h = Math.Max(1, virtualBounds.Height);

            using (var bitmap = new Bitmap(w, h, PixelFormat.Format24bppRgb))
            using (var graphics = Graphics.FromImage(bitmap))
            using (var titleFont = new Font(config.FontName, config.FontSize + 3f, FontStyle.Bold, GraphicsUnit.Point))
            using (var sectionFont = new Font(config.FontName, Math.Max(7f, config.FontSize - 1f), FontStyle.Bold, GraphicsUnit.Point))
            using (var font = new Font(config.FontName, config.FontSize, FontStyle.Regular, GraphicsUnit.Point))
            using (var footerFont = new Font(config.FontName, Math.Max(7f, config.FontSize - 2f), FontStyle.Regular, GraphicsUnit.Point))
            using (var textBrush = new SolidBrush(Color.FromArgb(240, 240, 240)))
            using (var labelBrush = new SolidBrush(Color.FromArgb(170, 170, 170)))
            using (var footerBrush = new SolidBrush(Color.FromArgb(140, 140, 140)))
            using (var panelBrush = new SolidBrush(Color.FromArgb(120, 0, 0, 0)))
            using (var shadowBrush = new SolidBrush(Color.FromArgb(90, 0, 0, 0)))
            using (var borderPen = new Pen(Color.FromArgb(60, 255, 255, 255), 1f))
            using (var accentBrush = new SolidBrush(config.AccentColor))
            {
                DrawBackground(graphics, config, w, h);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                var sections = data.Sections();

                float lineHeight = font.GetHeight(graphics) + LineSpacing;
                float titleHeight = titleFont.GetHeight(graphics);
                float sectionHeight = sectionFont.GetHeight(graphics);
                float footerHeight = footerFont.GetHeight(graphics);

                float labelColWidth = 1f, valueColWidth = 1f, sectionTitleWidth = 1f;
                foreach (var s in sections)
                {
                    float sw = graphics.MeasureString(s.Title, sectionFont).Width;
                    if (sw > sectionTitleWidth) sectionTitleWidth = sw;
                    foreach (var r in s.Rows)
                    {
                        float lw = graphics.MeasureString(string.IsNullOrEmpty(r.Label) ? " " : r.Label, font).Width;
                        if (lw > labelColWidth) labelColWidth = lw;
                        float vw = graphics.MeasureString(string.IsNullOrEmpty(r.Value) ? " " : r.Value, font).Width;
                        if (vw > valueColWidth) valueColWidth = vw;
                    }
                }

                string title = string.IsNullOrEmpty(data.Title) ? " " : data.Title;
                float titleWidth = graphics.MeasureString(title, titleFont).Width;
                string footer = BuildFooter();
                float footerWidth = graphics.MeasureString(footer, footerFont).Width;
                float bodyWidth = labelColWidth + ColumnGap + valueColWidth;

                float contentWidth = Math.Max(Math.Max(titleWidth, bodyWidth), Math.Max(footerWidth, sectionTitleWidth));

                float contentHeight = titleHeight + TitleGap + AccentHeight + AccentGap;
                for (int i = 0; i < sections.Count; i++)
                {
                    contentHeight += sectionHeight + SectionTitleGap + lineHeight * sections[i].Rows.Count;
                    if (i < sections.Count - 1) contentHeight += SectionGap;
                }
                contentHeight += FooterGap + footerHeight;

                float panelWidth = contentWidth + PanelPadding * 2;
                float panelHeight = contentHeight + PanelPadding * 2;
                float valueOffsetX = labelColWidth + ColumnGap;

                void DrawPanelAt(float ox, float oy)
                {
                    using (var shadowPath = RoundedRect(ox + ShadowOffset, oy + ShadowOffset, panelWidth, panelHeight, CornerRadius))
                    {
                        graphics.FillPath(shadowBrush, shadowPath);
                    }
                    using (var path = RoundedRect(ox, oy, panelWidth, panelHeight, CornerRadius))
                    {
                        graphics.FillPath(panelBrush, path);
                        graphics.DrawPath(borderPen, path);
                    }

                    float x = ox + PanelPadding;
                    float y = oy + PanelPadding;

                    graphics.DrawString(title, titleFont, textBrush, x, y);
                    y += titleHeight + TitleGap;

                    using (var accentPath = RoundedRect(x, y, contentWidth, AccentHeight, AccentHeight / 2f))
                    {
                        graphics.FillPath(accentBrush, accentPath);
                    }
                    y += AccentHeight + AccentGap;

                    for (int i = 0; i < sections.Count; i++)
                    {
                        graphics.DrawString(sections[i].Title, sectionFont, accentBrush, x, y);
                        y += sectionHeight + SectionTitleGap;
                        foreach (var r in sections[i].Rows)
                        {
                            graphics.DrawString(r.Label, font, labelBrush, x, y);
                            graphics.DrawString(r.Value, font, textBrush, x + valueOffsetX, y);
                            y += lineHeight;
                        }
                        if (i < sections.Count - 1) y += SectionGap;
                    }

                    y += FooterGap;
                    graphics.DrawString(footer, footerFont, footerBrush, x, y);
                }

                foreach (var m in monitors)
                {
                    var rect = new Rectangle(m.X - virtualBounds.X, m.Y - virtualBounds.Y, m.Width, m.Height);
                    PointF origin = PanelOrigin(config.Position, rect, panelWidth, panelHeight);
                    DrawPanelAt(origin.X, origin.Y);
                }

                var dir = Path.GetDirectoryName(config.OutputPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                bitmap.Save(config.OutputPath, ImageFormat.Bmp);
            }
        }

        private static void DrawBackground(Graphics graphics, AppConfig config, int w, int h)
        {
            if (!string.IsNullOrEmpty(config.BgImage) && File.Exists(config.BgImage))
            {
                try
                {
                    using (var img = Image.FromFile(config.BgImage))
                    {
                        graphics.DrawImage(img, new Rectangle(0, 0, w, h));
                        return;
                    }
                }
                catch { }
            }
            graphics.Clear(config.BgColor);
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

        private static PointF PanelOrigin(PanelPosition pos, Rectangle area, float pw, float ph)
        {
            float x, y;
            switch (pos)
            {
                case PanelPosition.TopRight:
                    x = area.Right - PanelMargin - pw; y = area.Top + PanelMargin;
                    break;
                case PanelPosition.BottomLeft:
                    x = area.Left + PanelMargin; y = area.Bottom - PanelMargin - ph;
                    break;
                case PanelPosition.BottomRight:
                    x = area.Right - PanelMargin - pw; y = area.Bottom - PanelMargin - ph;
                    break;
                case PanelPosition.TopLeft:
                default:
                    x = area.Left + PanelMargin; y = area.Top + PanelMargin;
                    break;
            }

            // Never start before the monitor's top-left edge.
            return new PointF(Math.Max(area.Left, x), Math.Max(area.Top, y));
        }
    }
}
