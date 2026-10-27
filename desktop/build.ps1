# Builds "Quant Terminal.exe" from launcher.cs using the C# compiler that ships with Windows.
# No downloads, no npm, no admin rights.
$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$root = Split-Path -Parent $here
$icon = Join-Path $here "icon.ico"
$exe = Join-Path $root "Quant Terminal.exe"

if (-not (Test-Path $icon)) {
    Add-Type -AssemblyName System.Drawing
    $size = 256
    $bmp = New-Object System.Drawing.Bitmap $size, $size
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.Clear([System.Drawing.Color]::Transparent)
    $r = 56
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $path.AddArc(0, 0, $r * 2, $r * 2, 180, 90)
    $path.AddArc($size - $r * 2, 0, $r * 2, $r * 2, 270, 90)
    $path.AddArc($size - $r * 2, $size - $r * 2, $r * 2, $r * 2, 0, 90)
    $path.AddArc(0, $size - $r * 2, $r * 2, $r * 2, 90, 90)
    $path.CloseFigure()
    $brush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(41, 98, 255))
    $g.FillPath($brush, $path)
    $pen = New-Object System.Drawing.Pen ([System.Drawing.Color]::White), 24
    $pen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $pen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $pen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
    $points = @(
        (New-Object System.Drawing.Point 58, 172),
        (New-Object System.Drawing.Point 104, 116),
        (New-Object System.Drawing.Point 144, 148),
        (New-Object System.Drawing.Point 198, 76)
    )
    $g.DrawLines($pen, $points)
    $g.Dispose()
    $png = Join-Path $env:TEMP "qt-icon.png"
    $bmp.Save($png, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    $bytes = [System.IO.File]::ReadAllBytes($png)
    $ms = New-Object System.IO.MemoryStream
    $bw = New-Object System.IO.BinaryWriter($ms)
    $bw.Write([UInt16]0); $bw.Write([UInt16]1); $bw.Write([UInt16]1)
    $bw.Write([Byte]0); $bw.Write([Byte]0); $bw.Write([Byte]0); $bw.Write([Byte]0)
    $bw.Write([UInt16]1); $bw.Write([UInt16]32)
    $bw.Write([UInt32]$bytes.Length); $bw.Write([UInt32]22)
    $bw.Write($bytes)
    $bw.Flush()
    [System.IO.File]::WriteAllBytes($icon, $ms.ToArray())
    $bw.Dispose(); $ms.Dispose(); $brush.Dispose(); $pen.Dispose(); $path.Dispose()
    Remove-Item $png -Force -ErrorAction SilentlyContinue
}

$csc = Join-Path $env:WINDIR "Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (-not (Test-Path $csc)) { $csc = Join-Path $env:WINDIR "Microsoft.NET\Framework\v4.0.30319\csc.exe" }
if (-not (Test-Path $csc)) { throw "Windows C# compiler (csc.exe) not found." }

& $csc /nologo /target:winexe /optimize+ "/out:$exe" "/win32icon:$icon" `
    /r:System.dll /r:System.Core.dll /r:System.Web.Extensions.dll `
    /r:System.Windows.Forms.dll /r:System.Drawing.dll `
    (Join-Path $here "launcher.cs")
if ($LASTEXITCODE -ne 0) { throw "Compiling failed (exit $LASTEXITCODE)." }
Write-Host "Built: $exe"
