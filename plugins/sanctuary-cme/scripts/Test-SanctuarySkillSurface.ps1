[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$pluginRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$manifestPath = Join-Path $pluginRoot '.codex-plugin/plugin.json'

if (-not (Test-Path $manifestPath)) {
    throw "Plugin manifest not found: $manifestPath"
}

$manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
if ([string]::IsNullOrWhiteSpace($manifest.skills)) {
    throw 'plugin.json does not declare a skills surface.'
}

$skillsRoot = (Resolve-Path (Join-Path $pluginRoot $manifest.skills)).Path
$skillFiles = Get-ChildItem -Path $skillsRoot -Directory |
    ForEach-Object { Join-Path $_.FullName 'SKILL.md' } |
    Where-Object { Test-Path $_ }

$required = @('sanctuary-cme', 'manuscript-construction')
$discovered = @()

foreach ($skillFile in $skillFiles) {
    $text = Get-Content $skillFile -Raw
    if (-not $text.StartsWith('---')) {
        throw "Skill is missing YAML front matter: $skillFile"
    }

    $nameMatch = [regex]::Match($text, '(?m)^name:\s*([^\r\n]+)$')
    $descriptionMatch = [regex]::Match($text, '(?m)^description:\s*([^\r\n]+)$')
    if (-not $nameMatch.Success -or -not $descriptionMatch.Success) {
        throw "Skill front matter must contain name and description: $skillFile"
    }

    $discovered += $nameMatch.Groups[1].Value.Trim()
}

foreach ($name in $required) {
    if ($discovered -notcontains $name) {
        throw "Required Sanctuary skill not exposed under manifest skills root: $name"
    }
}

$result = [ordered]@{
    plugin = $manifest.name
    version = $manifest.version
    skillsRoot = $manifest.skills
    discoveredSkills = @($discovered | Sort-Object)
    requiredSkills = $required
    exposure = 'PASS'
}

$result | ConvertTo-Json -Depth 4
