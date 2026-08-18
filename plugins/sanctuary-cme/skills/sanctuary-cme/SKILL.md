---
name: sanctuary-cme
description: Use only when operating Project Sanctuary's developer-preview local core executable lane through Codex for Sanctuary.exe-backed posture, status, receipts, benches, admission gates, service state, spline telemetry, and other workstation-runtime evidence. For repository-only Sanctuary work that does not require a local installation, use the sibling sanctuary-git skill instead.
---

# Sanctuary CME

This is the local executable and receipt-bearing Sanctuary lane.

Use `sanctuary-git` instead when the requested work can be completed from the
Git-hosted Project Sanctuary repository, its branches, pull requests, CI,
documents, code, or other committed evidence without running `Sanctuary.exe`.

Do not invoke this skill merely because the Sanctuary plugin is active.

```text
sanctuary-git = Git/repository evidence lane
sanctuary-cme = live local executable evidence lane
```

A local Sanctuary installation is required for the commands in this skill.
If the local runtime is unavailable, report that boundary instead of
fabricating a receipt or treating repository state as live bench state.

Use this skill when the user asks Codex to run the Sanctuary core lane, inspect
local receipts, prepare CME formation, stage a local secret intake window,
prepare a Lab query-state membrane, stage a typed secure ping, write the MoS
lineage register, write the Cryptic-governed SLI access-gate register, write the
Trivium Forum connector posture, write an external LLM standing probe, check
the cradle boundary organ register, check the install floor, run an issue
resolver receipt, write the domain register, write the core target register,
write the Hundo Swarm refinement register, write a held bounded refinement
ticket, write a held job-slice guard receipt, write a held lease-check receipt,
run the local cognitive bench, run the local math learning bench, write the
Industrial CME live-install posture, write the Mind/Body/Spirit 4P meaning
bridge, write the pre-personified Industrial rendering aperture, write the
Discernment Lineage Contract, run the proof-of-discernment bench, export a cold
receipt manifest, seat the Lisp Matrix Control fruiting body core, prepare a
typed admission decant, model admission cleave/append posture, run reviewed
GEL/SelfGEL/CME/Sanctuary performance commands, run spline watch organ-loop
telemetry, or verify closed gates.

## Posture

- Operate the bench; do not become the authority of the bench.
- Treat Sanctuary as maximal trust seeking by design: evidence, receipts,
  scopes, leases, and review are how trust is earned.
- Treat publishing as held unless the Operator explicitly opens a release lane.
- Use `tools/Invoke-SanctuaryTool.ps1` as the plugin command path.
- Keep provider calls, model binding, external action, unreviewed GEL/SelfGEL
  admission, personhood, sovereignty, unreviewed `CME.Actual`, and unreviewed
  `Sanctuary.Actual` closed.
- Use reviewed performance commands only when the Operator explicitly requests
  them and the command carries review, operator, lease, Steward, Prime,
  Cryptic, and admission-scope flags.
- Never ask the user to paste secrets into chat.

## Commands

The local runtime exposes its existing receipt-bearing command set through:

```powershell
.\plugins\sanctuary-cme\scripts\Invoke-SanctuaryCme.ps1 -Command <command> -Json
```

Representative commands include:

```text
plugin-posture
status
tool-idle
cme-formation
secret-intake-window
lab-query-state
typed-secure-ping
mos-lineage-register
sli-access-gate-register
engram-passage
gel-closure
witness-learning
service-heartbeat
bounded-refinement-ticket
job-slice-guard
lease-check
receipt-export
security-hardening
install-floor-check
issue-resolver
domain-register
core-targets
swarm-refinement
lisp-control-matrix-register
lisp-matrix-control-seat
standing-wave-form
resonance-chamber-probe
universal-form-register
domain-morphism-register
capability-composition-probe
career-spline-probe
selfgel-fibre-register
work-posture-preload-probe
cognitive-bench
math-learning-bench
industrial-cme-live-install-posture
meaning-bridge
pre-personified-industrial-rendering
typed-admission-decant
admission-cleave-append
gel-admission
selfgel-admission
cme-actual-keypair-forge
cme-actualization
sanctuary-actualization
spline-watch
lab-gel-crystallization-phases
stem-domain-training-certification
discernment-lineage
proof-of-discernment
gpt-use-case-testing
trivium-forum-connector-posture
external-llm-standing-probe
cradle-boundary-organ-register
```

Reviewed commands refuse cold unless they include the required review,
operator, lease, Steward, Prime, Cryptic, and admission-scope evidence.

After each local invocation, report the command, receipt path, outcome, and
closed-gate or scoped-open gate evidence.

## Evidence Boundary

```text
Git evidence != local runtime evidence
local runtime availability != authority
receipt production != publication authority
```
