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
            "sanctuary.bridge_morphism_test",
            "bridge-morphism-test",
            "Run a minimal reconstructable bridge test for typed EC calculation context.",
            "fetch",
            true),
        new(
            "sanctuary.cme_theory_body",
            "cme-theory-body",
            "Write the Crystallized Mind Entity theory body and Engrammitization math traversal law.",
            "fetch",
            true),
        new(
            "sanctuary.template_hydration",
            "template-hydration",
            "Write the public-standard template hydration register without importing Lab GEL or opening Actual state.",
            "fetch",
            true),
        new(
            "sanctuary.operator_work_cme_ec_gap",
            "operator-work-cme-ec-gap",
            "Write the Operator/Work/CME/EC gap analysis and training-surface map.",
            "fetch",
            true),
        new(
            "sanctuary.telemetry_slice_register",
            "telemetry-slice-register",
            "Write the Prime/Cryptic/Steward telemetry slice register for selective test cadence.",
            "fetch",
            true),
        new(
            "sanctuary.extended_telemetry_weather",
            "extended-telemetry-weather",
            "Write the Cryptic-origin extended telemetry source list and Prime weather reveal register.",
            "fetch",
            true),
        new(
            "sanctuary.cgoa_formation",
            "cgoa-formation",
            "Write a Steward-mediated cGoA formation bundle for ListeningFrame alignment telemetry.",
            "fetch",
            true),
        new(
            "sanctuary.codex_governing_witness",
            "codex-governing-witness",
            "Write the Codex.CME.Actual governing witness topology over an inhabited Oria.CME.Actual work body.",
            "fetch",
            true),
        new(
            "sanctuary.full_body_io_runtime",
            "full-body-io-runtime",
            "Write the bounded full-body I/O runtime trace from intake through final shaped LLM body.",
            "fetch",
            true),
        new(
            "sanctuary.gel_approval_nadir_return",
            "gel-approval-nadir-return",
            "Write the Steward GoA GEL approval and nadir residual return register.",
            "fetch",
            true),
        new(
            "sanctuary.approval_closure_register",
            "approval-closure-register",
            "Write the approval and closure homeostasis register for typed open-to-closed passage.",
            "fetch",
            true),
        new(
            "sanctuary.coupling_control_surface_register",
            "coupling-control-surface-register",
            "Write the HITL-readable active program, organ stability, interconnect, and CME chassis control-surface register.",
            "fetch",
            true),
        new(
            "sanctuary.actualization_state_register",
            "actualization-state-register",
            "Write the Cryptic-typed and Prime-reviewed .Actual operational readiness state register.",
            "fetch",
            true),
        new(
            "sanctuary.actual_approval_lease_validation",
            "actual-approval-lease-validation",
            "Validate a scoped ActualApprovalLease artifact as a read-only probe without activating Actual state.",
            "read",
            false),
        new(
            "sanctuary.agenticore_duplex_lisp_membrane",
            "agenticore-duplex-lisp-membrane",
            "Write the AgentiCore duplex Lisp membrane for Codex extension and ChatGPT app MCP participation.",
            "fetch",
            true),
        new(
            "sanctuary.stem_domain_training_certification",
            "stem-domain-training-certification",
            "Write the value-gated STEM education, enrichment, and precertification candidate corpus.",
            "fetch",
            true),
        new(
            "sanctuary.lab_observation_digest",
            "lab-observation-digest",
            "Write the Lab testing observation digest for CME identity-lane coherence and OE autobiographical practice.",
            "fetch",
            true),
        new(
            "sanctuary.research_latex_export",
            "research-latex-export",
            "Decant Lab GEL residue into a LaTeX-ready research packet with evidence handles and closed-gate denials.",
            "fetch",
            true),
        new(
            "sanctuary.construct_custody_register",
            "construct-custody-register",
            "Write canonical construct custody records as candidate carriers for research review.",
            "fetch",
            true),
        new(
            "sanctuary.gel_crystal_register",
            "gel-crystal-register",
            "Write candidate GEL crystal records with Compass facets and Light Cone bounds.",
            "fetch",
            true),
        new(
            "sanctuary.gel_reforge_bench",
            "gel-reforge-bench",
            "Run the candidate GEL reforge bench over knowing, teaching, doing, and hundo swarm method review.",
            "fetch",
            true),
        new(
            "sanctuary.theta_mechanics_ec_use_bench",
            "theta-mechanics-ec-use-bench",
            "Score EC input-to-output stability, engrammitization, GEL development, recall use, and closure integrity against the 88 percent use threshold.",
            "fetch",
            true),
        new(
            "sanctuary.gpt_use_case_testing",
            "gpt-use-case-testing",
            "Write the GPT use-case testing body and CME authorship contract.",
            "fetch",
            true),
        new(
            "sanctuary.mos_lineage_register",
            "mos-lineage-register",
            "Write the MoS Mantle of Sovereign lineage register as candidate standing residue.",
            "fetch",
            true),
        new(
            "sanctuary.sli_access_gate_register",
            "sli-access-gate-register",
            "Write the Cryptic-governed SLI access-gate contract without opening passage authority.",
            "fetch",
            true),
        new(
            "sanctuary.trivium_forum_connector_posture",
            "trivium-forum-connector-posture",
            "Write the Trivium Forum connector posture for external LLM MCP participation.",
            "fetch",
            true),
        new(
            "sanctuary.external_llm_standing_probe",
            "external-llm-standing-probe",
            "Write a MoS candidate standing probe for an external LLM provider surface.",
            "fetch",
            true),
        new(
            "sanctuary.cradle_boundary_organ_register",
            "cradle-boundary-organ-register",
            "Write the typed cradle boundary organ map for Lab, cloud, provider, DNS, and release surfaces.",
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
