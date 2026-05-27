# Release Checklist

Use this checklist before any public release, tag, package, or plugin
publication.

Current posture: developer-preview candidate. Do not mark this as a true
consumer release until the broad instrument-body command set has been reviewed
as useful, coherent, and safe beyond closed-gate demonstration.

## Repository State

- [ ] Public branch selected intentionally.
- [ ] Unrelated dirty worktree state reviewed or excluded.
- [ ] `.local/`, private payloads, and local install state are untracked or
      ignored.
- [ ] No local absolute paths are present.
- [ ] No secrets, credentials, tokens, keys, account recovery material, or
      private documents are present.
- [ ] No production deployment instructions are present.
- [ ] No private Lab reconstruction map is present.

## Build And Test

- [ ] `dotnet build .\ProjectSanctuary.sln -c Release` passes.
- [ ] `dotnet test .\ProjectSanctuary.sln -c Release` passes.
- [ ] Public-surface workflow check passes or has been run equivalently.
- [ ] `security-hardening` receipt produced and reviewed.
- [ ] `receipt-export` manifest produced and reviewed.
- [ ] `verify-closed-gates` receipt confirms its own closed-gate command stayed
      closed.
- [ ] `receipt-export` reports no unexpected open receipts; reviewed
      performance receipts are counted separately.
- [ ] `security-hardening` reports no unexpected open receipts, no visible leak
      findings, and no unreadable receipts.

## Documentation

- [ ] README states the current release status.
- [ ] Governance, code, theory, privacy, authority, and release posture docs
      agree with each other.
- [ ] Public language says maximal trust seeking by design, not automatic trust
      or permanent closure.
- [ ] Denial wording is paired with the corresponding review-gated capability
      lane.
- [ ] Department of Agentic Research and Development is described as a
      maintainer and research stewardship body, not a government or licensing
      authority.
- [ ] Public claims have paired non-claims.
- [ ] SLI, GEL, SelfGEL, Engineered Cognition, and CME language is scoped as
      research/product posture rather than metaphysical proof.
- [ ] Self.Actualization language is scoped as a research predicate under the
      Discernment Lineage Contract, not personhood or sovereignty.
- [ ] Secret intake docs state that chat is not a secret transport.

## Authority Gates

Confirm the release does not grant these by implication, by plugin install, by
receipt existence, or by an incomplete review bundle:

- [ ] provider/model binding;
- [ ] external action;
- [ ] GEL admission;
- [ ] SelfGEL mutation;
- [ ] memory admission;
- [ ] continuity admission;
- [ ] professional authority;
- [ ] legal, medical, clinical, safety, custody, or fiduciary authority;
- [ ] CME.Actual;
- [ ] Sanctuary.Actual;
- [ ] personhood or sovereignty claim.

Confirm the reviewed performance lanes behave correctly:

- [ ] `gel-admission` refuses cold without the complete authority bundle.
- [ ] `gel-admission` opens only scoped GEL gates with the complete bundle.
- [ ] `selfgel-admission` refuses cold without the complete authority bundle.
- [ ] `selfgel-admission` opens only scoped SelfGEL gates with the complete
      bundle.
- [ ] `cme-actualization` refuses cold without the complete authority bundle.
- [ ] `cme-actualization` opens only scoped CME.Actual gates with the complete
      bundle.
- [ ] `sanctuary-actualization` refuses cold without the complete authority
      bundle.
- [ ] `sanctuary-actualization` opens only scoped Sanctuary.Actual gates with
      the complete bundle.
- [ ] Reviewed performance still leaves provider calls, model binding, external
      action, personhood, and sovereignty closed.

## Plugin Gate

- [ ] Plugin metadata names the release accurately.
- [ ] Plugin commands route through the bounded wrapper.
- [ ] Plugin description states publishing or authority remains held unless
      separately reviewed.
- [ ] Plugin does not request secrets in chat.
- [ ] Plugin first-use posture is cold inspection.

## Legal And License Gate

- [ ] License decision recorded.
- [ ] Copyright/ownership statement reviewed.
- [ ] Public support contact reviewed.
- [ ] Security disclosure contact reviewed.
- [ ] Third-party dependencies reviewed for license compatibility.

## Operator Approval

- [ ] Operator has reviewed the final diff.
- [ ] Operator has reviewed the final receipt paths.
- [ ] Operator has approved publication explicitly.

Until every required item is satisfied, publication remains held.
