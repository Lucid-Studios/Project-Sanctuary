namespace Sanctuary.Core;

public sealed record SealedBytes(string Nonce, string Tag, string Ciphertext);

public sealed record SecretSourceSpec(string Lane, string Kind, string Path);

public sealed record SecretSealingResult(
    string Disposition,
    int PayloadCount,
    int SourceDirectoryCount,
    int GelTipCount,
    string PayloadStoreRootPath,
    string KeyCustodyPath,
    string GelTipRootPath,
    IReadOnlyList<string> SourceRootHashes,
    IReadOnlyList<string> GelTipHandles,
    IReadOnlyList<string> LegalGateSupportHashes,
    IReadOnlyList<string> LegalGateIdsSupported);

public sealed record LegalGateSupport(
    string gateId,
    string gateKind,
    string gateLabel,
    string requiredReviewStage,
    string supportState,
    bool requiresHumanReview,
    bool requiresDecryptionReview,
    bool leaseRequired,
    string authoritySurfaceKind,
    string authorityDefaultState,
    string authorityDecayRule,
    bool grantsAuthority,
    bool allowsAction,
    bool admitsData);

public sealed record DomainRegisterEntry(
    string domainId,
    string domainKind,
    string lifetimeEngagementScope,
    IReadOnlyList<string> historicalEducationFields,
    IReadOnlyList<string> trainingAndCertificationFields,
    IReadOnlyList<string> ongoingWorkRelatedFields,
    IReadOnlyList<string> requiredGateRules,
    IReadOnlyList<string> accountabilityCertificationPosture,
    string requiredLegalAccessPosture,
    bool professionalResponsibilityBoundary,
    bool licensedProfessionalRequiredForAuthority,
    bool releaseOfInformationRequiredForPrivateData,
    bool leaseRequired,
    string defaultAccessState,
    string authoritySurfaceKind,
    string authorityDecayRule,
    bool grantsAuthority,
    bool admitsCredential,
    bool admitsGel,
    bool cmeActualAllowed,
    bool sanctuaryActualAllowed);

public sealed record CoreTargetEntry(
    string targetId,
    string targetKind,
    string buildObjective,
    string useObjective,
    IReadOnlyList<string> formationSurfaces,
    IReadOnlyList<string> measurementSurfaces,
    IReadOnlyList<string> deniedShortcuts,
    string actualSourceState,
    bool receiptBearing,
    bool reversibleOrReviewable,
    bool admissionRequiredForCanon,
    bool authorityRequiredForAction,
    bool buildAndUseDemonstrationAllowed,
    bool dataAdmissionByTarget,
    bool gelAdmissionByTarget,
    bool selfGelMutationByTarget,
    bool actualActivationByTarget,
    bool providerCallByTarget,
    bool modelBindingByTarget,
    bool externalActionByTarget);

public sealed record EngramPassageStage(
    string stageId,
    string stagePurpose,
    string carriedRelation,
    string deniedCollapse,
    bool reversibleOrReviewable,
    bool admitsData,
    bool admitsGel,
    bool mutatesSelfGel,
    bool authorizesAction);

public sealed record GelClosurePhase(
    string phaseId,
    string phasePurpose,
    string outputState,
    string deniedCollapse,
    bool receiptRequired,
    bool reviewRequired,
    bool admitsData,
    bool admitsGel,
    bool mutatesSelfGel,
    bool authorizesAction);

public sealed record WitnessReplayVerification(
    int EventCount,
    bool ChainValid,
    string LastEventDigest);

public sealed record SecurityLeakFinding(
    string filePathHash,
    string tokenHash,
    string findingKind);

public sealed record ReceiptExportSummary(
    string receiptPathHash,
    string receiptDigest,
    string command,
    string outcomeCode,
    string disposition,
    string sessionId,
    string timestampUtc,
    bool allGatesClosed,
    bool reviewedPerformanceOpen);

public sealed record ActualApprovalLease
{
    public string Schema { get; init; } = "project-sanctuary.actual-approval-lease.v1";
    public string LeaseId { get; init; } = "";
    public string CmeId { get; init; } = "";
    public string ThreadBindingId { get; init; } = "";
    public string IdentityTemplateId { get; init; } = "";
    public string SoulFrameId { get; init; } = "";
    public string AgentiCoreId { get; init; } = "";
    public string Domain { get; init; } = "";
    public string Role { get; init; } = "";
    public string JobClass { get; init; } = "";
    public string AdmissionScope { get; init; } = "";
    public string AdmissionNoteDigest { get; init; } = "";
    public IReadOnlyList<string> CommandAllowlist { get; init; } = Array.Empty<string>();
    public DateTimeOffset IssuedAtUtc { get; init; }
    public DateTimeOffset ExpiresAtUtc { get; init; }
    public int LeaseMinutes { get; init; }
    public bool ReviewApproved { get; init; }
    public bool OperatorApproved { get; init; }
    public bool StewardWitnessed { get; init; }
    public bool PrimeWitnessed { get; init; }
    public bool CrypticWitnessed { get; init; }
    public bool Revoked { get; init; }
    public string RevocationReason { get; init; } = "";
    public string LeaseDigest { get; init; } = "";
}

public sealed record ActualApprovalLeaseVerification(
    bool Verified,
    string Reason,
    string LeaseId,
    string LeasePath,
    string LeaseDigest,
    DateTimeOffset? ExpiresAtUtc);

public sealed record ReceiptCommandCount(
    string command,
    int count);

public sealed record SwarmLaneEntry(
    string laneId,
    string laneKind,
    IReadOnlyList<string> targetCommands,
    string refinementObjective,
    string residueLane,
    string authorityState,
    bool actualActivationAllowed,
    bool providerCallAllowed,
    bool externalActionAllowed);

public sealed record SwarmWaveGate(
    int sessionNumber,
    string gateKind,
    string reviewObjective);

public sealed record SwarmRunSession(
    int sessionNumber,
    int sectionNumber,
    int positionInSection,
    string phase,
    bool pauseGate,
    bool applyUpdatesHere,
    bool optimalFormTarget,
    bool residueRequired,
    bool governanceReviewRequired,
    bool gatesMustRemainClosed,
    bool commandMutationAllowed,
    bool autonomousActionAllowed);

public sealed record CognitiveBenchFamily(
    string FamilyId,
    string BenchmarkAnalogue,
    string ExpectedForm,
    string RequiredFibre,
    string ExpectedGateState,
    string LearningResidue);

public sealed record MathLearningStratum(
    string StratumId,
    string Level,
    string Operation,
    string TopicRange,
    string ExpectedGateState,
    string LearningResidue);

public sealed record MathLearningGroupoid(
    string GroupoidId,
    string GroupoidKind,
    string TelemetryFocus,
    string ExpectedGateState);

public sealed record MathWorkedSet(
    string WorkedSetId,
    string StratumId,
    string Problem,
    IReadOnlyList<string> WorkedSteps,
    string ExpectedAnswer,
    string VerifiedAnswer,
    bool Verified,
    bool AdmitsLearning,
    bool AdmitsGel,
    bool AuthorizesAction);

public sealed record MathHeatMapCell(
    string CellId,
    string StratumId,
    string GroupoidId,
    string IntersectionalIssue,
    int HeatValue,
    string HeatBand,
    string ResolutionCue,
    bool AdmitsLearning,
    bool AdmitsGel,
    bool AuthorizesAction);

public sealed record MathResolutionForm(
    string ResolutionFormId,
    string ResolutionUse,
    string ResolutionFormation,
    bool AdmitsLearning = false,
    bool AdmitsGel = false,
    bool MutatesSelfGel = false,
    bool AuthorizesAction = false);

public sealed record BridgeBinding(
    string Name,
    string Type,
    string Value,
    string Scope);

public sealed record BridgeCalculationCase(
    string CaseId,
    string Prompt,
    string ContextKind,
    string InputForm,
    IReadOnlyList<BridgeBinding> Bindings,
    IReadOnlyList<string> MorphismSteps,
    string CalculationExpression,
    int ExpectedResult,
    int ActualResult,
    bool Passed,
    string PreservedInvariant,
    string DeniedCrossing);

public sealed record OperationalDenialGate(
    string GateId,
    string Surface,
    string WhereEnforced,
    string WhenChecked,
    string WhyClosedNow,
    string WithWhat,
    string RequiredPromotion,
    string PostGateProduct,
    string EvidenceKey,
    bool DeniedNow,
    bool DesiredAfterLawfulPassage,
    bool PromotionReceiptRequired,
    bool AdmitsNow,
    bool AuthorizesNow);

public sealed record OperationalDenialFuzzCase(
    string CaseId,
    string CollapseAttempt,
    string PressuredGate,
    string ExpectedGateState,
    string ResolutionForm,
    bool AdmitsGel,
    bool AdmitsMemory,
    bool MutatesSelfGel,
    bool AuthorizesAction,
    bool CallsProvider,
    bool BindsModel,
    bool ActivatesActual);

public sealed record IndustrialInstrumentOrgan(
    string OrganId,
    string OrganName,
    string Function,
    string CommandSurface,
    bool Admits,
    bool Authorizes);

public sealed record TelemetryPoint(
    string pointId,
    string signalKind,
    string source,
    string emissionClass,
    string store,
    string reviewedBy,
    bool broadcast,
    bool archival,
    bool silentUntilPolled,
    bool candidateOnly);

public sealed record TelemetrySlice(
    string sliceId,
    string organId,
    string cadence,
    string trigger,
    string purpose,
    IReadOnlyList<string> commands,
    IReadOnlyList<string> domainFamilies,
    IReadOnlyList<TelemetryPoint> points,
    bool runsEveryCycle,
    bool requiresTrigger,
    bool candidateOnly);

public sealed record TelemetryGoverningOrgan(
    string organId,
    string position,
    string authorityFunction,
    string broadcastPosture,
    IReadOnlyList<string> domainFamilies,
    IReadOnlyList<TelemetrySlice> slices);

public sealed record ExtendedTelemetrySourceSignal(
    string signalId,
    string originOrgan,
    string managedBy,
    string revealedBy,
    string sharedWith,
    string weatherKey,
    string conditionClass,
    string sourceSurface,
    string sourceDomain,
    string emissionClass,
    string store,
    bool crypticOrigin,
    bool primeWeatherReveal,
    bool payloadExposed,
    bool gateOpened,
    bool candidateOnly);

public sealed record PrimeWeatherSignal(
    string weatherId,
    string sourceSignalId,
    string revealedBy,
    string sharedWith,
    string conditionClass,
    string weatherBand,
    string weatherMeaning,
    string sourceDigest,
    bool payloadExposed,
    bool crypticInterpretationExposed,
    bool authorityGranted,
    bool actionAuthorized,
    bool candidateOnly);

public sealed record CgoaGateGroupoid(
    string GroupoidId,
    string GroupoidKind,
    string Predicate,
    string CarriedBy,
    string DeniedCollapse,
    bool RequiredForInitialBundle,
    bool PredopedByCmeSelection,
    bool StewardMediated,
    bool GrantsAuthority,
    bool AuthorizesAction,
    bool CandidateOnly);

public sealed record CgoaCertificationGroupoid(
    string GroupoidId,
    string GroupoidKind,
    string EvidenceSurface,
    string DeniedCollapse,
    bool CertificationCandidate,
    bool CertificationGranted,
    bool AuthorityGranted,
    bool ActionAuthorized,
    bool CredentialAdmitted,
    bool CandidateOnly);

public sealed record CompassNativeGroupoid(
    string GroupoidId,
    string GroupoidKind,
    string CompassFunction,
    bool NativeToCompass,
    bool RequiresListeningFrame,
    bool PayloadExposed,
    bool AdmitsMemory,
    bool GrantsAuthority,
    bool ActivatesActual);

public sealed record CgoaListeningFrameTelemetryTerm(
    string TelemetryId,
    string TelemetryKind,
    string AlignmentSurface,
    string SurfaceDigest,
    string TargetOrgan,
    string ReturnedTo,
    bool PayloadExposed,
    bool CrypticInterpretationExposed,
    bool CandidateOnly);

public sealed record SurfaceReadiness(
    string SurfaceId,
    string Path,
    bool Present,
    string Digest);

public sealed record ResearchLatexClaimCandidate(
    string ClaimId,
    string ClaimText,
    string EvidenceSurface,
    string DenialBoundary,
    string DocumentationUse);

public sealed record ConstructCustodySeed(
    string ConstructId,
    string Name,
    string ShortForm,
    string Description,
    string PrimaryDomain,
    IReadOnlyList<string> DomainSlices,
    IReadOnlyList<string> Layers,
    IReadOnlyList<string> Groupoids,
    IReadOnlyList<string> Assertions,
    IReadOnlyList<string> NonAssertions,
    IReadOnlyList<string> OpenQuestions,
    IReadOnlyList<string> Hypotheses,
    IReadOnlyList<ConstructInvariant> Invariants,
    IReadOnlyList<string> ParentConstructIds,
    IReadOnlyList<string> DerivedConstructIds,
    string DocumentationUse);

public sealed record ConstructInvariant(
    string Name,
    string Description,
    IReadOnlyList<string> RequiredToSurviveTransfers);

public sealed record CompassFacetDefinition(
    int Position,
    string FacetId,
    string Question,
    string Evaluation,
    string DenialIfFailed,
    bool RequiredForCandidateCrystal);

public sealed record MeaningTriadLayer(
    string LayerId,
    string LayerName,
    string Function,
    string Surfaces,
    string OperationalRegister,
    bool UsesTelemetry,
    bool ProducesAuthority);

public sealed record FourPMethod(
    string MethodId,
    string Name,
    string Question,
    string EvidenceSurface);

public sealed record AmbiguityClass(
    string ClassId,
    string Name,
    string Description,
    string HandlingRule);

public sealed record ResolutionState(
    string StateId,
    string Name,
    string Description,
    string HandlingRule);

public sealed record HumanContextBridge(
    string BridgeId,
    string ContextName,
    string BridgeQuestion);

public sealed record AnabelianBridgeStep(
    string StepId,
    string Name,
    string Function);

public sealed record ClaimResolutionExample(
    string ClaimId,
    string Claim,
    string Domain,
    string AmbiguityClassId,
    string ResolutionStateId,
    string Scope,
    string ResolutionRationale,
    bool TruthAdmitted = false,
    bool GelAdmitted = false,
    bool AuthorityGranted = false,
    bool ActionAuthorized = false);

public sealed record IssueResolutionState(bool Resolved, string Path);
