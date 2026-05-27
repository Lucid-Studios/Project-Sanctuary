# Local Codex Plugin Install

This guide installs the Project Sanctuary plugin from this repository into a
local Codex environment.

It does not publish the plugin, open a public service, call a provider, change
DNS, expose secrets, admit GEL/SelfGEL, or activate `.Actual`.

## Build First

From the repository root:

```powershell
dotnet build .\ProjectSanctuary.sln -c Release
dotnet test .\ProjectSanctuary.sln -c Release --no-build
```

## Validate The Plugin

The plugin body lives at:

```text
plugins/sanctuary-cme
```

The repo-local marketplace entry lives at:

```text
.agents/plugins/marketplace.json
```

It points to:

```text
./plugins/sanctuary-cme
```

## Install From Repo Marketplace

If this repo marketplace has not been added to Codex yet, add the marketplace
root once:

```powershell
codex plugin marketplace add .\.agents\plugins
```

Then install or refresh the plugin:

```powershell
codex plugin add sanctuary-cme@project-sanctuary
```

Start a new Codex thread after reinstalling so the updated skill text is loaded.

## First Cold Run

From the repository root:

```powershell
.\plugins\sanctuary-cme\scripts\Invoke-SanctuaryCme.ps1 -Command plugin-posture -Json
.\plugins\sanctuary-cme\scripts\Invoke-SanctuaryCme.ps1 -Command status -Json
.\plugins\sanctuary-cme\scripts\Invoke-SanctuaryCme.ps1 -Command verify-closed-gates -Json
```

Receipts are written under:

```text
.local/install/receipts
```

Local GEL residue is written under:

```text
.local/install/gel
```

Both paths are gitignored and belong to the active local install, not the public
source body.

## Update Loop

When the plugin changes locally:

```powershell
codex plugin add sanctuary-cme@project-sanctuary
```

Then start a new Codex thread.

If Codex does not pick up the update, bump the plugin cachebuster in
`plugins/sanctuary-cme/.codex-plugin/plugin.json`, reinstall, and start a new
thread.

## Boundary

Local plugin install means:

```text
Codex can call the local Sanctuary wrapper.
Sanctuary writes receipts.
The Operator reviews what happened.
```

Local plugin install does not mean:

```text
Codex becomes Sanctuary.
Chat becomes a secret transport.
Provider/model access is granted.
External action is authorized.
GEL/SelfGEL admission occurs.
Project Bicycle is thawed.
```
