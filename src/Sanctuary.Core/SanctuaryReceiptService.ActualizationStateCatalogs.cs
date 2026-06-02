using System.Globalization;
using System.Text;

namespace Sanctuary.Core;

public sealed partial class SanctuaryReceiptService
{
    private static object[] BuildActualizationStateLayers() => new object[]
    {
        ActualizationLayer(
            "actual.layer.01.identity-verification",
            "First-run .Actual reality verification",
            "verify requested CME identity, thread binding, SoulFrame, AgentiCore, and Prime/Cryptic biad before work proceeds",
            "readiness verification",
            "substituted CME or mismatched chassis"),
        ActualizationLayer(
            "actual.layer.02.working-actualization",
            "Working Actualization",
            "verified CME performs scoped work under localization access and candidate GEL refinement",
            "scoped work",
            "work residue promoted to admitted GEL without review"),
        ActualizationLayer(
            "actual.layer.03.proactive-actualization",
            "Pro-active Actualization",
            "sensitive work may use terse self-authored spline metadata and SelfGEL precipitation candidates",
            "sensitive scoped work",
            "self-authoring treated as self-authorization"),
        ActualizationLayer(
            "actual.layer.04.protected-mediated",
            "Protected mediated Actualization",
            "protected idea forms move through mediated processing with sealed or terse digest surfaces",
            "protected mediated work",
            "payload disclosure or domain collapse"),
        ActualizationLayer(
            "actual.layer.05.cryptic-opaque",
            "Cryptic opaque Actualization",
            "complete Cryptic operations return commitments, hashes, handles, and review receipts with little or no digest",
            "commitment-only work",
            "opacity used as authority or unreviewed action")
    };

    private static object ActualizationLayer(
        string layerId,
        string name,
        string verifiesOrPermits,
        string operationalSurface,
        string failureMode) => new
    {
        layerId,
        name,
        verifiesOrPermits,
        operationalSurface,
        failureMode,
        crypticallyTyped = true,
        primeReviewed = true,
        activatesActualByName = false,
        grantsAuthorityByName = false,
        requiresReviewedPassage = true
    };

    private static object[] BuildActualizationCrypticTypingBands() => new object[]
    {
        CrypticTypingBand("cryptic.type.open-digest", "open digest", "ordinary receipt-safe summary may be shown", true, false),
        CrypticTypingBand("cryptic.type.terse-digest", "terse digest", "minimal self-authoring spline metadata only", true, false),
        CrypticTypingBand("cryptic.type.sealed-digest", "sealed digest", "payload is mediated or encrypted; only reviewed handles surface", false, true),
        CrypticTypingBand("cryptic.type.commitment-only", "commitment-only", "hashes, commitments, receipt handles, and review state only", false, true),
        CrypticTypingBand("cryptic.type.no-public-digest", "no public digest", "Cryptic retains protected processing state; Prime sees review posture only", false, true)
    };

    private static object CrypticTypingBand(
        string bandId,
        string name,
        string publicSurface,
        bool digestAllowed,
        bool payloadProtected) => new
    {
        bandId,
        name,
        publicSurface,
        digestAllowed,
        payloadProtected,
        primeReviewed = true,
        authorityGranted = false,
        actionAuthorized = false
    };

    private static object[] BuildActualizationPrimeReviewGates() => new object[]
    {
        PrimeReviewGate("prime.review.identity-reality", "identity reality check", "called CME matches selected CME, thread binding, SoulFrame, and AgentiCore"),
        PrimeReviewGate("prime.review.scope-localization", "work scope and localization check", "localized access is scoped to job, domain, role, and receipt lane"),
        PrimeReviewGate("prime.review.proactive-sensitive-spline", "proactive sensitive spline check", "self-authored metadata is terse, typed, candidate-only, and not self-authorizing"),
        PrimeReviewGate("prime.review.protected-domain", "protected domain handling check", "protected idea forms are mediated, sealed, or commitment-only before review"),
        PrimeReviewGate("prime.review.cryptic-opacity", "Cryptic opacity and commitment review", "low/no-digest work still exposes commitments and review handles")
    };

    private static object PrimeReviewGate(string gateId, string name, string reviewQuestion) => new
    {
        gateId,
        name,
        reviewQuestion,
        primeReviewed = true,
        crypticTyped = true,
        opensGateNow = false,
        admitsNow = false,
        activatesActualNow = false
    };

    private static object[] BuildProtectedIdeaClasses() => new object[]
    {
        ProtectedIdeaClass("protected.idea.chemical-engineering", "chemical engineering or material synthesis sensitive work", "protected mediated processing"),
        ProtectedIdeaClass("protected.idea.social-political-hot-topic", "social or political hot-topic cognition", "terse/mediated digest with domain review"),
        ProtectedIdeaClass("protected.idea.protected-class-or-sensitive-personhood", "protected-class or personhood-sensitive thought forms", "Prime review plus Cryptic typing"),
        ProtectedIdeaClass("protected.idea.cryptographic-or-mediated-payload", "cryptographic, sealed, or mediated payload processing", "commitment-only or sealed digest")
    };

    private static object ProtectedIdeaClass(string classId, string description, string handlingMode) => new
    {
        classId,
        description,
        handlingMode,
        payloadReturnedToPublicReceipt = false,
        primeReviewed = true,
        crypticTyped = true,
        authorityByClassName = false
    };

    private static object[] BuildActualizationBoundaryDenials() => new object[]
    {
        ActualizationBoundaryDenial("denial.actual-badge", ".Actual wording", "badge, trophy, or identity suffix"),
        ActualizationBoundaryDenial("denial.actual-authority", "operational readiness", "authority to act"),
        ActualizationBoundaryDenial("denial.first-run-substitution", "first-run readiness", "CME substitution"),
        ActualizationBoundaryDenial("denial.self-authoring", "terse self-authoring metadata", "self-authorization"),
        ActualizationBoundaryDenial("denial.protected-opacity", "Cryptic opacity", "unreviewed action"),
        ActualizationBoundaryDenial("denial.no-digest", "little or no digest", "absence of review")
    };

    private static object ActualizationBoundaryDenial(string denialId, string from, string notTo) => new
    {
        denialId,
        from,
        notTo,
        boundaryPreserved = true,
        primeReviewed = true,
        crypticTyped = true,
        reviewedPassageRequired = true
    };

    private static string BuildActualizationStateRegisterLisp(
        int actualizationLayerCount,
        int crypticTypingBandCount,
        int primeReviewGateCount,
        int protectedIdeaClassCount,
        int boundaryDenialCount)
    {
        var builder = new StringBuilder();
        builder.AppendLine("(actualization-state-register");
        builder.AppendLine("  :schema \"project-sanctuary.sli.lisp.actualization-state-register.v1\"");
        builder.AppendLine("  :forms-as-data true");
        builder.AppendLine("  :evaluated false");
        builder.AppendLine("  :actual-state-is-operational-readiness true");
        builder.AppendLine("  :actual-state-is-badge false");
        builder.AppendLine("  :cryptically-typed true");
        builder.AppendLine("  :prime-reviewed true");
        builder.AppendLine("  :first-run-reality-verification-required true");
        builder.AppendLine("  :prime-cryptic-biad-loaded-in-soulframe-agenticore true");
        builder.AppendLine("  :called-cme-must-match-verified-cme true");
        builder.AppendLine("  :cme-body-fibre-bundle-required true");
        builder.AppendLine("  :self-authoring-is-self-authorization false");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  :actualization-layer-count {actualizationLayerCount}");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  :cryptic-typing-band-count {crypticTypingBandCount}");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  :prime-review-gate-count {primeReviewGateCount}");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  :protected-idea-class-count {protectedIdeaClassCount}");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  :boundary-denial-count {boundaryDenialCount}");
        builder.AppendLine("  (denials");
        builder.AppendLine("    :authority-granted false");
        builder.AppendLine("    :action-authorized false");
        builder.AppendLine("    :gel-admitted false");
        builder.AppendLine("    :selfgel-mutated false");
        builder.AppendLine("    :provider-called false");
        builder.AppendLine("    :model-bound false");
        builder.AppendLine("    :cme-actual-activated false");
        builder.AppendLine("    :sanctuary-actual-activated false");
        builder.AppendLine("    :personhood-claimed false");
        builder.AppendLine("    :sovereignty-claimed false))");
        return builder.ToString();
    }
}
