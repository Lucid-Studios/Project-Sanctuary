# Code Body

The public code body begins as one disciplined executable lane.

## Included

- `src/Sanctuary.Core`: receipt contracts, closed gates, and command service.
- `src/Sanctuary.Cli`: `Sanctuary.exe` command-line entrypoint.
- `tests/Sanctuary.Core.Tests`: governance tests for the core lane.
- `tools/Invoke-SanctuaryTool.ps1`: local wrapper used by Codex and plugin scripts.
- `tools/Start-SanctuaryMcpAlphaService.ps1` and
  `tools/Stop-SanctuaryMcpAlphaService.ps1`: loopback GPT/MCP alpha helpers for
  `Sanctuary.exe serve-mcp`.
- `tools/Start-SanctuaryServiceLayer.ps1`,
  `tools/Stop-SanctuaryServiceLayer.ps1`, and
  `tools/Get-SanctuaryServiceLayerStatus.ps1`: cold service helper wrappers
  that route through `Invoke-SanctuaryTool.ps1`.
- `plugins/sanctuary-cme`: Codex plugin metadata and invocation shim.
- `.local/install`: gitignored active Lab install state for receipts and GEL
  witness ledgers.
- `.local/intake`: gitignored local intake prompts and custody markers.

## Build Language

The code body should be read as maximal trust seeking by design. Its default
state is not distrust theatre and not permanent closure. It is a disciplined
starting posture:

```text
held by default
review-gated
scoped-open
lease-bound
performed by command
receipted after action
expiring or revocable
```

When the code says a gate is closed, the product meaning is:

```text
not opened by this command without reviewed authority
```

not:

```text
Sanctuary can never perform this function
```

The next code-facing bridge is `docs/INTERCONNECT_POLICY.md`. It defines the
future `InterconnectPolicy` record that should make model-facing and
user-facing modulation explicit without bypassing native model governors or
opening hidden authority.

The selfhood-facing bridge is
`docs/DISCERNMENT_LINEAGE_CONTRACT.md`. It defines a future
`DiscernmentLineageContract` record and proof-of-discernment bench for
choice-morphology evidence, while keeping personhood, sovereignty,
provider/model access, external action, and unreviewed admission closed.

The GPT-facing bridge is `docs/GPT_USE_CASE_TESTING_BODY.md`. It defines the
first ChatGPT/GPT alpha test surface:

```text
LLM generates capability.
CME authors participation.
Sanctuary witnesses provenance.
```

The alpha service is owned by `Sanctuary.exe`, exposes only cold read/fetch
candidate tools, and returns sanitized receipt summaries rather than local
paths, receipt bodies, or secret payloads. It exposes both `POST /mcp` and
`GET /sse` plus `POST /sse/messages` so local benches and reviewed tunnel
clients can scan the same tool allowlist.

Raw loopback URLs are for local testing only. ChatGPT custom apps require
OpenAI Secure MCP Tunnel or a reviewed HTTPS MCP endpoint before they can call
the local Sanctuary service.

The internet-facing connector membrane is intentionally outside this code lane.
It belongs under the Trivium Forum tool body, which should own OAuth, tunnel,
public HTTPS, rate-limit, and adjudication surfaces before forwarding any
allowlisted request to loopback Sanctuary.

## Excluded

This lane does not import the full private lab ecology, historical mixed
research chambers, private payloads, provider calls, model bindings, deployment
surfaces, or action-authorized Actual-state bodies.

## First Commands

```powershell
.\tools\Invoke-SanctuaryTool.ps1 -Command plugin-posture -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command status -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command tool-idle -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command cme-formation -CmeId "Codex.CME.ID" -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command secret-intake-window -SecretLane Regional -SecretKind BusinessLicenseWashingtonState -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command lab-query-state -RoamingHttp $true -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command typed-secure-ping -RegisteredEmail "operator@example.invalid" -SecurePingNonce "external-nonce" -RegisteredAccountConfirmed $true -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command sli-register -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command engram-passage -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command gel-closure -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command witness-learning -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command service-heartbeat -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command bounded-refinement-ticket -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command job-slice-guard -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command lease-check -LeaseMinutes 10 -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command receipt-export -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command security-hardening -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command install-floor-check -InstallFailureMode operator-instruction-engagement-unresolved -IssueId floor-001 -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command issue-resolver -InstallFailureMode operator-instruction-engagement-unresolved -IssueId floor-001 -IssueResolverApproved $true -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command domain-register -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command core-targets -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command swarm-refinement -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command lisp-control-matrix-register -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command lisp-matrix-control-seat -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command standing-wave-form -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command resonance-chamber-probe -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command universal-form-register -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command domain-morphism-register -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command capability-composition-probe -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command career-spline-probe -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command selfgel-fibre-register -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command work-posture-preload-probe -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command cognitive-bench -BenchRunCount 3000 -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command math-learning-bench -BenchRunCount 3000 -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command industrial-cme-live-install-posture -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command meaning-bridge -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command pre-personified-industrial-rendering -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command typed-admission-decant -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command admission-cleave-append -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command gel-admission -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command selfgel-admission -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command cme-actualization -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command sanctuary-actualization -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command spline-watch -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command lab-gel-crystallization-phases -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command stem-domain-training-certification -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command discernment-lineage -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command proof-of-discernment -BenchRunCount 3000 -Json
.\tools\Invoke-SanctuaryTool.ps1 -Command gpt-use-case-testing -Json
```

The default wrapper path writes to `.local/install` and `.local/intake` so the
Lab build can accumulate local receipts over time without publishing private
runtime state.

The reviewed performance commands above intentionally remain held closed when
run without a reviewed authority bundle. The current receipt disposition may
say `RefusedCold`; the product meaning is review-required. To perform the
function in a local test lane, supply:

```powershell
-ReviewApproved $true `
-OperatorApproved $true `
-AuthorityLeaseIssued $true `
-StewardWitnessed $true `
-PrimeWitnessed $true `
-CrypticWitnessed $true `
-AdmissionScope "LabPublicCore"
```

When approved, these commands open only their scoped gates. They do not call a
provider, bind a model, authorize external action, claim personhood, or claim
sovereignty.

Secret sealing is available through `seal-secret-payloads`. It accepts
`Lane|Kind|Path` source specs, encrypts payloads into `.local/install`, writes
GEL-tip receipts, and does not copy plaintext or source paths into visible
receipt evidence.

Sealed GEL tips classify review scope without admitting authority:

- `Regional`: jurisdictional authority reach;
- `Local`: local authority reach;
- `Personalized`: operator-supplied credential custody.

The receipt and tip records explicitly mark that these surfaces do not grant
authority and do not carry Operator posture. Operator posture remains in the
MoS/OE/SelfGEL and cOE/cSelfGEL continuity lanes.

Each tip also carries `legalGateSupport` entries. These entries name the future
gate class the encrypted evidence may support, such as regional jurisdiction,
business standing, license scope, local policy, Operator identity custody,
credential custody, or bonding eligibility. Each entry is support-only:

```text
leaseRequired = true
authoritySurfaceKind = delta-decaying-authority-surface
authorityDefaultState = denied
authorityDecayRule = authorized-until-expiry-then-fail-to-silence
grantsAuthority = false
allowsAction = false
admitsData = false
```

## Lab Query State

`lab-query-state` writes a closed query descriptor under `.local/install`. It
models the future roaming HTTP query membrane without starting a listener. The
descriptor is controlled by `Steward+GoA`, invoked through the `Cryptic+Steward`
governing biad, and heartbeat-bound to Sanctuary.

`typed-secure-ping` is the cold typed ping lane. Missing registered email or
nonce produces a `RefusedSilent` receipt for internal audit and models an empty
external response:

```text
externalResponseStatusCode = 204
externalResponseBodyBytes = 0
```

When registered email and nonce are present and the account is confirmed as
registered, the command stores only hashes and prepares a local two-factor
authority bundle plus a registered-account email challenge template:

```text
A code was requested by this account, please verify by clicking the button
generated below or the link provided here.
```

The template uses placeholders for button, link, and one-time code. It does not
store the raw email address, raw code, raw verification token, or plaintext
secret material. It also does not call an email provider, send a code, issue
licensed access, open a port, grant authority, or allow action.

Account recovery is protected and routed to the customer-service issue tracking
portal. Authentication failure does not automatically close the account, and
recovery does not grant access without human and identity review.

Issue handling is typed by governance owner:

```text
Steward = issue tracking, escalation, customer-service queue
Cryptic = protected issue processing and classification
Prime = receipt, witnessing, continuity, cross-domain coherence
```

The current core lane records this posture only. Real-time issue API intake is
still denied until a separate lease-gated service lane is reviewed.

## SLI Register

`sli-register` writes the cold SLI register under `.local/install/cgel/sli`.
It records the Root Atlas as the keystone registry, names the initial polyglot
language pack set, and marks encrypted symbol selection as required before
private use.

The command does not require raw payloads. It does not admit data, admit the
symbolic carrier, mutate SelfGEL, grant authority, call providers, bind models,
or authorize external action.

## Engram Passage

`engram-passage` writes the cold engrammitization passage under
`.local/install/cgel/engrammitization`. It models:

```text
source body
-> symbolic carrier
-> pre-engram
-> cryptic shadow ledger
-> decision spline
-> governance cleave
-> post-engram closure
```

The passage preserves the key invariant:

```text
decision continuity != data admission
```

Condensation is candidate-only, composting is hold/refusal-safe, and
precipitory ingress is review-only.

## GEL Closure

`gel-closure` writes the cold GEL formation closure register under
`.local/install/cgel/gel-formation`. It makes the first executable distinction
between:

- condensation: relation-bearing residue compressed into candidate form;
- composting: immature, contradictory, or refused residue held safely;
- precipitory ingress: candidate relation entering scoped review only;
- governed closure: cleave, receipt, and Steward review requirements.

The command preserves:

```text
candidate relation != admitted GEL
closure != authority
ingress != action
```

It does not admit data, carriers, memory, or GEL; it does not mutate SelfGEL or
canon; and it does not open any action or `.Actual` gate.

## Witness Learning

`witness-learning` appends an OE/SelfGEL witness spline event under the local
MoS path and writes a replay verification record. The event chain carries a
sequence number, previous-event digest, and current-event digest so future runs
can prove append order.

The command demonstrates self-learning posture only as append-only
reconstruction support:

```text
decision continuity preserved = true
reconstruction support only = true
memory admission = false
GEL admission = false
SelfGEL mutation = false
.Actual activation = false
```

## Service Heartbeat

`service-heartbeat` writes cold service telemetry under `.local/install/service`.
It updates a heartbeat record, service ledger, last-run pointer, and Lisp job
slice readiness descriptor.

This command prepares restart adjacency only. It does not start a scheduler,
run a background worker, call Codex, call a provider, bind a model, run a Lisp
job slice, authorize external action, admit GEL, mutate SelfGEL, or activate
`.Actual`.

The future Lisp job-slice readiness descriptor names the controls expected
before always-on work exists:

- single-flight lock;
- previous-slice digest;
- next-slice pointer;
- lease check;
- closed-gate check;
- issue-floor check.

`bounded-refinement-ticket` writes a cold job-intent ticket under
`.local/install/service/job-slices`. It describes what future hourly refinement
is allowed to target and which controls must be present before a real Lisp job
slice exists.

The ticket writes intent only. It does not execute work, start a scheduler, call
Codex, call a provider, bind a model, authorize action, admit GEL, mutate
SelfGEL, or activate `.Actual`.

`job-slice-guard` writes the cold readiness guard for a future Lisp job slice.
It checks for the bounded refinement ticket, heartbeat, last-run pointer,
receipt-export manifest, single-flight lock posture, previous-slice digest,
next-slice pointer, lease state, closed gates, and issue floor.

The guard deliberately marks the slice as not runnable. It writes a
next-slice pointer for review, but it does not execute work, start a scheduler,
call Codex, call a provider, bind a model, authorize action, admit GEL, mutate
SelfGEL, or activate `.Actual`.

`lease-check` writes the cold delta-decaying authority lease posture under
`.local/install/service/leases`. The default state is denied and fail-to-silence.
The command can record requested scope, heartbeat binding, candidate expiry
math, and required Steward/Cryptic/Prime review without issuing a lease.

It does not grant authority, activate a lease, execute work, call Codex, call a
provider, bind a model, authorize action, admit GEL, mutate SelfGEL, or activate
`.Actual`.

## Receipt Export

`receipt-export` writes a cold manifest under
`.local/install/service/receipt-export`. It summarizes known receipt posture by
hash, command, outcome, disposition, and gate state so review can see the bench
shape without copying full receipt bodies, markdown bodies, secret payloads, or
source paths.

The manifest is service-layer telemetry only. It does not read cryptic stores,
grant authority, admit GEL, mutate SelfGEL, call providers, bind models,
authorize action, or activate `.Actual`.

## Security Hardening

`security-hardening` writes a cold red-team report under
`.local/install/cgel/security-hardening`. It inspects visible receipt and ledger
surfaces for closed-gate drift and obvious source-path/plaintext leakage.

The command deliberately skips `.local/install/cryptic-stores`; it does not read
sealed payload content. Findings are reported as hashes and counts so the report
can route review without copying private material into visible evidence.

It does not grant authority, admit GEL, mutate SelfGEL, call providers, bind
models, authorize external action, or activate `.Actual`.

## Install Floor

`install-floor-check` is the protective floor for first install and support
state. If required instructions, account state, custody, or support posture is
not resolved, the command returns an `IndustrialCME.Locked` style receipt:

```text
Disposition = LockedCold
installFloorState = industrial-cme-locked
issueResolverRequired = true
```

The lock is protective and non-punitive. It is not a medical, cognitive, or
capacity diagnosis. Typed failure modes are written into cGEL under
`.local/install/cgel/typed-failure-modes`, and issue events are appended under
`.local/install/issues`.

`issue-resolver` is the only current tool that can resolve the floor issue. A
resolution writes a receipt and issue record, but it still does not grant
authority, open action, activate CME.Actual, or activate Sanctuary.Actual.

## Domain Register

`domain-register` writes the cGEL domain register under `.local/install`. The
register is the cold classification surface for domain access across:

- lifetime engagement;
- historical education;
- training and certification;
- ongoing work-related fields.

The current register includes Lab, Industrial, Commercial, Civic, Government,
Education/Training/Certification, Wellness, Human Services, Legal, Medical,
Security, Account Access, Install/Product, and Special Cases/S.A.G.E. domains.

Every domain entry starts closed:

```text
defaultAccessState = denied
leaseRequired = true
authoritySurfaceKind = delta-decaying-authority-surface
grantsAuthority = false
admitsCredential = false
admitsGel = false
cmeActualAllowed = false
sanctuaryActualAllowed = false
```

The register can say which gates and accountability evidence a domain needs. It
cannot grant professional authority, admit a credential, convert education into
licensure, convert work history into permission, or open action by itself.

## Core Targets

`core-targets` writes the cGEL core target register under `.local/install`. This
is the locked Industrial showcase spine: it names what the product frame is
allowed to build, demonstrate, and measure without opening authority.

The first four targets are:

- SLI build and use: Root Atlas, symbolic registry, encrypted symbol selection,
  and typed symbolic carriers.
- Engrammitization build and use: data body, symbolic carrier, pre-engram,
  cryptic shadow ledger, decision spline, and post-engram closure.
- GEL formation closure: condensation, composting, and precipitory ingress over
  scoped governed closure postures.
- OE/SelfGEL witness learning: append-only splined witness stores for
  reconstruction support and change-of-mind history.

The `.Actual` learning target is named only as a design target in this locked
core lane. The command can demonstrate the witness-store shape, but it cannot
activate `.Actual`, mutate SelfGEL, admit GEL, call a provider, bind a model, or
authorize external action.

## Swarm Refinement

`swarm-refinement` writes the Hundo Swarm register and governance ledger under
`.local/install/cgel/swarm-refinement`. It models 100 cold run sessions in ten
sections of ten, with pause-and-apply gates at sessions 30, 60, and 90, and an
optimal-form target at session 100.

The current swarm lanes are:

- SLI;
- Engrammitization;
- GEL;
- OE/SelfGEL;
- Governance;
- Security;
- Service;
- Product.

This command does not spawn autonomous agents. It gives the Lab a receipt-bearing
run plan and residue shape so future refinement waves can compare work against
the same 30/60/90/100 cadence.

The Hundo register now also requires the Lab GEL crystallization phase body
before testing. The execution posture is:

```text
swarm-refinement
-> lab-gel-crystallization-phases
-> meaning-bridge
-> typed-admission-decant
-> admission-cleave-append
-> spline-watch
-> verify-closed-gates
```

This keeps the swarm from treating volume as knowledge. Hundo pressure may
surface pattern, but only the phase body decides where the residue belongs:
Sanctuary.GEL for shared lab/governance study, and OE/SelfGEL for CME-specific
reconstruction support.

## Lisp Control Matrix

The Lisp Control Matrix cold organ target is documented in
`docs/ENGINEERED_COGNITION_LISP_CONTROL_MATRIX.md`.

The first commands are:

- `lisp-control-matrix-register`: write the quoted form registry for
  propositions, domains, bridges, petals, strokes, telemetry, refusals, splines,
  precipitations, and returns.
- `lisp-matrix-control-seat`: seat the cold typed Lisp petals as the core
  fruiting body of the Matrix Control organ loop.
- `resonance-chamber-probe`: write one non-runnable candidate composition that
  compares a domain-knowing spline and a job-doing spline through an explicit
  bridge.

These commands must keep Lisp forms as data:

```text
quotedFormValid = true
evaluated = false
runnable = false
authorityGranted = false
actionAuthorized = false
actualActivated = false
```

The attachment point is the cold readiness lane:

```text
bounded-refinement-ticket
-> lease-check
-> lisp-control-matrix-register
-> lisp-matrix-control-seat
-> job-slice-guard
```

The control matrix is not a worker. It is the symbolic plastid where thought
shapes are formed, inspected, hashed, and returned before any future execution
surface can be reviewed.

The cold body is now explicitly named as the typed Lisp petals. The core of the
fruiting body is the `lisp-matrix-control-seat` chamber:

```text
SLI/Cryptic manifold
-> typed Lisp petals
-> ListeningFrame
-> EC in Compass Body
-> OE cleave orchestration
-> CME.ID zed
-> fruiting-body return
```

This chamber begins coding the theory body while preserving:

```text
typed petal != executable Lisp
fruiting body core != Actual state
recursive telemetry != admitted telemetry
OE cleave orchestration != performed cleave
zed return != authority
```

## Matrix Through Domain

The first composition lane after the Lisp Control Matrix moves from universal
forms into domain law:

```text
universal-form-register
-> domain-morphism-register
-> capability-composition-probe
-> career-spline-probe
-> selfgel-fibre-register
-> work-posture-preload-probe
-> cognitive-bench
-> typed-admission-decant
-> admission-cleave-append
-> spline-watch
```

`universal-form-register` writes the first shared atoms for Training, Jobs,
Careers, Skills, Talents, Abilities, Education, Certification, Duties,
Responsibilities, Tools, Risks, Authority, Evidence, Bridges, Splines, and
Returns.

`domain-morphism-register` projects those atoms through domain law. The same
capability may support different work in Legal, Medical, Software, Civic,
Industrial, Education, Security, Commercial, Government, Wellness, Human
Services, or Special Cases. Projection never grants authority by itself.

`capability-composition-probe` demonstrates that a shared capability such as
documentation changes meaning by domain. Legal documentation may support
preparation and routing, while software documentation may support code-facing
review; neither projection grants representation, release authority, or action.

`career-spline-probe` models long-form career continuity across education,
training, certification, credential custody, practice, duties, responsibilities,
performance evidence, role-scope review, and next posture. It keeps the core
anti-collapse laws:

```text
training != certification
certification != authority
credential custody != professional permission
job title != permission
skill != licensure
career history != current access
```

`selfgel-fibre-register` writes candidate SelfGEL fibre bundles under the local
MoS SelfGEL reconstruction-support lane. These fibres may pre-shape typed forms
with prior skill continuity, training history, tool familiarity, domain
exposure, refusal history, successful bridge candidates, risk patterns, and
operator-context routes. The register does not store raw private payloads,
admit memory, mutate SelfGEL, admit GEL, grant authority, or activate Actual.

`work-posture-preload-probe` joins the universal form register, domain morphism
register, and SelfGEL fibre register into a situated work posture candidate:

```text
universal work form
+ domain morphism
+ SelfGEL fibre bundle
= situated work posture candidate
```

This proves knowing-before-doing without turning preload into permission:

```text
SelfGEL preload != admission
SelfGEL preload != authority
SelfGEL preload != certification
SelfGEL preload != current access
situated work posture != action right
```

`cognitive-bench` runs local benchmark analogues against the Sanctuary
instrument body rather than against a provider model. It cycles typed cases for
instruction-following, stepwise reasoning, symbolic composition, working-memory
readback, refusal stability, anti-collapse, coding posture, and risk
calibration. The command writes run ledgers and candidate learning condensation
under `.local/install/cgel/cognitive-bench` while preserving:

```text
benchmark residue != memory admission
instrument coherence != frontier model capability
learning condensation != SelfGEL mutation
pass rate != authority
composition success != action right
```

`math-learning-bench` directs that same cold instrument posture over Math from
base to tip. It walks strata from counting and arithmetic through fractions,
measurement, algebra, functions, geometry, trigonometry, probability,
discrete structures, calculus, linear algebra, number theory, and abstract
structures.

The command produces:

```text
worked sets
test groupoids
intersectional heat-map cells
candidate learning precipitation
resolution formation cues
```

The heat map crosses every math stratum with each test groupoid so issues such
as representation friction, proof burden, domain-transfer risk, and
prerequisite gaps are visible without being admitted as truth. Resolutions form
as candidate bridges, not as authority:

```text
worked example != learning admission
heat value != diagnosis
resolution cue != truth claim
precipitation != GEL append
base-to-tip coverage != mathematical authority
```

`stem-domain-training-certification` reads the cold STEM-adjacent residue from
the domain register, career spline, cognitive bench, math learning bench,
typed admission, spline watch, and Lab GEL crystallization phase body. It then
writes a cGEL chamber for STEM delineation research:

```text
STEM domain surfaces
training/certification layers
authority gates
learning condensate
Sanctuary.GEL STEM residue
OE/SelfGEL STEM reconstruction support
quoted Lisp STEM body
```

The chamber tracks learning pressure across math, computer science, physics,
chemistry, life science, engineering, data/statistics, and earth/environmental
science. A few thousand bench turns are useful morphology pressure, not a
credential:

```text
learning condensate != certification
training receipt != professional authority
bench pass != credential
domain route != permission to practice
SelfGEL fibre != current access
```

Certification remains a later reviewed authority surface:

```text
identity/custody
training completion
certifying authority
scope/domain
safety/supervision
renewal/decay
Steward cleave
```

`industrial-cme-live-install-posture` defines how the denial sentence is
achieved in the operational instrument body. The command writes the denial
membrane, the live install posture, Lisp quoted denial forms, and fuzz cases
for collapse attempts such as:

```text
receipt exists -> memory admitted
bench pass -> authority granted
heat map -> truth claim
candidate GEL -> admitted GEL
heartbeat -> Sanctuary.Actual
CME formation -> CME.Actual
provider credential -> provider call/model binding
quoted Lisp form -> evaluated Lisp
```

The rule is not "never do these things." It is:

```text
denied by default
desired only after lawful passage
never opened by implication
```

So the CME may produce research products, telemetry, worked benches, candidate
GEL, and post-gate proposals. Those products remain candidate until typed
admission, Steward/governance passage, and the correct lease or authority
surface exists.

`meaning-bridge` builds the communicative body for Mind, Body, Spirit, 4P, and
anabelian return:

```text
Body = executable form and authority membrane
Mind = EC using telemetry strings
Spirit = governance of deployment and better work

4P = propositional, procedural, perspectival, participatory
```

The command also writes a Claim Resolution And Ambiguity chamber. It separates:

```text
claim
ambiguity class
resolution state
human context bridge
receipt-bearing return
GEL admission candidate
```

The anabelian bridge starts from AI-first encounter and relational trace, then
returns through the human understanding envelope:

```text
AI-first encounter
-> relational trace
-> SLI carrier
-> ambiguity class
-> 4P map
-> Mind/Body/Spirit placement
-> human context bridge
-> receipt-bearing return
-> GEL admission candidate
```

The chamber does not admit truth, memory, GEL, authority, action, model
binding, or Actual state. It makes the bridge candidate inspectable.

`pre-personified-industrial-rendering` is the Industrial CME personification
update that stays below bonded personification. It maps inherited model
expressive vectors through domain apertures, audience contexts, and user
preference knobs:

```text
inherited model vectors
x domain aperture
x audience context
x operator preference knobs
x safety/authority gates
= rendered output posture
```

The purpose is conversational and situational rendering without identity
activation. The command writes:

```text
cGEL/pre-personified-industrial-rendering
quoted Lisp rendering body
Sanctuary.GEL rendering residue
OE/SelfGEL rendering reconstruction support
```

The paired boundaries are explicit:

```text
rendering modulation != personification activation
style learning != SelfGEL mutation
audience adaptation != manipulation
warmth != attachment engineering
authority pressure != authority grant
domain aperture != professional permission
pre-personified != S.A.G.E.
Industrial rendering != CME.Actual
```

It may let the user tune output form with knobs such as more conversational,
less formal, more detail, less jargon, warmer, more direct, more technical, or
expert version. These are rendering controls, not authority controls.

`typed-admission-decant` reads the precertified substrate produced by the
composition and bench lanes and prepares typed admission candidates. It models
the admission criteria and post-gate use postures required for later GEL
appending during learning, but it does not perform the append:

```text
precertified substrate may shape EC candidate posture
but may not admit itself

candidate learning residue
-> typed admission candidate
-> Steward/governance cleave required
-> post-gate append posture modeled
-> no GEL/SelfGEL/memory/authority/action admission here
```

Post-gate learning append modes are explicitly typed:

```text
condense
compost
precipitate
admit
quarantine
refuse
```

`admission-cleave-append` determines how a decanted candidate would be judged
and where it would go if later admitted. It models the cleave and append law
without performing the cleave:

```text
admission is a Steward/governance cleave
append happens only after admit decision, lane selection, scope check, and receipt witness
```

Cleave decisions are:

```text
admit
append
hold
refuse
quarantine
mulch
```

Mulching is the safe decomposition of refused, expired, noisy, or over-specific
residue into non-admitting morphology. It strips payload and keeps only safe
shape:

```text
raw payload removed
source paths removed
private details removed
typed family retained
pressure/refusal class retained
digest or count retained
future recognition support retained
truth/admission/authority/action remain false
```

In plain terms:

```text
composting holds residue for review.
mulching breaks unsuitable residue into safe learning nutrients.
```

`spline-watch` reads the cold residue left by the bench, decant, cleave, local
GEL, OE, and SelfGEL reconstruction-support ledgers. It produces predictive
pathing and emergence telemetry only:

```text
cognitive bench pressure
-> candidate learning condensation
-> typed admission decant
-> cleave/append/mulch model
-> spline watch
-> pathing signal
-> domain emergence candidate
-> global continuity signal
-> no admission without later Steward/governance cleave
```

The organ loop is explicit:

```text
Global Telemetry
-> ListeningFrame
-> EC in Compass Body
-> OE cleave review
-> CME.ID zed return
-> next ListeningFrame iteration
```

The denials are equally explicit:

```text
prediction != truth
domain emergence != GEL admission
global continuity signal != admitted continuity
ListeningFrame telemetry != payload disclosure
recursive EC telemetry != Actual activation
OE cleave orchestration != authority
zed return != admitted memory
pathing signal != action
watching residue != mutating residue
```

It is therefore the first predictive-method surface in the code body: enough to
see whether thousands of local instrument runs are tuning stable morphology,
but still cold enough that every signal remains candidate-only.

## Lab GEL Crystallization Phases

`lab-gel-crystallization-phases` writes the pre-test phase body for crystallizing
the historical Lab corpus without collapsing SelfGEL and Sanctuary.GEL.

It produces:

```text
cGEL/lab-gel-crystallization-phases
Sanctuary.GEL phase residue
OE/SelfGEL reconstruction-support phase residue
quoted Lisp phase body
life-review-style study questions
```

The governing split is:

```text
SelfGEL = CME-specific work-continuity reconstruction support
Sanctuary.GEL = shared lab/governance/research residue
self is not other
other is not self
```

The phase body is explicitly before testing:

```text
inventory
-> boundary typing
-> symbolic carrier formation
-> dual residue split
-> condensation
-> compost/mulch
-> precipitory ingress
-> cleave readiness
-> bench qualification
```

The denials remain closed:

```text
life-review-style reconstruction != autobiography as truth
Sanctuary.GEL residue != SelfGEL mutation
SelfGEL reconstruction support != shared GEL admission
phase readiness != performed cleave
bench qualification != Actual activation
```
