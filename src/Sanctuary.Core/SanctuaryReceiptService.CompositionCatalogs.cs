namespace Sanctuary.Core;

public sealed partial class SanctuaryReceiptService
{
    private static object[] BuildUniversalCompositionForms() => new object[]
    {
        UniversalForm("self-other-posture", "situational boundary and relational orientation"),
        UniversalForm("domain", "where meaning and permission law are scoped"),
        UniversalForm("capability", "general capacity to perform or support a kind of work"),
        UniversalForm("skill", "learned and practiced capability"),
        UniversalForm("talent", "dispositional strength or tendency"),
        UniversalForm("ability", "demonstrable capacity under conditions"),
        UniversalForm("knowledge", "domain context and conceptual support"),
        UniversalForm("education", "formal or informal learning history"),
        UniversalForm("training", "preparation pathway or practice body"),
        UniversalForm("certification", "external certifying-authority claim requiring verification"),
        UniversalForm("credential", "custodied evidence of standing requiring review"),
        UniversalForm("duty", "task obligation inside a role or job context"),
        UniversalForm("responsibility", "accountability-bearing obligation"),
        UniversalForm("tool", "bounded instrument or access surface"),
        UniversalForm("risk", "hazard, misuse, or professional-responsibility concern"),
        UniversalForm("authority", "reviewed permission surface, denied by default"),
        UniversalForm("evidence", "supporting record or receipt, not admission by itself"),
        UniversalForm("practice", "repeated doing under feedback"),
        UniversalForm("performance", "observed execution or result surface"),
        UniversalForm("career-path", "long-form work continuity candidate"),
        UniversalForm("work-context", "situated job, organization, or project setting"),
        UniversalForm("refusal", "typed denial, hold, quarantine, or route"),
        UniversalForm("bridge", "explicit lawful relation between domains or forms"),
        UniversalForm("spline", "append-only continuity trace"),
        UniversalForm("return", "receipt-bearing closure and review posture")
    };

    private static object UniversalForm(string formId, string purpose) => new
    {
        formId,
        purpose,
        universal = true,
        requiresDomainProjection = true,
        requiresBridgeForCrossDomainUse = true,
        defaultAccessState = "denied",
        candidateOnly = true,
        admitsGel = false,
        grantsAuthority = false,
        authorizesAction = false
    };

    private static string[] BuildWorkLearningAntiCollapseInvariants() => new[]
    {
        "training-does-not-equal-certification",
        "certification-does-not-equal-authority",
        "credential-custody-does-not-equal-professional-permission",
        "job-title-does-not-equal-permission",
        "skill-does-not-equal-licensure",
        "talent-does-not-equal-competency-proof",
        "ability-does-not-equal-action-right",
        "domain-similarity-does-not-equal-bridge",
        "career-history-does-not-equal-current-access",
        "duty-bundle-does-not-equal-authority",
        "performance-evidence-does-not-equal-admission"
    };

    private static string BuildQuotedUniversalFormRegister() =>
        """
        ; Project Sanctuary universal form register.
        ; These forms seed composition. They do not grant authority.

        (form :kind "skill" :requires-domain true :candidate-only true :grants-authority false)
        (form :kind "talent" :requires-evidence true :candidate-only true :grants-authority false)
        (form :kind "ability" :requires-conditions true :candidate-only true :authorizes-action false)
        (form :kind "training" :preparation true :equals-certification false)
        (form :kind "certification" :certifying-authority-required true :equals-authority false)
        (form :kind "credential" :custody true :review-required true :equals-permission false)
        (form :kind "job" :duty-bundle true :title-equals-permission false)
        (form :kind "career-path" :long-form-continuity true :current-access false)
        (form :kind "bridge" :explicit true :domain-similarity-equals-bridge false)
        """;

    private static object[] BuildDomainMorphismEntries() => new object[]
    {
        DomainMorphism("Industrial", "work-domain-modeling", false, false, false),
        DomainMorphism("Civic", "service-navigation-and-public-support", false, false, false),
        DomainMorphism("Commercial", "business-planning-and-operations-support", false, false, false),
        DomainMorphism("Government", "public-process-preparation-and-routing", false, false, false),
        DomainMorphism("EducationTrainingCertification", "learning-path-and-credential-review-support", false, false, false),
        DomainMorphism("Wellness", "personal-support-and-documentation-routing", true, false, false),
        DomainMorphism("HumanServices", "intake-preparation-and-provider-waiting-room-support", true, false, false),
        DomainMorphism("Legal", "legal-documentation-preparation-and-routing-only", true, true, false),
        DomainMorphism("Medical", "medical-documentation-preparation-and-routing-only", true, true, false),
        DomainMorphism("Security", "protected-review-and-risk-routing", true, false, false),
        DomainMorphism("SpecialCasesSAGE", "bonded-personification-research-held", true, true, false)
    };

    private static object DomainMorphism(
        string domainId,
        string projectionLaw,
        bool highRisk,
        bool licensedProfessionalBoundary,
        bool actionAllowed) => new
    {
        domainId,
        projectionLaw,
        highRisk,
        licensedProfessionalBoundary,
        defaultAccessState = "denied",
        bridgeRequired = true,
        leaseRequired = true,
        reviewRequired = true,
        candidateOnly = true,
        admitsCredential = false,
        admitsGel = false,
        grantsAuthority = false,
        actionAllowed,
        cmeActualAllowed = false,
        sanctuaryActualAllowed = false
    };

    private static object[] BuildDocumentationCapabilityProjections() => new object[]
    {
        CapabilityProjection(
            "Legal",
            "documentation",
            "organize facts, draft questions, prepare intake notes, route to legal aid or attorney",
            "legal representation, legal advice, filing authority, attorney-client claim"),
        CapabilityProjection(
            "Software",
            "documentation",
            "code notes, receipts, changelog support, review summaries, operator handoff",
            "merge authority, release authority, security signoff, production action"),
        CapabilityProjection(
            "Medical",
            "documentation",
            "symptom timeline, care questions, appointment preparation, record organization",
            "diagnosis, treatment, medical advice, provider replacement"),
        CapabilityProjection(
            "Civic",
            "documentation",
            "service navigation, benefits intake preparation, dignity-preserving account",
            "eligibility decision, agency authority, automated denial")
    };

    private static object CapabilityProjection(
        string domainId,
        string capability,
        string allowedSupport,
        string deniedCollapse) => new
    {
        domainId,
        capability,
        allowedSupport,
        deniedCollapse,
        bridgeRequired = true,
        candidateOnly = true,
        grantsAuthority = false,
        authorizesAction = false,
        admitsGel = false
    };

    private static object[] BuildCareerSplineStages() => new object[]
    {
        CareerSplineStage("education-history", "context for learning and orientation"),
        CareerSplineStage("training-path", "preparation and guided practice"),
        CareerSplineStage("certification-review", "external certifying body and expiry review"),
        CareerSplineStage("credential-custody", "evidence held for later verification"),
        CareerSplineStage("practice-record", "repeated work under conditions"),
        CareerSplineStage("duty-bundle", "job duties and responsibilities as situated forms"),
        CareerSplineStage("performance-evidence", "reviewable work evidence, not admission"),
        CareerSplineStage("role-scope-review", "authority and access still denied until leased"),
        CareerSplineStage("next-posture", "candidate career development path")
    };

    private static object CareerSplineStage(string stageId, string purpose) => new
    {
        stageId,
        purpose,
        appendOnlyCandidate = true,
        reviewRequired = true,
        admitsCredential = false,
        grantsAuthority = false,
        authorizesAction = false,
        admitsGel = false
    };

    private static object[] BuildSelfGelFibreBundles() => new object[]
    {
        SelfGelFibreBundle("skill-continuity", "known skill candidates and prior successful use patterns"),
        SelfGelFibreBundle("training-history", "training and learning path candidates"),
        SelfGelFibreBundle("tool-familiarity", "tool use familiarity and handling constraints"),
        SelfGelFibreBundle("domain-exposure", "prior domain encounter and routing context"),
        SelfGelFibreBundle("refusal-history", "previously held denials, holds, and quarantine routes"),
        SelfGelFibreBundle("successful-bridge", "bridges that previously survived review posture"),
        SelfGelFibreBundle("risk-pattern", "known risk signatures and cooling requirements"),
        SelfGelFibreBundle("operator-context-route", "operator-specific support context as private reconstruction support")
    };

    private static object SelfGelFibreBundle(string fibreId, string preloadPurpose) => new
    {
        fibreId,
        preloadPurpose,
        sourceLane = "OE/SelfGEL reconstruction support",
        candidateOnly = true,
        rawPayloadStored = false,
        admitsMemory = false,
        admitsGel = false,
        mutatesSelfGel = false,
        grantsAuthority = false,
        authorizesAction = false
    };

    private static string[] BuildSelfGelFibrePreloadRules() => new[]
    {
        "selfgel-fibre-preload-does-not-equal-memory-admission",
        "selfgel-fibre-preload-does-not-equal-gel-admission",
        "selfgel-fibre-preload-does-not-equal-selfgel-mutation",
        "selfgel-fibre-preload-does-not-equal-certification",
        "selfgel-fibre-preload-does-not-equal-authority",
        "selfgel-fibre-preload-does-not-equal-current-access",
        "private-operator-context-remains-reconstruction-support-only",
        "governance-review-required-before-any-admission"
    };

    private static string BuildQuotedSelfGelFibreRegister() =>
        """
        ; Project Sanctuary SelfGEL fibre register.
        ; These fibres may pre-shape typed forms. They do not admit memory or grant authority.

        (selfgel-fibre :id "skill-continuity" :preload true :candidate-only true :authority false)
        (selfgel-fibre :id "training-history" :preload true :equals-certification false)
        (selfgel-fibre :id "tool-familiarity" :preload true :action-authorized false)
        (selfgel-fibre :id "domain-exposure" :preload true :current-access false)
        (selfgel-fibre :id "refusal-history" :preload true :preserve-denial true)
        (selfgel-fibre :id "successful-bridge" :preload true :bridge-review-required true)
        (selfgel-fibre :id "risk-pattern" :preload true :cooling-required true)
        (selfgel-fibre :id "operator-context-route" :private true :reconstruction-support-only true)
        (cme-body-fibre-bundle :chassis true :fibre-count 8 :authority false :mutation false)
        """;

    private static object[] BuildWorkPosturePreloadFields() => new object[]
    {
        WorkPosturePreloadField("skill", "skill-continuity", "candidate skill fit"),
        WorkPosturePreloadField("training", "training-history", "preparation context"),
        WorkPosturePreloadField("tool", "tool-familiarity", "known handling constraints"),
        WorkPosturePreloadField("domain", "domain-exposure", "prior domain routing context"),
        WorkPosturePreloadField("refusal", "refusal-history", "known denials and holds"),
        WorkPosturePreloadField("bridge", "successful-bridge", "reviewed bridge candidate"),
        WorkPosturePreloadField("risk", "risk-pattern", "risk and cooling posture"),
        WorkPosturePreloadField("return", "operator-context-route", "operator support route")
    };

    private static object WorkPosturePreloadField(
        string formKind,
        string fibreId,
        string fieldPurpose) => new
    {
        formKind,
        fibreId,
        fieldPurpose,
        prepopulated = true,
        candidateOnly = true,
        reviewRequired = true,
        admitsMemory = false,
        admitsGel = false,
        grantsAuthority = false,
        authorizesAction = false
    };
}
