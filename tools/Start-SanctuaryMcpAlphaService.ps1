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
    [string] $JobClass = "GptUseCaseAlpha",
    [string] $ThreadBindingId = "",
    [string] $SoulFrameId = "",
    [string] $AgentiCoreId = "",
    [string] $HostName = "127.0.0.1",
    [int] $Port = 8717,
    [int] $MaxRequests = 0,
    [switch] $PromptForCmeIdentity,
    [switch] $UseIndustrialCore,
    [switch] $NoBuild
)

$ErrorActionPreference = "Stop"

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repositoryRoot "src\Sanctuary.Cli\Sanctuary.Cli.csproj"
$exePath = Join-Path $repositoryRoot "src\Sanctuary.Cli\bin\Release\net8.0\Sanctuary.exe"

if ([string]::IsNullOrWhiteSpace($InstallRoot)) {
    $InstallRoot = Join-Path $repositoryRoot ".local\install"
}

if ([string]::IsNullOrWhiteSpace($IntakeRoot)) {
    $IntakeRoot = Join-Path $repositoryRoot ".local\intake"
}

$identityResolverPath = Join-Path $PSScriptRoot "Resolve-SanctuaryCmeIdentity.ps1"
$participantIdentityRequested =
    (-not [string]::IsNullOrWhiteSpace($CmeId)) -or
    $UseIndustrialCore -or
    $PromptForCmeIdentity
if ($participantIdentityRequested) {
    $identity = & $identityResolverPath `
        -InstallRoot $InstallRoot `
        -CmeId $CmeId `
        -UseIndustrialCore:$UseIndustrialCore `
        -Optional `
        -PromptIfNeeded:$PromptForCmeIdentity
} else {
    $identity = [pscustomobject]@{
        CmeId = ""
        CmeActualLabel = ""
        Source = "not-selected-service-only"
        Prompted = $false
        Selected = $false
        ParticipantIdentityPattern = "{Name}.CME.ID"
        CmeIdentityIsParticipant = $false
        ServiceIdentityId = "Sanctuary.Actual.ID"
        ServiceIdentityIsCme = $false
        CmeIdentityIsServiceIdentity = $false
        IdentityTemplateId = "SLI.Lisp.Industrial.CME.Template"
        IdentityTemplateIsIdentity = $false
        CmeIdentityIsTemplateIdentity = $false
        ThreadBindingId = ""
        DomainRole = ""
        SoulFrameId = ""
        AgentiCoreId = ""
        ToolUseAdmitsGel = $false
        ToolUseMutatesSelfGel = $false
        ToolUseActivatesActual = $false
        ToolUseGrantsAuthority = $false
        ToolUseBindsModel = $false
        ToolUseCallsProvider = $false
        ToolUseAuthorizesExternalAction = $false
    }
}
$CmeId = $identity.CmeId
if ([string]::IsNullOrWhiteSpace($ThreadBindingId)) {
    if (-not [string]::IsNullOrWhiteSpace($env:SANCTUARY_THREAD_BINDING_ID)) {
        $ThreadBindingId = $env:SANCTUARY_THREAD_BINDING_ID
    } elseif ($identity.ThreadBindingId) {
        $ThreadBindingId = $identity.ThreadBindingId
    }
}
if ([string]::IsNullOrWhiteSpace($SoulFrameId) -and $identity.SoulFrameId) {
    $SoulFrameId = $identity.SoulFrameId
}
if ([string]::IsNullOrWhiteSpace($AgentiCoreId) -and $identity.AgentiCoreId) {
    $AgentiCoreId = $identity.AgentiCoreId
}

$labContextResolverPath = Join-Path $PSScriptRoot "Resolve-SanctuaryInstallLabContext.ps1"
$labContext = & $labContextResolverPath `
    -InstallRoot $InstallRoot `
    -LabActorCmeId $CmeId `
    -ServiceIdentityId $ServiceIdentityId `
    -IdentityTemplateId $IdentityTemplateId `
    -SubjectCmeId $SubjectCmeId
$ServiceIdentityId = $labContext.ServiceIdentityId
$IdentityTemplateId = $labContext.IdentityTemplateId
if (-not [string]::IsNullOrWhiteSpace($CmeId)) {
    $SubjectCmeId = $labContext.SubjectCmeId
}
$participantCmeSelectedForServiceWake = -not [string]::IsNullOrWhiteSpace($CmeId)
$stateActiveContextUniverseId = if ($participantCmeSelectedForServiceWake) { $labContext.ActiveContextUniverseId } else { "" }
$stateActiveContextLaneKind = if ($participantCmeSelectedForServiceWake) { $labContext.ActiveContextLaneKind } else { "service-only" }
$stateActiveParticipantCmeId = if ($participantCmeSelectedForServiceWake) { $labContext.ActiveParticipantCmeId } else { "" }
$stateActiveParticipantActualLabel = if ($participantCmeSelectedForServiceWake) { $labContext.ActiveParticipantActualLabel } else { "" }
$stateActiveParticipantLaneDeclared = if ($participantCmeSelectedForServiceWake) { $labContext.ActiveParticipantLaneDeclared } else { $false }
$stateActiveParticipantThreadBindingId = if ($participantCmeSelectedForServiceWake) { $labContext.ActiveParticipantThreadBindingId } else { "" }
$stateActiveParticipantDomain = if ($participantCmeSelectedForServiceWake) { $labContext.ActiveParticipantDomain } else { "" }
$stateActiveParticipantRole = if ($participantCmeSelectedForServiceWake) { $labContext.ActiveParticipantRole } else { "" }
$stateActiveParticipantJobClass = if ($participantCmeSelectedForServiceWake) { $labContext.ActiveParticipantJobClass } else { "" }

if (-not $NoBuild) {
    dotnet build $projectPath -c Release | Out-Host
}

if (-not (Test-Path -LiteralPath $exePath -PathType Leaf)) {
    throw "Sanctuary executable not found at '$exePath'. Run without -NoBuild first."
}

$arguments = @(
    "serve-mcp",
    "--host", $HostName,
    "--port", $Port,
    "--install-root", $InstallRoot,
    "--intake-root", $IntakeRoot,
    "--operator-name", $OperatorName,
    "--service-id", $ServiceIdentityId,
    "--identity-template-id", $IdentityTemplateId,
    "--domain", $Domain,
    "--role", $Role,
    "--job-class", $JobClass
)

if (-not [string]::IsNullOrWhiteSpace($CmeId)) {
    $arguments += @("--cme-id", $CmeId)
}

if (-not [string]::IsNullOrWhiteSpace($SubjectCmeId)) {
    $arguments += @("--subject-cme-id", $SubjectCmeId)
}

if (-not [string]::IsNullOrWhiteSpace($ThreadBindingId)) {
    $arguments += @("--thread-binding-id", $ThreadBindingId)
}

if (-not [string]::IsNullOrWhiteSpace($SoulFrameId)) {
    $arguments += @("--soulframe-id", $SoulFrameId)
}

if (-not [string]::IsNullOrWhiteSpace($AgentiCoreId)) {
    $arguments += @("--agenticore-id", $AgentiCoreId)
}

if ($MaxRequests -gt 0) {
    $arguments += @("--max-requests", $MaxRequests)
}

$process = Start-Process `
    -FilePath $exePath `
    -ArgumentList $arguments `
    -PassThru `
    -WindowStyle Hidden

$stateRoot = Join-Path $InstallRoot "service"
$statePath = Join-Path $stateRoot "sanctuary-mcp-alpha-service.json"
New-Item -ItemType Directory -Path $stateRoot -Force | Out-Null
@{
    schema = "project-sanctuary.service.mcp-alpha-process.v1"
    startedAtUtc = (Get-Date).ToUniversalTime().ToString("O")
    processId = $process.Id
    executable = $exePath
    host = $HostName
    port = $Port
    installRoot = $InstallRoot
    cmeId = $CmeId
    defaultCallerCmeId = $CmeId
    identityTemplateId = $IdentityTemplateId
    subjectCmeId = $SubjectCmeId
    serviceIdentityId = $ServiceIdentityId
    serviceIdentityIsCme = $false
    participantCmeSelectedForServiceWake = $participantCmeSelectedForServiceWake
    installLocalCmeLaneDeclarationPath = $labContext.Path
    installLocalCmeLaneDeclarationPresent = $labContext.Present
    installLocalCmeLaneDeclarationScope = $labContext.Scope
    installLocalCmeLaneDeclarationMatchesRequest = $labContext.MatchesRequest
    installLocalCmeLaneDeclarationIsPreinstallDoctrine = $labContext.IsPreinstallDoctrine
    installLocalLabActorCmeId = $labContext.LabActorCmeId
    installLocalLabActorActualLabel = $labContext.LabActorActualLabel
    installLocalTelemetrySubjectCmeId = $labContext.TelemetrySubjectCmeId
    installLocalTelemetrySubjectActualLabel = $labContext.TelemetrySubjectActualLabel
    installLocalRequestedLabActorCmeId = $labContext.RequestedLabActorCmeId
    installLocalRequestedSubjectCmeId = $labContext.RequestedSubjectCmeId
    installLocalActiveContextUniverseId = $stateActiveContextUniverseId
    installLocalActiveContextLaneKind = $stateActiveContextLaneKind
    installLocalActiveParticipantCmeId = $stateActiveParticipantCmeId
    installLocalActiveParticipantActualLabel = $stateActiveParticipantActualLabel
    installLocalActiveParticipantLaneDeclared = $stateActiveParticipantLaneDeclared
    installLocalActiveParticipantThreadBindingId = $stateActiveParticipantThreadBindingId
    installLocalActiveParticipantDomain = $stateActiveParticipantDomain
    installLocalActiveParticipantRole = $stateActiveParticipantRole
    installLocalActiveParticipantJobClass = $stateActiveParticipantJobClass
    installLocalContextUniverses = @($labContext.ContextUniverses)
    installLocalResearchLanes = @($labContext.ResearchLanes)
    installLocalGovernanceAgents = @($labContext.GovernanceAgents)
    installLocalServiceIdentityId = $labContext.ServiceIdentityId
    installLocalTemplateIdentityId = $labContext.TemplateIdentityId
    installLocalGelResidueIsCandidateOnly = $labContext.GelResidueIsCandidateOnly
    installLocalTelemetryReturnIsSelfGelMutation = $labContext.TelemetryReturnIsSelfGelMutation
    installLocalToolUseAdmitsGel = $labContext.ToolUseAdmitsGel
    installLocalToolUseActivatesActual = $labContext.ToolUseActivatesActual
    installLocalToolUseGrantsAuthority = $labContext.ToolUseGrantsAuthority
    installLocalToolUseBindsModel = $labContext.ToolUseBindsModel
    installLocalToolUseCallsProvider = $labContext.ToolUseCallsProvider
    installLocalToolUseAuthorizesExternalAction = $labContext.ToolUseAuthorizesExternalAction
    cmeIdentitySource = $identity.Source
    cmeIdentityPrompted = $identity.Prompted
    defaultThreadBindingId = $ThreadBindingId
    defaultSoulFrameId = $SoulFrameId
    defaultAgentiCoreId = $AgentiCoreId
    serviceOwner = "Sanctuary.exe"
    posture = "cold-read-fetch-candidate-only"
} | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $statePath -Encoding UTF8

[pscustomobject]@{
    ProcessId = $process.Id
    Endpoint = "http://${HostName}:$Port/"
    StatePath = $statePath
}
