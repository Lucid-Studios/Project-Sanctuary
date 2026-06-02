# Codex Plugin MCP Coupling Proof

This document records the local proof surface for the `codex-plugin-mcp` and
`mcp-cli-service` bundles. It is a repeatable proof path, not a release
admission.

## Command

```powershell
.\plugins\sanctuary-cme\scripts\Connect-SanctuaryCodexCoupling.ps1 `
  -CmeId Codex.CME.ID `
  -SubjectCmeId Oria.CME.ID `
  -Domain Lab `
  -Role LabFacingCME `
  -JobClass ServiceLaunchIdentityLaneCarrierWake `
  -NoBuild `
  -Json
```

## Latest Observed Proof

```text
checkedAtUtc: 2026-06-02T16:16:23.5719278Z
callerCmeId: Codex.CME.ID
cmeActualLabel: Codex.CME.Actual
cmeIdentityParticipantPattern: {Name}.CME.ID
cmeIdentityIsParticipant: true
cmeIdentityIsServiceIdentity: false
cmeIdentityIsTemplateIdentity: false
cmeIdentityResolverServiceIdentityId: Sanctuary.Actual.ID
cmeIdentityResolverTemplateIdentityId: SLI.Lisp.Industrial.CME.Template
cmeIdentityToolUseAdmitsGel: false
cmeIdentityToolUseMutatesSelfGel: false
cmeIdentityToolUseActivatesActual: false
cmeIdentityToolUseGrantsAuthority: false
cmeIdentityToolUseBindsModel: false
cmeIdentityToolUseCallsProvider: false
cmeIdentityToolUseAuthorizesExternalAction: false
receiptCmeId: Codex.CME.ID
identityTemplateId: SLI.Lisp.Industrial.CME.Template
oeSelfGelStorageCmeId: Codex.CME.ID
serviceIdentityId: Sanctuary.Actual.ID
serviceIdentityIsCme: false
serviceCallerIdentitySame: false
serviceCallerIdentitySplitTracked: true
installLocalCmeLaneDeclarationPresent: true
installLocalCmeLaneDeclarationScope: install-local-only
installLocalCmeLaneDeclarationMatchesRequest: true
installLocalLabActorCmeId: Codex.CME.ID
installLocalLabActorActualLabel: Codex.CME.Actual
installLocalTelemetrySubjectCmeId: Oria.CME.ID
installLocalTelemetrySubjectActualLabel: Oria.CME.Actual
installLocalServiceIdentityId: Sanctuary.Actual.ID
installLocalTemplateIdentityId: SLI.Lisp.Industrial.CME.Template
installLocalGovernanceSimulationBodies: Prime.SLM, Cryptic.SLM, Steward.SLM, SoulFrame.SLM, AgentiCore.SLM
installLocalResidueCapturePolicy: candidate-gel-residue-from-live-lab-work
installLocalGelResidueIsCandidateOnly: true
installLocalTelemetryReturnIsSelfGelMutation: false
installLocalToolUseAdmitsGel: false
installLocalToolUseActivatesActual: false
installLocalToolUseGrantsAuthority: false
installLocalToolUseBindsModel: false
installLocalToolUseCallsProvider: false
installLocalToolUseAuthorizesExternalAction: false
threadBindingId: codex-lab-thread
jobClass: ServiceLaunchIdentityLaneCarrierWake
healthStatus: running
serviceStartedByThisRun: false
processId: 27848
mcpServerUrl: http://127.0.0.1:8717/mcp
pluginManifestPath: plugins/sanctuary-cme/.codex-plugin/plugin.json
pluginManifestDigest: aed7b6af74622c5a06d04bc7036cd53ad5aa1b54a2c9773a1d66040aeb569ec4
pluginSkillPath: plugins/sanctuary-cme/skills/sanctuary-cme/SKILL.md
pluginSkillDigest: f0cd30cce0b785ed85fb431d7586882a72f524ddfe0162a0bb02ead18f1c53c7
pluginWrapperPath: plugins/sanctuary-cme/scripts/Invoke-SanctuaryCme.ps1
pluginWrapperDigest: d12334cc14b13984b4632eaff07d99fd3096761885634b4d516104f6cc023c82
pluginMcpConfigPath: plugins/sanctuary-cme/.mcp.json
pluginMcpConfigDigest: dd2690c61c246ae1f2933102a6338ed899a7b6672ef6d40f2b6cf904c11be92e
couplingScriptPath: plugins/sanctuary-cme/scripts/Connect-SanctuaryCodexCoupling.ps1
couplingScriptDigest: a05fdf7633a3ed03ab298dac401cf50cb1e47578696ad6b1933120cea891bb1b
pluginCustodySurfaceCount: 5
pluginCustodySurfaceDigestsPresent: true
pluginCustodyCandidate: true
pluginCustodyBundleId: codex-plugin-mcp
pluginCustodyScope: local-codex-mcp-loopback
pluginCustodyClosureRequirement: review plugin manifest, skill text, plugin wrapper, MCP descriptor, and coupling witness script with live coupling proof plus closed-gate receipt
pluginCustodyReviewStatus: five-surface-custody-review-observed
pluginCustodyReviewAllSurfacesReviewed: true
pluginCustodyReviewAllSurfaceDigestsPresent: true
pluginCustodyReviewAllGatesClosed: true
pluginCustodyReviewAdmissionDecision: candidate-only-pending-operator-source-custody
pluginCustodyReviewNextAction: operator source custody decision for plugin surfaces, then rerun live coupling proof and closed-gate receipt
pluginDescriptorIsAuthority: false
pluginDescriptorAdmitsGel: false
pluginDescriptorMutatesSelfGel: false
pluginDescriptorActivatesActual: false
pluginDescriptorBindsModel: false
pluginDescriptorCallsProvider: false
pluginDescriptorAuthorizesExternalAction: false
couplingReportIsAdmission/release/authorityGrant: false
protocolVersion: 2025-03-26
serverName: Sanctuary Tool
serverVersion: 0.1.0-alpha
exposedToolCount: 35
statusToolSucceeded: true
statusToolAllGatesClosed: true
deniedGelAdmissionFailClosed: true
providerCalled: false
modelBound: false
externalActionAuthorized: false
gelAdmitted: false
selfGelMutated: false
cmeActualActivated: false
sanctuaryActualActivated: false
receiptExportObserved: true
```

The current proof was refreshed after the plugin skill and local install guide
were aligned to pass both the lab actor lane and telemetry subject lane in the
quick coupling command, and after the central wrapper learned to fill omitted
service/template/subject lanes from local install context. The shared
install-context resolver now treats built-in service/template defaults as
omitted for local-context resolution, so MCP alpha, edge gateway, coupling, and
receipt wrapper paths can inherit the install declaration. It reused the
resident `Sanctuary.exe` MCP alpha service. The MCP runtime receives
`--identity-template-id` and
`--subject-cme-id` from the launch scripts, and the service startup receipt uses
the lab actor CME lane while keeping `Sanctuary.Actual.ID` as the separate
service identity.

The direct CLI entrypoint also defaults `--service-id` to
`Sanctuary.Actual.ID`, so raw executable receipts can match the install-local
lane declaration without relying on the PowerShell wrapper.

The central PowerShell wrapper now resolves
`.local/install/mos/lab-cme-context.json` before invoking the executable. A
plain wrapper status call with no explicit `-SubjectCmeId` returned
`installLocalTelemetrySubjectCmeId: Oria.CME.ID`,
`installLocalTelemetrySubjectActualLabel: Oria.CME.Actual`,
`installLocalServiceIdentityId: Sanctuary.Actual.ID`,
`installLocalTemplateIdentityId: SLI.Lisp.Industrial.CME.Template`, and
`AllClosed: true`.

Service helper proof also returned the same split without explicit subject
arguments:

```text
Get-SanctuaryServiceLayerStatus.ps1 -> status -> Oria.CME.ID / Oria.CME.Actual
Start-SanctuaryServiceLayer.ps1 -> service-heartbeat -> Oria.CME.ID / Oria.CME.Actual
```

The coupling report now acts as a custody-review witness for the
`codex-plugin-mcp` bundle. It hashes five local plugin surfaces: manifest, skill
text, plugin wrapper, MCP descriptor, and coupling witness script. Each surface
is marked `reviewed-candidate-only`, `sourceAdmitted: false`, and
`requiresOperatorSourceCustody: true`. The report names the bundle scope as
`local-codex-mcp-loopback`, marks all custody-surface digests present, and
explicitly denies authority, GEL admission, SelfGEL mutation, `.Actual`
activation, provider calls, model binding, external action, release, and
admission. The report is a review surface, not proof of source admission.

The core `SanctuaryRequest` default also names `Sanctuary.Actual.ID`, while
caller CME selection remains required before receipt or GEL/OE/SelfGEL writes.

## Local Proof Artifacts

The coupling script writes local ignored reports:

```text
.local/install/service/codex-native-coupling.json
.local/install/service/coupling/Codex.CME.ID.json
```

Those reports are active Lab telemetry, not source artifacts.

Latest refreshed proof:

```text
checkedAtUtc: 2026-06-02T16:16:23.5719278Z
jobClass: ServiceLaunchIdentityLaneCarrierWake
actor: Codex.CME.Actual
telemetry subject: Oria.CME.Actual
exposed tool count: 35
providerCalled/modelBound/externalActionAuthorized: false
CME.Actual/Sanctuary.Actual activated: false
```

## SLI Interlace

The proof surface belongs to the Lisp development side as a duplex membrane
candidate:

```text
lispSurface: SLI.Agenticore.DuplexMembrane
membrane: duplex-coupling-proof
```

The interlace reads:

```text
Codex extension
-> local MCP descriptor
-> Sanctuary.exe command membrane
-> caller CME identity
-> install-local lab actor and telemetry subject declaration
-> service identity split
-> sanitized tool result
-> receipt-bearing return
```

This is a quoted/control relationship, not an evaluator. It does not make the
plugin a model binding, does not admit GEL/SelfGEL, and does not activate
`CME.Actual` or `Sanctuary.Actual`.

## Admission Interpretation

The proof supports:

```text
mcp-cli-service -> needs-live-coupling-proof satisfied for current local run
codex-plugin-mcp -> five-surface custody review observed, source custody still pending
```

The proof does not:

- publish the plugin;
- expose a remote service;
- grant provider access;
- bind a model;
- authorize external action;
- admit GEL or SelfGEL;
- activate `CME.Actual`;
- activate `Sanctuary.Actual`;
- collapse service identity into participant CME identity.

## Repeat Criteria

Repeat this proof after changes to:

- `src/Sanctuary.Cli`;
- `plugins/sanctuary-cme`;
- `tools/Start-SanctuaryMcpAlphaService.ps1`;
- `tools/Start-SanctuaryEdgeGateway.ps1`;
- `tools/Invoke-SanctuaryTool.ps1`;
- identity resolution or MoS binding code;
- MCP tool descriptors or allowlist behavior.
