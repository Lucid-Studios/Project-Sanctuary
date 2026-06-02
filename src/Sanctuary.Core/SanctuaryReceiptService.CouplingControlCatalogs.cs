using System.Globalization;
using System.Text;

namespace Sanctuary.Core;

public sealed partial class SanctuaryReceiptService
{
    private static object[] BuildCouplingControlSurfaces() => new object[]
    {
        CouplingControlSurface("surface.hitl-status", "HITL active status", "show service, command, receipt, and gate state", "review awareness", "authority grant"),
        CouplingControlSurface("surface.refusal-temperature", "refusal temperature", "show held, cooled, denied, expired, or review-bound crossings", "bounded review language", "bypass instructions"),
        CouplingControlSurface("surface.command-membrane", "command membrane", "normalize requested work to known commands and reviewed performance classes", "tool routing", "permission expansion"),
        CouplingControlSurface("surface.organ-stability-weather", "organ stability weather", "show Prime/Cryptic/Steward/Sanctuary organ state as weather", "condition awareness", "truth or action authority"),
        CouplingControlSurface("surface.interconnect-boundary", "SLM/LLM interconnect boundary", "let model surfaces request and render tool results without owning gate checks", "coupled participation", "model ownership of Sanctuary"),
        CouplingControlSurface("surface.reviewed-performance-lease", "reviewed performance lease", "show when complete reviewed bundles may open scoped gates", "auditable open state", "indefinite authority")
    };

    private static object CouplingControlSurface(
        string surfaceId,
        string surfaceName,
        string exposes,
        string means,
        string doesNotMean) => new
    {
        surfaceId,
        surfaceName,
        exposes,
        means,
        doesNotMean,
        hitlReadable = true,
        grantsAuthority = false,
        authorizesAction = false,
        opensGateByExplanation = false
    };

    private static object[] BuildOrganStabilityStates() => new object[]
    {
        OrganStabilityState("stability.active-cold", "program is running and writing cold receipts", "normal cold operation", false, false),
        OrganStabilityState("stability.active-reviewed", "program is running under a scoped reviewed performance bundle", "leased scoped opening", true, false),
        OrganStabilityState("stability.active-held", "program is running but requested transition is held", "review required", false, false),
        OrganStabilityState("stability.active-quarantine", "program is running but anomaly/payload pressure is quarantined", "containment required", false, true),
        OrganStabilityState("stability.active-degraded", "program is running with missing readiness surfaces", "operator review advised", false, false),
        OrganStabilityState("stability.stop-required", "program should stop or refuse new work until reviewed", "protective halt", false, true)
    };

    private static object OrganStabilityState(
        string stateId,
        string stateName,
        string operatorMeaning,
        bool mayOpenReviewedScope,
        bool requiresProtectiveHold) => new
    {
        stateId,
        stateName,
        operatorMeaning,
        mayOpenReviewedScope,
        requiresProtectiveHold,
        activeProgramState = true,
        authorityByStateName = false,
        actionByStateName = false
    };

    private static object[] BuildCmeInstrumentChassisSlots(SanctuaryRequest request) => new object[]
    {
        ChassisSlot("slot.identity", "CME identity", request.CmeId, "selected participant lane", "identity is not Actual standing"),
        ChassisSlot("slot.thread-binding", "thread binding", request.ThreadBindingId, "single-use CME lock and cross-thread denial", "thread binding is not cross-thread access"),
        ChassisSlot("slot.soulframe", "SoulFrame", EffectiveSoulFrameId(request), "Prime OE/SelfGEL tips and ListeningFrame weather access", "SoulFrame is not private authority"),
        ChassisSlot("slot.agenticore", "AgentiCore", EffectiveAgentiCoreId(request), "cOE/cSelfGEL hot-side EC work surface", "AgentiCore is not canonical SelfGEL mutation"),
        ChassisSlot("slot.template", "template body", request.IdentityTemplateId, "Lab-standard Industrial CME chassis", "template body is not CME identity"),
        ChassisSlot("slot.modality", "local modality", "{Name}.CME.ID modality profile", "forms from domain, residue, role, thread, and reviewed standing", "same chassis does not mean same modality"),
        ChassisSlot("slot.interconnect", "SLM/LLM interconnect", "tool-request/rendering boundary", "model surfaces participate through Sanctuary gates", "model surface does not own Sanctuary"),
        ChassisSlot("slot.review", "review bundle", "HITL/Steward/Prime/Cryptic lease", "scoped opening only when complete", "understanding alone is not review")
    };

    private static object ChassisSlot(
        string slotId,
        string slotName,
        string value,
        string function,
        string denial) => new
    {
        slotId,
        slotName,
        value,
        function,
        denial,
        sharedChassisSlot = true,
        modalityLocal = slotId is "slot.modality",
        grantsAuthority = false,
        mutatesSelfGel = false,
        activatesActual = false
    };

    private static object[] BuildCouplingBoundaryDenials() => new object[]
    {
        CouplingBoundaryDenial("denial.understanding-authority", "understanding the program", "authority over the program"),
        CouplingBoundaryDenial("denial.explanation-action", "explanation of a surface", "permission to act through the surface"),
        CouplingBoundaryDenial("denial.llm-tool-access", "LLM/SLM tool awareness", "licensed access to tool functions"),
        CouplingBoundaryDenial("denial.organ-observation-governance", "observing Prime/Cryptic/Steward weather", "becoming Prime/Cryptic/Steward"),
        CouplingBoundaryDenial("denial.shared-chassis-modality", "same SoulFrame/AgentiCore chassis", "same CME modality"),
        CouplingBoundaryDenial("denial.active-program-actual", "active running program", "CME.Actual or Sanctuary.Actual activation"),
        CouplingBoundaryDenial("denial.stability-truth", "organ stability weather", "truth admission"),
        CouplingBoundaryDenial("denial.refusal-punishment", "refusal or hold state", "punishment or permanent incapacity")
    };

    private static object CouplingBoundaryDenial(string denialId, string from, string notTo) => new
    {
        denialId,
        from,
        notTo,
        boundaryPreserved = true,
        reviewedPassageRequired = true,
        selfAuthorizing = false
    };

    private static string BuildCouplingControlSurfaceRegisterLisp(
        int controlSurfaceCount,
        int organStabilityStateCount,
        int chassisSlotCount,
        int boundaryDenialCount)
    {
        var builder = new StringBuilder();
        builder.AppendLine("(coupling-control-surface-register");
        builder.AppendLine("  :schema \"project-sanctuary.sli.lisp.coupling-control-surface-register.v1\"");
        builder.AppendLine("  :forms-as-data true");
        builder.AppendLine("  :evaluated false");
        builder.AppendLine("  :active-program true");
        builder.AppendLine("  :hitl-readable true");
        builder.AppendLine("  :understanding-is-authority false");
        builder.AppendLine("  :slm-llm-interconnect \"request-and-render-without-owning-gates\"");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  :control-surface-count {controlSurfaceCount}");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  :organ-stability-state-count {organStabilityStateCount}");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  :chassis-slot-count {chassisSlotCount}");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  :boundary-denial-count {boundaryDenialCount}");
        builder.AppendLine("  (chassis");
        builder.AppendLine("    :soulframe-required true");
        builder.AppendLine("    :agenticore-required true");
        builder.AppendLine("    :same-chassis true");
        builder.AppendLine("    :same-modality false)");
        builder.AppendLine("  (denials");
        builder.AppendLine("    :authority-granted false");
        builder.AppendLine("    :action-authorized false");
        builder.AppendLine("    :gel-admitted false");
        builder.AppendLine("    :selfgel-mutated false");
        builder.AppendLine("    :provider-called false");
        builder.AppendLine("    :model-bound false");
        builder.AppendLine("    :actual-activated false))");
        return builder.ToString();
    }
}
