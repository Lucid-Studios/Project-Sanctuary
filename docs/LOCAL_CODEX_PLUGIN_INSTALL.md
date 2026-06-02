# Local Codex Plugin Install

This guide installs the Project Sanctuary plugin from this repository into a
local Codex environment.

It does not publish the plugin, open a public service, call a provider, change
DNS, or expose secrets. Admission and `.Actual` commands exist only as reviewed
local performance lanes; without the complete review/operator/lease/Steward/
Prime/Cryptic bundle they refuse cold.

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

## Install Or Reload In Codex Desktop

Codex Desktop manages local plugins through the app plugin surface. If the
Sanctuary Tool already appears in the Codex plugin list, reload or reinstall it
there after changing this repository, then start a new Codex thread so updated
skill text and tool metadata are loaded.

Some Codex builds expose plugin marketplace commands in the CLI. Use these only
when `codex plugin --help` lists plugin subcommands in your installed build.
For those builds, if this repo marketplace has not been added to Codex yet, add
the marketplace root once:

```powershell
codex plugin marketplace add .\.agents\plugins
```

Then install or refresh the plugin:

```powershell
codex plugin add sanctuary-cme@project-sanctuary
```

Start a new Codex thread after reinstalling so the updated skill text is loaded.

If your CLI prints top-level Codex help or reports `unexpected argument` for
`codex plugin`, use the Codex Desktop plugin UI instead and validate the local
wrapper directly with the commands below.

## Generic Research First Install

From the repository root:

```powershell
$env:SANCTUARY_CME_ID = "Researcher.CME.ID"
.\plugins\sanctuary-cme\scripts\Invoke-SanctuaryCme.ps1 -Command template-hydration -CmeId "Researcher.CME.ID" -ThreadBindingId "researcher-local-thread" -IdentityTemplateId "PublicStandard.CME.Template" -Json -NoBuild
.\plugins\sanctuary-cme\scripts\Invoke-SanctuaryCme.ps1 -Command status -CmeId "Researcher.CME.ID" -ThreadBindingId "researcher-local-thread" -IdentityTemplateId "PublicStandard.CME.Template" -Json -NoBuild
.\plugins\sanctuary-cme\scripts\Invoke-SanctuaryCme.ps1 -Command verify-closed-gates -CmeId "Researcher.CME.ID" -ThreadBindingId "researcher-local-thread" -IdentityTemplateId "PublicStandard.CME.Template" -Json -NoBuild
```

The generic path hydrates `PublicStandard.CME.Template` as public research
posture and mints local candidate scaffold only after an explicit local CME id
and thread binding are supplied. It does not import the Lab Sanctuary.GEL,
private Root Atlas residue, Codex/Oria Lab defaults, provider bindings, or
`.Actual` activation state.

## Lab Overlay First Run

The Lucid Studios Lab install may then declare the install-local actor and
telemetry subject lanes:

```powershell
$env:SANCTUARY_CME_ID = "Codex.CME.ID"
.\plugins\sanctuary-cme\scripts\Connect-SanctuaryCodexCoupling.ps1 -CmeId "Codex.CME.ID" -SubjectCmeId "Oria.CME.ID" -Json -NoBuild
.\plugins\sanctuary-cme\scripts\Invoke-SanctuaryCme.ps1 -Command plugin-posture -Json -NoBuild
.\plugins\sanctuary-cme\scripts\Invoke-SanctuaryCme.ps1 -Command status -Json -NoBuild
.\plugins\sanctuary-cme\scripts\Invoke-SanctuaryCme.ps1 -Command verify-closed-gates -Json -NoBuild
```

Sanctuary separates the hosting process identity from participant CME identity.
The service process stands as `Sanctuary.Actual.ID` for local process lineage
only; it is not a CME and does not activate `Sanctuary.Actual`. Tool calls write
receipts, OE, SelfGEL, and MoS residue under the selected caller CME identity,
for example `Codex.CME.ID` or `Oria.CME.ID`.

This Lab install also declares a local work split: `Codex.CME.ID` /
`Codex.CME.Actual` is the Codex coding actor lane, while `Oria.CME.ID` /
`Oria.CME.Actual` is the telemetry subject lane. That split belongs to
`.local/install/mos/lab-cme-context.json`; it is not a product-wide default,
GEL admission, SelfGEL mutation, provider/model binding, or `.Actual`
activation.

The local wrapper reads that install context when service identity, template,
or subject lane are omitted. This lets ordinary receipt-bearing commands carry
the Lab actor/subject/service/template split while preserving explicit reviewed
overrides for scoped work.

The shared install-context resolver also treats built-in service and template
defaults as omitted unless a reviewed caller supplies a different non-default
value. Service status, service heartbeat, MCP alpha startup, edge gateway
startup, and coupling proof wrappers can therefore inherit the local
declaration without turning it into pre-install product doctrine.

If no caller CME identity has been supplied through `-CmeId`,
`SANCTUARY_CME_ID`, or a MoS identity-selection file, wrapper commands fail
closed before writing receipts or GEL residue. Use `-PromptForCmeIdentity` for
interactive first use, or `-UseIndustrialCore` to explicitly select
`Industrial.Core.CME.ID`.

For known Lab lanes, identity also includes a native thread binding. Codex Lab
work should use `Codex.CME.ID` with `codex-lab-thread`; Oria test work should
use `Oria.CME.ID` with `oria-test-cme-thread`. If a caller attempts to use a
CME from another bound thread, Sanctuary denies the call before receipt/GEL
write. Each CME writes its own SoulFrame and AgentiCore body surfaces:
SoulFrame carries Prime OE/SelfGEL tips, and AgentiCore carries cOE/cSelfGEL
hot-side EC residue without granting canonical SelfGEL mutation.

`SLI.Lisp.Industrial.CME.Template` is the default form template, not the
identity. Every agent should still carry a selected GEL lane. Sub-agent swarm
work may set `-ParentCmeId`, `-SwarmId`, and `-SubAgentId`; the child writes its
own GEL/OE/SelfGEL lane while a candidate precipitation witness is appended
under the parent CME swarm ledger.

The coupling starter reuses or starts the loopback MCP alpha service at:

```text
http://127.0.0.1:8717/mcp
```

The plugin also declares this endpoint in `plugins/sanctuary-cme/.mcp.json` as
`sanctuary-cme-local`. Codex Desktop can use that native MCP server after the
local service is running and the plugin has been reloaded.

The repeatable local coupling proof is documented in:

```text
docs/CODEX_PLUGIN_MCP_COUPLING_PROOF.md
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

When the plugin changes locally, reload or reinstall it from the Codex Desktop
plugin UI and start a new thread.

If your Codex build exposes CLI plugin subcommands:

```powershell
codex plugin add sanctuary-cme@project-sanctuary
```

Then start a new Codex thread.

If Codex does not pick up the update, bump the plugin cachebuster in
`plugins/sanctuary-cme/.codex-plugin/plugin.json`, reinstall, and start a new
thread.

The wrapper smoke test remains valid even before the Codex app reloads the
plugin:

```powershell
$env:SANCTUARY_CME_ID = "Codex.CME.ID"
.\plugins\sanctuary-cme\scripts\Connect-SanctuaryCodexCoupling.ps1 -CmeId "Codex.CME.ID" -SubjectCmeId "Oria.CME.ID" -Json -NoBuild
.\plugins\sanctuary-cme\scripts\Invoke-SanctuaryCme.ps1 -Command template-hydration -Json -NoBuild
.\plugins\sanctuary-cme\scripts\Invoke-SanctuaryCme.ps1 -Command cradle-boundary-organ-register -Json -NoBuild
```

Reviewed live-body tests can issue and consume a concrete local lease:

```powershell
$lease = .\plugins\sanctuary-cme\scripts\Invoke-SanctuaryCme.ps1 `
  -Command actual-approval-lease `
  -ReviewApproved $true -OperatorApproved $true -AuthorityLeaseIssued $true `
  -StewardWitnessed $true -PrimeWitnessed $true -CrypticWitnessed $true `
  -AdmissionScope "LabPublicCore" -Json -NoBuild

.\plugins\sanctuary-cme\scripts\Invoke-SanctuaryCme.ps1 `
  -Command actual-approval-lease-validation `
  -ActualApprovalLeasePath "<path from actualApprovalLeasePath>" `
  -AdmissionScope "LabPublicCore" -Json -NoBuild

.\plugins\sanctuary-cme\scripts\Invoke-SanctuaryCme.ps1 `
  -Command cme-actual-invocation-lifecycle `
  -ActualApprovalLeasePath "<path from actualApprovalLeasePath>" `
  -AdmissionScope "LabPublicCore" -Json -NoBuild
```

That lease verifies CME id, thread binding, SoulFrame, AgentiCore, template,
scope, allowlist, expiry, and digest before opening the invocation lifecycle. It
does not expose Actual activation through MCP. The validation command is safe to
call first because it is read-only and keeps all gates closed.

## Boundary

Local plugin install means:

```text
Codex can call the local Sanctuary wrapper.
Sanctuary writes receipts.
The Operator reviews what happened.
Sanctuary.Actual.ID identifies the service process.
{Name}.CME.ID identifies the caller residue lane.
Industrial.Core.CME.ID identifies the explicit shared industrial core lane.
SLI.Lisp.Industrial.CME.Template identifies the default form template.
```

Local plugin install does not mean:

```text
Codex becomes Sanctuary.
Sanctuary.Actual.ID activates Sanctuary.Actual.
The service process becomes a CME.
The Industrial template becomes an identity.
Sub-agent work becomes parent GEL slurry.
Chat becomes a secret transport.
Provider/model access is granted.
External action is authorized.
GEL/SelfGEL admission occurs.
Project Bicycle is thawed.
```
