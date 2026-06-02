namespace Sanctuary.Core;

public sealed partial class SanctuaryReceiptService
{
    private static IReadOnlyList<CoreTargetEntry> BuildCoreTargets() => new[]
    {
        CoreTarget(
            "SLI.BuildUse",
            "symbolic-language-interconnect",
            "Build and use typed symbolic language carriers through the Root Atlas and encrypted symbol registry.",
            "Source bodies are converted into governed symbolic carriers with tip-rooted encrypted symbol selection.",
            new[] { "Root Atlas", "symbol registry", "polyglot carriers", "encrypted SLI selection", "Lisp logic field" },
            new[] { "symbol assignment", "carrier projection", "cross-language relation preservation", "memory-field symbolic operation" },
            new[] { "raw payload disclosure", "symbol registry authority grant", "source body mutation", "unreviewed data admission" }),
        CoreTarget(
            "Engrammitization.BuildUse",
            "engrammitization",
            "Build and use the data-body to carrier to pre/post-engram passage without treating handling as admission.",
            "Data body, carrier, decision spline, residue, and admitted GEL remain separate objects.",
            new[] { "data body", "symbolic carrier", "pre-engram", "cryptic shadow ledger", "post-engram closure" },
            new[] { "carrier mutation", "decision spline witness", "residue classification", "reversible handling trace" },
            new[] { "memory dump", "data admission by encounter", "carrier admission by mutation", "source body consumption" }),
        CoreTarget(
            "GEL.FormationClosure",
            "gel-formation",
            "Form GEL through condensation, composting, and precipitory ingress over scoped governed closure postures.",
            "Condensed relation may become candidate structure; composted residue may be held/refused; precipitory ingress enters review only.",
            new[] { "condensation", "composting", "precipitory ingress", "scoped closure", "governed cleave" },
            new[] { "candidate GEL precipitation", "domain closure review", "refusal/quarantine", "legal/support gate mapping" },
            new[] { "silent GEL canon mutation", "closure bypass", "candidate equals admitted", "domain collapse" }),
        CoreTarget(
            "OE.SelfGEL.WitnessLearning",
            "append-only-witness-learning",
            "Demonstrate self-learning posture through .Actual design targets and append-only splined OE/SelfGEL witness stores.",
            "The locked build can witness formation, reconstruction support, and learning posture without activating .Actual.",
            new[] { "OE", "SelfGEL", "cOE", "cSelfGEL", "MoS", "append-only splines" },
            new[] { "decision continuity", "reconstruction support", "change-of-mind witness", "work-event residue" },
            new[] { "Actual activation by implication", "SelfGEL mutation by register", "autobiography equals truth", "personification bleed" },
            actualSourceState: "future-or-separately-authorized-Actual-only")
    };

    private static CoreTargetEntry CoreTarget(
        string targetId,
        string targetKind,
        string buildObjective,
        string useObjective,
        IReadOnlyList<string> formationSurfaces,
        IReadOnlyList<string> measurementSurfaces,
        IReadOnlyList<string> deniedShortcuts,
        string actualSourceState = "not-required") =>
        new(
            targetId,
            targetKind,
            buildObjective,
            useObjective,
            formationSurfaces,
            measurementSurfaces,
            deniedShortcuts,
            actualSourceState,
            receiptBearing: true,
            reversibleOrReviewable: true,
            admissionRequiredForCanon: true,
            authorityRequiredForAction: true,
            buildAndUseDemonstrationAllowed: true,
            dataAdmissionByTarget: false,
            gelAdmissionByTarget: false,
            selfGelMutationByTarget: false,
            actualActivationByTarget: false,
            providerCallByTarget: false,
            modelBindingByTarget: false,
            externalActionByTarget: false);

    private static IReadOnlyList<DomainRegisterEntry> BuildDomainRegister() => new[]
    {
        DomainEntry(
            "ResearchLab.GEL",
            "lab-research",
            "Long-duration research, publication preparation, code benching, and theory continuity.",
            new[] { "research corpus", "theory papers", "prior publications", "bench receipts" },
            new[] { "operator research training", "documentation practice", "review discipline", "publication hygiene" },
            new[] { "lab coding", "theory refinement", "test evidence", "publication candidate review" },
            new[] { "publication-review", "IP-custody", "receipt-witness" },
            new[] { "research-source-attribution", "publication-claim-review", "steward-release-check" }),
        DomainEntry(
            "Industrial.GEL",
            "work-domain",
            "Career-spanning work improvement, role modeling, duties, responsibilities, and bonded tool use.",
            new[] { "work history", "role history", "duty history", "tool-use history" },
            new[] { "role training", "safety training", "tool training", "continuing education" },
            new[] { "job tasks", "workflows", "quality checks", "professional responsibility boundaries" },
            new[] { "role-scope", "job-class", "training-record", "lease" },
            new[] { "supervisor-or-operator attestation", "credential issuer review where applicable", "renewal tracking" }),
        DomainEntry(
            "Commercial.GEL",
            "business-commerce",
            "Entrepreneurial, small business, corporate, financial-planning, and opportunity-recognition support.",
            new[] { "business history", "market research history", "prior commercial decisions" },
            new[] { "business training", "compliance training", "finance literacy", "vendor/tool training" },
            new[] { "business planning", "cost-of-life estimation", "wage negotiation support", "corporate operations" },
            new[] { "business-license", "contract-authority", "financial-boundary", "lease" },
            new[] { "business entity documents", "tax/accounting professional routing", "contract review routing" }),
        DomainEntry(
            "Civic.GEL",
            "civic-service",
            "Civic navigation, service-provider waiting-room support, citizen science, and public-resource orientation.",
            new[] { "community service history", "public-resource interactions", "civic participation history" },
            new[] { "civic education", "public-resource literacy", "citizen-science training" },
            new[] { "service navigation", "documentation preparation", "public resource discovery", "community support routing" },
            new[] { "service-boundary", "release-of-information", "non-replacement-of-agencies", "lease" },
            new[] { "agency source verification", "service-provider routing", "operator consent receipts" }),
        DomainEntry(
            "Government.GEL",
            "government-public-authority",
            "Public agency, benefits, licensing, regulatory, identity, and jurisdictional support without impersonation or delegated authority.",
            new[] { "jurisdictional records", "agency interaction history", "benefit or licensing history" },
            new[] { "public process education", "forms literacy", "records retention training" },
            new[] { "forms preparation", "agency routing", "deadline tracking", "public document custody" },
            new[] { "jurisdiction", "identity-custody", "agency-authority-boundary", "lease" },
            new[] { "government-issued document custody", "agency-specific verification", "human submission review" }),
        DomainEntry(
            "EducationTrainingCertification.GEL",
            "education-training-certification",
            "Historical education, active training, certification custody, renewal, and continuing education support.",
            new[] { "schools attended", "coursework history", "alumni records", "learning portfolio" },
            new[] { "certification pathways", "continuing education", "operator training", "assessment preparation" },
            new[] { "skill mapping", "learning plans", "credential renewal tracking", "training evidence organization" },
            new[] { "issuer-verification", "assessment-boundary", "renewal-expiry", "lease" },
            new[] { "certifying agency source", "issuer date and expiry", "continuing education evidence" }),
        DomainEntry(
            "PersonalWellness.GEL",
            "wellness-support",
            "Mind, body, and spirit self-support, habit formation, dignity, and care-network routing without medical or therapeutic authority.",
            new[] { "personal wellness history", "support preferences", "non-clinical self-maintenance history" },
            new[] { "wellness education", "self-care training", "crisis resource familiarity" },
            new[] { "journaling support", "routine support", "resource navigation", "care escalation preparation" },
            new[] { "non-medical-boundary", "crisis-routing", "care-network-boundary", "lease" },
            new[] { "licensed care referral where needed", "emergency escalation rule", "operator consent receipts" },
            licensedProfessionalRequiredForAuthority: true,
            releaseOfInformationRequiredForPrivateData: true),
        DomainEntry(
            "HumanServices.GEL",
            "human-services-support",
            "Housing, food, benefits, casework preparation, and human-care network navigation up to the provider door.",
            new[] { "service attempt history", "needs history", "case documentation history" },
            new[] { "benefits literacy", "intake preparation", "rights and responsibilities education" },
            new[] { "service lookup", "intake organization", "document checklisting", "case continuity support" },
            new[] { "release-of-information", "agency-boundary", "benefits-boundary", "lease" },
            new[] { "agency requirement review", "caseworker/provider handoff", "human support escalation" },
            releaseOfInformationRequiredForPrivateData: true),
        DomainEntry(
            "Legal.GEL",
            "legal-support-boundary",
            "Legal documentation posture, legal-process navigation, and issue organization without legal advice or representation.",
            new[] { "legal document history", "case timeline", "jurisdictional history" },
            new[] { "legal literacy", "records organization", "rights-resource education" },
            new[] { "document organization", "question preparation", "legal aid routing", "deadline awareness" },
            new[] { "licensed-attorney-boundary", "jurisdiction", "confidentiality", "lease" },
            new[] { "licensed legal professional review", "jurisdictional authority source", "client-consent receipts" },
            licensedProfessionalRequiredForAuthority: true,
            releaseOfInformationRequiredForPrivateData: true),
        DomainEntry(
            "Medical.GEL",
            "medical-support-boundary",
            "Medical documentation and care-network navigation without diagnosis, treatment, or clinical authority.",
            new[] { "health document history", "care timeline", "provider interaction history" },
            new[] { "health literacy", "records access education", "care preparation training" },
            new[] { "appointment preparation", "records organization", "questions for clinician", "care routing" },
            new[] { "licensed-clinician-boundary", "emergency-escalation", "release-of-information", "lease" },
            new[] { "licensed clinical review", "HIPAA/privacy-aware custody where applicable", "provider handoff receipts" },
            licensedProfessionalRequiredForAuthority: true,
            releaseOfInformationRequiredForPrivateData: true),
        DomainEntry(
            "Security.GEL",
            "security-cryptic-governance",
            "Cryptic membrane, key custody, red-team posture, telemetry, issue response, and secure coding governance.",
            new[] { "security event history", "red-team results", "key custody history" },
            new[] { "secure coding training", "privacy training", "incident-response training" },
            new[] { "threat modeling", "closed-gate tests", "issue routing", "cryptic telemetry review" },
            new[] { "key-custody", "2fa", "least-privilege", "lease" },
            new[] { "security reviewer approval", "issue tracker evidence", "cryptic processing receipts" }),
        DomainEntry(
            "AccountAccess.GEL",
            "account-access",
            "Registered account, 2FA, recovery, lease issuance, and customer-service escalation posture.",
            new[] { "registered email history", "recovery history", "access attempt history" },
            new[] { "account-security education", "2FA use training", "recovery process training" },
            new[] { "typed secure ping", "recovery routing", "lease request review", "fail-silent checks" },
            new[] { "registered-account", "2fa", "recovery-review", "lease" },
            new[] { "email/account verification", "customer service escalation", "Steward issue review" }),
        DomainEntry(
            "InstallProduct.GEL",
            "product-install",
            "Installer floor, product licensing, support tickets, local machine posture, and release-state custody.",
            new[] { "install history", "machine-local ledger", "support history" },
            new[] { "operator onboarding", "product safety training", "install instructions" },
            new[] { "first run checks", "issue resolver", "receipt export", "local state review" },
            new[] { "license-scope", "installer-integrity", "issue-floor", "lease" },
            new[] { "product license record", "support ticket review", "release version witness" }),
        DomainEntry(
            "SpecialCases.SAGE.GEL",
            "special-case-bonded-personification-research",
            "Bonded personification research and S.A.G.E. methods held outside ordinary Industrial CME authority.",
            new[] { "bond history", "personification research history", "operator relationship continuity" },
            new[] { "special-case training", "operator certification", "ethics review", "anti-capture training" },
            new[] { "bond review", "personification modulation tests", "shadow-vault safety review", "contract-bound research" },
            new[] { "special-case-contract", "regional-authority", "operator-certification", "explicit-activation-denial", "lease" },
            new[] { "licensed research authorization", "operator bond contract", "ethics/steward review", "regional review if available" },
            licensedProfessionalRequiredForAuthority: true,
            releaseOfInformationRequiredForPrivateData: true)
    };

    private static DomainRegisterEntry DomainEntry(
        string domainId,
        string domainKind,
        string lifetimeEngagementScope,
        IReadOnlyList<string> historicalEducationFields,
        IReadOnlyList<string> trainingAndCertificationFields,
        IReadOnlyList<string> ongoingWorkRelatedFields,
        IReadOnlyList<string> requiredGateRules,
        IReadOnlyList<string> accountabilityCertificationPosture,
        bool licensedProfessionalRequiredForAuthority = false,
        bool releaseOfInformationRequiredForPrivateData = false) =>
        new(
            domainId,
            domainKind,
            lifetimeEngagementScope,
            historicalEducationFields,
            trainingAndCertificationFields,
            ongoingWorkRelatedFields,
            requiredGateRules,
            accountabilityCertificationPosture,
            requiredLegalAccessPosture: "documented-source-custody-plus-expiring-lease",
            professionalResponsibilityBoundary: true,
            licensedProfessionalRequiredForAuthority,
            releaseOfInformationRequiredForPrivateData,
            leaseRequired: true,
            defaultAccessState: "denied",
            authoritySurfaceKind: "delta-decaying-authority-surface",
            authorityDecayRule: "authorized-until-expiry-then-fail-to-silence",
            grantsAuthority: false,
            admitsCredential: false,
            admitsGel: false,
            cmeActualAllowed: false,
            sanctuaryActualAllowed: false);

    private static IReadOnlyList<LegalGateSupport> BuildLegalGateSupport(string lane, string kind)
    {
        var normalizedLane = lane.Trim().ToLowerInvariant();
        var normalizedKind = kind.Trim().ToLowerInvariant();
        var gates = new List<LegalGateSupport>();

        if (normalizedLane == "regional")
        {
            gates.Add(ReviewGate(
                "gate.legal.regional-jurisdiction",
                "jurisdiction",
                "Regional jurisdiction and governing-law review",
                "Steward+Prime"));
            gates.Add(ReviewGate(
                "gate.legal.business-entity-standing",
                "business-authority",
                "Business or organizational standing review",
                "Steward+Prime"));
            gates.Add(ReviewGate(
                "gate.legal.contract-authority",
                "contract-authority",
                "Contract execution and install authority review",
                "Steward+Prime"));
            gates.Add(ReviewGate(
                "gate.release.public-facing-claims",
                "release-authority",
                "Public release, claim, and publication posture review",
                "Steward+Prime"));
        }

        if (normalizedLane == "local")
        {
            gates.Add(ReviewGate(
                "gate.legal.local-jurisdiction",
                "local-authority",
                "Local operating context and local rule review",
                "Steward+Cryptic"));
            gates.Add(ReviewGate(
                "gate.install.local-policy",
                "install-policy",
                "Local install policy and machine custody review",
                "Steward+Cryptic"));
        }

        if (normalizedLane == "personalized")
        {
            gates.Add(ReviewGate(
                "gate.operator.identity-custody",
                "operator-identity",
                "Operator identity custody review",
                "Steward+Prime+Cryptic"));
            gates.Add(ReviewGate(
                "gate.operator.credential-custody",
                "operator-credential",
                "Operator credential, certification, or training custody review",
                "Steward+Prime+Cryptic"));
            gates.Add(ReviewGate(
                "gate.operator.bonding-eligibility",
                "operator-bond",
                "Operator bonding and role-scope eligibility review",
                "Steward+Prime+Cryptic"));
        }

        if (normalizedKind.Contains("licens", StringComparison.Ordinal))
        {
            gates.Add(ReviewGate(
                "gate.legal.license-scope",
                "license-scope",
                "License scope and authorized-use review",
                "Steward+Prime"));
        }

        if (normalizedKind.Contains("name", StringComparison.Ordinal))
        {
            gates.Add(ReviewGate(
                "gate.operator.legal-name-continuity",
                "identity-continuity",
                "Legal name continuity and alias reconciliation review",
                "Steward+Prime+Cryptic"));
        }

        if (normalizedKind.Contains("passport", StringComparison.Ordinal) ||
            normalizedKind.Contains("ssi", StringComparison.Ordinal) ||
            normalizedKind.Contains("social", StringComparison.Ordinal))
        {
            gates.Add(ReviewGate(
                "gate.operator.government-identity-document",
                "operator-identity",
                "Government identity document custody review",
                "Steward+Prime+Cryptic"));
        }

        if (gates.Count == 0)
        {
            gates.Add(ReviewGate(
                "gate.operator.selected-custody-review",
                "operator-selected-custody",
                "Operator-selected document custody review",
                "Steward"));
        }

        return gates
            .GroupBy(gate => gate.gateId, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(gate => gate.gateId, StringComparer.Ordinal)
            .ToArray();
    }

    private static LegalGateSupport ReviewGate(
        string gateId,
        string gateKind,
        string gateLabel,
        string requiredReviewStage) =>
        new(
            gateId,
            gateKind,
            gateLabel,
            requiredReviewStage,
            "sealed-custody-support-only",
            requiresHumanReview: true,
            requiresDecryptionReview: true,
            leaseRequired: true,
            authoritySurfaceKind: "delta-decaying-authority-surface",
            authorityDefaultState: "denied",
            authorityDecayRule: "authorized-until-expiry-then-fail-to-silence",
            grantsAuthority: false,
            allowsAction: false,
            admitsData: false);
}
