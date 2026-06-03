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

function Get-ContextString {
    param(
        [object] $Source,
        [string] $Name
    )

    if ($null -eq $Source) {
        return ""
    }

    $property = $Source.PSObject.Properties[$Name]
    if ($null -eq $property -or $null -eq $property.Value) {
        return ""
    }

    return [string] $property.Value
}

function ConvertTo-ResearchLane {
    param([object] $Lane)

    if ($null -eq $Lane) {
        return $null
    }

    if ($Lane -is [string]) {
        $laneCmeId = [string] $Lane
        return [pscustomobject]@{
            contextUniverseId = $laneCmeId
            laneKind = "research"
            cmeId = $laneCmeId
            actualLabel = ConvertTo-CmeActualLabel -CmeId $laneCmeId
            domain = ""
            role = ""
            jobClass = ""
            threadBindingId = ""
            posture = "declared-install-local-research-lane"
        }
    }

    $laneCmeId = Get-ContextString -Source $Lane -Name "cmeId"
    if ([string]::IsNullOrWhiteSpace($laneCmeId)) {
        return $null
    }

    $actualLabel = Get-ContextString -Source $Lane -Name "actualLabel"
    if ([string]::IsNullOrWhiteSpace($actualLabel)) {
        $actualLabel = ConvertTo-CmeActualLabel -CmeId $laneCmeId
    }

    [pscustomobject]@{
        contextUniverseId = Get-ContextString -Source $Lane -Name "contextUniverseId"
        laneKind = Get-ContextString -Source $Lane -Name "laneKind"
        cmeId = $laneCmeId
        actualLabel = $actualLabel
        domain = Get-ContextString -Source $Lane -Name "domain"
        role = Get-ContextString -Source $Lane -Name "role"
        jobClass = Get-ContextString -Source $Lane -Name "jobClass"
        threadBindingId = Get-ContextString -Source $Lane -Name "threadBindingId"
        posture = Get-ContextString -Source $Lane -Name "posture"
    }
}

function ConvertTo-ContextUniverse {
    param([object] $Universe)

    if ($null -eq $Universe) {
        return $null
    }

    if ($Universe -is [string]) {
        return [pscustomobject]@{
            contextUniverseId = [string] $Universe
            contextKind = ""
            domain = ""
            posture = ""
        }
    }

    [pscustomobject]@{
        contextUniverseId = Get-ContextString -Source $Universe -Name "contextUniverseId"
        contextKind = Get-ContextString -Source $Universe -Name "contextKind"
        domain = Get-ContextString -Source $Universe -Name "domain"
        posture = Get-ContextString -Source $Universe -Name "posture"
    }
}

function ConvertTo-GovernanceAgent {
    param([object] $Agent)

    if ($null -eq $Agent) {
        return $null
    }

    if ($Agent -is [string]) {
        return [pscustomobject]@{
            agentId = [string] $Agent
            displayName = [string] $Agent
            organ = ""
            coat = ""
            hat = ""
            ownsOeSelfGelResidue = $false
        }
    }

    $agentId = Get-ContextString -Source $Agent -Name "agentId"
    if ([string]::IsNullOrWhiteSpace($agentId)) {
        return $null
    }

    $ownsResidue = $false
    $ownsResidueProperty = $Agent.PSObject.Properties["ownsOeSelfGelResidue"]
    if ($null -ne $ownsResidueProperty -and $null -ne $ownsResidueProperty.Value) {
        $ownsResidue = [bool] $ownsResidueProperty.Value
    }

    [pscustomobject]@{
        agentId = $agentId
        displayName = Get-ContextString -Source $Agent -Name "displayName"
        organ = Get-ContextString -Source $Agent -Name "organ"
        coat = Get-ContextString -Source $Agent -Name "coat"
        hat = Get-ContextString -Source $Agent -Name "hat"
        ownsOeSelfGelResidue = $ownsResidue
    }
}

$contextPath = Join-Path $InstallRoot "mos\lab-cme-context.json"
$context = $null
if (Test-Path -LiteralPath $contextPath -PathType Leaf) {
    $context = Get-Content -LiteralPath $contextPath -Raw | ConvertFrom-Json
}

$builtInServiceIdentityId = "Sanctuary.Actual.ID"
$builtInIdentityTemplateId = "SLI.Lisp.Industrial.CME.Template"
$requestedLabActorCmeId = $LabActorCmeId
$requestedSubjectCmeId = $SubjectCmeId
$baseLabActorCmeId = ""
$baseTelemetrySubjectCmeId = ""
$researchLanes = @()
$contextUniverses = @()
$governanceAgents = @()

if ($null -ne $context) {
    if ($context.labActorCmeId) {
        $baseLabActorCmeId = [string] $context.labActorCmeId
    }

    if ($context.telemetrySubjectCmeId) {
        $baseTelemetrySubjectCmeId = [string] $context.telemetrySubjectCmeId
    }

    if ($context.researchLanes) {
        foreach ($lane in @($context.researchLanes)) {
            $convertedLane = ConvertTo-ResearchLane -Lane $lane
            if ($null -ne $convertedLane) {
                $researchLanes += $convertedLane
            }
        }
    }

    if ($context.contextUniverses) {
        foreach ($universe in @($context.contextUniverses)) {
            $convertedUniverse = ConvertTo-ContextUniverse -Universe $universe
            if ($null -ne $convertedUniverse -and -not [string]::IsNullOrWhiteSpace($convertedUniverse.contextUniverseId)) {
                $contextUniverses += $convertedUniverse
            }
        }
    }

    if ($context.governanceAgents) {
        foreach ($agent in @($context.governanceAgents)) {
            $convertedAgent = ConvertTo-GovernanceAgent -Agent $agent
            if ($null -ne $convertedAgent) {
                $governanceAgents += $convertedAgent
            }
        }
    }

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

if ([string]::IsNullOrWhiteSpace($baseLabActorCmeId)) {
    $baseLabActorCmeId = $LabActorCmeId
}

if ([string]::IsNullOrWhiteSpace($baseTelemetrySubjectCmeId)) {
    $baseTelemetrySubjectCmeId = $SubjectCmeId
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
    ConvertTo-CmeActualLabel -CmeId $baseLabActorCmeId
}

$telemetrySubjectActualLabel = if ($null -ne $context -and $context.telemetrySubjectActualLabel) {
    [string] $context.telemetrySubjectActualLabel
} else {
    ConvertTo-CmeActualLabel -CmeId $baseTelemetrySubjectCmeId
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

$activeLane = $null
foreach ($lane in $researchLanes) {
    if (
        (-not [string]::IsNullOrWhiteSpace($LabActorCmeId) -and [string] $lane.cmeId -eq $LabActorCmeId) -or
        (-not [string]::IsNullOrWhiteSpace($SubjectCmeId) -and [string] $lane.cmeId -eq $SubjectCmeId)
    ) {
        $activeLane = $lane
        break
    }
}

$activeParticipantCmeId = if ($null -ne $activeLane) {
    [string] $activeLane.cmeId
} else {
    $LabActorCmeId
}

$activeParticipantActualLabel = if ($null -ne $activeLane -and -not [string]::IsNullOrWhiteSpace($activeLane.actualLabel)) {
    [string] $activeLane.actualLabel
} else {
    ConvertTo-CmeActualLabel -CmeId $activeParticipantCmeId
}

$activeContextUniverseId = if ($null -ne $activeLane -and -not [string]::IsNullOrWhiteSpace($activeLane.contextUniverseId)) {
    [string] $activeLane.contextUniverseId
} elseif (-not [string]::IsNullOrWhiteSpace($activeParticipantCmeId)) {
    $activeParticipantCmeId
} else {
    "lab"
}

$activeLaneKind = if ($null -ne $activeLane -and -not [string]::IsNullOrWhiteSpace($activeLane.laneKind)) {
    [string] $activeLane.laneKind
} else {
    "base-lab"
}

$baseActorMatchesRequest = [string]::IsNullOrWhiteSpace($requestedLabActorCmeId) -or
    [string]::IsNullOrWhiteSpace($baseLabActorCmeId) -or
    $baseLabActorCmeId -eq $LabActorCmeId
$baseSubjectMatchesRequest = [string]::IsNullOrWhiteSpace($requestedSubjectCmeId) -or
    [string]::IsNullOrWhiteSpace($baseTelemetrySubjectCmeId) -or
    $baseTelemetrySubjectCmeId -eq $SubjectCmeId
$serviceAndTemplateMatch =
    ($null -eq $context -or (-not $context.serviceIdentityId) -or ([string] $context.serviceIdentityId) -eq $ServiceIdentityId) -and
    ($null -eq $context -or (-not $context.identityTemplateId) -or ([string] $context.identityTemplateId) -eq $IdentityTemplateId)

$matchesRequest = $true
if ($null -ne $context) {
    $matchesRequest = $serviceAndTemplateMatch -and (($baseActorMatchesRequest -and $baseSubjectMatchesRequest) -or ($null -ne $activeLane))
}

[pscustomobject]@{
    Path = $contextPath
    Present = ($null -ne $context)
    Schema = if ($null -ne $context -and $context.schema) { [string] $context.schema } else { "project-sanctuary.install.lab-cme-context.v1" }
    Active = if ($null -ne $context -and $null -ne $context.active) { [bool] $context.active } else { $false }
    Scope = "install-local-only"
    MatchesRequest = $matchesRequest
    IsPreinstallDoctrine = $false
    RequestedLabActorCmeId = $LabActorCmeId
    RequestedSubjectCmeId = $SubjectCmeId
    LabActorCmeId = $baseLabActorCmeId
    LabActorActualLabel = $labActorActualLabel
    BaseLabActorCmeId = $baseLabActorCmeId
    BaseLabActorActualLabel = $labActorActualLabel
    SubjectCmeId = $SubjectCmeId
    TelemetrySubjectCmeId = $baseTelemetrySubjectCmeId
    TelemetrySubjectActualLabel = $telemetrySubjectActualLabel
    BaseTelemetrySubjectCmeId = $baseTelemetrySubjectCmeId
    BaseTelemetrySubjectActualLabel = $telemetrySubjectActualLabel
    ActiveContextUniverseId = $activeContextUniverseId
    ActiveContextLaneKind = $activeLaneKind
    ActiveParticipantCmeId = $activeParticipantCmeId
    ActiveParticipantActualLabel = $activeParticipantActualLabel
    ActiveParticipantLaneDeclared = ($null -ne $activeLane)
    ActiveParticipantThreadBindingId = if ($null -ne $activeLane) { [string] $activeLane.threadBindingId } else { "" }
    ActiveParticipantDomain = if ($null -ne $activeLane) { [string] $activeLane.domain } else { "" }
    ActiveParticipantRole = if ($null -ne $activeLane) { [string] $activeLane.role } else { "" }
    ActiveParticipantJobClass = if ($null -ne $activeLane) { [string] $activeLane.jobClass } else { "" }
    ResearchLanes = $researchLanes
    ContextUniverses = $contextUniverses
    GovernanceAgents = $governanceAgents
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
