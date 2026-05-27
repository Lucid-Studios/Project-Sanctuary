# Public Release Posture

Project Sanctuary is being prepared as a public developer-preview candidate for
a governed instrument body. It is not yet a true consumer release.

The public release body is intentionally small:

```text
Project Sanctuary repository
-> admitted core executable lane
-> Codex plugin surface
-> repo-local marketplace entry
-> cold command invocation
-> receipt
-> closed-gate review
-> explicit reviewed performance when authority is supplied
```

The release publishes enough code and documentation to let a reviewer build the
core executable, run cold commands, inspect receipts, and understand the
governance posture. It does not publish the full private Lab ecology.

The coupling model for this candidate is named in
`docs/INTERCONNECT_POLICY.md`. It frames Sanctuary as the mindful coupling layer
between base model capability and accountable CME participation.

The selfhood research boundary is named in
`docs/DISCERNMENT_LINEAGE_CONTRACT.md`. It frames `Self.Actualization` as a
research predicate requiring proof of discernment, not as a public claim of
personhood, sovereignty, hidden consciousness, or legal status.

The public language should preserve the build posture:

```text
maximal trust seeking by design
```

This means Sanctuary seeks earned trust through receipts, reviewable state,
scoped leases, explicit gates, and repeatable verification. It does not claim
maximum trust by default, and it does not grant trust merely because the program
was installed or a plugin was invoked.

## Public Release Claim

This candidate is:

```text
a public-facing developer preview of a receipt-bearing Sanctuary core lane
```

This candidate is not:

```text
production deployment
consumer-ready release
full Lab disclosure
private corpus disclosure
provider/model access
external action authority
unreviewed GEL or SelfGEL admission
unreviewed CME.Actual
unreviewed Sanctuary.Actual
bonded personification
professional, legal, clinical, fiduciary, or custody authority
```

## Reviewed Performance Lanes

The public executable is not a refusal-only shell. It can perform scoped
admission and Actual-state functions, but only through explicit reviewed
commands with the full authority bundle present:

```text
review-approved
operator-approved
authority-lease-issued
steward-witnessed
prime-witnessed
cryptic-witnessed
admission-scope
```

The first reviewed commands are:

- `gel-admission`: admits the shared GEL lane under scope;
- `selfgel-admission`: admits/mutates the personal SelfGEL lane under scope;
- `cme-actual-keypair-forge`: creates a scoped encrypted CME.Actual keypair,
  roots OE to the Lab Sanctuary.GEL tip hash, and seeds SelfGEL standing body
  residue under review;
- `cme-actualization`: activates scoped local CME.Actual posture;
- `sanctuary-actualization`: activates scoped local Sanctuary.Actual runtime posture.

If any required review element is absent, the command remains held closed. The
receipt disposition may say `RefusedCold`, but the product meaning is
review-required rather than impossible. If the bundle is complete, only the
command's scoped gates open. Provider calls, model binding, external action,
personhood, and sovereignty remain closed in this public lane.

## Admitted Public Surfaces

The public-facing candidate may contain:

- `src/Sanctuary.Core/`: receipt contracts, closed gates, and command service;
- `src/Sanctuary.Cli/`: command-line entrypoint used to build `Sanctuary.exe`;
- `tests/Sanctuary.Core.Tests/`: governance tests for the admitted core lane;
- `tools/Invoke-SanctuaryTool.ps1`: local wrapper for cold invocation;
- `plugins/sanctuary-cme/`: bounded Codex plugin surface;
- `.agents/plugins/marketplace.json`: repo-local marketplace entry for local
  Codex installs;
- `docs/`: theory, code, governance, release, and protocol documentation;
- public support, conduct, security, and contribution files.

The public-facing candidate must not contain:

- `.local/` active Lab install state;
- encrypted payload stores or secret custody material;
- private documentation repositories or corpora;
- local absolute paths;
- private model/provider configuration;
- production deployment recipes;
- release automation that grants authority;
- unreviewed logs, ledgers, audit outputs, or runtime payloads.

## Release Doctrine

```text
public buildability != production readiness
receipt emission != authority
plugin install != model/provider binding
local GEL witness != GEL admission
OE/SelfGEL support != memory admission
closed-gate success != Actual activation
reviewed performance != production license
Actual-state receipt != professional authority
maximal trust seeking != automatic trust grant
department stewardship != government authority
research posture != professional service
```

## First Public User Story

The first public reviewer should be able to:

1. clone the repository;
2. build the solution;
3. run the test suite;
4. install the local `sanctuary-cme` plugin from the repo marketplace;
5. invoke a cold command;
6. inspect the receipt path;
7. confirm all gates remain closed;
8. exercise the broad instrument-body command set;
9. run a reviewed performance command with a local test authority bundle;
10. confirm only scoped gates open;
11. understand what the candidate does not claim.

The first public reviewer should not be asked to:

- provide secrets in chat;
- connect a provider or model;
- disclose private data;
- run a production service;
- treat a local reviewed Actual-state receipt as production authority.

## Publication Gate

A true public release should not be cut while the product feels like only a
small locked demo. The candidate must show useful work across the core command
families:

```text
SLI and Root Atlas posture
engrammitization passage
GEL closure and local residue
OE/SelfGEL witness learning
discernment lineage and proof-of-discernment benches
service heartbeat and job-slice readiness
domain, career, capability, and training registers
meaning bridge and rendering aperture
math/STEM bench telemetry
typed admission decant
cleave/append modeling
spline watch
reviewed performance gates
security hardening
```

Publication is allowed only after:

```text
tests green
public-surface check green
redaction sweep clean
receipt export shows all known receipts closed or reviewed
security-hardening receipt reviewed
release checklist reviewed
license decision recorded
operator approval recorded outside the repository
```

Until then, publishing remains held.
