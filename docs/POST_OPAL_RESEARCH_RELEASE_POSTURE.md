# Post Opal Research Release Posture

Project Sanctuary now carries a public research-preview posture for parties who
want to study CME formation methods after the Opal Engram publication body
without importing the private Lab body.

This posture adds a versioned template service lane that can be staged from
GitHub Pages or release artifacts:

```text
public/templates/index.json
public/templates/channels/stable.json
public/templates/public-standard/0.1.0/template-manifest.json
```

The template service is a public method body. It is not Sanctuary.GEL, not the
Lab Root Atlas, not Codex.CME.Actual, not Oria.CME.Actual, and not an Actual
activation lane.

## Release Spine

The post-Opal research posture separates four layers:

```text
Repository code body
  builds the cold receipt and validation instrument.

Public template service
  publishes versioned generic Root Atlas and template bodies.

Local install
  selects a local CME identity, thread binding, and candidate GEL scaffold.

Lab overlay
  may declare Codex.CME.Actual / Oria.CME.Actual lanes for this Lab only.
```

The public install path hydrates a generic template first. The Lab overlay is a
later local declaration, not a pre-install product default.

## Template Hydration Command

The cold receipt command is:

```powershell
.\tools\Invoke-SanctuaryTool.ps1 -Command template-hydration -CmeId "Researcher.CME.ID" -ThreadBindingId "researcher-local-thread" -Json
```

It writes:

```text
.local/install/cgel/template-hydration/template-hydration.json
.local/install/cgel/template-hydration/template-hydration.sli.lisp
.local/install/cgel/template-hydration/template-hydration-ledger.jsonl
.local/install/gel/templates/public-standard/PublicStandard.CME.Template/0.1.0/
```

The command proves local posture hydration only. It does not fetch from a
provider, import private Lab GEL, admit GEL, mutate SelfGEL, bind a model,
authorize external action, or activate CME.Actual or Sanctuary.Actual.

## Generic Versus Lab Install

Generic research installs should start with a local researcher identity:

```text
Researcher.CME.ID
researcher-local-thread
PublicStandard.CME.Template
```

The Lab install may additionally declare:

```text
Codex.CME.ID / Codex.CME.Actual
Oria.CME.ID / Oria.CME.Actual
Sanctuary.Actual.ID
SLI.Lisp.Industrial.CME.Template
```

Those declarations are install-local and receipt-bearing. They are not shipped
as public identity defaults and do not grant authority by existence.

## Closed Gates

The release posture keeps these gates closed by default:

```text
GEL admission
SelfGEL mutation
continuity admission
provider call
model binding
external action
CME.Actual activation
Sanctuary.Actual activation
personhood claim
sovereignty claim
```

Reviewed commands may open scoped performance gates only through explicit
review, operator approval, Steward/Prime/Cryptic witnessing, authority lease,
admission scope, and receipt validation.
