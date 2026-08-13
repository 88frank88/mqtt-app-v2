using System.Reflection;

namespace BetriebsmittelPublisher.Core
{
    public static class VersionInfo
    {
        public static string Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
        public static string Product => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "Betriebsmittel Publisher";
        public static string Company => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "Industrial Automation";
        public static string Copyright => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? "© 2026";
        
        public static string FullInfo => $"{Product} v{Version} - {Copyright}";
        public static string ShortInfo => $"{Product} v{Version}";
    }
}