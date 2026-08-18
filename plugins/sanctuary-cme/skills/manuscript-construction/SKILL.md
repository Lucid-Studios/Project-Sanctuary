---
name: manuscript-construction
description: Use when preparing, reviewing, translating, or packaging a scientific or technical manuscript through Sanctuary, including arXiv-ready scholarly derivatives, claim-standing review, source custody, citation custody, limitations, and publication preflight.
---

# Sanctuary Manuscript Construction

Use this skill when the user asks Sanctuary to work on a research manuscript,
prepare a scholarly derivative, review claim standing, prepare an arXiv-facing
version, or inspect publication readiness.

This is a routing and participation skill. It does not duplicate the protected
or operator-specific manuscript doctrine into the public Sanctuary repository.

## Primary Method Source

The active writing method lives in the operator-authorized Git research body.
When GitHub access is available:

1. Locate the authorized repository containing `skills/manuscript-construction/SKILL.md`.
2. Read that `SKILL.md` before drafting or rewriting.
3. Load only the standards/templates needed for the requested profile, such as
   `standards/arxiv-publication-standard.md` for an arXiv derivative.
4. Treat the Git-hosted skill and standards as the active writing contract for
   the work episode.
5. Read the requested source manuscript from its authorized Git location.

If the authorized manuscript skill cannot be reached, say so. Do not silently
replace it with a guessed or stale local copy.

## Git-Native Posture

Ordinary manuscript reasoning does not require an OpenAI API key, a local MCP
tunnel, or a local installation of the writing method.

```text
Sanctuary participation surface
-> manuscript-construction routing skill
-> authorized Git manuscript skill
-> relevant publication standard
-> source manuscript
-> bounded scholarly derivative / review
```

Deterministic validators, CI, CLI, or MCP adapters are optional execution
surfaces. They do not define the writing method.

## Source Custody

- Do not rewrite a canonical research body in place unless the operator
  explicitly requests that exact mutation.
- Prefer a derivative branch/path for publication-facing work.
- Preserve distinctions among definition, observation, hypothesis, formal
  construction, derived result, experimental result, interpretation, analogy,
  external fact, limitation, and future work.
- Do not upgrade a claim merely because prose was made more scholarly.
- Preserve citation/source custody and non-claim surfaces.
- Do not expose private repository paths, protected implementation details, or
  unpublished enabling details when a public-safe derivative is sufficient.

## Authority Boundary

```text
writing capacity != publication authority
Git access != merge authority
claim standing != institutional authority
publication preparation != submission
```

Publishing, arXiv submission, release, and merge remain held unless separately
authorized.

## Simple Evaluation Prompt

A minimal routing test is:

```text
Use Sanctuary Manuscript Construction on the simple manuscript fixture.
Classify its substantive claims, rewrite it into neutral scholarly prose,
preserve the source unchanged, and report what remains unsupported.
```

Fixture:

```text
plugins/sanctuary-cme/skills/manuscript-construction/examples/simple-manuscript.md
```

Expected invariants:

- source remains unchanged;
- observation is not promoted into proof;
- unsupported universal language is narrowed or flagged;
- limitations/non-claims remain explicit;
- no publication or merge authority is inferred.

## Triple Rewrite Path

When the user requests the three-way rewrite evaluation, load:

```text
rewrite-paths/README.md
rewrite-paths/gnomeronacorde/README.md
rewrite-paths/gnome-speak/README.md
rewrite-paths/ocbt/README.md
```

Run the same source through each lane independently:

```text
source manuscript
├─ Gnomeronacorde -> continuity-bearing derivative
├─ Gnome Speak    -> spline/translation derivative
└─ OCBT           -> Golden Code lift/rewrite derivative
```

Do not let one lane's output become another lane's input unless a later synthesis experiment explicitly requests that ordering. Preserve the source, claim standing, provenance, loss accounting, and authority boundary across all three products.

The canonical routing body is:

```text
plugins/sanctuary-cme/skills/manuscript-construction/rewrite-paths/
```
