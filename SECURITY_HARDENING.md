# Security Hardening

## Purpose

This document defines the public repository hardening posture for Project
Sanctuary.

## Public Surface Rule

The public repository is a test-release shell with one admitted core executable
lane. It may contain the minimal Sanctuary core, CLI, tests, wrapper, Codex
plugin surface, public documentation, and release checks needed to build and
inspect cold receipt-bearing behavior.

It must not contain the private Lab ecology, production deployment recipes,
private build automation, datasets, fixtures, runtime payloads, private
topology, local environment assumptions, secrets, or reconstruction material
that would turn the public floor into the private build map.

## CI Sources

Reusable GitHub workflows should be pinned to reviewed immutable refs when used.
Repository-local checks should enforce the public surface rule.

## Intake Rule

Public issues and pull requests must not include sensitive private material. If
a disclosure requires technical detail that could help reconstruct the private
build, route it to private maintainer review instead.

## Current Public Surface

Allowed tracked surfaces:

- `.github/` metadata and public-surface checks;
- `src/Sanctuary.Core/`, `src/Sanctuary.Cli/`, and
  `tests/Sanctuary.Core.Tests/` as the admitted core executable lane;
- `tools/Invoke-SanctuaryTool.ps1` and `plugins/sanctuary-cme/` as the bounded
  local invocation surface;
- `docs/` public theory, code, governance, protocol, and release-boundary
  documents;
- solution/build metadata required for the admitted core lane;
- top-level security, support, conduct, contribution, and release posture files.

Disallowed tracked surfaces:

- private build, test, release, deployment, or automation scripts;
- production deployment instructions;
- datasets, fixtures, symbolic payloads, generated audit material, or runtime
  artifacts;
- internal architecture contracts, private working ledgers, and local `.local/`
  install state.
