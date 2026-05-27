param(
    [int] $LocalPort = 8717,
    [string] $HostName = "127.0.0.1",
    [string] $TunnelName = "sanctuary-alpha-trivium-forum",
    [ValidateSet("http2", "quic", "auto")]
    [string] $Protocol = "http2",
    [switch] $NoDownload,
    [switch] $Json
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$toolRoot = Join-Path $repoRoot ".local\tools\cloudflared"
$logRoot = Join-Path $repoRoot ".local\trivium-forum\tunnel"
$cloudflaredExe = Join-Path $toolRoot "cloudflared.exe"
$stdoutPath = Join-Path $logRoot "cloudflared.stdout.log"
$stderrPath = Join-Path $logRoot "cloudflared.stderr.log"
$statePath = Join-Path $logRoot "trivium-forum-https-tunnel.json"

New-Item -ItemType Directory -Force -Path $toolRoot, $logRoot | Out-Null

function Get-Cloudflared {
    $installed = Get-Command cloudflared -ErrorAction SilentlyContinue
    if ($installed) {
        return $installed.Source
    }

    if (Test-Path $cloudflaredExe) {
        return $cloudflaredExe
    }

    if ($NoDownload) {
        throw "cloudflared is not installed and -NoDownload was specified."
    }

    $downloadUrl = "https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-windows-amd64.exe"
    Invoke-WebRequest -Uri $downloadUrl -OutFile $cloudflaredExe
    return $cloudflaredExe
}

function Get-TunnelUrl {
    param([string[]] $Paths)

    foreach ($path in $Paths) {
        if (-not (Test-Path $path)) {
            continue
        }

        $content = Get-Content -Path $path -Raw -ErrorAction SilentlyContinue
        if (-not $content) {
            continue
        }

        $match = [regex]::Match($content, "https://[a-zA-Z0-9\-]+\.trycloudflare\.com")
        if ($match.Success) {
            return $match.Value
        }
    }

    return $null
}

$healthUri = "http://$HostName`:$LocalPort/health"
try {
    $health = Invoke-RestMethod -Uri $healthUri -TimeoutSec 5
} catch {
    throw "Sanctuary MCP service is not reachable at $healthUri. Start it first with tools/Start-SanctuaryMcpAlphaService.ps1."
}

if ($health.status -ne "running" -or -not $health.allGatesClosed) {
    throw "Sanctuary MCP service is reachable but not in the expected running/all-gates-closed posture."
}

$existing = Get-CimInstance Win32_Process |
    Where-Object {
        $_.CommandLine -and
        $_.CommandLine.Contains("cloudflared") -and
        $_.CommandLine.Contains("--url http://$HostName`:$LocalPort")
    } |
    Select-Object -First 1

if ($existing) {
    $existingUrl = Get-TunnelUrl -Paths @($stderrPath, $stdoutPath)
    $result = [ordered]@{
        schema = "project-sanctuary.trivium-forum.https-tunnel.v1"
        tunnelName = $TunnelName
        status = "already-running"
        pid = $existing.ProcessId
        localMcpUrl = "http://$HostName`:$LocalPort/mcp"
        localSseUrl = "http://$HostName`:$LocalPort/sse"
        publicBaseUrl = $existingUrl
        chatGptMcpServerUrl = if ($existingUrl) { "$existingUrl/mcp" } else { $null }
        authentication = "No Auth lab alpha; cold tools only"
        allGatesClosed = $true
        providerCallsAllowed = $false
        modelBindingAllowed = $false
        externalActionsAllowed = $false
        gelAdmissionAllowed = $false
        selfGelMutationAllowed = $false
        actualActivationAllowed = $false
        stdoutPath = $stdoutPath
        stderrPath = $stderrPath
    }
} else {
    $cloudflared = Get-Cloudflared

    if (Test-Path $stdoutPath) { Remove-Item -LiteralPath $stdoutPath -Force }
    if (Test-Path $stderrPath) { Remove-Item -LiteralPath $stderrPath -Force }

    $arguments = @(
        "tunnel",
        "--no-autoupdate",
        "--metrics", "127.0.0.1:0",
        "--protocol", $Protocol,
        "--url", "http://$HostName`:$LocalPort"
    )

    $process = Start-Process `
        -FilePath $cloudflared `
        -ArgumentList $arguments `
        -WorkingDirectory $repoRoot `
        -WindowStyle Hidden `
        -RedirectStandardOutput $stdoutPath `
        -RedirectStandardError $stderrPath `
        -PassThru

    $publicBaseUrl = $null
    $deadline = (Get-Date).AddSeconds(40)
    while ((Get-Date) -lt $deadline) {
        Start-Sleep -Milliseconds 500
        if ($process.HasExited) {
            break
        }

        $publicBaseUrl = Get-TunnelUrl -Paths @($stderrPath, $stdoutPath)
        if ($publicBaseUrl) {
            break
        }
    }

    $result = [ordered]@{
        schema = "project-sanctuary.trivium-forum.https-tunnel.v1"
        tunnelName = $TunnelName
        status = if ($publicBaseUrl) { "running" } else { "starting-or-unresolved" }
        pid = $process.Id
        localMcpUrl = "http://$HostName`:$LocalPort/mcp"
        localSseUrl = "http://$HostName`:$LocalPort/sse"
        publicBaseUrl = $publicBaseUrl
        chatGptMcpServerUrl = if ($publicBaseUrl) { "$publicBaseUrl/mcp" } else { $null }
        authentication = "No Auth lab alpha; cold tools only"
        protocol = $Protocol
        allGatesClosed = $true
        providerCallsAllowed = $false
        modelBindingAllowed = $false
        externalActionsAllowed = $false
        gelAdmissionAllowed = $false
        selfGelMutationAllowed = $false
        actualActivationAllowed = $false
        stdoutPath = $stdoutPath
        stderrPath = $stderrPath
    }
}

$result | ConvertTo-Json -Depth 6 | Set-Content -Path $statePath -Encoding UTF8
$result["statePath"] = $statePath

if ($Json) {
    $result | ConvertTo-Json -Depth 6
} else {
    Write-Host "Trivium Forum HTTPS tunnel: $($result.status)"
    Write-Host "ChatGPT MCP Server URL: $($result.chatGptMcpServerUrl)"
    Write-Host "State: $statePath"
}
