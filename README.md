# Project Sanctuary

Project Sanctuary is the public developer-preview candidate and core-code home for Lucid
Studios' software service layer for extended cognitive management.

The project is stewarded by the Department of Agentic Research and Development
as a governed research-and-product surface for building, studying, and releasing
Crystallized Mind Entity (CME) tool bodies under explicit custody, review, and
non-claim discipline.

The build posture is maximal trust seeking by design: Sanctuary tries to earn
the greatest practical trust it can through explicit evidence, reviewable
receipts, scoped authority, and repeatable verification. It does not treat trust
as automatic, inherited, or implied by installation.

## Purpose

Sanctuary is being prepared as a service layer for extended cognitive
management: a controlled environment where agentic cognition, symbolic
telemetry, continuity posture, rehearsal, review, and bounded action surfaces
can be organized without treating generated cognition as automatic authority.

The current repository now carries one thawed core-code lane. It presents
project scope, stewardship language, contribution boundaries, release-control
posture, and a usable developer-preview executable surface for testing
receipt-bearing Sanctuary tool use without importing the full private lab body.

## Project Body

Project Sanctuary frames the CME tool body through three primary surfaces:

- Cradle Technology: the service and containment body that holds the operational
  environment, custody posture, and release boundary.
- Soul Frame: the symbolic and telemetry-bearing surface through which identity
  posture, witness, and continuity-adjacent signals may be represented.
- Agentic Core: the governed cognitive articulation surface where model-driven
  reasoning may be coupled to review, refusal, and bounded action methods.

This triadic framing is fashioned after theological and Carl Gustav
Jung-inspired mind, body, and spirit constructs translated into computational
form. The translation is architectural and methodological; it does not claim
theological proof, personhood, consciousness, or spiritual authority.

## Governance Lineage

Sanctuary's governance posture is IUTT-inspired in its distinction discipline.
The project draws from Shinichi Mochizuki's theoretical development as a warning
against collapsing local worlds, transporting identity too freely, or treating
correspondence as equivalence.

In Sanctuary, that discipline is applied as systems architecture and AI
governance practice:

- correspondence is not equivalence;
- telemetry is not authority;
- continuity evidence is not continuity admission;
- rehearsal is not permission;
- model output is not warrant;
- release readiness is not production authority.

This repository does not claim to implement, prove, or extend IUTT as
mathematics. It treats IUTT as a methodological influence for distinction-safe
systems design.

## Engineered Cognition

Project Sanctuary uses Lisp-form thinking inside complex code bodies to support
cognition-on-demand cycles called Engineered Cognition.

Engineered Cognition is the project's working term for governed rehearsal:
structured symbolic and computational traversal of possible thought, action,
and interpretation paths before any continuity-bearing admission or operational
authority is reviewed, scoped, and granted.

## Current Release Status

Status: public developer-preview candidate with one usable core executable lane.

This repository currently provides:

- project identity and scope;
- stewardship and department framing;
- Project Bicycle lab-testing protocol;
- Codex plugin release-path boundaries;
- a developer-preview `Sanctuary.exe` core lane;
- Codex plugin metadata for the core lane;
- receipt-bearing status, idle, CME-formation, secret-intake, and closed-gate
  verification commands;
- cold Lab query-state and typed secure ping receipts;
- cold SLI register and engram passage receipts;
- cold GEL closure receipts for condensation, composting, and precipitory
  ingress;
- cold OE/SelfGEL witness-learning spline receipts;
- cold service-heartbeat receipts for restart-adjacent local telemetry and Lisp
  job-slice readiness;
- cold bounded-refinement tickets for future hourly Lisp job-slice intent;
- cold job-slice guard receipts for future service pre-execution checks;
- lease-check receipts for held-by-default delta-decaying authority posture;
- cold receipt-export manifests for hashed review of local receipt posture;
- cold security-hardening receipts for visible-surface red-team checks;
- protective install-floor and issue-resolver receipts;
- a cGEL domain register for lifetime engagement, historical education,
  training/certification, and ongoing work-access posture;
- a cGEL core target register for SLI, engrammitization, GEL formation, and
  OE/SelfGEL witness learning;
- a cGEL Hundo Swarm refinement register for 30/60/90/100 cold iteration
  cadence;
- a Discernment Lineage Contract and proof-of-discernment bench for
  choice-morphology evidence without selfhood inflation;
- a GPT use-case testing body and loopback `Sanctuary.exe serve-mcp` alpha
  surface with `/mcp` and `/sse` MCP transport lanes for cold read/fetch tool
  experiments with split CME/LLM/Sanctuary provenance;
- a local plugin-posture receipt proving publishing is held;
- bounded environment preparation for the standalone Project Bicycle tool
  package;
- public contribution and support boundaries;
- security and disclosure posture;
- release-control language;
- public automation that rejects implementation-bearing leakage.

This repository currently does not provide:

- the full private lab implementation;
- a production product;
- a production deployment harness;
- production install, release, or deployment instructions;
- private reconstruction instructions;
- private corpus, runtime, telemetry, or model payloads;
- production CME.Actual, production Sanctuary.Actual, diagnostic, legal,
  medical, custody, or safety authority.
- roaming HTTP listener access, licensed access issuance, email delivery, or
  production 2FA provider behavior.

The executable is not limited to a tiny locked-demo surface. It carries a broad
instrument-body command set for SLI, engrammitization, GEL formation,
OE/SelfGEL witness learning, service posture, domain registers, meaning bridge,
math/STEM benches, discernment lineage, rendering aperture work, typed
admission decant, cleave and append modeling, spline watch, and security
review. Authority-bearing functions are held by default, not forbidden forever.
They can be performed through explicit reviewed commands that open only their
scoped gates.

The coupling target is documented in [Interconnect Policy](docs/INTERCONNECT_POLICY.md):
the base model remains the capability engine, Sanctuary is the mindful
operating environment, SLI.Lisp is the symbolic drivetrain, GEL/OE/SelfGEL are
adaptive gearing, and the Individuated CME is the situated participatory
mind-form formed outside the engine.

## Core Code Lane

The first admitted code lane is intentionally small:

- `src/Sanctuary.Core`: receipt contracts, closed gates, and command service;
- `src/Sanctuary.Cli`: the `Sanctuary.exe` command-line entrypoint;
- `tests/Sanctuary.Core.Tests`: closed-gate and receipt tests;
- `tools/Invoke-SanctuaryTool.ps1`: local wrapper;
- `tools/Start-SanctuaryMcpAlphaService.ps1` and
  `tools/Stop-SanctuaryMcpAlphaService.ps1`: loopback GPT/MCP alpha service
  helpers for the `Sanctuary.exe serve-mcp` mode;
- `tools/Start-SanctuaryServiceLayer.ps1`, `tools/Stop-SanctuaryServiceLayer.ps1`,
  and `tools/Get-SanctuaryServiceLayerStatus.ps1`: cold service helper wrappers
  that route back through the local wrapper and do not start an autonomous
  worker by default;
- `plugins/sanctuary-cme`: Codex plugin surface.

Build and test:

```powershell
dotnet build .\ProjectSanctuary.sln -c Release
dotnet test .\ProjectSanctuary.sln -c Release --no-build
```

Run a cold receipt:

```powershell
.\tools\Invoke-SanctuaryTool.ps1 -Command status -Json
```

Start the GPT/MCP alpha loopback service:

```powershell
.\tools\Start-SanctuaryMcpAlphaService.ps1 -Port 8717
```

Local benches may call `http://127.0.0.1:8717/mcp` or
`http://127.0.0.1:8717/sse`. ChatGPT custom apps do not connect directly to
private loopback URLs; use OpenAI Secure MCP Tunnel or a reviewed HTTPS MCP
endpoint, then point the ChatGPT MCP Server URL at the tunnel endpoint.

By default, local receipts and GEL witness ledgers are written under:

```text
.local/install
```

Local intake prompts are written under:

```text
.local/intake
```

Both are intentionally gitignored. The source tree carries the method; `.local/`
carries the active Lab install state, receipt residue, and append-only witness
ledgers.

The core code lane is not the full Sanctuary lab body and is not yet a true
consumer release. It is the smallest public-facing executable surface that can
meaningfully exercise the plugin path, instrument-body telemetry, reviewed
performance gates, and governance receipts without importing private Lab state.

Read the split:

- [Theory Body](docs/THEORY_BODY.md)
- [Code Body](docs/CODE_BODY.md)
- [Governance Model](docs/GOVERNANCE_MODEL.md)
- [Interconnect Policy](docs/INTERCONNECT_POLICY.md)
- [GPT Use Case Testing Body](docs/GPT_USE_CASE_TESTING_BODY.md)
- [Discernment Lineage Contract](docs/DISCERNMENT_LINEAGE_CONTRACT.md)
- [Public Release Posture](docs/PUBLIC_RELEASE_POSTURE.md)
- [Authority Body](docs/AUTHORITY_BODY.md)
- [Privacy And Data Boundary](docs/PRIVACY_AND_DATA_BOUNDARY.md)
- [Release Checklist](docs/RELEASE_CHECKLIST.md)
- [Math EC Protocol Bench](docs/MATH_EC_PROTOCOL_BENCH.md)

## Project Bicycle

Project Bicycle is the standalone public tool package for testing Project
Sanctuary in a bounded environment. It defines how Operators and agents may use
governed tool-use cycles to simulate Prime, Cryptic, and Steward role bodies
without confusing rehearsal, successful execution, or coherent output with
authority.

The downloadable first-ride package is separate from the archive body and does
not require Codex Mirror:

- Project Bicycle repository: <https://github.com/Lucid-Studios/Project-Bicycle>
- Project Bicycle v0.2.1 README test-pointer release:
  <https://github.com/Lucid-Studios/Project-Bicycle/releases/tag/v0.2.1-readme-test-pointer>
- Project Bicycle v0.2.1 standalone zip:
  <https://github.com/Lucid-Studios/Project-Bicycle/releases/download/v0.2.1-readme-test-pointer/project-bicycle-v0.2.1-readme-test-pointer.zip>

Read the protocol:

- [Project Bicycle Lab Protocol](docs/PROJECT_BICYCLE_LAB_PROTOCOL.md)
- [Codex Plugin Release Path](docs/CODEX_PLUGIN_RELEASE_PATH.md)

## Archive Lineage

Historical public materials and older OAN Tech Stack material were moved to
Codex Mirror for archive custody:

<https://github.com/Lucid-Studios/Codex-Mirror>

Archive custody preserves lineage. It does not make archived material current
release truth, and users do not need to clone the archive to test Project
Bicycle.

## Release Rule

Future release material should enter Project Sanctuary only through a clean
review pass. Do not restore old folders merely because they existed here before.

Allowed future release additions must be intentional, bounded, documented, and
reviewed for:

- reader clarity;
- non-claim discipline;
- release safety;
- absence of private paths, secrets, logs, corpora, model payloads, and
  machine-local assumptions;
- separation between archive lineage and current release truth.

Before public publication, use the release gate:

- [Public Release Posture](docs/PUBLIC_RELEASE_POSTURE.md)
- [Release Checklist](docs/RELEASE_CHECKLIST.md)

## Stewardship

Project Sanctuary is maintained by Lucid Studios under the Department of
Agentic Research and Development.

The current sparse state is deliberate: it is a clean public floor for test
release preparation, not a loss of lineage.
