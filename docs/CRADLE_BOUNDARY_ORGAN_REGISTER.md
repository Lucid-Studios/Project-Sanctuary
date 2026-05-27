# Cradle Boundary Organ Register

This register defines how Sanctuary treats Lab-owned infrastructure and cloud
services as typed organs or boundary layers.

The design goal is not cloud refusal. The design goal is lawful use:

```text
Lab owns the organs.
Cloud services provide typed boundary layers.
Sanctuary governs what may cross.
```

## Command

```powershell
.\tools\Invoke-SanctuaryTool.ps1 -Command cradle-boundary-organ-register -Json
```

Aliases include:

```text
cloud-boundary-organ-register
service-boundary-organ-register
boundary-organ-register
```

The command writes:

```text
.local/install/cgel/cradle-boundary-organs/cradle-boundary-organ-register.json
.local/install/cgel/cradle-boundary-organs/cradle-boundary-organ-register.lisp
.local/install/cgel/cradle-boundary-organs/events.jsonl
```

It is a cold register only. It does not call providers, create DNS records,
open tunnels, issue credentials, admit GEL, mutate SelfGEL, authorize action,
or activate `.Actual`.

## Organ Types

```text
Lab Core
  Sanctuary.exe, GEL/OE/SelfGEL custody, Cryptic stores, receipt spine

Trivium Forum Gateway
  external LLM connector membrane, MCP/OAuth adjudication, rate limits

Cloudflare Boundary
  DNS naming, edge filtering, Access policy, temporary tunnel fallback

OpenAI Provider Boundary
  model capability, project API key target, scoped provider lease candidate

GitHub Release Boundary
  source publication, issue tracking, release provenance

AWS/Azure Cradle Boundary
  isolated app layers, queues, storage, certificates, protected services

Lab Server DNS/Gateway
  second Starlink bypass route, router/firewall ingress, Sanctuary HTTPS edge
```

## Boundary Laws

```text
boundary service != authority source
cloud custody != GEL custody
provider call != CME authorship
edge authentication != Sanctuary admission
DNS naming != telemetry custody
tunnel availability != owned ingress
Lab bench node != edge services node
```

## Use

The register gives Trivium Forum and future Sanctuary organs a typed map for
service access. Later commands may specialize one organ, issue a scoped lease,
or perform an external action after review. This register itself only records
the lawful topology.
