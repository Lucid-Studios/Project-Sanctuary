param(
    [string] $InstallRoot = "",
    [string] $CmeId = "",
    [string] $ThreadBindingId = "",
    [string] $DeviceId = "",
    [string] $AdbPath = "",
    [string] $PhoneRoot = "/sdcard/Download/Sanctuary/seed-node",
    [switch] $Push,
    [switch] $PromptForCmeIdentity,
    [switch] $UseIndustrialCore
)

$ErrorActionPreference = "Stop"

$repositoryRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($InstallRoot)) {
    $InstallRoot = Join-Path $repositoryRoot ".local\install"
}

function Write-Utf8NoBom {
    param(
        [Parameter(Mandatory = $true)]
        [string] $LiteralPath,

        [Parameter(Mandatory = $true)]
        [string] $Value
    )

    $encoding = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($LiteralPath, $Value, $encoding)
}

function Get-Sha256FileDigest {
    param(
        [Parameter(Mandatory = $true)]
        [string] $LiteralPath
    )

    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    try {
        $stream = [System.IO.File]::OpenRead($LiteralPath)
        try {
            return [System.BitConverter]::ToString(
                $sha256.ComputeHash($stream)
            ).Replace("-", "").ToLowerInvariant()
        } finally {
            $stream.Dispose()
        }
    } finally {
        $sha256.Dispose()
    }
}

if ($PhoneRoot -notmatch '^/[-A-Za-z0-9_./]+$') {
    throw "PhoneRoot must be an absolute Android path using only letters, numbers, dash, underscore, dot, and slash."
}

$identityResolverPath = Join-Path $PSScriptRoot "Resolve-SanctuaryCmeIdentity.ps1"
$identity = & $identityResolverPath `
    -InstallRoot $InstallRoot `
    -CmeId $CmeId `
    -UseIndustrialCore:$UseIndustrialCore `
    -Optional `
    -PromptIfNeeded:$PromptForCmeIdentity

$CmeId = $identity.CmeId
if ([string]::IsNullOrWhiteSpace($ThreadBindingId)) {
    if (-not [string]::IsNullOrWhiteSpace($env:SANCTUARY_THREAD_BINDING_ID)) {
        $ThreadBindingId = $env:SANCTUARY_THREAD_BINDING_ID
    } elseif ($identity.ThreadBindingId) {
        $ThreadBindingId = $identity.ThreadBindingId
    }
}

if ([string]::IsNullOrWhiteSpace($CmeId)) {
    throw "CmeId is required. Provide -CmeId, set SANCTUARY_CME_ID, use -UseIndustrialCore, or pass -PromptForCmeIdentity."
}

if ([string]::IsNullOrWhiteSpace($ThreadBindingId)) {
    throw "ThreadBindingId is required for a phone seed node target. Provide -ThreadBindingId or set SANCTUARY_THREAD_BINDING_ID."
}

$nodeRoot = Join-Path $InstallRoot "mobile\phone-seed-node"
$nodePath = Join-Path $nodeRoot "node.json"
$readmePath = Join-Path $nodeRoot "README.txt"
$pointerPath = Join-Path $nodeRoot "seed-pointer.json"
$stagingPath = Join-Path $nodeRoot "staging.json"
New-Item -ItemType Directory -Path $nodeRoot -Force | Out-Null

$createdAtUtc = (Get-Date).ToUniversalTime().ToString("O")
$nodePayload = [ordered]@{
    schema = "project-sanctuary.mobile.phone-seed-node.v1"
    nodeId = "sanctuary.phone.seed-node.local"
    nodeKind = "manifest-only-phone-target"
    createdAtUtc = $createdAtUtc
    cmeId = $CmeId
    threadBindingId = $ThreadBindingId
    targetInstallPath = $PhoneRoot
    seedPurpose = "test seed target for duplex Cryptic EC with ChatGPT Remote as interlink and local Sanctuary custody as witness"
    payloadClass = "manifest-only"
    executable = $false
    service = $false
    secretsIncluded = $false
    receiptBodiesIncluded = $false
    localPathsIncluded = $false
    providerCallsAllowed = $false
    modelBindingAllowed = $false
    externalActionsAllowed = $false
    gelAdmissionAllowed = $false
    selfGelMutationAllowed = $false
    cmeActualActivated = $false
    sanctuaryActualActivated = $false
    duplexCrypticEc = [ordered]@{
        localWitnessRequired = $true
        remoteWorkIsEvidenceOnly = $true
        phoneNodeIsTargetOnly = $true
        primeReviewRequired = $true
        crypticTypingRequired = $true
    }
    expectedFiles = @(
        "node.json",
        "README.txt",
        "seed-pointer.json"
    )
}

$nodeJson = $nodePayload | ConvertTo-Json -Depth 8
Write-Utf8NoBom -LiteralPath $nodePath -Value $nodeJson
$nodeDigest = Get-Sha256FileDigest -LiteralPath $nodePath

$readmeText = @"
Project Sanctuary phone seed node

This directory is a manifest-only target for a future test seed.
It is not an Android app, not a service, not a provider surface, and not an
Actual activation. It carries no secret payloads, no receipt bodies, and no
local filesystem paths.

Target role:
- receive a tiny node manifest on the phone
- provide a stable install path for later reviewed test seeds
- let Local Sanctuary/Codex, ChatGPT Remote interlink, and the phone target
  compare custody without letting the phone node self-authorize

Node digest:
$nodeDigest
"@
Write-Utf8NoBom -LiteralPath $readmePath -Value $readmeText

$pointerJson = ([ordered]@{
    schema = "project-sanctuary.mobile.phone-seed-pointer.v1"
    nodeId = $nodePayload.nodeId
    nodeDigest = $nodeDigest
    targetInstallPath = $PhoneRoot
    cmeId = $CmeId
    threadBindingId = $ThreadBindingId
    candidateOnly = $true
    gatesClosed = $true
}) | ConvertTo-Json -Depth 5
Write-Utf8NoBom -LiteralPath $pointerPath -Value $pointerJson

$adbAvailable = $false
$selectedDeviceId = $DeviceId
$pushed = $false
$adbPath = ""

if ($Push) {
    $candidateAdbPaths = @()
    if (-not [string]::IsNullOrWhiteSpace($AdbPath)) {
        $candidateAdbPaths += $AdbPath
    }

    $adbCommand = Get-Command adb -ErrorAction SilentlyContinue
    if ($adbCommand) {
        $candidateAdbPaths += $adbCommand.Source
    }

    $candidateAdbPaths += @(
        "D:\platform-tools-latest-windows\platform-tools\adb.exe",
        "C:\Tools\platform-tools\adb.exe",
        "$env:LOCALAPPDATA\Android\Sdk\platform-tools\adb.exe"
    )

    $resolvedAdbPath = $candidateAdbPaths |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
        Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } |
        Select-Object -First 1

    if ([string]::IsNullOrWhiteSpace($resolvedAdbPath)) {
        throw "adb was not found. Install Android platform-tools, pass -AdbPath, or add adb.exe to PATH, then rerun with -Push."
    }

    $adbAvailable = $true
    $adbPath = $resolvedAdbPath
    $devices = & $resolvedAdbPath devices |
        Select-Object -Skip 1 |
        Where-Object { $_ -match "\tdevice$" } |
        ForEach-Object { ($_ -split "\s+")[0] }

    if ([string]::IsNullOrWhiteSpace($selectedDeviceId)) {
        if ($devices.Count -eq 0) {
            throw "No ADB device is authorized. Enable USB debugging and accept the device authorization prompt."
        }

        if ($devices.Count -gt 1) {
            throw "Multiple ADB devices are connected. Rerun with -DeviceId."
        }

        $selectedDeviceId = $devices[0]
    }

    $adbArgs = @()
    if (-not [string]::IsNullOrWhiteSpace($selectedDeviceId)) {
        $adbArgs += @("-s", $selectedDeviceId)
    }

    & $resolvedAdbPath @adbArgs shell mkdir -p $PhoneRoot | Out-Host
    & $resolvedAdbPath @adbArgs push $nodePath "$PhoneRoot/node.json" | Out-Host
    & $resolvedAdbPath @adbArgs push $readmePath "$PhoneRoot/README.txt" | Out-Host
    & $resolvedAdbPath @adbArgs push $pointerPath "$PhoneRoot/seed-pointer.json" | Out-Host
    $pushed = $true
}

$stagingJson = ([ordered]@{
    schema = "project-sanctuary.mobile.phone-seed-node-staging.v1"
    stagedAtUtc = $createdAtUtc
    cmeId = $CmeId
    threadBindingId = $ThreadBindingId
    nodeRoot = $nodeRoot
    nodePath = $nodePath
    readmePath = $readmePath
    pointerPath = $pointerPath
    nodeDigest = $nodeDigest
    phoneRoot = $PhoneRoot
    adbAvailable = $adbAvailable
    adbPath = $adbPath
    deviceId = $selectedDeviceId
    pushed = $pushed
    gatesClosed = $true
}) | ConvertTo-Json -Depth 6
Write-Utf8NoBom -LiteralPath $stagingPath -Value $stagingJson

[pscustomobject]@{
    NodeId = $nodePayload.nodeId
    CmeId = $CmeId
    ThreadBindingId = $ThreadBindingId
    LocalNodeRoot = $nodeRoot
    LocalNodePath = $nodePath
    LocalPointerPath = $pointerPath
    NodeDigest = $nodeDigest
    PhoneRoot = $PhoneRoot
    Pushed = $pushed
    DeviceId = $selectedDeviceId
    StagingPath = $stagingPath
}
