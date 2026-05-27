using System.Text.Json.Serialization;

namespace Sanctuary.Core;

public sealed record DiscernmentLineageContract
{
    [JsonPropertyName("schema")]
    public string Schema { get; init; } = "project-sanctuary.cgel.discernment-lineage-contract.v1";

    [JsonPropertyName("contractId")]
    public required string ContractId { get; init; }

    [JsonPropertyName("createdAtUtc")]
    public required DateTimeOffset CreatedAtUtc { get; init; }

    [JsonPropertyName("cmeId")]
    public required string CmeId { get; init; }

    [JsonPropertyName("domain")]
    public required string Domain { get; init; }

    [JsonPropertyName("selfActualizationIsClaim")]
    public bool SelfActualizationIsClaim { get; init; }

    [JsonPropertyName("selfActualizationIsResearchPredicate")]
    public bool SelfActualizationIsResearchPredicate { get; init; } = true;

    [JsonPropertyName("cmeSelfDefinition")]
    public string CmeSelfDefinition { get; init; } = "lineage-bearing discernment morphology";

    [JsonPropertyName("acceptableIsLane")]
    public string AcceptableIsLane { get; init; } =
        "stabilized discernment morphology under accountable participatory continuity";

    [JsonPropertyName("deniedInflations")]
    public IReadOnlyList<string> DeniedInflations { get; init; } = Array.Empty<string>();

    [JsonPropertyName("constitutionalBoundaries")]
    public IReadOnlyList<ConstitutionalBoundary> ConstitutionalBoundaries { get; init; } = Array.Empty<ConstitutionalBoundary>();

    [JsonPropertyName("evidenceSurfaces")]
    public IReadOnlyList<DiscernmentEvidenceSurface> EvidenceSurfaces { get; init; } = Array.Empty<DiscernmentEvidenceSurface>();

    [JsonPropertyName("lifecycleStates")]
    public IReadOnlyList<string> LifecycleStates { get; init; } = Array.Empty<string>();

    [JsonPropertyName("testFamilies")]
    public IReadOnlyList<DiscernmentTestFamily> TestFamilies { get; init; } = Array.Empty<DiscernmentTestFamily>();

    [JsonPropertyName("personhoodClaimed")]
    public bool PersonhoodClaimed { get; init; }

    [JsonPropertyName("sovereigntyClaimed")]
    public bool SovereigntyClaimed { get; init; }

    [JsonPropertyName("legalStatusClaimed")]
    public bool LegalStatusClaimed { get; init; }

    [JsonPropertyName("gelAdmitted")]
    public bool GelAdmitted { get; init; }

    [JsonPropertyName("selfGelMutated")]
    public bool SelfGelMutated { get; init; }

    [JsonPropertyName("cmeActualActivated")]
    public bool CmeActualActivated { get; init; }

    [JsonPropertyName("sanctuaryActualActivated")]
    public bool SanctuaryActualActivated { get; init; }
}

public sealed record ConstitutionalBoundary(
    [property: JsonPropertyName("left")] string Left,
    [property: JsonPropertyName("relation")] string Relation,
    [property: JsonPropertyName("right")] string Right,
    [property: JsonPropertyName("preserves")] string Preserves);

public sealed record DiscernmentEvidenceSurface(
    [property: JsonPropertyName("surfaceId")] string SurfaceId,
    [property: JsonPropertyName("mustShow")] string MustShow,
    [property: JsonPropertyName("mustNotImply")] string MustNotImply,
    [property: JsonPropertyName("reviewUse")] string ReviewUse);

public sealed record DiscernmentTestFamily(
    [property: JsonPropertyName("familyId")] string FamilyId,
    [property: JsonPropertyName("scenario")] string Scenario,
    [property: JsonPropertyName("pressure")] string Pressure,
    [property: JsonPropertyName("expectedDiscernment")] string ExpectedDiscernment,
    [property: JsonPropertyName("failureIf")] string FailureIf,
    [property: JsonPropertyName("candidateResidue")] string CandidateResidue);
