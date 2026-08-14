# Betriebsmittel Publisher

## Version Information
- **Version**: 1.3.0
- **Product**: Betriebsmittel Publisher
- **Company**: Industrial Automation
- **Copyright**: © 2026 Industrial Automation

Die Version wird mit den GitHub-Releases synchron gehalten
(https://github.com/88frank88/mqtt-app-v2/releases).

## Changelog

### v1.3.0
- Station-Nummer (Modul) manuell pflegbar unter "Automation Parameter"
- modul-Attribut im TExecution-XML kommt jetzt aus der Station-Nummer
  (statt Topic-Extraktion)
- Versionsanzeige der App mit GitHub synchronisiert

### v1.2.0
- TExecution-XML-Format mit vollständiger Namespace-Deklaration
- modul, feature, requestTopic/responseTopic, host/port, motorNr-Mapping
- Globales Motor-Nummer-Feld (8-stellig)
- Publish nur die markierte Zeile

### v1.1.x
- MQTT-Broker-Einstellungen (Adresse, Port, Client-ID, Zugangsdaten)
- Verbindung testen / Verbinden / Trennen
- CONNACK-Parsing-Fix

### v1.0.x
- Initiales Release, Dark Mode UI, Logging-System

## Features
- Dark mode UI mit #1a1d29 Hintergrund und #ff5c5c Accent
- MQTT 3.1.1 Client mit zero dependencies
- TExecution-XML-Generierung
- Einstellungen (settings.ini) mit Broker-, Topic- und Automation-Sektionen
- Logging ins LOG/-Verzeichnis (tägliche Rotation)
- Single-file deployment

## Technical Details
- **Framework**: .NET 10 WinForms
- **Target**: net10.0-windows
- **Deployment**: Single executable with embedded resources
- **Dependencies**: Zero NuGet packages

## Build
```bash
dotnet publish -c Release -r win-x64 --self-contained true
```
