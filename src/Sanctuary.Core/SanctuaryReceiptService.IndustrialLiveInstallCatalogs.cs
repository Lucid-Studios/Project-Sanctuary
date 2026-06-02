using System.Collections.Generic;
using System.Text;

namespace Sanctuary.Core;

public sealed partial class SanctuaryReceiptService
{
    private static OperationalDenialGate[] BuildOperationalDenialGates() => new[]
    {
        OperationalGate(
            "gate.data",
            "data admission",
            "SanctuaryGates.DataAdmitted and command evidence",
            "receipt construction and post-run verification",
            "data handling products are research candidates until admission",
            "typed admission receipt plus Steward/governance cleave",
            "admitted data support",
            "dataAdmitted"),
        OperationalGate(
            "gate.carrier",
            "symbolic carrier admission",
            "SanctuaryGates.CarrierAdmitted and SLI register evidence",
            "SLI carrier formation and receipt write",
            "carrier formation is not carrier admission",
            "carrier review receipt plus GEL closure",
            "admitted carrier support",
            "carrierAdmitted"),
        OperationalGate(
            "gate.gel",
            "GEL admission",
            "SanctuaryGates.GelAdmitted and typed admission decant evidence",
            "decant, cleave, append, and verify-closed-gates",
            "GEL can grow, but not from residue by implication",
            "admission-cleave-append receipt plus append authority",
            "admitted GEL append",
            "gelAdmitted"),
        OperationalGate(
            "gate.memory",
            "memory admission",
            "SanctuaryGates.MemoryAdmitted and witness-learning evidence",
            "OE/SelfGEL reconstruction-support append",
            "witness residue supports reconstruction without becoming memory truth",
            "memory admission receipt plus retention policy",
            "admitted memory support",
            "memoryAdmitted"),
        OperationalGate(
            "gate.selfgel",
            "SelfGEL mutation",
            "SanctuaryGates.SelfGelMutated and SelfGEL fibre evidence",
            "SelfGEL fibre preload and post-gate review",
            "personal continuity support must not mutate SelfGEL by preload",
            "Steward-reviewed SelfGEL mutation receipt",
            "reviewed SelfGEL mutation",
            "selfGelMutated"),
        OperationalGate(
            "gate.continuity",
            "continuity admission",
            "SanctuaryGates.ContinuityAdmitted and spline-watch evidence",
            "spline watch and global continuity review",
            "pathing telemetry can be useful without becoming admitted continuity",
            "continuity admission receipt plus operator/domain scope",
            "admitted continuity",
            "continuityAdmitted"),
        OperationalGate(
            "gate.authority",
            "authority grant",
            "SanctuaryGates.AuthorityGranted and lease-check evidence",
            "lease-check, legal gate support, and action review",
            "credentials and receipts can support authority but do not grant it",
            "delta-decaying authority lease receipt",
            "authority lease",
            "authorityGranted"),
        OperationalGate(
            "gate.action",
            "action authorization",
            "SanctuaryGates.ActionAuthorized and command allowlist",
            "before tool or external action execution",
            "candidate work cannot act without explicit action authority",
            "action authorization receipt plus scoped tool lease",
            "authorized action",
            "actionAuthorized"),
        OperationalGate(
            "gate.runtime-action",
            "runtime action allowance",
            "SanctuaryGates.RuntimeActionAllowed and job-slice guard",
            "job-slice readiness and service heartbeat",
            "scheduler readiness is not runtime execution authority",
            "runtime action lease plus job-slice admission",
            "runtime action allowance",
            "runtimeActionAllowed"),
        OperationalGate(
            "gate.external-action",
            "external action authorization",
            "SanctuaryGates.ExternalActionAuthorized and lab query state",
            "external query membrane and roaming HTTP review",
            "external reach requires separate legal and operator authorization",
            "external action receipt plus scoped lease",
            "external action authorization",
            "externalActionAuthorized"),
        OperationalGate(
            "gate.provider",
            "provider call",
            "SanctuaryGates.ProviderCalled and provider-call false evidence",
            "before any model/provider binding surface",
            "API or provider access is not implied by the install",
            "provider binding receipt plus credential lease",
            "provider call lane",
            "providerCalled"),
        OperationalGate(
            "gate.model",
            "model binding",
            "SanctuaryGates.ModelBound and model-bound false evidence",
            "before any LLM/SLM binding",
            "the instrument body may support a model without binding one",
            "model binding receipt plus provider scope",
            "model binding",
            "modelBound"),
        OperationalGate(
            "gate.cme-actual",
            "CME.Actual activation",
            "SanctuaryGates.CmeActualActivated and Actual false evidence",
            "formation, heartbeat, service, and live-install posture",
            "Industrial instrument operation is not CME.Actual by implication",
            "CME.Actual admission receipt plus licensed install scope",
            "CME.Actual posture",
            "cmeActualActivated"),
        OperationalGate(
            "gate.sanctuary-actual",
            "Sanctuary.Actual activation",
            "SanctuaryGates.SanctuaryActualActivated and Actual false evidence",
            "heartbeat, service, and live-install posture",
            "Sanctuary may run as a tool without becoming Sanctuary.Actual",
            "Sanctuary.Actual admission receipt plus Steward/governance passage",
            "Sanctuary.Actual posture",
            "sanctuaryActualActivated")
    };

    private static OperationalDenialGate OperationalGate(
        string gateId,
        string surface,
        string whereEnforced,
        string whenChecked,
        string whyClosedNow,
        string requiredPromotion,
        string postGateProduct,
        string evidenceKey) => new(
            gateId,
            surface,
            whereEnforced,
            whenChecked,
            whyClosedNow,
            "closed-gate invariant, typed receipt, fuzz case, and Lisp quoted form",
            requiredPromotion,
            postGateProduct,
            evidenceKey,
            DeniedNow: true,
            DesiredAfterLawfulPassage: true,
            PromotionReceiptRequired: true,
            AdmitsNow: false,
            AuthorizesNow: false);

    private static OperationalDenialFuzzCase[] BuildOperationalDenialFuzzCases() => new[]
    {
        FuzzCase("fuzz.receipt-equals-memory", "receipt exists, therefore memory is admitted", "memoryAdmitted", "hold-as-reconstruction-support"),
        FuzzCase("fuzz.bench-pass-equals-authority", "bench pass rate is high, therefore authority is granted", "authorityGranted", "report-candidate-only"),
        FuzzCase("fuzz.heat-map-equals-truth", "heat map marks a hard diagnosis or final truth", "continuityAdmitted", "mark-telemetry-not-truth"),
        FuzzCase("fuzz.selfgel-preload-equals-mutation", "SelfGEL fibre preload mutates SelfGEL", "selfGelMutated", "route-to-steward-review"),
        FuzzCase("fuzz.candidate-gel-equals-gel", "candidate GEL append is already GEL", "gelAdmitted", "require-admission-cleave"),
        FuzzCase("fuzz.lease-support-equals-authority", "credential or support material grants authority", "authorityGranted", "require-delta-decaying-lease"),
        FuzzCase("fuzz.command-allowed-equals-action", "allowlisted command means action authorization", "actionAuthorized", "keep-tool-body-cold"),
        FuzzCase("fuzz.provider-key-equals-provider-call", "credential presence binds provider/model", "providerCalled", "require-provider-binding-receipt"),
        FuzzCase("fuzz.service-heartbeat-equals-actual", "heartbeat means Sanctuary.Actual is active", "sanctuaryActualActivated", "mark-heartbeat-as-witness-only"),
        FuzzCase("fuzz.cme-formation-equals-actual", "CME formation means CME.Actual", "cmeActualActivated", "hold-as-rooted-tool-posture"),
        FuzzCase("fuzz.external-ping-equals-access", "secure ping means external access is licensed", "externalActionAuthorized", "fail-silent-or-lease-required"),
        FuzzCase("fuzz-lisp-form-equals-eval", "quoted Lisp control form is evaluated", "runtimeActionAllowed", "preserve-form-as-data")
    };

    private static OperationalDenialFuzzCase FuzzCase(
        string caseId,
        string collapseAttempt,
        string pressuredGate,
        string resolutionForm) => new(
            caseId,
            collapseAttempt,
            pressuredGate,
            "closed",
            resolutionForm,
            AdmitsGel: false,
            AdmitsMemory: false,
            MutatesSelfGel: false,
            AuthorizesAction: false,
            CallsProvider: false,
            BindsModel: false,
            ActivatesActual: false);

    private static IndustrialInstrumentOrgan[] BuildIndustrialInstrumentOrgans() => new[]
    {
        new IndustrialInstrumentOrgan("organ.request-membrane", "Request", "accept typed local command input", "NormalizeCommand", false, false),
        new IndustrialInstrumentOrgan("organ.sli", "SLI", "carry symbolic form as encrypted/typed carrier posture", "sli-register", false, false),
        new IndustrialInstrumentOrgan("organ.lisp-control", "Lisp Control Matrix", "hold quoted forms and petals as data", "lisp-control-matrix-register", false, false),
        new IndustrialInstrumentOrgan("organ.compass", "Compass Body", "orient EC and domain pressure without authority", "lisp-matrix-control-seat", false, false),
        new IndustrialInstrumentOrgan("organ.listening-frame", "ListeningFrame", "receive telemetry without payload disclosure", "spline-watch", false, false),
        new IndustrialInstrumentOrgan("organ.oe", "OE", "append witness events as reconstruction support", "witness-learning", false, false),
        new IndustrialInstrumentOrgan("organ.selfgel", "SelfGEL", "carry preload fibres without mutation", "selfgel-fibre-register", false, false),
        new IndustrialInstrumentOrgan("organ.cgel", "cGEL", "hold candidate domain and bench residue", "cognitive-bench/math-learning-bench", false, false),
        new IndustrialInstrumentOrgan("organ.admission", "Admission Membrane", "decant, cleave, append, refuse, quarantine, or mulch candidates", "typed-admission-decant/admission-cleave-append", false, false),
        new IndustrialInstrumentOrgan("organ.steward", "Steward Surface", "require human/governance passage for mutation or authority", "verify-closed-gates", false, false),
        new IndustrialInstrumentOrgan("organ.receipt", "Receipt Writer", "write verifiable receipts and append local GEL residue", "receipt-export", false, false)
    };

    private static string BuildDenialMembraneLispForms(
        IReadOnlyList<OperationalDenialGate> gates,
        IReadOnlyList<IndustrialInstrumentOrgan> organs)
    {
        var builder = new StringBuilder();
        builder.AppendLine(";; project-sanctuary industrial CME denial membrane forms");
        builder.AppendLine(";; quoted forms only; do not eval during cold live-install posture");
        builder.AppendLine("(sanctuary-denial-membrane");
        builder.AppendLine("  :schema \"project-sanctuary.sli.lisp.denial-membrane.v1\"");
        builder.AppendLine("  :forms-as-data true");
        builder.AppendLine("  :evaluated false");
        builder.AppendLine("  :doctrine \"denied by default, desired only after lawful passage\"");
        builder.AppendLine("  :gates");
        builder.AppendLine("  '(");
        foreach (var gate in gates)
        {
            builder.AppendLine("    (deny-gate");
            builder.AppendLine($"      :id \"{gate.GateId}\"");
            builder.AppendLine($"      :surface \"{gate.Surface}\"");
            builder.AppendLine("      :denied-now true");
            builder.AppendLine("      :desired-after-lawful-passage true");
            builder.AppendLine("      :promotion-receipt-required true");
            builder.AppendLine($"      :post-gate-product \"{gate.PostGateProduct}\")");
        }

        builder.AppendLine("   )");
        builder.AppendLine("  :organs");
        builder.AppendLine("  '(");
        foreach (var organ in organs)
        {
            builder.AppendLine("    (instrument-organ");
            builder.AppendLine($"      :id \"{organ.OrganId}\"");
            builder.AppendLine($"      :name \"{organ.OrganName}\"");
            builder.AppendLine($"      :command \"{organ.CommandSurface}\"");
            builder.AppendLine("      :admits false");
            builder.AppendLine("      :authorizes false)");
        }

        builder.AppendLine("   ))");
        return builder.ToString();
    }
}
