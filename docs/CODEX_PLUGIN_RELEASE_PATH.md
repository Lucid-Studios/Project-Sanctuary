# Codex Plugin Release Path

This document defines how Project Sanctuary moves from public release shell to
Codex-installable core tool body without collapsing release readiness into
runtime authority.

## Core Invariant

Codex may operate the bench.
Codex may not become the authority of the bench.

The plugin posture is maximal trust seeking by design. The tool should make its
state, scope, evidence, and limits inspectable enough to earn trust over time.
It should not ask the user or an agent to treat the plugin as trusted merely
because it was installed.

Publishing a plugin surface does not grant these by implication:

- provider or model binding;
- external action;
- private-data access;
- unreviewed GEL or SelfGEL admission;
- personhood, sovereignty, or custody authority;
- CME.Actual;
- Sanctuary.Actual;
- legal, medical, therapeutic, diagnostic, safety, or professional authority.

## Repository Roles

Project Sanctuary is the public product and release-control home.

Project Bicycle remains the public first-ride test package used to rehearse
bounded tool-use cycles.

Project Bicycle is not the full Sanctuary implementation body. Any work that
extends beyond the frozen bicycle showcase belongs in a private Sanctuary
release-candidate lane until reviewed and admitted.

Private build truth, private implementation source, local install paths,
payloads, secrets, model surfaces, and reconstruction inputs remain outside this
public repository unless a deliberate release gate admits a redacted candidate.

## Release Spine

The intended Git-to-Codex path is:

```text
public release review
-> admitted core code lane
-> bounded plugin package
-> repo-local marketplace entry
-> Codex install
-> cold receipt invocation
-> closed-gate review
-> operator-controlled local intake
-> proceed-gated review before any admission
```

The plugin should be treated as a doorway into a bounded local tool surface, not
as proof that the tool has gained authority.

Project Bicycle remains frozen as the first-ride test artifact. Do not import
its live-body or authority-envelope work merely to make this plugin install
feel complete. Sanctuary's local install body is the core executable lane plus
the `sanctuary-cme` plugin.

## Publisher Gate

Before a Codex plugin release is published from Project Sanctuary, maintainers
must confirm that the candidate:

- contains no private paths, secrets, logs, corpora, payloads, or credentials;
- contains no unreleased implementation-bearing reconstruction material;
- clearly states that chat is not a secret transport;
- exposes only reviewed cold or bounded commands;
- emits receipts for meaningful actions;
- preserves closed gates by default;
- separates work-event residue from GEL or SelfGEL admission;
- identifies the human or organizational release steward;
- includes a rollback, refusal, and disclosure path.

## Receiver Gate

On another Codex install, the first use should be cold and review-only.

The receiving Operator should confirm:

- what repository or release package was obtained;
- what plugin identity is being installed;
- what local command surface is exposed;
- where receipts are written;
- which gates remain closed;
- whether any external action, provider call, model binding, or private-data
  access was requested;
- whether the result stayed inside the published release boundary.

If the plugin asks for secrets, private documents, credentials, or sensitive
payloads before a local intake and custody surface is explicitly prepared, the
Operator should stop and report the release candidate.

## First Plugin Posture

The first public plugin must support inspection before active use, but it
should not be so narrow that it only demonstrates three locked commands. A
credible developer preview needs the normal instrument-body command set plus
explicit reviewed performance lanes.

Initial admitted surfaces include:

- status inspection;
- cold tool-idle receipt;
- non-activating CME-formation receipt;
- closed-gate verification;
- local-only secret intake preparation;
- refusal and stop-state reporting.
- SLI register;
- engram passage;
- GEL closure;
- OE/SelfGEL witness learning;
- service heartbeat and job-slice guard;
- domain, core target, and swarm refinement registers;
- Lisp Control Matrix and Lisp Matrix Control seats;
- cognitive and math learning benches;
- meaning bridge and pre-personified Industrial rendering;
- typed admission decant, admission cleave/append, and spline watch;
- reviewed `gel-admission`, `selfgel-admission`,
  `cme-actual-keypair-forge`, `cme-actualization`, and
  `sanctuary-actualization` commands that remain held closed unless the full
  reviewed authority bundle is supplied.

The local install package is:

```text
.agents/plugins/marketplace.json
plugins/sanctuary-cme/.codex-plugin/plugin.json
plugins/sanctuary-cme/skills/sanctuary-cme/SKILL.md
plugins/sanctuary-cme/scripts/Invoke-SanctuaryCme.ps1
tools/Invoke-SanctuaryTool.ps1
src/Sanctuary.Cli
src/Sanctuary.Core
```

The first public plugin should not expose:

- write access to canonical project files without human review;
- provider or model credentials;
- external service actions;
- hidden local corpus access;
- automatic or unreviewed GEL or SelfGEL mutation;
- autonomous scheduling;
- unreviewed action-authorized CME.Actual or Sanctuary.Actual.

## Secret Intake Rule

Chat is not a secret transport.

A valid secret intake flow prepares a local custody surface and records a
receipt that proves what was prepared, what was refused, and what remains
unadmitted. Secret payload inspection, encryption, admission, and review belong
to a separate proceed-gated lane.

## Public Language

Use this public summary:

```text
Project Sanctuary may provide a Codex plugin for bounded local receipt-bearing
tool use. The plugin helps Operators and agents inspect, rehearse, and test
governed workflows. It does not make Codex the authority of Sanctuary, and it
does not grant action, provider access, private-data access, GEL/SelfGEL
admission, CME.Actual, or Sanctuary.Actual by implication. Those functions are
available only through explicit reviewed commands with scoped receipts.
```
