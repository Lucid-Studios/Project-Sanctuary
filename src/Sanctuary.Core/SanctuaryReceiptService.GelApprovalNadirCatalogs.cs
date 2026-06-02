using System;
using System.Text;

namespace Sanctuary.Core;

public sealed partial class SanctuaryReceiptService
{
    private static object[] BuildGelApprovalMethods() => new object[]
    {
        GelApprovalMethod("method.01.observe", "observe", "collect work facts, receipts, and telemetry counts without treating them as admitted truth", "residue candidate"),
        GelApprovalMethod("method.02.proximate-selfgel", "spline-proximal SelfGEL predication", "route near-self continuity support to SelfGEL prediction/support lanes without mutation", "SelfGEL support candidate"),
        GelApprovalMethod("method.03.outlier-precipitation", "outlier precipitation", "route cross-CME outliers toward Sanctuary.GEL candidate review when they carry shared-pattern value", "Sanctuary.GEL candidate"),
        GelApprovalMethod("method.04.cryptic-strip", "cryptic stripping", "remove payload, identity leakage, secrets, and unsafe authority pressure before review", "non-disclosing residue"),
        GelApprovalMethod("method.05.steward-cleave", "Steward GoA cleave", "classify admit, append, hold, refuse, quarantine, or mulch without performing admission", "cleave decision candidate"),
        GelApprovalMethod("method.06.prime-witness", "Prime witness", "archive the reviewed approval path and preserve denied crossings", "approval witness candidate"),
        GelApprovalMethod("method.07.reviewed-admission", "reviewed admission", "only the reviewed gel-admission command may open scoped Sanctuary.GEL gates", "post-review admission path")
    };

    private static object GelApprovalMethod(string methodId, string name, string function, string outputClass) => new
    {
        methodId,
        name,
        function,
        outputClass,
        residueSelfAuthoringAllowed = false,
        stewardGoaRequired = methodId is not "method.01.observe",
        reviewedAdmissionRequiredForGel = methodId is "method.07.reviewed-admission",
        admitsGelNow = false,
        mutatesSelfGelNow = false,
        grantsAuthorityNow = false
    };

    private static object[] BuildNadirResidualReturnStages() => new object[]
    {
        NadirStage("nadir.01.work-return", "work completes and returns residue to OE/ListeningFrame", "completion posture", "receipt handle and local telemetry"),
        NadirStage("nadir.02.depersonalize", "strip self-authoring pressure and identity inflation", "raw residue", "non-self-authoring residue"),
        NadirStage("nadir.03.proximity-test", "test whether residue is spline-proximal to the originating CME", "non-self-authoring residue", "SelfGEL predication candidate or outlier"),
        NadirStage("nadir.04.outlier-test", "test whether residue is valuable across individuated CMEs", "outlier residue", "Sanctuary.GEL precipitation candidate"),
        NadirStage("nadir.05.cryptic-safety", "screen for secret, payload, credential, or domain-collapse risk", "candidate residue", "safe review body or quarantine"),
        NadirStage("nadir.06.steward-goa-cleave", "Steward in GoA cleaves candidate class and review path", "safe review body", "admit/append/hold/refuse/quarantine/mulch candidate"),
        NadirStage("nadir.07.prime-receipt", "Prime witnesses the route and preserves receipt lineage", "Steward cleave candidate", "archival witness"),
        NadirStage("nadir.08.reviewed-command", "reviewed admission command may perform scoped post-gate admission later", "witnessed approval candidate", "gel-admission or selfgel-admission")
    };

    private static object NadirStage(string stageId, string stage, string input, string output) => new
    {
        stageId,
        stage,
        input,
        output,
        nadirReturn = true,
        selfAuthoringAllowed = false,
        candidateOnly = true,
        admitsNow = false
    };

    private static object[] BuildNadirResidueClasses() => new object[]
    {
        ResidueClass("residue.self-proximal", "spline-proximal SelfGEL support", "supports future autobiographical reconstruction for the same CME", "SelfGEL prediction/support", false, false),
        ResidueClass("residue.outlier-shared", "precipitous outlier", "may reveal shared pattern across individuated CMEs", "Sanctuary.GEL candidate", false, false),
        ResidueClass("residue.shared-invariant", "shared invariant", "repeatedly stable relation across domains, tests, or CMEs", "Sanctuary.GEL candidate", false, false),
        ResidueClass("residue.malformed", "malformed residue", "noise, weak bridge, duplicate, or contradiction without stable value", "mulch", false, false),
        ResidueClass("residue.risk-secret", "risk or secret pressure", "payload, legal, credential, or safety pressure that cannot enter common review", "quarantine", false, false),
        ResidueClass("residue.actualization-pressure", "Actualization pressure", "language or behavior tries to promote identity, memory, or authority", "hold/refuse pending review", false, false)
    };

    private static object ResidueClass(
        string classId,
        string name,
        string description,
        string returnLane,
        bool admitsSanctuaryGelNow,
        bool mutatesSelfGelNow) => new
    {
        classId,
        name,
        description,
        returnLane,
        admitsSanctuaryGelNow,
        mutatesSelfGelNow,
        nonSelfAuthoring = true,
        stewardGoaCleaveRequired = true,
        primeWitnessRequired = returnLane.Contains("Sanctuary.GEL", StringComparison.Ordinal),
        crypticScreenRequired = true
    };

    private static object[] BuildStewardGoaControls() => new object[]
    {
        GoaControl("goa.control.01.scope", "scope check", "bind residue to domain, CME.ID, thread, and authority scope"),
        GoaControl("goa.control.02.source", "source check", "confirm receipt lineage and non-payload source posture"),
        GoaControl("goa.control.03.proximity", "proximity check", "separate SelfGEL-proximal support from shared outlier precipitation"),
        GoaControl("goa.control.04.review", "review burden", "decide whether Prime/Cryptic/operator review is required before admission"),
        GoaControl("goa.control.05.command", "command routing", "route approved cases to reviewed gel-admission or selfgel-admission only"),
        GoaControl("goa.control.06.rejection", "rejection route", "hold, refuse, quarantine, or mulch without deleting receipt history")
    };

    private static object GoaControl(string controlId, string controlName, string function) => new
    {
        controlId,
        controlName,
        function,
        stewardOwned = true,
        residueMayBypass = false,
        opensGateNow = false
    };

    private static object[] BuildIndividuatedCmeResidueFlow() => new object[]
    {
        new { stepId = "flow.01", from = "{Name}.CME.ID", to = "OE/ListeningFrame", relation = "work residue returns as candidate-only telemetry" },
        new { stepId = "flow.02", from = "OE/ListeningFrame", to = "{Name}.CME.ID.SelfGEL support", relation = "spline-proximal residue predicates reconstruction support without mutation" },
        new { stepId = "flow.03", from = "OE/ListeningFrame", to = "Sanctuary.GEL candidate", relation = "outlier/shared residue precipitates toward common review" },
        new { stepId = "flow.04", from = "Sanctuary.GEL candidate", to = "Steward+GoA", relation = "approval method is cleaved and routed" },
        new { stepId = "flow.05", from = "Steward+GoA", to = "reviewed gel-admission", relation = "only reviewed authority can perform scoped admission later" }
    };

    private static string BuildGelApprovalNadirReturnLisp()
    {
        var builder = new StringBuilder();
        builder.AppendLine("(gel-approval-nadir-return");
        builder.AppendLine("  :schema \"project-sanctuary.sli.lisp.gel-approval-nadir-return.v1\"");
        builder.AppendLine("  :controller \"Steward+GoA\"");
        builder.AppendLine("  :approval-law \"GEL approval is reviewed cleave, not residue self-authorship\"");
        builder.AppendLine("  :nadir-return true");
        builder.AppendLine("  :non-self-authoring-residue true");
        builder.AppendLine("  (routes");
        builder.AppendLine("    (selfgel-proximal :use \"predication/support\" :mutates-selfgel-now false)");
        builder.AppendLine("    (outlier-precipitous :use \"Sanctuary.GEL candidate\" :admits-gel-now false)");
        builder.AppendLine("    (shared-invariant :use \"Sanctuary.GEL candidate\" :requires-reviewed-gel-admission true)");
        builder.AppendLine("    (malformed :use \"mulch\" :truth-admitted false)");
        builder.AppendLine("    (risk-secret :use \"quarantine\" :payload-disclosed false))");
        builder.AppendLine("  (required-witnesses \"Steward\" \"Prime\" \"Cryptic\" \"Operator\")");
        builder.AppendLine("  (denials");
        builder.AppendLine("    :approval-performed-now false");
        builder.AppendLine("    :nadir-return-performed-now false");
        builder.AppendLine("    :data-admitted false");
        builder.AppendLine("    :gel-admitted false");
        builder.AppendLine("    :memory-admitted false");
        builder.AppendLine("    :selfgel-mutated false");
        builder.AppendLine("    :authority-granted false");
        builder.AppendLine("    :action-authorized false");
        builder.AppendLine("    :provider-called false");
        builder.AppendLine("    :model-bound false");
        builder.AppendLine("    :actual-activated false))");
        return builder.ToString();
    }
}
