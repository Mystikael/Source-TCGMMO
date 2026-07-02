# Real Unity batch compile + EditMode tests (Start-Process -Wait).
$root = Split-Path $PSScriptRoot -Parent
$scratch = $env:GOAL_SCRATCH
if (-not $scratch) { $scratch = Join-Path $env:TEMP 'grok-goal-49c05a47fd0a\implementer' }
New-Item -ItemType Directory -Force -Path $scratch | Out-Null
$compileLog = Join-Path $scratch 'unity-compile.log'
$clientLog = Join-Path $scratch 'client-tests.log'
$testResults = Join-Path $scratch 'editmode-results.xml'
$unity = 'D:\Unity\6000.5.1f1\Editor\Unity.exe'
Set-Location $root

if (-not (Test-Path $unity)) { exit 1 }

"Unity: $unity" | Set-Content $compileLog -Encoding utf8

$setupLog = Join-Path $scratch 'unity-batchsetup.log'
$setupArgs = @('-batchmode','-nographics','-quit','-projectPath',$root,'-executeMethod','SourceTCG.Editor.AlphaSceneSetup.BatchSetup','-logFile',$setupLog)
$p1 = Start-Process -FilePath $unity -ArgumentList $setupArgs -Wait -PassThru -NoNewWindow
Add-Content $compileLog "BatchSetup exit $($p1.ExitCode)"
if ($p1.ExitCode -ne 0) { exit 1 }

if (Test-Path $testResults) { Remove-Item $testResults -Force }
$editLog = Join-Path $scratch 'unity-editmode.log'
$testArgs = @('-batchmode','-nographics','-projectPath',$root,'-runTests','-testPlatform','EditMode','-assemblyNames','SourceTCG.Tests','-testResults',$testResults,'-logFile',$editLog)
$p2 = Start-Process -FilePath $unity -ArgumentList $testArgs -Wait -PassThru -NoNewWindow
Add-Content $compileLog "EditMode exit $($p2.ExitCode)"

if (Test-Path $editLog) { Get-Content $editLog -Tail 40 | Add-Content $compileLog }

if (-not (Test-Path $testResults)) {
    Add-Content $compileLog 'FAIL: no editmode-results.xml'
    exit 1
}

[xml]$xml = Get-Content $testResults
$run = $xml.'test-run'
$summary = "EditMode: $($run.passed)/$($run.total) passed, $($run.failed) failed"
Add-Content $compileLog $summary
Get-Content $editLog -Tail 60 | Set-Content $clientLog -Encoding utf8
Add-Content $clientLog $summary
Add-Content $clientLog 'Unity EditMode assembly: SourceTCG.Tests (HexResolver H3 plugin, HUD formatters, scene YAML wiring)'

if ([int]$run.total -eq 0 -or [int]$run.failed -gt 0 -or $p2.ExitCode -ne 0) { exit 1 }

Add-Content $compileLog 'Unity compile + EditMode tests PASS'
Write-Output 'PASS: Unity compile and EditMode tests'
exit 0