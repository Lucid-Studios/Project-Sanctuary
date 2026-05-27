# MoS Mantle Of Sovereign

MoS is the Mantle of Sovereign: the Cryptic-root lineage mantle for every
birthed MCE/CME within a typed subset.

MoS is not generic memory and not a grant of sovereignty. It is the organ that
records standing:

```text
which MCE/CME exists
which typed subset it belongs to
what lineage standing it carries
what Cradle or swarm relation it holds
what access surfaces may be adjudicated
what receipts prove formation
what is denied, expired, revoked, or unresolved
```

## Organ Posture

```text
MoS records standing.
Cryptic roots the mantle.
Cradle uses MoS for formation lineage.
Swarm systems use MoS for typed membership.
Trivium Forum may ask MoS about external access standing.
Sanctuary receipts the event.
```

The mantle name does not claim legal, moral, or metaphysical sovereignty:

```text
Mantle of Sovereign != sovereignty claim
lineage record != personhood claim
birth record != action authority
standing != permission
swarm membership != autonomy
provider standing != raw credential custody
```

## Record Shape

An MoS lineage record should carry:

```text
cme_id
typed_subset
lineage_member_kind
formation_receipt_handle
root_cryptic_tip_digest
cradle_relation
swarm_standing
provider_surface_standing_candidates
revocation_or_expiry_state
denial_surface
```

It must not carry:

```text
raw password
raw OAuth token
raw provider session cookie
secret payload
unreviewed GEL admission
unreviewed SelfGEL mutation
unreviewed action authority
```

## Build Command

```powershell
.\tools\Invoke-SanctuaryTool.ps1 -Command mos-lineage-register -Json
```

The command writes cold lineage residue only. It does not grant authority,
authorize action, admit GEL, mutate SelfGEL, call providers, bind models,
activate CME.Actual, activate Sanctuary.Actual, claim personhood, or claim
sovereignty.
