param(
    [ValidateSet("status", "plugin-posture", "tool-idle", "cme-formation", "secret-intake-window", "seal-secret-payloads", "lab-query-state", "typed-secure-ping", "mos-lineage-register", "sli-register", "sli-access-gate-register", "engram-passage", "gel-closure", "witness-learning", "service-heartbeat", "bounded-refinement-ticket", "job-slice-guard", "lease-check", "receipt-export", "security-hardening", "install-floor-check", "issue-resolver", "domain-register", "core-targets", "swarm-refinement", "lisp-control-matrix-register", "lisp-matrix-control-seat", "standing-wave-form", "resonance-chamber-probe", "universal-form-register", "domain-morphism-register", "capability-composition-probe", "career-spline-probe", "selfgel-fibre-register", "work-posture-preload-probe", "cognitive-bench", "math-learning-bench", "bridge-morphism-test", "cme-theory-body", "operator-work-cme-ec-gap", "telemetry-slice-register", "extended-telemetry-weather", "cgoa-formation", "codex-governing-witness", "full-body-io-runtime", "gel-approval-nadir-return", "approval-closure-register", "coupling-control-surface-register", "actualization-state-register", "agenticore-duplex-lisp-membrane", "industrial-cme-live-install-posture", "meaning-bridge", "pre-personified-industrial-rendering", "typed-admission-decant", "admission-cleave-append", "gel-admission", "selfgel-admission", "actual-approval-lease", "actual-approval-lease-validation", "cme-actual-keypair-forge", "cme-actualization", "cme-actual-invocation-lifecycle", "sanctuary-actualization", "spline-watch", "hdt-holographic-slice-frame", "bonded-cme-protective-cleave", "core-body-protection", "lawful-action-body-register", "ec-organ-loop-engram-candidate", "install-individuation-register", "template-hydration", "negative-image-body-register", "photonic-harmonic-transition-register", "opal-engram-continuity-register", "meaning-making-event-register", "relational-delta-perception-register", "opal-engram-white-paper-register", "lab-gel-crystallization-phases", "stem-domain-training-certification", "lab-observation-digest", "research-latex-export", "construct-custody-register", "gel-crystal-register", "gel-reforge-bench", "theta-mechanics-ec-use-bench", "discernment-lineage", "proof-of-discernment", "gpt-use-case-testing", "trivium-forum-connector-posture", "external-llm-standing-probe", "cradle-boundary-organ-register", "verify-closed-gates")]
    [string] $Command = "status",

    [string] $InstallRoot = "",
    [string] $IntakeRoot = "",
    [string] $OperatorName = "Operator",
    [string] $CmeId = "",
    [string] $ServiceIdentityId = "Sanctuary.Actual.ID",
    [string] $Domain = "Lab",
    [string] $Role = "IndustrialCME",
    [string] $JobClass = "ColdBench",
    [string] $SecretLane = "Regional",
    [string] $SecretKind = "BusinessLicenseWashingtonState",
    [string[]] $SecretSource = @(),
    [string] $RegisteredEmail = "",
    [string] $SecurePingNonce = "",
    [string] $LicenseScope = "LabQueryState",
    [string] $AdmissionScope = "LabPublicCore",
    [string] $AdmissionNote = "",
    [string] $ActualApprovalLeasePath = "",
    [bool] $RegisteredAccountConfirmed = $false,
    [bool] $ReviewApproved = $false,
    [bool] $OperatorApproved = $false,
    [bool] $AuthorityLeaseIssued = $false,
    [bool] $StewardWitnessed = $false,
    [bool] $PrimeWitnessed = $false,
    [bool] $CrypticWitnessed = $false,
    [string] $InstallFailureMode = "",
    [string] $IssueId = "",
    [string] $IssueResolutionNote = "",
    [string] $HttpHost = "127.0.0.1",
    [int] $HttpPort = 0,
    [int] $LeaseMinutes = 15,
    [int] $HeartbeatSeconds = 60,
    [int] $BenchRunCount = 3000,
    [string] $IdentityTemplateId = "SLI.Lisp.Industrial.CME.Template",
    [string] $ThreadBindingId = "",
    [string] $SoulFrameId = "",
    [string] $AgentiCoreId = "",
    [string] $ParentCmeId = "",
    [string] $SubjectCmeId = "",
    [string] $SwarmId = "",
    [string] $SubAgentId = "",
    [bool] $OpenReceivingWindow = $false,
    [bool] $SearchMyPc = $false,
    [bool] $RoamingHttp = $false,
    [bool] $IssueResolverApproved = $false,
    [bool] $OperatorInstructionAcknowledged = $false,
    [switch] $PromptForCmeIdentity,
    [switch] $UseIndustrialCore,
    [switch] $Json,
    [switch] $NoBuild
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $PSCommandPath
$pluginRoot = Split-Path -Parent $scriptRoot
$pluginsRoot = Split-Path -Parent $pluginRoot
$repositoryRoot = Split-Path -Parent $pluginsRoot
$toolPath = Join-Path $repositoryRoot "tools\Invoke-SanctuaryTool.ps1"

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

& $toolPath `
    -Command $Command `
    -InstallRoot $InstallRoot `
    -IntakeRoot $IntakeRoot `
    -OperatorName $OperatorName `
    -CmeId $CmeId `
    -ServiceIdentityId $ServiceIdentityId `
    -Domain $Domain `
    -Role $Role `
    -JobClass $JobClass `
    -SecretLane $SecretLane `
    -SecretKind $SecretKind `
    -SecretSource $SecretSource `
    -RegisteredEmail $RegisteredEmail `
    -SecurePingNonce $SecurePingNonce `
    -LicenseScope $LicenseScope `
    -AdmissionScope $AdmissionScope `
    -AdmissionNote $AdmissionNote `
    -ActualApprovalLeasePath $ActualApprovalLeasePath `
    -RegisteredAccountConfirmed $RegisteredAccountConfirmed `
    -ReviewApproved $ReviewApproved `
    -OperatorApproved $OperatorApproved `
    -AuthorityLeaseIssued $AuthorityLeaseIssued `
    -StewardWitnessed $StewardWitnessed `
    -PrimeWitnessed $PrimeWitnessed `
    -CrypticWitnessed $CrypticWitnessed `
    -InstallFailureMode $InstallFailureMode `
    -IssueId $IssueId `
    -IssueResolutionNote $IssueResolutionNote `
    -HttpHost $HttpHost `
    -HttpPort $HttpPort `
    -LeaseMinutes $LeaseMinutes `
    -HeartbeatSeconds $HeartbeatSeconds `
    -BenchRunCount $BenchRunCount `
    -IdentityTemplateId $IdentityTemplateId `
    -ThreadBindingId $ThreadBindingId `
    -SoulFrameId $SoulFrameId `
    -AgentiCoreId $AgentiCoreId `
    -ParentCmeId $ParentCmeId `
    -SubjectCmeId $SubjectCmeId `
    -SwarmId $SwarmId `
    -SubAgentId $SubAgentId `
    -OpenReceivingWindow $OpenReceivingWindow `
    -SearchMyPc $SearchMyPc `
    -RoamingHttp $RoamingHttp `
    -IssueResolverApproved $IssueResolverApproved `
    -OperatorInstructionAcknowledged $OperatorInstructionAcknowledged `
    -UseIndustrialCore:$UseIndustrialCore `
    -Json:$Json `
    -NoBuild:$NoBuild
