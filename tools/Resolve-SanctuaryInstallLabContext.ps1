param(
    [string] $InstallRoot = "",
    [string] $LabActorCmeId = "",
    [string] $ServiceIdentityId = "Sanctuary.Actual.ID",
    [string] $IdentityTemplateId = "SLI.Lisp.Industrial.CME.Template",
    [string] $SubjectCmeId = ""
)

$ErrorActionPreference = "Stop"

$repositoryRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($InstallRoot)) {
    $InstallRoot = Join-Path $repositoryRoot ".local\install"
}

function ConvertTo-CmeActualLabel {
    param([string] $CmeId)

    if ([string]::IsNullOrWhiteSpace($CmeId)) {
        return ""
    }

    if ($CmeId.EndsWith(".CME.ID")) {
        return "$($CmeId.Substring(0, $CmeId.Length - ".CME.ID".Length)).CME.Actual"
    }

    return "$CmeId.Actual"
}

$contextPath = Join-Path $InstallRoot "mos\lab-cme-context.json"
$context = $null
if (Test-Path -LiteralPath $contextPath -PathType Leaf) {
    $context = Get-Content -LiteralPath $contextPath -Raw | ConvertFrom-Json
}

$builtInServiceIdentityId = "Sanctuary.Actual.ID"
$builtInIdentityTemplateId = "SLI.Lisp.Industrial.CME.Template"

if ($null -ne $context) {
    if ([string]::IsNullOrWhiteSpace($SubjectCmeId) -and $context.telemetrySubjectCmeId) {
        $SubjectCmeId = [string] $context.telemetrySubjectCmeId
    }

    if (
        ([string]::IsNullOrWhiteSpace($ServiceIdentityId) -or $ServiceIdentityId -eq $builtInServiceIdentityId) -and
        $context.serviceIdentityId
    ) {
        $ServiceIdentityId = [string] $context.serviceIdentityId
    }

    if (
        ([string]::IsNullOrWhiteSpace($IdentityTemplateId) -or $IdentityTemplateId -eq $builtInIdentityTemplateId) -and
        $context.identityTemplateId
    ) {
        $IdentityTemplateId = [string] $context.identityTemplateId
    }

    if ([string]::IsNullOrWhiteSpace($LabActorCmeId) -and $context.labActorCmeId) {
        $LabActorCmeId = [string] $context.labActorCmeId
    }
}

if ([string]::IsNullOrWhiteSpace($ServiceIdentityId)) {
    $ServiceIdentityId = $builtInServiceIdentityId
}

if ([string]::IsNullOrWhiteSpace($IdentityTemplateId)) {
    $IdentityTemplateId = $builtInIdentityTemplateId
}

$labActorActualLabel = if ($null -ne $context -and $context.labActorActualLabel) {
    [string] $context.labActorActualLabel
} else {
    ConvertTo-CmeActualLabel -CmeId $LabActorCmeId
}

$telemetrySubjectActualLabel = if ($null -ne $context -and $context.telemetrySubjectActualLabel) {
    [string] $context.telemetrySubjectActualLabel
} else {
    ConvertTo-CmeActualLabel -CmeId $SubjectCmeId
}

$residueCapturePolicy = if ($null -ne $context -and $context.residueCapturePolicy) {
    [string] $context.residueCapturePolicy
} else {
    "undeclared-install-local-context"
}

$governanceSimulationBodies = @()
if ($null -ne $context -and $context.governanceSimulationBodies) {
    $governanceSimulationBodies = @($context.governanceSimulationBodies)
}

$matchesRequest = $true
if ($null -ne $context) {
    $matchesRequest =
        ((-not $context.labActorCmeId) -or ([string] $context.labActorCmeId) -eq $LabActorCmeId) -and
        ((-not $context.telemetrySubjectCmeId) -or ([string] $context.telemetrySubjectCmeId) -eq $SubjectCmeId) -and
        ((-not $context.serviceIdentityId) -or ([string] $context.serviceIdentityId) -eq $ServiceIdentityId) -and
        ((-not $context.identityTemplateId) -or ([string] $context.identityTemplateId) -eq $IdentityTemplateId)
}

[pscustomobject]@{
    Path = $contextPath
    Present = ($null -ne $context)
    Schema = if ($null -ne $context -and $context.schema) { [string] $context.schema } else { "project-sanctuary.install.lab-cme-context.v1" }
    Active = if ($null -ne $context -and $null -ne $context.active) { [bool] $context.active } else { $false }
    Scope = "install-local-only"
    MatchesRequest = $matchesRequest
    IsPreinstallDoctrine = $false
    LabActorCmeId = $LabActorCmeId
    LabActorActualLabel = $labActorActualLabel
    SubjectCmeId = $SubjectCmeId
    TelemetrySubjectCmeId = $SubjectCmeId
    TelemetrySubjectActualLabel = $telemetrySubjectActualLabel
    ServiceIdentityId = $ServiceIdentityId
    IdentityTemplateId = $IdentityTemplateId
    TemplateIdentityId = $IdentityTemplateId
    GovernanceSimulationBodies = $governanceSimulationBodies
    ResidueCapturePolicy = $residueCapturePolicy
    GelResidueIsCandidateOnly = $true
    TelemetryReturnIsSelfGelMutation = $false
    ToolUseAdmitsGel = $false
    ToolUseActivatesActual = $false
    ToolUseGrantsAuthority = $false
    ToolUseBindsModel = $false
    ToolUseCallsProvider = $false
    ToolUseAuthorizesExternalAction = $false
}
