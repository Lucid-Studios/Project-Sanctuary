param(
    [string] $DomainName = "sanctuary.lucidtechnologies.tech",
    [int] $Port = 443,
    [string] $LocalHealthUrl = "",
    [switch] $Json
)

$ErrorActionPreference = "Stop"

function Test-PrivateAddress {
    param([string] $Address)

    if ($Address -match '^10\.') { return $true }
    if ($Address -match '^192\.168\.') { return $true }
    if ($Address -match '^172\.(1[6-9]|2[0-9]|3[0-1])\.') { return $true }
    if ($Address -match '^100\.(6[4-9]|[7-9][0-9]|1[01][0-9]|12[0-7])\.') { return $true }
    return $false
}

$localAddresses = Get-NetIPAddress -AddressFamily IPv4 -ErrorAction SilentlyContinue |
    Where-Object {
        $_.IPAddress -notlike '127.*' -and
        $_.IPAddress -notlike '169.254.*' -and
        $_.PrefixOrigin -ne 'WellKnown'
    } |
    Select-Object InterfaceAlias, IPAddress, PrefixLength

$defaultRoutes = Get-NetRoute -DestinationPrefix '0.0.0.0/0' -ErrorAction SilentlyContinue |
    Sort-Object RouteMetric |
    Select-Object -First 5 InterfaceAlias, NextHop, RouteMetric

$publicIpv4 = $null
try {
    $publicIpv4 = (Invoke-RestMethod -Uri 'https://api.ipify.org' -TimeoutSec 10).Trim()
} catch {
    $publicIpv4 = ""
}

$dnsRecords = @()
try {
    $dnsRecords = Resolve-DnsName $DomainName -ErrorAction Stop |
        Where-Object { $_.Type -in @('A', 'AAAA', 'CNAME') } |
        Select-Object Name, Type, IPAddress, NameHost
} catch {
    $dnsRecords = @()
}

$listeners = Get-NetTCPConnection -State Listen -ErrorAction SilentlyContinue |
    Where-Object { $_.LocalPort -eq $Port } |
    Select-Object LocalAddress, LocalPort, OwningProcess

$listenerProcesses = @()
foreach ($listener in $listeners) {
    $process = Get-Process -Id $listener.OwningProcess -ErrorAction SilentlyContinue
    if ($process) {
        $listenerProcesses += [pscustomobject]@{
            LocalAddress = $listener.LocalAddress
            LocalPort = $listener.LocalPort
            ProcessId = $process.Id
            ProcessName = $process.ProcessName
            Path = $process.Path
        }
    }
}

$localHealth = $null
if (-not [string]::IsNullOrWhiteSpace($LocalHealthUrl)) {
    try {
        $localHealth = Invoke-RestMethod -Uri $LocalHealthUrl -SkipCertificateCheck -TimeoutSec 10
    } catch {
        $localHealth = [pscustomobject]@{
            error = $_.Exception.Message
        }
    }
}

$domainPointsToPublicIpv4 = $false
if ($publicIpv4) {
    $domainPointsToPublicIpv4 = @($dnsRecords | Where-Object { $_.IPAddress -eq $publicIpv4 }).Count -gt 0
}

$publicIpv4LooksPrivate = if ($publicIpv4) { Test-PrivateAddress $publicIpv4 } else { $false }

$report = [ordered]@{
    schema = "project-sanctuary.edge.network-check.v1"
    checkedAt = (Get-Date).ToUniversalTime().ToString("O")
    domainName = $DomainName
    port = $Port
    publicIpv4 = $publicIpv4
    publicIpv4LooksPrivateOrCgnatRange = $publicIpv4LooksPrivate
    localIpv4Addresses = $localAddresses
    defaultRoutes = $defaultRoutes
    dnsRecords = $dnsRecords
    domainPointsToDetectedPublicIpv4 = $domainPointsToPublicIpv4
    listenersOnPort = $listenerProcesses
    localHealthUrl = $LocalHealthUrl
    localHealth = $localHealth
    directHomeHostingChecklist = @(
        "Create A or AAAA record for $DomainName after choosing public route.",
        "Confirm Starlink account/router WAN address matches the detected public IP or provides a usable IPv6 route.",
        "Forward TCP $Port from router/firewall to the Sanctuary host.",
        "Allow TCP $Port in Windows Firewall.",
        "Run Sanctuary Edge Gateway with a trusted certificate for $DomainName.",
        "Verify from outside the LAN before adding the ChatGPT connector."
    )
    starlinkCgnatNote = "If the Starlink router WAN address is in 100.64.0.0/10 or another private range, direct IPv4 inbound hosting will not work without Starlink Public IP/business support, IPv6, or a Lab-owned relay."
}

if ($Json) {
    $report | ConvertTo-Json -Depth 8
} else {
    $report
}
