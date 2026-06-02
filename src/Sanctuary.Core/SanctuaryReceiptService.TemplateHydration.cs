using System.Text;
using System.Text.Json;

namespace Sanctuary.Core;

public sealed partial class SanctuaryReceiptService
{
    private static readonly string[] PublicTemplateLanguageSurfaces =
    {
        "English",
        "French",
        "German",
        "Spanish",
        "Portuguese",
        "ChineseSimplified",
        "Korean",
        "Japanese",
        "Russian",
        "Arabic",
        "Hindi",
        "Bengali",
        "Indonesian",
        "Turkish",
        "Vietnamese",
        "Persian"
    };

    private static readonly string[] PublicTemplateStemSurfaces =
    {
        "mathematics",
        "formal-logic",
        "computer-science",
        "systems-engineering",
        "statistics",
        "physics",
        "chemistry",
        "biology",
        "cognitive-science"
    };

    private static void AddTemplateHydrationEvidence(
        Dictionary<string, object?> evidence,
        SanctuaryRequest request,
        DateTimeOffset timestamp)
    {
        const string templateId = "PublicStandard.CME.Template";
        const string templateVersion = "0.1.0";
        const string templateChannel = "stable";

        var safeTemplateId = SafeSegment(templateId);
        var hydrationRoot = Path.Combine(request.InstallRootPath, "cgel", "template-hydration");
        var registerPath = Path.Combine(hydrationRoot, "template-hydration.json");
        var registerLispPath = Path.Combine(hydrationRoot, "template-hydration.sli.lisp");
        var ledgerPath = Path.Combine(hydrationRoot, "template-hydration-ledger.jsonl");
        var localTemplateRoot = Path.Combine(
            request.InstallRootPath,
            "gel",
            "templates",
            "public-standard",
            safeTemplateId,
            templateVersion);
        var manifestPath = Path.Combine(localTemplateRoot, "template-manifest.json");
        var rootAtlasPath = Path.Combine(localTemplateRoot, "root-atlas.json");
        var templateBodyPath = Path.Combine(localTemplateRoot, "template-body.json");
        var lispBodyPath = Path.Combine(localTemplateRoot, "template-body.sli.lisp");
        var sourcePaths = new[]
        {
            "public/templates/index.json",
            "public/templates/channels/stable.json",
            "public/templates/public-standard/0.1.0/template-manifest.json",
            "public/templates/public-standard/0.1.0/root-atlas.json",
            "public/templates/public-standard/0.1.0/template-body.json",
            "public/templates/public-standard/0.1.0/template-body.sli.lisp"
        };

        var manifest = new
        {
            schema = "project-sanctuary.public-template-manifest.v1",
            templateId,
            version = templateVersion,
            channel = templateChannel,
            hydratedAtUtc = timestamp,
            hydratedByCmeId = request.CmeId,
            threadBindingId = request.ThreadBindingId,
            sourceMode = "repo-static-template-service",
            sourcePaths,
            importsLabSanctuaryGel = false,
            importsPrivateRootAtlas = false,
            admitsGel = false,
            mutatesSelfGel = false,
            activatesActual = false,
            providerCallAllowed = false,
            modelBindingAllowed = false,
            externalActionAllowed = false
        };
        var rootAtlas = new
        {
            schema = "project-sanctuary.public-root-atlas.v1",
            atlasId = "PublicStandard.RootAtlas",
            templateId,
            version = templateVersion,
            atlasKind = "symbolic-polyglot-stem-research-carrier",
            languageSurfaces = PublicTemplateLanguageSurfaces,
            stemSurfaces = PublicTemplateStemSurfaces,
            installIndividuationRequired = true,
            labOverlayIsProductDefault = false,
            everyLanguageBecomesTypedSurface = true,
            stemDomainTransportIsProofTransfer = false,
            candidateOnly = true
        };
        var templateBody = new
        {
            schema = "project-sanctuary.public-template-body.v1",
            templateId,
            version = templateVersion,
            templateBodyKind = "PublicResearchCME",
            templateIsIdentity = false,
            cmeIdentityPattern = "{Researcher}.CME.ID",
            cmeActualIsStateNotIdentity = true,
            sharedPrimeRealityLayer = "Sanctuary.Actual.weather-system",
            installLocalIndividuationRequired = true,
            labOverlayAllowedAfterGenericHydration = true,
            labOverlayIsNotProductDefault = true,
            languageSurfaceCount = PublicTemplateLanguageSurfaces.Length,
            stemSurfaceCount = PublicTemplateStemSurfaces.Length,
            denials = new
            {
                admitsGelByTemplate = false,
                mutatesSelfGelByTemplate = false,
                grantsAuthorityByTemplate = false,
                activatesCmeActualByTemplate = false,
                activatesSanctuaryActualByTemplate = false,
                bindsProviderOrModelByTemplate = false,
                authorizesExternalActionByTemplate = false,
                claimsPersonhoodByTemplate = false,
                claimsSovereigntyByTemplate = false
            }
        };

        WriteJsonFile(manifestPath, manifest);
        WriteJsonFile(rootAtlasPath, rootAtlas);
        WriteJsonFile(templateBodyPath, templateBody);
        WriteTextFile(lispBodyPath, BuildPublicTemplateHydrationLisp(templateId, templateVersion));

        var register = new
        {
            schema = "project-sanctuary.cgel.template-hydration.v1",
            createdAtUtc = timestamp,
            cmeId = request.CmeId,
            callerCmeId = string.IsNullOrWhiteSpace(request.CallerCmeId) ? request.CmeId : request.CallerCmeId,
            threadBindingId = request.ThreadBindingId,
            serviceIdentityId = request.ServiceIdentityId,
            requestedIdentityTemplateId = request.IdentityTemplateId,
            publicTemplateId = templateId,
            publicTemplateVersion = templateVersion,
            publicTemplateChannel = templateChannel,
            sourceMode = "repo-static-template-service",
            sourcePaths,
            localTemplateRoot,
            manifestPath,
            rootAtlasPath,
            templateBodyPath,
            lispBodyPath,
            languageSurfaces = PublicTemplateLanguageSurfaces,
            stemSurfaces = PublicTemplateStemSurfaces,
            languageSurfaceCount = PublicTemplateLanguageSurfaces.Length,
            stemSurfaceCount = PublicTemplateStemSurfaces.Length,
            candidateOnly = true,
            hydrationFetchPerformed = false,
            localGelScaffoldWritten = true,
            publicTemplateCanBeServedByGitPages = true,
            labOverlayIsProductDefault = false,
            importsLabSanctuaryGel = false,
            importsPrivateRootAtlas = false,
            admitsGel = false,
            mutatesSelfGel = false,
            admitsContinuity = false,
            grantsAuthority = false,
            activatesCmeActual = false,
            activatesSanctuaryActual = false,
            providerCalled = false,
            modelBound = false,
            externalActionAuthorized = false,
            claimsPersonhood = false,
            claimsSovereignty = false
        };
        var registerDigest = Digest(JsonSerializer.Serialize(register, JsonOptions));
        WriteJsonFile(registerPath, register);
        WriteTextFile(registerLispPath, BuildTemplateHydrationRegisterLisp(request, templateId, templateVersion, PublicTemplateLanguageSurfaces.Length, PublicTemplateStemSurfaces.Length));
        AppendJsonLine(
            ledgerPath,
            JsonSerializer.Serialize(
                new
                {
                    schema = "project-sanctuary.cgel.template-hydration-ledger-event.v1",
                    eventType = "template-hydration-written",
                    timestampUtc = timestamp,
                    cmeId = request.CmeId,
                    templateId,
                    templateVersion,
                    registerDigest,
                    importsLabSanctuaryGel = false,
                    admitsGel = false,
                    activatesActual = false
                },
                JsonOptions));

        evidence["templateHydrationSchema"] = "project-sanctuary.cgel.template-hydration.v1";
        evidence["templateHydrationPublicTemplateId"] = templateId;
        evidence["templateHydrationPublicTemplateVersion"] = templateVersion;
        evidence["templateHydrationPublicTemplateChannel"] = templateChannel;
        evidence["templateHydrationSourceMode"] = "repo-static-template-service";
        evidence["templateHydrationSourcePaths"] = sourcePaths;
        evidence["templateHydrationFetchPerformed"] = false;
        evidence["templateHydrationLocalGelScaffoldWritten"] = true;
        evidence["templateHydrationPublicTemplateCanBeServedByGitPages"] = true;
        evidence["templateHydrationLabOverlayIsProductDefault"] = false;
        evidence["templateHydrationRegisterPath"] = registerPath;
        evidence["templateHydrationLispPath"] = registerLispPath;
        evidence["templateHydrationLedgerPath"] = ledgerPath;
        evidence["templateHydrationLocalTemplateRootPath"] = localTemplateRoot;
        evidence["templateHydrationManifestPath"] = manifestPath;
        evidence["templateHydrationRootAtlasPath"] = rootAtlasPath;
        evidence["templateHydrationTemplateBodyPath"] = templateBodyPath;
        evidence["templateHydrationTemplateLispPath"] = lispBodyPath;
        evidence["templateHydrationLanguageSurfaceCount"] = PublicTemplateLanguageSurfaces.Length;
        evidence["templateHydrationStemSurfaceCount"] = PublicTemplateStemSurfaces.Length;
        evidence["templateHydrationEveryLanguageBecomesTypedSurface"] = true;
        evidence["templateHydrationStemTransportIsProofTransfer"] = false;
        evidence["templateHydrationImportsLabSanctuaryGel"] = false;
        evidence["templateHydrationImportsPrivateRootAtlas"] = false;
        evidence["templateHydrationAdmitsGel"] = false;
        evidence["templateHydrationMutatesSelfGel"] = false;
        evidence["templateHydrationAdmitsContinuity"] = false;
        evidence["templateHydrationGrantsAuthority"] = false;
        evidence["templateHydrationActivatesCmeActual"] = false;
        evidence["templateHydrationActivatesSanctuaryActual"] = false;
        evidence["templateHydrationCallsProvider"] = false;
        evidence["templateHydrationBindsModel"] = false;
        evidence["templateHydrationAuthorizesExternalAction"] = false;
        evidence["templateHydrationClaimsPersonhood"] = false;
        evidence["templateHydrationClaimsSovereignty"] = false;
        evidence["templateHydrationDigest"] = registerDigest;
        evidence["templateHydrationSourceReadiness"] = sourcePaths
            .Select(path => BuildSurfaceReadiness($"template-source:{path}", path))
            .ToArray();
    }

    private static string BuildPublicTemplateHydrationLisp(string templateId, string templateVersion) =>
        $$"""
        (public-standard-cme-template
          (:schema "project-sanctuary.public-template-body.v1")
          (:template-id "{{LispString(templateId)}}")
          (:version "{{LispString(templateVersion)}}")
          (:template-is-identity false)
          (:install-local-individuation-required true)
          (:lab-overlay-is-product-default false)
          (:every-language-becomes-typed-surface true)
          (:stem-domain-transport-is-proof-transfer false)
          (:denials
            (:admits-gel false)
            (:mutates-selfgel false)
            (:grants-authority false)
            (:activates-cme-actual false)
            (:activates-sanctuary-actual false)
            (:provider-call false)
            (:model-binding false)
            (:external-action false)))
        """;

    private static string BuildTemplateHydrationRegisterLisp(
        SanctuaryRequest request,
        string templateId,
        string templateVersion,
        int languageSurfaceCount,
        int stemSurfaceCount)
    {
        var builder = new StringBuilder();
        builder.AppendLine("(template-hydration-register");
        builder.AppendLine("  :schema \"project-sanctuary.sli.lisp.template-hydration.v1\"");
        builder.AppendLine($"  :cme-id \"{LispString(request.CmeId)}\"");
        builder.AppendLine($"  :thread-binding-id \"{LispString(request.ThreadBindingId)}\"");
        builder.AppendLine($"  :template-id \"{LispString(templateId)}\"");
        builder.AppendLine($"  :version \"{LispString(templateVersion)}\"");
        builder.AppendLine($"  :language-surface-count {languageSurfaceCount}");
        builder.AppendLine($"  :stem-surface-count {stemSurfaceCount}");
        builder.AppendLine("  :source-mode \"repo-static-template-service\"");
        builder.AppendLine("  :fetch-performed false");
        builder.AppendLine("  :local-gel-scaffold-written true");
        builder.AppendLine("  :lab-overlay-is-product-default false");
        builder.AppendLine("  :candidate-only true");
        builder.AppendLine("  :imports-lab-sanctuary-gel false");
        builder.AppendLine("  :imports-private-root-atlas false");
        builder.AppendLine("  :admits-gel false");
        builder.AppendLine("  :mutates-selfgel false");
        builder.AppendLine("  :activates-actual false)");
        return builder.ToString();
    }
}
