param(
    [string] $InstallRoot = "",
    [string] $IntakeRoot = "",
    [string] $OperatorName = "Operator",
    [string] $CmeId = "",
    [string] $ServiceIdentityId = "Sanctuary.Actual.ID",
    [string] $IdentityTemplateId = "SLI.Lisp.Industrial.CME.Template",
    [string] $SubjectCmeId = "",
    [string] $ThreadBindingId = "",
    [string] $Domain = "Lab",
    [string] $Role = "IndustrialCME",
    [string] $JobClass = "ColdBench",
    [switch] $PromptForCmeIdentity,
    [switch] $UseIndustrialCore,
    [switch] $Json,
    [switch] $NoBuild
)

$ErrorActionPreference = "Stop"

$toolPath = Join-Path $PSScriptRoot "Invoke-SanctuaryTool.ps1"

& $toolPath `
    -Command verify-closed-gates `
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
    -Json:$Json `
    -NoBuild:$NoBuild
