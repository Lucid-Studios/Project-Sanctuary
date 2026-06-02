using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Sanctuary.Core;

public sealed partial class SanctuaryReceiptService
{
    private static object BuildCmeBodyFibreBundleRecord(SanctuaryReceipt receipt)
    {
        var actualizationStatePath = Path.Combine(
            receipt.InstallRootPath,
            "mos",
            "actual",
            SafeSegment(receipt.CmeId),
            "actualization-state.json");
        var fibres = BuildCmeBodyFibreSurfaces(receipt, actualizationStatePath);
        return new
        {
            schema = "project-sanctuary.mos.cme-body-fibre-bundle.v1",
            createdAtUtc = receipt.TimestampUtc,
            cmeId = receipt.CmeId,
            threadBindingId = receipt.Evidence.GetValueOrDefault("mosThreadBindingId"),
            callerCmeId = receipt.Evidence.GetValueOrDefault("mosCallerCmeId"),
            serviceIdentityId = receipt.Evidence.GetValueOrDefault("mosServiceIdentityId"),
            soulFrameId = receipt.Evidence.GetValueOrDefault("mosSoulFrameId"),
            agentiCoreId = receipt.Evidence.GetValueOrDefault("mosAgentiCoreId"),
            templateBodyId = receipt.Evidence.GetValueOrDefault("mosIdentityTemplateId"),
            templateBodyIsIdentity = false,
            cmeActualIsStateNotIdentity = true,
            bodyDoctrine = receipt.Evidence.GetValueOrDefault("mosCmeBodyFibreBundleDoctrine"),
            baseSpace = receipt.Evidence.GetValueOrDefault("mosCmeBodyFibreBaseSpace"),
            totalSpace = receipt.Evidence.GetValueOrDefault("mosCmeBodyFibreTotalSpace"),
            projection = receipt.Evidence.GetValueOrDefault("mosCmeBodyFibreProjection"),
            connection = "thread binding + domain/role/job slice + review gates",
            localTrivialization = "this CME thread binding owns this local body chart",
            transitionFunctionLaw = "cross-thread or cross-domain transfer requires reviewed morphism; no fibre self-promotes",
            fibres,
            fibreCount = fibres.Length,
            actualizationStatePath,
            actualizationStatePresent = File.Exists(actualizationStatePath),
            gates = new
            {
                receipt.Gates.AllClosed,
                receipt.Gates.GelAdmitted,
                receipt.Gates.SelfGelMutated,
                receipt.Gates.ContinuityAdmitted,
                receipt.Gates.AuthorityGranted,
                receipt.Gates.ActionAuthorized,
                receipt.Gates.ExternalActionAuthorized,
                receipt.Gates.ProviderCalled,
                receipt.Gates.ModelBound,
                receipt.Gates.CmeActualActivated,
                receipt.Gates.SanctuaryActualActivated,
                receipt.Gates.PersonhoodClaimed,
                receipt.Gates.SovereigntyClaimed
            },
            candidateOnly = !receipt.Gates.CmeActualActivated,
            mutationAllowedByBundle = false,
            authorityGrantedByBundle = false,
            actionAuthorizedByBundle = false,
            providerCalledByBundle = false,
            modelBoundByBundle = false,
            personhoodClaimedByBundle = false,
            sovereigntyClaimedByBundle = false
        };
    }

    private static object[] BuildCmeBodyFibreSurfaces(SanctuaryReceipt receipt, string actualizationStatePath) => new object[]
    {
        CmeBodyFibreSurface(
            "soulframe.prime-oe",
            "SoulFrame",
            "Prime.OE append-only tip",
            receipt.Evidence.GetValueOrDefault("localMosSoulFramePrimeOeTipPath") as string ?? "",
            true),
        CmeBodyFibreSurface(
            "soulframe.prime-selfgel",
            "SoulFrame",
            "Prime.SelfGEL append-only tip",
            receipt.Evidence.GetValueOrDefault("localMosSoulFramePrimeSelfGelTipPath") as string ?? "",
            true),
        CmeBodyFibreSurface(
            "oe.reconstruction",
            "OE",
            "autobiographical observation-event reconstruction ledger",
            receipt.Evidence.GetValueOrDefault("localMosOeLedgerPath") as string ?? "",
            true),
        CmeBodyFibreSurface(
            "selfgel.reconstruction",
            "SelfGEL",
            "reconstruction-support ledger",
            receipt.Evidence.GetValueOrDefault("localMosSelfGelLedgerPath") as string ?? "",
            true),
        CmeBodyFibreSurface(
            "agenticore.coe",
            "AgentiCore",
            "hot-side cOE EC-use ledger",
            receipt.Evidence.GetValueOrDefault("localMosAgentiCoreCoeLedgerPath") as string ?? "",
            true),
        CmeBodyFibreSurface(
            "agenticore.cselfgel",
            "AgentiCore",
            "hot-side cSelfGEL EC-use ledger",
            receipt.Evidence.GetValueOrDefault("localMosAgentiCoreCSelfGelLedgerPath") as string ?? "",
            true),
        CmeBodyFibreSurface(
            "template.chassis",
            "Template",
            "lab-standard SLI.Lisp Industrial CME chassis",
            receipt.Evidence.GetValueOrDefault("localGelLabStandardTemplateBodyJsonPath") as string ?? "",
            true),
        CmeBodyFibreSurface(
            "actual.readiness",
            "Actualization",
            "reviewed CME.Actual operational readiness state when present",
            actualizationStatePath,
            File.Exists(actualizationStatePath))
    };

    private static object CmeBodyFibreSurface(
        string fibreId,
        string organ,
        string purpose,
        string path,
        bool present) => new
    {
        fibreId,
        organ,
        purpose,
        path,
        present,
        candidateOnly = true,
        admitsGel = false,
        mutatesSelfGel = false,
        grantsAuthority = false,
        authorizesAction = false,
        claimsPersonhood = false,
        claimsSovereignty = false
    };

    private static string BuildCmeBodyFibreBundleLisp(SanctuaryReceipt receipt)
    {
        var builder = new StringBuilder();
        builder.AppendLine("(cme-body-fibre-bundle");
        builder.AppendLine("  :schema \"project-sanctuary.sli.lisp.cme-body-fibre-bundle.v1\"");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  :cme-id \"{LispString(receipt.CmeId)}\"");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  :soulframe-id \"{LispString(receipt.Evidence.GetValueOrDefault("mosSoulFrameId") as string ?? "")}\"");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  :agenticore-id \"{LispString(receipt.Evidence.GetValueOrDefault("mosAgentiCoreId") as string ?? "")}\"");
        builder.AppendLine("  :template-is-identity false");
        builder.AppendLine("  :cme-actual-is-state-not-identity true");
        builder.AppendLine("  :base-space \"Sanctuary.GEL.shared-prime-weather\"");
        builder.AppendLine("  :total-space \"MoS/OE/SelfGEL/cOE/cSelfGEL\"");
        builder.AppendLine("  :projection \"instantiated CME body fibres project into scoped participant posture\"");
        builder.AppendLine("  :fibre-count 8");
        builder.AppendLine("  :mutation-allowed-by-bundle false");
        builder.AppendLine("  :authority-granted-by-bundle false");
        builder.AppendLine("  :action-authorized-by-bundle false");
        builder.AppendLine("  :personhood-claimed false");
        builder.AppendLine("  :sovereignty-claimed false");
        builder.AppendLine("  (fibres");
        builder.AppendLine("    (fibre :id \"soulframe.prime-oe\" :organ \"SoulFrame\")");
        builder.AppendLine("    (fibre :id \"soulframe.prime-selfgel\" :organ \"SoulFrame\")");
        builder.AppendLine("    (fibre :id \"oe.reconstruction\" :organ \"OE\")");
        builder.AppendLine("    (fibre :id \"selfgel.reconstruction\" :organ \"SelfGEL\")");
        builder.AppendLine("    (fibre :id \"agenticore.coe\" :organ \"AgentiCore\")");
        builder.AppendLine("    (fibre :id \"agenticore.cselfgel\" :organ \"AgentiCore\")");
        builder.AppendLine("    (fibre :id \"template.chassis\" :organ \"Template\")");
        builder.AppendLine("    (fibre :id \"actual.readiness\" :organ \"Actualization\")))");
        return builder.ToString();
    }

    private static void WriteGelTemplateBodies(SanctuaryReceipt receipt)
    {
        var templateId = receipt.Evidence.GetValueOrDefault("mosIdentityTemplateId") as string ?? "SLI.Lisp.Industrial.CME.Template";
        var templateRegistryPath = receipt.Evidence.GetValueOrDefault("localGelTemplateRegistryPath") as string ?? "";
        var labTemplateBodyJsonPath = receipt.Evidence.GetValueOrDefault("localGelLabStandardTemplateBodyJsonPath") as string ?? "";
        var labTemplateBodyLispPath = receipt.Evidence.GetValueOrDefault("localGelLabStandardTemplateBodyLispPath") as string ?? "";
        var localCustomRegistryPath = receipt.Evidence.GetValueOrDefault("localGelLocalCustomTemplateRegistryPath") as string ?? "";

        if (string.IsNullOrWhiteSpace(templateRegistryPath) ||
            string.IsNullOrWhiteSpace(labTemplateBodyJsonPath) ||
            string.IsNullOrWhiteSpace(labTemplateBodyLispPath) ||
            string.IsNullOrWhiteSpace(localCustomRegistryPath))
        {
            return;
        }

        var templateBody = new
        {
            schema = "project-sanctuary.gel.template-body.v1",
            templateId,
            templateBodyKind = "IndustrialCME",
            lane = "LabStandard",
            canonical = string.Equals(templateId, "SLI.Lisp.Industrial.CME.Template", StringComparison.Ordinal),
            labStandard = true,
            customLocalBodiesAllowedAfterLabStandard = true,
            templateIsIdentity = false,
            cmeIdentityPattern = "{Name}.CME.ID",
            cmeActualIsStateNotIdentity = true,
            defaultActualizationState = "reviewed-post-gate-only",
            sharedPrimeRealityLayer = "Sanctuary.Actual.weather-system",
            sharedPrimeRealityMembrane = BuildSharedPrimeRealityMembrane(receipt.Evidence.GetValueOrDefault("mosServiceIdentityId") as string),
            personalCmePrivateRadioStation = false,
            cmeMayReceiveSharedPrimeWeather = true,
            cmeMayBroadcastPrimeReality = false,
            cmePrivateTelemetryDefinesSharedPrime = false,
            cmeLocalObservationCandidateOnly = true,
            governingNeedsMatrixKind = "domain-job-contractual-obligation-matrix",
            governingNeedsMatrixIsHumanNeedsHierarchy = false,
            governingNeedsMatrix = BuildGoverningNeedsMatrix(),
            governingAccessLevelsKind = "slice-tool-groupoid-access-degrees",
            governingAccessLevelsManufacturedFrom = "domain-predicate-locality-over-typed-local-access",
            governingAccessLevels = BuildGoverningAccessLevels(),
            negativeGoverningLevelsScope = "security-enhancement-outside-civic-access",
            negativeGoverningLevels = BuildNegativeGoverningLevels(),
            bodyFibreBundleChassis = BuildTemplateBodyFibreBundleChassis(),
            bodyFibreBundleChassisCount = BuildTemplateBodyFibreBundleChassis().Length,
            organs = new[]
            {
                "SoulFrame",
                "AgentiCore",
                "OE",
                "SelfGEL",
                "cOE",
                "cSelfGEL",
                "MoS",
                "SLI.Lisp"
            },
            denials = new
            {
                admitsGelByTemplate = false,
                mutatesSelfGelByTemplate = false,
                grantsAuthorityByTemplate = false,
                activatesCmeActualByTemplate = false,
                activatesSanctuaryActualByTemplate = false,
                claimsPersonhoodByTemplate = false,
                claimsSovereigntyByTemplate = false
            }
        };

        WriteJsonFile(labTemplateBodyJsonPath, templateBody);
        WriteTextFile(
            labTemplateBodyLispPath,
            $$"""
            (industrial-cme-template-body
              (:template-id "{{LispString(templateId)}}")
              (:lane "LabStandard")
              (:template-is-identity false)
              (:cme-identity-pattern "{Name}.CME.ID")
              (:cme-actual-is-state-not-identity true)
              (:custom-local-bodies-after-lab-standard true)
              (:shared-prime-reality-layer "Sanctuary.Actual.weather-system")
              (:personal-cme-private-radio-station false)
              (:cme-may-receive-shared-prime-weather true)
              (:cme-may-broadcast-prime-reality false)
              (:cme-private-telemetry-defines-shared-prime false)
              (:cme-local-observation-candidate-only true)
              (:governing-needs-matrix-kind "domain-job-contractual-obligation-matrix")
              (:governing-needs-matrix-is-human-needs-hierarchy false)
              (:governing-access-levels-kind "slice-tool-groupoid-access-degrees")
              (:governing-access-levels-manufactured-from "domain-predicate-locality-over-typed-local-access")
              (:negative-governing-levels-scope "security-enhancement-outside-civic-access")
              (:body-fibre-bundle-chassis true)
              (:body-fibre-bundle-chassis-count 8)
              (:organs ("SoulFrame" "AgentiCore" "OE" "SelfGEL" "cOE" "cSelfGEL" "MoS" "SLI.Lisp"))
              (:admits-gel-by-template false)
              (:mutates-selfgel-by-template false)
              (:grants-authority-by-template false)
              (:activates-cme-actual-by-template false)
              (:activates-sanctuary-actual-by-template false)
              (:personhood-claim false)
              (:sovereignty-claim false))
            """);

        WriteJsonFile(
            templateRegistryPath,
            new
            {
                schema = "project-sanctuary.gel.template-registry.v1",
                laneOrder = "LabStandardThenLocalCustom",
                labStandardTemplateBodyId = templateId,
                labStandardTemplateBodyPath = labTemplateBodyJsonPath,
                localCustomTemplateRegistryPath = localCustomRegistryPath,
                templateBodiesAreIdentities = false,
                cmeActualIsStateNotIdentity = true,
                customLocalBodiesRequireReview = true
            });

        if (!File.Exists(localCustomRegistryPath))
        {
            WriteJsonFile(
                localCustomRegistryPath,
                new
                {
                    schema = "project-sanctuary.gel.local-template-registry.v1",
                    lane = "LocalCustomAfterLabStandard",
                    labStandardTemplateBodyId = templateId,
                    customBodies = Array.Empty<object>(),
                    customBodiesAreIdentities = false,
                    reviewRequiredBeforeUse = true
                });
        }
    }
}
