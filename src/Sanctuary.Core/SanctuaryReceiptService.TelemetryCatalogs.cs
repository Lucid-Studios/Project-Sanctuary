using System.Globalization;
using System.Text;

namespace Sanctuary.Core;

public sealed partial class SanctuaryReceiptService
{
    private static IReadOnlyList<TelemetryGoverningOrgan> BuildPrimeCrypticStewardTelemetryOrgans() => new[]
    {
        new TelemetryGoverningOrgan(
            "Prime",
            "witness-authorization-archival-review",
            "hold shared-prime weather, receipt truth posture, release posture, and closed-gate attestation",
            "broadcast only shared weather and review outcomes; archive receipts and attestations",
            new[] { "ResearchLab.GEL", "Government.GEL", "EducationTrainingCertification.GEL", "Legal.GEL", "Medical.GEL", "SpecialCases.SAGE.GEL" },
            new[]
            {
                BuildTelemetrySlice(
                    "prime.closed-gate-attestation",
                    "Prime",
                    "every-meaningful-run",
                    "after command group or coupling check",
                    "prove the cold lane did not leak authority, provider calls, model binding, GEL admission, or Actual state",
                    new[] { "status", "plugin-posture", "extended-telemetry-weather", "approval-closure-register", "coupling-control-surface-register", "verify-closed-gates" },
                    new[] { "ResearchLab.GEL", "InstallProduct.GEL" },
                    new[]
                    {
                        BuildTelemetryPoint("prime.closed-gates.all-closed", "gate-state", "verify-closed-gates receipt", "archival", "receipts/closed-gate-verification", "Prime", false, true, false),
                        BuildTelemetryPoint("prime.provider-model-denial", "provider/model denial", "receipt gates", "archival", "receipt evidence", "Prime", false, true, false),
                        BuildTelemetryPoint("prime.public-weather-summary", "shared-prime weather", "status/plugin-posture", "broadcast", "Sanctuary.Actual.weather-system", "Prime", true, false, false),
                        BuildTelemetryPoint("prime.extended-weather.reveal", "Cryptic-origin weather reveal", "extended-telemetry-weather", "broadcast", "Sanctuary.Actual.weather-system", "Prime", true, false, false),
                        BuildTelemetryPoint("prime.gate-homeostasis.approval-closure", "approval and closure homeostasis", "approval-closure-register", "archival", "cgel/approval-closure", "Prime", false, true, false),
                        BuildTelemetryPoint("prime.coupling-control.hitl-stability", "HITL-readable active program coupling posture", "coupling-control-surface-register", "archival", "cgel/coupling-control-surface", "Prime", false, true, false)
                    }),
                BuildTelemetrySlice(
                    "prime.release-authority",
                    "Prime",
                    "manual-or-release-candidate",
                    "public release, domain posture, or authority language changes",
                    "check what can be shown publicly and what remains held by Lab review",
                    new[] { "domain-register", "industrial-cme-live-install-posture", "receipt-export" },
                    new[] { "ResearchLab.GEL", "Government.GEL", "Legal.GEL", "InstallProduct.GEL" },
                    new[]
                    {
                        BuildTelemetryPoint("prime.release.claim-surface", "public claim posture", "domain-register", "silent-until-polled", "cgel/domain-register", "Prime", false, false, true),
                        BuildTelemetryPoint("prime.release.instrument-readiness", "instrument readiness", "industrial-cme-live-install-posture", "archival", "cgel/industrial-cme-live-install", "Prime", false, true, false),
                        BuildTelemetryPoint("prime.release.receipt-manifest", "receipt manifest", "receipt-export", "archival", "receipt manifest", "Prime", false, true, false)
                    }),
                BuildTelemetrySlice(
                    "prime.actualization-readiness",
                    "Prime",
                    "reviewed-research-only",
                    "operator requests Actualization study or discernment-lineage review",
                    "track readiness predicates without opening CME.Actual",
                    new[] { "operator-work-cme-ec-gap", "actualization-state-register", "discernment-lineage", "proof-of-discernment" },
                    new[] { "ResearchLab.GEL", "EducationTrainingCertification.GEL", "SpecialCases.SAGE.GEL" },
                    new[]
                    {
                        BuildTelemetryPoint("prime.actualization.gap-map", "gap closure map", "operator-work-cme-ec-gap", "archival", "cgel/operator-work-cme-ec-gap", "Prime", false, true, false),
                        BuildTelemetryPoint("prime.actualization.state-spectrum", "Actual operational readiness spectrum", "actualization-state-register", "archival", "cgel/actualization-state", "Prime", false, true, false),
                        BuildTelemetryPoint("prime.actualization.discernment", "discernment predicate", "discernment-lineage", "archival", "cgel/discernment-lineage", "Prime", false, true, false),
                        BuildTelemetryPoint("prime.actualization.pressure-bench", "discernment pressure", "proof-of-discernment", "residue", "cgel/discernment-lineage/proof-of-discernment", "Prime", false, false, true)
                    })
            }),
        new TelemetryGoverningOrgan(
            "Cryptic",
            "membrane-residue-security-anomaly-processing",
            "hold SLI access posture, cryptic custody, anomaly review, and decantable residue without payload exposure",
            "broadcast only normalized conditions; keep sensitive logic and payload interpretation silent until polled by authority",
            new[] { "Security.GEL", "AccountAccess.GEL", "InstallProduct.GEL", "SpecialCases.SAGE.GEL" },
            new[]
            {
                BuildTelemetrySlice(
                    "cryptic.sli-access",
                    "Cryptic",
                    "first-run-or-access-change",
                    "SLI, Root Atlas, account access, or connector posture changes",
                    "verify symbolic interconnect and access-gate posture without opening passage authority",
                    new[] { "sli-register", "sli-access-gate-register", "typed-secure-ping" },
                    new[] { "Security.GEL", "AccountAccess.GEL" },
                    new[]
                    {
                        BuildTelemetryPoint("cryptic.sli.root-atlas", "symbolic registry", "sli-register", "archival", "cgel/sli", "Cryptic", false, true, false),
                        BuildTelemetryPoint("cryptic.sli.access-gate", "passage gate", "sli-access-gate-register", "archival", "cgel/sli-access-gate", "Cryptic", false, true, false),
                        BuildTelemetryPoint("cryptic.sli.ping", "secure ping condition", "typed-secure-ping", "broadcast", "typed secure ping receipt", "Cryptic", true, false, false)
                    }),
                BuildTelemetrySlice(
                    "cryptic.security-hardening",
                    "Cryptic",
                    "security-change-or-pre-network",
                    "secret custody, network exposure, issue resolver, or lease posture changes",
                    "shake the membrane before network, secret, or support surfaces are widened",
                    new[] { "security-hardening", "lease-check", "install-floor-check", "issue-resolver" },
                    new[] { "Security.GEL", "InstallProduct.GEL", "AccountAccess.GEL" },
                    new[]
                    {
                        BuildTelemetryPoint("cryptic.security.drift", "gate drift", "security-hardening", "residue", "cgel/security-hardening", "Cryptic", false, false, true),
                        BuildTelemetryPoint("cryptic.security.lease", "lease expiry", "lease-check", "broadcast", "service weather", "Cryptic", true, false, false),
                        BuildTelemetryPoint("cryptic.security.install-floor", "install floor", "install-floor-check/issue-resolver", "archival", "cgel/install-floor", "Cryptic", false, true, false)
                    }),
                BuildTelemetrySlice(
                    "cryptic.residue-decant",
                    "Cryptic",
                    "hourly-or-residue-threshold",
                    "residue accumulation, anomaly, failed validation, or mulch pressure",
                    "sort residue into admit-candidate, append-candidate, mulch, quarantine, or refusal lanes",
                    new[] { "typed-admission-decant", "admission-cleave-append", "spline-watch" },
                    new[] { "Security.GEL", "ResearchLab.GEL", "EducationTrainingCertification.GEL" },
                    new[]
                    {
                        BuildTelemetryPoint("cryptic.decant.admission-candidate", "admission candidate", "typed-admission-decant", "residue", "cgel/typed-admission-decant", "Cryptic", false, false, true),
                        BuildTelemetryPoint("cryptic.decant.cleave-decision", "cleave decision", "admission-cleave-append", "archival", "cgel/admission-cleave", "Cryptic", false, true, false),
                        BuildTelemetryPoint("cryptic.decant.spline-pressure", "spline pressure", "spline-watch", "broadcast", "ListeningFrame weather", "Cryptic", true, false, false)
                    })
            }),
        new TelemetryGoverningOrgan(
            "Steward",
            "service-learning-cadence-care-and-work-orchestration",
            "hold service health, learning cadence, work composition, and operator-facing continuity without converting telemetry into authority",
            "broadcast service weather and learning summaries; archive cadence receipts; keep detailed history pollable",
            new[] { "Industrial.GEL", "Commercial.GEL", "Civic.GEL", "PersonalWellness.GEL", "HumanServices.GEL", "EducationTrainingCertification.GEL" },
            new[]
            {
                BuildTelemetrySlice(
                    "steward.service-health",
                    "Steward",
                    "heartbeat-or-startup",
                    "service start, restart adjacency, or idle gap",
                    "keep the tool body oriented without running heavy benches",
                    new[] { "service-heartbeat", "job-slice-guard", "bounded-refinement-ticket", "tool-idle" },
                    new[] { "InstallProduct.GEL", "ResearchLab.GEL" },
                    new[]
                    {
                        BuildTelemetryPoint("steward.service.heartbeat", "heartbeat", "service-heartbeat", "broadcast", "service ledger", "Steward", true, false, false),
                        BuildTelemetryPoint("steward.service.next-slice", "next slice pointer", "job-slice-guard", "silent-until-polled", "service next-slice pointer", "Steward", false, false, true),
                        BuildTelemetryPoint("steward.service.refinement-ticket", "bounded job ticket", "bounded-refinement-ticket", "archival", "service refinement ticket", "Steward", false, true, false)
                    }),
                BuildTelemetrySlice(
                    "steward.training-learning",
                    "Steward",
                    "sampled-cadence",
                    "learning target, STEM corpus update, or bench residue review",
                    "sample cognitive, math, and STEM learning surfaces without waking all tests",
                    new[] { "cognitive-bench", "math-learning-bench", "stem-domain-training-certification", "lab-observation-digest", "research-latex-export", "construct-custody-register", "gel-crystal-register", "gel-reforge-bench" },
                    new[] { "EducationTrainingCertification.GEL", "ResearchLab.GEL", "Industrial.GEL" },
                    new[]
                    {
                        BuildTelemetryPoint("steward.learning.cognitive", "cognitive bench condensate", "cognitive-bench", "residue", "cgel/cognitive-bench", "Steward", false, false, true),
                        BuildTelemetryPoint("steward.learning.math", "math learning heat", "math-learning-bench", "residue", "cgel/math-learning-bench", "Steward", false, false, true),
                        BuildTelemetryPoint("steward.learning.stem", "STEM certification telemetry", "stem-domain-training-certification", "archival", "cgel/stem-domain-training-certification", "Steward", false, true, false),
                        BuildTelemetryPoint("steward.learning.lab-observation-digest", "testing digest and OE autobiographical practice", "lab-observation-digest", "archival", "cgel/lab-observation-digest", "Steward", false, true, false),
                        BuildTelemetryPoint("steward.learning.research-latex-export", "rarified research LaTeX decant", "research-latex-export", "archival", "cgel/research-latex-export", "Steward", false, true, false),
                        BuildTelemetryPoint("steward.learning.construct-custody", "custodied construct canon", "construct-custody-register", "archival", "cgel/construct-custody", "Steward", false, true, false),
                        BuildTelemetryPoint("steward.learning.gel-crystal", "candidate GEL crystal survivorship", "gel-crystal-register", "archival", "cgel/gel-crystal", "Steward", false, true, false),
                        BuildTelemetryPoint("steward.learning.gel-reforge", "knowing teaching doing reforge", "gel-reforge-bench", "archival", "cgel/gel-reforge", "Steward", false, true, false)
                    }),
                BuildTelemetrySlice(
                    "steward.work-composition",
                    "Steward",
                    "domain-or-work-map-change",
                    "new work domain, job slice, skill/career map, or bridge requirement",
                    "compose work posture from universal form, domain morphism, SelfGEL fibre, and bridge tests",
                    new[] { "universal-form-register", "domain-morphism-register", "capability-composition-probe", "career-spline-probe", "selfgel-fibre-register", "work-posture-preload-probe", "bridge-morphism-test", "cgoa-formation", "codex-governing-witness", "full-body-io-runtime" },
                    new[] { "Industrial.GEL", "Commercial.GEL", "Civic.GEL", "HumanServices.GEL", "EducationTrainingCertification.GEL" },
                    new[]
                    {
                        BuildTelemetryPoint("steward.work.domain-bridge", "domain bridge", "domain-morphism-register/bridge-morphism-test", "archival", "cgel/domain-morphism-register", "Steward", false, true, false),
                        BuildTelemetryPoint("steward.work.capability", "capability composition", "capability-composition-probe", "residue", "cgel/capability-composition", "Steward", false, false, true),
                        BuildTelemetryPoint("steward.work.preload", "work posture preload", "work-posture-preload-probe", "silent-until-polled", "cgel/work-posture-preload", "Steward", false, false, true),
                        BuildTelemetryPoint("steward.work.cgoa", "candidate Gate of Alignment bundle", "cgoa-formation", "archival", "cgel/cgoa-formation", "Steward", false, true, false),
                        BuildTelemetryPoint("steward.work.codex-governing-witness", "Codex governing witness over Oria work", "codex-governing-witness", "archival", "cgel/codex-governing-witness", "Steward", false, true, false),
                        BuildTelemetryPoint("steward.work.full-body-io-runtime", "I-to-O runtime trace", "full-body-io-runtime", "archival", "cgel/full-body-io-runtime", "Steward", false, true, false)
                    })
            })
    };

    private static TelemetrySlice BuildTelemetrySlice(
        string sliceId,
        string organId,
        string cadence,
        string trigger,
        string purpose,
        IReadOnlyList<string> commands,
        IReadOnlyList<string> domainFamilies,
        IReadOnlyList<TelemetryPoint> points) => new(
            sliceId,
            organId,
            cadence,
            trigger,
            purpose,
            commands,
            domainFamilies,
            points,
            runsEveryCycle: false,
            requiresTrigger: true,
            candidateOnly: true);

    private static TelemetryPoint BuildTelemetryPoint(
        string pointId,
        string signalKind,
        string source,
        string emissionClass,
        string store,
        string reviewedBy,
        bool broadcast,
        bool archival,
        bool silentUntilPolled) => new(
            pointId,
            signalKind,
            source,
            emissionClass,
            store,
            reviewedBy,
            broadcast,
            archival,
            silentUntilPolled,
            candidateOnly: true);

    private static string BuildTelemetrySliceRegisterLisp(IReadOnlyList<TelemetryGoverningOrgan> organs)
    {
        var builder = new StringBuilder();
        builder.AppendLine("(telemetry-slice-register");
        builder.AppendLine("  :schema \"project-sanctuary.sli.lisp.telemetry-slice-register.v1\"");
        builder.AppendLine("  :scheduler-mode \"selective-slice-cadence\"");
        builder.AppendLine("  :all-tests-run-all-times false");
        builder.AppendLine("  :global-fanout-default false");
        builder.AppendLine("  :candidate-telemetry-only true");
        builder.AppendLine("  (governing-organs");
        foreach (var organ in organs)
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $"    ({organ.organId.ToLowerInvariant()} :position \"{organ.position}\" :slice-count {organ.slices.Count})");
        }
        builder.AppendLine("  )");
        builder.AppendLine("  (slices");
        foreach (var slice in organs.SelectMany(organ => organ.slices))
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $"    (slice :id \"{slice.sliceId}\" :organ \"{slice.organId}\" :cadence \"{slice.cadence}\" :requires-trigger true :runs-every-cycle false)");
        }
        builder.AppendLine("  )");
        builder.AppendLine("  (denials");
        builder.AppendLine("    :telemetry-admitted false");
        builder.AppendLine("    :memory-admitted false");
        builder.AppendLine("    :gel-admitted false");
        builder.AppendLine("    :selfgel-mutated false");
        builder.AppendLine("    :authority-granted false");
        builder.AppendLine("    :action-authorized false");
        builder.AppendLine("    :provider-called false");
        builder.AppendLine("    :model-bound false");
        builder.AppendLine("    :actual-activated false))");
        return builder.ToString();
    }


    private static IReadOnlyList<ExtendedTelemetrySourceSignal> BuildExtendedTelemetrySourceSignals() => new[]
    {
        BuildExtendedTelemetrySourceSignal(
            "cryptic.source.residue-pressure",
            "residue-front",
            "candidate residue accumulation",
            "typed-admission-decant/admission-cleave-append",
            "ResearchLab.GEL",
            "residue",
            "cgel/typed-admission-decant"),
        BuildExtendedTelemetrySourceSignal(
            "cryptic.source.sli-gate-pressure",
            "sli-gate-weather",
            "symbolic access gate pressure",
            "sli-register/sli-access-gate-register",
            "Security.GEL",
            "silent-until-polled",
            "cgel/sli-access-gate"),
        BuildExtendedTelemetrySourceSignal(
            "cryptic.source.lease-decay",
            "lease-weather",
            "delta-decaying authority lease posture",
            "lease-check",
            "AccountAccess.GEL",
            "broadcast",
            "service weather"),
        BuildExtendedTelemetrySourceSignal(
            "cryptic.source.identity-binding",
            "identity-weather",
            "CME identity binding and first-writer lock pressure",
            "MoS identity binding",
            "InstallProduct.GEL",
            "archival",
            "mos/cme-bindings"),
        BuildExtendedTelemetrySourceSignal(
            "cryptic.source.cross-thread-denial",
            "boundary-weather",
            "cross-thread CME identity denial pressure",
            "first-use-cme-lock",
            "Security.GEL",
            "residue",
            "mos/cme-bindings"),
        BuildExtendedTelemetrySourceSignal(
            "cryptic.source.secret-custody",
            "custody-weather",
            "secret intake and sealed payload custody posture",
            "secret-intake-window/seal-secret-payloads",
            "Legal.GEL",
            "silent-until-polled",
            "cryptic-stores"),
        BuildExtendedTelemetrySourceSignal(
            "cryptic.source.network-exposure",
            "edge-weather",
            "loopback, HTTPS, public-bind, and connector edge posture",
            "gpt-use-case-testing/trivium-forum-connector-posture",
            "AccountAccess.GEL",
            "broadcast",
            "cgel/gpt-use-case-testing"),
        BuildExtendedTelemetrySourceSignal(
            "cryptic.source.bench-heat",
            "bench-weather",
            "cognitive/math/STEM bench pressure",
            "cognitive-bench/math-learning-bench/stem-domain-training-certification",
            "EducationTrainingCertification.GEL",
            "residue",
            "cgel/math-learning-bench"),
        BuildExtendedTelemetrySourceSignal(
            "cryptic.source.decant-queue",
            "decant-weather",
            "admit, append, hold, refuse, quarantine, and mulch queue pressure",
            "typed-admission-decant/admission-cleave-append",
            "ResearchLab.GEL",
            "silent-until-polled",
            "cgel/admission-cleave"),
        BuildExtendedTelemetrySourceSignal(
            "cryptic.source.mulch-pressure",
            "mulch-weather",
            "failed, noisy, or not-yet-admissible residue value",
            "admission-cleave-append",
            "ResearchLab.GEL",
            "residue",
            "cgel/admission-cleave"),
        BuildExtendedTelemetrySourceSignal(
            "cryptic.source.outlier-precipitation",
            "outlier-weather",
            "outlier shared-pattern residue for Sanctuary.GEL review",
            "gel-approval-nadir-return",
            "ResearchLab.GEL",
            "residue",
            "cgel/gel-approval-nadir-return"),
        BuildExtendedTelemetrySourceSignal(
            "cryptic.source.selfgel-proximal",
            "selfgel-proximal-weather",
            "SelfGEL proximal reconstruction support pressure",
            "selfgel-fibre-register/witness-learning",
            "PersonalWellness.GEL",
            "silent-until-polled",
            "gel/mos/{CME.ID}/selfgel")
    };

    private static ExtendedTelemetrySourceSignal BuildExtendedTelemetrySourceSignal(
        string signalId,
        string weatherKey,
        string conditionClass,
        string sourceSurface,
        string sourceDomain,
        string emissionClass,
        string store) => new(
            signalId,
            originOrgan: "Cryptic",
            managedBy: "Prime",
            revealedBy: "Prime",
            sharedWith: "Cryptic",
            weatherKey,
            conditionClass,
            sourceSurface,
            sourceDomain,
            emissionClass,
            store,
            crypticOrigin: true,
            primeWeatherReveal: true,
            payloadExposed: false,
            gateOpened: false,
            candidateOnly: true);

    private static PrimeWeatherSignal BuildPrimeWeatherSignal(ExtendedTelemetrySourceSignal signal) => new(
        weatherId: $"prime.weather.{signal.weatherKey}",
        sourceSignalId: signal.signalId,
        revealedBy: "Prime",
        sharedWith: signal.sharedWith,
        conditionClass: signal.conditionClass,
        weatherBand: BuildWeatherBand(signal.emissionClass),
        weatherMeaning: "shared condition only; Cryptic payload and interpretation remain behind the membrane",
        sourceDigest: Digest($"{signal.signalId}|{signal.weatherKey}|{signal.sourceSurface}|{signal.store}"),
        payloadExposed: false,
        crypticInterpretationExposed: false,
        authorityGranted: false,
        actionAuthorized: false,
        candidateOnly: true);

    private static string BuildWeatherBand(string emissionClass) =>
        emissionClass switch
        {
            "broadcast" => "visible-watch",
            "archival" => "recorded-stable",
            "silent-until-polled" => "latent-pollable",
            "residue" => "residue-front",
            _ => "condition-watch"
        };

    private static string BuildExtendedTelemetryWeatherLisp(IReadOnlyList<ExtendedTelemetrySourceSignal> sourceSignals)
    {
        var builder = new StringBuilder();
        builder.AppendLine("(extended-telemetry-weather");
        builder.AppendLine("  :schema \"project-sanctuary.sli.lisp.extended-telemetry-weather.v1\"");
        builder.AppendLine("  :source-owner \"Cryptic\"");
        builder.AppendLine("  :managed-by \"Prime\"");
        builder.AppendLine("  :revealed-by \"Prime\"");
        builder.AppendLine("  :shared-with \"Cryptic\"");
        builder.AppendLine("  :reveal-mode \"weather-only\"");
        builder.AppendLine("  :payload-exposed false");
        builder.AppendLine("  :cryptic-interpretation-exposed false");
        builder.AppendLine("  (source-signals");
        foreach (var signal in sourceSignals)
        {
            builder.AppendLine(
                CultureInfo.InvariantCulture,
                $"    (signal :id \"{signal.signalId}\" :weather-key \"{signal.weatherKey}\" :condition \"{signal.conditionClass}\" :origin \"Cryptic\" :managed-by \"Prime\" :candidate-only true)");
        }
        builder.AppendLine("  )");
        builder.AppendLine("  (denials");
        builder.AppendLine("    :telemetry-admitted false");
        builder.AppendLine("    :memory-admitted false");
        builder.AppendLine("    :gel-admitted false");
        builder.AppendLine("    :selfgel-mutated false");
        builder.AppendLine("    :truth-admitted-by-weather false");
        builder.AppendLine("    :authority-granted-by-weather false");
        builder.AppendLine("    :action-authorized-by-weather false");
        builder.AppendLine("    :provider-called false");
        builder.AppendLine("    :model-bound false");
        builder.AppendLine("    :actual-activated false))");
        return builder.ToString();
    }
}
