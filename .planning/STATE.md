# Betriebsmittel Publisher Project State

## Current Status
**Version**: v1.3.0 (mit GitHub-Release synchron)
**Status**: Produktiv im Test durch Frank (2026-08-14)
**Repository**: https://github.com/88frank88/mqtt-app-v2
**Letzter Stand**: Commit 95d00fb, Release v1.3.0

## Wiedereinstieg (Resume)
```bash
cd <projekt>
git pull origin main
```
Dann in opencode:
- `/gsd-next` — GSD schlägt den nächsten Schritt vor
- `/gsd-resume-work` — voller Kontext-Restore aus dieser Datei
- Neue Anforderungen: `/gsd-capture` oder direkt Phase anlegen mit `/gsd-phase`

## Architektur-Überblick (Stand v1.3.0)

| Bereich | Dateien | Zweck |
|---------|---------|-------|
| UI | UI/MainForm.cs, UI/SettingsWindow.cs, UI/AutomationWindow.cs, UI/BaseForm.cs | Haupt-, Einstellungs- und Automatisierungsfenster, Dark-Mode-Basis |
| Core | Core/DesignSystem.cs, Core/FontManager.cs, Core/Logger.cs, Core/VersionInfo.cs | Design-Konstanten, eingebettete Fonts, LOG/-Verzeichnis, Versionsanzeige |
| MQTT | Services/ConnectionManager.cs, MqttPacketBuilder.cs, MqttPacketParser.cs, Models/MqttMessage.cs | Eigener MQTT-3.1.1-Client (TcpClient, zero dependencies) |
| XML | Services/XmlConverter.cs | TExecution-XML-Generierung |
| Persistenz | Services/SettingsPersistence.cs, Models/SettingsModel.cs | settings.ini ([Broker], [Betriebsmittel], [Automation]) |

## Wichtige Entscheidungen

### D001: Zero Dependencies
Alle Funktionalität in-house (kein NuGet) — Offline-Fähigkeit für industrielle Umgebung.

### D002: Single-File Deployment
PublishSingleFile + SelfContained, Fonts embedded als Ressourcen.

### D003: Design System
Dark Mode #1a1d29, Accent #ff5c5c (Koralle), JetBrains Mono + Inter.

### D006: TExecution-XML-Format (v1.2.0)
- Payload beginnt exakt mit `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>`
- Vollständige execution:TExecution-Struktur mit allen de-gmbh-Namespaces
- Mapping: feature = PG-Nummer, requestTopic/responseTopic = Topic 1:1,
  host/port = Broker-Settings, motorNr = globale 8-stellige Motor-Nummer,
  id = neue GUID pro Publish
- Vorlage: FAIL/XML.txt

### D007: Publish-Workflow (v1.2.0)
- Tabelle: Betriebsmittel-Dropdown (1-4) + freie PG-Nummer, max. 10 Zeilen
- Publish veröffentlicht NUR die markierte Zeile (MultiSelect aus)

### D008: Station-Nummer manuell (v1.3.0)
- modul-Attribut kommt aus manuellem Feld "Station-Nummer (Modul)"
  in Automation Parameter (station_number in settings.ini)
- Automatische Topic-Extraktion entfernt (war fehlerhaft)
- StationNumberParser gelöscht

### D009: Version-Sync mit GitHub
- .csproj (Version/AssemblyVersion/FileVersion) wird bei jedem Release
  mit dem GitHub-Tag synchron gebumpt — vor `gh release create` pflegen!
- Releases: v1.0.0 → v1.3.0 gepflegt

### D010: Logging (v1.0.2)
- LOG/-Ordner neben der .exe, täglich rotierend (app_YYYY-MM-DD.log)
- Level: DEBUG/INFO/WARNING/ERROR, Exceptions mit Stack-Trace
- MQTT-Operationen, Font-Laden, Settings, Publish werden geloggt

## settings.ini Struktur
```ini
[Broker]
broker_address / broker_port / client_id / username / password
[Betriebsmittel]
betriebsmittel1-4_topic
[Automation]
motor_number (8-stellig) / station_number (=modul) / quitk / tv / ma /
bauart / tool_position / connect_timeout / dmc
```

## Bekannte Punkte / Mögliche nächste Schritte
- Produktiv-Test läuft — Rückmeldung von Frank ausstehen
- Potenziell: README.md fürs Repository, verfeinerte Fehlerbehandlung,
  QoS 1-Unterstützung, Reconnect-Logik
- Phasen 1-5 der ursprünglichen Roadmap sind funktional abgedeckt
  (Roadmap entspricht nicht mehr 1:1 dem implementierten Produkt)

## Build & Deployment
```bash
dotnet publish -c Release -r win-x64 --self-contained true
# Ergebnis: bin/Release/net10.0-windows/win-x64/publish/BetriebsmittelPublisher.exe
```
Kein dotnet auf dem Entwicklungsserver verfügbar — Builds finden auf
Franks Windows-Maschine statt. Globale Git-Config ist gesetzt
(user.name=Frank, defaultBranch=main, excludesfile=~/.gitignore_global).
