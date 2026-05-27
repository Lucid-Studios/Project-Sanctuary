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
Sanctuary-owned HTTPS edge selection
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

## Sanctuary-Owned Lab Edge

The preferred Lab-facing connector membrane is owned by `Sanctuary.exe`
itself:

```powershell
.\tools\Start-SanctuaryEdgeGateway.ps1 `
  -HostName "0.0.0.0" `
  -Port 443 `
  -PublicBaseUrl "https://<your-lab-domain>" `
  -CertificatePath "<path-to-lab-domain.pfx>" `
  -CertificatePasswordEnv "SANCTUARY_EDGE_CERT_PASSWORD"
```

This starts a native HTTPS MCP edge on the Lab machine and serves:

```text
GET  /health
GET  /tools
GET  /.well-known/sanctuary-lab.json
GET  /app/manifest.json
POST /mcp
GET  /sse
POST /sse/messages
```

The ChatGPT custom app MCP Server URL becomes:

```text
https://<your-lab-domain>/mcp
```

For the current Lab domain, reserve the root portal and use a subdomain:

```text
lucidtechnologies.tech
  Society of the Crystallized Mind portal

sanctuary.lucidtechnologies.tech
  Sanctuary MCP Lab edge
```

Direct home hosting is the first preferred route:

```text
sanctuary.lucidtechnologies.tech
-> Starlink public route
-> Lab router/firewall TCP 443
-> Sanctuary.exe EdgeGateway
```

Current Lab decision:

```text
Sanctuary-owned GPT/MCP edge
  status: parked infrastructure dependency
  waiting_on: second Starlink route in bypass mode with Lab-managed router/server
  preferred_dns: sanctuary.lucidtechnologies.tech
  acceptable_alpha_state: local loopback and dev-certificate smoke tests only
```

The Lab server may later host DNS or gateway services once the network route is
owned end-to-end. Until that route exists, production-like ChatGPT app testing
must not depend on the default app-only Starlink router, unreviewed tunnels, or
implicit loopback exposure. The registrar/DNS zone can continue to hold the
public records until the Lab explicitly moves authoritative DNS to a reviewed
server posture.

Before changing DNS, run:

```powershell
.\tools\Test-SanctuaryEdgeNetwork.ps1 `
  -DomainName "sanctuary.lucidtechnologies.tech" `
  -Port 443 `
  -Json
```

Starlink may place residential service behind CGNAT. If the Starlink router WAN
address is private or in `100.64.0.0/10`, direct IPv4 inbound hosting will not
work without Public IP/business support, IPv6, or a Lab-owned relay. That is a
network-route limitation, not a Sanctuary limitation.

This route still exposes only the cold GPT alpha allowlist. It does not expose
secret intake, reviewed performance commands, provider/model calls, external
actions, GEL/SelfGEL admission, or Actual-state mutation through ChatGPT.

## Lab HTTPS Bridge Dev Fallback

The temporary tunnel bridge is retained only as a dev fallback when a reviewed
Lab domain and certificate are not available:

```powershell
.\tools\Start-TriviumForumHttpsTunnel.ps1 -Protocol http2 -Json
```

The launcher verifies the local `Sanctuary.exe serve-mcp` loopback service,
starts a Cloudflare quick tunnel, records tunnel state under
`.local/trivium-forum/tunnel`, and returns a `chatGptMcpServerUrl` suitable for
short-lived ChatGPT developer-mode connector testing.

This is not the Lab-owned connector membrane. It does not issue OAuth tokens,
grant provider/model access, admit GEL/SelfGEL, or activate Actual states. It
only bridges HTTPS to the cold MCP tool allowlist when the owned edge is not
available.

An external provider relation can be staged as a candidate standing record:

```powershell
.\tools\Invoke-SanctuaryTool.ps1 -Command external-llm-standing-probe -LicenseScope "OpenAI.ChatGPT.MCP" -Json
```

The service-boundary organ topology can be staged without touching any provider
or DNS surface:

```powershell
.\tools\Invoke-SanctuaryTool.ps1 -Command cradle-boundary-organ-register -Json
```
