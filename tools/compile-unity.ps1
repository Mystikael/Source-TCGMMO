# Authoritative Unity gate: BatchSetup -> VerifyAlphaScenes -> EditMode tests.
$root = Split-Path $PSScriptRoot -Parent
$scratch = $env:GOAL_SCRATCH
if (-not $scratch) { $scratch = Join-Path $env:TEMP 'grok-goal-49c05a47fd0a\implementer' }
New-Item -ItemType Directory -Force -Path $scratch | Out-Null
$compileLog = Join-Path $scratch 'unity-compile.log'
$clientLog = Join-Path $scratch 'client-tests.log'
$testResults = Join-Path $scratch 'editmode-results.xml'
$unity = 'D:\Unity\6000.5.1f1\Editor\Unity.exe'
Set-Location $root

if (-not (Test-Path $unity)) {
    "FAIL: Unity not found at $unity" | Set-Content $compileLog -Encoding utf8
    exit 1
}

"Unity: $unity" | Set-Content $compileLog -Encoding utf8

# Remove hand-edited broken metas so Unity regenerates valid 32-char GUIDs.
$staleMetas = @(
    'Assets\Scenes\Bootstrap.unity.meta',
    'Assets\Scenes\WorldMap.unity.meta',
    'Assets\Scenes\Inventory.unity.meta',
    'Assets\Plugins\H3.meta'
)
foreach ($rel in $staleMetas) {
    $full = Join-Path $root $rel
    if (Test-Path $full) {
        Remove-Item $full -Force
        Add-Content $compileLog "Removed stale meta: $rel"
    }
}

function Invoke-UnityBatch([string]$method, [string]$logPath) {
    $args = @('-batchmode','-nographics','-quit','-projectPath',$root,'-executeMethod',$method,'-logFile',$logPath)
    return Start-Process -FilePath $unity -ArgumentList $args -Wait -PassThru -NoNewWindow
}

$setupLog = Join-Path $scratch 'unity-batchsetup.log'
$p1 = Invoke-UnityBatch 'SourceTCG.Editor.AlphaSceneSetup.BatchSetup' $setupLog
Add-Content $compileLog "BatchSetup exit $($p1.ExitCode)"
if ($p1.ExitCode -ne 0) { exit 1 }

$verifyLog = Join-Path $scratch 'unity-verify.log'
$pVerify = Invoke-UnityBatch 'SourceTCG.Editor.AlphaSceneSetup.VerifyAlphaScenes' $verifyLog
Add-Content $compileLog "VerifyAlphaScenes exit $($pVerify.ExitCode)"
if ($pVerify.ExitCode -ne 0) { exit 1 }

foreach ($log in @($setupLog, $verifyLog)) {
    if (Test-Path $log) {
        $bad = Select-String -Path $log -Pattern 'does not have a valid GUID' -SimpleMatch
        if ($bad) {
            Add-Content $compileLog "FAIL: invalid GUID reported in $log"
            exit 1
        }
    }
}

# Record post-Unity GUID wiring for evidence.
$buildSettings = Join-Path $root 'ProjectSettings\EditorBuildSettings.asset'
$metaPaths = @(
    'Assets\Scenes\Bootstrap.unity.meta',
    'Assets\Scenes\WorldMap.unity.meta',
    'Assets\Scenes\Inventory.unity.meta'
)
Add-Content $compileLog '--- scene GUID evidence ---'
foreach ($metaRel in $metaPaths) {
    $metaFull = Join-Path $root $metaRel
    if (-not (Test-Path $metaFull)) {
        Add-Content $compileLog "FAIL: missing $metaRel after BatchSetup"
        exit 1
    }
    $text = Get-Content $metaFull -Raw
    if ($text -notmatch 'guid: ([a-f0-9]{32})') {
        Add-Content $compileLog "FAIL: invalid GUID in $metaRel"
        exit 1
    }
    $metaGuid = $Matches[1]
    $sceneName = [System.IO.Path]::GetFileNameWithoutExtension(
        [System.IO.Path]::GetFileNameWithoutExtension($metaRel))
    $buildText = Get-Content $buildSettings -Raw
    if ($buildText -notmatch "path: Assets/Scenes/$sceneName\.unity[\s\S]*?guid: $metaGuid") {
        Add-Content $compileLog "FAIL: EditorBuildSettings GUID mismatch for $sceneName (meta=$metaGuid)"
        exit 1
    }
    Add-Content $compileLog "$sceneName meta/build GUID: $metaGuid"
}

if (Test-Path $testResults) { Remove-Item $testResults -Force }
$editLog = Join-Path $scratch 'unity-editmode.log'
$testArgs = @('-batchmode','-nographics','-projectPath',$root,'-runTests','-testPlatform','EditMode','-assemblyNames','SourceTCG.Tests','-testResults',$testResults,'-logFile',$editLog)
$p2 = Start-Process -FilePath $unity -ArgumentList $testArgs -Wait -PassThru -NoNewWindow
Add-Content $compileLog "EditMode exit $($p2.ExitCode)"

if (Test-Path $editLog) {
    $badEdit = Select-String -Path $editLog -Pattern 'does not have a valid GUID' -SimpleMatch
    if ($badEdit) {
        Add-Content $compileLog 'FAIL: invalid GUID in unity-editmode.log'
        exit 1
    }
    Get-Content $editLog -Tail 40 | Add-Content $compileLog
}

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
Add-Content $clientLog 'Unity EditMode: SourceTCG.Tests (H3, economy, scene load + build GUID wiring)'

if ([int]$run.total -eq 0 -or [int]$run.failed -gt 0 -or $p2.ExitCode -ne 0) { exit 1 }

Add-Content $compileLog 'Unity compile + VerifyAlphaScenes + EditMode PASS'
Write-Output 'PASS: Unity compile and EditMode tests'
exit 0