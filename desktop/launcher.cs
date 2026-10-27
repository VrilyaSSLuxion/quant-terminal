using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;

// Small Windows launcher for Quant Terminal.
// Starts the local Node server if needed, then opens the app in its own
// Edge/Chrome app window (no tabs, no address bar). Closing the window stops
// the server again if this launcher started it.
class Program
{
    const string AppName = "Quant Terminal";
    const string MutexName = "Local\\QuantTerminalLauncher";
    static readonly string Root = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
    static Process serverProc;
    static bool weStartedServer;

    [DllImport("user32.dll")]
    static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [STAThread]
    static void Main()
    {
        bool created;
        using (var mutex = new Mutex(true, MutexName, out created))
        {
            if (!created)
            {
                ActivateExisting();
                return;
            }
            try
            {
                Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Quant Terminal konnte nicht gestartet werden.\n\n" + ex.Message,
                    AppName,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            GC.KeepAlive(mutex);
        }
    }

    static void Run()
    {
        string server = Path.Combine(Root, "server.js");
        if (!File.Exists(server))
        {
            MessageBox.Show(
                "Bitte lege \"Quant Terminal.exe\" in den quant-terminal-Ordner (neben server.js).",
                AppName,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        int port = FindServer();
        if (port == 0)
        {
            string node = FindNode();
            if (node == null)
            {
                MessageBox.Show(
                    "Node.js wurde nicht gefunden.\n\nBitte installiere Node.js 18 oder neuer von https://nodejs.org und starte die App danach erneut.",
                    AppName,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            port = StartServer(node, server, FindFreePort());
            if (port == 0)
            {
                MessageBox.Show(
                    "Der lokale Server ist nicht gestartet.\n\nDetails stehen in: data\\launcher.log",
                    AppName,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            weStartedServer = true;
        }

        OpenWindow(port);

        if (weStartedServer && serverProc != null && !serverProc.HasExited)
        {
            try { serverProc.Kill(); } catch { }
        }
    }

    static int FindServer()
    {
        for (int p = 3777; p < 3798; p++)
        {
            if (!PortOpen(p)) continue;
            var j = Health(p);
            if (j != null && SameRoot(j)) return p;
        }
        return 0;
    }

    static bool PortOpen(int port)
    {
        try
        {
            using (var c = new TcpClient())
            {
                var task = c.ConnectAsync("127.0.0.1", port);
                return task.Wait(100) && c.Connected;
            }
        }
        catch { return false; }
    }

    static int FindFreePort()
    {
        for (int p = 3777; p < 3798; p++)
        {
            try
            {
                var listener = new TcpListener(IPAddress.Loopback, p);
                listener.Start();
                listener.Stop();
                return p;
            }
            catch { }
        }
        return 3777;
    }

    static Dictionary<string, object> Health(int port)
    {
        try
        {
            var req = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:" + port + "/api/health");
            req.Proxy = null;
            req.Timeout = 700;
            req.ReadWriteTimeout = 700;
            req.KeepAlive = false;
            using (var resp = (HttpWebResponse)req.GetResponse())
            using (var sr = new StreamReader(resp.GetResponseStream()))
            {
                return new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(sr.ReadToEnd());
            }
        }
        catch { return null; }
    }

    static bool SameRoot(Dictionary<string, object> j)
    {
        object app, root;
        if (!j.TryGetValue("app", out app) || !j.TryGetValue("root", out root)) return false;
        if (Convert.ToString(app) != "quant-terminal") return false;
        try
        {
            return string.Equals(
                Path.GetFullPath(Convert.ToString(root)).TrimEnd('\\'),
                Root,
                StringComparison.OrdinalIgnoreCase);
        }
        catch { return false; }
    }

    static string FindNode()
    {
        string pathVar = Environment.GetEnvironmentVariable("PATH") ?? "";
        foreach (var dir in pathVar.Split(';'))
        {
            try
            {
                string f = Path.Combine(dir.Trim(), "node.exe");
                if (File.Exists(f)) return f;
            }
            catch { }
        }
        string[] candidates = new string[]
        {
            @"C:\Program Files\nodejs\node.exe",
            @"C:\Program Files (x86)\nodejs\node.exe",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\nodejs\node.exe")
        };
        foreach (var c in candidates)
            if (File.Exists(c)) return c;
        return null;
    }

    static int StartServer(string node, string server, int port)
    {
        var psi = new ProcessStartInfo(node, "\"" + server + "\"");
        psi.WorkingDirectory = Root;
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;
        psi.EnvironmentVariables["PORT"] = port.ToString();

        string dataDir = Path.Combine(Root, "data");
        Directory.CreateDirectory(dataDir);
        string logPath = Path.Combine(dataDir, "launcher.log");

        serverProc = new Process();
        serverProc.StartInfo = psi;
        serverProc.OutputDataReceived += (s, e) => Log(logPath, e.Data);
        serverProc.ErrorDataReceived += (s, e) => Log(logPath, e.Data);
        serverProc.Start();
        serverProc.BeginOutputReadLine();
        serverProc.BeginErrorReadLine();

        for (int i = 0; i < 100; i++)
        {
            Thread.Sleep(300);
            if (serverProc.HasExited) return 0;
            var j = Health(port);
            if (j != null && SameRoot(j)) return port;
        }
        return 0;
    }

    static void Log(string path, string line)
    {
        if (line == null) return;
        try
        {
            lock (typeof(Program))
            {
                File.AppendAllText(path, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + line + Environment.NewLine);
            }
        }
        catch { }
    }

    static void OpenWindow(int port)
    {
        string exe = FindBrowser();
        string url = "http://localhost:" + port;
        string profile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "QuantTerminal",
            "window");
        Directory.CreateDirectory(profile);

        if (exe != null)
        {
            string args = "--app=" + url +
                " --user-data-dir=\"" + profile + "\"" +
                " --window-size=1600,1000" +
                " --no-first-run --no-default-browser-check --disable-background-mode" +
                " --disable-features=Translate,TranslateUI";
            var psi = new ProcessStartInfo(exe, args);
            psi.UseShellExecute = false;
            var p = Process.Start(psi);
            if (p != null) p.WaitForExit();
        }
        else
        {
            // No Edge/Chrome found: fall back to the default browser.
            Process.Start(url);
        }
    }

    static string FindBrowser()
    {
        string[] candidates = new string[]
        {
            @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
            @"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
            @"C:\Program Files\Google\Chrome\Application\chrome.exe",
            @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Google\Chrome\Application\chrome.exe")
        };
        foreach (var c in candidates)
            if (File.Exists(c)) return c;
        return null;
    }

    static void ActivateExisting()
    {
        string[] names = new string[] { "msedge", "chrome" };
        foreach (var name in names)
        {
            foreach (var p in Process.GetProcessesByName(name))
            {
                try
                {
                    if (p.MainWindowTitle == AppName)
                    {
                        ShowWindow(p.MainWindowHandle, 9);
                        SetForegroundWindow(p.MainWindowHandle);
                        return;
                    }
                }
                catch { }
            }
        }
    }
}
