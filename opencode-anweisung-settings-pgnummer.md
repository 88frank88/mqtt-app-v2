# Feature: Settings-Tab (Publishtopics) + PG-Nummer-Automation

## GSD-Einstieg

Empfohlen: `/gsd:execute-phase` — es sind mehrere zusammenhängende Komponenten betroffen
(neuer UI-Tab, lokale Konfig-Persistenz, erweiterte XML-Generierung, DataGridView-Logik
mit Button-Spalte). Falls als einzelner kleiner Task behandelt: `/gsd:quick`.

Nicht direkt am Repo arbeiten ohne GSD-Workflow (siehe CLAUDE.md / GSD Workflow Enforcement
im Projekt).

## Kontext

Projekt: **MQTT Tester** — .NET 10 / C# WinForms, offline-fähig, kein NuGet, Single-File-.exe.
Bestehende Struktur: `MqttTester.csproj`, `Program.cs`, `MqttClient.cs`, `MainForm.cs`,
`NuGet.Config`, `BUILD.bat`, `LIES_MICH.txt`. Automation-Tab existiert bereits (Feature-ID-Test
mit XML-Payload via `XDocument`, Namespace vermutlich
`http://www.de-gmbh.com/workDesc/data/execution`).

Diese Aufgabe erweitert die Anwendung um zwei Dinge:

1. Einen neuen **Settings-Tab** zur Pflege der 4 Publishtopics (Betriebsmittel 1–4).
2. Eine **PG-Nummer-Automation** im Automation-Tab mit übergeordnetem Motornummer-Feld, die
   pro Zeile ein Betriebsmittel-Topic auswählt, eine 8-stellige PG-Nummer ins Grund-XML
   einsetzt und per Button einzeln published.

## Anforderungen — Fenster 1: Tab "Einstellungen"

Neuer Tab in der bestehenden `TabControl` (gleiches Owner-Draw-Theme wie die anderen Tabs).

Felder (Label + TextBox je Zeile):

- Publishtopic Betriebsmittel 1
- Publishtopic Betriebsmittel 2
- Publishtopic Betriebsmittel 3
- Publishtopic Betriebsmittel 4

**Kein separates Stationsnummer-Feld mehr** — die Stationsnummer (`modul`-Attribut im XML)
wird automatisch aus dem gewählten Topic abgeleitet (Segment-Index 2, 0-basiert, nach Split an
`/`). Beispiel: Topic `0012/27/43/70027043/ZAE/SCR/Req` → `modul = "43"`. Siehe Abschnitt
"Grund-XML-Template" unten für Details.

Verhalten:

- Werte werden in einer neuen Klasse `SettingsManager.cs` gehalten (`Betriebsmittel1Topic` …
  `Betriebsmittel4Topic`).
- Persistenz lokal neben der .exe (z. B. `settings.ini`), **kein NuGet/JSON-Library** —
  einfaches Key=Value-Format, Parsing mit `StreamReader`/`StreamWriter` aus der BCL.
- Beim Start automatisch laden (falls Datei vorhanden), leere Felder wenn nicht.
- Beim Verlassen des Tabs oder per "Speichern"-Button sichern (Vorschlag: expliziter
  Speichern-Button, damit nichts versehentlich überschrieben wird).
- Einfache Validierung: Topic-Felder dürfen nicht leer sein, wenn sie im Automation-Tab
  referenziert werden sollen (Hinweis/Rotfärbung statt harter Sperre).

## Anforderungen — Fenster 2: Automation-Tab erweitern (PG-Nummer)

Neuer Bereich/Sub-Tab **oder** zusätzliche Sektion im bestehenden Automation-Tab (Vorschlag:
eigener Sub-Tab "PG-Nummer" innerhalb des Automation-Tabs, um bestehende Feature-ID-Logik
nicht zu vermischen).

**Übergeordnetes Feld (oberhalb der Tabelle, gilt für alle 10 Zeilen):**

- **Motornummer** (`motorNr` im XML) — TextBox, ein einziger Wert pro Fenster/Session, nicht
  pro Zeile. Wird bei jedem Publish in jeder Zeile ins `motorNr`-Attribut übernommen. Format im
  Beispiel 8-stellig, numerisch mit führenden Nullen (`"00000002"`) — Validierung analog zur
  PG-Nummer (8-stellig, nur Ziffern), zu bestätigen.

`DataGridView` mit **bis zu 10 Zeilen**, Spalten:

| Spalte | Typ | Beschreibung |
|---|---|---|
| Betriebsmittel | `DataGridViewComboBoxColumn` | Werte "Betriebsmittel 1" … "Betriebsmittel 4", gemappt auf das jeweilige Topic aus den Einstellungen |
| PG-Nummer | `DataGridViewTextBoxColumn` | Exakt 8-stellig, nur Ziffern |
| Publish | `DataGridViewButtonColumn` (oder `DataGridViewButtonCell` je Zeile) | Löst Publish für genau diese Zeile aus |

Validierung PG-Nummer (bei `CellValidating` bzw. vor Publish-Klick):

- Genau 8 Zeichen, ausschließlich `0-9`.
- Bei Verstoß: Zelle rot markieren, Publish-Button dieser Zeile blockieren (kein Exception-Popup).

Ablauf pro Zeilen-Klick auf "Publish":

1. Ausgewähltes Betriebsmittel (1–4) → zugehöriges Topic aus `SettingsManager` auflösen.
2. Wenn Topic leer → Fehlermeldung ("Topic für Betriebsmittel X ist nicht konfiguriert,
   siehe Einstellungen") statt Publish.
3. `modul` per Split des Topics an `/` ermitteln (Segment-Index 2, 0-basiert) —
   Beispiel: `0012/27/43/70027043/ZAE/SCR/Req` → `modul = "43"`.
4. XML nach dem bestätigten Template aufbauen (`XDocument`, siehe Abschnitt
   "Grund-XML-Template" unten):
   - `feature` = eingegebene PG-Nummer dieser Zeile
   - `modul` = aus Schritt 3
   - `id` = neue GUID (`Guid.NewGuid().ToString()`)
   - `requestTopic` **und** `responseTopic` = derselbe aufgelöste Topic-String aus Schritt 1
   - `host`/`port` = aus der bestehenden MQTT-Verbindungskonfiguration der App (Connect-Tab),
     **nicht** aus dem neuen Settings-Tab — dieselben Werte, mit denen die App aktuell
     verbunden ist
   - `motorNr` = Wert aus dem übergeordneten Motornummer-Feld (siehe oben)
5. Fertiges XML als String serialisieren, per bestehendem `MqttClient` auf den aufgelösten
   Topic publishen (QoS wie im bestehenden Publish-Tab Standard, i.d.R. QoS 0 oder 1 — an
   bestehendes Verhalten angleichen).
6. Ergebnis (Erfolg/Fehler) im Log-Tab protokollieren, inkl. Zeitstempel, Topic, PG-Nummer,
   Motornummer.

Die 10 Zeilen sind unabhängig voneinander — jede Zeile hat ihre eigene Auswahl, eigene
PG-Nummer und ihren eigenen Publish-Button, exakt wie im Beispiel:

```
1. Betriebsmittel 2 -> 51000023 -> [Publish]
2. Betriebsmittel 4 -> 51000245 -> [Publish]
3. Betriebsmittel 1 -> 51000123 -> [Publish]
4. Betriebsmittel 2 -> 51000989 -> [Publish]
```

## Grund-XML-Template (jetzt bekannt, verbindlich)

Root-Element bestätigt: `execution:TExecution`, Namespace
`http://www.de-gmbh.com/workDesc/data/execution` (wie in STACK.md vermutet). Das Dokument
deklariert zusätzlich ca. 25 weitere Namespaces (`mappingArticle`, `mail`, `table`, `layout`,
`parameterdefinition`, `pickHardware`, `balance`, `camera`, `markup`, `testrigxml`, `sound`,
`reports`, `scanner`, `rfid`, `browser`, `laser`, `bde`, `pickHardwareLaser`, `mappingAddress`,
`configuration`, `library`, `StateEngineConfig-1.0.0`, `DEDatabaseConfig`, `classpath`,
`modules` u. a.) — diese müssen beim Aufbau via `XDocument`/`XNamespace` alle mit ausgegeben
werden, auch wenn sie im konkreten Task nicht genutzt werden (sonst valideren nachgelagerte
Systeme das Dokument ggf. nicht).

Beispiel-Payload (reales Beispiel, vom User bereitgestellt):

```xml
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<execution:TExecution xmlns:mappingArtice="http://www.de-gmbh.com/mappingArticle" xmlns:mail="http://www.de-gmbh.com/mail" xmlns:table="http://www.de-gmbh.com/table" xmlns:layout="http://www.de-gmbh.com/layout" xmlns:parameterdefinition="http://www.de-gmbh.com/parameterdefinition" xmlns:pickHardware="http://www.de-gmbh.com/pickHardware" xmlns:balance="http://www.de-gmbh.com/balance" xmlns:camera="http://www.de-gmbh.com/camera" xmlns:ns9="http://www.de-gmbh.com/markup" xmlns:testrigxml="http://www.de-gmbh.com/testrigxml" xmlns:ns11="http://www.de-gmbh.com/sound" xmlns:reports="http://www.de-gmbh.com/reports" xmlns:scanner="http://www.de-gmbh.com/scanner" xmlns:rfid="http://www.de-gmbh.com/rfid" xmlns:ns15="http://www.de-gmbh.com/browser" xmlns:laser="http://www.de-gmbh.com/laser" xmlns:bde="http://www.de-gmbh.com/bde" xmlns:ns18="http://www.de-gmbh.com/pickHardwareLaser" xmlns:mappingAddress="http://www.de-gmbh.com/mappingAddress" xmlns:config="http://www.de-gmbh.com/configuration" xmlns:library="http://www.de-gmbh.com/library" xmlns:StateEngineConf="http://www.de-gmbh.com/StateEngineConfig-1.0.0" xmlns:dbConfig="http://www.de-gmbh.com/DEDatabaseConfig" xmlns:classpath="http://www.de-gmbh.com/classpath" xmlns:execution="http://www.de-gmbh.com/workDesc/data/execution" xmlns:module="http://www.de-gmbh.com/modules">
    <execution:tasks>
        <execution:task id="81b40f8b-c80d-4a50-8e75-36ffaad062ab" modul="43" toolPosition="1" feature="59000999">
            <execution:parameter name="QUITK" value="R"/>
            <execution:parameter name="requestTopic" value="0012/27/43/70027043/ZAE/SCR/Req"/>
            <execution:parameter name="TV" value="37191"/>
            <execution:parameter name="MA" value="0004808061"/>
            <execution:parameter name="bauart" value="2013"/>
            <execution:parameter name="port" value="1883"/>
            <execution:parameter name="host" value="127.0.0.1"/>
            <execution:parameter name="connectTimeout" value="10000"/>
            <execution:parameter name="responseTopic" value="0012/27/43/70027043/ZAE/SCR/Req"/>
            <execution:parameter name="DMC" value=""/>
            <execution:parameter name="motorNr" value="00000002"/>
        </execution:task>
    </execution:tasks>
</execution:TExecution>
```

**PG-Nummer-Einfügepunkt bestätigt:** Attribut `feature` am `<execution:task>`-Element
(`feature="59000999"` — 8-stellig, passt exakt zum Anforderungsformat). Für jede Publish-Aktion
ist dieses Attribut durch die in der Automation-Zeile eingegebene PG-Nummer zu ersetzen.

**`id`-Attribut**: GUID, pro Publish neu generieren (`Guid.NewGuid().ToString()`), nicht
wiederverwenden.

## Offene Punkte — vor Umsetzung klären

Folgendes ist jetzt **geklärt**:

- ✅ PG-Nummer → `feature`-Attribut
- ✅ Motornummer → `motorNr`-Attribut, neues übergeordnetes Feld in Fenster 2
- ✅ `modul` → wird aus dem Topic geparst (Segment-Index 2), kein separates Stationsnummer-Feld
  in den Einstellungen nötig
- ✅ `requestTopic`/`responseTopic` → beide erhalten denselben, aus den Einstellungen
  aufgelösten Betriebsmittel-Topic
- ✅ `host`/`port` → aus der bestehenden MQTT-Verbindungskonfiguration der App, nicht aus dem
  neuen Settings-Tab

Weiterhin offen:

1. `toolPosition` (im Beispiel `"1"`) — fester Wert, oder pro Betriebsmittel/Zeile
   unterschiedlich?
2. `TV`, `MA`, `bauart`, `connectTimeout` — wirken wie feste Testrig-Konstanten. Bleiben diese
   für alle Betriebsmittel/PG-Nummern identisch (dann im Code fest hinterlegen), oder variieren
   sie (dann Konfigurationsfelder nötig)?
3. `DMC` ist im Beispiel leer — bleibt das beim manuellen Publish über diese App immer leer?
4. QoS/Retain-Flag für diese Publishes — gleich wie bestehender Publish-Tab oder separat
   konfigurierbar?
5. Sollen die 10 Zeilen (Betriebsmittel + PG-Nummer) dauerhaft gespeichert werden, oder nur
   pro Session?

## Betroffene Dateien

- `MainForm.cs` — neuer Settings-Tab, DataGridView-Erweiterung im Automation-Tab
- `SettingsManager.cs` — **neu**, lokale Konfig laden/speichern
- `MqttClient.cs` — ggf. keine Änderung nötig, falls Publish-Methode bereits generisch genug ist
- Optional `XmlTemplateBuilder.cs` — **neu**, falls XML-Aufbau aus `MainForm.cs` ausgelagert
  werden soll (empfohlen für Testbarkeit)

## Akzeptanzkriterien

- [ ] Einstellungen-Tab zeigt 4 Topic-Felder (Betriebsmittel 1–4), Werte werden persistiert und
      beim nächsten Start wieder geladen. Kein separates Stationsnummer-Feld.
- [ ] Automation-Tab enthält übergeordnetes Motornummer-Feld plus Tabelle mit bis zu 10 Zeilen,
      jede Zeile unabhängig bedienbar.
- [ ] PG-Nummer- und Motornummer-Eingabe werden auf 8-stellig/numerisch validiert, fehlerhafte
      Eingabe verhindert Publish der betroffenen Zeile(n).
- [ ] Klick auf "Publish" einer Zeile sendet das korrekt zusammengesetzte XML (inkl. korrekt
      geparstem `modul`, `requestTopic`/`responseTopic`, `host`/`port` aus der aktiven
      Verbindung, `motorNr`) an das richtige Topic und protokolliert das Ergebnis im Log-Tab.
- [ ] Keine NuGet-Abhängigkeiten, weiterhin Single-File-.exe-Build ohne Internet.
