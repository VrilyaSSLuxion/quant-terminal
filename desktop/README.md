# Windows launcher

`Quant Terminal.exe` is a small launcher, not a full standalone app.

It looks for a running Quant Terminal server on ports 3777–3797, starts `server.js` with Node.js if none is running, then opens the app in an Edge/Chrome app window. When that window closes, the launcher stops the server again if it started it.

Requirements:

- Windows
- Node.js 18 or newer
- Microsoft Edge or Google Chrome

## Rebuild

Close the running app window first (the exe is locked while it runs), then in PowerShell:

```powershell
powershell -ExecutionPolicy Bypass -File .\desktop\build.ps1
```

The build uses the C# compiler that ships with Windows. It downloads nothing and needs no admin rights. The icon is generated automatically if `desktop/icon.ico` is missing.

## Why not a full standalone app?

A real standalone build (Electron or Tauri) would bundle the browser engine and Node runtime. That makes the download much larger and adds update, signing and security work. The launcher keeps the download tiny and updates instantly with the source files. See `README-DE.md` for the trade-offs.
