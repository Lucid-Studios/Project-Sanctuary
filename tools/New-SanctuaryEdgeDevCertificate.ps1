param(
    [string] $DnsName = "localhost",
    [string] $OutputPath = "",
    [string] $PasswordEnv = "SANCTUARY_EDGE_CERT_PASSWORD",
    [int] $ValidDays = 30
)

$ErrorActionPreference = "Stop"

$repositoryRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $repositoryRoot ".local\certs\sanctuary-edge-dev.pfx"
}

$password = [Environment]::GetEnvironmentVariable($PasswordEnv)
if ([string]::IsNullOrWhiteSpace($password)) {
    $password = [Guid]::NewGuid().ToString("N")
    [Environment]::SetEnvironmentVariable($PasswordEnv, $password, "Process")
    [Environment]::SetEnvironmentVariable($PasswordEnv, $password, "User")
}

$certRoot = Split-Path -Parent $OutputPath
New-Item -ItemType Directory -Path $certRoot -Force | Out-Null

$cert = New-SelfSignedCertificate `
    -DnsName $DnsName `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -NotAfter (Get-Date).AddDays($ValidDays) `
    -KeyAlgorithm RSA `
    -KeyLength 3072 `
    -KeyExportPolicy Exportable `
    -FriendlyName "Project Sanctuary Edge Dev Certificate"

$securePassword = ConvertTo-SecureString -String $password -AsPlainText -Force
Export-PfxCertificate `
    -Cert "Cert:\CurrentUser\My\$($cert.Thumbprint)" `
    -FilePath $OutputPath `
    -Password $securePassword | Out-Null

[pscustomobject]@{
    DnsName = $DnsName
    OutputPath = $OutputPath
    PasswordEnv = $PasswordEnv
    PasswordStoredInUserEnvironment = $true
    Thumbprint = $cert.Thumbprint
    NotAfter = $cert.NotAfter
}
