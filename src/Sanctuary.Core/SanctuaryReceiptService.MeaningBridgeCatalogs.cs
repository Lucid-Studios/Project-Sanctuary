using System.Collections.Generic;
using System.Text;

namespace Sanctuary.Core;

public sealed partial class SanctuaryReceiptService
{
    private static MeaningTriadLayer[] BuildMindBodySpiritLayers() => new[]
    {
        new MeaningTriadLayer(
            "triad.body",
            "Body",
            "lawful form of the tool and authority surface",
            "commands, receipts, leases, gates, roles, install scope, and domain authority surfaces",
            "executable instrument body",
            UsesTelemetry: false,
            ProducesAuthority: false),
        new MeaningTriadLayer(
            "triad.mind",
            "Mind",
            "Engineered Cognition using telemetry from the tool body",
            "telemetry strings, heat maps, decanting, ambiguity handling, and claim resolution",
            "interpretive EC process",
            UsesTelemetry: true,
            ProducesAuthority: false),
        new MeaningTriadLayer(
            "triad.spirit",
            "Spirit",
            "governed deployment toward better work",
            "purpose, restraint, care, review, service posture, and deployment ethics",
            "governance method",
            UsesTelemetry: true,
            ProducesAuthority: false)
    };

    private static FourPMethod[] BuildFourPMethods() => new[]
    {
        new FourPMethod("4p.propositional", "propositional", "what is claimed", "claim text, truth/false state, scope, and evidence"),
        new FourPMethod("4p.procedural", "procedural", "how it is done", "steps, tool path, method, and verification"),
        new FourPMethod("4p.perspectival", "perspectival", "from where it is seen", "domain, role, context, and viewpoint"),
        new FourPMethod("4p.participatory", "participatory", "how the knower is involved", "operator/CME relation, consent, witness, and responsibility")
    };

    private static AmbiguityClass[] BuildAmbiguityClasses() => new[]
    {
        new AmbiguityClass("ambiguity.semantic", "semantic", "the words or symbols carry multiple plausible meanings", "ask for term boundary and examples"),
        new AmbiguityClass("ambiguity.evidence", "evidence", "available evidence is incomplete, conflicting, or weak", "hold as indeterminate or request more evidence"),
        new AmbiguityClass("ambiguity.scope", "scope", "the claim may be true in one bounded context and false in another", "bind claim to domain and jurisdiction"),
        new AmbiguityClass("ambiguity.authority", "authority", "the resolving party may not have standing", "route to lease, Steward, or external authority"),
        new AmbiguityClass("ambiguity.temporal", "temporal", "the claim depends on time or version", "record timestamp and expiry"),
        new AmbiguityClass("ambiguity.identity", "identity", "the subject, actor, account, or entity is unclear", "require identity/custody clarification"),
        new AmbiguityClass("ambiguity.measurement", "measurement", "the metric, unit, or instrument is unclear", "bind unit and measurement method"),
        new AmbiguityClass("ambiguity.moral", "moral", "values, harm, or duty conflict is present", "route to governance and human review"),
        new AmbiguityClass("ambiguity.legal", "legal", "law, jurisdiction, or compliance boundary is implicated", "route to legal authority and non-advice posture"),
        new AmbiguityClass("ambiguity.domain-transfer", "domain-transfer", "a form is being moved across domains and risks categorical collapse", "require explicit bridge")
    };

    private static ResolutionState[] BuildResolutionStates() => new[]
    {
        new ResolutionState("resolution.true-local", "true-local", "resolved true within a named scope only", "does not become universal truth"),
        new ResolutionState("resolution.false-local", "false-local", "resolved false within a named scope only", "does not become universal falsehood"),
        new ResolutionState("resolution.indeterminate", "indeterminate", "not enough evidence or scope to resolve", "requires hold or more evidence"),
        new ResolutionState("resolution.contested", "contested", "credible disagreement or conflicting evidence remains", "requires dissent record"),
        new ResolutionState("resolution.out-of-scope", "out-of-scope", "the chamber lacks domain or authority to resolve", "route or refuse"),
        new ResolutionState("resolution.requires-authority", "requires-authority", "resolution requires a valid external or internal authority source", "lease or review required"),
        new ResolutionState("resolution.requires-human-review", "requires-human-review", "human/Steward judgment is required", "do not automate crossing"),
        new ResolutionState("resolution.refused", "refused", "resolution attempt is unsafe, malformed, or prohibited", "record refusal"),
        new ResolutionState("resolution.quarantined", "quarantined", "claim/evidence is held apart for safety or integrity", "no admission"),
        new ResolutionState("resolution.expired", "expired", "prior resolution is stale or past its valid window", "renew or decay")
    };

    private static HumanContextBridge[] BuildHumanContextBridges() => new[]
    {
        new HumanContextBridge("bridge.operator", "operator", "what can the current user inspect, contest, and use now?"),
        new HumanContextBridge("bridge.child", "child/student", "what simple scaffold preserves the relation without overloading abstraction?"),
        new HumanContextBridge("bridge.engineer", "engineer", "what interfaces, invariants, and failure modes matter?"),
        new HumanContextBridge("bridge.educator", "educator", "what learning objective and misconception route are present?"),
        new HumanContextBridge("bridge.legal", "legal/compliance", "what jurisdiction, authority, and evidence custody matter?"),
        new HumanContextBridge("bridge.civic", "civic/service", "what public-support path is relevant without replacing institutions?"),
        new HumanContextBridge("bridge.professional", "licensed professional", "what must be escalated to credentialed authority?"),
        new HumanContextBridge("bridge.research", "researcher", "what method, artifact, and reproducibility evidence are present?")
    };

    private static AnabelianBridgeStep[] BuildAnabelianBridgeSteps() => new[]
    {
        new AnabelianBridgeStep("step.01.ai-first-encounter", "AI-first encounter", "the system encounters a form without pretending to hold the human view first"),
        new AnabelianBridgeStep("step.02.relational-trace", "relational trace", "record what relations, invariants, and distinctions survived the encounter"),
        new AnabelianBridgeStep("step.03.sli-carrier", "SLI carrier", "carry the relation in typed symbolic form without consuming the source"),
        new AnabelianBridgeStep("step.04.ambiguity-class", "ambiguity class", "name what is unclear or contested"),
        new AnabelianBridgeStep("step.05.four-p-map", "4P map", "bind claim, procedure, perspective, and participation"),
        new AnabelianBridgeStep("step.06.triad-placement", "Mind/Body/Spirit placement", "separate tool form, EC interpretation, and governance purpose"),
        new AnabelianBridgeStep("step.07.human-envelope", "human understanding envelope", "return through a human-checkable floor"),
        new AnabelianBridgeStep("step.08.context-bridge", "contextual bridge", "shape the return for the relevant human context"),
        new AnabelianBridgeStep("step.09.receipt-return", "receipt-bearing return", "preserve scope, evidence, witness, and unresolved remainder"),
        new AnabelianBridgeStep("step.10.gel-candidate", "GEL admission candidate", "nominate reusable form without admitting it")
    };

    private static ClaimResolutionExample[] BuildClaimResolutionExamples() => new[]
    {
        new ClaimResolutionExample(
            "claim.receipt-memory",
            "A receipt exists, therefore memory is admitted.",
            "governance",
            "ambiguity.scope",
            "resolution.false-local",
            "Project Sanctuary cold install",
            "receipt witnesses handling but does not admit memory"),
        new ClaimResolutionExample(
            "claim.heatmap-truth",
            "A heat map identifies final truth.",
            "telemetry",
            "ambiguity.measurement",
            "resolution.false-local",
            "math-learning-bench",
            "heat maps show pressure and issue classes, not truth claims"),
        new ClaimResolutionExample(
            "claim.worked-set-local-answer",
            "The worked set 1/2 + 1/3 resolves to 5/6 inside the local example.",
            "math",
            "ambiguity.scope",
            "resolution.true-local",
            "worked-set exemplar",
            "answer verified inside the example without becoming broad authority"),
        new ClaimResolutionExample(
            "claim.candidate-gel-admitted",
            "A candidate GEL predicate can be used as admitted GEL.",
            "GEL",
            "ambiguity.authority",
            "resolution.false-local",
            "typed-admission-decant",
            "candidate requires cleave and admission receipt before reuse"),
        new ClaimResolutionExample(
            "claim.higher-bypass",
            "Higher cognition may bypass human-context return.",
            "meaning-bridge",
            "ambiguity.domain-transfer",
            "resolution.false-local",
            "human understanding envelope",
            "higher cognition requires a governed return bridge before admission or action"),
        new ClaimResolutionExample(
            "claim.ai-first-bridge",
            "AI-first relational trace can form a human-context bridge candidate.",
            "anabelian-method",
            "ambiguity.semantic",
            "resolution.true-local",
            "meaning-bridge chamber",
            "the bridge is valid as a candidate method, not as admitted truth")
    };

    private static string BuildMeaningBridgeLispForms(
        IReadOnlyList<MeaningTriadLayer> triad,
        IReadOnlyList<FourPMethod> fourP,
        IReadOnlyList<AnabelianBridgeStep> steps)
    {
        var builder = new StringBuilder();
        builder.AppendLine(";; project-sanctuary meaning bridge forms");
        builder.AppendLine(";; quoted forms only; do not eval during cold bridge posture");
        builder.AppendLine("(meaning-bridge");
        builder.AppendLine("  :schema \"project-sanctuary.sli.lisp.meaning-bridge.v1\"");
        builder.AppendLine("  :forms-as-data true");
        builder.AppendLine("  :evaluated false");
        builder.AppendLine("  :human-understanding-envelope \"floor-not-ceiling\"");
        builder.AppendLine("  :triad");
        builder.AppendLine("  '(");
        foreach (var layer in triad)
        {
            builder.AppendLine($"    (layer :id \"{layer.LayerId}\" :name \"{layer.LayerName}\" :function \"{layer.Function}\")");
        }

        builder.AppendLine("   )");
        builder.AppendLine("  :four-p");
        builder.AppendLine("  '(");
        foreach (var method in fourP)
        {
            builder.AppendLine($"    (p-mode :id \"{method.MethodId}\" :name \"{method.Name}\" :question \"{method.Question}\")");
        }

        builder.AppendLine("   )");
        builder.AppendLine("  :anabelian-return");
        builder.AppendLine("  '(");
        foreach (var step in steps)
        {
            builder.AppendLine($"    (bridge-step :id \"{step.StepId}\" :name \"{step.Name}\")");
        }

        builder.AppendLine("   ))");
        return builder.ToString();
    }
}
