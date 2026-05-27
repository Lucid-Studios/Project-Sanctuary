param(
    [string] $InstallRoot = "",
    [string] $IntakeRoot = "",
    [string] $OperatorName = "Operator",
    [string] $CmeId = "Codex.CME.ID",
    [string] $Domain = "Lab",
    [string] $Role = "IndustrialCME",
    [string] $JobClass = "GptUseCaseAlpha",
    [string] $HostName = "127.0.0.1",
    [int] $Port = 8717,
    [int] $MaxRequests = 0,
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
    "--cme-id", $CmeId,
    "--domain", $Domain,
    "--role", $Role,
    "--job-class", $JobClass
)

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
    serviceOwner = "Sanctuary.exe"
    posture = "cold-read-fetch-candidate-only"
} | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $statePath -Encoding UTF8

[pscustomobject]@{
    ProcessId = $process.Id
    Endpoint = "http://${HostName}:$Port/"
    StatePath = $statePath
}
