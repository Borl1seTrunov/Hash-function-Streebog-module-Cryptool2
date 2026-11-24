param(
    [string]$Ct2Dir = 'C:\Program Files\CrypTool 2',
    [switch]$Install,
    [switch]$InstallMachine
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$objDir = Join-Path $root 'obj'
$binDir = Join-Path $root 'bin'
New-Item -ItemType Directory -Force -Path $objDir, $binDir | Out-Null

$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path $csc)) {
    $csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
}
if (-not (Test-Path $csc)) {
    throw 'csc.exe (.NET Framework 4.x) was not found.'
}

$fwDir = Split-Path -Parent $csc
$wpfDir = Join-Path $fwDir 'WPF'

$iconPath = Join-Path $root 'icon.png'
if (-not (Test-Path $iconPath)) {
    Add-Type -AssemblyName System.Drawing
    $bmp = New-Object System.Drawing.Bitmap 32, 32
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.Clear([System.Drawing.Color]::FromArgb(255, 30, 80, 160))
    $font = New-Object System.Drawing.Font('Segoe UI', 13, [System.Drawing.FontStyle]::Bold)
    $g.DrawString('ST', $font, [System.Drawing.Brushes]::White, 1, 6)
    $g.Dispose()
    $bmp.Save($iconPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    $font.Dispose()
}

$resourcePath = Join-Path $objDir 'Streebog.g.resources'
if (Test-Path $resourcePath) {
    Remove-Item $resourcePath -Force
}
$writer = New-Object System.Resources.ResourceWriter($resourcePath)

function Add-StreamResource($writer, $name, $filePath) {
    $bytes = [IO.File]::ReadAllBytes($filePath)
    $stream = New-Object System.IO.MemoryStream
    $stream.Write($bytes, 0, $bytes.Length)
    $stream.Position = 0
    $writer.AddResource($name, $stream)
}

Add-StreamResource $writer 'icon.png' $iconPath
Add-StreamResource $writer 'detaileddescription/doc.xml' (Join-Path $root 'DetailedDescription\doc.xml')
$writer.Close()

$outputPath = Join-Path $binDir 'Streebog.dll'
$compilerArgs = @(
    '/target:library',
    '/platform:anycpu',
    '/optimize+',
    "/out:$outputPath",
    "/resource:$resourcePath",
    "/reference:$(Join-Path $Ct2Dir 'CrypPluginBase.dll')",
    "/reference:$(Join-Path $wpfDir 'PresentationFramework.dll')",
    "/reference:$(Join-Path $wpfDir 'PresentationCore.dll')",
    "/reference:$(Join-Path $wpfDir 'WindowsBase.dll')",
    "/reference:$(Join-Path $fwDir 'System.Xaml.dll')",
    "/reference:$(Join-Path $fwDir 'System.dll')",
    "/reference:$(Join-Path $fwDir 'System.Core.dll')",
    (Join-Path $root 'Streebog.cs'),
    (Join-Path $root 'StreebogCore.cs'),
    (Join-Path $root 'StreebogTables.cs'),
    (Join-Path $root 'StreebogSettings.cs'),
    (Join-Path $root 'StreebogTraceStep.cs'),
    (Join-Path $root 'StreebogPresentation.cs'),
    (Join-Path $root 'Properties\AssemblyInfo.cs')
)

& $csc $compilerArgs
if ($LASTEXITCODE -ne 0) {
    throw "Compilation failed with exit code $LASTEXITCODE."
}

Write-Host "Built: $outputPath"

if ($Install) {
    $storeRoot = Join-Path $env:LOCALAPPDATA 'CrypTool2\CrypToolStore'
    $storeType = 'Release'
    if (-not (Test-Path (Join-Path $storeRoot $storeType)) -and
        (Test-Path (Join-Path $storeRoot 'Developer'))) {
        $storeType = 'Developer'
    }
    $pluginDir = Join-Path $storeRoot "$storeType\plugins"
    New-Item -ItemType Directory -Force -Path $pluginDir | Out-Null
    Copy-Item $outputPath (Join-Path $pluginDir 'Streebog.dll') -Force
    Write-Host "Installed (per user): $(Join-Path $pluginDir 'Streebog.dll')"
    Write-Host 'Restart CrypTool 2 to load the new component.'
}

if ($InstallMachine) {
    $pluginDir = Join-Path $Ct2Dir 'CrypPlugins'
    if (-not (Test-Path $pluginDir)) {
        throw "CrypPlugins folder not found: $pluginDir"
    }
    Copy-Item $outputPath (Join-Path $pluginDir 'Streebog.dll') -Force
    Write-Host "Installed (machine-wide): $(Join-Path $pluginDir 'Streebog.dll')"
    Write-Host 'Restart CrypTool 2 to load the new component.'
}
