param(
    [string] $InstallRoot = "",
    [switch] $PassThru
)

$ErrorActionPreference = "Stop"

$repositoryRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($InstallRoot)) {
    $InstallRoot = Join-Path $repositoryRoot ".local\install"
}

$statePath = Join-Path $InstallRoot "service\sanctuary-mcp-alpha-service.json"
if (-not (Test-Path -LiteralPath $statePath -PathType Leaf)) {
    if ($PassThru) {
        [pscustomobject]@{
            Stopped = $false
            Reason = "state-file-missing"
            StatePath = $statePath
        }
    }
    return
}

$state = Get-Content -LiteralPath $statePath -Raw | ConvertFrom-Json
$process = Get-Process -Id ([int] $state.processId) -ErrorAction SilentlyContinue
if ($null -ne $process) {
    Stop-Process -Id $process.Id -Force
}

$stoppedPath = Join-Path $InstallRoot "service\sanctuary-mcp-alpha-service-stopped.json"
@{
    schema = "project-sanctuary.service.mcp-alpha-process-stop.v1"
    stoppedAtUtc = (Get-Date).ToUniversalTime().ToString("O")
    processId = [int] $state.processId
    statePath = $statePath
    processWasRunning = $null -ne $process
} | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $stoppedPath -Encoding UTF8

if ($PassThru) {
    [pscustomobject]@{
        Stopped = $true
        ProcessWasRunning = $null -ne $process
        StatePath = $statePath
        StoppedPath = $stoppedPath
    }
}
