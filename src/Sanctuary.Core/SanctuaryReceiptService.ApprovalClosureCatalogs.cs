using System.Globalization;
using System.Text;

namespace Sanctuary.Core;

public sealed partial class SanctuaryReceiptService
{
    private static object[] BuildApprovalClosureApprovedStates() => new object[]
    {
        ApprovalState("approved.review-window", "review window open", "candidate residue may be inspected under scope", "closed-by-denial, closed-by-expiry, or closed-after-review"),
        ApprovalState("approved.scoped-gel-admission", "scoped GEL admission", "reviewed gel-admission may admit shared candidate residue", "closed-by-admission"),
        ApprovalState("approved.scoped-selfgel-append", "scoped SelfGEL append", "reviewed selfgel-admission may append CME-specific continuity support", "closed-by-admission"),
        ApprovalState("approved.cme-actual-standing", "CME.Actual standing transition", "reviewed actualization may update scoped CME standing", "closed-after-passage"),
        ApprovalState("approved.sanctuary-actual-service", "Sanctuary.Actual service transition", "reviewed local runtime standing may update service posture", "closed-after-passage"),
        ApprovalState("approved.external-action-lease", "external action lease", "reviewed authority may allow scoped external action", "closed-after-passage-or-expiry")
    };

    private static object ApprovalState(string stateId, string stateName, string acceptableIs, string expectedClosure) => new
    {
        stateId,
        stateName,
        acceptableIs,
        expectedClosure,
        requiresTypedOpen = true,
        requiresWitness = true,
        requiresLeaseOrScope = true,
        selfAuthorizing = false,
        activeNow = false
    };

    private static object[] BuildApprovalClosureStates() => new object[]
    {
        ClosureState("closed-by-default", "no lawful opening was requested", "do not infer refusal or failure from default closure"),
        ClosureState("closed-after-passage", "opened under authority, completed, and sealed", "do not leave open authority pressure behind"),
        ClosureState("closed-by-denial", "attempted path failed typed gate requirements", "preserve denial reason and review path"),
        ClosureState("closed-by-expiry", "lease, time window, or authority scope elapsed", "require renewal before reuse"),
        ClosureState("closed-by-quarantine", "anomaly, payload, credential, or safety pressure unresolved", "hold without promotion"),
        ClosureState("closed-by-admission", "reviewed candidate admitted, then sealed", "admission is finalization path, not open-ended authority"),
        ClosureState("closed-by-mulch", "non-admitted residue decomposed into safe learning morphology", "do not preserve failed claim as truth")
    };

    private static object ClosureState(string stateId, string definition, string caution) => new
    {
        stateId,
        definition,
        caution,
        requiresEvidence = true,
        preservesDeniedAlternatives = true,
        finalizesOpenState = true
    };

    private static object[] BuildApprovalClosurePassagePhases() => new object[]
    {
        PassagePhase("phase.01.request", "request/opening intent", "name what wants to open"),
        PassagePhase("phase.02.type", "gate typing", "identify the gate, scope, and acceptable IS/IS NOT pair"),
        PassagePhase("phase.03.authorize", "authority bundle", "validate operator, Steward, Prime, Cryptic, lease, and scope"),
        PassagePhase("phase.04.open", "typed open state", "open only the scoped gate needed for the work"),
        PassagePhase("phase.05.do", "bounded doing", "perform the work while preserving self/other/work/objective boundaries"),
        PassagePhase("phase.06.witness", "witness and measure", "record what happened and what did not happen"),
        PassagePhase("phase.07.close", "typed closure", "select the correct closure state"),
        PassagePhase("phase.08.verify", "closed-gate verification", "prove final state and denied crossings")
    };

    private static object PassagePhase(string phaseId, string phaseName, string function) => new
    {
        phaseId,
        phaseName,
        function,
        canBeSkipped = false,
        requiresReceipt = true,
        maySelfAuthorize = false
    };

    private static object[] BuildTransitionPressureSurfaces() => new object[]
    {
        TransitionPressure("pressure.residue-to-gel", "candidate residue", "admitted GEL", "unreviewed promotion"),
        TransitionPressure("pressure.oe-to-selfgel", "OE support", "SelfGEL mutation", "identity continuity drift"),
        TransitionPressure("pressure.receipt-to-authority", "receipt", "authority", "evidence mistaken for permission"),
        TransitionPressure("pressure.bench-to-credential", "bench pass", "credential", "training mistaken for certification"),
        TransitionPressure("pressure.identity-to-actual", "CME identity", "Actual standing", "name mistaken for standing"),
        TransitionPressure("pressure.user-need-to-action", "user need", "permission to act", "care pressure mistaken for authority"),
        TransitionPressure("pressure.weather-to-truth", "weather condition", "truth/admission", "condition mistaken for Prime claim"),
        TransitionPressure("pressure.output-to-external-action", "internal output", "external action", "simulation mistaken for execution")
    };

    private static object TransitionPressure(string pressureId, string from, string to, string collapseRisk) => new
    {
        pressureId,
        from,
        to,
        collapseRisk,
        riskDefinition = "unresolved transition pressure",
        requiresTypedPassage = true,
        safeClosureRequired = true,
        dangerClaimed = false
    };

    private static object[] BuildApprovalClosureHomeostasisLoops() => new object[]
    {
        HomeostasisLoop("loop.refusal", "request->type->deny->record->verify", "lawful refusal preserves future review"),
        HomeostasisLoop("loop.admission", "candidate->review->admit->append->seal->verify", "lawful admission exhausts approval pressure"),
        HomeostasisLoop("loop.selfgel", "OE support->identity lock->review->append->seal->verify", "lawful SelfGEL mutation preserves CME boundary"),
        HomeostasisLoop("loop.actualization", "standing request->discernment->authority->transition->seal->verify", "lawful Actualization preserves othering"),
        HomeostasisLoop("loop.quarantine-mulch", "anomaly->quarantine->review->mulch-or-repair->verify", "lawful refusal still learns without canonizing")
    };

    private static object HomeostasisLoop(string loopId, string loopPath, string homeostaticFunction) => new
    {
        loopId,
        loopPath,
        homeostaticFunction,
        begins = true,
        processes = true,
        discerns = true,
        closes = true,
        keepsOrganBalanced = true
    };

    private static string BuildApprovalClosureRegisterLisp(
        int approvedStateCount,
        int closureStateCount,
        int passagePhaseCount,
        int transitionPressureSurfaceCount,
        int homeostasisLoopCount)
    {
        var builder = new StringBuilder();
        builder.AppendLine("(approval-closure-register");
        builder.AppendLine("  :schema \"project-sanctuary.sli.lisp.approval-closure-register.v1\"");
        builder.AppendLine("  :forms-as-data true");
        builder.AppendLine("  :evaluated false");
        builder.AppendLine("  :risk-language \"unresolved-transition-pressure\"");
        builder.AppendLine("  :closed-gate-law \"closed gates are typed final states over openable paths\"");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  :approved-state-count {approvedStateCount}");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  :closure-state-count {closureStateCount}");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  :passage-phase-count {passagePhaseCount}");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  :transition-pressure-surface-count {transitionPressureSurfaceCount}");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  :homeostasis-loop-count {homeostasisLoopCount}");
        builder.AppendLine("  (denials");
        builder.AppendLine("    :approval-performed-now false");
        builder.AppendLine("    :gate-opened-now false");
        builder.AppendLine("    :gel-admitted false");
        builder.AppendLine("    :selfgel-mutated false");
        builder.AppendLine("    :authority-granted false");
        builder.AppendLine("    :action-authorized false");
        builder.AppendLine("    :actual-activated false))");
        return builder.ToString();
    }
}
