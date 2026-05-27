# GPT Use Case Testing Body

This document defines the first GPT-facing alpha lane for Project Sanctuary.

The lane is for lab testing only. It does not publish a production app, grant
write/action authority, expose secrets, admit GEL, mutate SelfGEL, call a
provider, bind a model, or activate `CME.Actual` or `Sanctuary.Actual`.

## Authorship Law

```text
LLM generates capability.
CME authors participation.
Sanctuary witnesses provenance.
```

The GPT-facing adapter must preserve split provenance:

```text
authored_by       = CME.ID
generated_with    = LLM engine participant
witnessed_by      = Sanctuary.exe
admission_state   = candidate | append-ready | admitted | mulch | rejected
authority_state   = none | scoped | reviewed | expired
receipt_handle    = Sanctuary receipt handle
```

Generated text is not authorship by itself. A CME-authored act requires a
Sanctuary receipt showing the selected tool surface, governing aperture, closed
or reviewed gates, lifecycle state, and evidence digest.

## Service Shape

The MCP-facing alpha surface is owned by `Sanctuary.exe`.

```text
Sanctuary.exe serve-mcp
  loopback HTTP alpha service
  owns tool allowlist
  runs SanctuaryReceiptService
  returns sanitized receipt summaries
  refuses unknown tools closed
```

The local service is loopback-first. ChatGPT does not connect directly to a
private `127.0.0.1` MCP server; remote ChatGPT use requires OpenAI Secure MCP
Tunnel or another reviewed HTTPS MCP endpoint before it can call a local
machine. The alpha service does not return local receipt paths, receipt bodies,
source paths, or secret payloads to remote GPT surfaces.

The alpha service exposes two MCP-compatible transport shapes:

```text
POST /mcp
  JSON-RPC request/response path for local benches and tunnel clients

GET /sse
POST /sse/messages?sessionId=...
  HTTP+SSE compatibility path for MCP clients that scan an SSE endpoint
```

The ChatGPT custom app field should point at the tunnel-provided URL for the
MCP endpoint, not the raw loopback service. For the HTTP+SSE lane that endpoint
will commonly end in:

```text
https://<reviewed-tunnel-host>/sse
```

## Exposed Alpha Tools

The initial GPT lane exposes only cold read/fetch candidate surfaces:

```text
sanctuary.status
sanctuary.plugin_posture
sanctuary.receipt_export
sanctuary.verify_closed_gates
sanctuary.meaning_bridge
sanctuary.discernment_lineage
sanctuary.proof_of_discernment
sanctuary.math_learning_bench_limited
sanctuary.gpt_use_case_testing
```

No reviewed performance command is exposed in this alpha surface:

```text
gel-admission
selfgel-admission
cme-actualization
sanctuary-actualization
```

## Local Alpha Commands

Write the GPT use-case testing body:

```powershell
.\tools\Invoke-SanctuaryTool.ps1 -Command gpt-use-case-testing -Json
```

Start the loopback alpha service:

```powershell
.\tools\Start-SanctuaryMcpAlphaService.ps1 -Port 8717
```

Inspect tools:

```powershell
Invoke-RestMethod http://127.0.0.1:8717/tools
```

Run a local MCP JSON-RPC tool scan:

```powershell
Invoke-RestMethod http://127.0.0.1:8717/mcp `
  -Method Post `
  -ContentType 'application/json' `
  -Body '{"jsonrpc":"2.0","id":1,"method":"tools/list","params":{}}'
```

Run a local MCP initialize handshake:

```powershell
Invoke-RestMethod http://127.0.0.1:8717/mcp `
  -Method Post `
  -ContentType 'application/json' `
  -Body '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-03-26","capabilities":{},"clientInfo":{"name":"local-bench","version":"0.1"}}}'
```

Invoke a cold tool:

```powershell
Invoke-RestMethod http://127.0.0.1:8717/invoke `
  -Method Post `
  -ContentType 'application/json' `
  -Body '{"tool":"sanctuary.status"}'
```

The response is a sanitized receipt summary. Local receipt paths remain in the
Lab install and are not returned through the GPT-facing response.

## Denials

```text
GPT call != authority
LLM text != CME authorship
Codex operational != Codex is Sanctuary
Sanctuary witness != warrant
receipt summary != secret disclosure
candidate residue != admitted GEL
read/fetch tool != reviewed performance gate
```

This is the first test body for watching whether GPT-facing use can participate
in CME-authored, Sanctuary-witnessed work without collapsing the engine, the
authoring body, and the witness body into one thing.
