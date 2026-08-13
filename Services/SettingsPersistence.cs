using System;
using System.IO;
using BetriebsmittelPublisher.Models;

namespace BetriebsmittelPublisher.Services
{
    public static class SettingsPersistence
    {
        private static readonly string SettingsFilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, 
            "settings.ini"
        );

        public static SettingsModel Load()
        {
            var settings = new SettingsModel();

            if (!File.Exists(SettingsFilePath))
                return settings;

            try
            {
                var lines = File.ReadAllLines(SettingsFilePath);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || line.StartsWith("["))
                        continue;

                    var parts = line.Split(new[] { '=' }, 2);
                    if (parts.Length != 2) continue;

                    var key = parts[0].Trim();
                    var value = parts[1].Trim();

                    switch (key)
                    {
                        case "betriebsmittel1_topic":
                            settings.Betriebsmittel1Topic = value;
                            break;
                        case "betriebsmittel2_topic":
                            settings.Betriebsmittel2Topic = value;
                            break;
                        case "betriebsmittel3_topic":
                            settings.Betriebsmittel3Topic = value;
                            break;
                        case "betriebsmittel4_topic":
                            settings.Betriebsmittel4Topic = value;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but return defaults
                Console.WriteLine($"Error loading settings: {ex.Message}");
            }

            return settings;
        }

        public static bool Save(SettingsModel settings)
        {
            try
            {
                var content = $"[Betriebsmittel]\n" +
                             $"betriebsmittel1_topic={settings.Betriebsmittel1Topic}\n" +
                             $"betriebsmittel2_topic={settings.Betriebsmittel2Topic}\n" +
                             $"betriebsmittel3_topic={settings.Betriebsmittel3Topic}\n" +
                             $"betriebsmittel4_topic={settings.Betriebsmittel4Topic}\n";

                File.WriteAllText(SettingsFilePath, content);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
                return false;
            }
        }
    }
}