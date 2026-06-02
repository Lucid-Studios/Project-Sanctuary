namespace Sanctuary.Core;

public sealed partial class SanctuaryReceiptService
{
    private static IReadOnlyList<SwarmLaneEntry> BuildSwarmLanes() => new[]
    {
        SwarmLane(
            "SLI",
            "symbolic-language-interconnect",
            new[] { "sli-register", "sli-tip-form", "sli-carrier-probe" },
            "Root Atlas, encrypted symbol selection, carrier formation, and memory-field symbolic operation."),
        SwarmLane(
            "Engrammitization",
            "engram-passage",
            new[] { "engram-passage", "pre-engram", "post-engram-closure" },
            "Data body, carrier, spline, residue, and closure stay distinct."),
        SwarmLane(
            "GEL",
            "formation-closure",
            new[] { "gel-closure", "lab-gel-crystallization-phases", "typed-admission-decant", "admission-cleave-append", "spline-watch" },
            "Candidate GEL formation through condensation, composting, governed ingress, dual Sanctuary.GEL/OE-SelfGEL residue split, and cleave readiness."),
        SwarmLane(
            "MatrixDomain",
            "universal-domain-composition",
            new[] { "universal-form-register", "domain-morphism-register", "capability-composition-probe", "career-spline-probe", "selfgel-fibre-register", "work-posture-preload-probe", "cognitive-bench", "typed-admission-decant", "admission-cleave-append", "spline-watch" },
            "Universal form atoms, domain morphisms, SelfGEL fibre preloads, capability composition, cognitive bench residue, typed admission decants, cleave/append rules, spline watches, and career spline candidates."),
        SwarmLane(
            "OE-SelfGEL",
            "append-only-witness-learning",
            new[] { "witness-learning", "selfgel-fibre-register", "lab-gel-crystallization-phases", "spline-watch" },
            "Decision splines, changes of mind, life-review-style reconstruction support, and self/other separation without truth admission."),
        SwarmLane(
            "Governance",
            "closed-gate-accountability",
            new[] { "domain-register", "core-targets", "verify-closed-gates" },
            "Receipt witness, pause gates, issue-floor, and authority denial posture."),
        SwarmLane(
            "Security",
            "cryptic-membrane-hardening",
            new[] { "red-team", "secret-leak-check", "authority-bypass-check" },
            "Cryptic membrane, sealed payloads, fail-silent access, and no disclosure."),
        SwarmLane(
            "Service",
            "local-service-readiness",
            new[] { "heartbeat", "last-run-pointer", "receipt-export", "lisp-control-matrix-register", "resonance-chamber-probe" },
            "Local service cadence, restart adjacency, and tool-body telemetry."),
        SwarmLane(
            "Product",
            "install-release-posture",
            new[] { "first-run", "operator-prompts", "support-floor" },
            "Install path clarity, locked Industrial state, support routing, and public-safe posture.")
    };

    private static object BuildSwarmCrystallizationPosture() => new
    {
        postureId = "hundo-lab-gel-crystallization-posture",
        command = "lab-gel-crystallization-phases",
        phaseBodyRequiredBeforeTesting = true,
        sanctuaryGelResidueRequired = true,
        selfGelReconstructionResidueRequired = true,
        selfOtherCollapseDenied = true,
        lifeReviewStyleStudyAllowed = true,
        lifeReviewStyleStudyAdmitsMemory = false,
        phaseReadinessPerformsCleave = false,
        phaseReadinessActivatesActual = false,
        testingWithoutPhaseBodyAllowed = false
    };

    private static object[] BuildHundoSwarmExecutionOrder() => new object[]
    {
        HundoSwarmExecutionStep(
            1,
            "frame",
            "write or refresh the Hundo register and governance residue",
            "swarm-refinement"),
        HundoSwarmExecutionStep(
            2,
            "phase",
            "write Lab GEL crystallization phases and dual residue lanes",
            "lab-gel-crystallization-phases"),
        HundoSwarmExecutionStep(
            3,
            "bridge",
            "refresh meaning bridge before admission review",
            "meaning-bridge"),
        HundoSwarmExecutionStep(
            4,
            "decant",
            "prepare typed admission candidates without admission",
            "typed-admission-decant"),
        HundoSwarmExecutionStep(
            5,
            "cleave-model",
            "model admit/append/hold/refuse/quarantine/mulch without performing them",
            "admission-cleave-append"),
        HundoSwarmExecutionStep(
            6,
            "watch",
            "read residue pathing and global continuity as candidate telemetry",
            "spline-watch"),
        HundoSwarmExecutionStep(
            7,
            "verify",
            "prove closed gates after the pass",
            "verify-closed-gates")
    };

    private static object HundoSwarmExecutionStep(
        int step,
        string stepKind,
        string stepPurpose,
        string command) => new
    {
        step,
        stepKind,
        stepPurpose,
        command,
        receiptRequired = true,
        candidateOnly = true,
        gatesMustRemainClosed = true,
        admitsGel = false,
        mutatesSelfGel = false,
        grantsAuthority = false,
        authorizesAction = false,
        activatesActual = false
    };

    private static SwarmLaneEntry SwarmLane(
        string laneId,
        string laneKind,
        IReadOnlyList<string> targetCommands,
        string refinementObjective) =>
        new(
            laneId,
            laneKind,
            targetCommands,
            refinementObjective,
            residueLane: "cGEL/GEL/MoS append witness",
            authorityState: "denied-by-default",
            actualActivationAllowed: false,
            providerCallAllowed: false,
            externalActionAllowed: false);

    private static IReadOnlyList<SwarmWaveGate> BuildSwarmWaveGates() => new[]
    {
        new SwarmWaveGate(
            30,
            "baseline-morphology-pause",
            "Pause, review residue, apply narrow schema and receipt corrections."),
        new SwarmWaveGate(
            60,
            "formation-coherence-pause",
            "Pause, compare lane morphology, apply condensation/engram discipline updates."),
        new SwarmWaveGate(
            90,
            "hardening-pause",
            "Pause, red-team closed gates, apply security and governance hardening."),
        new SwarmWaveGate(
            100,
            "optimal-form-target",
            "Produce the best cold candidate form and receipt pack for Operator review.")
    };

    private static SwarmRunSession BuildSwarmRunSession(int sessionNumber, IReadOnlyList<int> pauseGates)
    {
        var sectionNumber = ((sessionNumber - 1) / 10) + 1;
        var positionInSection = ((sessionNumber - 1) % 10) + 1;
        var phase = sessionNumber switch
        {
            <= 30 => "baseline-discovery",
            <= 60 => "formation-refinement",
            <= 90 => "hardening-red-team",
            _ => "optimal-form-consolidation"
        };
        var pauseGate = pauseGates.Contains(sessionNumber);
        var optimalTarget = sessionNumber == 100;

        return new SwarmRunSession(
            sessionNumber,
            sectionNumber,
            positionInSection,
            phase,
            pauseGate,
            applyUpdatesHere: pauseGate,
            optimalFormTarget: optimalTarget,
            residueRequired: true,
            governanceReviewRequired: pauseGate || optimalTarget,
            gatesMustRemainClosed: true,
            commandMutationAllowed: pauseGate || optimalTarget,
            autonomousActionAllowed: false);
    }
}
