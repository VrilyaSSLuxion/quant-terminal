# Quant Terminal – Windows Edition

Deutsche Anleitung für diese inoffizielle Community-Edition.

## Was ist das?

Quant Terminal ist ein Trading-Terminal für den eigenen PC: Charts, 87 Indikatoren, Zeichenwerkzeuge, Alerts, Screener, Strategie-Tester, Bar-Replay und Paper-Trading. Kein Konto, kein Abo, keine Cloud.

Diese Edition ist ein Community-Build von Lewis Jackson. Sie ist **nicht** die offizielle Version und gehört nicht zu Part-Time Quant. Das Original und die MIT-Lizenz stehen unten.

## Schnellstart unter Windows

1. **Node.js 18 oder neuer** installieren: [nodejs.org](https://nodejs.org) (die LTS-Version reicht).
2. Auf der [Releases-Seite](https://github.com/VrilyaSSLuxion/quant-terminal/releases) die neueste Datei `Quant-Terminal-vX.Y.Z-Windows.zip` herunterladen.
3. Die ZIP in einen eigenen Ordner entpacken, zum Beispiel `C:\Users\DeinName\QuantTerminal`.
4. **`Quant Terminal.exe`** doppelklicken.
5. Es öffnet sich ein eigenes Edge-Fenster ohne Adressleiste. Beim ersten Start die Watchlist einrichten.

Ohne exe: `start.bat` doppelklicken oder im Ordner `npm start` ausführen.

Wichtig: Die exe ist nur ein kleiner Starthelfer. **Node.js und Edge/Chrome müssen installiert sein.** Eine wirklich eigenständige App ohne Node ist ein separater Schritt (siehe unten).

## Wo sind meine Daten?

Alles Persönliche liegt im Ordner `data/` neben der exe: Watchlists, Zeichnungen, Alerts, Paper-Konto und Einstellungen. Dieser Ordner ist absichtlich **nicht** in Git und wird bei Updates nicht überschrieben, solange du ihn behältst.

API-Keys (falls du welche nutzt) liegen in `data/feeds.json`. Niemals in Git hochladen. Die App sendet einen Key nur an den jeweiligen Datenanbieter.

## Versionen und Backups

- `main` ist immer der neueste Stand.
- Jede Version bekommt einen Git-Tag und eine GitHub Release, zum Beispiel `v1.1.0`.
- Alte Versionen bleiben als ZIP downloadbar. Es gibt keine doppelten `V2`-Ordner.
- **Update:** neue ZIP herunterladen, über den alten Programmordner entpacken, `data/` behalten.
- **Zurück:** alte ZIP von der Releases-Seite laden, Programmdateien ersetzen, `data/` behalten.

Aktuelle Version: **v1.1.0** (27.10.2026).

## Was ist in v1.1.0 neu?

- **Banner-Schalter:** Zahnrad → Settings → Academy banner → Hide. Der kleine × oben rechts versteckt den Banner nur für 7 Tage; der Schalter dauerhaft.
- **Zeichnungen verschieben:** Cursor-Werkzeug wählen, Zeichnung anklicken und mit gedrückter Maustaste ziehen. Funktioniert für Rechteck, Trendlinie, Ray, Kanal, Fib und die anderen Werkzeuge.
- **Windows-Launcher:** `Quant Terminal.exe` startet den lokalen Server und öffnet ein eigenes App-Fenster.
- **Firefox-Fix:** Die Datei `quant.js` heißt jetzt `quant-checks.js`, damit Werbeblocker sie nicht blockieren.
- 196 automatische Tests grün.

## Zeichnungen: Was geht, was noch nicht?

Momentan: auswählen, verschieben, löschen und Farbe ändern. Rechtecke und Linien bleiben an Zeit und Preis verankert.

Noch nicht wie TradingView: Größe an den Ecken ziehen, einzelne Punkte verschieben und Magnetmodus. Das können wir als Nächstes ergänzen.

## Eigene Indikatoren und Pine Script

Eigene Indikatoren sind JavaScript-Dateien in `custom/`. Anleitung: [`docs/custom-indicators.md`](docs/custom-indicators.md), Vorlage: [`docs/indicator-template.js`](docs/indicator-template.js).

Pine Script läuft nur auf TradingViews Servern. Es gibt keine öffentliche API, mit der man die Pine-Datenbank automatisch auslesen darf. Wir können Pine-Code übersetzen, wenn du den Code selbst hast oder er frei lizenziert ist. Einfache Sachen (SMA, RSI, Crosses, Alerts) gehen schnell; komplexe Sachen (Arrays, Objekte, `request.security`, Schleifen) brauchen Handarbeit. Die 87 eingebauten Indikatoren folgen den Pine-Formeln und sind gegen TradingView-Referenzwerte getestet.

## Fear & Greed und Liquidation Heatmap

Deine bestehenden Projekte lassen sich anbinden:

- **Variante A (einfach):** Das Terminal liest die veröffentlichten JSON-Daten deiner Fear-&-Greed-Seite und zeigt sie in einem eigenen Panel. Der Collector läuft weiter wie bisher.
- **Variante B (eng):** Der Collector läuft im Terminal mit und schreibt in `data/`. Dafür braucht er dieselben API-Keys wie dein jetziges Programm. Keys bleiben lokal.
- **Heatmap:** Als eigene Ansicht oder als Zonen im Chart, je nachdem, welche Daten deine Heatmap liefert.

Dafür braucht es die genaue Datenquelle: welche URL, welches JSON-Format, live oder stündlich. Dann baue ich das Panel.

## Volumenprofil und PbD-Signale

Das Terminal hat bereits Volumenprofil (sichtbarer Bereich), Session-Volumenprofil, POC und Value Area.

Die **PbD-Methode** von Tom Vorwald und Patrick Nill ist Kursmaterial. Ich kopiere keine kostenpflichtigen Inhalte. Öffentlich beschrieben sind: Volumenprofil im Wochen-/Tageschart, POC und Value Area, Marktprofil-Formen (D = Balance, P = Käufer, B = Verkäufer), Breakout aus der Balance und ein festes Risiko (oft 1 %).

Darauf aufbauend ist ein eigener Indikator möglich: Zonen (VAH/VAL/POC), Breakout-Pfeile, optionale Retest-Bestätigung und Entry/Stop/Ziel im Chart. Für die exakte PbD-Regel brauche ich deine Beschreibung oder ein Beispiel, damit die Signale nicht nur „ähnlich“ aussehen.

## Standalone-App: der Unterschied

Die jetzige `Quant Terminal.exe` ist ein **Launcher** (15 KB). Sie startet Node.js und öffnet Edge im App-Fenster. Vorteile: winzig, sofort aktualisiert, kein Installer. Nachteile: Node.js und Edge/Chrome müssen vorhanden sein.

Eine **vollständig eigenständige App** packt Node und Browser gleich mit:

- **Electron:** ca. 150–250 MB, einfacher Build, Auto-Update und Installer möglich.
- **Tauri:** ca. 10–30 MB, nutzt WebView2 (auf Windows 10/11 meist schon da), Build mit Rust.
- **Vorteile:** keine Node-Installation, ein Programm, native Menüs, Tray und Auto-Update.
- **Nachteile:** groß, mehr Sicherheitsupdates, Code-Signierung und Build-Tools nötig. Die Charts und Grenzen der App bleiben gleich; standalone macht sie nicht automatisch zu TradingView.

Ich baue das gern, aber erst nach deinem OK, weil dafür zusätzliche Build-Tools heruntergeladen werden.

## Lizenz und Credits

- MIT-Lizenz, Copyright 2026 Lewis Jackson. Siehe [`LICENSE`](LICENSE).
- Chart-Engine: TradingView Lightweight Charts, Apache 2.0, siehe [`NOTICE`](NOTICE).
- Nicht verbunden mit TradingView, Inc. oder Part-Time Quant.
- Keine Finanzberatung. Vergangene Ergebnisse sagen nichts über die Zukunft.

## Links

- Releases: https://github.com/VrilyaSSLuxion/quant-terminal/releases
- Quellcode: https://github.com/VrilyaSSLuxion/quant-terminal
- Original: https://part-timequant.com/
