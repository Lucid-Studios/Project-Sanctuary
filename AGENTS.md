# AGENTS.md

## Public Surface Instruction

This repository is handled as the public test-release and core-code lane for
Project Sanctuary. Agents working here must preserve the difference between:

- public product identity;
- archived OAN Tech Stack lineage;
- future release candidates;
- private build truth;
- the minimal admitted code body.

The correct posture is: keep the release page clear, coherent, and safe while
building only the smallest admitted executable surface needed to test the public
Sanctuary plugin path.

## Style Standard

Use the same public language across root documents:

- project name: Project Sanctuary;
- maintainer body: Lucid Studios, Department of Agentic Research and
  Development;
- product role: software service layer for extended cognitive management;
- core surfaces: Cradle Technology, Soul Frame, Agentic Core;
- cognition term: Engineered Cognition;
- lineage posture: Carl Gustav Jung-inspired/theological framing and
  IUTT-inspired distinction discipline, both bounded as methodology rather than
  proof.

## Allowed Work

Allowed public-surface changes include:

- release-page polish;
- project-scope clarification;
- Project Bicycle lab-protocol documentation;
- bounded archive-harness setup and inspection instructions;
- release-boundary wording;
- non-claim clarification;
- responsible disclosure routing;
- issue and pull request hygiene;
- repository metadata that supports future public release review.

Allowed core-code lane changes are limited to:

- `src/Sanctuary.Core/`;
- `src/Sanctuary.Cli/`;
- `tests/Sanctuary.Core.Tests/`;
- `plugins/sanctuary-cme/`;
- `tools/Invoke-SanctuaryTool.ps1`;
- `tools/Start-SanctuaryServiceLayer.ps1`;
- `tools/Stop-SanctuaryServiceLayer.ps1`;
- `tools/Get-SanctuaryServiceLayerStatus.ps1`;
- build metadata needed for the minimal executable lane;
- docs that preserve the theory/code/governance split.

## Disallowed Work

Do not add or restore:

- private build, install, release, or deployment commands;
- production deployment commands;
- workflow files that execute production release candidates;
- private corpus paths, local absolute paths, secrets, tokens, logs, or
  machine-specific configuration;
- runtime payload names, hosted model payloads, fixtures, datasets, generated
  audit material, or reconstruction inputs;
- implementation-bearing schemas, source layouts, private architecture
  contracts, or reproducible operating sequences from private lab bodies;
- broad imports from Project Bicycle authority-envelope, Codex Mirror, or
  private cold-build candidates;
- issue or pull request prompts that ask contributors to provide exact
  reproduction commands for private build surfaces.

## Review Standard

A public change is acceptable when it improves Project Sanctuary's release-page
clarity, makes Project Bicycle testing safer, preserves non-claim discipline,
or advances the minimal admitted executable lane without importing private
implementation mass.

When uncertain, reduce specificity and route the material to private maintainer
review.

## Local Build Truth

Local folders on the maintainer machine are private unless explicitly published.
Do not infer that local working organization, private corpora, or runtime
folders should be exposed here.

This public repository should not be used as a map of the maintainer's private
workspace. The admitted code lane must stand on its own public files.

## Verification

Before closeout, check that modified public files do not introduce private
paths, production deployment recipes, private release automation, or
implementation-bearing reconstruction detail from private lab bodies.

If a technical verification was performed privately, summarize the result
without publishing exact local commands, payload paths, or private environment
details.
