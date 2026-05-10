param(
    [switch]$SkipSite
)

$ErrorActionPreference = "Stop"

Write-Host "==> dotnet build"
dotnet build .\LearningArchitect.sln

if (-not $SkipSite) {
    $siteCopyTool = Join-Path $PSScriptRoot "Site\scripts\site-copy-tool.mjs"
    if (Test-Path $siteCopyTool) {
        $node = Get-Command node -ErrorAction SilentlyContinue
        if ($node) {
            Write-Host "==> rebuild site copy payload"
            Push-Location (Join-Path $PSScriptRoot "Site")
            try {
                node .\scripts\site-copy-tool.mjs build-json
            }
            finally {
                Pop-Location
            }
        }
        else {
            Write-Warning "Node.js is not available. Skipping Site/content runtime JSON rebuild."
        }
    }
}

Write-Host ""
Write-Host "Next Unity validation steps:"
Write-Host "  1. Tools/LearningArchitect/Validate Showcase Configuration"
Write-Host "  2. Run EditMode tests"
Write-Host "  3. Run PlayMode smoke tests if runtime wiring changed"
