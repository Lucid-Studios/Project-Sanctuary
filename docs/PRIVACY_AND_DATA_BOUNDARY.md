# Privacy And Data Boundary

Project Sanctuary's public release must keep source, carrier, residue, receipt,
secret custody, and action authority separate.

## Public Repository Boundary

The public repository may carry:

- source code for the admitted core executable lane;
- tests for closed-gate behavior;
- documentation of the governance model;
- public plugin metadata;
- public wrapper scripts;
- examples that do not expose private payloads.

The public repository must not carry:

- `.local/` install state;
- private GEL, cGEL, OE, SelfGEL, cSelfGEL, or cryptic store payloads;
- private identity documents;
- credential copies;
- legal documents;
- Social Security, passport, tax, business-license, or account-recovery
  material;
- raw operator logs or private receipt bodies;
- provider keys, tokens, model credentials, or deployment secrets;
- machine-local paths or reconstruction maps.

## Chat Boundary

Chat is not a secret transport.

If a command or workflow needs sensitive material, the public posture is:

```text
prepare a local intake surface
tell the Operator where it is
refuse chat-secret passage
wait for local proceed-gated review
emit a receipt
keep admission false
```

## Receipt Boundary

Receipts may show:

- command name;
- outcome;
- disposition;
- gate state;
- hashes or digests;
- counts;
- review posture;
- local receipt path when running inside the Lab.

Public release material should not publish:

- secret payload content;
- raw private receipt bodies;
- cryptic store contents;
- source paths for private documents;
- private local install paths;
- sensitive operator details.

## GEL Boundary

Public documentation may describe GEL concepts. It must not publish private
GEL bodies or imply that public residue has become admitted truth.

```text
candidate residue != admitted GEL
local witness ledger != public truth
receipt count != memory admission
SelfGEL support != SelfGEL mutation
encrypted custody != authority grant
```

## Data Handling Rule

When uncertain, publish the method and withhold the payload.

```text
method can be public
payload stays private
digest can support review
plaintext requires custody
custody requires authorization
authorization requires a separate gate
```
