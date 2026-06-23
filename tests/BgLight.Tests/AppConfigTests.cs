using System.Drawing;
using Xunit;

namespace BgLight.Tests
{
    public class AppConfigTests
    {
        [Fact]
        public void Defaults_are_applied_when_no_args()
        {
            var c = AppConfig.Parse(new string[0]);

            Assert.Equal(@"C:\ProgramData\BgLight\wallpaper_info.bmp", c.OutputPath);
            Assert.Equal(11f, c.FontSize);
            Assert.Equal(PanelPosition.TopRight, c.Position);
            Assert.Equal(ColorTranslator.FromHtml("#0078D4"), c.AccentColor);
            Assert.Equal(ColorTranslator.FromHtml("#202020"), c.BgColor);
            Assert.Equal("Segoe UI", c.FontName);
        }

        [Fact]
        public void LogPath_is_logtxt_next_to_output()
        {
            var c = AppConfig.Parse(new[] { @"/outputPath=C:\Temp\Sub\wp.bmp" });

            Assert.Equal(@"C:\Temp\Sub\wp.bmp", c.OutputPath);
            Assert.Equal(@"C:\Temp\Sub\log.txt", c.LogPath);
        }

        [Fact]
        public void Parses_all_known_arguments_case_insensitive()
        {
            var c = AppConfig.Parse(new[]
            {
                "/FONTSIZE=14",
                "/position=BottomRight",
                "/bgColor=#102030",
                "/fontName=Consolas"
            });

            Assert.Equal(14f, c.FontSize);
            Assert.Equal(PanelPosition.BottomRight, c.Position);
            Assert.Equal(ColorTranslator.FromHtml("#102030"), c.BgColor);
            Assert.Equal("Consolas", c.FontName);
        }

        [Fact]
        public void Quoted_values_are_unwrapped()
        {
            var c = AppConfig.Parse(new[] { "/fontName=\"Segoe UI Semibold\"" });
            Assert.Equal("Segoe UI Semibold", c.FontName);
        }

        [Fact]
        public void Invalid_values_fall_back_to_defaults()
        {
            var c = AppConfig.Parse(new[]
            {
                "/fontSize=notanumber",
                "/position=Sideways",
                "/bgColor=notacolor",
                "/accentColor=nope"
            });

            Assert.Equal(11f, c.FontSize);
            Assert.Equal(PanelPosition.TopRight, c.Position);
            Assert.Equal(ColorTranslator.FromHtml("#0078D4"), c.AccentColor);
            Assert.Equal(ColorTranslator.FromHtml("#202020"), c.BgColor);
        }

        [Fact]
        public void Only_one_outer_quote_pair_is_removed()
        {
            var c = AppConfig.Parse(new[] { "/fontName=\"\"Consolas\"\"" });
            Assert.Equal("\"Consolas\"", c.FontName);
        }

        [Fact]
        public void Parses_accent_color()
        {
            var c = AppConfig.Parse(new[] { "/accentColor=#FF8800" });
            Assert.Equal(ColorTranslator.FromHtml("#FF8800"), c.AccentColor);
        }
    }
}
