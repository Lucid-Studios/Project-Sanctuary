param(
    [string] $InstallRoot = "",
    [string] $CmeId = "",
    [switch] $UseIndustrialCore,
    [switch] $Optional,
    [switch] $PromptIfNeeded
)

$ErrorActionPreference = "Stop"

$repositoryRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($InstallRoot)) {
    $InstallRoot = Join-Path $repositoryRoot ".local\install"
}

$defaultServiceIdentityId = "Sanctuary.Actual.ID"
$defaultIdentityTemplateId = "SLI.Lisp.Industrial.CME.Template"

function ConvertTo-CmeActualLabel {
    param([string] $Identity)

    if ([string]::IsNullOrWhiteSpace($Identity)) {
        return ""
    }

    if ($Identity.EndsWith(".CME.ID")) {
        return "$($Identity.Substring(0, $Identity.Length - ".CME.ID".Length)).CME.Actual"
    }

    return "$Identity.Actual"
}

function Assert-ParticipantIdentityAllowed {
    param([string] $Identity)

    if ([string]::IsNullOrWhiteSpace($Identity)) {
        return
    }

    if ($Identity -eq $defaultServiceIdentityId) {
        throw "CME identity '$Identity' is the Sanctuary service/process identity, not a participant CME. Select a {Name}.CME.ID lane before receipt, OE, SelfGEL, or MoS writes."
    }

    if ($Identity -eq $defaultIdentityTemplateId) {
        throw "CME identity '$Identity' is the Industrial CME template body, not a participant CME. Select a {Name}.CME.ID lane before receipt, OE, SelfGEL, or MoS writes."
    }
}

function New-IdentityResult {
    param(
        [string] $Identity,
        [string] $Source,
        [bool] $Prompted = $false,
        [string] $ThreadBindingId = "",
        [string] $DomainRole = "",
        [string] $SoulFrameId = "",
        [string] $AgentiCoreId = ""
    )

    [pscustomobject]@{
        CmeId = $Identity
        CmeActualLabel = ConvertTo-CmeActualLabel -Identity $Identity
        Source = $Source
        Prompted = $Prompted
        Selected = -not [string]::IsNullOrWhiteSpace($Identity)
        ParticipantIdentityPattern = "{Name}.CME.ID"
        CmeIdentityIsParticipant = (-not [string]::IsNullOrWhiteSpace($Identity)) -and
            $Identity -ne $defaultServiceIdentityId -and
            $Identity -ne $defaultIdentityTemplateId
        ServiceIdentityId = $defaultServiceIdentityId
        ServiceIdentityIsCme = $false
        CmeIdentityIsServiceIdentity = $Identity -eq $defaultServiceIdentityId
        IdentityTemplateId = $defaultIdentityTemplateId
        IdentityTemplateIsIdentity = $false
        CmeIdentityIsTemplateIdentity = $Identity -eq $defaultIdentityTemplateId
        ThreadBindingId = $ThreadBindingId
        DomainRole = $DomainRole
        SoulFrameId = $SoulFrameId
        AgentiCoreId = $AgentiCoreId
        ToolUseAdmitsGel = $false
        ToolUseMutatesSelfGel = $false
        ToolUseActivatesActual = $false
        ToolUseGrantsAuthority = $false
        ToolUseBindsModel = $false
        ToolUseCallsProvider = $false
        ToolUseAuthorizesExternalAction = $false
    }
}

function Get-IdentityCandidate {
    param([string] $Identity)

    if ([string]::IsNullOrWhiteSpace($Identity)) {
        return $null
    }

    $candidatePath = Join-Path $InstallRoot "mos\identity-candidates.json"
    if (-not (Test-Path -LiteralPath $candidatePath -PathType Leaf)) {
        return $null
    }

    $registry = Get-Content -LiteralPath $candidatePath -Raw | ConvertFrom-Json
    foreach ($candidate in @($registry.candidates)) {
        if ($candidate.cmeId -eq $Identity) {
            return $candidate
        }
    }

    return $null
}

function New-ResolvedIdentity {
    param(
        [string] $Identity,
        [string] $Source,
        [bool] $Prompted = $false
    )

    Assert-ParticipantIdentityAllowed -Identity $Identity
    $candidate = Get-IdentityCandidate -Identity $Identity
    $threadBindingId = ""
    $domainRole = ""
    $soulFrameId = ""
    $agentiCoreId = ""
    if ($null -ne $candidate) {
        $threadBindingId = [string] $candidate.lane
        $domainRole = [string] $candidate.domainRole
        $soulFrameId = [string] $candidate.soulFrameId
        $agentiCoreId = [string] $candidate.agentiCoreId
    }

    return New-IdentityResult `
        -Identity $Identity `
        -Source $Source `
        -Prompted $Prompted `
        -ThreadBindingId $threadBindingId `
        -DomainRole $domainRole `
        -SoulFrameId $soulFrameId `
        -AgentiCoreId $agentiCoreId
}

if (-not [string]::IsNullOrWhiteSpace($CmeId)) {
    return New-ResolvedIdentity -Identity $CmeId -Source "explicit-parameter"
}

if ($UseIndustrialCore) {
    return New-ResolvedIdentity -Identity "Industrial.Core.CME.ID" -Source "industrial-core-explicit"
}

if (-not [string]::IsNullOrWhiteSpace($env:SANCTUARY_CME_ID)) {
    return New-ResolvedIdentity -Identity $env:SANCTUARY_CME_ID -Source "environment"
}

$selectionPath = Join-Path $InstallRoot "mos\identity-selection.json"
if (Test-Path -LiteralPath $selectionPath -PathType Leaf) {
    $selection = Get-Content -LiteralPath $selectionPath -Raw | ConvertFrom-Json
    if (-not [string]::IsNullOrWhiteSpace($selection.cmeId)) {
        return New-ResolvedIdentity -Identity $selection.cmeId -Source "mos-identity-selection-file"
    }
}

$mosRoot = Join-Path $InstallRoot "gel\mos"
$mosCandidates = @()
if (Test-Path -LiteralPath $mosRoot -PathType Container) {
    $mosCandidates = Get-ChildItem -LiteralPath $mosRoot -Directory |
        Select-Object -ExpandProperty Name |
        Sort-Object
}

if ($mosCandidates.Count -eq 1) {
    return New-ResolvedIdentity -Identity $mosCandidates[0] -Source "single-mos-candidate"
}

if ($PromptIfNeeded) {
    Write-Host "[I] Industrial.Core.CME.ID"
    if ($mosCandidates.Count -gt 1) {
        Write-Host "Select Sanctuary CME identity:"
        for ($index = 0; $index -lt $mosCandidates.Count; $index++) {
            Write-Host "[$($index + 1)] $($mosCandidates[$index])"
        }

        $answer = Read-Host "CME identity number, I for Industrial.Core.CME.ID, or typed CME.ID"
        if ($answer -match '(?i)^(i|industrial|industrial\.core|industrial\.core\.cme\.id)$') {
            return New-ResolvedIdentity -Identity "Industrial.Core.CME.ID" -Source "interactive-industrial-core-selection" -Prompted $true
        }

        $selectedNumber = 0
        if ([int]::TryParse($answer, [ref] $selectedNumber)) {
            $selectedIndex = $selectedNumber - 1
            if ($selectedIndex -ge 0 -and $selectedIndex -lt $mosCandidates.Count) {
                return New-ResolvedIdentity -Identity $mosCandidates[$selectedIndex] -Source "interactive-mos-selection" -Prompted $true
            }
        }

        if (-not [string]::IsNullOrWhiteSpace($answer)) {
            return New-ResolvedIdentity -Identity $answer -Source "interactive-typed-selection" -Prompted $true
        }
    } else {
        $answer = Read-Host "Enter Sanctuary CME identity, or I for Industrial.Core.CME.ID"
        if ($answer -match '(?i)^(i|industrial|industrial\.core|industrial\.core\.cme\.id)$') {
            return New-ResolvedIdentity -Identity "Industrial.Core.CME.ID" -Source "interactive-industrial-core-selection" -Prompted $true
        }

        if (-not [string]::IsNullOrWhiteSpace($answer)) {
            return New-ResolvedIdentity -Identity $answer -Source "interactive-typed-selection" -Prompted $true
        }
    }
}

if ($Optional) {
    return New-IdentityResult -Identity "" -Source "not-selected-service-only"
}

throw "CME identity selection required before Sanctuary tool use. Pass -CmeId, set SANCTUARY_CME_ID, create '$selectionPath', run with -PromptForCmeIdentity, or pass -UseIndustrialCore."
