# Trivium Forum Connector Body

Trivium Forum is the wrapper and adjudication body for external LLM
participation.

Its purpose is to let top-tier engines participate through proper MCP,
OAuth, tunnel, and provider-native access surfaces without modifying their
base model code.

```text
Frontier LLM
  unchanged provider engine

Trivium Forum
  wrapper/adjudication body
  OAuth/tunnel/MCP/provider-surface mediation

SLI
  Cryptic-governed symbolic access gate

MoS
  Mantle of Sovereign standing and lineage check

Sanctuary.exe
  local witness, receipts, gates, and cold tool body
```

## Owned Surfaces

Trivium Forum owns the external connector membrane:

```text
MCP adapter
OAuth provider posture
secure tunnel selection
token scope adjudication
rate-limit policy
cross-agent review
provider-surface mediation
```

It does not own Sanctuary core organs:

```text
Trivium Forum wraps access.
Trivium Forum does not rewrite the LLM.
Trivium Forum does not replace provider safety.
Trivium Forum does not own GEL.
Trivium Forum does not admit SelfGEL.
Trivium Forum does not activate Actual states.
```

## Build Command

```powershell
.\tools\Invoke-SanctuaryTool.ps1 -Command trivium-forum-connector-posture -Json
```

This command writes the posture only. It does not open a public port, create a
tunnel, issue OAuth tokens, call providers, bind models, grant authority, or
authorize action.

An external provider relation can be staged as a candidate standing record:

```powershell
.\tools\Invoke-SanctuaryTool.ps1 -Command external-llm-standing-probe -LicenseScope "OpenAI.ChatGPT.MCP" -Json
```
