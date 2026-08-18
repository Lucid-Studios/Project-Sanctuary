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

$required = @('sanctuary-git', 'sanctuary-cme', 'manuscript-construction')
$discovered = @()
$skillTextByName = @{}

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

    $name = $nameMatch.Groups[1].Value.Trim()
    $discovered += $name
    $skillTextByName[$name] = $text
}

foreach ($name in $required) {
    if ($discovered -notcontains $name) {
        throw "Required Sanctuary skill not exposed under manifest skills root: $name"
    }
}

$gitSkill = $skillTextByName['sanctuary-git']
if ($gitSkill -notmatch 'local Sanctuary installation is not required') {
    throw 'sanctuary-git does not explicitly preserve the no-local-install invariant.'
}
if ($gitSkill -notmatch 'Git evidence != local runtime evidence') {
    throw 'sanctuary-git does not distinguish repository evidence from live runtime evidence.'
}

$manuscriptSkill = $skillTextByName['manuscript-construction']
if ($manuscriptSkill -notmatch 'does not require an OpenAI API key') {
    throw 'manuscript-construction does not preserve its Git-native no-API-key posture.'
}

$result = [ordered]@{
    plugin = $manifest.name
    displayName = $manifest.interface.displayName
    version = $manifest.version
    skillsRoot = $manifest.skills
    discoveredSkills = @($discovered | Sort-Object)
    requiredSkills = $required
    gitNativeWithoutLocalInstall = $true
    localRuntimeSkill = 'sanctuary-cme'
    exposure = 'PASS'
}

$result | ConvertTo-Json -Depth 4
