# Projekt-Brief: Betriebsmittel Publisher (Arbeitstitel)

## GSD-Einstieg

**Wichtig für opencode: komplett neuer, eigenständiger Projektordner.** Nicht im bestehenden
`MqttTester`-Repo/-Ordner arbeiten, keine Dateien von dort kopieren oder wiederverwenden, keine
Commits/Änderungen im alten Repo. Neuen, leeren Ordner anlegen (z. B. `BetriebsmittelPublisher/`,
Name final klären — siehe "Offene Punkte"), darin:

```
/gsd:new-project --auto @dieser-brief.md
```

Das alte MQTT-Tester-Projekt dient **ausschließlich als fachliche/technische Referenz**
(gleicher Tech-Stack, gleiche Build-Vorgaben — siehe unten), nicht als Code-Basis. Der Code
wird im neuen Projekt komplett neu geschrieben.

## Projektidee

Eine eigenständige Windows-Desktop-Anwendung, die zwei bereits definierte Bausteine
zusammenführt:

1. Die **funktionalen Anforderungen** aus `opencode-anweisung-settings-pgnummer.md`
   (Settings-Tab für Publishtopics + PG-Nummer-Automation).
2. Das **Design-System** aus dem Hero-Mockup (`hero-design.html`) — visuell auf eine
   WinForms-Desktop-Oberfläche übertragen statt auf eine Webseite.

Technisch bleibt es bei denselben Grundfesten wie beim bisherigen MQTT-Tester-Projekt
(siehe unten), nur mit neuem Look statt WinForms-Standard-Optik.

## Tech-Stack & Build-Spec (verbindlich zu übernehmen, aber als neuer Code — keine Wiederverwendung alter Dateien)

- .NET 10, C# WinForms, `net10.0-windows`
- **Kein NuGet** — eigene `NuGet.Config` mit `<clear/>`, offline-fähiger Build
- **Einzige .exe** — `PublishSingleFile=true`, `SelfContained=true`, `RuntimeIdentifier=win-x64`
- Gleiches BUILD.bat-Prinzip wie im alten Projekt: `dotnet publish --no-restore`, keine
  Internetverbindung beim Build nötig — Skript im neuen Projekt eigenständig neu anlegen
- MQTT 3.1.1, reine TCP-Verbindung Port 1883 (kein WebSocket, kein TLS) — eigene
  Protokollimplementierung über `TcpClient`/`NetworkStream` (kein MQTTnet/M2Mqtt), fachlich
  gleicher Ansatz wie im bestehenden MQTT-Tester-Projekt, aber als eigenständiger, neu
  geschriebener Code im neuen Repo
- Komplett offline-fähig, keine Internetverbindung bei Build oder Betrieb nötig

## Funktionale Anforderungen

### Fenster 1: Einstellungen

- Textfelder: Publishtopic Betriebsmittel 1, 2, 3, 4
- **Kein separates Stationsnummer-Feld** — wird automatisch aus dem gewählten Topic geparst
- Lokale Persistenz (z. B. `settings.ini` neben der .exe), Laden beim Start, Speichern-Button
- Details siehe `opencode-anweisung-settings-pgnummer.md`

### Fenster 2: Automation (PG-Nummer)

- Übergeordnetes Feld **Motornummer** (gilt für alle Zeilen, nicht pro Zeile)
- Tabelle mit bis zu 10 Zeilen, je Zeile unabhängig:
  - Auswahl Betriebsmittel 1–4 (ComboBox, gemappt auf Topic aus Einstellungen)
  - PG-Nummer (8-stellig, numerisch, Validierung)
  - Publish-Button je Zeile → baut XML mit eingesetzter PG-Nummer, Motornummer, geparster
    Stationsnummer (`modul`) und `host`/`port` aus der aktiven MQTT-Verbindung, published auf
    den aufgelösten Topic (gleicher Wert für `requestTopic` und `responseTopic`)
- Vollständiges XML-Template, Feldzuordnung und verbleibende offene Punkte (`toolPosition`,
  `TV`, `MA`, `bauart`, `connectTimeout`, `DMC`) siehe `opencode-anweisung-settings-pgnummer.md`
  — diese Klärungen gelten unverändert auch hier

## Design-Anforderungen (aus `hero-design.html` abgeleitet, für WinForms übersetzt)

Das Web-Mockup war für eine Marketing-Hero-Sektion gedacht — hier wird nur das
**Design-System** (Farben, Typografie, Formsprache) übernommen, nicht das Layout
(Split-Screen-Vergleich, Fenster-Deko-Mockups sind für dieses Projekt nicht relevant).

- **Dark Mode**, Hintergrund `#1a1d29`
- Akzentfarbe Koralle `#ff5c5c` für Warnungen, ungültige Eingaben (z. B. PG-Nummer-Validierung),
  destruktive Aktionen
- Sekundärfarbe Blaugrau `#5b6478` für UI-Chrome: Rahmen, Platzhaltertexte, inaktive Elemente
- **Flaches Design**: keine Schatten, dünne 1px Outline-Borders statt WinForms-Standard-3D-Rahmen
  (`FlatStyle = Flat` auf allen Controls, wo möglich)
- Schriften:
  - **JetBrains Mono** für technische/Code-Inhalte (XML-Vorschau, Log-Ausgabe, Topic-Anzeige)
  - **Inter** für Fließtext/UI-Labels
  - Beide Fonts müssen **als Ressourcen eingebettet** werden (offline-fähig, kein Systemfont
    vorausgesetzt) — Laden zur Laufzeit über `PrivateFontCollection`
    (`System.Drawing.Text.PrivateFontCollection`), `.ttf`-Dateien als eingebettete Ressourcen
    im Projekt. Beide Fonts sind unter offenen Lizenzen (JetBrains Mono: Apache 2.0, Inter: SIL
    Open Font License) redistributionsfähig — Lizenzdateien mit ausliefern.
  - Fallback, falls Embedding fehlschlägt: `Consolas` (mono) / `Segoe UI` (sans)

## Offene Punkte — vor Umsetzung klären

1. Verbleibende offene Punkte aus `opencode-anweisung-settings-pgnummer.md` (`toolPosition`,
   `TV`, `MA`, `bauart`, `connectTimeout`, `DMC`, QoS/Retain, Persistenz der 10 Automation-Zeilen)
2. Neuer Projektname (Arbeitstitel "Betriebsmittel Publisher") und damit Ordnername für das
   neue Repo — endgültig festlegen
3. Soll die restliche Anwendung (Subscribe-Tab, Log-Tab, Verbindungsaufbau-UI, falls aus dem
   alten Projekt bekannt) ebenfalls Teil dieser neuen App sein, oder beschränkt sich der
   Funktionsumfang bewusst nur auf Einstellungen + PG-Nummer-Automation? Eine MQTT-Verbindung
   (host/port) muss in jedem Fall vorhanden sein, da Fenster 2 darauf zugreift.

## Akzeptanzkriterien

- [ ] Komplett neuer, eigenständiger Projektordner/Repo — keine Dateien aus dem bestehenden
      MQTT-Tester-Projekt wiederverwendet, keine Änderung am alten Repo
- [ ] Einstellungen- und Automation-Fenster funktional wie in
      `opencode-anweisung-settings-pgnummer.md` beschrieben (inkl. Motornummer-Feld,
      modul-Parsing aus Topic, host/port aus aktiver Verbindung)
- [ ] Visuelle Umsetzung folgt dem Design-System (Farben, Flat-Look, eingebettete Fonts)
      konsistent über beide Fenster
- [ ] Gleicher Build-/Offline-Standard wie das alte Projekt: kein NuGet, Single-File-.exe,
      kein Internet bei Build oder Betrieb nötig
