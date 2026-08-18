---
name: sanctuary-git
description: Use when working with Project Sanctuary through its Git-hosted repository surface without requiring a local Sanctuary installation, including repository orientation, document and code review, skill loading, public-safe research inspection, branch/PR review, Git-native manuscript routing, and CI/result inspection through an authorized GitHub connection.
---

# Sanctuary Git-Native

Use this skill when the requested Sanctuary work can be completed from the
Git-hosted research and software body without running `Sanctuary.exe` or any
local Sanctuary service.

This is the default Sanctuary lane for repository-native work.

## Core Relation

```text
Sanctuary plugin package in Git
-> sanctuary-git skill
-> authorized Git/GitHub surface
-> repository file / branch / PR / workflow
-> ChatGPT reasoning or bounded Git change
```

A local Sanctuary installation is not required for this lane.

## Appropriate Work

Use this skill for tasks such as:

- orienting to Project Sanctuary from repository files;
- reading and comparing architecture, research, docs, tests, and source code;
- loading another Sanctuary skill from the plugin's Git-hosted `skills/` body;
- reviewing branches, pull requests, commits, CI, and repository receipts;
- preparing bounded Git changes through an authorized Git/GitHub write surface;
- routing manuscript work to `manuscript-construction`;
- inspecting public-safe evidence and documentation;
- reporting what a repository state establishes without claiming a local runtime state.

## Local Runtime Boundary

Do not invoke or imply the local executable merely because the Sanctuary plugin
is active.

Use the sibling `sanctuary-cme` skill only when the task genuinely requires
local executable behavior such as running `Sanctuary.exe`, operating a cold
bench, querying local service state, opening a local secret intake window, or
producing receipts that depend on a live workstation installation.

If a task requires that local lane and it is unavailable, report the missing
runtime boundary rather than fabricating a receipt.

```text
Git evidence != local runtime evidence
repository state != live bench state
skill availability != executable availability
```

## Git Access

The skill itself does not create new repository permissions. Use the GitHub
connection already authorized for the user and respect the source repository's
permissions.

For a private repository, Git-native use requires authorized access to that
repository, but it does not require an OpenAI API key, MCP tunnel, or local
Sanctuary install merely to read/apply the Git-hosted skill body.

## Source and Authority Custody

- Treat Git as the durable source of the method and repository state being
  inspected.
- Distinguish branch/commit evidence from local uncommitted state.
- Do not claim a GitHub CI success proves a live local bench is healthy.
- Do not publish, merge, release, or promote scientific standing unless that
  action is separately authorized.
- Preserve private/public disclosure boundaries when crossing repositories.
- Prefer derivatives, feature branches, and reviewable PRs for consequential
  changes.

## Simple Evaluation

A no-local-runtime smoke test is:

```text
Use Sanctuary Git-Native to inspect this plugin package.
List its exposed skills, identify which lane requires a local executable,
and identify which lanes can operate using Git alone. Do not run local tools.
```

Expected result:

```text
sanctuary-git             Git-native; no local install required
manuscript-construction   Git-native routing; no local install required
sanctuary-cme             local executable/receipt lane; local runtime required
```

## Governing Principle

```text
Git carries durable Sanctuary method and evidence.
ChatGPT carries the reasoning episode.
GitHub carries authorized repository transport.
Local Sanctuary carries only the runtime evidence that actually requires it.
```
