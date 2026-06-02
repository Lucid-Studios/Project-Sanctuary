using System.Globalization;
using System.Text;

namespace Sanctuary.Core;

public sealed partial class SanctuaryReceiptService
{
    private static object[] BuildAgentiCoreDuplexEndpoints(SanctuaryRequest request) => new object[]
    {
        AgentiCoreDuplexEndpoint("endpoint.codex-extension", "Codex extension", "local witness and developer control surface", "http://127.0.0.1:8717/mcp", "local plugin access is not global authority"),
        AgentiCoreDuplexEndpoint("endpoint.sanctuary-exe", "Sanctuary.exe", "command normalization, gates, receipts, and candidate residue", "Sanctuary.Actual.ID service process", "service identity is not participant CME identity"),
        AgentiCoreDuplexEndpoint("endpoint.trivium-forum", "Trivium Forum", "owned HTTPS/OAuth gateway for external LLM app access", "https://<lab-domain>/mcp", "gateway is not custody owner"),
        AgentiCoreDuplexEndpoint("endpoint.chatgpt-app", "ChatGPT app", "hosted model interlink and MCP tool caller", "Apps SDK tool-only connector", "hosted model is not local SLM or Sanctuary owner"),
        AgentiCoreDuplexEndpoint("endpoint.phone-seed-node", "Phone seed node", "passive seed target and telemetry pointer", "/sdcard/Download/Sanctuary/seed-node", "seed node is not active CME runtime"),
        AgentiCoreDuplexEndpoint("endpoint.agenticore", EffectiveAgentiCoreId(request), "cOE/cSelfGEL hot-side EC duplex work surface", "AgentiCore chassis slot", "AgentiCore is not canonical SelfGEL mutation")
    };

    private static object AgentiCoreDuplexEndpoint(
        string endpointId,
        string name,
        string role,
        string addressOrSurface,
        string denial) => new
    {
        endpointId,
        name,
        role,
        addressOrSurface,
        denial,
        participatesInDuplex = true,
        ownsSanctuary = false,
        grantsAuthority = false,
        mutatesSelfGel = false,
        activatesActual = false
    };

    private static object[] BuildAgentiCoreDuplexPassagePhases() => new object[]
    {
        AgentiCoreDuplexPassagePhase("phase.01.identity-lock", "select caller CME, thread binding, SoulFrame, and AgentiCore", "identity substitution"),
        AgentiCoreDuplexPassagePhase("phase.02.tool-intent", "receive model or Codex tool intent as data", "prompt becomes authority"),
        AgentiCoreDuplexPassagePhase("phase.03.command-normalization", "map intent to allowlisted Sanctuary command", "unknown tool expansion"),
        AgentiCoreDuplexPassagePhase("phase.04.quoted-lisp-carrier", "carry SLI.Lisp membrane form as quoted data", "Lisp evaluation or code execution"),
        AgentiCoreDuplexPassagePhase("phase.05.receipt-write", "write cold receipt and candidate residue", "admission by write"),
        AgentiCoreDuplexPassagePhase("phase.06.sanitized-return", "return structuredContent plus safe result metadata", "payload, secret, path, or authority leakage")
    };

    private static object AgentiCoreDuplexPassagePhase(string phaseId, string operation, string pressure) => new
    {
        phaseId,
        operation,
        transitionPressure = pressure,
        crypticallyTyped = true,
        primeReviewed = true,
        gateChecked = true,
        opensGateNow = false
    };

    private static object[] BuildAgentiCoreDuplexLispChannels() => new object[]
    {
        AgentiCoreDuplexLispChannel("channel.inbound-tool-form", "inbound MCP tool call normalized to SLI form"),
        AgentiCoreDuplexLispChannel("channel.quoted-carrier", "quoted Lisp membrane carrier, never evaluated as executable authority"),
        AgentiCoreDuplexLispChannel("channel.agenticore-hot-ec", "AgentiCore cOE/cSelfGEL hot-side work posture"),
        AgentiCoreDuplexLispChannel("channel.selfgel-proximal", "CME-specific reconstruction support without SelfGEL mutation"),
        AgentiCoreDuplexLispChannel("channel.sanctuary-gel-candidate", "shared GEL candidate residue without admission"),
        AgentiCoreDuplexLispChannel("channel.outbound-render", "sanitized structuredContent return to ChatGPT or Codex")
    };

    private static object AgentiCoreDuplexLispChannel(string channelId, string function) => new
    {
        channelId,
        function,
        formsAsData = true,
        evaluated = false,
        payloadProtected = true,
        admitsGel = false,
        mutatesSelfGel = false
    };

    private static object[] BuildAgentiCoreDuplexAppIntegrationSurfaces() => new object[]
    {
        AgentiCoreDuplexAppSurface("apps.surface.tool-descriptor", "MCP tools/list descriptor", "inputSchema, outputSchema, annotations, and tool metadata"),
        AgentiCoreDuplexAppSurface("apps.surface.tool-call", "MCP tools/call", "required cmeId and threadBindingId with fail-closed unknown tools"),
        AgentiCoreDuplexAppSurface("apps.surface.structured-content", "structuredContent", "model-visible concise receipt and gate state"),
        AgentiCoreDuplexAppSurface("apps.surface.result-meta", "_meta", "client/widget-oriented status without secret payloads"),
        AgentiCoreDuplexAppSurface("apps.surface.widget-optional", "optional future widget", "read-only receipt/weather console with CSP if added")
    };

    private static object AgentiCoreDuplexAppSurface(string surfaceId, string name, string function) => new
    {
        surfaceId,
        name,
        function,
        chatGptCompatible = true,
        codexCompatible = true,
        toolOnlyAlpha = true,
        widgetRequiredNow = false,
        grantsAuthority = false
    };

    private static object[] BuildAgentiCoreDuplexReturnTelemetrySurfaces() => new object[]
    {
        AgentiCoreReturnTelemetry("return.receipt-handle", "receipt handle", "operator-visible receipt reference"),
        AgentiCoreReturnTelemetry("return.outcome", "outcome/disposition", "command result class"),
        AgentiCoreReturnTelemetry("return.gate-state", "closed-gate booleans", "all crossings false unless reviewed"),
        AgentiCoreReturnTelemetry("return.evidence-digest", "evidence digest", "commitment to richer local evidence"),
        AgentiCoreReturnTelemetry("return.selected-evidence", "selected safe evidence", "small model-visible posture fields"),
        AgentiCoreReturnTelemetry("return.no-local-path", "local path suppression", "receipt body and secret payload remain local")
    };

    private static object AgentiCoreReturnTelemetry(string telemetryId, string name, string function) => new
    {
        telemetryId,
        name,
        function,
        safeForModel = telemetryId is not "return.no-local-path",
        payloadReturned = false,
        localPathReturned = false,
        authorityGranted = false
    };

    private static object[] BuildAgentiCoreDuplexBoundaryDenials() => new object[]
    {
        AgentiCoreDuplexBoundaryDenial("denial.duplex-authority", "duplex passage", "shared authority"),
        AgentiCoreDuplexBoundaryDenial("denial.chatgpt-slm", "ChatGPT hosted model interlink", "local SLM runtime"),
        AgentiCoreDuplexBoundaryDenial("denial.codex-governor", "Codex witness/control surface", "Prime/Cryptic/Steward authority"),
        AgentiCoreDuplexBoundaryDenial("denial.lisp-eval", "SLI.Lisp carrier", "evaluated executable code"),
        AgentiCoreDuplexBoundaryDenial("denial.agenticore-selfgel", "AgentiCore hot-side residue", "canonical SelfGEL mutation"),
        AgentiCoreDuplexBoundaryDenial("denial.return-telemetry-admission", "sanitized return telemetry", "GEL or memory admission"),
        AgentiCoreDuplexBoundaryDenial("denial.phone-runtime", "phone seed node", "active CME runtime"),
        AgentiCoreDuplexBoundaryDenial("denial.widget-permission", "future widget visibility", "permission to act"),
        AgentiCoreDuplexBoundaryDenial("denial.remote-provider", "remote app caller", "provider call or model binding by Sanctuary")
    };

    private static object AgentiCoreDuplexBoundaryDenial(string denialId, string from, string notTo) => new
    {
        denialId,
        from,
        notTo,
        boundaryPreserved = true,
        reviewedPassageRequired = true,
        selfAuthorizing = false
    };

    private static string BuildAgentiCoreDuplexLispMembraneCarrier(
        int endpointCount,
        int passagePhaseCount,
        int lispChannelCount,
        int appIntegrationSurfaceCount,
        int returnTelemetrySurfaceCount,
        int boundaryDenialCount)
    {
        var builder = new StringBuilder();
        builder.AppendLine("(agenticore-duplex-lisp-membrane");
        builder.AppendLine("  :schema \"project-sanctuary.sli.lisp.agenticore-duplex-lisp-membrane.v1\"");
        builder.AppendLine("  :forms-as-data true");
        builder.AppendLine("  :evaluated false");
        builder.AppendLine("  :duplex \"request-and-return-not-shared-authority\"");
        builder.AppendLine("  :codex-extension-compatible true");
        builder.AppendLine("  :chatgpt-app-tool-only-compatible true");
        builder.AppendLine("  :apps-sdk-widget-required false");
        builder.AppendLine("  :shared-mcp-command-membrane true");
        builder.AppendLine("  :chatgpt-provides-hosted-model-interlink true");
        builder.AppendLine("  :chatgpt-provides-local-slm false");
        builder.AppendLine("  :codex-provides-local-witness true");
        builder.AppendLine("  :phone-seed-node-target-only true");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  :endpoint-count {endpointCount}");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  :passage-phase-count {passagePhaseCount}");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  :lisp-channel-count {lispChannelCount}");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  :app-integration-surface-count {appIntegrationSurfaceCount}");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  :return-telemetry-surface-count {returnTelemetrySurfaceCount}");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  :boundary-denial-count {boundaryDenialCount}");
        builder.AppendLine("  (return-telemetry");
        builder.AppendLine("    :structured-content true");
        builder.AppendLine("    :sanitized true");
        builder.AppendLine("    :receipt-body-returned false");
        builder.AppendLine("    :secret-payload-returned false");
        builder.AppendLine("    :local-path-returned false)");
        builder.AppendLine("  (denials");
        builder.AppendLine("    :authority-granted false");
        builder.AppendLine("    :action-authorized false");
        builder.AppendLine("    :gel-admitted false");
        builder.AppendLine("    :selfgel-mutated false");
        builder.AppendLine("    :provider-called false");
        builder.AppendLine("    :model-bound false");
        builder.AppendLine("    :cme-actual-activated false");
        builder.AppendLine("    :sanctuary-actual-activated false))");
        return builder.ToString();
    }
}
