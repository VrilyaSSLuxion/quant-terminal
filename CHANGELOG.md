# Changelog

All notable changes to this Windows edition. The upstream project is Quant Terminal 1.0.0 (MIT).

## v1.1.0 – 2026-10-27

### Added

- `Quant Terminal.exe`: a small Windows launcher that starts the local server and opens the app in an Edge/Chrome app window.
- Settings → **Academy banner** → **Hide** for a permanent hide. The `×` button still hides it for 7 days.
- **Drag drawings** with the cursor tool: rectangles, trend lines, rays, channels, fib retracements and the other tools can be moved after they are on the chart.
- German guide (`README-DE.md`) with install, update, backup, Pine and standalone notes.
- Versioned releases: `main` is the newest state, every release keeps its own source ZIP and Windows ZIP.

### Fixed

- Firefox: ad blockers blocked `/lib/quant.js` because the name looked like a tracker. It is now `quant-checks.js`.

### Checks

- 196 offline tests pass.
- Data sources reachable: Binance, Coinbase, Kraken, Nasdaq, Yahoo, Forex Factory.
- Live smoke test passes.

## v1.0.0 – upstream base

- Quant Terminal 1.0.0 by Lewis Jackson, MIT licence.
- 87 built-in indicators, 13 drawing tools, alerts, screener, heatmap, strategy tester, bar replay and paper trading.
- Chart engine: TradingView Lightweight Charts (Apache 2.0).
- Upstream: https://part-timequant.com/
