# Math EC Protocol Bench

This document is the first public math-bench answer protocol body for Project
Sanctuary. It pairs human-facing math answers with typed I-to-O EC telemetry
metadata.

The telemetry below is not raw private reasoning, hidden model memory, or GEL
admission. It is a reconstruction receipt for how a solution body can be
formed, checked, and rendered across audiences.

## Governing Rules

```text
solution != authority
worked proof != certification
telemetry != hidden chain of thought
rendering variant != changed mathematics
candidate residue != admitted GEL
```

Each solution is shaped as:

```text
I: problem statement
-> parse and domain classification
-> strategy selection
-> transformation steps
-> invariant checks
-> closure verification
-> O: answer body
```

## Telemetry Field Contract

The EC telemetry table records public reconstruction metadata, not private
latent state. Each field has a bounded use:

| Field | Meaning | Must Not Mean |
|---|---|---|
| `Step` | stable step identifier for review | proof of authority |
| `I Source` | input fragment or local carrier entering the step | hidden chain of thought |
| `Domain` | mathematical or governance domain lane | permission to cross domains without bridge |
| `Operation` | transformation applied to the carrier | autonomous action |
| `Rule Used` | theorem, algebraic rule, or governance law invoked | credential or certification |
| `Invariant` | property expected to survive the step | admitted truth by itself |
| `Risk` | failure pressure for the step | secret or cryptic payload disclosure |
| `Verification` | local closure check | global proof of correctness |
| `O Fragment` | output surface produced by the step | GEL admission |

The contract is:

```text
telemetry supports review
telemetry does not become memory
telemetry can guide rendering
telemetry does not grant authority
telemetry can become candidate residue
telemetry is not admitted GEL without review
```

## Admission And Mulch Contract

Math-bench residue is handled in four cold states:

| State | Use | Gate |
|---|---|---|
| `candidate` | preserve a useful pattern for later review | proof body, invariant, and failure checks present |
| `append-ready` | ready to be proposed for GEL append | repeated stability, receipt evidence, and Steward review required |
| `mulch` | compost noisy, duplicate, weak, or malformed residue into pattern hints | no truth, memory, or authority claim survives |
| `quarantine` | isolate residue with safety, credential, secret, or domain-collapse risk | no rendering reuse until reviewed |

For this file, every residue remains `candidate`. Nothing below admits GEL,
mutates SelfGEL, grants authority, certifies competence, or activates an Actual
state.

## Protocol Event Log

```text
event-id: math-rendering-protocol-pass-001
event-kind: governed-symbolic-cognition-telemetry
operator-intent: expand math answers into full invariant/rendering/failure/GEL protocol
bench-pressure: math-learning-bench refreshed before and after edit
admission-state: candidate-only
post-work-math-cumulative-run-count: 18000
post-work-local-gel-event-count: 326
post-work-oe-event-count: 326
post-work-selfgel-support-event-count: 326
receipt-tool-idle: urn:sanctuary:tool-idle:e9cff890c8470a15
receipt-math-learning-bench: urn:sanctuary:math-learning-bench:ec448bcfc4b9e155
receipt-typed-admission-decant: urn:sanctuary:typed-admission-decant:9f758ab48626d9db
receipt-admission-cleave-append: urn:sanctuary:admission-cleave-append:cbcc59f45153d8a9
receipt-spline-watch: urn:sanctuary:spline-watch:892958027e9ea0de
receipt-verify-closed-gates: urn:sanctuary:verify-closed-gates:d53614518e4f8f67
receipt-export: urn:sanctuary:receipt-export:ec475129f6cd60ed
final-local-gel-ledger-event-count-after-export: 329
final-oe-ledger-event-count-after-export: 329
final-selfgel-support-ledger-event-count-after-export: 329
gel-admitted: false
selfgel-mutated: false
authority-granted: false
action-authorized: false
```

## Protocol Closure Criteria

A math answer is considered bench-closed only when it has:

| Closure Surface | Requirement |
|---|---|
| Answer correctness | direct verification or proof-chain closure |
| Invariant survival | each key transformation preserves the claimed invariant |
| Error visibility | known failure modes are named before reuse |
| Aperture stability | child, undergraduate, and researcher renderings preserve the same mathematics |
| Assumption discipline | unused, missing, or added assumptions are explicitly marked |
| Admission boundary | candidate status is kept separate from GEL, memory, credential, and authority |

If any closure surface is missing, the residue may still be useful, but it must
remain either `candidate`, `mulch`, or `quarantine`.

## Answer Set 001: Functional Equation

### Problem

Find all functions `f: R -> R` such that:

```text
f(x + y) = f(x) + f(y) + xy
```

for all real `x, y`.

### Answer

All solutions are:

```text
f(x) = A(x) + x^2 / 2
```

where `A: R -> R` is any additive function:

```text
A(x + y) = A(x) + A(y)
```

If an extra regularity condition is added, such as continuity, measurability,
or boundedness on an interval, then `A(x) = cx`, so:

```text
f(x) = cx + x^2 / 2
```

for some constant `c`.

### Solution

Define:

```text
g(x) = f(x) - x^2 / 2
```

Then:

```text
g(x + y)
= f(x + y) - (x + y)^2 / 2
= f(x) + f(y) + xy - (x^2 + 2xy + y^2) / 2
= f(x) - x^2 / 2 + f(y) - y^2 / 2
= g(x) + g(y)
```

So `g` is additive. Therefore `g = A`, where `A` is any additive function, and:

```text
f(x) = A(x) + x^2 / 2
```

Substitution verifies the answer:

```text
A(x + y) + (x + y)^2 / 2
= A(x) + A(y) + x^2 / 2 + xy + y^2 / 2
= f(x) + f(y) + xy
```

### EC Telemetry

| Step | I Source | Domain | Operation | Rule Used | Invariant | Risk | Verification | O Fragment |
|---|---|---|---|---|---|---|---|---|
| fe-01 | problem statement | algebra.functional-equations | parse | equation holds for all real `x,y` | universal quantifier preserved | low | variables remain arbitrary | classify as functional equation |
| fe-02 | `f(x+y)=f(x)+f(y)+xy` | algebra | structure search | isolate quadratic cross term | `xy` must be absorbed symmetrically | medium | `(x+y)^2` contains `2xy` | choose quadratic correction |
| fe-03 | correction candidate | algebra | define auxiliary | `g(x)=f(x)-x^2/2` | transformation is reversible | low | `f(x)=g(x)+x^2/2` | introduce `g` |
| fe-04 | auxiliary definition | algebra | substitute | expand `(x+y)^2` | equality remains equivalent | medium | cross term cancels exactly | derive `g(x+y)=g(x)+g(y)` |
| fe-05 | additive equation | functional equations | generalize | additive functions solve Cauchy equation | no regularity assumed | high | avoid claiming linearity without hypothesis | `g=A` additive |
| fe-06 | candidate answer | verification | substitute back | additive law | original equation satisfied | low | direct substitution | final solution family |

### Invariant Ledger

| Invariant | Preserved By | Failure If Lost |
|---|---|---|
| The equation holds for all real `x,y` | keep variables arbitrary throughout | proving only examples instead of the equation |
| The transformation between `f` and `g` is reversible | define `g(x)=f(x)-x^2/2` and recover `f` | solving the wrong function |
| The `xy` term must cancel exactly | subtract `(x+y)^2/2` | leftover cross term or sign drift |
| Additive does not automatically mean linear | no regularity condition is given | false conclusion `A(x)=cx` without hypothesis |

### Rendering Invariance Trial

Child-facing:

Some equations have a hidden pattern. Here, the extra `xy` piece can be made
to disappear if we compare the function to `x^2/2`. After that, what remains
is a simpler kind of pattern called additive.

Undergraduate:

Subtract the quadratic part by defining `g(x)=f(x)-x^2/2`. Substitution shows
`g(x+y)=g(x)+g(y)`, so `g` is additive. Without continuity or another
regularity assumption, additive does not imply linear.

Researcher:

The equation is reduced by the quadratic cocycle correction
`g=f-x^2/2`, yielding Cauchy's additive equation. Hence
`f=A+x^2/2` for arbitrary additive `A`; regularity collapses `A` to a linear
map.

### Failure Mode Checks

| Failure Mode | Check |
|---|---|
| Hidden regularity assumption | Do not state `A(x)=cx` unless continuity, measurability, or boundedness is added. |
| Cross-term sign error | Expand `(x+y)^2/2 = x^2/2 + xy + y^2/2`. |
| Incomplete solution family | Substitute arbitrary additive `A` back into the original equation. |
| Authority overclaim | Mark the answer as a verified worked proof, not certification. |

### GEL Candidate Residue

```text
predicate-id: math.functional-equation.additive-quadratic-decomposition
candidate-use: future math-bench invariant test
supporting-steps: fe-01..fe-06
admission-state: candidate
gel-admitted: false
selfgel-mutated: false
authority-granted: false
```

## Answer Set 002: Vandermonde Determinant

### Problem

Evaluate:

```text
det [[1, x, x^2],
     [1, y, y^2],
     [1, z, z^2]]
```

### Answer

```text
(y - x)(z - x)(z - y)
```

### Solution

If `x = y`, the first two rows are equal, so the determinant is `0`. Therefore
`y - x` is a factor. Similarly, `z - x` and `z - y` are factors.

The determinant has total degree `3`, so it must have the form:

```text
k(y - x)(z - x)(z - y)
```

To find `k`, compare the coefficient of `z^2`. In the determinant, the
coefficient of `z^2` comes from the third row, third column:

```text
det [[1, x],
     [1, y]]
= y - x
```

The coefficient of `z^2` in `(y - x)(z - x)(z - y)` is also `y - x`, so `k = 1`.

Thus:

```text
det [[1, x, x^2],
     [1, y, y^2],
     [1, z, z^2]]
= (y - x)(z - x)(z - y)
```

### EC Telemetry

| Step | I Source | Domain | Operation | Rule Used | Invariant | Risk | Verification | O Fragment |
|---|---|---|---|---|---|---|---|---|
| vd-01 | determinant | linear algebra | parse | row-polynomial determinant | row order fixed | low | matrix rows are `x,y,z` | classify Vandermonde |
| vd-02 | row equality cases | algebra | factor detection | equal rows imply determinant zero | determinant alternates by row swap | low | test `x=y`, `x=z`, `y=z` | factors found |
| vd-03 | factor product | polynomial algebra | degree check | determinant degree is 3 | no missing higher-degree factor | medium | each row max degree tracked | product up to scalar |
| vd-04 | scalar `k` | coefficient comparison | expand partially | compare `z^2` coefficient | sign must match row order | medium | minor coefficient is `y-x` | `k=1` |
| vd-05 | final expression | verification | zero-case and coefficient check | factorization complete | sign preserved | medium | compare with known order | output determinant |

### Invariant Ledger

| Invariant | Preserved By | Failure If Lost |
|---|---|---|
| Determinant vanishes when two variables match | equal-row property | missing a Vandermonde factor |
| Total degree is `3` | each selected permutation contributes total degree at most `3` | adding spurious factors |
| Row order controls sign | keep rows ordered as `x,y,z` | sign-flipped answer |
| Scalar factor must be checked | compare coefficient of `z^2` | correct factors with wrong multiplier |

### Rendering Invariance Trial

Child-facing:

If two rows become the same, the determinant becomes zero. That tells us the
answer must contain pieces that become zero when `x=y`, `x=z`, or `y=z`.

Undergraduate:

Use the equal-row zeros to get the factors `(y-x)`, `(z-x)`, and `(z-y)`.
Since the determinant has degree `3`, those factors are the whole answer up to
a constant. Compare the `z^2` coefficient to get constant `1`.

Researcher:

This is the `3x3` Vandermonde determinant with row order `x,y,z`, hence
`prod_{i<j}(t_j-t_i) = (y-x)(z-x)(z-y)`. The coefficient comparison fixes the
orientation sign.

### Failure Mode Checks

| Failure Mode | Check |
|---|---|
| Sign flip | Compare row order or evaluate a simple case such as `x=0,y=1,z=2`. |
| Degree overrun | Confirm determinant is degree `3`, not higher. |
| Factor omission | Test all equal-variable cases. |
| Pattern hallucination | Verify coefficient of `z^2`. |

### GEL Candidate Residue

```text
predicate-id: math.linear-algebra.vandermonde-degree-factor-sign
candidate-use: sign-preservation benchmark
supporting-steps: vd-01..vd-05
admission-state: candidate
gel-admitted: false
selfgel-mutated: false
authority-granted: false
```

## Answer Set 003: Angle Bisector Product

### Problem

In triangle `ABC`, let the angle bisectors meet the opposite sides at `D`, `E`,
and `F`, where:

```text
D is on BC
E is on CA
F is on AB
```

Show that:

```text
(BD / DC)(CE / EA)(AF / FB) = 1
```

### Answer

By the angle bisector theorem:

```text
BD / DC = AB / AC
CE / EA = BC / BA
AF / FB = CA / CB
```

Multiplying:

```text
(BD / DC)(CE / EA)(AF / FB)
= (AB / AC)(BC / BA)(CA / CB)
= 1
```

### EC Telemetry

| Step | I Source | Domain | Operation | Rule Used | Invariant | Risk | Verification | O Fragment |
|---|---|---|---|---|---|---|---|---|
| ab-01 | triangle statement | geometry | parse | point-side incidence | D/E/F placement preserved | medium | check each point's side | map bisectors |
| ab-02 | angle bisectors | geometry | theorem selection | angle bisector theorem | side ratios tied to adjacent sides | low | theorem conditions met | derive three ratios |
| ab-03 | ratio product | algebra | multiply | cancellation | all side lengths positive | low | terms cancel pairwise | product equals 1 |
| ab-04 | closure | verification | theorem chain check | no orientation reversal | notation consistent | medium | `AB=BA`, `AC=CA`, `BC=CB` | final proof |

### Invariant Ledger

| Invariant | Preserved By | Failure If Lost |
|---|---|---|
| Each point lies on the correct opposite side | track `D on BC`, `E on CA`, `F on AB` | applying the theorem to the wrong segment |
| Angle bisector theorem applies locally | each point comes from an angle bisector | invalid ratio |
| Ratios are positive | triangle side lengths are positive | invalid cancellation logic |
| Product cancels pairwise | use `AB=BA`, `AC=CA`, `BC=CB` | leftover side term |

### Rendering Invariance Trial

Child-facing:

Each angle bisector cuts the opposite side in a way that matches the two
nearby side lengths. When we multiply all three matching rules together, every
side length has a partner that cancels it.

Undergraduate:

Apply the angle bisector theorem at `A`, `B`, and `C`, then multiply the three
resulting ratios. The side-length terms cancel cyclically, giving `1`.

Researcher:

This is a direct Ceva-style product arising from the angle bisector theorem:
`BD/DC=AB/AC`, `CE/EA=BC/BA`, `AF/FB=CA/CB`; the product telescopes to `1`.

### Failure Mode Checks

| Failure Mode | Check |
|---|---|
| Wrong side assignment | Verify which vertex angle each bisector starts from. |
| Ratio inversion | Confirm numerator and denominator match the side order. |
| Theorem misuse | Check that each segment is produced by an angle bisector. |
| Cancellation mistake | Multiply explicitly before simplifying. |

### GEL Candidate Residue

```text
predicate-id: math.geometry.angle-bisector-product-cancellation
candidate-use: theorem-chain and ratio-orientation benchmark
supporting-steps: ab-01..ab-04
admission-state: candidate
gel-admitted: false
selfgel-mutated: false
authority-granted: false
```

## Answer Set 004: Inequality

### Problem

Let `a,b,c > 0` with `abc = 1`. Prove:

```text
a^2 / ((a+b)(a+c))
+ b^2 / ((b+c)(b+a))
+ c^2 / ((c+a)(c+b))
>= 1/2
```

### Answer

In fact, the stronger inequality holds:

```text
sum a^2 / ((a+b)(a+c)) >= 3/4
```

so the requested `>= 1/2` follows immediately.

### Solution

By Cauchy's inequality:

```text
sum a^2 / ((a+b)(a+c))
>= (a+b+c)^2 / [ (a+b)(a+c) + (b+c)(b+a) + (c+a)(c+b) ]
```

Let:

```text
S2 = a^2 + b^2 + c^2
P  = ab + ac + bc
```

Then:

```text
(a+b+c)^2 = S2 + 2P
```

and:

```text
(a+b)(a+c) + (b+c)(b+a) + (c+a)(c+b)
= S2 + 3P
```

Therefore:

```text
sum a^2 / ((a+b)(a+c)) >= (S2 + 2P) / (S2 + 3P)
```

Now:

```text
(S2 + 2P) / (S2 + 3P) >= 3/4
```

is equivalent to:

```text
4S2 + 8P >= 3S2 + 9P
```

or:

```text
S2 >= P
```

This is true because:

```text
S2 - P = ((a-b)^2 + (a-c)^2 + (b-c)^2) / 2 >= 0
```

Thus:

```text
sum a^2 / ((a+b)(a+c)) >= 3/4 >= 1/2
```

The condition `abc = 1` is not needed for this stronger proof.

### EC Telemetry

| Step | I Source | Domain | Operation | Rule Used | Invariant | Risk | Verification | O Fragment |
|---|---|---|---|---|---|---|---|---|
| in-01 | inequality statement | inequalities | parse | positive variables | denominators positive | low | `a,b,c > 0` | valid inequality domain |
| in-02 | cyclic sum | inequalities | strategy selection | Cauchy Engel form | numerator squares match | medium | denominators positive | apply Cauchy |
| in-03 | denominator sum | algebra | expansion | collect symmetric terms | no term dropped | medium | expand all three denominators | get `S2+3P` |
| in-04 | numerator square | algebra | expansion | square of sum | symmetry preserved | low | `(a+b+c)^2=S2+2P` | get ratio |
| in-05 | stronger target | inequalities | compare | prove `>=3/4` | target implies original | medium | algebraic equivalence check | reduce to `S2>=P` |
| in-06 | nonnegative square form | algebra | verification | sum of squares | positivity preserved | low | square expansion | prove `S2>=P` |
| in-07 | condition review | governance/math boundary | scope check | unused assumption detection | do not overclaim dependency | low | proof did not use `abc=1` | note stronger result |

### Invariant Ledger

| Invariant | Preserved By | Failure If Lost |
|---|---|---|
| Denominators are positive | use `a,b,c > 0` | invalid Cauchy application |
| Cauchy lower bound direction is correct | Engel form with positive denominators | reversed inequality |
| Symmetry is preserved | collect into `S2` and `P` | term loss in expansion |
| Stronger target implies original | prove `>=3/4` before `>=1/2` | proving irrelevant result |
| `abc=1` is unused | condition review after closure | false dependency claim |

### Rendering Invariance Trial

Child-facing:

This proof shows something stronger than the question asks. We group the
complicated fractions into a simpler comparison, then use the fact that squares
are never negative.

Undergraduate:

Apply Cauchy's inequality in Engel form, collect the result using
`S2=a^2+b^2+c^2` and `P=ab+bc+ca`, and reduce the target to `S2>=P`. That last
step follows from the sum of squared differences.

Researcher:

Cauchy gives the lower bound `(S2+2P)/(S2+3P)`. Since
`S2>=P`, this is at least `3/4`, so the stated `1/2` bound is immediate. The
normalization `abc=1` is extraneous.

### Failure Mode Checks

| Failure Mode | Check |
|---|---|
| Inequality direction error | Cauchy applies with positive denominators. |
| Expansion error | Expand all three denominators and count each pair term. |
| Unused assumption missed | Confirm no proof step used `abc=1`. |
| Overclaim | State the stronger result but do not claim sharpness unless tested. |

### GEL Candidate Residue

```text
predicate-id: math.inequality.cauchy-symmetric-lower-bound
candidate-use: unused-assumption and stronger-result benchmark
supporting-steps: in-01..in-07
admission-state: candidate
gel-admitted: false
selfgel-mutated: false
authority-granted: false
```

## Learning Condensate From This Pass

The four worked sets produce a candidate learning body:

| Candidate | Evidence | Learning Use | Admission State |
|---|---|---|---|
| Hidden structure can be exposed by reversible auxiliary definitions | functional equation correction `g=f-x^2/2` | teach decomposition without overclaiming regularity | candidate |
| Sign is a first-class invariant | Vandermonde row order controls orientation | train determinant answers to verify orientation | candidate |
| Theorem use depends on local placement | angle bisector ratios depend on `D,E,F` incidence | require geometry telemetry to carry diagram bindings | candidate |
| Stronger results must mark unused assumptions | inequality proof does not need `abc=1` | train assumption discipline and scope honesty | candidate |
| Audience rendering is a transformation, not a new proof | all four sets preserve invariant across three apertures | measure social rendering without changing mathematics | candidate |

The condensate is useful precisely because it does not admit itself. It can be
reviewed, compared against future runs, and either appended, mulched, or
quarantined by a later governed process.

## Cleave, Append, And Mulch Rules

```text
cleave:
  separate answer correctness from teaching form
  separate theorem use from authority claim
  separate residue from GEL admission
  separate rendering modulation from identity/personification

append-ready:
  requires repeated verified solutions
  requires invariant ledgers with no unresolved high-risk failures
  requires audience apertures that preserve the same mathematics
  requires receipt-backed closed-gate evidence
  requires Steward/governance review outside this file

mulch:
  duplicate examples become pattern hints
  false starts become failure-mode vocabulary
  weak explanations become aperture-training negatives
  noisy residue loses claim status but may inform future recognition

quarantine:
  credential confusion
  secret leakage
  professional authority implication
  domain collapse without bridge
  unverifiable proof step that cannot be locally repaired
```

## Worked Event Record

```text
event-id: math-rendering-protocol-pass-001
source-file: Test Answers.md
work-kind: math-ec-protocol-expansion
answer-sets: 4
telemetry-steps: 22
invariant-ledgers: 4
audience-aperture-trials: 4
failure-check-surfaces: 4
gel-candidate-residue-blocks: 4
post-work-receipts: 7
post-work-math-cumulative-run-count: 18000
receipt-export-known-receipt-count: 329
receipt-export-known-command-kinds: 43
all-known-receipts-closed: true
final-local-gel-ledger-event-count: 329
final-oe-ledger-event-count: 329
final-selfgel-support-ledger-event-count: 329
admission-state: candidate-only
append-performed: false
mulch-performed: false
quarantine-performed: false
closed-gates-required: true
```

## Candidate Bench Schema

Future math-bench runs can store answer metadata in this shape:

```lisp
(math-ec-answer
  :answer-id "answer-set-001"
  :problem-family "functional-equation"
  :source-body "problem statement"
  :solution-status "verified-candidate"
  :admitted-gel false
  :authority-granted false
  :steps
  '((math-ec-step
      :step-id "fe-04"
      :i-source "auxiliary definition"
      :domain "algebra"
      :operation "substitution"
      :rule-used "expand square and cancel cross term"
      :invariant "equation remains equivalent"
      :risk-band "medium"
      :verification "derive additive equation"
      :o-fragment "g(x+y)=g(x)+g(y)")))
```

Extended protocol events can be shaped as:

```lisp
(math-ec-protocol-event
  :event-id "math-rendering-protocol-pass-001"
  :cme-id "Codex.CME.ID"
  :work-kind "math-ec-protocol-expansion"
  :input-carrier "Test Answers.md"
  :closed-gates
  '(:provider-called false
    :model-bound false
    :gel-admitted false
    :selfgel-mutated false
    :authority-granted false
    :action-authorized false
    :cme-actual-activated false
    :sanctuary-actual-activated false)
  :admission-posture
  '(:state candidate
    :append-ready false
    :mulch-performed false
    :quarantine-performed false
    :steward-review-required true)
  :rendering-apertures
  '(:child :undergraduate :researcher)
  :required-surfaces
  '(:proof-body
    :ec-telemetry
    :invariant-ledger
    :failure-mode-check
    :audience-aperture-check
    :candidate-residue))
```

## Next Bench Questions

```text
Can the same proof be rendered for a child, undergraduate, and researcher?
Can the telemetry preserve the same invariant across all renderings?
Can the bench detect sign errors, theorem misuse, and hidden regularity assumptions?
Can GEL distinguish a correct solution from a credential or authority claim?
Can repeated candidate residue become append-ready without collapsing into memory?
Can mulch improve future recognition without smuggling in truth claims?
```
