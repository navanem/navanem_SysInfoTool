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
        private const int PanelMargin = 40;    // distance panneau <-> bord écran
        private const int PanelPadding = 24;    // distance contenu <-> bord panneau
        private const int LineSpacing = 8;     // espace vertical entre lignes du corps
        private const int CornerRadius = 12;    // rayon des coins arrondis du panneau
        private const int TitleGap = 10;    // espace sous le titre (avant le trait)
        private const int AccentHeight = 3;     // épaisseur du trait d'accent
        private const int AccentGap = 14;    // espace sous le trait (avant le corps)
        private const int ColumnGap = 28;    // espace entre colonne label et colonne valeur

        public static void Render(SystemInfoData data, AppConfig config, int width, int height)
        {
            var rows = data.Rows();

            using (var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb))
            using (var graphics = Graphics.FromImage(bitmap))
            using (var titleFont = new Font(config.FontName, config.FontSize + 3f, FontStyle.Bold, GraphicsUnit.Point))
            using (var font = new Font(config.FontName, config.FontSize, FontStyle.Regular, GraphicsUnit.Point))
            using (var textBrush = new SolidBrush(Color.FromArgb(240, 240, 240)))
            using (var labelBrush = new SolidBrush(Color.FromArgb(170, 170, 170)))
            using (var panelBrush = new SolidBrush(Color.FromArgb(140, 0, 0, 0)))
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
                float contentWidth = Math.Max(titleWidth, bodyWidth);

                float contentHeight = titleHeight + TitleGap + AccentHeight + AccentGap + lineHeight * rows.Count;

                float panelWidth = contentWidth + PanelPadding * 2;
                float panelHeight = contentHeight + PanelPadding * 2;

                PointF panelOrigin = PanelOrigin(config.Position, width, height, panelWidth, panelHeight);

                using (var path = RoundedRect(panelOrigin.X, panelOrigin.Y, panelWidth, panelHeight, CornerRadius))
                {
                    graphics.FillPath(panelBrush, path);
                }

                float x = panelOrigin.X + PanelPadding;
                float y = panelOrigin.Y + PanelPadding;

                // Titre (nom du PC)
                graphics.DrawString(title, titleFont, textBrush, x, y);
                y += titleHeight + TitleGap;

                // Trait d'accent sous le titre
                using (var accentPath = RoundedRect(x, y, contentWidth, AccentHeight, AccentHeight / 2f))
                {
                    graphics.FillPath(accentBrush, accentPath);
                }
                y += AccentHeight + AccentGap;

                // Corps : deux colonnes alignées
                float valueX = x + labelColWidth + ColumnGap;
                foreach (var r in rows)
                {
                    graphics.DrawString(r.Label, font, labelBrush, x, y);
                    graphics.DrawString(r.Value, font, textBrush, valueX, y);
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

            // Ne jamais démarrer hors écran (grande police / petite résolution).
            return new PointF(Math.Max(0f, x), Math.Max(0f, y));
        }
    }
}
