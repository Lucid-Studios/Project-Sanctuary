param(
    [ValidateSet("status", "plugin-posture", "tool-idle", "cme-formation", "secret-intake-window", "seal-secret-payloads", "lab-query-state", "typed-secure-ping", "sli-register", "engram-passage", "gel-closure", "witness-learning", "service-heartbeat", "bounded-refinement-ticket", "job-slice-guard", "lease-check", "receipt-export", "security-hardening", "install-floor-check", "issue-resolver", "domain-register", "core-targets", "swarm-refinement", "lisp-control-matrix-register", "lisp-matrix-control-seat", "standing-wave-form", "resonance-chamber-probe", "universal-form-register", "domain-morphism-register", "capability-composition-probe", "career-spline-probe", "selfgel-fibre-register", "work-posture-preload-probe", "cognitive-bench", "math-learning-bench", "industrial-cme-live-install-posture", "meaning-bridge", "pre-personified-industrial-rendering", "typed-admission-decant", "admission-cleave-append", "gel-admission", "selfgel-admission", "cme-actualization", "sanctuary-actualization", "spline-watch", "lab-gel-crystallization-phases", "stem-domain-training-certification", "discernment-lineage", "proof-of-discernment", "gpt-use-case-testing", "verify-closed-gates")]
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
    [bool] $ChatSecretPassage = $false,
    [bool] $RoamingHttp = $false,
    [bool] $IssueResolverApproved = $false,
    [bool] $OperatorInstructionAcknowledged = $false,
    [switch] $Json,
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

if (-not $NoBuild) {
    dotnet build $projectPath -c Release | Out-Host
}

if (-not (Test-Path -LiteralPath $exePath)) {
    throw "Sanctuary executable not found at '$exePath'. Run without -NoBuild first."
}

$arguments = @(
    $Command,
    "--install-root", $InstallRoot,
    "--intake-root", $IntakeRoot,
    "--operator-name", $OperatorName,
    "--cme-id", $CmeId,
    "--domain", $Domain,
    "--role", $Role,
    "--job-class", $JobClass,
    "--secret-lane", $SecretLane,
    "--secret-kind", $SecretKind,
    "--registered-email", $RegisteredEmail,
    "--secure-ping-nonce", $SecurePingNonce,
    "--license-scope", $LicenseScope,
    "--admission-scope", $AdmissionScope,
    "--admission-note", $AdmissionNote,
    "--registered-account-confirmed", $RegisteredAccountConfirmed,
    "--review-approved", $ReviewApproved,
    "--operator-approved", $OperatorApproved,
    "--authority-lease-issued", $AuthorityLeaseIssued,
    "--steward-witnessed", $StewardWitnessed,
    "--prime-witnessed", $PrimeWitnessed,
    "--cryptic-witnessed", $CrypticWitnessed,
    "--install-failure-mode", $InstallFailureMode,
    "--issue-id", $IssueId,
    "--issue-resolution-note", $IssueResolutionNote,
    "--http-host", $HttpHost,
    "--http-port", $HttpPort,
    "--lease-minutes", $LeaseMinutes,
    "--heartbeat-seconds", $HeartbeatSeconds,
    "--bench-run-count", $BenchRunCount,
    "--open-receiving-window", $OpenReceivingWindow,
    "--search-my-pc", $SearchMyPc,
    "--chat-secret-passage", $ChatSecretPassage,
    "--roaming-http", $RoamingHttp,
    "--issue-resolver-approved", $IssueResolverApproved,
    "--operator-instruction-acknowledged", $OperatorInstructionAcknowledged
)

foreach ($source in $SecretSource) {
    $arguments += @("--secret-source", $source)
}

if ($Json) {
    $arguments += "--json"
}

& $exePath @arguments
