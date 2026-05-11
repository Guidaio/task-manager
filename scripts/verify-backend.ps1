param(
    [switch]$SkipTest
)

# Run from any directory. Builds backend solution; runs tests unless -SkipTest.
# Integration tests need SQL (see README Quick start). Stop TaskManager.Api if MSB3027 occurs.

$ErrorActionPreference = 'Stop'
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
Set-Location $repoRoot

Write-Host "Repository root: $repoRoot"
Write-Host 'dotnet build .\backend\TaskManager.sln'
dotnet build .\backend\TaskManager.sln
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

if (-not $SkipTest) {
  Write-Host 'dotnet test .\backend\TaskManager.sln'
  dotnet test .\backend\TaskManager.sln
  exit $LASTEXITCODE
}

Write-Host 'Skipping tests (-SkipTest). Done.'
exit 0
