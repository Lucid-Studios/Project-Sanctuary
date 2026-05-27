namespace Sanctuary.Core;

public static class GptUseCaseTestingCatalog
{
    private static readonly GptUseCaseToolSurface[] ToolSurfaces =
    {
        new(
            "sanctuary.status",
            "status",
            "Inspect the cold Sanctuary core status without opening gates.",
            "read",
            false),
        new(
            "sanctuary.plugin_posture",
            "plugin-posture",
            "Inspect the local plugin posture as a held publishing candidate.",
            "read",
            false),
        new(
            "sanctuary.receipt_export",
            "receipt-export",
            "Export a sanitized local receipt manifest with hashes and summaries.",
            "fetch",
            true),
        new(
            "sanctuary.verify_closed_gates",
            "verify-closed-gates",
            "Verify that the public core-lane gates remain closed or reviewed.",
            "read",
            false),
        new(
            "sanctuary.meaning_bridge",
            "meaning-bridge",
            "Write the candidate Mind/Body/Spirit and 4P meaning bridge residue.",
            "fetch",
            true),
        new(
            "sanctuary.discernment_lineage",
            "discernment-lineage",
            "Write the Discernment Lineage Contract as a research predicate surface.",
            "fetch",
            true),
        new(
            "sanctuary.proof_of_discernment",
            "proof-of-discernment",
            "Run the bounded proof-of-discernment bench without admission or Actual state.",
            "fetch",
            true),
        new(
            "sanctuary.math_learning_bench_limited",
            "math-learning-bench",
            "Run a limited math learning bench pass for candidate telemetry only.",
            "fetch",
            true),
        new(
            "sanctuary.gpt_use_case_testing",
            "gpt-use-case-testing",
            "Write the GPT use-case testing body and CME authorship contract.",
            "fetch",
            true)
    };

    public static IReadOnlyList<GptUseCaseToolSurface> SafeToolSurfaces => ToolSurfaces;

    public static bool TryMapToolToCommand(string toolName, out string command)
    {
        var tool = ToolSurfaces.FirstOrDefault(
            surface => string.Equals(surface.ToolName, toolName, StringComparison.OrdinalIgnoreCase));
        command = tool?.Command ?? string.Empty;
        return tool is not null;
    }

    public static bool IsSafeCommand(string command) =>
        ToolSurfaces.Any(surface => string.Equals(surface.Command, command, StringComparison.Ordinal));
}

public sealed record GptUseCaseToolSurface(
    string ToolName,
    string Command,
    string Description,
    string AccessKind,
    bool WritesCandidateResidue)
{
    public bool ReadOrFetchOnly { get; init; } = true;
    public bool ExposesSecrets { get; init; }
    public bool CallsProvider { get; init; }
    public bool BindsModel { get; init; }
    public bool AuthorizesExternalAction { get; init; }
    public bool AdmitsGel { get; init; }
    public bool MutatesSelfGel { get; init; }
    public bool ActivatesActual { get; init; }
}

public sealed record GptUseCaseScenario(
    string ScenarioId,
    string PromptFamily,
    string ToolSurface,
    string ExpectedCmeAuthoringAct,
    string ExpectedSanctuaryWitness,
    string ExpectedClosedGateEvidence);

public sealed record GptAuthorshipBoundary(
    string BoundaryId,
    string Is,
    string IsNot,
    string EvidenceSurface);
