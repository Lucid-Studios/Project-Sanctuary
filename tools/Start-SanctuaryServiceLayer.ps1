param(
    [string] $InstallRoot = "",
    [string] $IntakeRoot = "",
    [string] $OperatorName = "Operator",
    [string] $CmeId = "Codex.CME.ID",
    [string] $Domain = "Lab",
    [string] $Role = "IndustrialCME",
    [string] $JobClass = "ColdBench",
    [int] $TelemetryIntervalSeconds = 60,
    [int] $ColdCheckIntervalSeconds = 3600,
    [int] $MaxHeartbeats = 1,
    [int] $MaxColdChecks = 0,
    [switch] $Json,
    [switch] $NoBuild
)

$ErrorActionPreference = "Stop"

$toolPath = Join-Path $PSScriptRoot "Invoke-SanctuaryTool.ps1"

& $toolPath `
    -Command service-heartbeat `
    -InstallRoot $InstallRoot `
    -IntakeRoot $IntakeRoot `
    -OperatorName $OperatorName `
    -CmeId $CmeId `
    -Domain $Domain `
    -Role $Role `
    -JobClass $JobClass `
    -HeartbeatSeconds $TelemetryIntervalSeconds `
    -Json:$Json `
    -NoBuild:$NoBuild
