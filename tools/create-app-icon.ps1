Add-Type -AssemblyName System.Drawing

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$outDir = Join-Path $root "resources"
$outFile = Join-Path $outDir "app.ico"
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

function New-RoundedRectPath {
    param(
        [float]$X,
        [float]$Y,
        [float]$Width,
        [float]$Height,
        [float]$Radius
    )

    $path = [System.Drawing.Drawing2D.GraphicsPath]::new()
    $d = $Radius * 2
    $path.AddArc($X, $Y, $d, $d, 180, 90)
    $path.AddArc($X + $Width - $d, $Y, $d, $d, 270, 90)
    $path.AddArc($X + $Width - $d, $Y + $Height - $d, $d, $d, 0, 90)
    $path.AddArc($X, $Y + $Height - $d, $d, $d, 90, 90)
    $path.CloseFigure()
    return $path
}

function New-IconPngBytes {
    param([int]$Size)

    $bitmap = [System.Drawing.Bitmap]::new($Size, $Size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.Clear([System.Drawing.Color]::Transparent)

    $scale = $Size / 256.0
    $bg = New-RoundedRectPath (12 * $scale) (12 * $scale) (232 * $scale) (232 * $scale) (54 * $scale)
    $bgBrush = [System.Drawing.Drawing2D.LinearGradientBrush]::new(
        [System.Drawing.RectangleF]::new(0, 0, $Size, $Size),
        [System.Drawing.Color]::FromArgb(255, 19, 24, 34),
        [System.Drawing.Color]::FromArgb(255, 40, 53, 71),
        45
    )
    $graphics.FillPath($bgBrush, $bg)

    $ringPen = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(255, 45, 212, 191), [Math]::Max(5, 18 * $scale))
    $ringPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $ringPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $graphics.DrawArc($ringPen, 58 * $scale, 48 * $scale, 140 * $scale, 140 * $scale, 210, 300)

    $accentBrush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255, 125, 211, 252))
    $coreBrush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255, 255, 255, 255))
    $shadowBrush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(70, 0, 0, 0))

    $nodes = @(
        @(75, 82),
        @(184, 93),
        @(127, 178)
    )
    foreach ($node in $nodes) {
        $graphics.FillEllipse($shadowBrush, ($node[0] - 17) * $scale, ($node[1] - 13) * $scale, 34 * $scale, 34 * $scale)
        $graphics.FillEllipse($accentBrush, ($node[0] - 14) * $scale, ($node[1] - 14) * $scale, 28 * $scale, 28 * $scale)
    }

    $linePen = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(220, 255, 255, 255), [Math]::Max(3, 10 * $scale))
    $linePen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $linePen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $graphics.DrawLine($linePen, 82 * $scale, 92 * $scale, 126 * $scale, 164 * $scale)
    $graphics.DrawLine($linePen, 176 * $scale, 102 * $scale, 132 * $scale, 164 * $scale)

    $mPen = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(255, 248, 250, 252), [Math]::Max(4, 16 * $scale))
    $mPen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
    $mPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $mPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $graphics.DrawLines($mPen, [System.Drawing.PointF[]]@(
        [System.Drawing.PointF]::new(70 * $scale, 188 * $scale),
        [System.Drawing.PointF]::new(70 * $scale, 135 * $scale),
        [System.Drawing.PointF]::new(104 * $scale, 168 * $scale),
        [System.Drawing.PointF]::new(138 * $scale, 135 * $scale),
        [System.Drawing.PointF]::new(138 * $scale, 188 * $scale)
    ))

    $graphics.Dispose()
    $bgBrush.Dispose()
    $ringPen.Dispose()
    $accentBrush.Dispose()
    $coreBrush.Dispose()
    $shadowBrush.Dispose()
    $linePen.Dispose()
    $mPen.Dispose()
    $bg.Dispose()

    $stream = [System.IO.MemoryStream]::new()
    $bitmap.Save($stream, [System.Drawing.Imaging.ImageFormat]::Png)
    $bitmap.Dispose()
    $bytes = $stream.ToArray()
    $stream.Dispose()
    return ,$bytes
}

$sizes = @(16, 24, 32, 48, 64, 128, 256)
$images = foreach ($size in $sizes) {
    [PSCustomObject]@{
        Size = $size
        Bytes = New-IconPngBytes -Size $size
    }
}

$writerStream = [System.IO.MemoryStream]::new()
$writer = [System.IO.BinaryWriter]::new($writerStream)
$writer.Write([UInt16]0)
$writer.Write([UInt16]1)
$writer.Write([UInt16]$images.Count)

$offset = 6 + (16 * $images.Count)
foreach ($image in $images) {
    $writer.Write([byte]($(if ($image.Size -eq 256) { 0 } else { $image.Size })))
    $writer.Write([byte]($(if ($image.Size -eq 256) { 0 } else { $image.Size })))
    $writer.Write([byte]0)
    $writer.Write([byte]0)
    $writer.Write([UInt16]1)
    $writer.Write([UInt16]32)
    $writer.Write([UInt32]$image.Bytes.Length)
    $writer.Write([UInt32]$offset)
    $offset += $image.Bytes.Length
}

foreach ($image in $images) {
    $writer.Write($image.Bytes)
}

$writer.Flush()
[System.IO.File]::WriteAllBytes($outFile, $writerStream.ToArray())
$writer.Dispose()
$writerStream.Dispose()

Write-Host "Created $outFile"
