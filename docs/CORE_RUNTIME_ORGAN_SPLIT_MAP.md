# Core Runtime Organ Split Map

This document is an admission-readiness map for the current
`core-runtime` bundle. It does not perform the refactor and does not admit the
bundle. It identifies safe review boundaries for reducing
`SanctuaryReceiptService.cs` without changing behavior.

Current pressure:

```text
bundle: core-runtime
dirty version: 0.1.246
review lane: core-runtime-organ-split
primary file: src/Sanctuary.Core/SanctuaryReceiptService.cs
current primary file lines: 22560
actual approval lease split file: src/Sanctuary.Core/SanctuaryReceiptService.ActualApprovalLease.cs
actual approval lease split file lines: 145
actual invocation catalogs split file: src/Sanctuary.Core/SanctuaryReceiptService.ActualInvocationCatalogs.cs
actual invocation catalogs split file lines: 155
approval closure catalogs split file: src/Sanctuary.Core/SanctuaryReceiptService.ApprovalClosureCatalogs.cs
approval closure catalogs split file lines: 148
coupling control catalogs split file: src/Sanctuary.Core/SanctuaryReceiptService.CouplingControlCatalogs.cs
coupling control catalogs split file lines: 150
actualization state catalogs split file: src/Sanctuary.Core/SanctuaryReceiptService.ActualizationStateCatalogs.cs
actualization state catalogs split file lines: 187
AgentiCore duplex catalogs split file: src/Sanctuary.Core/SanctuaryReceiptService.AgentiCoreDuplexCatalogs.cs
AgentiCore duplex catalogs split file lines: 189
GEL approval/nadir-return catalogs split file: src/Sanctuary.Core/SanctuaryReceiptService.GelApprovalNadirCatalogs.cs
GEL approval/nadir-return catalogs split file lines: 146
Industrial live-install catalogs split file: src/Sanctuary.Core/SanctuaryReceiptService.IndustrialLiveInstallCatalogs.cs
Industrial live-install catalogs split file lines: 252
Meaning Bridge catalogs split file: src/Sanctuary.Core/SanctuaryReceiptService.MeaningBridgeCatalogs.cs
Meaning Bridge catalogs split file lines: 189
identity split file: src/Sanctuary.Core/SanctuaryReceiptService.Identity.cs
identity split file lines: 241
secret security split file: src/Sanctuary.Core/SanctuaryReceiptService.SecretSecurity.cs
secret security split file lines: 225
security scan split file: src/Sanctuary.Core/SanctuaryReceiptService.SecurityScan.cs
security scan split file lines: 100
governance matrix split file: src/Sanctuary.Core/SanctuaryReceiptService.GovernanceMatrix.cs
governance matrix split file lines: 230
local GEL bodies split file: src/Sanctuary.Core/SanctuaryReceiptService.LocalGelBodies.cs
local GEL bodies split file lines: 305
local GEL residue split file: src/Sanctuary.Core/SanctuaryReceiptService.LocalGelResidue.cs
local GEL residue split file lines: 262
JSON readiness split file: src/Sanctuary.Core/SanctuaryReceiptService.JsonReadiness.cs
JSON readiness split file lines: 251
composition catalogs split file: src/Sanctuary.Core/SanctuaryReceiptService.CompositionCatalogs.cs
composition catalogs split file lines: 266
swarm catalogs split file: src/Sanctuary.Core/SanctuaryReceiptService.SwarmCatalogs.cs
swarm catalogs split file lines: 192
domain target catalogs split file: src/Sanctuary.Core/SanctuaryReceiptService.DomainTargetCatalogs.cs
domain target catalogs split file lines: 374
telemetry catalogs split file: src/Sanctuary.Core/SanctuaryReceiptService.TelemetryCatalogs.cs
telemetry catalogs split file lines: 437
record split file: src/Sanctuary.Core/SanctuaryReceiptService.Records.cs
record split file lines: 515
record split declarations: 50
persistence/crypto split file: src/Sanctuary.Core/SanctuaryReceiptService.PersistenceCrypto.cs
persistence/crypto split file lines: 248
```

2026-06-02 custody note:

The current split files remain source-admission candidate material until
review and staging. Current evidence shows the files are compiled by the SDK
project glob, remain in `Sanctuary.Core`, and do not introduce a public API
command surface, provider surface, authority surface, or `.Actual` activation
path. The split map update is custody evidence, not admission.

## Split Law

First pass should use `partial class SanctuaryReceiptService` files in the same
namespace. That keeps behavior, visibility, and call topology stable while
making the organ body inspectable.

The split does not:

- change public API;
- change receipt schemas;
- change gate defaults;
- alter GEL/OE/SelfGEL paths;
- admit GEL or SelfGEL;
- activate `.Actual`;
- grant authority;
- call providers or bind models.

## Proposed Organs

| Organ File | Current Span | Function |
| --- | ---: | --- |
| `SanctuaryReceiptService.Identity.cs` | extracted 2026-06-02; 241 lines | CME identity selection, thread binding, service identity split, SoulFrame/AgentiCore ids, CME Actual labels, shared Prime membrane helpers, and lab template path helpers. |
| `SanctuaryReceiptService.ActualApprovalLease.cs` | extracted 2026-06-02; 145 lines | reviewed performance command posture, reviewed authority bundle checks, ActualApprovalLease verification, and lease digest calculation. |
| `SanctuaryReceiptService.ActualInvocationCatalogs.cs` | extracted 2026-06-02; 155 lines | CME.Actual invocation lifecycle states, interior process terms, EC phases, telemetry products, denial catalog, and SLI.Lisp lifecycle rendering without issuing leases or activating Actual. |
| `SanctuaryReceiptService.ApprovalClosureCatalogs.cs` | extracted 2026-06-02; 148 lines | approval states, closure states, passage phases, transition-pressure surfaces, homeostasis loops, and SLI.Lisp approval-closure rendering without writing evidence, granting authority, or admitting approval. |
| `SanctuaryReceiptService.CouplingControlCatalogs.cs` | extracted 2026-06-02; 150 lines | coupling-control surfaces, organ-stability states, CME instrument chassis slots, boundary denials, and SLI.Lisp coupling-control rendering without writing evidence, granting authority, or opening control surfaces. |
| `SanctuaryReceiptService.ActualizationStateCatalogs.cs` | extracted 2026-06-02; 187 lines | actualization layers, cryptic typing bands, Prime review gates, protected idea classes, boundary denials, and SLI.Lisp actualization-state rendering without writing evidence, activating Actual, granting authority, or admitting protected payloads. |
| `SanctuaryReceiptService.AgentiCoreDuplexCatalogs.cs` | extracted 2026-06-02; 189 lines | AgentiCore duplex endpoints, passage phases, Lisp channels, app integration surfaces, return telemetry surfaces, boundary denials, and SLI.Lisp membrane carrier rendering without writing evidence, evaluating Lisp, granting authority, or mutating SelfGEL. |
| `SanctuaryReceiptService.GelApprovalNadirCatalogs.cs` | extracted 2026-06-02; 146 lines | GEL approval methods, nadir return stages, residue classes, Steward GoA controls, individuated CME residue flow, and SLI.Lisp GEL approval/nadir-return rendering without writing evidence, admitting GEL/SelfGEL, granting authority, or performing approval. |
| `SanctuaryReceiptService.IndustrialLiveInstallCatalogs.cs` | extracted 2026-06-02; 252 lines | operational denial gates, denial fuzz cases, industrial instrument organs, and quoted SLI.Lisp denial membrane rendering without writing evidence, evaluating Lisp, admitting GEL/SelfGEL, granting authority, or activating Actual. |
| `SanctuaryReceiptService.MeaningBridgeCatalogs.cs` | extracted 2026-06-02; 189 lines | Mind/Body/Spirit layers, 4P methods, ambiguity classes, resolution states, human context bridges, anabelian bridge steps, claim resolution examples, and quoted SLI.Lisp meaning bridge rendering without writing evidence, admitting truth/GEL/SelfGEL, granting authority, or authorizing action. |
| `SanctuaryReceiptService.Runner.cs` | lines 272-1880 | gate construction, command run path, command normalization, disposition, outcome, and evidence dispatch. |
| `SanctuaryReceiptService.LocalGel.cs` | lines 1881-2659 | local GEL residue, MoS body fibres, template bodies, needs matrix, and access level surfaces. |
| `SanctuaryReceiptService.SecretSecurity.cs` | partially extracted 2026-06-02; 225 lines | secret source parsing, secure ping fail-silent posture, loopback classification, account challenge text, issue failure-mode classification, cGEL failure records, and issue tracking events. Remaining main-file spans still include lab query state, secure ping evidence, install floors, issue resolver evidence, secret sealing, security hardening, and receipt export scans. |
| `SanctuaryReceiptService.SecurityScan.cs` | extracted 2026-06-02; 100 lines | JSON string property reading, visible security roots and leak tokens, text surface classification, skipped-root filtering, closed-gate receipt property checks, and case-insensitive JSON lookup. |
| `SanctuaryReceiptService.GovernanceMatrix.cs` | extracted 2026-06-02; 230 lines | body-fibre template chassis slots, governing needs matrix, access-level groupoids, and negative governing levels reused by local GEL, SelfGEL fibre, and Actualization support. |
| `SanctuaryReceiptService.LocalGelBodies.cs` | extracted 2026-06-02; 305 lines | CME body-fibre bundle records, body-fibre SLI.Lisp rendering, and GEL template body/registry writers used by local GEL residue while preserving candidate-only posture. |
| `SanctuaryReceiptService.LocalGelResidue.cs` | extracted 2026-06-02; 262 lines | local GEL path evidence stamping, GEL residue writes, MoS/OE/SelfGEL/AgentiCore/body-fibre ledger appends, thread-binding fallback writes, and candidate-only swarm precipitation events. |
| `SanctuaryReceiptService.JsonReadiness.cs` | extracted 2026-06-02; 251 lines | JSON and JSONL readback helpers used by bench, telemetry, witness, and readiness evidence without changing admission posture. |
| `SanctuaryReceiptService.CompositionCatalogs.cs` | extracted 2026-06-02; 266 lines | universal composition forms, domain morphism entries, capability projections, career spline stages, SelfGEL fibre preload rules, and work posture preload fields. |
| `SanctuaryReceiptService.SwarmCatalogs.cs` | extracted 2026-06-02; 192 lines | swarm lanes, crystallization posture, Hundo execution order, wave gates, and run-session catalog support without executing autonomous action. |
| `SanctuaryReceiptService.DomainTargetCatalogs.cs` | extracted 2026-06-02; 374 lines | core target catalog, domain register entries, and legal gate support catalog builders used by source sealing and domain review without writing evidence or granting authority. |
| `SanctuaryReceiptService.TelemetryCatalogs.cs` | extracted 2026-06-02; 437 lines | Prime/Cryptic/Steward telemetry slice catalogs, extended telemetry weather source catalogs, Prime weather projection helpers, and SLI.Lisp telemetry rendering helpers without writing evidence or admitting telemetry. |
| `SanctuaryReceiptService.SliMatrix.cs` | lines 3553-6547 | SLI register, MoS lineage, access gates, engram passage, GEL closure, witness learning, service heartbeat, control matrix, and universal form registers. |
| `SanctuaryReceiptService.BenchBridge.cs` | lines 6677-8237 and 19187-19987 | cognitive bench, math bench, bridge morphism tests, CME theory body, operator/work/CME/EC gap, operational denial gates, instrument organs, and meaning bridge helpers. |
| `SanctuaryReceiptService.TelemetryCgoa.cs` | lines 8271-9781 | telemetry slices, extended weather, cGoA formation, Codex governing witness, full-body IO runtime. |
| `SanctuaryReceiptService.ApprovalActualization.cs` | lines 9782-11463 | GEL approval nadir-return writer, approval closure, coupling control surfaces, actualization states, AgentiCore duplex membrane. |
| `SanctuaryReceiptService.RenderingAdmission.cs` | lines 11464-12886 and 18421-19186 | industrial live install, pre-personified rendering, typed admission decant, admission cleave/append, spline watch, append need, mulch, and post-gate use posture. |
| `SanctuaryReceiptService.ResearchCrystals.cs` | lines 12887-16438 | lab crystallization phases, STEM training certification, lab observation digest, research LaTeX export, construct custody, GEL crystal, and GEL reforge. |
| `SanctuaryReceiptService.DiscernmentExternal.cs` | lines 16439-17531 | discernment lineage, proof of discernment, GPT use-case testing, Trivium Forum, external LLM standing probe, cradle boundary organ. |
| `SanctuaryReceiptService.DomainCatalogs.cs` | lines 17733-18420 and 19987-21111 | STEM domain catalog helpers, predictive method terms, domain emergence, and global telemetry. Core targets, domain register entries, and legal gate support now live in `SanctuaryReceiptService.DomainTargetCatalogs.cs`. |
| `SanctuaryReceiptService.PersistenceCrypto.cs` | extracted 2026-06-02; 248 lines | local key custody, encryption helpers, Markdown rendering, install-local context JSON reading, safe segmenting, string escaping, hashing, and file writes. |
| `SanctuaryReceiptService.Records.cs` | lines 21276-end | shared record types used by the split organs. |

Line spans are review coordinates, not rigid extraction commands. If nearby
helper methods move with their callers, preserve behavior over exact line
fidelity.

## SLI Interlace

The split map also gives the Lisp Control Matrix a stable bridge into the C#
body. Each proposed organ is a C# implementation boundary and an SLI development
surface. The bridge is descriptive only:

```text
organ map != Lisp evaluation
organ map != source admission
organ map != GEL/SelfGEL mutation
organ map != Actual activation
```

The first extracted file is interlaced as:

```text
src/Sanctuary.Core/SanctuaryReceiptService.Records.cs
  lispSurface: SLI.ConstructCustodyRecordSubstrate
  membrane: symbolic-record-substrate
  role: typed record carriers for receipt, construct, GEL, and control-matrix organs
```

The second extracted file is interlaced as:

```text
src/Sanctuary.Core/SanctuaryReceiptService.PersistenceCrypto.cs
  lispSurface: SLI.PersistenceCryptoCustodyMembrane
  membrane: local-custody-helper-substrate
  role: key custody, encryption, digest, write, Markdown, and install-context helper organ
```

The third extracted file is interlaced as:

```text
src/Sanctuary.Core/SanctuaryReceiptService.Identity.cs
  lispSurface: SLI.MoS.IdentityThreadBinding
  membrane: cme-identity-thread-binding
  role: participant identity selection, service/template split, thread lock, Actual label, and shared Prime membrane helper organ
```

The fourth extracted file is interlaced as:

```text
src/Sanctuary.Core/SanctuaryReceiptService.ActualApprovalLease.cs
  lispSurface: SLI.GoA.ActualApprovalLease
  membrane: actual-approval-lease-authority
  role: reviewed authority bundle check and ActualApprovalLease verification without authority by existence
```

The fifth extracted file is interlaced as:

```text
src/Sanctuary.Core/SanctuaryReceiptService.SecretSecurity.cs
  lispSurface: SLI.Security.SecretSupport
  membrane: secret-security-support
  role: secret source parsing, fail-silent secure ping posture, issue failure classification, cGEL failure records, and issue event routing
```

The sixth extracted file is interlaced as:

```text
src/Sanctuary.Core/SanctuaryReceiptService.SecurityScan.cs
  lispSurface: SLI.Security.ScanReview
  membrane: security-scan-review
  role: JSON property reading, visible security scan roots, leak-token checks, text surface classification, skipped-root filtering, and closed-gate receipt scan helpers
```

The seventh extracted file is interlaced as:

```text
src/Sanctuary.Core/SanctuaryReceiptService.GovernanceMatrix.cs
  lispSurface: SLI.Governance.Matrix
  membrane: governance-matrix-template-chassis
  role: body-fibre template chassis slots, governing needs, typed access levels, and negative governing levels reused across local GEL and Actualization support
```

The eighth extracted file is interlaced as:

```text
src/Sanctuary.Core/SanctuaryReceiptService.LocalGelBodies.cs
  lispSurface: SLI.LocalGel.BodyFibreTemplate
  membrane: local-gel-body-fibre-template-writer
  role: local GEL body-fibre bundle records, SLI.Lisp body-fibre rendering, and GEL template body/registry writers while preserving candidate-only residue
```

The ninth extracted file is interlaced as:

```text
src/Sanctuary.Core/SanctuaryReceiptService.LocalGelResidue.cs
  lispSurface: SLI.LocalGel.ResidueWriter
  membrane: local-gel-append-only-residue-writer
  role: local GEL path evidence stamping and append-only GEL/OE/SelfGEL/AgentiCore/body-fibre ledger writes while preserving candidate-only residue
```

The tenth extracted file is interlaced as:

```text
src/Sanctuary.Core/SanctuaryReceiptService.JsonReadiness.cs
  lispSurface: SLI.Readiness.JsonTelemetryReader
  membrane: json-readiness-readback-helper
  role: JSON and JSONL readback helpers for bench, telemetry, witness, and readiness evidence without admitting payloads
```

The eleventh extracted file is interlaced as:

```text
src/Sanctuary.Core/SanctuaryReceiptService.CompositionCatalogs.cs
  lispSurface: SLI.MatrixDomain.CompositionCatalog
  membrane: universal-domain-composition-catalog
  role: universal forms, domain morphisms, capability projections, career splines, SelfGEL fibres, and work posture preload fields as non-evaluated catalog substance
```

The twelfth extracted file is interlaced as:

```text
src/Sanctuary.Core/SanctuaryReceiptService.SwarmCatalogs.cs
  lispSurface: SLI.Swarm.ExecutionCatalog
  membrane: swarm-execution-catalog
  role: swarm lanes, wave gates, Hundo execution order, and run-session posture as non-autonomous catalog substance
```

The thirteenth extracted file is interlaced as:

```text
src/Sanctuary.Core/SanctuaryReceiptService.DomainTargetCatalogs.cs
  lispSurface: SLI.Domain.TargetLegalGateCatalog
  membrane: domain-target-legal-gate-catalog
  role: core targets, domain register entries, and legal gate support as non-evaluated catalog substance
```

The fourteenth extracted file is interlaced as:

```text
src/Sanctuary.Core/SanctuaryReceiptService.TelemetryCatalogs.cs
  lispSurface: SLI.Telemetry.SliceWeatherCatalog
  membrane: telemetry-slice-weather-catalog
  role: Prime/Cryptic/Steward telemetry slices and extended telemetry weather terms as non-evaluated catalog substance
```

The fifteenth extracted file is interlaced as:

```text
src/Sanctuary.Core/SanctuaryReceiptService.ActualInvocationCatalogs.cs
  lispSurface: SLI.GoA.ActualInvocationLifecycleCatalog
  membrane: actual-invocation-lifecycle-catalog
  role: CME.Actual invocation lifecycle, interior process, EC phase, telemetry product, and denial terms as non-activating catalog substance
```

The sixteenth extracted file is interlaced as:

```text
src/Sanctuary.Core/SanctuaryReceiptService.ApprovalClosureCatalogs.cs
  lispSurface: SLI.GoA.ApprovalClosureCatalog
  membrane: approval-closure-catalog
  role: approval state, closure state, passage phase, transition pressure, homeostasis loop, and SLI.Lisp approval-closure terms as non-admitting catalog substance
```

The seventeenth extracted file is interlaced as:

```text
src/Sanctuary.Core/SanctuaryReceiptService.CouplingControlCatalogs.cs
  lispSurface: SLI.Interconnect.CouplingControlCatalog
  membrane: coupling-control-catalog
  role: coupling control surface, organ stability, chassis slot, boundary denial, and SLI.Lisp coupling-control terms as non-authorizing catalog substance
```

The eighteenth extracted file is interlaced as:

```text
src/Sanctuary.Core/SanctuaryReceiptService.ActualizationStateCatalogs.cs
  lispSurface: SLI.GoA.ActualizationStateCatalog
  membrane: actualization-state-catalog
  role: actualization layer, cryptic typing, Prime review, protected idea, boundary denial, and SLI.Lisp actualization-state terms as non-activating catalog substance
```

The nineteenth extracted file is interlaced as:

```text
src/Sanctuary.Core/SanctuaryReceiptService.AgentiCoreDuplexCatalogs.cs
  lispSurface: SLI.AgentiCore.DuplexLispMembraneCatalog
  membrane: agenticore-duplex-lisp-membrane-catalog
  role: AgentiCore duplex endpoint, passage, Lisp channel, app surface, return telemetry, boundary denial, and membrane carrier terms as non-evaluated catalog substance
```

The twentieth extracted file is interlaced as:

```text
src/Sanctuary.Core/SanctuaryReceiptService.GelApprovalNadirCatalogs.cs
  lispSurface: SLI.GoA.GelApprovalNadirReturnCatalog
  membrane: gel-approval-nadir-return-catalog
  role: GEL approval method, nadir return stage, residue class, Steward GoA control, CME residue flow, and SLI.Lisp nadir-return terms as non-admitting catalog substance
```

The twenty-first extracted file is interlaced as:

```text
src/Sanctuary.Core/SanctuaryReceiptService.IndustrialLiveInstallCatalogs.cs
  lispSurface: SLI.IndustrialCme.LiveInstallDenialCatalog
  membrane: industrial-live-install-denial-catalog
  role: operational denial gates, denial fuzz cases, instrument organ posture, and quoted SLI.Lisp denial membrane terms as non-evaluated catalog substance
```

The twenty-second extracted file is interlaced as:

```text
src/Sanctuary.Core/SanctuaryReceiptService.MeaningBridgeCatalogs.cs
  lispSurface: SLI.MeaningBridge.ContextResolutionCatalog
  membrane: meaning-bridge-context-catalog
  role: Mind/Body/Spirit, 4P, ambiguity, resolution, human-context, anabelian bridge, claim-resolution, and SLI.Lisp meaning bridge terms as non-admitting catalog substance
```

Future splits should preserve the same shape:

```text
C# organ file
-> named SLI surface
-> non-evaluated symbolic membrane
-> test and receipt proof
-> custody decision
```

## Extraction Order

1. Move record types into `SanctuaryReceiptService.Records.cs`. Done in the
   2026-05-31 overnight admission lane as a mechanical top-level record split;
   2026-06-02 review measured 515 actual lines and 50 top-level record
   declarations.
   Current proof remains candidate-only until the untracked source file is
   intentionally staged or otherwise held by operator review.
2. Move persistence, crypto, Markdown, and file helpers. Done 2026-06-02 as
   `SanctuaryReceiptService.PersistenceCrypto.cs`; current proof remains
   candidate-only until the untracked source file is intentionally staged or
   otherwise held by operator review.
3. Move identity and thread-binding helpers. Done 2026-06-02 as
   `SanctuaryReceiptService.Identity.cs`; current proof remains candidate-only
   until the untracked source file is intentionally staged or otherwise held by
   operator review.
4. Move ActualApprovalLease authority and verification helpers. Done 2026-06-02
    as `SanctuaryReceiptService.ActualApprovalLease.cs`; current proof remains
    candidate-only until the untracked source file is intentionally staged or
    otherwise held by operator review.
5. Move CME.Actual invocation lifecycle catalog helpers. Done 2026-06-02 as
   `SanctuaryReceiptService.ActualInvocationCatalogs.cs`; current proof remains
   candidate-only until the untracked source file is intentionally staged or
   otherwise held by operator review.
6. Move approval-closure catalog helpers. Done 2026-06-02 as
   `SanctuaryReceiptService.ApprovalClosureCatalogs.cs`; current proof remains
   candidate-only until the untracked source file is intentionally staged or
   otherwise held by operator review.
7. Move coupling-control catalog helpers. Done 2026-06-02 as
   `SanctuaryReceiptService.CouplingControlCatalogs.cs`; current proof remains
   candidate-only until the untracked source file is intentionally staged or
   otherwise held by operator review.
8. Move actualization-state catalog helpers. Done 2026-06-02 as
   `SanctuaryReceiptService.ActualizationStateCatalogs.cs`; current proof
   remains candidate-only until the untracked source file is intentionally
   staged or otherwise held by operator review.
9. Move AgentiCore duplex Lisp membrane catalog helpers. Done 2026-06-02 as
   `SanctuaryReceiptService.AgentiCoreDuplexCatalogs.cs`; current proof remains
   candidate-only until the untracked source file is intentionally staged or
   otherwise held by operator review.
10. Move first SecretSecurity helper cluster. Done 2026-06-02 as
    `SanctuaryReceiptService.SecretSecurity.cs`; current proof remains
    candidate-only until the untracked source file is intentionally staged or
    otherwise held by operator review.
11. Move security scan helper cluster. Done 2026-06-02 as
    `SanctuaryReceiptService.SecurityScan.cs`; current proof remains
    candidate-only until the untracked source file is intentionally staged or
    otherwise held by operator review.
12. Move governance matrix helper cluster. Done 2026-06-02 as
    `SanctuaryReceiptService.GovernanceMatrix.cs`; current proof remains
    candidate-only until the untracked source file is intentionally staged or
    otherwise held by operator review.
13. Move local GEL body-fibre/template helper cluster. Done 2026-06-02 as
    `SanctuaryReceiptService.LocalGelBodies.cs`; current proof remains
    candidate-only until the untracked source file is intentionally staged or
    otherwise held by operator review.
14. Move local GEL evidence and event-ledger write helpers. Done 2026-06-02 as
    `SanctuaryReceiptService.LocalGelResidue.cs`; current proof remains
    candidate-only until the untracked source file is intentionally staged or
    otherwise held by operator review.
15. Move JSON and JSONL readiness readback helpers. Done 2026-06-02 as
    `SanctuaryReceiptService.JsonReadiness.cs`; current proof remains
    candidate-only until the untracked source file is intentionally staged or
    otherwise held by operator review.
16. Move first low-risk composition catalog cluster. Done 2026-06-02 as
    `SanctuaryReceiptService.CompositionCatalogs.cs`; current proof remains
    candidate-only until the untracked source file is intentionally staged or
    otherwise held by operator review.
17. Move swarm execution catalog cluster. Done 2026-06-02 as
    `SanctuaryReceiptService.SwarmCatalogs.cs`; current proof remains
    candidate-only until the untracked source file is intentionally staged or
    otherwise held by operator review.
18. Move domain target and legal gate support catalog cluster. Done 2026-06-02
    as `SanctuaryReceiptService.DomainTargetCatalogs.cs`; current proof remains
    candidate-only until the untracked source file is intentionally staged or
    otherwise held by operator review.
19. Move telemetry slice and weather catalog cluster. Done 2026-06-02 as
    `SanctuaryReceiptService.TelemetryCatalogs.cs`; current proof remains
    candidate-only until the untracked source file is intentionally staged or
    otherwise held by operator review.
20. Move GEL approval/nadir-return catalog helpers. Done 2026-06-02 as
    `SanctuaryReceiptService.GelApprovalNadirCatalogs.cs`; current proof
    remains candidate-only until the untracked source file is intentionally
    staged or otherwise held by operator review.
21. Move Industrial CME live-install denial catalog helpers. Done 2026-06-02 as
    `SanctuaryReceiptService.IndustrialLiveInstallCatalogs.cs`; current proof
    remains candidate-only until the untracked source file is intentionally
    staged or otherwise held by operator review.
22. Move Meaning Bridge catalog and SLI.Lisp rendering helpers. Done
    2026-06-02 as `SanctuaryReceiptService.MeaningBridgeCatalogs.cs`; current
    proof remains candidate-only until the untracked source file is
    intentionally staged or otherwise held by operator review.
23. Move remaining low-risk catalog-heavy organs.
24. Move command/evidence organs only after tests pass at every earlier step.
25. Leave `Run`, `BuildEvidence`, `BuildGates`, `BuildDisposition`, and
    `BuildOutcomeCode` in the runner file until the rest of the body is stable.

## Proof Gates

Each extraction step must pass:

```text
dotnet test ProjectSanctuary.sln
.\tools\Invoke-SanctuaryTool.ps1 -Command verify-closed-gates -CmeId Codex.CME.ID -Domain Lab -Role LabFacingCME -JobClass CoreRuntimeOrganSplit -NoBuild -Json
```

Required proof surfaces:

- full unit test pass;
- closed-gate receipt;
- no public API drift;
- no receipt schema drift;
- no file path drift for GEL/OE/SelfGEL/cGEL outputs;
- no `.Actual` activation;
- no authority grant.

Latest proof checkpoint:

```text
checkpoint: 2026-06-02T15:06Z
standing focused regression coverage: InstallLocalLabCmeContextStampsActorSubjectServiceAndTemplateLanes
standing focused regression coverage: CoreRequestDefaultsSanctuaryServiceIdentityWithoutCollapsingCallerCme
full core test bench: 122/122 passed
core split custody proof: primary receipt organ plus Actual approval lease split plus ActualInvocationCatalogs split plus ApprovalClosureCatalogs split plus CouplingControlCatalogs split plus ActualizationStateCatalogs split plus AgentiCoreDuplexCatalogs split plus GelApprovalNadirCatalogs split plus IndustrialLiveInstallCatalogs split plus MeaningBridgeCatalogs split plus identity/thread-binding split plus SecretSecurity split plus SecurityScan split plus GovernanceMatrix split plus LocalGelBodies split plus LocalGelResidue split plus JsonReadiness split plus CompositionCatalogs split plus SwarmCatalogs split plus DomainTargetCatalogs split plus TelemetryCatalogs split plus record-carrier split plus persistence/crypto split observed; 23 surfaces, all digests present, all gates closed
release CLI build: passed after stopping prior resident Sanctuary.exe process
direct CLI default service receipt: .local/install/receipts/status/direct-cli-service-default-20260602-1009/receipt.json
service startup receipt: .local/install/receipts/gpt-use-case-testing/sanctuary-gpt-use-case-service-start/receipt.json
wrapper default proof: Invoke-SanctuaryTool status without explicit SubjectCmeId returned Oria.CME.ID / Oria.CME.Actual from install context
plugin wrapper proof: Invoke-SanctuaryCme status without explicit SubjectCmeId returned Oria.CME.ID / Oria.CME.Actual from install context
service helper proof: Get-SanctuaryServiceLayerStatus status and Start-SanctuaryServiceLayer service-heartbeat returned Oria.CME.ID / Oria.CME.Actual from install context
resolver override proof: temporary install context replaced built-in service/template defaults while keeping gates closed
codex-plugin-mcp custody proof: coupling report hashes plugin manifest, skill text, plugin wrapper, MCP descriptor, and coupling witness, marks five surfaces reviewed-candidate-only, classifies Codex.CME.ID as participant identity, and denies service/template collapse plus authority/admission/action/provider/model/Actual
identity-and-posture tooling custody proof: bundle posture writer marks central wrapper, CME identity resolver, install-local context resolver, and bundle posture writer as reviewed-candidate-only with Codex/Oria/service/template lanes and closed gates
core record split custody proof: bundle posture writer marks the primary receipt organ, SanctuaryReceiptService.ActualApprovalLease.cs, SanctuaryReceiptService.ActualInvocationCatalogs.cs, SanctuaryReceiptService.ApprovalClosureCatalogs.cs, SanctuaryReceiptService.CouplingControlCatalogs.cs, SanctuaryReceiptService.ActualizationStateCatalogs.cs, SanctuaryReceiptService.AgentiCoreDuplexCatalogs.cs, SanctuaryReceiptService.GelApprovalNadirCatalogs.cs, SanctuaryReceiptService.IndustrialLiveInstallCatalogs.cs, SanctuaryReceiptService.MeaningBridgeCatalogs.cs, SanctuaryReceiptService.Identity.cs, SanctuaryReceiptService.SecretSecurity.cs, SanctuaryReceiptService.SecurityScan.cs, SanctuaryReceiptService.GovernanceMatrix.cs, SanctuaryReceiptService.LocalGelBodies.cs, SanctuaryReceiptService.LocalGelResidue.cs, SanctuaryReceiptService.JsonReadiness.cs, SanctuaryReceiptService.CompositionCatalogs.cs, SanctuaryReceiptService.SwarmCatalogs.cs, SanctuaryReceiptService.DomainTargetCatalogs.cs, SanctuaryReceiptService.TelemetryCatalogs.cs, SanctuaryReceiptService.Records.cs, and SanctuaryReceiptService.PersistenceCrypto.cs as reviewed-candidate-only with 50 record declarations, digests present, sourceAdmitted=false, and closed gates
closed-gate receipt: .local/install/receipts/closed-gate-verification/sanctuary-verify-closed-gates-20260602-150804-5735702-f6f7122a/receipt.json
coupling proof: .local/install/service/codex-native-coupling.json
coupling caller/receipt CME: Codex.CME.ID
coupling CME Actual label: Codex.CME.Actual
coupling CME identity participant: true
coupling CME identity service identity: false
coupling CME identity template identity: false
coupling service identity: Sanctuary.Actual.ID
coupling install-local actor: Codex.CME.Actual
coupling install-local telemetry subject: Oria.CME.Actual
coupling checkedAtUtc: 2026-06-02T15:08:04.9630966Z
coupling exposed tool count: 35
coupling plugin custody review: five-surface-custody-review-observed
coupling plugin custody admission decision: candidate-only-pending-operator-source-custody
tooling custody review: identity-and-posture-tooling-custody-review-observed
tooling custody admission decision: candidate-only-pending-operator-source-custody
core record split custody review: core-record-split-custody-review-observed
core record split admission decision: candidate-only-pending-operator-source-custody
coupling GEL admission fail-closed: true
provider/model/external-action/Actual gates: closed
```

The checkpoint proves the current split body still compiles, that core request
defaults and direct CLI receipts default to the service identity, that the MCP
runtime default subject/template lanes reach startup and tool receipts, and that
closed gates remain preserved. It does not prove release readiness, GEL
admission, SelfGEL mutation, authority grant, or Actual activation.

## Admission Posture

The `core-runtime` bundle is review-ready, but not admission-ready. Its current
next action is decomposition review followed by mechanical partial-class splits
with verification after each extraction.
