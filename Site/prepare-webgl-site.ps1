param(
  [string]$BuildRoot = "webgl/Build",
  [string]$WebGlRoot = "webgl",
  [string]$ManifestPath = "webgl/build-manifest.json",
  [string]$CompanyName = "LearningArchitect",
  [string]$ProductName = "LearningArchitect",
  [string]$ProductVersion = "1.0.0",
  [switch]$DisableDevicePixelRatioClamp,
  [switch]$KeepCompressedArtifactsOnly
)

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

function Resolve-InputPath {
  param(
    [Parameter(Mandatory = $true)]
    [string]$Value
  )

  if ([System.IO.Path]::IsPathRooted($Value)) {
    return [System.IO.Path]::GetFullPath($Value)
  }

  return [System.IO.Path]::GetFullPath((Join-Path $scriptDir $Value))
}

$resolvedBuildRoot = Resolve-InputPath -Value $BuildRoot
$resolvedWebGlRoot = Resolve-InputPath -Value $WebGlRoot
$resolvedManifestPath = Resolve-InputPath -Value $ManifestPath

if (-not (Test-Path -LiteralPath $resolvedBuildRoot)) {
  throw "Build root not found: $resolvedBuildRoot"
}

function Get-RelativeSitePath {
  param(
    [string]$AbsolutePath
  )

  $baseUri = New-Object System.Uri(($scriptDir.TrimEnd('\') + '\'))
  $targetUri = New-Object System.Uri($AbsolutePath)
  $relative = $baseUri.MakeRelativeUri($targetUri).ToString()
  return ($relative -replace "\\", "/")
}

function Find-RequiredFile {
  param(
    [string[]]$Patterns,
    [string]$Label,
    [string]$PreferredStem
  )

  $matches = @()
  foreach ($pattern in $Patterns) {
    $matches += Get-ChildItem -LiteralPath $resolvedBuildRoot -File -Filter $pattern
  }

  $matches = $matches |
    Sort-Object FullName -Unique |
    Sort-Object LastWriteTime -Descending

  if ($PreferredStem) {
    $preferred = $matches | Where-Object { $_.BaseName -like "$PreferredStem*" } | Select-Object -First 1
    if ($null -ne $preferred) {
      return $preferred
    }
  }

  if ($matches.Count -gt 0) {
    return $matches[0]
  }

  $patternSummary = $Patterns -join ", "
  throw "Unable to find $Label in $resolvedBuildRoot. Looked for: $patternSummary"
}

function Get-DecompressedOutputPath {
  param(
    [Parameter(Mandatory = $true)]
    [string]$SourcePath
  )

  if ($SourcePath.EndsWith(".unityweb", [System.StringComparison]::OrdinalIgnoreCase)) {
    return $SourcePath.Substring(0, $SourcePath.Length - ".unityweb".Length)
  }

  if ($SourcePath.EndsWith(".br", [System.StringComparison]::OrdinalIgnoreCase) -or $SourcePath.EndsWith(".gz", [System.StringComparison]::OrdinalIgnoreCase)) {
    return $SourcePath.Substring(0, $SourcePath.LastIndexOf('.'))
  }

  return $SourcePath
}

function Expand-CompressedFileIfNeeded {
  param(
    [Parameter(Mandatory = $true)]
    [System.IO.FileInfo]$File
  )

  $fullPath = $File.FullName
  $extension = [System.IO.Path]::GetExtension($fullPath).ToLowerInvariant()

  if ($KeepCompressedArtifactsOnly -or ($extension -ne ".br" -and $extension -ne ".gz" -and $extension -ne ".unityweb")) {
    return $File.Name
  }

  $targetPath = Get-DecompressedOutputPath -SourcePath $fullPath
  if ([System.StringComparer]::OrdinalIgnoreCase.Equals($targetPath, $fullPath)) {
    return $File.Name
  }

  $readStream = [System.IO.File]::OpenRead($fullPath)
  try {
    if ($extension -eq ".gz") {
      $decompressStream = New-Object System.IO.Compression.GZipStream($readStream, [System.IO.Compression.CompressionMode]::Decompress)
      try {
        $writeStream = [System.IO.File]::Create($targetPath)
        try {
          $decompressStream.CopyTo($writeStream)
        }
        finally {
          $writeStream.Dispose()
        }
      }
      finally {
        $decompressStream.Dispose()
      }
    } else {
      $nodeScript = @'
const fs = require("fs");
const zlib = require("zlib");
const sourcePath = process.argv[1];
const targetPath = process.argv[2];
const input = fs.readFileSync(sourcePath);
const output = zlib.brotliDecompressSync(input);
fs.writeFileSync(targetPath, output);
'@
      $escapedScript = [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($nodeScript))
      $nodeCommand = @"
const script = Buffer.from('$escapedScript','base64').toString('utf8');
eval(script);
"@
      & node -e $nodeCommand -- $fullPath $targetPath | Out-Null
      if ($LASTEXITCODE -ne 0) {
        throw "Node Brotli decompression failed for $($File.Name)"
      }
    }
  }
  finally {
    $readStream.Dispose()
  }

  return [System.IO.Path]::GetFileName($targetPath)
}

$loaderFile = Find-RequiredFile -Label "loader file" -Patterns @("*.loader.js")
$loaderStem = $loaderFile.BaseName -replace "\.loader$", ""
$dataFile = Find-RequiredFile -Label "data file" -Patterns @("*.data", "*.data.gz", "*.data.br", "*.data.unityweb") -PreferredStem $loaderStem
$frameworkFile = Find-RequiredFile -Label "framework file" -Patterns @("*.framework.js", "*.framework.js.gz", "*.framework.js.br", "*.framework.js.unityweb") -PreferredStem $loaderStem
$codeFile = Find-RequiredFile -Label "wasm file" -Patterns @("*.wasm", "*.wasm.gz", "*.wasm.br", "*.wasm.unityweb") -PreferredStem $loaderStem

$resolvedDataFileName = Expand-CompressedFileIfNeeded -File $dataFile
$resolvedFrameworkFileName = Expand-CompressedFileIfNeeded -File $frameworkFile
$resolvedCodeFileName = Expand-CompressedFileIfNeeded -File $codeFile

$streamingAssetsPath = Join-Path $resolvedWebGlRoot "StreamingAssets"
$streamingAssetsRelative = if (Test-Path -LiteralPath $streamingAssetsPath) {
  "./" + (Get-RelativeSitePath -AbsolutePath $streamingAssetsPath)
} else {
  "./webgl/StreamingAssets"
}

$manifest = [ordered]@{
  enabled = $true
  webglRoot = "./" + (Get-RelativeSitePath -AbsolutePath $resolvedWebGlRoot)
  buildRoot = "./" + (Get-RelativeSitePath -AbsolutePath $resolvedBuildRoot)
  loaderFile = $loaderFile.Name
  dataFile = $resolvedDataFileName
  frameworkFile = $resolvedFrameworkFileName
  codeFile = $resolvedCodeFileName
  streamingAssetsUrl = $streamingAssetsRelative
  companyName = $CompanyName
  productName = $ProductName
  productVersion = $ProductVersion
  matchWebGLToCanvasSize = $true
  devicePixelRatio = if ($DisableDevicePixelRatioClamp) { $null } else { 1 }
}

if (-not $DisableDevicePixelRatioClamp) {
  $manifest.devicePixelRatio = 1
} else {
  $manifest.Remove("devicePixelRatio")
}

$manifestDirectory = Split-Path -Parent $resolvedManifestPath
if (-not (Test-Path -LiteralPath $manifestDirectory)) {
  New-Item -ItemType Directory -Path $manifestDirectory | Out-Null
}

$manifest | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $resolvedManifestPath -Encoding utf8

Write-Host "WebGL site manifest written:" -ForegroundColor Green
Write-Host "  $resolvedManifestPath"
Write-Host ""
Write-Host "Detected files:" -ForegroundColor Cyan
Write-Host "  Loader    $($loaderFile.Name)"
Write-Host "  Data      $resolvedDataFileName"
Write-Host "  Framework $resolvedFrameworkFileName"
Write-Host "  Wasm      $resolvedCodeFileName"
Write-Host ""
Write-Host "Next step:" -ForegroundColor Yellow
Write-Host "  Serve the Site folder and open index.html. The page will auto-discover this manifest."
