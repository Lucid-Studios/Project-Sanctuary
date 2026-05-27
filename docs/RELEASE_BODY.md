# Release Body

This document defines the minimum repository body required before Project
Sanctuary is useful as a local Codex-installable developer preview.

It is not a production launch plan. It is the public/local install spine.

## Current Target

```text
Project Sanctuary
  public developer-preview repository
  one admitted core executable lane
  one local Codex plugin surface
  cold receipt-bearing command set
  reviewed performance gates
  documented non-claims
```

The release body is complete enough when another local Codex install can:

1. clone the repository;
2. build `Sanctuary.exe`;
3. install the `sanctuary-cme` plugin from the local marketplace;
4. run a cold posture command;
5. inspect receipts under `.local/install`;
6. verify closed or reviewed gates;
7. understand which surfaces remain held.

## Repository Split

```text
Project Sanctuary
  active public/core release lane
  owns the current plugin and executable posture

Project Bicycle
  frozen first-ride artifact
  remains a bounded public test package
  is not thawed into this release body

Private Lab
  active research, secrets, private corpora, and full install posture
  remains outside the public repository
```

No Project Bicycle authority-envelope, private Lab ecology, private corpora,
secret stores, or runtime residue should be imported merely to make the
Sanctuary plugin feel larger.

## Required Public Body

The release body should contain only:

```text
src/Sanctuary.Core
src/Sanctuary.Cli
tests/Sanctuary.Core.Tests
tools/Invoke-SanctuaryTool.ps1
tools/Start-SanctuaryMcpAlphaService.ps1
tools/Stop-SanctuaryMcpAlphaService.ps1
tools/Start-SanctuaryEdgeGateway.ps1
tools/New-SanctuaryEdgeDevCertificate.ps1
tools/Test-SanctuaryEdgeNetwork.ps1
plugins/sanctuary-cme
.agents/plugins/marketplace.json
docs
README/support/security/contribution files
```

The release body must not contain:

```text
.local active Lab state
encrypted payload stores
raw secrets or identity documents
private document repositories
private model payloads
production deployment automation
unreviewed provider credentials
Project Bicycle live-body imports
```

## Local Plugin Requirement

The local plugin surface is the first install body. It must remain small enough
to inspect and large enough to demonstrate Sanctuary as more than a locked demo.

The minimum local plugin path is:

```text
.agents/plugins/marketplace.json
-> plugins/sanctuary-cme/.codex-plugin/plugin.json
-> plugins/sanctuary-cme/skills/sanctuary-cme/SKILL.md
-> plugins/sanctuary-cme/scripts/Invoke-SanctuaryCme.ps1
-> tools/Invoke-SanctuaryTool.ps1
-> Sanctuary.exe
```

The plugin may expose cold read/fetch and reviewed-performance commands, but
installing it does not grant:

```text
provider/model binding
external action
secret intake completion
GEL admission
SelfGEL mutation
CME.Actual
Sanctuary.Actual
personhood or sovereignty claims
professional authority
```

## Completion Gate

Before the repository is considered complete for local installs:

```text
dotnet build passes
dotnet test passes
plugin manifest validates
local marketplace points at plugins/sanctuary-cme
README links the local install path
release checklist names local plugin install
receipt-export reports no unexpected open gates
Project Bicycle remains documented as frozen/reference only
```

Publication to GitHub or a public marketplace remains a separate Operator
decision.
