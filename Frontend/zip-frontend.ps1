# ============================================
# Zip Frontend Project - exclude bin & obj
# ============================================

$src = $PSScriptRoot
$parentDir = (Get-Item $src).Parent.FullName
$timestamp = Get-Date -Format "yyyyMMdd"
$dest = Join-Path $parentDir "Frontend_$timestamp.zip"

$excludeDirs = @("bin", "obj")

Write-Host ""
Write-Host "============================================"
Write-Host " Zip Frontend Project"
Write-Host "============================================"
Write-Host ""
Write-Host "Source : $src"
Write-Host "Output : $dest"
Write-Host "Exclude: $($excludeDirs -join ', ') folders"
Write-Host ""

Write-Host "Scanning files..." -ForegroundColor Gray

# Get all files, excluding those under bin/obj directories
$files = Get-ChildItem -LiteralPath $src -Recurse -File | Where-Object {
    $skip = $false
    $path = $_.FullName
    foreach ($ex in $excludeDirs) {
        if ($path -match "\\($ex)\\" -or $path -match "/$ex/") {
            $skip = $true
            break
        }
    }
    -not $skip
}

if ($files.Count -eq 0) {
    Write-Host "[ERROR] No files found to zip." -ForegroundColor Red
    exit 1
}

Write-Host ("Found " + $files.Count + " files") -ForegroundColor Gray

if (Test-Path $dest) {
    Write-Host "Removing existing zip..." -ForegroundColor Yellow
    Remove-Item $dest -Force
}

Write-Host "Creating archive..." -ForegroundColor Gray

Add-Type -AssemblyName "System.IO.Compression"
Add-Type -AssemblyName "System.IO.Compression.FileSystem"

# ZipFile.Open returns ZipArchive
$zip = [System.IO.Compression.ZipFile]::Open($dest, [System.IO.Compression.ZipArchiveMode]::Create)
$count = 0
$total = $files.Count

foreach ($file in $files) {
    # Compute relative path within the zip
    $entryName = $file.FullName.Substring($src.Length).TrimStart("\").Replace("\", "/")
    # Use ZipFileExtensions to create entry from file
    $entry = $zip.CreateEntry($entryName, [System.IO.Compression.CompressionLevel]::Optimal)
    $bytes = [System.IO.File]::ReadAllBytes($file.FullName)
    $stream = $entry.Open()
    $stream.Write($bytes, 0, $bytes.Length)
    $stream.Dispose()
    $count++
    if ($count % 100 -eq 0) {
        Write-Host ("  $count/$total files...") -ForegroundColor Gray
    }
}

$zip.Dispose()

Write-Host ("Done! $count files archived.") -ForegroundColor Green

$size = (Get-Item $dest).Length
if ($size -gt 1MB) {
    $sizeStr = [math]::Round($size / 1MB, 2)
    Write-Host ("Size: $sizeStr MB") -ForegroundColor Cyan
} else {
    $sizeStr = [math]::Round($size / 1KB, 1)
    Write-Host ("Size: $sizeStr KB") -ForegroundColor Cyan
}

Write-Host ""
Write-Host "============================================"
Write-Host " Zip completed successfully!"
Write-Host "============================================"
Write-Host ""
