param(
    [string] $InstallRoot = "",
    [string] $IntakeRoot = "",
    [string] $OperatorName = "Operator",
    [string] $CmeId = "",
    [string] $ServiceIdentityId = "Sanctuary.Actual.ID",
    [string] $IdentityTemplateId = "SLI.Lisp.Industrial.CME.Template",
    [string] $SubjectCmeId = "",
    [string] $Domain = "Lab",
    [string] $Role = "IndustrialCME",
    [string] $JobClass = "ColdBench",
    [string] $ThreadBindingId = "",
    [int] $TelemetryIntervalSeconds = 60,
    [int] $ColdCheckIntervalSeconds = 3600,
    [int] $MaxHeartbeats = 1,
    [int] $MaxColdChecks = 0,
    [switch] $PromptForCmeIdentity,
    [switch] $UseIndustrialCore,
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
    -ServiceIdentityId $ServiceIdentityId `
    -IdentityTemplateId $IdentityTemplateId `
    -SubjectCmeId $SubjectCmeId `
    -ThreadBindingId $ThreadBindingId `
    -PromptForCmeIdentity:$PromptForCmeIdentity `
    -UseIndustrialCore:$UseIndustrialCore `
    -Domain $Domain `
    -Role $Role `
    -JobClass $JobClass `
    -HeartbeatSeconds $TelemetryIntervalSeconds `
    -Json:$Json `
    -NoBuild:$NoBuild
