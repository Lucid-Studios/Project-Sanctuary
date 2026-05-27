param(
    [ValidateSet("status", "plugin-posture", "tool-idle", "cme-formation", "secret-intake-window", "seal-secret-payloads", "lab-query-state", "typed-secure-ping", "mos-lineage-register", "sli-register", "sli-access-gate-register", "engram-passage", "gel-closure", "witness-learning", "service-heartbeat", "bounded-refinement-ticket", "job-slice-guard", "lease-check", "receipt-export", "security-hardening", "install-floor-check", "issue-resolver", "domain-register", "core-targets", "swarm-refinement", "lisp-control-matrix-register", "lisp-matrix-control-seat", "standing-wave-form", "resonance-chamber-probe", "universal-form-register", "domain-morphism-register", "capability-composition-probe", "career-spline-probe", "selfgel-fibre-register", "work-posture-preload-probe", "cognitive-bench", "math-learning-bench", "industrial-cme-live-install-posture", "meaning-bridge", "pre-personified-industrial-rendering", "typed-admission-decant", "admission-cleave-append", "gel-admission", "selfgel-admission", "cme-actualization", "sanctuary-actualization", "spline-watch", "lab-gel-crystallization-phases", "stem-domain-training-certification", "discernment-lineage", "proof-of-discernment", "gpt-use-case-testing", "trivium-forum-connector-posture", "external-llm-standing-probe", "verify-closed-gates")]
    [string] $Command = "status",

    [string] $InstallRoot = "",
    [string] $IntakeRoot = "",
    [string] $OperatorName = "Operator",
    [string] $CmeId = "Codex.CME.ID",
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
    [bool] $OpenReceivingWindow = $false,
    [bool] $SearchMyPc = $false,
    [bool] $RoamingHttp = $false,
    [bool] $IssueResolverApproved = $false,
    [bool] $OperatorInstructionAcknowledged = $false,
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

& $toolPath `
    -Command $Command `
    -InstallRoot $InstallRoot `
    -IntakeRoot $IntakeRoot `
    -OperatorName $OperatorName `
    -CmeId $CmeId `
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
    -OpenReceivingWindow $OpenReceivingWindow `
    -SearchMyPc $SearchMyPc `
    -RoamingHttp $RoamingHttp `
    -IssueResolverApproved $IssueResolverApproved `
    -OperatorInstructionAcknowledged $OperatorInstructionAcknowledged `
    -Json:$Json `
    -NoBuild:$NoBuild
