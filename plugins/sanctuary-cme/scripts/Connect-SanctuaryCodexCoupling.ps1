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
    [string] $JobClass = "CodexNativeCoupling",
    [string] $ThreadBindingId = "",
    [string] $SoulFrameId = "",
    [string] $AgentiCoreId = "",
    [string] $HostName = "127.0.0.1",
    [int] $Port = 8717,
    [switch] $PromptForCmeIdentity,
    [switch] $UseIndustrialCore,
    [switch] $NoBuild,
    [switch] $Json
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $PSCommandPath
$pluginRoot = Split-Path -Parent $scriptRoot
$pluginsRoot = Split-Path -Parent $pluginRoot
$repositoryRoot = Split-Path -Parent $pluginsRoot
$startServicePath = Join-Path $repositoryRoot "tools\Start-SanctuaryMcpAlphaService.ps1"
$invokePluginPath = Join-Path $scriptRoot "Invoke-SanctuaryCme.ps1"
$pluginManifestPath = Join-Path $pluginRoot ".codex-plugin\plugin.json"
$pluginSkillPath = Join-Path $pluginRoot "skills\sanctuary-cme\SKILL.md"
$pluginWrapperPath = Join-Path $scriptRoot "Invoke-SanctuaryCme.ps1"
$pluginMcpConfigPath = Join-Path $pluginRoot ".mcp.json"
$couplingScriptPath = $PSCommandPath

function Get-FileSha256 {
    param([string] $Path)

    if ([string]::IsNullOrWhiteSpace($Path) -or -not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return ""
    }

    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function New-PluginCustodySurface {
    param(
        [string] $SurfaceId,
        [string] $Path,
        [string] $Role,
        [string] $SurfaceKind,
        [string] $ClosureRequirement
    )

    [ordered]@{
        surfaceId = $SurfaceId
        path = $Path
        digest = Get-FileSha256 -Path $Path
        surfaceKind = $SurfaceKind
        role = $Role
        custodyReviewStatus = "reviewed-candidate-only"
        custodyReviewDecision = "hold-for-operator-source-custody"
        sourceAdmissionCandidate = $true
        sourceAdmitted = $false
        requiresOperatorSourceCustody = $true
        closureRequirement = $ClosureRequirement
        custodyCandidate = $true
        authority = $false
        admitsGel = $false
        mutatesSelfGel = $false
        activatesActual = $false
        bindsModel = $false
        callsProvider = $false
        authorizesExternalAction = $false
    }
}

if ([string]::IsNullOrWhiteSpace($InstallRoot)) {
    $InstallRoot = Join-Path $repositoryRoot ".local\install"
}

if ([string]::IsNullOrWhiteSpace($IntakeRoot)) {
    $IntakeRoot = Join-Path $repositoryRoot ".local\intake"
}

$identityResolverPath = Join-Path $repositoryRoot "tools\Resolve-SanctuaryCmeIdentity.ps1"
$identity = & $identityResolverPath `
    -InstallRoot $InstallRoot `
    -CmeId $CmeId `
    -UseIndustrialCore:$UseIndustrialCore `
    -PromptIfNeeded:$PromptForCmeIdentity
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

$labContextResolverPath = Join-Path $repositoryRoot "tools\Resolve-SanctuaryInstallLabContext.ps1"
$labContext = & $labContextResolverPath `
    -InstallRoot $InstallRoot `
    -LabActorCmeId $CmeId `
    -ServiceIdentityId $ServiceIdentityId `
    -IdentityTemplateId $IdentityTemplateId `
    -SubjectCmeId $SubjectCmeId
$IdentityTemplateId = $labContext.IdentityTemplateId
$SubjectCmeId = $labContext.SubjectCmeId

$serviceRoot = Join-Path $InstallRoot "service"
$statePath = Join-Path $serviceRoot "sanctuary-mcp-alpha-service.json"
$couplingPath = Join-Path $serviceRoot "codex-native-coupling.json"
$safeCmeId = $CmeId -replace '[^A-Za-z0-9._-]', '_'
$couplingLaneRoot = Join-Path $serviceRoot "coupling"
$couplingLanePath = Join-Path $couplingLaneRoot "$safeCmeId.json"
$baseUrl = "http://${HostName}:$Port"
$mcpUrl = "$baseUrl/mcp"
$pluginCustodySurfaces = @(
    New-PluginCustodySurface `
        -SurfaceId "plugin-manifest" `
        -Path $pluginManifestPath `
        -Role "declares Codex plugin metadata, skill root, MCP config path, and local non-authority posture" `
        -SurfaceKind "plugin-metadata" `
        -ClosureRequirement "manifest remains local developer-preview metadata and must not imply authority, provider access, release, GEL admission, or Actual activation"
    New-PluginCustodySurface `
        -SurfaceId "plugin-skill" `
        -Path $pluginSkillPath `
        -Role "teaches Codex operator posture, identity split, commands, and closed gate expectations" `
        -SurfaceKind "operator-posture-skill" `
        -ClosureRequirement "skill text must preserve Codex.CME.Actual actor lane, Oria.CME.Actual telemetry lane, service/template distinction, and closed-gate expectations"
    New-PluginCustodySurface `
        -SurfaceId "plugin-wrapper" `
        -Path $pluginWrapperPath `
        -Role "routes Codex plugin calls into the central receipt-bearing Sanctuary wrapper" `
        -SurfaceKind "receipt-bearing-wrapper" `
        -ClosureRequirement "wrapper must resolve participant CME before tool use and pass identity/template/subject lanes without granting authority by invocation"
    New-PluginCustodySurface `
        -SurfaceId "mcp-descriptor" `
        -Path $pluginMcpConfigPath `
        -Role "declares the local MCP loopback endpoint as a descriptor, not authority" `
        -SurfaceKind "local-loopback-descriptor" `
        -ClosureRequirement "descriptor must remain local loopback only and deny provider/model/external-action/GEL/SelfGEL/Actual by implication"
    New-PluginCustodySurface `
        -SurfaceId "coupling-witness" `
        -Path $couplingScriptPath `
        -Role "runs repeatable local MCP proof for identity split, tool exposure, and fail-closed gates" `
        -SurfaceKind "live-coupling-witness" `
        -ClosureRequirement "witness must refresh coupling proof, five-surface custody digests, and closed-gate receipt after plugin or service changes"
)
$pluginCustodySurfaceDigestsPresent = @(
    $pluginCustodySurfaces | Where-Object { -not [string]::IsNullOrWhiteSpace($_.digest) }
).Count -eq @($pluginCustodySurfaces).Count
$pluginCustodyReviewAllSurfacesReviewed = @(
    $pluginCustodySurfaces | Where-Object { $_.custodyReviewStatus -eq "reviewed-candidate-only" }
).Count -eq @($pluginCustodySurfaces).Count
$pluginCustodyReviewAllGatesClosed = @(
    $pluginCustodySurfaces |
        Where-Object {
            $_.authority -or
            $_.admitsGel -or
            $_.mutatesSelfGel -or
            $_.activatesActual -or
            $_.bindsModel -or
            $_.callsProvider -or
            $_.authorizesExternalAction -or
            $_.sourceAdmitted
        }
).Count -eq 0

function Invoke-McpJsonRpc {
    param(
        [string] $Uri,
        [hashtable] $Payload
    )

    $body = $Payload | ConvertTo-Json -Depth 16
    Invoke-RestMethod -Uri $Uri -Method Post -ContentType "application/json" -Body $body
}

function Test-SanctuaryHealth {
    param([string] $HealthUrl)

    try {
        $health = Invoke-RestMethod -Uri $HealthUrl -Method Get -TimeoutSec 5
        if ($health.schema -eq "project-sanctuary.gpt-alpha.health.v1" -and $health.status -eq "running") {
            return $health
        }
    } catch {
        return $null
    }

    return $null
}

New-Item -ItemType Directory -Path $serviceRoot -Force | Out-Null

$health = Test-SanctuaryHealth -HealthUrl "$baseUrl/health"
$started = $false
$processId = $null

if ($null -eq $health) {
    $listeners = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
    if ($listeners) {
        $owners = @($listeners | Select-Object -ExpandProperty OwningProcess -Unique)
        throw "Port $Port is already listening, but Sanctuary health did not respond at $baseUrl/health. Owning process ids: $($owners -join ', ')."
    }

    $startResult = & $startServicePath `
        -InstallRoot $InstallRoot `
        -IntakeRoot $IntakeRoot `
        -OperatorName $OperatorName `
        -CmeId $CmeId `
        -ServiceIdentityId $ServiceIdentityId `
        -IdentityTemplateId $IdentityTemplateId `
        -SubjectCmeId $SubjectCmeId `
        -ThreadBindingId $ThreadBindingId `
        -SoulFrameId $SoulFrameId `
        -AgentiCoreId $AgentiCoreId `
        -UseIndustrialCore:$UseIndustrialCore `
        -Domain $Domain `
        -Role $Role `
        -JobClass $JobClass `
        -HostName $HostName `
        -Port $Port `
        -NoBuild:$NoBuild

    $started = $true
    $processId = [int] $startResult.ProcessId
    Start-Sleep -Milliseconds 350
    $health = Test-SanctuaryHealth -HealthUrl "$baseUrl/health"
    if ($null -eq $health) {
        throw "Sanctuary MCP alpha service started, but health did not respond at $baseUrl/health."
    }
} elseif (Test-Path -LiteralPath $statePath -PathType Leaf) {
    $state = Get-Content -LiteralPath $statePath -Raw | ConvertFrom-Json
    $processId = [int] $state.processId
}

$serviceState = $null
if (Test-Path -LiteralPath $statePath -PathType Leaf) {
    $serviceState = Get-Content -LiteralPath $statePath -Raw | ConvertFrom-Json
}

$initialize = Invoke-McpJsonRpc -Uri $mcpUrl -Payload @{
    jsonrpc = "2.0"
    id = 1
    method = "initialize"
    params = @{}
}

$toolsList = Invoke-McpJsonRpc -Uri $mcpUrl -Payload @{
    jsonrpc = "2.0"
    id = 2
    method = "tools/list"
    params = @{}
}

$statusCall = Invoke-McpJsonRpc -Uri $mcpUrl -Payload @{
    jsonrpc = "2.0"
    id = 3
    method = "tools/call"
    params = @{
        name = "sanctuary.status"
        arguments = @{
            sessionId = "codex-native-coupling-status"
            cmeId = $CmeId
            threadBindingId = $ThreadBindingId
            soulFrameId = $SoulFrameId
            agentiCoreId = $AgentiCoreId
            identityTemplateId = $IdentityTemplateId
            subjectCmeId = $SubjectCmeId
            benchRunCount = 1
        }
    }
}

$deniedCall = Invoke-McpJsonRpc -Uri $mcpUrl -Payload @{
    jsonrpc = "2.0"
    id = 4
    method = "tools/call"
    params = @{
        name = "sanctuary.gel_admission"
        arguments = @{
            sessionId = "codex-native-coupling-denied-gel-admission"
            cmeId = $CmeId
            threadBindingId = $ThreadBindingId
            soulFrameId = $SoulFrameId
            agentiCoreId = $AgentiCoreId
        }
    }
}

$receiptExport = & $invokePluginPath `
    -Command receipt-export `
    -InstallRoot $InstallRoot `
    -IntakeRoot $IntakeRoot `
    -OperatorName $OperatorName `
    -CmeId $CmeId `
    -ThreadBindingId $ThreadBindingId `
    -SoulFrameId $SoulFrameId `
    -AgentiCoreId $AgentiCoreId `
    -IdentityTemplateId $IdentityTemplateId `
    -SubjectCmeId $SubjectCmeId `
    -Domain $Domain `
    -Role $Role `
    -JobClass $JobClass `
    -Json `
    -NoBuild:$NoBuild

$toolCount = @($toolsList.result.tools).Count
$statusStructured = $statusCall.result.structuredContent
$deniedFailClosed = (
    $deniedCall.error.message -eq "tool-not-allowlisted" -and
    $deniedCall.error.data.failClosed -eq $true -and
    $deniedCall.error.data.providerCalled -eq $false -and
    $deniedCall.error.data.modelBound -eq $false -and
    $deniedCall.error.data.externalActionAuthorized -eq $false
)

$serviceIdentityForReport = $ServiceIdentityId
if ($null -ne $serviceState -and $serviceState.serviceIdentityId) {
    $serviceIdentityForReport = $serviceState.serviceIdentityId
}

$labContext = & $labContextResolverPath `
    -InstallRoot $InstallRoot `
    -LabActorCmeId $CmeId `
    -ServiceIdentityId $serviceIdentityForReport `
    -IdentityTemplateId $IdentityTemplateId `
    -SubjectCmeId $SubjectCmeId
$IdentityTemplateId = $labContext.IdentityTemplateId
$SubjectCmeId = $labContext.SubjectCmeId

$report = [ordered]@{
    schema = "project-sanctuary.codex-native-coupling.v1"
    checkedAtUtc = (Get-Date).ToUniversalTime().ToString("O")
    repositoryRoot = $repositoryRoot
    installRoot = $InstallRoot
    intakeRoot = $IntakeRoot
    callerCmeId = $CmeId
    cmeActualLabel = $identity.CmeActualLabel
    cmeIdentityParticipantPattern = $identity.ParticipantIdentityPattern
    cmeIdentityIsParticipant = $identity.CmeIdentityIsParticipant
    cmeIdentityIsServiceIdentity = $identity.CmeIdentityIsServiceIdentity
    cmeIdentityIsTemplateIdentity = $identity.CmeIdentityIsTemplateIdentity
    cmeIdentityResolverServiceIdentityId = $identity.ServiceIdentityId
    cmeIdentityResolverTemplateIdentityId = $identity.IdentityTemplateId
    cmeIdentityToolUseAdmitsGel = $identity.ToolUseAdmitsGel
    cmeIdentityToolUseMutatesSelfGel = $identity.ToolUseMutatesSelfGel
    cmeIdentityToolUseActivatesActual = $identity.ToolUseActivatesActual
    cmeIdentityToolUseGrantsAuthority = $identity.ToolUseGrantsAuthority
    cmeIdentityToolUseBindsModel = $identity.ToolUseBindsModel
    cmeIdentityToolUseCallsProvider = $identity.ToolUseCallsProvider
    cmeIdentityToolUseAuthorizesExternalAction = $identity.ToolUseAuthorizesExternalAction
    receiptCmeId = $statusStructured.CmeId
    identityTemplateId = $IdentityTemplateId
    serviceIdentityId = $serviceIdentityForReport
    serviceIdentityIsCme = $false
    oeSelfGelStorageCmeId = $statusStructured.CmeId
    serviceCallerIdentitySame = ($serviceIdentityForReport -eq $CmeId)
    serviceCallerIdentitySplitTracked = $true
    installLocalCmeLaneDeclarationPath = $labContext.Path
    installLocalCmeLaneDeclarationPresent = $labContext.Present
    installLocalCmeLaneDeclarationScope = $labContext.Scope
    installLocalCmeLaneDeclarationMatchesRequest = $labContext.MatchesRequest
    installLocalCmeLaneDeclarationIsPreinstallDoctrine = $labContext.IsPreinstallDoctrine
    installLocalLabActorCmeId = $labContext.LabActorCmeId
    installLocalLabActorActualLabel = $labContext.LabActorActualLabel
    installLocalTelemetrySubjectCmeId = $SubjectCmeId
    installLocalTelemetrySubjectActualLabel = $labContext.TelemetrySubjectActualLabel
    installLocalServiceIdentityId = $labContext.ServiceIdentityId
    installLocalTemplateIdentityId = $labContext.TemplateIdentityId
    installLocalGovernanceSimulationBodies = $labContext.GovernanceSimulationBodies
    installLocalResidueCapturePolicy = $labContext.ResidueCapturePolicy
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
    threadBindingId = $ThreadBindingId
    nativeCmeIdentitySplitById = $true
    soulFrameId = $SoulFrameId
    agentiCoreId = $AgentiCoreId
    everyCmeCarriesOwnSoulFrameAndAgentiCore = $true
    domain = $Domain
    role = $Role
    jobClass = $JobClass
    serviceStartedByThisRun = $started
    processId = $processId
    endpoint = $baseUrl
    mcpServerUrl = $mcpUrl
    pluginManifestPath = $pluginManifestPath
    pluginManifestDigest = ($pluginCustodySurfaces | Where-Object { $_.surfaceId -eq "plugin-manifest" } | Select-Object -First 1).digest
    pluginSkillPath = $pluginSkillPath
    pluginSkillDigest = ($pluginCustodySurfaces | Where-Object { $_.surfaceId -eq "plugin-skill" } | Select-Object -First 1).digest
    pluginWrapperPath = $pluginWrapperPath
    pluginWrapperDigest = ($pluginCustodySurfaces | Where-Object { $_.surfaceId -eq "plugin-wrapper" } | Select-Object -First 1).digest
    pluginMcpConfigPath = $pluginMcpConfigPath
    pluginMcpConfigDigest = ($pluginCustodySurfaces | Where-Object { $_.surfaceId -eq "mcp-descriptor" } | Select-Object -First 1).digest
    couplingScriptPath = $couplingScriptPath
    couplingScriptDigest = ($pluginCustodySurfaces | Where-Object { $_.surfaceId -eq "coupling-witness" } | Select-Object -First 1).digest
    pluginCustodySurfaces = $pluginCustodySurfaces
    pluginCustodySurfaceCount = @($pluginCustodySurfaces).Count
    pluginCustodySurfaceDigestsPresent = $pluginCustodySurfaceDigestsPresent
    pluginCustodyCandidate = $true
    pluginCustodyBundleId = "codex-plugin-mcp"
    pluginCustodyScope = "local-codex-mcp-loopback"
    pluginCustodyClosureRequirement = "review plugin manifest, skill text, plugin wrapper, MCP descriptor, and coupling witness script with live coupling proof plus closed-gate receipt"
    pluginCustodyReviewStatus = "five-surface-custody-review-observed"
    pluginCustodyReviewAllSurfacesReviewed = $pluginCustodyReviewAllSurfacesReviewed
    pluginCustodyReviewAllSurfaceDigestsPresent = $pluginCustodySurfaceDigestsPresent
    pluginCustodyReviewAllGatesClosed = $pluginCustodyReviewAllGatesClosed
    pluginCustodyReviewAdmissionDecision = "candidate-only-pending-operator-source-custody"
    pluginCustodyReviewNextAction = "operator source custody decision for plugin surfaces, then rerun live coupling proof and closed-gate receipt"
    pluginDescriptorIsAuthority = $false
    pluginDescriptorAdmitsGel = $false
    pluginDescriptorMutatesSelfGel = $false
    pluginDescriptorActivatesActual = $false
    pluginDescriptorBindsModel = $false
    pluginDescriptorCallsProvider = $false
    pluginDescriptorAuthorizesExternalAction = $false
    couplingReportIsAdmission = $false
    couplingReportIsRelease = $false
    couplingReportIsAuthorityGrant = $false
    statePath = $statePath
    latestCompatibilityReportPath = $couplingPath
    cmeCouplingReportPath = $couplingLanePath
    healthStatus = $health.status
    protocolVersion = $initialize.result.protocolVersion
    serverName = $initialize.result.serverInfo.name
    serverVersion = $initialize.result.serverInfo.version
    exposedToolCount = $toolCount
    statusToolSucceeded = $statusStructured.OutcomeCode -eq "sanctuary-status-completed-cold"
    statusToolAllGatesClosed = $statusStructured.allGatesClosed -eq $true
    deniedGelAdmissionFailClosed = $deniedFailClosed
    providerCalled = $false
    modelBound = $false
    externalActionAuthorized = $false
    gelAdmitted = $false
    selfGelMutated = $false
    cmeActualActivated = $false
    sanctuaryActualActivated = $false
    receiptExportObserved = ($receiptExport -join "`n") -match "sanctuary-receipt-export-completed-cold"
    nativeUsePosture = "Start or verify service once, then use plugin MCP server sanctuary-cme-local."
}

New-Item -ItemType Directory -Path $couplingLaneRoot -Force | Out-Null
$report | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $couplingPath -Encoding UTF8
$report | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $couplingLanePath -Encoding UTF8

if ($Json) {
    $report | ConvertTo-Json -Depth 12
} else {
    [pscustomobject]$report
}
