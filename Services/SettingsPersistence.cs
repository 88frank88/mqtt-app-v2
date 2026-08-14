using System;
using System.IO;
using BetriebsmittelPublisher.Models;
using BetriebsmittelPublisher.Core;

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

            Logger.Info($"Lade Einstellungen von: {SettingsFilePath}");

            if (!File.Exists(SettingsFilePath))
            {
                Logger.Info("Keine settings.ini gefunden - verwende Standardwerte");
                return settings;
            }

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
                        case "broker_address":
                            settings.BrokerAddress = value;
                            break;
                        case "broker_port":
                            if (int.TryParse(value, out int port) && port > 0 && port <= 65535)
                                settings.BrokerPort = port;
                            break;
                        case "client_id":
                            settings.ClientId = value;
                            break;
                        case "username":
                            settings.Username = value;
                            break;
                        case "password":
                            settings.Password = value;
                            break;
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
                        case "motor_number":
                            settings.MotorNumber = value;
                            break;
                        case "quitk":
                            settings.Quitk = value;
                            break;
                        case "tv":
                            settings.Tv = value;
                            break;
                        case "ma":
                            settings.Ma = value;
                            break;
                        case "bauart":
                            settings.Bauart = value;
                            break;
                        case "tool_position":
                            settings.ToolPosition = value;
                            break;
                        case "connect_timeout":
                            settings.ConnectTimeout = value;
                            break;
                        case "dmc":
                            settings.Dmc = value;
                            break;
                    }
                }
                Logger.Info("Einstellungen erfolgreich geladen");
            }
            catch (Exception ex)
            {
                Logger.Error("Fehler beim Laden der Einstellungen", ex);
            }

            return settings;
        }

        public static bool Save(SettingsModel settings)
        {
            try
            {
                Logger.Info($"Speichere Einstellungen nach: {SettingsFilePath}");

                var content = $"[Broker]\n" +
                             $"broker_address={settings.BrokerAddress}\n" +
                             $"broker_port={settings.BrokerPort}\n" +
                             $"client_id={settings.ClientId}\n" +
                             $"username={settings.Username}\n" +
                             $"password={settings.Password}\n" +
                             $"[Betriebsmittel]\n" +
                             $"betriebsmittel1_topic={settings.Betriebsmittel1Topic}\n" +
                             $"betriebsmittel2_topic={settings.Betriebsmittel2Topic}\n" +
                             $"betriebsmittel3_topic={settings.Betriebsmittel3Topic}\n" +
                             $"betriebsmittel4_topic={settings.Betriebsmittel4Topic}\n" +
                             $"[Automation]\n" +
                             $"motor_number={settings.MotorNumber}\n" +
                             $"quitk={settings.Quitk}\n" +
                             $"tv={settings.Tv}\n" +
                             $"ma={settings.Ma}\n" +
                             $"bauart={settings.Bauart}\n" +
                             $"tool_position={settings.ToolPosition}\n" +
                             $"connect_timeout={settings.ConnectTimeout}\n" +
                             $"dmc={settings.Dmc}\n";

                File.WriteAllText(SettingsFilePath, content);
                Logger.Info("Einstellungen erfolgreich gespeichert");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Fehler beim Speichern der Einstellungen", ex);
                return false;
            }
        }
    }
}