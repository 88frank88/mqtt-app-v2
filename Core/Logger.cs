using System;
using System.IO;
using System.Text;

namespace BetriebsmittelPublisher.Core
{
    public enum LogLevel
    {
        DEBUG,
        INFO,
        WARNING,
        ERROR
    }

    public static class Logger
    {
        private static readonly object _lock = new object();
        private static string _logDirectory = string.Empty;
        private static string _currentLogFile = string.Empty;
        private static bool _initialized = false;

        public static void Initialize()
        {
            if (_initialized)
                return;

            try
            {
                var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                _logDirectory = Path.Combine(appDirectory, "LOG");

                if (!Directory.Exists(_logDirectory))
                {
                    Directory.CreateDirectory(_logDirectory);
                }

                var today = DateTime.Now.ToString("yyyy-MM-dd");
                _currentLogFile = Path.Combine(_logDirectory, $"app_{today}.log");

                _initialized = true;

                Log(LogLevel.INFO, "=== Betriebsmittel Publisher gestartet ===");
                Log(LogLevel.INFO, $"Version: {VersionInfo.Version}");
                Log(LogLevel.INFO, $"Product: {VersionInfo.Product}");
                Log(LogLevel.INFO, $"Log-Verzeichnis: {_logDirectory}");
                Log(LogLevel.INFO, $"OS: {Environment.OSVersion}");
                Log(LogLevel.INFO, $".NET Runtime: {Environment.Version}");
                Log(LogLevel.INFO, $"Machine: {Environment.MachineName}");
                Log(LogLevel.INFO, $"User: {Environment.UserName}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logger initialization failed: {ex.Message}");
                _initialized = true;
            }
        }

        public static void Log(LogLevel level, string message)
        {
            if (!_initialized)
                Initialize();

            try
            {
                lock (_lock)
                {
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    var logLine = $"[{timestamp}] [{level,-7}] {message}{Environment.NewLine}";

                    File.AppendAllText(_currentLogFile, logLine, Encoding.UTF8);

                    System.Diagnostics.Debug.Write(logLine);
                }
            }
            catch
            {
            }
        }

        public static void Debug(string message) => Log(LogLevel.DEBUG, message);
        public static void Info(string message) => Log(LogLevel.INFO, message);
        public static void Warning(string message) => Log(LogLevel.WARNING, message);

        public static void Error(string message) => Log(LogLevel.ERROR, message);

        public static void Error(string message, Exception ex)
        {
            Log(LogLevel.ERROR, $"{message}");
            Log(LogLevel.ERROR, $"  Exception: {ex.GetType().Name}: {ex.Message}");
            Log(LogLevel.ERROR, $"  StackTrace: {ex.StackTrace}");

            if (ex.InnerException != null)
            {
                Log(LogLevel.ERROR, $"  InnerException: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
            }
        }

        public static void Shutdown()
        {
            Log(LogLevel.INFO, "=== Betriebsmittel Publisher beendet ===");
        }

        public static string GetCurrentLogFile() => _currentLogFile;
        public static string GetLogDirectory() => _logDirectory;
    }
}