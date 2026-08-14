using System.Drawing;

namespace BetriebsmittelPublisher.Core
{
    public static class DesignSystem
    {
        public static class Colors
        {
            public static readonly Color WindowBackground = Color.FromArgb(0x1a, 0x1d, 0x29);
            public static readonly Color Background = Color.FromArgb(0x1a, 0x1d, 0x29);
            public static readonly Color Accent = Color.FromArgb(0xff, 0x5c, 0x5c);
            public static readonly Color Secondary = Color.FromArgb(0x5b, 0x64, 0x78);
            public static readonly Color TextPrimary = Color.FromArgb(0xff, 0xff, 0xff);
            public static readonly Color TextSecondary = Color.FromArgb(0xcc, 0xcc, 0xcc);
            public static readonly Color TextDisabled = Color.FromArgb(0x88, 0x88, 0x88);
            public static readonly Color ControlBackground = Color.FromArgb(0x22, 0x25, 0x33);
            public static readonly Color ControlBorder = Color.FromArgb(0x3a, 0x3e, 0x50);
            public static readonly Color ControlHover = Color.FromArgb(0x2a, 0x2e, 0x3d);
            public static readonly Color Success = Color.FromArgb(0x4a, 0x7c, 0x59);
            public static readonly Color Warning = Color.FromArgb(0xc9, 0x8b, 0x35);
            public static readonly Color Error = Color.FromArgb(0xd9, 0x5c, 0x5c);
            public static readonly Color ButtonForeground = Color.White;
        }

        public static class Spacing
        {
            public static readonly int Small = 4;
            public static readonly int Medium = 8;
            public static readonly int Large = 16;
            public static readonly int XLarge = 24;
        }

        public static class Fonts
        {
            public static class Headline
            {
                public static readonly Font Instance = Typography.GetSansFont(12.0f, FontStyle.Bold);
                public static implicit operator Font(Headline headline) => Instance;
            }

            public static class Body
            {
                public static readonly Font Instance = Typography.GetSansFont(9.5f);
                public static implicit operator Font(Body body) => Instance;
            }

            public static class Caption
            {
                public static readonly Font Instance = Typography.GetSansFont(8.0f);
                public static implicit operator Font(Caption caption) => Instance;
            }

            public static class Monospace
            {
                public static readonly Font Instance = Typography.GetMonoFont(9.0f);
                public static implicit operator Font(Monospace mono) => Instance;
            }
        }

        public static class Typography
        {
            public static readonly float DefaultMonoFontSize = 9.0f;
            public static readonly float DefaultSansFontSize = 9.5f;
            public static readonly float HeadingFontSize = 12.0f;

            public static FontFamily MonoFontFamily => FontManager.JetBrainsMonoFamily;
            public static FontFamily SansFontFamily => FontManager.InterFamily;

            public static Font GetMonoFont(float size = 9.0f, FontStyle style = FontStyle.Regular)
            {
                return FontManager.GetJetBrainsMonoFont(size, style);
            }

            public static Font GetSansFont(float size = 9.5f, FontStyle style = FontStyle.Regular)
            {
                return FontManager.GetInterFont(size, style);
            }
        }
    }
}