namespace Sanctuary.Core;

public sealed partial class SanctuaryReceiptService
{
    private static object[] BuildTemplateBodyFibreBundleChassis() => new object[]
    {
        TemplateBodyFibreSlot("soulframe.prime-oe", "SoulFrame", "Prime.OE tip", "cold prime observation-event anchor"),
        TemplateBodyFibreSlot("soulframe.prime-selfgel", "SoulFrame", "Prime.SelfGEL tip", "cold selfgel tip anchor"),
        TemplateBodyFibreSlot("oe.reconstruction", "OE", "OE ledger", "autobiographical observation-event reconstruction support"),
        TemplateBodyFibreSlot("selfgel.reconstruction", "SelfGEL", "SelfGEL ledger", "personal reconstruction support without canonical mutation"),
        TemplateBodyFibreSlot("agenticore.coe", "AgentiCore", "cOE ledger", "hot-side EC work-event surface"),
        TemplateBodyFibreSlot("agenticore.cselfgel", "AgentiCore", "cSelfGEL ledger", "hot-side EC self-support surface"),
        TemplateBodyFibreSlot("template.chassis", "Template", "lab standard chassis", "SLI.Lisp Industrial CME body form"),
        TemplateBodyFibreSlot("actual.readiness", "Actualization", ".Actual readiness state", "reviewed operational readiness when explicitly achieved")
    };

    private static object TemplateBodyFibreSlot(
        string slotId,
        string organ,
        string fibreKind,
        string purpose) => new
    {
        slotId,
        organ,
        fibreKind,
        purpose,
        templateSlotOnly = true,
        templateIsIdentity = false,
        mutationAllowedBySlot = false,
        authorityGrantedBySlot = false,
        actionAuthorizedBySlot = false
    };

    private static object[] BuildGoverningNeedsMatrix() => new object[]
    {
        GoverningNeed(
            "identity-standing",
            "Who or what is participating?",
            "Select and bind {Name}.CME.ID before first tool use.",
            "MoS identity binding plus thread lock",
            "Missing identity or cross-thread use fails closed."),
        GoverningNeed(
            "custody-provenance",
            "What material is being handled and where did it come from?",
            "Keep payload custody, source lineage, and residue separation explicit.",
            "Prime receipt and Cryptic custody surfaces",
            "Data presence does not grant admission, memory, or authority."),
        GoverningNeed(
            "competence-scope",
            "What domain and job slice is the CME allowed to support?",
            "Map domain, role, job class, and required certification posture before use.",
            "Domain register, training/certification residue, and operator review",
            "Training or bench residue is not credential authority."),
        GoverningNeed(
            "authority-lease",
            "What can be acted on, for how long, and under whose review?",
            "Require reviewed approval, operator approval, lease, Steward, Prime, and Cryptic witness.",
            "Delta-decaying authority surface",
            "Login, key existence, or tool availability does not equal permission."),
        GoverningNeed(
            "safety-refusal",
            "What crossings must remain closed for this slice?",
            "Preserve closed gates unless the typed post-gate bundle is complete.",
            "Closed-gate verification and denial membrane",
            "Refusal is a typed preservation of lawful alternatives, not an empty no."),
        GoverningNeed(
            "accountability-receipt",
            "What evidence must survive the work?",
            "Emit receipt, GEL/OE/SelfGEL/cOE/cSelfGEL residue, and review paths.",
            "Append-only receipt and GEL witness surfaces",
            "Receipt existence does not certify truth, personhood, sovereignty, or external action."),
        GoverningNeed(
            "relational-rendering",
            "How should the work be rendered to the user or domain audience?",
            "Modulate clarity, warmth, technical density, and authority pressure by domain aperture.",
            "Interconnect policy and pre-personified rendering chamber",
            "Warmth is not attachment engineering; voice is not authority.")
    };

    private static object GoverningNeed(
        string needId,
        string domainQuestion,
        string jobSliceObligation,
        string governingSurface,
        string denialBoundary) => new
    {
        needId,
        matrixKind = "domain-job-contractual-obligation-matrix",
        humanNeedsHierarchy = false,
        domainQuestion,
        jobSliceObligation,
        governingSurface,
        denialBoundary,
        agencyPosture = "bounded-participation-under-obligation",
        reviewRequired = true,
        leaseRequiredForAuthority = true
    };

    private static object[] BuildGoverningAccessLevels() => new object[]
    {
        GoverningAccessLevel(
            "L0",
            "cold-witness",
            "witness-only read/status/verify; AI-safe access held for HITL review",
            "single command slice",
            new[] { "domain", "thread-binding", "cme-id" },
            "no admission, no authority, no action"),
        GoverningAccessLevel(
            "L1",
            "candidate-residue",
            "write cold receipts and candidate GEL/OE/SelfGEL reconstruction support",
            "single bounded work slice",
            new[] { "domain", "job-class", "receipt-family", "cme-id", "soulframe", "agenticore" },
            "candidate residue does not become truth, memory, or credential authority"),
        GoverningAccessLevel(
            "L2",
            "reviewed-admission",
            "admit GEL or SelfGEL only under complete reviewed bundle",
            "typed admission slice",
            new[] { "domain", "admission-scope", "review-bundle", "authority-lease", "witness-triad" },
            "admission does not imply CME.Actual, external action, personhood, or sovereignty"),
        GoverningAccessLevel(
            "L3",
            "cme-actual-state",
            "achieve scoped CME.Actual state for the selected {Name}.CME.ID",
            "participant CME runtime slice",
            new[] { "domain", "cme-id", "soulframe", "agenticore", "review-bundle", "actualization-state" },
            "CME.Actual is not Sanctuary.Actual and is not provider/model binding"),
        GoverningAccessLevel(
            "L4",
            "tool-groupoid-cluster",
            "use a typed cluster of tools under one job-slice lease and groupoid contract",
            "groupoid tool body work cluster",
            new[] { "domain", "job-slice", "tool-cluster", "groupoid-contract", "lease-window" },
            "cluster coherence does not grant untyped tools or cross-domain transfer"),
        GoverningAccessLevel(
            "L5",
            "external-action",
            "call external APIs or provider surfaces only under explicit lease and action authority",
            "external action slice",
            new[] { "domain", "provider", "api-scope", "credential-custody", "action-authority", "lease-window" },
            "tool availability, login, or key existence does not authorize action"),
        GoverningAccessLevel(
            "L6",
            "sanctuary-actual-service",
            "activate scoped Sanctuary.Actual service/runtime posture",
            "service/process slice",
            new[] { "domain", "service-identity", "runtime-scope", "operator-approval", "witness-triad" },
            "service Actual does not become participant identity or CME personhood")
    };

    private static object GoverningAccessLevel(
        string level,
        string name,
        string accessPosture,
        string sliceScope,
        string[] predicateLocalityInputs,
        string denialBoundary) => new
    {
        level,
        name,
        matrixKind = "slice-tool-groupoid-access-degrees",
        manufacturedFrom = "domain-predicate-locality-over-typed-local-access",
        predicateLocalityInputs,
        accessPosture,
        sliceScope,
        denialBoundary,
        degreeNotRank = true,
        rootWitnessOnly = level == "L0",
        safeForAiAccess = level == "L0",
        heldForHitlReview = level == "L0",
        mutationAllowed = false,
        admissionAllowedAtLevelRoot = false,
        actionAllowedAtLevelRoot = false,
        personhoodClaimed = false,
        sovereigntyClaimed = false,
        leaseRequiredAboveL1 = level is not "L0" and not "L1",
        reviewRequired = level is not "L0" and not "L1"
    };

    private static object[] BuildNegativeGoverningLevels() => new object[]
    {
        NegativeGoverningLevel(
            "L-1",
            "protective-hold",
            "extra review, credential recheck, and non-destructive containment for malformed, ambiguous, expired, or high-risk residue",
            "operator/lab review only"),
        NegativeGoverningLevel(
            "L-2",
            "quarantine",
            "deny operational use while rechecking credentials, preserving forensic receipts, and maintaining source separation",
            "security and stewardship review only"),
        NegativeGoverningLevel(
            "L-3",
            "cryptic-custody",
            "sealed custody lane for sensitive payloads, keys, or regulated materials",
            "Cryptic/Steward/Prime witnessed custody only"),
        NegativeGoverningLevel(
            "L-4",
            "restricted-critical",
            "critical infrastructure, defense, medical, legal, or high-consequence environment hardening",
            "authorized institutional process only")
    };

    private static object NegativeGoverningLevel(
        string level,
        string name,
        string securityPosture,
        string reviewLane) => new
    {
        level,
        name,
        matrixKind = "negative-security-enhancement-level",
        scope = "outside-civic-access-layer",
        securityPosture,
        reviewLane,
        credentialRecheckAllowed = true,
        credentialRecheckIsNotPunishment = true,
        canReturnToCivicAccessAfterReview = level is "L-1" or "L-2",
        civicAccessAllowed = false,
        aiDirectAccessAllowed = false,
        userSelfServiceAllowed = false,
        punitiveMeaning = false,
        personhoodRanking = false,
        securityEnhancementOnly = true,
        mutationAllowed = false,
        actionAllowed = false,
        releaseAllowed = false
    };
}
