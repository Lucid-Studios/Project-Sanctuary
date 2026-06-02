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
  loopback HTTP alpha service or Sanctuary-owned HTTPS edge
  owns tool allowlist
  runs SanctuaryReceiptService
  returns sanitized receipt summaries
  refuses unknown tools closed
```

The service is loopback-first by default. For Lab-owned GPT testing, run the
same Sanctuary executable as an explicit HTTPS edge with a Lab-controlled
domain and certificate:

```text
ChatGPT
-> https://<your-lab-domain>/mcp
-> Sanctuary.exe HTTPS edge
-> SanctuaryReceiptService
```

Remote ChatGPT use still requires a reachable HTTPS MCP endpoint. The preferred
Lab posture is a Sanctuary-owned endpoint, not a third-party quick tunnel. The
alpha service does not return local receipt paths, receipt bodies, source
paths, or secret payloads to remote GPT surfaces.

The alpha service exposes two MCP-compatible transport shapes:

```text
POST /mcp
  JSON-RPC request/response path for local benches and tunnel clients

GET /sse
POST /sse/messages?sessionId=...
  HTTP+SSE compatibility path for MCP clients that scan an SSE endpoint
```

The ChatGPT custom app field should point at the Lab-owned HTTPS MCP endpoint,
not the raw loopback service:

```text
https://<your-lab-domain>/mcp
```

Current Lab note:

```text
remote GPT app connection: parked
reason: owned ingress depends on second Starlink route in bypass mode
desired edge: Sanctuary.exe behind Lab-managed router/server
dns target: sanctuary.lucidtechnologies.tech
```

Until that route is live, the GPT lane remains available for local loopback
benches and dev-certificate smoke tests only. Any temporary tunnel remains a
reviewed fallback, not the preferred Lab membrane.

The public HTTPS/OAuth connector membrane belongs to the Trivium Forum tool
body, but Trivium can now be hosted by Sanctuary itself:

```text
ChatGPT / remote caller
-> Trivium Forum connector membrane
-> reviewed Sanctuary-owned HTTPS/OAuth/adjudication layer
-> Sanctuary.exe MCP alpha service
```

Sanctuary remains the local witness and receipt body. Trivium Forum owns the
external exposure question: tunnel selection, OAuth provider posture, token
scope adjudication, rate limits, public connector policy, and cross-agent
review before any remote caller reaches the local service.

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
sanctuary.bridge_morphism_test
sanctuary.cme_theory_body
sanctuary.operator_work_cme_ec_gap
sanctuary.telemetry_slice_register
sanctuary.extended_telemetry_weather
sanctuary.cgoa_formation
sanctuary.codex_governing_witness
sanctuary.full_body_io_runtime
sanctuary.gel_approval_nadir_return
sanctuary.approval_closure_register
sanctuary.coupling_control_surface_register
sanctuary.actualization_state_register
sanctuary.stem_domain_training_certification
sanctuary.lab_observation_digest
sanctuary.research_latex_export
sanctuary.construct_custody_register
sanctuary.gel_crystal_register
sanctuary.gel_reforge_bench
sanctuary.gpt_use_case_testing
sanctuary.mos_lineage_register
sanctuary.sli_access_gate_register
sanctuary.trivium_forum_connector_posture
sanctuary.external_llm_standing_probe
sanctuary.cradle_boundary_organ_register
```

No reviewed performance command is exposed in this alpha surface:

```text
gel-admission
selfgel-admission
cme-actual-keypair-forge
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

Start a lab-only Trivium Forum HTTPS bridge for ChatGPT developer-mode
connector testing. Preferred owned-edge posture:

```powershell
.\tools\Start-SanctuaryEdgeGateway.ps1 `
  -HostName "0.0.0.0" `
  -Port 443 `
  -PublicBaseUrl "https://<your-lab-domain>" `
  -CertificatePath "<path-to-lab-domain.pfx>" `
  -CertificatePasswordEnv "SANCTUARY_EDGE_CERT_PASSWORD"
```

Use `https://<your-lab-domain>/mcp` as the ChatGPT MCP Server URL.

Temporary tunnel fallback:

```powershell
.\tools\Start-TriviumForumHttpsTunnel.ps1 -Protocol http2 -Json
```

Use the returned `chatGptMcpServerUrl` only when the owned edge is unavailable.
This temporary bridge exists only to let ChatGPT reach the local MCP service
over HTTPS during alpha testing; it does not issue OAuth tokens, expose secret
intake, grant action, or admit GEL/SelfGEL.

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

The ChatGPT app alpha is currently a tool-only Apps SDK posture. Sanctuary
advertises MCP tools with input schemas, output schemas, read-only/destructive
annotations, and invocation metadata; widget resources remain absent until a
read-only receipt/weather console is intentionally added with CSP.

The duplex membrane tool is:

```text
sanctuary.agenticore_duplex_lisp_membrane
```

It writes `agenticore-duplex-lisp-membrane` candidate residue for the
Codex-extension/ChatGPT-app bridge without provider calls, model binding,
external action, GEL admission, SelfGEL mutation, or Actual activation.

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
