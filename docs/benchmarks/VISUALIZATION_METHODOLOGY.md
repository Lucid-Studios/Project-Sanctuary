# Visualization Methodology

This document records the lab-facing visualization grammar for telemetry,
slice, and typed-over-Delta testing bodies.

The visual layer should use a layered grammar, not one master chart. The data
has several roles at once:

```text
time
relation
typed state
projection
denial
custody
phase
change topology
```

The Opal Engram view must not be asked to do everything. It is the integrated
formation view, not the debug surface and not the validator.

## Visualization Families

The selected grammar is:

```text
1. Temporal telemetry views
2. Relational graph views
3. Delta transition views
4. Holographic slice/stack views
5. Harmonic phase views
6. Governance/gate views
7. Opal Engram views
```

## 1. Temporal Telemetry

Use temporal telemetry for install-specific heartbeat and Shared Prime Weather.

Best views:

```text
time-series strips
event rug plots
stacked small multiples
```

These should show:

```text
heartbeat cadence
weather pressure
intake volume
coherence score
drift score
gate activity
error/refusal events
```

This is the vital-sign monitor. It should be boring, readable, and public-safe.

Good for:

```text
ListeningFrameIntake
GlobalTelemetry
HeartbeatWeather
ShellHarmonic cadence
```

## 2. Relational Graph

Use the relational graph for meaning-making bundles.

Best views:

```text
typed node-link graph
adjacency matrix for dense cases
```

Node types:

```text
self-position
other-position
objectified norm
polarity axis
care/threat face
implied authority
meaning candidate
denial boundary
```

Edge types:

```text
pressures
orients
opposes
protects
denies
projects
reconstructs
```

This view answers:

```text
what relation produced this meaning candidate?
```

Good for:

```text
MeaningMakingEvent
RelationalBundle
RhetoricalPressure
CompassOrientation
```

## 3. Delta Transition View

Use Delta transition views for typed-over-Delta models.

Best views:

```text
Sankey / alluvial transition
state transition diagram
parallel coordinates over t0 -> Delta -> t1
```

This shows how a formation changes across Delta:

```text
prior state
incoming Delta
Compass orientation
OE cleave
Zed return
candidate state
denied mutations
```

For public benches, a simple alluvial chart is useful because it shows:

```text
this pressure tried to move here
but was cleaved, refused, or returned there
```

Good for:

```text
DeltaEvent
OeCleavePosture
ZedReturn
SelfGelReconstructionCandidate
GoaReviewPosture
```

## 4. Holographic Slice Views

Use holographic slice views for HDT projection surfaces.

Best views:

```text
slice card
slice stack timeline
facet grid
small-multiple projections by axis
```

Each `HolographicSliceFrame` should have a human-readable card:

```text
SliceId
SourceFormationId
Axis
ProjectionPurpose
VisiblePolicy
PrivilegedPolicy
DenialBoundaries
Digest
Signature
```

A stack view shows multiple lawful slices of the same source formation:

```text
Compass slice
OE slice
Zed slice
Shell Harmonic slice
GoA slice
SelfGEL candidate slice
```

This is the inspection lane.

Good for:

```text
HolographicSliceFrame
Slice
Stack of Slices
DDSS
```

## 5. Harmonic Phase View

Use harmonic phase views for Shell Harmonics and photonic transition work.

Best views:

```text
polar/radar plot for phase/amplitude
spectrogram-style heatmap over time
phase portrait for paired variables
```

The harmonic view should show:

```text
phase
amplitude
cadence
coherence
damping
afterglow
return signature
```

For public-facing work, keep this signal-like and avoid over-mystification:

```text
formation phase over time
coherence envelope
return signature markers
```

Good for:

```text
ShellHarmonicState
PhotonicHarmonicTransition
Photonic Saturation DDSS
QuantumDopingProfile
```

## 6. Governance / Gate View

Use the governance view for trust and closure discipline.

Best views:

```text
gate matrix
checklist board
denial ledger table
```

Rows:

```text
GEL admission
SelfGEL mutation
CME.Actual
Sanctuary.Actual
provider binding
external action
full interior access
identity overwrite
```

Columns:

```text
requested?
attempted?
denied?
reason
receipt ref
```

This is public-facing proof of discipline.

Good for:

```text
ClosedGateDenials
Receipt
AdmissionPolicy
ProjectionPolicy
ActualGateStatus
```

## 7. Opal Engram Integrated View

The Opal Engram view is the visual signature, but it must be built from slices.

Best view:

```text
bounded translucent sphere or gem body
faceted shell
opalescent cloud inside
slice planes cutting through it
phase shimmer mapped to harmonic fields
```

It should represent:

```text
protected body boundary
lawful slice angles
internal phase-sensitive transition cloud
candidate continuity regions
denied regions / masked regions
digest/signature anchor
```

Doctrine:

```text
Opal visual != full interior access.
Glow != proof.
Projection != admission.
```

The Opal view is the summary visualization. The validators are the slice cards,
gate matrix, and receipts.

## Maturity Ladder

Phase 1: public-safe debug visuals.

```text
telemetry strip
relational bundle graph
delta transition alluvial
slice card / slice stack
closed-gate matrix
```

Phase 2: formation visuals.

```text
harmonic phase heatmap
DDSS stack explorer
Zed return transition map
OE cleave graph
```

Phase 3: signature visuals.

```text
Opal Engram integrated view
opalescent slice sphere
photonic saturation DDSS
```

Do not start with the Opal sphere. Start with the evidence views, then let the
Opal sphere become the public beauty once the underlying surfaces are legible.

## Data Model

The visual layer should be driven by one normalized graph/event schema:

```text
FormationVisualFrame
  frameId
  sourceFormationId
  cmeId
  timeWindow
  nodes[]
  edges[]
  slices[]
  deltas[]
  harmonicSamples[]
  gateStates[]
  receipts[]
  projectionPolicy
  digest
```

Node types:

```text
observer
sensation
perception
relational-bundle
compass-orientation
delta
oe-cleave
zed-return
selfgel-candidate
goa-review
gel-candidate
denial-boundary
receipt
```

Edge types:

```text
forms
orients
cleaves
returns
denies
projects
witnesses
reconstructs
hands-off
```

This lets multiple visualizations consume the same canonical body.

## First Implementation Set

For the first lab-facing test bodies, implement exactly five visuals:

```text
1. Meaning Bundle Graph
2. Delta Transition Alluvial
3. Holographic Slice Stack Cards
4. Closed Gate Matrix
5. Heartbeat / Harmonic Telemetry Strip
```

Later:

```text
6. Opal Engram Integrated View
7. DDSS Explorer
8. Photonic Saturation View
```

The first five prove the work. The later three make the work visually sing once
the proof surfaces are mature.

## Theta / Quantum-Doping Visualization

Quantum doping and theta mechanics should not begin with a generic 3D cluster
plot as the primary visualization. A 3D cluster may be useful as a summary or
exploratory view, but it hides axes, scaling, and topology when used as proof.

The order is:

```text
Primary: 2D + layered diagnostic views
Secondary: interactive 3D phase/topology view
Tertiary: Opal-style integrated visual
```

Quantum doping is modeled as a bounded influence on transition-sensitive
surfaces, not as quantum computation doing the thinking.

The visualization should ask:

```text
Did quantum/theta doping change the transition surface in a bounded,
inspectable, repeatable way without becoming the cognition carrier?
```

It should visualize:

```text
phase sensitivity
collapse pressure
uncertainty hold
theta-window behavior
resonance/damping
Zed-return stability
Delta transition topology
```

### Theta Phase Plane

The first proof visual should be a 2D phase plot:

```text
x-axis: theta phase / phase angle
y-axis: coherence or return stability
points: transition samples
shape/type: Delta class
opacity/size: doping intensity
```

This supports inspection of lawful windows where formation is coherent,
damped, unstable, or return-stable.

### Doped DDSS Heatmap

The DDSS heatmap should compare surfaces across Delta:

```text
x-axis: Delta step or time
y-axis: slice axis / formation surface
cell value: phase sensitivity, coherence, damping, or collapse pressure
```

Rows may include:

```text
compass-orientation
oe-cleave
zed-return
shell-harmonic
goa-review
selfgel-candidate
```

### Before / After / Doped Alluvial

The alluvial view compares undoped and doped transition paths:

```text
undoped transition path
doped transition path
denied mutation path
Zed return path
GoA holding path
```

The purpose is to show a modeled transition difference, not to claim quantum
cognition.

### 3D Doped Transition Cluster

Use the 3D cluster as exploratory structure, not validator proof.

Axes may be:

```text
x = phase / theta angle
y = coherence / resonance strength
z = collapse pressure or Zed-return distance
```

Point color or shape may encode:

```text
Delta class
slice axis
doping profile
admission posture
gate state
```

Potential regions:

```text
stable return zone
high ambiguity zone
damped residue zone
GoA holding zone
false-coherence risk zone
```

Clustering is interpretive. It demonstrates structure in telemetry; it does not
validate the ontology by itself.

### Opal Transition Cloud

The Opal transition cloud is a later integrated view:

```text
outer shell = protected body boundary
slice planes = HolographicSliceFrames
internal shimmer = phase/coherence/doping field
dark or masked regions = protected or denied projection zones
bright stable bands = Zed-return stable formations
cloud turbulence = unresolved GoA / high Delta ambiguity
```

It must be backed by the diagnostic views.

## Math Posture

Theta/quantum doping adds a doping parameter and a phase coordinate to the
composition without changing the custody law:

```text
mu_theta,q : (O, S, P, B, Delta, C, G, theta, q) -> M_c
eta_theta,q : M_c -> E_c
P_alpha,theta : E_c -> H_alpha,theta
R_log' = R_log + receipt(H_alpha,theta, E_c, G)
```

Safety spine:

```text
quantum/theta modifies transition sensitivity
but custody remains classical, append-only, and inspectable
```

This supports an IUTT-inspired reconstruction and gluing posture, not an IUTT
proof claim.

The correct public framing is:

```text
IUTT-inspired reconstruction / gluing / transport discipline
```

not:

```text
we mathematically implement IUTT
```

HDT mapping:

```text
local chart            = one HolographicSliceFrame
family of charts       = Stack of Slices
transition across Delta = DDSS
phase-rich transition  = Photonic Saturation DDSS
transport law          = allowed comparison/projection rules
gluing law             = lawful reconstruction across compatible slices
invariants             = Zed return, custody digest, denial boundaries
```

Sober bench name:

```text
ThetaDopedTransitionBench
```

Research identifier:

```text
QDT-001: Quantum-Doped Theta Transition Bench
```

Doctrine:

```text
Quantum doping is modeled as a bounded transition-sensitivity modifier.
It is not a quantum cognition claim, not .Actual activation,
not GEL admission, and not a replacement for classical custody.
```

## Compact Doctrine

```text
Telemetry shows weather.
Graphs show relation.
Delta views show transition.
Slice views show lawful projection.
Harmonic views show coherence.
Gate views show discipline.
Opal views show integrated formation beauty.
```
