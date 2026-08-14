using System.Drawing.Text;
using System.Reflection;
using System.Runtime.InteropServices;

namespace BetriebsmittelPublisher.Core
{
    public static class FontManager
    {
        private static PrivateFontCollection? _privateFontCollection;
        private static FontFamily? _jetBrainsMonoFamily;
        private static FontFamily? _interFamily;
        private static bool _initialized = false;

        static FontManager() { }

        public static void Initialize()
        {
            if (_initialized)
                return;

            try
            {
                _privateFontCollection = new PrivateFontCollection();

                Logger.Info("Lade JetBrains Mono Font...");
                _jetBrainsMonoFamily = LoadFontFromResource("BetriebsmittelPublisher.Resources.JetBrainsMono-Regular.ttf");
                Logger.Info(_jetBrainsMonoFamily != null ? "JetBrains Mono erfolgreich geladen" : "JetBrains Mono nicht gefunden - verwende Fallback");

                Logger.Info("Lade Inter Font...");
                _interFamily = LoadFontFromResource("BetriebsmittelPublisher.Resources.Inter-Regular.ttf");
                Logger.Info(_interFamily != null ? "Inter erfolgreich geladen" : "Inter nicht gefunden - verwende Fallback");

                _initialized = true;
                Logger.Info("FontManager initialisiert");
            }
            catch (Exception ex)
            {
                Logger.Error("Font loading failed", ex);
                _initialized = true;
            }
        }

        private static FontFamily? LoadFontFromResource(string resourcePath)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream(resourcePath))
                {
                    if (stream == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Font resource not found: {resourcePath}");
                        return null;
                    }

                    byte[] fontData = new byte[stream.Length];
                    stream.ReadExactly(fontData, 0, fontData.Length);

                    // Pin the memory and add font to collection
                    IntPtr fontPtr = Marshal.UnsafeAddrOfPinnedArrayElement(fontData, 0);
                    _privateFontCollection?.AddMemoryFont(fontPtr, fontData.Length);

                    // Return the last added font family
                    if (_privateFontCollection != null)
                    {
                        return _privateFontCollection.Families[_privateFontCollection.Families.Length - 1];
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load font {resourcePath}: {ex.Message}");
                return null;
            }
        }

        public static Font GetJetBrainsMonoFont(float size, FontStyle style = FontStyle.Regular)
        {
            if (!_initialized)
                Initialize();

            if (_jetBrainsMonoFamily != null)
            {
                return GetFont(_jetBrainsMonoFamily, size, style);
            }

            // Fallback to Consolas
            return new Font("Consolas", size, style);
        }

        public static Font GetInterFont(float size, FontStyle style = FontStyle.Regular)
        {
            if (!_initialized)
                Initialize();

            if (_interFamily != null)
            {
                return GetFont(_interFamily, size, style);
            }

            // Fallback to Segoe UI
            return new Font("Segoe UI", size, style);
        }

        private static Font GetFont(FontFamily family, float size, FontStyle style)
        {
            try
            {
                if (family.IsStyleAvailable(style))
                {
                    return new Font(family, size, style);
                }
                else
                {
                    // Fallback to Regular if requested style not available
                    return new Font(family, size, FontStyle.Regular);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to create font: {ex.Message}");
                // Ultimate fallback to default system font
                return new Font("Microsoft Sans Serif", size, style);
            }
        }

        public static FontFamily? JetBrainsMonoFamily
        {
            get
            {
                if (!_initialized)
                    Initialize();
                return _jetBrainsMonoFamily;
            }
        }

        public static FontFamily? InterFamily
        {
            get
            {
                if (!_initialized)
                    Initialize();
                return _interFamily;
            }
        }
    }
}