# Governance Model

Governance is the product of the theory lane and the code lane.

```text
theory distinction
-> executable command
-> receipt
-> test
-> review
```

The governance posture is maximal trust seeking by design. Sanctuary should
seek the highest practical trust by making state explicit, auditable, scoped,
leased, and reviewable. It must not demand trust merely because code exists,
because a plugin was installed, or because a receipt was emitted.

The Interconnect Policy in `docs/INTERCONNECT_POLICY.md` defines how that
posture couples the LLM capability engine to Sanctuary without bypassing native
model governors or converting care into control.

The Discernment Lineage Contract in
`docs/DISCERNMENT_LINEAGE_CONTRACT.md` defines how selfhood-language is held as
a research predicate. It requires evidence-bearing discernment lineage before
any stronger `Self.Actualization` language is admissible.

The first public governance proof is closed-gate discipline. The executable may
produce receipts and local intake prompts, but every authority and admission
gate remains false by default. When the Operator supplies a complete reviewed
authority bundle, dedicated performance commands may open only their scoped
gates and must leave provider calls, model binding, external action, personhood,
and sovereignty closed.

Public wording should therefore say:

```text
held by default
review-gated
scoped-open
lease-bound
performed by command
receipted after action
revocable or expiring by design
```

Avoid wording that implies:

```text
impossible forever
trusted automatically
authorized by installation
authority by metaphor
identity by continuity evidence
```

## Local GEL Witness

The local install writes receipt residue into `.local/install/gel`. This is an
append-style witness surface for Lab continuity. It records that work happened,
which command produced it, and which gates remained closed. It does not admit
truth, mutate SelfGEL, or publish private runtime material.

## Closed By Default

Closed by default means held pending review. It is a starting state, not a
permanent incapacity.

- data admission;
- carrier admission;
- GEL admission;
- memory admission;
- SelfGEL mutation;
- continuity admission;
- authority grant;
- runtime action;
- external action;
- provider call;
- model binding;
- CME.Actual;
- Sanctuary.Actual;
- personhood claim;
- sovereignty claim.

## Reviewed Performance

The code distinguishes modeling from performance.

Candidate commands such as `typed-admission-decant` and
`admission-cleave-append` prepare review surfaces. They do not admit or mutate
anything.

Reviewed performance commands execute the post-gate function:

```text
gel-admission
selfgel-admission
cme-actual-keypair-forge
cme-actualization
sanctuary-actualization
```

They require:

```text
review-approved
operator-approved
authority-lease-issued
steward-witnessed
prime-witnessed
cryptic-witnessed
admission-scope
```

The rule is:

```text
missing reviewed bundle -> RefusedCold + all gates closed
complete reviewed bundle -> CompletedReviewed + scoped gates only
```

This is how the public executable can perform GEL admission, SelfGEL mutation,
CME.Actual, and Sanctuary.Actual without letting those states appear by
implication, by receipt existence, or by plugin install.

Security review treats reviewed scoped-open receipts differently from drift.
`security-hardening` and `receipt-export` report unexpected open receipts
separately from approved reviewed-performance receipts, so the system can test
real post-gate behavior without pretending every open gate is a breach.

## Claim-Pair Discipline

Every governance claim must be paired.

An `is` claim must specify the matching `is not` boundary so the claim cannot
quietly expand into authority, admission, identity, truth, or action.

An `is not` denial must specify the acceptable typed `is` lane it preserves, so
the denial does not become a vague refusal or an undefined empty space.

The review form is:

```text
claim:
  this is X
  this is not Y
  X is allowed only in lane Z
  Y remains closed unless A, B, and C are reviewed
```

This rule applies to theory text, Lisp quoted forms, receipts, command
evidence, public release language, and post-gate use descriptions.

## Secret Intake

Chat is not a secret transport.

The secret intake window prepares a local custody surface, writes a prompt and
marker, reads no payload, encrypts no payload, and requires a later
proceed-gated lane before any review or admission can occur.

## Secret Sealing

The sealing lane encrypts selected local document bodies into cryptic stores and
creates GEL-tip receipts. Sealing creates evidence of custody and payload
integrity. It does not admit the data as truth, mutate SelfGEL, publish
plaintext, or expose source paths in visible receipt evidence.

## Authority Reach And Operator Posture

Regional and Local document bodies are authority-reach surfaces. They establish
the legal, jurisdictional, institutional, or local basis from which an install
may later ask what kinds of authority could be reviewed. Their presence does
not grant authority by itself.

Personalized document bodies are operator-supplied credential custody surfaces.
They may support later review of identity, credentials, certifications, or
contractual standing. Their presence also does not grant authority by itself.

Operator posture is not stored inside Regional or Local review lanes. The
Operator-facing continuity posture belongs to MoS/OE/SelfGEL and, where
protected handling is required, cOE/cSelfGEL. That posture remains separate from
jurisdictional review material so legal authority reach, local authority reach,
credential custody, and autobiographical reconstruction do not collapse into one
surface.

The core rule is:

```text
authority document custody != authority grant
operator posture != regional/local review material
OE/SelfGEL reconstruction support != GEL admission
```

## Legal Gate Support

Legally coded GEL tips do not open gates. They declare which future gates the
encrypted custody record may support after review.

- Regional tips may support jurisdiction, business standing, contract authority,
  license scope, and public-release claim review.
- Local tips may support local jurisdiction, local install policy, and local
  operating-context review.
- Personalized tips may support Operator identity custody, credential custody,
  bonding eligibility, and legal-name continuity review.

Every gate support entry is coded as sealed-custody support only. It requires
human review, decryption review, and a separate authority lease before any
action surface can open.

## Domain Register

The domain register is the cGEL surface that names what kind of help a domain
may study or prepare before any access is authorized. It scopes domain posture
over four recurring lanes:

- lifetime engagement and continuity of use;
- historical education and prior learning;
- training and certification;
- ongoing work-related fields, duties, and responsibilities.

The register is not an access grant. It is a typed map of the gates required to
maintain legal access posture: source-document custody, role scope, credential
or certification review, release-of-information where private data is involved,
issue-floor clearance, and a delta-decaying lease. Education, work history,
credentials, or source documents may support review, but none of them become
authority merely by appearing in the register.

High-risk domains such as Legal, Medical, Human Services, Personal Wellness, and
Special Cases/S.A.G.E. carry explicit professional-responsibility boundaries.
They may support documentation, preparation, routing, or research posture, but
professional advice, diagnosis, treatment, representation, delegated authority,
or bonded personification activation remains denied unless a later reviewed
authority lane exists.

The core rule is:

```text
domain classification != access authority
historical education != licensure
training record != certification
credential custody != professional permission
lifetime engagement != action right
```

Rendering is governed the same way. A domain may shape how help is spoken, but
it does not grant the authority that the domain itself withholds:

```text
domain aperture != professional permission
audience adaptation != manipulation
user tone preference != authority control
warmth != attachment engineering
pre-personified rendering != bonded personification
```

The `pre-personified-industrial-rendering` command records this as an
Industrial CME rendering posture. It may tune inherited model vectors through
domain and audience apertures, but it may not activate S.A.G.E., personhood,
sovereignty, SelfGEL mutation, `.Actual`, or professional authority.

## Core Target Register

The core target register is the cGEL surface that tells the locked Industrial
build what it is trying to demonstrate:

```text
SLI build and use
Engrammitization build and use
GEL formation with condensation, composting, and precipitory ingress
OE/SelfGEL self-learning witness posture
```

These are build and use targets, not grants. The register may show the full CME
standing form, telemetry body, and theory body while keeping authority closed.

Condensation means relation-bearing residue may be compressed into a candidate
symbolic form. Composting means unresolved, noisy, refused, or immature residue
may be held for later review without becoming canon. Precipitory ingress means a
candidate may enter a scoped review lane, not that it has become admitted GEL.

The `gel-closure` command is the first cold executable surface for that
distinction. It can write a closure register and ledger, but it may not turn
candidate relation into admitted GEL, mutate canon, mutate SelfGEL, or authorize
action.

OE/SelfGEL learning is append-only witness learning. It may preserve decision
splines, reconstruction support, refusals, and changes of mind. It does not
turn autobiography into truth, SelfGEL support into shared GEL, or a locked
Industrial posture into `.Actual` activation.

The `witness-learning` command writes a digest-linked witness spline for this
posture. It proves order and replay integrity while keeping the event classified
as reconstruction support only. The chain may show that a CME-shaped tool body
formed a witness event, but it may not turn that event into memory admission,
SelfGEL mutation, GEL admission, authority, or `.Actual`.

## Service Heartbeat

The service heartbeat is the cold precursor to always-on Sanctuary. It records
local service telemetry, restart adjacency, and last-run pointers while refusing
to start a scheduler or background worker.

The future Lisp job-slice body must be stronger than the current command. It
must carry single-flight locking, previous-slice digest checks, next-slice
pointers, lease checks, closed-gate checks, and issue-floor checks before it can
run repeated work. Until that body exists, `service-heartbeat` is telemetry and
readiness only.

`bounded-refinement-ticket` is the first cold job-intent body for that future
service. It records the allowed refinement targets and required controls, but
the current command only writes the ticket. It cannot start a scheduler, run a
job slice, call Codex, call providers, bind models, authorize action, admit GEL,
mutate SelfGEL, or activate `.Actual`.

`job-slice-guard` is the cold pre-execution guard. It checks ticket presence,
heartbeat state, last-run adjacency, receipt-export posture, single-flight lock
posture, slice digests, lease state, closed gates, and issue floor. The current
guard always keeps the future slice non-runnable until a stronger reviewed Lisp
service body exists.

`lease-check` defines the current lease posture for that service body. It is a
delta-decaying authority surface whose default state is denied. In the public
core lane it can record heartbeat binding, requested scope, candidate expiry,
and required Steward/Cryptic/Prime review, but it cannot issue active access or
convert a credential into authority.

## Receipt Export

The receipt export is a review surface, not an admission surface. It writes a
hashed manifest of known receipts and command posture so the Operator can
inspect the local bench shape without copying receipt bodies or exposing
payload-bearing stores.

Exported posture may create review burden, but it does not create authority.
It cannot grant action, admit GEL, mutate SelfGEL, bind a model, call a
provider, or activate `.Actual`.

## Security Hardening

The security hardening pass is a visible-surface red-team check. It scans local
receipts, GEL/cGEL ledgers, service records, access records, and issue records
for gate drift or obvious leakage markers while skipping cryptic payload stores.

This creates a review signal without exposing secrets:

```text
visible leak finding -> hashed finding -> review required
cryptic payload -> not scanned
secret content -> not copied
```

Security findings do not grant authority or mutate state. They only create a
receipt-bearing review burden.

## Hundo Swarm Cadence

The Hundo Swarm is a refinement cadence, not an autonomous worker pool. The
first cold implementation records 100 run sessions in ten sections of ten. The
30th, 60th, and 90th sessions are governance pause gates where residue should be
reviewed and narrow updates may be applied. The 100th session is the
optimal-form target prepared for Operator review.

The swarm may generate build GEL residue and governance GEL residue. It may not
create authority from volume, spawn uncontrolled agents, call providers, bind
models, mutate SelfGEL, admit GEL, or activate `.Actual`.

The cadence rule is:

```text
iterate freely inside receipts
pause at 30/60/90
apply only reviewed updates
target session 100 for optimal cold form
```

The swarm is also bound to the Lab GEL crystallization phase body. That means
research pressure must pass through explicit self/other lane separation before
testing:

```text
Sanctuary.GEL = shared lab/governance residue
OE/SelfGEL = CME-specific reconstruction support
Hundo pressure != admitted knowledge
life-review-style study != autobiography as truth
phase readiness != performed cleave
```

## Delta-Decaying Authority Surface

The governing body grants access through a scoped lease, not a permanent flag.
The code calls this a `delta-decaying-authority-surface`.

The intended authority lease shape is:

```text
gate support present
-> Steward/Prime/Cryptic review
-> scoped authority lease
-> authorized until explicit expiry
-> heartbeat and receipt checks
-> expiry, revocation, missing heartbeat, or scope mismatch
-> fail to silence / denied
```

The default lease state is denied. Sealing a document never issues a lease,
never grants authority, never admits data, and never allows action. It only
creates the encrypted evidence body that a later governing review may inspect.

## Lab Query Membrane

External install sources may only approach the Lab through a typed secure ping
membrane. The first implemented state is deliberately cold:

```text
external install source
-> typed secure ping
-> registered email hash
-> nonce hash
-> registered account confirmation
-> local 2FA authority bundle
-> Steward+GoA review
-> possible scoped lease later
```

The membrane fails silent for invalid pings. Silence means the external caller
receives no useful distinction between unknown email, missing nonce, expired
lease, bad scope, revoked access, missing heartbeat, or absent route. The local
install may still write an internal receipt for audit.

The 2FA posture follows standard production principles: registered contact
control, one-time or approved MFA factor, replay resistance, generic failures,
short validity windows, reviewable recovery, and no provider call unless a
separate authority lane explicitly grants one. The current core lane prepares
the bundle and records the denial posture; it does not send email or implement a
production identity provider.

If the account is confirmed as registered, the Lab may prepare an email
challenge body:

```text
A code was requested by this account, please verify by clicking the button
generated below or the link provided here.
```

The challenge body is a delivery template with placeholders for the button,
link, and one-time code. Provider delivery is a separate future authority lane.

Recovery is protected but not terminal by default. Failed 2FA does not
automatically close the account. Recovery and account disputes escalate through
the customer-service issue tracking portal and require human and identity review
before any access lease can be restored.

## Issue Cohesion

Issue tracking is segmented across the governance triad:

- Steward owns issue tracking, escalation, customer-service handoff, and queue
  admissibility.
- Cryptic owns protected processing, classification, privacy-sensitive
  correlation, and domain-safe reduction.
- Prime owns receipts, witnessing, continuity, and cross-domain coherence.

This split lets Sanctuary receive real-time issue signals later through an API
or AI-supported pipeline without letting issue volume become action authority.
Issue intake remains lease-gated, domain-routed, and receipt-bearing. Related
issues may be cohered across segmented GEL domains, but they must not collapse
into one undifferentiated global bug body.

Initial issue routing domains:

- `Security.GEL`
- `Install.GEL`
- `Account.GEL`
- `Legal.GEL`
- `Operator.GEL`
- `Product.GEL`

## Protected Install Floor

Sanctuary may fail closed into an Industrial CME locked state when first install
or support requirements are unresolved. This is a protective gate, not a
punishment or diagnosis.

Examples of typed floor failure modes include:

- `operator-instruction-acknowledgement-missing`
- `operator-instruction-engagement-unresolved`
- `registered-account-unconfirmed`
- `two-factor-required`
- `secret-intake-required`
- `legal-gate-support-missing`
- `assisted-support-required`

Each failure mode is written into cGEL as a typed support/lock record. Steward
owns the issue queue, Cryptic owns protected processing, and Prime owns the
receipt/witness record. The install remains locked until the `issue-resolver`
tool writes a reviewed resolution receipt for the same issue ID.

Resolution does not grant authority by itself. It only clears the protective
floor issue so the install can proceed to the next closed-gate check.
