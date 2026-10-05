param(
    [Parameter(Mandatory = $true)]
    [string]$UnityEditorPath,
    [switch]$SkipPlayerBuild
)

$ErrorActionPreference = 'Stop'
$taskRepository = Split-Path -Parent $PSScriptRoot
$taskEditor = (Resolve-Path -LiteralPath $UnityEditorPath).Path
$taskRoot = Join-Path $taskRepository ('Library/GoogleSheetsReaderChecks/Run-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path "$taskRoot/Assets/Editor", "$taskRoot/Assets/Tool", "$taskRoot/Packages", "$taskRoot/ProjectSettings" | Out-Null
Copy-Item -LiteralPath "$taskRepository/Assets/01_Scripts/Data/GoogleSpeadSheetReader.cs" -Destination "$taskRoot/Assets/Tool/GoogleSpeadSheetReader.cs"
Copy-Item -LiteralPath "$PSScriptRoot/GoogleSpeadSheetReaderChecks.cs" -Destination "$taskRoot/Assets/Editor/GoogleSpeadSheetReaderChecks.cs"
Copy-Item -LiteralPath "$taskRepository/Assets/03_SideAssets/Google Sheets to Unity/Scripts" -Destination "$taskRoot/Assets/GSTU" -Recurse
Copy-Item -LiteralPath "$taskRepository/ProjectSettings/ProjectVersion.txt" -Destination "$taskRoot/ProjectSettings/ProjectVersion.txt"
Copy-Item -LiteralPath "$PSScriptRoot/Fixtures/BingoRoulette.json" -Destination "$taskRoot/fixture.json"
$taskSourceManifest = Get-Content -LiteralPath "$taskRepository/Packages/manifest.json" -Raw | ConvertFrom-Json
$taskModules = [ordered]@{}
$taskSourceManifest.dependencies.PSObject.Properties | Where-Object { $_.Name.StartsWith('com.unity.modules.') } | ForEach-Object { $taskModules[$_.Name] = $_.Value }
@{ dependencies = $taskModules } | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath "$taskRoot/Packages/manifest.json" -Encoding utf8

function Invoke-ReaderCheck([string]$Stage) {
    $taskLog = Join-Path $taskRoot "$Stage.log"
    $taskArguments = @('-batchmode', '-nographics', '-quit', '-projectPath', ('"' + $taskRoot + '"'),
        '-executeMethod', 'GoogleSpeadSheetReaderChecks.Run', '-gstuCheckStage', $Stage, '-logFile', ('"' + $taskLog + '"'))
    $taskProcess = Start-Process -FilePath $taskEditor -ArgumentList $taskArguments -WindowStyle Hidden -PassThru
    $taskProcess.WaitForExit()
    $taskProcess.Refresh()
    $taskResult = Join-Path $taskRoot "checks-$Stage.json"
    if ($taskProcess.ExitCode -ne 0 -or !(Test-Path -LiteralPath $taskResult)) {
        throw "Check '$Stage' failed. Log: $taskLog"
    }
    $taskSummary = Get-Content -LiteralPath $taskResult -Raw | ConvertFrom-Json
    if (!$taskSummary.success) { throw "Check '$Stage' failed. Log: $taskLog" }
    Write-Output "$Stage : $($taskSummary.assertions) assertions passed"
}

Invoke-ReaderCheck 'missing'
Set-Content -LiteralPath "$taskRoot/Assets/GstuCheckFixtureEnum.cs" -Encoding utf8 -Value 'namespace GstuCheckFixture { public enum ESoundType { BGM = 0, UI = 1, SFX = 2 } }'
Invoke-ReaderCheck 'generate'
Invoke-ReaderCheck 'import'
Invoke-ReaderCheck 'reload'
if (!$SkipPlayerBuild) { Invoke-ReaderCheck 'player' }
Write-Output "Isolated check artifacts: $taskRoot"
