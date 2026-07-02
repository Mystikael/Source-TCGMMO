# Runs shipped client pure-logic NUnit tests (hex, haversine, HUD formatters, catalog).
$root = if ($PSScriptRoot) { Split-Path $PSScriptRoot -Parent } else { Get-Location }
Set-Location $root

$dotnetTest = dotnet test tests/ClientLogicTests/ClientLogic.Tests.csproj 2>&1
$dotnetTest | ForEach-Object { Write-Output $_ }
if ($LASTEXITCODE -ne 0) {
    Write-Output 'FAIL: dotnet NUnit client logic tests'
    exit 1
}

Write-Output 'PASS: client logic NUnit tests (hex, proximity, HUD formatters, catalog)'
exit 0