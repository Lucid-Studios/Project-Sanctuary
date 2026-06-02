# Bundle Version Posture

This document defines the repo-facing dirt counter for Project Sanctuary.

The committed public repo is treated as the current baseline:

```text
baseline version: 0.1
zero point: git HEAD
```

The working tree may move ahead of that baseline before the work is cleanly
committed. Instead of treating all uncommitted work as one undifferentiated dirty
state, Sanctuary groups the delta into bundle postures.

## Bundle Version Law

The bundle counter is a working-tree maturity signal, not a release version.

```text
bundle dirty version
=
baseline version
+
version steps past baseline
```

The default step law is:

```text
versionStepsPastBaseline
=
dirty path count
+
ceiling(changed line count / 250)
```

This makes both file spread and line mass visible. A small change across many
interfaces and a deep change inside one organ both register as version pressure.

## Denials

The dirty counter does not:

- create a Git tag;
- publish a release;
- admit GEL;
- mutate SelfGEL;
- activate CME.Actual or Sanctuary.Actual;
- grant authority;
- certify maturity;
- replace tests, review, or commit history.

It answers only:

```text
How far has this bundle moved past the committed 0.1 baseline?
```

## Admission Readiness Overlay

The version counter now carries an admission-readiness overlay. This overlay
does not decide admission. It routes review.

Each bundle receives:

```text
decision
readyForAdmissionReview
reviewLane
holdReason
nextReviewAction
requiredProofs
denials
```

The overlay answers:

```text
What kind of review does this bundle need before it can be considered for code
admission?
```

It does not answer:

```text
Is this bundle admitted?
```

Current review decisions include:

```text
needs-decomposition-review
core-split-output-needs-custody
core-record-split-proof-observed
core-record-split-reviewed-pending-source-custody
needs-live-coupling-proof
live-coupling-proof-observed
verify-regression-proof
needs-public-language-review
needs-plugin-custody-review
plugin-custody-proof-observed
plugin-custody-reviewed-pending-source-custody
needs-tooling-custody-review
tooling-custody-proof-observed
tooling-custody-reviewed-pending-source-custody
stage-or-hold-before-admission
hold-lab-residue
candidate-for-bundle-review
```

Untracked files are never silently treated as admission-ready. They must be
staged, ignored, committed, or explicitly held as lab residue.

The generated posture also carries a readiness summary:

```text
review-ready bundle count
held bundle count
highest pressure bundles
decision groups
next action
```

This lets a reviewer see the whole working-tree admission posture before
opening individual diffs.

The generated posture also carries the install-local CME lane declaration when
the local lab context is present:

```text
lab actor
telemetry subject
service identity
identity template
residue capture policy
governance simulation bodies
```

For this lab install, that means `Codex.CME.ID` / `Codex.CME.Actual` can be
seen as the local coding actor while `Oria.CME.ID` / `Oria.CME.Actual` remains
the telemetry subject. `Sanctuary.Actual.ID` remains service/process identity,
and `SLI.Lisp.Industrial.CME.Template` remains template identity.

This declaration is install-local review posture. It is not preinstall doctrine,
not release admission, not GEL admission, not SelfGEL mutation, not authority,
not provider/model binding, and not `.Actual` activation.

Untracked paths are also gathered into a custody queue. The queue gives each
untracked file a recommended review posture, but never treats it as admitted:

```text
source-admission-candidate-after-review
tooling-admission-candidate-after-review
plugin-admission-candidate-after-review
doc-admission-candidate-after-review
hold-lab-residue
manual-custody-review-required
```

The queue exists to make the operator decision explicit:

```text
stage
hold
ignore
delete by explicit request
or keep as lab residue
```

The generated summary groups these untracked paths by recommended custody so a
reviewer can decide whole classes of files together while still preserving
per-path review.

For `codex-plugin-mcp`, an observed five-surface custody review can move the
bundle from generic custody-before-review into a more precise holding decision:

```text
plugin-custody-reviewed-pending-source-custody
```

This means the manifest, skill text, wrapper, MCP descriptor, and coupling
witness have live custody-review metadata and closed-gate proof, but untracked
plugin source paths still require an operator staging, hold, ignore, or commit
decision. Reviewed candidate-only does not mean admitted.

For `tooling-service-scripts`, an observed identity-and-posture tooling custody
review can move the bundle from generic custody-before-review into:

```text
tooling-custody-reviewed-pending-source-custody
```

This means the central tool wrapper, CME identity resolver, install-local
context resolver, and bundle posture writer carry custody-review metadata,
install-local actor/subject/service/template lanes, and closed-gate denials.
Untracked tooling paths still require an operator staging, hold, ignore, or
commit decision. Tooling review does not admit source, grant authority, bind a
provider/model, admit GEL/SelfGEL, or activate `.Actual`.

For `core-runtime`, an observed organ split custody review can move the bundle
from generic split-output custody into:

```text
core-record-split-reviewed-pending-source-custody
```

This means `SanctuaryReceiptService.ActualApprovalLease.cs`,
`SanctuaryReceiptService.ActualInvocationCatalogs.cs`,
`SanctuaryReceiptService.ApprovalClosureCatalogs.cs`,
`SanctuaryReceiptService.CouplingControlCatalogs.cs`,
`SanctuaryReceiptService.ActualizationStateCatalogs.cs`,
`SanctuaryReceiptService.AgentiCoreDuplexCatalogs.cs`,
`SanctuaryReceiptService.GelApprovalNadirCatalogs.cs`,
`SanctuaryReceiptService.IndustrialLiveInstallCatalogs.cs`,
`SanctuaryReceiptService.MeaningBridgeCatalogs.cs`,
`SanctuaryReceiptService.Identity.cs`,
`SanctuaryReceiptService.Records.cs`, and
`SanctuaryReceiptService.PersistenceCrypto.cs`, and
`SanctuaryReceiptService.SecretSecurity.cs`, and
`SanctuaryReceiptService.SecurityScan.cs`, and
`SanctuaryReceiptService.GovernanceMatrix.cs`, and
`SanctuaryReceiptService.LocalGelBodies.cs`, and
`SanctuaryReceiptService.LocalGelResidue.cs`, and
`SanctuaryReceiptService.JsonReadiness.cs`, and
`SanctuaryReceiptService.CompositionCatalogs.cs`, and
`SanctuaryReceiptService.SwarmCatalogs.cs`, and
`SanctuaryReceiptService.DomainTargetCatalogs.cs`, and
`SanctuaryReceiptService.TelemetryCatalogs.cs` have been reviewed as
candidate-only core organ splits beside the primary receipt organ, with all
surfaces digested and all gates closed. The split remains source-admission
pending until the operator chooses staging, hold, ignore, or commit posture for
the untracked core files. Organ split review does not admit source, admit
GEL/SelfGEL, grant authority, bind a provider/model, or activate `.Actual`.

The tool also writes a local ignored review checklist:

```text
.local/versioning/untracked-custody-review.md
```

## SLI Interlace Overlay

The custody queue is also linked into the Lisp development side. This is an
interlace, not an admission.

Each untracked path receives:

```text
lispSurface
membrane
interlaceRole
closureRequirement
evaluated = false
runnable = false
admitsGel = false
mutatesSelfGel = false
activatesActual = false
grantsAuthority = false
```

This lets the repo say:

```text
this file is not merely dirty;
it is an unadmitted symbolic or executable-adjacent surface
belonging to a named SLI development membrane.
```

Examples:

```text
docs/BUNDLE_VERSION_POSTURE.md
  -> SLI.ControlMatrix.BundleCustody

plugins/sanctuary-cme/.mcp.json
  -> SLI.Agenticore.DuplexMembrane

src/Sanctuary.Core/SanctuaryReceiptService.Records.cs
  -> SLI.ConstructCustodyRecordSubstrate

src/Sanctuary.Core/SanctuaryReceiptService.Identity.cs
  -> SLI.MoS.IdentityThreadBinding

src/Sanctuary.Core/SanctuaryReceiptService.ActualApprovalLease.cs
  -> SLI.GoA.ActualApprovalLease

src/Sanctuary.Core/SanctuaryReceiptService.ActualInvocationCatalogs.cs
  -> SLI.GoA.ActualInvocationLifecycleCatalog

src/Sanctuary.Core/SanctuaryReceiptService.ApprovalClosureCatalogs.cs
  -> SLI.GoA.ApprovalClosureCatalog

src/Sanctuary.Core/SanctuaryReceiptService.CouplingControlCatalogs.cs
  -> SLI.Interconnect.CouplingControlCatalog

src/Sanctuary.Core/SanctuaryReceiptService.ActualizationStateCatalogs.cs
  -> SLI.GoA.ActualizationStateCatalog

src/Sanctuary.Core/SanctuaryReceiptService.AgentiCoreDuplexCatalogs.cs
  -> SLI.AgentiCore.DuplexLispMembraneCatalog

src/Sanctuary.Core/SanctuaryReceiptService.GelApprovalNadirCatalogs.cs
  -> SLI.GoA.GelApprovalNadirReturnCatalog

src/Sanctuary.Core/SanctuaryReceiptService.IndustrialLiveInstallCatalogs.cs
  -> SLI.IndustrialCme.LiveInstallDenialCatalog

src/Sanctuary.Core/SanctuaryReceiptService.MeaningBridgeCatalogs.cs
  -> SLI.MeaningBridge.ContextResolutionCatalog

src/Sanctuary.Core/SanctuaryReceiptService.PersistenceCrypto.cs
  -> SLI.PersistenceCryptoCustodyMembrane

src/Sanctuary.Core/SanctuaryReceiptService.SecretSecurity.cs
  -> SLI.Security.SecretSupport

src/Sanctuary.Core/SanctuaryReceiptService.SecurityScan.cs
  -> SLI.Security.ScanReview

src/Sanctuary.Core/SanctuaryReceiptService.GovernanceMatrix.cs
  -> SLI.Governance.Matrix

src/Sanctuary.Core/SanctuaryReceiptService.LocalGelBodies.cs
  -> SLI.LocalGel.BodyFibreTemplate

src/Sanctuary.Core/SanctuaryReceiptService.LocalGelResidue.cs
  -> SLI.LocalGel.ResidueWriter

src/Sanctuary.Core/SanctuaryReceiptService.JsonReadiness.cs
  -> SLI.Readiness.JsonTelemetryReader

src/Sanctuary.Core/SanctuaryReceiptService.CompositionCatalogs.cs
  -> SLI.MatrixDomain.CompositionCatalog

src/Sanctuary.Core/SanctuaryReceiptService.SwarmCatalogs.cs
  -> SLI.Swarm.ExecutionCatalog

src/Sanctuary.Core/SanctuaryReceiptService.DomainTargetCatalogs.cs
  -> SLI.Domain.TargetLegalGateCatalog

src/Sanctuary.Core/SanctuaryReceiptService.TelemetryCatalogs.cs
  -> SLI.Telemetry.SliceWeatherCatalog

tools/Resolve-SanctuaryCmeIdentity.ps1
  -> SLI.MoS.IdentitySelection

tools/Resolve-SanctuaryInstallLabContext.ps1
  -> SLI.MoS.InstallLocalCmeLane

tools/Start-SanctuaryMcpAlphaService.ps1
  -> SLI.ServiceLaunch.IdentityLaneCarrier

tools/Start-SanctuaryEdgeGateway.ps1
  -> SLI.ServiceLaunch.IdentityLaneCarrier
```

`SLI.MoS.IdentitySelection` resolves a participant CME lane before any
receipt-bearing path writes residue. It must fail closed if a caller attempts to
use `Sanctuary.Actual.ID` as a participant CME identity or
`SLI.Lisp.Industrial.CME.Template` as an identity. Those are service/process and
template bodies, not participant lanes.

The overlay preserves a key law:

```text
SLI interlace != Lisp evaluation
SLI interlace != runtime authority
SLI interlace != GEL admission
SLI interlace != SelfGEL mutation
SLI interlace != Actual activation
```

The interlace gives the operator and future CME a traversable map from Git
custody to symbolic morphology. It does not close the gate by itself.

For `core-runtime`, the first review map is:

```text
docs/CORE_RUNTIME_ORGAN_SPLIT_MAP.md
```

For `mcp-cli-service` and `codex-plugin-mcp`, the live local proof path is:

```text
docs/CODEX_PLUGIN_MCP_COUPLING_PROOF.md
```

If `.local/install/service/codex-native-coupling.json` shows healthy MCP
status, closed status gates, denied GEL admission fail-closed behavior, and
service/participant identity separation, the `mcp-cli-service` decision becomes
`live-coupling-proof-observed`.

## Bundles

The current bundle taxonomy is:

```text
core-runtime
  src/Sanctuary.Core

mcp-cli-service
  src/Sanctuary.Cli

test-bench
  tests

tooling-service-scripts
  tools

codex-plugin-mcp
  plugins/sanctuary-cme

public-docs-release-posture
  README.md
  docs/*

phone-seed-node
  docs/PHONE_SEED_NODE.md
  tools/Install-SanctuaryPhoneSeedNode.ps1

repo-meta-other
  any remaining tracked or untracked surfaces
```

The phone seed node is separated because it is an experimental target seed and
future duplex test surface, not part of the core release lane.

## Tool

Run:

```powershell
.\tools\Get-SanctuaryBundleVersionPosture.ps1
```

The tool writes the current posture to:

```text
.local/versioning/bundle-version-posture.json
.local/versioning/bundle-version-posture.md
```

`.local` remains gitignored. The committed tool and this doctrine file define
how to reconstruct the counter at any time.

For machine-readable output:

```powershell
.\tools\Get-SanctuaryBundleVersionPosture.ps1 -Json
```
