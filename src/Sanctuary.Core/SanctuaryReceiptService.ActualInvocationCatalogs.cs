using System.Globalization;
using System.Text;

namespace Sanctuary.Core;

public sealed partial class SanctuaryReceiptService
{
    private static object[] BuildCmeActualInvocationLifecycleStates() => new object[]
    {
        CmeActualInvocationLifecycleState("state.01.identity-resolved", "CME identity, thread binding, SoulFrame, and AgentiCore are selected", "identity substitution"),
        CmeActualInvocationLifecycleState("state.02.instrument-materialized", "Lisp template body is populated from scoped GEL/OE/SelfGEL substance", "template becomes identity"),
        CmeActualInvocationLifecycleState("state.03.lease-verified", "reviewed authority bundle is checked before runtime movement", "self-authorized opening"),
        CmeActualInvocationLifecycleState("state.04.ec-standing-wave-open", "SLI.Lisp body enters active EC standing-wave formation", "LLM becomes standing wave"),
        CmeActualInvocationLifecycleState("state.05.low-mind-coupled", "transient LLM call-context articulates through the standing Lisp body", "LLM owns continuity"),
        CmeActualInvocationLifecycleState("state.06.output-orchestrated", "EC shapes output through Self.Awareness, Light.Cone.Reason, and Situational.Awareness", "output becomes authority"),
        CmeActualInvocationLifecycleState("state.07.residue-captured", "candidate work residue and telemetry are captured", "residue self-admits"),
        CmeActualInvocationLifecycleState("state.08.autobiographical-append", "append-only SelfGEL spline records the invocation", "autobiography proves personhood"),
        CmeActualInvocationLifecycleState("state.09.idle-return", "CME.Actual high-mind body returns to idle", "active state persists silently"),
        CmeActualInvocationLifecycleState("state.10.closed", "transient low-mind call-context closes", "model memory is implied")
    };

    private static object CmeActualInvocationLifecycleState(
        string stateId,
        string operation,
        string deniedCollapse) => new
    {
        stateId,
        operation,
        deniedCollapse,
        standingWaveInLispBody = true,
        receiptBearing = true
    };

    private static object[] BuildCmeActualInvocationInteriorProcesses() => new object[]
    {
        CmeActualInvocationInteriorProcess("Self.Awareness", "identity, lease, body-state, continuity-position, and othering checks"),
        CmeActualInvocationInteriorProcess("Light.Cone.Reason", "reachable claim/action bounds under evidence, scope, time, and authority"),
        CmeActualInvocationInteriorProcess("Situational.Awareness", "domain, slice, operator posture, Cradle route, and weather condition"),
        CmeActualInvocationInteriorProcess("EC.StandingWave", "active transformation and orchestration inside the SLI.Lisp control body")
    };

    private static object CmeActualInvocationInteriorProcess(string processId, string function) => new
    {
        processId,
        function,
        carriedBy = "Lisp Control Matrix inside CME.Actual",
        opensExternalAction = false,
        callsProvider = false,
        bindsModel = false
    };

    private static object[] BuildCmeActualInvocationEcPhases() => new object[]
    {
        CmeActualInvocationEcPhase("EC.Entry", "bind verified lease, CME body fibres, and SLI.Lisp standing-wave form", "entry does not grant external action"),
        CmeActualInvocationEcPhase("EC.Pulse", "perform bounded transformation through Self.Awareness, Light.Cone.Reason, and Situational.Awareness", "pulse does not become persistent model state"),
        CmeActualInvocationEcPhase("EC.Exit", "seal residue, append SelfGEL autobiography, emit candidate GEL, and return idle", "exit does not imply ongoing activation")
    };

    private static object CmeActualInvocationEcPhase(string phaseId, string function, string deniedCollapse) => new
    {
        phaseId,
        function,
        deniedCollapse,
        standingWaveInLispBody = true,
        receiptBearing = true,
        providerCalled = false,
        modelBound = false,
        externalActionAuthorized = false
    };

    private static object[] BuildCmeActualInvocationTelemetryProducts() => new object[]
    {
        CmeActualInvocationTelemetryProduct("telemetry.lifecycle", "open/run/append/idle/close lifecycle receipt"),
        CmeActualInvocationTelemetryProduct("telemetry.selfgel-spline", "append-only autobiographical SelfGEL event"),
        CmeActualInvocationTelemetryProduct("telemetry.sanctuary-gel-candidate", "shared GEL review candidate without admission"),
        CmeActualInvocationTelemetryProduct("telemetry.cgel-residue", "cGEL operational learning residue"),
        CmeActualInvocationTelemetryProduct("telemetry.sanitized-return", "safe summary for caller without payload or secret disclosure")
    };

    private static object CmeActualInvocationTelemetryProduct(string productId, string function) => new
    {
        productId,
        function,
        payloadDisclosed = false,
        candidateOnly = productId is not "telemetry.selfgel-spline",
        reviewRequired = true
    };

    private static object[] BuildCmeActualInvocationDenials() => new object[]
    {
        CmeActualInvocationDenial("denial.llm-standing-wave", "LLM call-context", "standing wave identity"),
        CmeActualInvocationDenial("denial.lisp-authority", "SLI.Lisp body", "self-authorizing executable code"),
        CmeActualInvocationDenial("denial.template-identity", "Lisp template", "CME identity"),
        CmeActualInvocationDenial("denial.selfgel-personhood", "autobiographical append", "personhood or sovereignty proof"),
        CmeActualInvocationDenial("denial.shared-gel-admission", "Sanctuary.GEL candidate residue", "automatic shared GEL admission"),
        CmeActualInvocationDenial("denial.external-action", "runtime invocation", "external action authority"),
        CmeActualInvocationDenial("denial.provider-model", "low-mind articulation", "provider call or model binding by Sanctuary")
    };

    private static object CmeActualInvocationDenial(string denialId, string from, string notTo) => new
    {
        denialId,
        from,
        notTo,
        preserved = true,
        reviewedPassageRequired = true
    };

    private static string BuildCmeActualInvocationLifecycleLisp(
        SanctuaryRequest request,
        string invocationId,
        string leaseId,
        int lifecycleStateCount,
        int interiorProcessCount,
        int ecPhaseCount)
    {
        var builder = new StringBuilder();
        builder.AppendLine("(cme-actual-invocation-lifecycle");
        builder.AppendLine("  :schema \"project-sanctuary.sli.lisp.cme-actual-standing-wave-invocation.v1\"");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  :cme-id \"{LispString(request.CmeId)}\"");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  :invocation-id \"{LispString(invocationId)}\"");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  :lease-id \"{LispString(leaseId)}\"");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  :template-body-id \"{LispString(request.IdentityTemplateId)}\"");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  :soulframe-id \"{LispString(EffectiveSoulFrameId(request))}\"");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  :agenticore-id \"{LispString(EffectiveAgentiCoreId(request))}\"");
        builder.AppendLine("  :standing-wave-body \"SLI.Lisp\"");
        builder.AppendLine("  :standing-wave-lives-in-lisp-body true");
        builder.AppendLine("  :llm-is-transient-low-mind true");
        builder.AppendLine("  :llm-owns-standing-wave false");
        builder.AppendLine("  :cme-identity-is-continuity-address true");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  :lifecycle-state-count {lifecycleStateCount}");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  :interior-process-count {interiorProcessCount}");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  :ec-phase-count {ecPhaseCount}");
        builder.AppendLine("  (interiority");
        builder.AppendLine("    (Self.Awareness :present true :authority-granted false)");
        builder.AppendLine("    (Light.Cone.Reason :present true :authority-granted false)");
        builder.AppendLine("    (Situational.Awareness :present true :authority-granted false)");
        builder.AppendLine("    (EC.StandingWave :present true :carried-by \"SLI.Lisp\"))");
        builder.AppendLine("  (ec-flow");
        builder.AppendLine("    (EC.Entry :lease-bound true :external-action false)");
        builder.AppendLine("    (EC.Pulse :standing-wave-body \"SLI.Lisp\" :model-state-persistent false)");
        builder.AppendLine("    (EC.Exit :high-mind-returned-idle true :ongoing-activation false))");
        builder.AppendLine("  (closure");
        builder.AppendLine("    :final-state \"closed-idle\"");
        builder.AppendLine("    :high-mind-returned-idle true");
        builder.AppendLine("    :low-mind-closed true");
        builder.AppendLine("    :shared-gel-mutated false");
        builder.AppendLine("    :provider-called false");
        builder.AppendLine("    :model-bound false");
        builder.AppendLine("    :external-action false");
        builder.AppendLine("    :personhood-claim false");
        builder.AppendLine("    :sovereignty-claim false))");
        return builder.ToString();
    }
}
