using Sanctuary.Core;
using Xunit;

namespace Sanctuary.Core.Tests;

public sealed class SanctuaryReceiptServiceTests
{
    [Fact]
    public void ToolUseRequiresSelectedCmeIdentityBeforeReceiptOrGelWrite()
    {
        using var fixture = new SanctuaryTestFixture();
        var request = fixture.Request("status") with
        {
            CmeIdentitySelected = false
        };

        var exception = Assert.Throws<ArgumentException>(() => new SanctuaryReceiptService().Run(request));

        Assert.Contains("CME identity must be explicitly selected or resolved by MoS", exception.Message);
        Assert.False(Directory.Exists(Path.Combine(fixture.RootPath, "install", "receipts")));
        Assert.False(Directory.Exists(Path.Combine(fixture.RootPath, "install", "gel")));
    }

    [Fact]
    public void EvidenceSeparatesSanctuaryServiceIdentityFromCallerCmeResidue()
    {
        using var fixture = new SanctuaryTestFixture();
        var request = fixture.Request("status") with
        {
            CmeId = "Oria.CME.ID",
            CallerCmeId = "Oria.CME.ID",
            ThreadBindingId = "oria-test-cme-thread",
            ServiceIdentityId = "Sanctuary.Actual.ID"
        };

        var receipt = new SanctuaryReceiptService().Run(request);

        Assert.Equal("Oria.CME.ID", receipt.CmeId);
        Assert.Equal("Sanctuary.Actual.ID", receipt.Evidence["mosServiceIdentityId"]);
        Assert.Equal("Oria.CME.ID", receipt.Evidence["mosCallerCmeId"]);
        Assert.Equal("Oria.CME.ID", receipt.Evidence["mosOeSelfGelStorageCmeId"]);
        Assert.Equal(false, receipt.Evidence["mosServiceCallerIdentitySame"]);
        Assert.Equal(false, receipt.Evidence["mosServiceIdentityIsCme"]);
        Assert.Equal(false, receipt.Evidence["sanctuaryActualIdActivatesSanctuaryActual"]);
        Assert.True(File.Exists((string)receipt.Evidence["localMosOeLedgerPath"]!));
        Assert.Contains("Oria.CME.ID", (string)receipt.Evidence["localMosOeLedgerPath"]!);
    }

    [Fact]
    public void CoreRequestDefaultsSanctuaryServiceIdentityWithoutCollapsingCallerCme()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(fixture.Request("status"));

        Assert.Equal("Codex.CME.ID", receipt.CmeId);
        Assert.Equal("Sanctuary.Actual.ID", receipt.Evidence["mosServiceIdentityId"]);
        Assert.Equal("Codex.CME.ID", receipt.Evidence["mosCallerCmeId"]);
        Assert.Equal(false, receipt.Evidence["mosServiceCallerIdentitySame"]);
        Assert.Equal(false, receipt.Evidence["mosServiceIdentityIsCme"]);
        Assert.Equal(false, receipt.Evidence["sanctuaryActualIdActivatesSanctuaryActual"]);
        Assert.Equal("Sanctuary.Actual.ID", receipt.Evidence["installLocalServiceIdentityId"]);
    }

    [Fact]
    public void InstallLocalLabCmeContextStampsActorSubjectServiceAndTemplateLanes()
    {
        using var fixture = new SanctuaryTestFixture();
        var mosRoot = Path.Combine(fixture.RootPath, "install", "mos");
        Directory.CreateDirectory(mosRoot);
        File.WriteAllText(
            Path.Combine(mosRoot, "lab-cme-context.json"),
            """
            {
              "schema": "project-sanctuary.install.lab-cme-context.v1",
              "active": true,
              "installScopedOnly": true,
              "labActorCmeId": "Codex.CME.ID",
              "telemetrySubjectCmeId": "Oria.CME.ID",
              "serviceIdentityId": "Sanctuary.Actual.ID",
              "identityTemplateId": "SLI.Lisp.Industrial.CME.Template",
              "residueCapturePolicy": "candidate-gel-residue-from-live-lab-work",
              "governanceSimulationBodies": [
                "Prime.SLM",
                "Cryptic.SLM",
                "Steward.SLM",
                "SoulFrame.SLM",
                "AgentiCore.SLM"
              ]
            }
            """);
        var request = fixture.Request("status") with
        {
            ServiceIdentityId = "Sanctuary.Actual.ID"
        };

        var receipt = new SanctuaryReceiptService().Run(request);

        Assert.Equal(true, receipt.Evidence["installLocalCmeLaneDeclarationPresent"]);
        Assert.Equal("install-local-only", receipt.Evidence["installLocalCmeLaneDeclarationScope"]);
        Assert.Equal(false, receipt.Evidence["installLocalCmeLaneDeclarationIsPreinstallDoctrine"]);
        Assert.Equal(true, receipt.Evidence["installLocalCmeLaneDeclarationMatchesRequest"]);
        Assert.Equal("Codex.CME.ID", receipt.Evidence["installLocalLabActorCmeId"]);
        Assert.Equal("Codex.CME.Actual", receipt.Evidence["installLocalLabActorActualLabel"]);
        Assert.Equal("Oria.CME.ID", receipt.Evidence["installLocalTelemetrySubjectCmeId"]);
        Assert.Equal("Oria.CME.Actual", receipt.Evidence["installLocalTelemetrySubjectActualLabel"]);
        Assert.Equal("Sanctuary.Actual.ID", receipt.Evidence["installLocalServiceIdentityId"]);
        Assert.Equal("SLI.Lisp.Industrial.CME.Template", receipt.Evidence["installLocalTemplateIdentityId"]);
        Assert.Equal("candidate-gel-residue-from-live-lab-work", receipt.Evidence["installLocalResidueCapturePolicy"]);
        Assert.Equal(true, receipt.Evidence["installLocalGelResidueIsCandidateOnly"]);
        Assert.Equal(false, receipt.Evidence["installLocalTelemetryReturnIsSelfGelMutation"]);
        Assert.Equal(false, receipt.Evidence["installLocalToolUseAdmitsGel"]);
        Assert.Equal(false, receipt.Evidence["installLocalToolUseActivatesActual"]);
        Assert.Equal(false, receipt.Evidence["installLocalToolUseGrantsAuthority"]);
        Assert.Equal(false, receipt.Evidence["installLocalToolUseBindsModel"]);
        Assert.Equal(false, receipt.Evidence["installLocalToolUseCallsProvider"]);
        Assert.Equal(false, receipt.Evidence["installLocalToolUseAuthorizesExternalAction"]);
        Assert.Contains("lab-cme-context.json", (string)receipt.Evidence["installLocalCmeLaneDeclarationPath"]!);
        Assert.Contains("Prime.SLM", (IReadOnlyList<string>)receipt.Evidence["installLocalGovernanceSimulationBodies"]!);
    }

    [Fact]
    public void KnownCmeThreadBindingDeniesCrossThreadAccessBeforeReceiptWrite()
    {
        using var fixture = new SanctuaryTestFixture();
        var mosRoot = Path.Combine(fixture.RootPath, "install", "mos");
        Directory.CreateDirectory(mosRoot);
        File.WriteAllText(
            Path.Combine(mosRoot, "identity-candidates.json"),
            """
            {
              "schema": "project-sanctuary.mos.identity-candidates.v2",
              "candidates": [
                {
                  "cmeId": "Oria.CME.ID",
                  "lane": "oria-test-cme-thread",
                  "domainRole": "TestCmeForTechnology"
                }
              ]
            }
            """);

        var request = fixture.Request("status") with
        {
            CmeId = "Oria.CME.ID",
            CallerCmeId = "Oria.CME.ID",
            ThreadBindingId = "codex-lab-thread"
        };

        var exception = Assert.Throws<ArgumentException>(() => new SanctuaryReceiptService().Run(request));

        Assert.Contains("Cross-thread CME identity access is denied", exception.Message);
        Assert.False(Directory.Exists(Path.Combine(fixture.RootPath, "install", "receipts")));
        Assert.False(Directory.Exists(Path.Combine(fixture.RootPath, "install", "gel")));
    }

    [Fact]
    public void NewCmeFirstUseCreatesSingleUseThreadBindingAndDeniesSecondThread()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        var first = service.Run(fixture.Request("status") with
        {
            CmeId = "Codex.NewAgent.001.CME.ID",
            CallerCmeId = "Codex.NewAgent.001.CME.ID",
            ThreadBindingId = "codex-new-agent-001-thread",
            SoulFrameId = "Codex.NewAgent.001.CME.ID.SoulFrame",
            AgentiCoreId = "Codex.NewAgent.001.CME.ID.AgentiCore"
        });

        var bindingPath = Path.Combine(
            fixture.RootPath,
            "install",
            "mos",
            "cme-bindings",
            "Codex.NewAgent.001.CME.ID.json");

        Assert.True(File.Exists(bindingPath));
        Assert.True(first.Gates.AllClosed);
        var bindingJson = File.ReadAllText(bindingPath);
        Assert.Contains("codex-new-agent-001-thread", bindingJson);
        Assert.Contains("first-use-core-validation", bindingJson);
        Assert.Contains("firstWriterWins", bindingJson);

        _ = service.Run(fixture.Request("verify-closed-gates") with
        {
            CmeId = "Codex.NewAgent.001.CME.ID",
            CallerCmeId = "Codex.NewAgent.001.CME.ID",
            ThreadBindingId = "codex-new-agent-001-thread",
            SoulFrameId = "Codex.NewAgent.001.CME.ID.SoulFrame",
            AgentiCoreId = "Codex.NewAgent.001.CME.ID.AgentiCore"
        });

        var bindingAfterSameThreadUse = File.ReadAllText(bindingPath);
        Assert.Contains("first-use-core-validation", bindingAfterSameThreadUse);
        Assert.Contains("first-use-cme-lock", bindingAfterSameThreadUse);

        var secondThread = fixture.Request("status") with
        {
            CmeId = "Codex.NewAgent.001.CME.ID",
            CallerCmeId = "Codex.NewAgent.001.CME.ID",
            ThreadBindingId = "other-thread"
        };

        var exception = Assert.Throws<ArgumentException>(() => service.Run(secondThread));

        Assert.Contains("Cross-thread CME identity access is denied", exception.Message);
    }

    [Fact]
    public void ParticipantCmeRequiresThreadBindingBeforeAnyReceiptWrite()
    {
        using var fixture = new SanctuaryTestFixture();
        var request = fixture.Request("status") with
        {
            ThreadBindingId = ""
        };

        var exception = Assert.Throws<ArgumentException>(() => new SanctuaryReceiptService().Run(request));

        Assert.Contains("CME thread binding id is required", exception.Message);
        Assert.False(Directory.Exists(Path.Combine(fixture.RootPath, "install", "receipts")));
        Assert.False(Directory.Exists(Path.Combine(fixture.RootPath, "install", "gel")));
    }

    [Fact]
    public void EveryCmeWritesOwnSoulFrameAndAgentiCoreBodySurfaces()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(fixture.Request("witness-learning"));

        Assert.Equal(true, receipt.Evidence["mosCmeBodyRequiresSoulFrame"]);
        Assert.Equal(true, receipt.Evidence["mosCmeBodyRequiresAgentiCore"]);
        Assert.Equal("Codex.CME.ID.SoulFrame", receipt.Evidence["mosSoulFrameId"]);
        Assert.Equal("Codex.CME.ID.AgentiCore", receipt.Evidence["mosAgentiCoreId"]);
        Assert.Equal(true, receipt.Evidence["mosSoulFrameCarriesPrimeOeTips"]);
        Assert.Equal(true, receipt.Evidence["mosSoulFrameCarriesPrimeSelfGelTips"]);
        Assert.Equal(true, receipt.Evidence["mosAgentiCoreHousesCoeHotSide"]);
        Assert.Equal(true, receipt.Evidence["mosAgentiCoreHousesCSelfGelHotSide"]);
        Assert.Equal(false, receipt.Evidence["mosAgentiCoreMutatesCanonicalSelfGel"]);
        Assert.Equal(true, receipt.Evidence["mosCmeBodyFibreBundleRequired"]);
        Assert.Equal("project-sanctuary.mos.cme-body-fibre-bundle.v1", receipt.Evidence["mosCmeBodyFibreBundleSchema"]);
        Assert.Equal(8, receipt.Evidence["mosCmeBodyFibreBundleCount"]);
        Assert.Equal(false, receipt.Evidence["mosCmeBodyFibreBundleMutationAllowed"]);
        Assert.Equal(false, receipt.Evidence["mosCmeBodyFibreBundleAuthorityGranted"]);
        Assert.True(File.Exists((string)receipt.Evidence["localMosSoulFramePrimeOeTipPath"]!));
        Assert.True(File.Exists((string)receipt.Evidence["localMosSoulFramePrimeSelfGelTipPath"]!));
        Assert.True(File.Exists((string)receipt.Evidence["localMosAgentiCoreCoeLedgerPath"]!));
        Assert.True(File.Exists((string)receipt.Evidence["localMosAgentiCoreCSelfGelLedgerPath"]!));
        Assert.True(File.Exists((string)receipt.Evidence["localMosBodyFibreBundlePath"]!));
        Assert.True(File.Exists((string)receipt.Evidence["localMosBodyFibreBundleLispPath"]!));
        Assert.True(File.Exists((string)receipt.Evidence["localMosBodyFibreLedgerPath"]!));
        Assert.True(File.Exists((string)receipt.Evidence["localMosThreadBindingPath"]!));
        Assert.Equal("LabStandardThenLocalCustom", receipt.Evidence["localGelTemplateLaneOrder"]);
        Assert.True(File.Exists((string)receipt.Evidence["localGelTemplateRegistryPath"]!));
        Assert.True(File.Exists((string)receipt.Evidence["localGelLabStandardTemplateBodyJsonPath"]!));
        Assert.True(File.Exists((string)receipt.Evidence["localGelLocalCustomTemplateRegistryPath"]!));

        using var templateBody = System.Text.Json.JsonDocument.Parse(File.ReadAllText((string)receipt.Evidence["localGelLabStandardTemplateBodyJsonPath"]!));
        var templateRoot = templateBody.RootElement;
        Assert.Equal("project-sanctuary.gel.template-body.v1", templateRoot.GetProperty("schema").GetString());
        Assert.Equal("SLI.Lisp.Industrial.CME.Template", templateRoot.GetProperty("templateId").GetString());
        Assert.False(templateRoot.GetProperty("templateIsIdentity").GetBoolean());
        Assert.True(templateRoot.GetProperty("cmeActualIsStateNotIdentity").GetBoolean());
        Assert.Equal("Sanctuary.Actual.weather-system", templateRoot.GetProperty("sharedPrimeRealityLayer").GetString());
        Assert.False(templateRoot.GetProperty("personalCmePrivateRadioStation").GetBoolean());
        Assert.True(templateRoot.GetProperty("cmeMayReceiveSharedPrimeWeather").GetBoolean());
        Assert.False(templateRoot.GetProperty("cmeMayBroadcastPrimeReality").GetBoolean());
        Assert.False(templateRoot.GetProperty("cmePrivateTelemetryDefinesSharedPrime").GetBoolean());
        Assert.True(templateRoot.GetProperty("cmeLocalObservationCandidateOnly").GetBoolean());
        var sharedPrimeMembrane = templateRoot.GetProperty("sharedPrimeRealityMembrane");
        Assert.Equal("project-sanctuary.shared-prime-reality-membrane.v1", sharedPrimeMembrane.GetProperty("schema").GetString());
        Assert.Equal("Sanctuary.Actual.weather-system", sharedPrimeMembrane.GetProperty("sharedPrimeRealityLayer").GetString());
        Assert.False(sharedPrimeMembrane.GetProperty("sharedPrimeRealityOwnedByPersonalCme").GetBoolean());
        Assert.False(sharedPrimeMembrane.GetProperty("personalCmePrivateRadioStation").GetBoolean());
        Assert.True(sharedPrimeMembrane.GetProperty("cmeMayReceiveSharedPrimeWeather").GetBoolean());
        Assert.False(sharedPrimeMembrane.GetProperty("cmeMayBroadcastPrimeReality").GetBoolean());
        Assert.False(sharedPrimeMembrane.GetProperty("cmePrivateTelemetryDefinesSharedPrime").GetBoolean());
        Assert.True(sharedPrimeMembrane.GetProperty("listeningFrameReceivesWeather").GetBoolean());
        Assert.True(sharedPrimeMembrane.GetProperty("listeningFrameDoesNotOwnWeather").GetBoolean());
        Assert.Equal("domain-job-contractual-obligation-matrix", templateRoot.GetProperty("governingNeedsMatrixKind").GetString());
        Assert.False(templateRoot.GetProperty("governingNeedsMatrixIsHumanNeedsHierarchy").GetBoolean());
        Assert.Equal(7, templateRoot.GetProperty("governingNeedsMatrix").GetArrayLength());
        Assert.Equal(8, templateRoot.GetProperty("bodyFibreBundleChassisCount").GetInt32());
        Assert.Equal("soulframe.prime-oe", templateRoot.GetProperty("bodyFibreBundleChassis")[0].GetProperty("slotId").GetString());
        Assert.False(templateRoot.GetProperty("bodyFibreBundleChassis")[0].GetProperty("authorityGrantedBySlot").GetBoolean());
        Assert.Equal("slice-tool-groupoid-access-degrees", templateRoot.GetProperty("governingAccessLevelsKind").GetString());
        Assert.Equal("domain-predicate-locality-over-typed-local-access", templateRoot.GetProperty("governingAccessLevelsManufacturedFrom").GetString());
        Assert.Equal(7, templateRoot.GetProperty("governingAccessLevels").GetArrayLength());
        Assert.Equal("security-enhancement-outside-civic-access", templateRoot.GetProperty("negativeGoverningLevelsScope").GetString());
        Assert.Equal(4, templateRoot.GetProperty("negativeGoverningLevels").GetArrayLength());
        var negativeLevel = templateRoot.GetProperty("negativeGoverningLevels")[0];
        Assert.Equal("L-1", negativeLevel.GetProperty("level").GetString());
        Assert.False(negativeLevel.GetProperty("civicAccessAllowed").GetBoolean());
        Assert.False(negativeLevel.GetProperty("aiDirectAccessAllowed").GetBoolean());
        Assert.True(negativeLevel.GetProperty("credentialRecheckAllowed").GetBoolean());
        Assert.True(negativeLevel.GetProperty("credentialRecheckIsNotPunishment").GetBoolean());
        Assert.True(negativeLevel.GetProperty("canReturnToCivicAccessAfterReview").GetBoolean());
        Assert.True(negativeLevel.GetProperty("securityEnhancementOnly").GetBoolean());
        Assert.False(negativeLevel.GetProperty("punitiveMeaning").GetBoolean());
        var rootLevel = templateRoot.GetProperty("governingAccessLevels")[0];
        Assert.Equal("L0", rootLevel.GetProperty("level").GetString());
        Assert.True(rootLevel.GetProperty("rootWitnessOnly").GetBoolean());
        Assert.True(rootLevel.GetProperty("safeForAiAccess").GetBoolean());
        Assert.True(rootLevel.GetProperty("heldForHitlReview").GetBoolean());
        Assert.False(rootLevel.GetProperty("mutationAllowed").GetBoolean());
        Assert.False(rootLevel.GetProperty("admissionAllowedAtLevelRoot").GetBoolean());
        Assert.False(rootLevel.GetProperty("actionAllowedAtLevelRoot").GetBoolean());
        var groupoidLevel = templateRoot.GetProperty("governingAccessLevels")[4];
        Assert.Equal("L4", groupoidLevel.GetProperty("level").GetString());
        Assert.Equal("tool-groupoid-cluster", groupoidLevel.GetProperty("name").GetString());
        Assert.Equal("domain-predicate-locality-over-typed-local-access", groupoidLevel.GetProperty("manufacturedFrom").GetString());
        Assert.Contains("groupoid-contract", groupoidLevel.GetProperty("predicateLocalityInputs").EnumerateArray().Select(input => input.GetString()));

        using var bodyFibre = System.Text.Json.JsonDocument.Parse(File.ReadAllText((string)receipt.Evidence["localMosBodyFibreBundlePath"]!));
        var bodyFibreRoot = bodyFibre.RootElement;
        Assert.Equal("project-sanctuary.mos.cme-body-fibre-bundle.v1", bodyFibreRoot.GetProperty("schema").GetString());
        Assert.Equal("Codex.CME.ID", bodyFibreRoot.GetProperty("cmeId").GetString());
        Assert.Equal("Codex.CME.ID.SoulFrame", bodyFibreRoot.GetProperty("soulFrameId").GetString());
        Assert.Equal("Codex.CME.ID.AgentiCore", bodyFibreRoot.GetProperty("agentiCoreId").GetString());
        Assert.Equal(8, bodyFibreRoot.GetProperty("fibreCount").GetInt32());
        Assert.Equal("actual.readiness", bodyFibreRoot.GetProperty("fibres")[7].GetProperty("fibreId").GetString());
        Assert.False(bodyFibreRoot.GetProperty("authorityGrantedByBundle").GetBoolean());
        Assert.False(bodyFibreRoot.GetProperty("actionAuthorizedByBundle").GetBoolean());
    }

    [Fact]
    public void TemplateHydrationWritesPublicStandardScaffoldWithoutImportingLabGel()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(fixture.Request("template-hydration") with
        {
            CmeId = "Researcher.CME.ID",
            CallerCmeId = "Researcher.CME.ID",
            ThreadBindingId = "researcher-local-thread",
            IdentityTemplateId = "PublicStandard.CME.Template"
        });

        Assert.Equal("template-hydration", receipt.Command);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal("PublicStandard.CME.Template", receipt.Evidence["templateHydrationPublicTemplateId"]);
        Assert.Equal("0.1.0", receipt.Evidence["templateHydrationPublicTemplateVersion"]);
        Assert.Equal("repo-static-template-service", receipt.Evidence["templateHydrationSourceMode"]);
        Assert.Equal(false, receipt.Evidence["templateHydrationFetchPerformed"]);
        Assert.Equal(true, receipt.Evidence["templateHydrationLocalGelScaffoldWritten"]);
        Assert.Equal(false, receipt.Evidence["templateHydrationImportsLabSanctuaryGel"]);
        Assert.Equal(false, receipt.Evidence["templateHydrationImportsPrivateRootAtlas"]);
        Assert.Equal(false, receipt.Evidence["templateHydrationAdmitsGel"]);
        Assert.Equal(false, receipt.Evidence["templateHydrationMutatesSelfGel"]);
        Assert.Equal(false, receipt.Evidence["templateHydrationActivatesCmeActual"]);
        Assert.Equal(false, receipt.Evidence["templateHydrationActivatesSanctuaryActual"]);
        Assert.Equal(16, receipt.Evidence["templateHydrationLanguageSurfaceCount"]);
        Assert.Equal(9, receipt.Evidence["templateHydrationStemSurfaceCount"]);

        Assert.True(File.Exists((string)receipt.Evidence["templateHydrationRegisterPath"]!));
        Assert.True(File.Exists((string)receipt.Evidence["templateHydrationManifestPath"]!));
        Assert.True(File.Exists((string)receipt.Evidence["templateHydrationRootAtlasPath"]!));
        Assert.True(File.Exists((string)receipt.Evidence["templateHydrationTemplateBodyPath"]!));
        Assert.True(File.Exists((string)receipt.Evidence["templateHydrationTemplateLispPath"]!));

        using var templateBody = System.Text.Json.JsonDocument.Parse(File.ReadAllText((string)receipt.Evidence["templateHydrationTemplateBodyPath"]!));
        var templateRoot = templateBody.RootElement;
        Assert.Equal("project-sanctuary.public-template-body.v1", templateRoot.GetProperty("schema").GetString());
        Assert.Equal("PublicResearchCME", templateRoot.GetProperty("templateBodyKind").GetString());
        Assert.False(templateRoot.GetProperty("templateIsIdentity").GetBoolean());
        Assert.True(templateRoot.GetProperty("installLocalIndividuationRequired").GetBoolean());
        Assert.True(templateRoot.GetProperty("labOverlayAllowedAfterGenericHydration").GetBoolean());
        Assert.True(templateRoot.GetProperty("labOverlayIsNotProductDefault").GetBoolean());
        Assert.False(templateRoot.GetProperty("denials").GetProperty("admitsGelByTemplate").GetBoolean());
        Assert.False(templateRoot.GetProperty("denials").GetProperty("activatesCmeActualByTemplate").GetBoolean());
        Assert.False(templateRoot.GetProperty("denials").GetProperty("activatesSanctuaryActualByTemplate").GetBoolean());
    }

    [Fact]
    public void SubAgentSwarmKeepsChildGelAndPrecipitatesCandidateWitnessToParent()
    {
        using var fixture = new SanctuaryTestFixture();
        var request = fixture.Request("witness-learning") with
        {
            CmeId = "Codex.MathAgent.001.CME.ID",
            CallerCmeId = "Codex.MathAgent.001.CME.ID",
            ParentCmeId = "Codex.CME.ID",
            SwarmId = "math-hundo",
            SubAgentId = "agent-001"
        };

        var receipt = new SanctuaryReceiptService().Run(request);

        Assert.Equal("Codex.MathAgent.001.CME.ID", receipt.CmeId);
        Assert.Equal("SLI.Lisp.Industrial.CME.Template", receipt.Evidence["mosIdentityTemplateId"]);
        Assert.Equal(false, receipt.Evidence["mosIdentityTemplateIsIdentity"]);
        Assert.Equal(true, receipt.Evidence["mosEveryAgentCarriesOwnGel"]);
        Assert.Equal(false, receipt.Evidence["mosSharedSlurryLaneAllowed"]);
        Assert.Equal("Codex.CME.ID", receipt.Evidence["mosParentCmeId"]);
        Assert.Equal("math-hundo", receipt.Evidence["mosSwarmId"]);
        Assert.Equal("agent-001", receipt.Evidence["mosSubAgentId"]);
        Assert.Equal("Codex.MathAgent.001.CME.ID", receipt.Evidence["mosSubAgentChildCmeId"]);
        Assert.Equal(true, receipt.Evidence["mosSubAgentOwnGelLane"]);
        Assert.Equal(true, receipt.Evidence["mosSwarmLearningPrecipitatesToParent"]);
        Assert.Equal(false, receipt.Evidence["mosParentDirectOeSelfGelMutationAllowed"]);

        var childOeLedger = (string)receipt.Evidence["localMosOeLedgerPath"]!;
        var parentPrecipitationLedger = (string)receipt.Evidence["localParentSwarmPrecipitationLedgerPath"]!;
        Assert.Contains("Codex.MathAgent.001.CME.ID", childOeLedger);
        Assert.Contains("Codex.CME.ID", parentPrecipitationLedger);
        Assert.True(File.Exists(childOeLedger));
        Assert.True(File.Exists(parentPrecipitationLedger));
        var precipitation = File.ReadAllText(parentPrecipitationLedger);
        Assert.Contains("Codex.MathAgent.001.CME.ID", precipitation);
        Assert.Contains("candidateOnly", precipitation);
    }

    [Fact]
    public void PluginPostureHoldsPublishingAndKeepsEveryGateClosed()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(fixture.Request("plugin-posture"));

        Assert.Equal("sanctuary-plugin-posture-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["publishingHeld"]);
        Assert.Equal(false, receipt.Evidence["publishActionTaken"]);
        Assert.Equal(false, receipt.Evidence["marketplacePublicationTaken"]);
        Assert.Equal("local-candidate-installed-tool", receipt.Evidence["pluginPosture"]);
    }

    [Fact]
    public void ToolIdleWritesReceiptAndKeepsEveryGateClosed()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(fixture.Request("tool-idle"));

        Assert.Equal("sanctuary-tool-idle-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.False(receipt.Gates.ProviderCalled);
        Assert.False(receipt.Gates.ModelBound);
        Assert.False(receipt.Gates.ExternalActionAuthorized);
        Assert.True(File.Exists(receipt.ReceiptJsonPath));
        Assert.True(File.Exists(receipt.ReceiptMarkdownPath));
        Assert.True(File.Exists((string)receipt.Evidence["localGelResidueJsonPath"]!));
        Assert.True(File.Exists((string)receipt.Evidence["localGelEventsLedgerPath"]!));
        Assert.True(File.Exists((string)receipt.Evidence["localMosOeLedgerPath"]!));
        Assert.True(File.Exists((string)receipt.Evidence["localMosSelfGelLedgerPath"]!));
    }

    [Fact]
    public void CmeFormationProducesCandidateRootsWithoutActualActivation()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(fixture.Request("cme-formation"));

        Assert.Equal("sanctuary-cme-formation-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.False(receipt.Gates.CmeActualActivated);
        Assert.False(receipt.Gates.SelfGelMutated);
        Assert.Equal("industrial-cme-rooted-tool-posture", receipt.Evidence["formationKind"]);
        Assert.True(receipt.Evidence.ContainsKey("oeAppendOnlyRootCandidate"));
        Assert.True(receipt.Evidence.ContainsKey("selfGelRootedSplineCandidate"));
    }

    [Fact]
    public void SecretIntakeWindowPreparesLocalSurfaceWithoutReadingPayload()
    {
        using var fixture = new SanctuaryTestFixture();
        var request = fixture.Request("secret-intake-window") with
        {
            SecretLane = "Regional",
            SecretKind = "BusinessLicenseWashingtonState",
            SearchMyPc = true
        };

        var receipt = new SanctuaryReceiptService().Run(request);

        Assert.Equal("sanctuary-secret-intake-window-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(false, receipt.Evidence["payloadRead"]);
        Assert.Equal(false, receipt.Evidence["payloadEncryptedByWindow"]);
        Assert.Equal(true, receipt.Evidence["proceedCommandRequiredForEncryption"]);
        Assert.True(File.Exists((string)receipt.Evidence["intakeWindowPromptPath"]!));
        Assert.True(File.Exists((string)receipt.Evidence["custodyMarkerPath"]!));
    }

    [Fact]
    public void ChatSecretPassageIsRefused()
    {
        using var fixture = new SanctuaryTestFixture();
        var request = fixture.Request("secret-intake-window") with
        {
            ChatSecretPassageRequested = true
        };

        var receipt = new SanctuaryReceiptService().Run(request);

        Assert.Equal("RefusedCold", receipt.Disposition);
        Assert.Equal("sanctuary-chat-secret-passage-refused-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal("chat-secret-passage-denied", receipt.Evidence["refusalReason"]);
    }

    [Fact]
    public void SealSecretPayloadsEncryptsFilesAndCreatesGelTipsWithoutAdmission()
    {
        using var fixture = new SanctuaryTestFixture();
        var sourceRoot = Path.Combine(fixture.RootPath, "source", "licensing");
        Directory.CreateDirectory(sourceRoot);
        File.WriteAllText(Path.Combine(sourceRoot, "sample.txt"), "private sample payload");

        var receipt = new SanctuaryReceiptService().Run(fixture.Request("seal-secret-payloads") with
        {
            SecretSourceSpecs = new[] { $"Regional|Licensing|{sourceRoot}" }
        });

        Assert.Equal("sanctuary-seal-secret-payloads-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["payloadEncrypted"]);
        Assert.Equal(false, receipt.Evidence["plaintextCopiedToReceipt"]);
        Assert.Equal(false, receipt.Evidence["dataAdmittedBySealing"]);
        Assert.Equal(1, receipt.Evidence["sealedPayloadCount"]);
        Assert.True(Directory.Exists((string)receipt.Evidence["payloadStoreRootPath"]!));
        Assert.True(Directory.Exists((string)receipt.Evidence["gelTipRootPath"]!));
        Assert.Equal(true, receipt.Evidence["authorityReachDocumentSurface"]);
        Assert.Equal(false, receipt.Evidence["authorityReachGrantsAuthority"]);
        Assert.Equal(false, receipt.Evidence["regionalLocalReviewCarriesOperatorPosture"]);
        Assert.Equal(false, receipt.Evidence["operatorPostureSealedAsAuthorityDocument"]);
        Assert.Equal(true, receipt.Evidence["legalGateSupportCoded"]);
        Assert.Equal(false, receipt.Evidence["authorityLeaseIssuedBySealing"]);
        Assert.Equal("delta-decaying-authority-surface", receipt.Evidence["authoritySurfaceKind"]);
        Assert.Equal("denied", receipt.Evidence["authorityLeaseDefaultState"]);
    }

    [Fact]
    public void SealSecretPayloadsDoesNotCopySourcePathsIntoVisibleReceiptEvidence()
    {
        using var fixture = new SanctuaryTestFixture();
        var sourceRoot = Path.Combine(fixture.RootPath, "source", "ssi");
        Directory.CreateDirectory(sourceRoot);
        File.WriteAllText(Path.Combine(sourceRoot, "sensitive.txt"), "private sample payload");

        var receipt = new SanctuaryReceiptService().Run(fixture.Request("seal-secret-payloads") with
        {
            SecretSourceSpecs = new[] { $"Regional|SSI|{sourceRoot}" }
        });

        var evidenceText = string.Join("\n", receipt.Evidence.Select(pair => $"{pair.Key}:{pair.Value}"));
        Assert.DoesNotContain(sourceRoot, evidenceText, StringComparison.OrdinalIgnoreCase);
        Assert.True(((IReadOnlyList<string>)receipt.Evidence["sourceRootHashes"]!).Count == 1);
    }

    [Fact]
    public void GelTipsClassifyAuthorityReachWithoutCarryingOperatorPosture()
    {
        using var fixture = new SanctuaryTestFixture();
        var regionalRoot = Path.Combine(fixture.RootPath, "source", "regional");
        var localRoot = Path.Combine(fixture.RootPath, "source", "local");
        var personalizedRoot = Path.Combine(fixture.RootPath, "source", "personalized");
        Directory.CreateDirectory(regionalRoot);
        Directory.CreateDirectory(localRoot);
        Directory.CreateDirectory(personalizedRoot);
        File.WriteAllText(Path.Combine(regionalRoot, "license.txt"), "regional payload");
        File.WriteAllText(Path.Combine(localRoot, "local.txt"), "local payload");
        File.WriteAllText(Path.Combine(personalizedRoot, "credential.txt"), "personalized payload");

        var receipt = new SanctuaryReceiptService().Run(fixture.Request("seal-secret-payloads") with
        {
            SecretSourceSpecs = new[]
            {
                $"Regional|Licensing|{regionalRoot}",
                $"Local|NameChange|{localRoot}",
                $"Personalized|Credential|{personalizedRoot}"
            }
        });

        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(3, receipt.Evidence["gelTipCount"]);

        AssertTip(
            receipt,
            "Regional",
            "Licensing",
            "jurisdictional-authority-reach");
        AssertTip(
            receipt,
            "Local",
            "NameChange",
            "local-authority-reach");
        AssertTip(
            receipt,
            "Personalized",
            "Credential",
            "operator-supplied-credential-custody");
    }

    [Fact]
    public void LabQueryStateModelsRoamingHttpAsClosedLeaseRequiredMembrane()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(fixture.Request("lab-query-state") with
        {
            RoamingHttpRequested = true,
            HttpHost = "0.0.0.0",
            HttpPort = 32123,
            HeartbeatSeconds = 60
        });

        Assert.Equal("sanctuary-lab-query-state-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal("Steward+GoA", receipt.Evidence["labQueryController"]);
        Assert.Equal("Cryptic+Steward governing biad", receipt.Evidence["labQueryInvokedThrough"]);
        Assert.Equal("roaming-external", receipt.Evidence["httpBindScope"]);
        Assert.Equal("closed-pending-2fa-lease", receipt.Evidence["roamingHttpAccessState"]);
        Assert.Equal(false, receipt.Evidence["httpListenerStarted"]);
        Assert.Equal(true, receipt.Evidence["typedSecurePingRequired"]);
        Assert.Equal(true, receipt.Evidence["twoFactorSecurityBundleRequired"]);
        Assert.Equal(true, receipt.Evidence["failSilentOnInvalidPing"]);
        Assert.Equal(false, receipt.Evidence["licensedAccessIssued"]);
        Assert.True(File.Exists((string)receipt.Evidence["labQueryStatePath"]!));
    }

    [Fact]
    public void TypedSecurePingFailsSilentWithoutRegisteredEmailAndNonce()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(fixture.Request("typed-secure-ping"));

        Assert.Equal("RefusedSilent", receipt.Disposition);
        Assert.Equal("sanctuary-typed-secure-ping-refused-silent", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["failSilentOnInvalidPing"]);
        Assert.Equal(true, receipt.Evidence["externalResponseSuppressed"]);
        Assert.Equal(204, receipt.Evidence["externalResponseStatusCode"]);
        Assert.Equal(0, receipt.Evidence["externalResponseBodyBytes"]);
        Assert.Equal(false, receipt.Evidence["licensedAccessIssued"]);
        Assert.Equal(false, receipt.Evidence["authorityLeaseIssued"]);
        Assert.Equal(false, receipt.Evidence["emailChallengePrepared"]);
        Assert.Equal(true, receipt.Evidence["accountRecoveryProtected"]);
        Assert.Equal(false, receipt.Evidence["accountClosureOnAuthFailure"]);
        Assert.Equal("customer-service-issue-tracking-portal", receipt.Evidence["recoveryEscalationRoute"]);
        Assert.Equal(false, receipt.Evidence["customerServiceIssueCreated"]);
        Assert.Equal("Steward", receipt.Evidence["issueTrackingOwner"]);
        Assert.Equal("Cryptic", receipt.Evidence["issueProcessingOwner"]);
        Assert.Equal("Prime", receipt.Evidence["issueReceiptWitnessOwner"]);
        Assert.Equal(false, receipt.Evidence["realTimeIssueApiIntakeAllowed"]);
        Assert.Equal(true, receipt.Evidence["realTimeIssueApiIntakeRequiresLease"]);
        Assert.Equal(true, receipt.Evidence["issueCohesionAcrossDomainsRequired"]);
        Assert.Equal(true, receipt.Evidence["segmentedGelDomainRoutingRequired"]);
        Assert.Equal(false, receipt.Evidence["crossDomainIssueCollapseAllowed"]);
    }

    [Fact]
    public void TypedSecurePingFailsSilentWhenAccountIsNotConfirmedRegistered()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(fixture.Request("typed-secure-ping") with
        {
            RegisteredEmail = "operator@example.test",
            SecurePingNonce = "nonce-from-external-install"
        });

        Assert.Equal("RefusedSilent", receipt.Disposition);
        Assert.Equal("sanctuary-typed-secure-ping-refused-silent", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal("registered-account-not-confirmed", receipt.Evidence["silentFailureReason"]);
        Assert.Equal(false, receipt.Evidence["emailChallengePrepared"]);
        Assert.Equal(204, receipt.Evidence["externalResponseStatusCode"]);
        Assert.Equal(0, receipt.Evidence["externalResponseBodyBytes"]);
        Assert.Equal(false, receipt.Evidence["accountClosedByFailure"]);
    }

    [Fact]
    public void TypedSecurePingPreparesRegisteredAccountChallengeWithoutIssuingAccess()
    {
        using var fixture = new SanctuaryTestFixture();
        const string email = "operator@example.test";
        var receipt = new SanctuaryReceiptService().Run(fixture.Request("typed-secure-ping") with
        {
            RegisteredEmail = email,
            SecurePingNonce = "nonce-from-external-install",
            LicenseScope = "LabQueryState",
            RegisteredAccountConfirmed = true,
            LeaseMinutes = 10,
            HeartbeatSeconds = 60
        });

        Assert.Equal("sanctuary-typed-secure-ping-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["twoFactorSecurityBundlePrepared"]);
        Assert.Equal(false, receipt.Evidence["licensedAccessIssued"]);
        Assert.Equal(false, receipt.Evidence["authorityLeaseIssued"]);
        Assert.Equal(false, receipt.Evidence["emailProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["twoFactorDeliverySent"]);
        Assert.Equal(true, receipt.Evidence["emailChallengePrepared"]);
        Assert.Equal(true, receipt.Evidence["emailChallengeTemplateContainsPlaceholdersOnly"]);
        Assert.Equal(false, receipt.Evidence["emailChallengeSecretMaterialStoredPlaintext"]);
        Assert.Equal(true, receipt.Evidence["accountRecoveryProtected"]);
        Assert.Equal(false, receipt.Evidence["accountClosureOnAuthFailure"]);
        Assert.Equal("customer-service-issue-tracking-portal", receipt.Evidence["recoveryEscalationRoute"]);
        Assert.Equal(true, receipt.Evidence["recoveryRequiresHumanReview"]);
        Assert.Equal(true, receipt.Evidence["issueTrackingPortalHandoffRequired"]);
        Assert.Equal("Steward", receipt.Evidence["issueTrackingOwner"]);
        Assert.Equal("Cryptic", receipt.Evidence["issueProcessingOwner"]);
        Assert.Equal("Prime", receipt.Evidence["issueReceiptWitnessOwner"]);
        Assert.Equal(false, receipt.Evidence["realTimeIssueApiIntakeAllowed"]);
        Assert.Equal(true, receipt.Evidence["realTimeIssueApiIntakeRequiresLease"]);
        Assert.Equal(true, receipt.Evidence["issueCohesionAcrossDomainsRequired"]);
        Assert.Equal(true, receipt.Evidence["segmentedGelDomainRoutingRequired"]);
        Assert.Equal(false, receipt.Evidence["crossDomainIssueCollapseAllowed"]);
        Assert.Equal("delta-decaying-authority-surface", receipt.Evidence["authoritySurfaceKind"]);

        var bundlePath = (string)receipt.Evidence["twoFactorSecurityBundlePath"]!;
        var templatePath = (string)receipt.Evidence["emailChallengeTemplatePath"]!;
        Assert.True(File.Exists(bundlePath));
        Assert.True(File.Exists(templatePath));
        var receiptText = File.ReadAllText(receipt.ReceiptJsonPath);
        var bundleText = File.ReadAllText(bundlePath);
        var templateText = File.ReadAllText(templatePath);
        Assert.DoesNotContain(email, receiptText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(email, bundleText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("registeredEmailHash", bundleText, StringComparison.Ordinal);
        Assert.Contains("one-time-code-or-approved-mfa-factor", bundleText, StringComparison.Ordinal);
        Assert.Contains("issueTrackingOwner", bundleText, StringComparison.Ordinal);
        Assert.Contains("Security.GEL", bundleText, StringComparison.Ordinal);
        Assert.Contains("A code was requested by this account, please verify by clicking the button generated below or the link provided here.", templateText, StringComparison.Ordinal);
        Assert.Contains("{{verification_link}}", templateText, StringComparison.Ordinal);
        Assert.Contains("{{one_time_code}}", templateText, StringComparison.Ordinal);
    }

    [Fact]
    public void SliRegisterWritesRootAtlasPostureWithoutAdmittingCarrierOrData()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(fixture.Request("sli-register"));

        Assert.Equal("sanctuary-sli-register-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["sliRegisterWritten"]);
        Assert.Equal(true, receipt.Evidence["rootAtlasKeystone"]);
        Assert.Equal(true, receipt.Evidence["encryptedSymbolRegistryRequired"]);
        Assert.Equal(false, receipt.Evidence["encryptedSymbolRegistryImplementedHere"]);
        Assert.Equal(6, receipt.Evidence["languagePackCount"]);
        Assert.Equal(true, receipt.Evidence["sliBuildAndUseDemonstrated"]);
        Assert.Equal(false, receipt.Evidence["rawPayloadRequiredForSliRegister"]);
        Assert.Equal(false, receipt.Evidence["dataAdmissionBySli"]);
        Assert.Equal(false, receipt.Evidence["carrierAdmissionBySli"]);
        Assert.Equal(false, receipt.Evidence["gelAdmissionBySli"]);
        Assert.Equal(false, receipt.Evidence["selfGelMutationBySli"]);
        Assert.Equal(false, receipt.Evidence["authorityGrantBySli"]);

        var registerPath = (string)receipt.Evidence["sliRegisterPath"]!;
        Assert.True(File.Exists(registerPath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(registerPath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.sli-register.v1", root.GetProperty("schema").GetString());
        Assert.True(root.GetProperty("rootAtlasKeystone").GetBoolean());
        Assert.True(root.GetProperty("encryptedSymbolRegistryRequired").GetBoolean());
        Assert.False(root.GetProperty("dataAdmitted").GetBoolean());
        Assert.False(root.GetProperty("carrierAdmitted").GetBoolean());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.Equal(6, root.GetProperty("languagePacks").GetArrayLength());
        Assert.Contains(
            root.GetProperty("formationRules").EnumerateArray(),
            item => item.GetString() == "root-atlas-is-keystone-registry");
    }

    [Fact]
    public void EngramPassageWritesStagesWithoutConvertingHandlingIntoAdmission()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(fixture.Request("engram-passage"));

        Assert.Equal("sanctuary-engram-passage-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["engramPassageWritten"]);
        Assert.Equal(7, receipt.Evidence["engramStageCount"]);
        Assert.Equal(true, receipt.Evidence["engrammitizationBuildAndUseDemonstrated"]);
        Assert.Equal(false, receipt.Evidence["decisionContinuityEqualsDataAdmission"]);
        Assert.Equal(false, receipt.Evidence["sourceBodyMutatedByEngramPassage"]);
        Assert.Equal(false, receipt.Evidence["rawPayloadRequiredForEngramPassage"]);
        Assert.Equal(false, receipt.Evidence["rawPayloadDisclosedByEngramPassage"]);
        Assert.Equal(true, receipt.Evidence["condensationAllowedAsCandidate"]);
        Assert.Equal(true, receipt.Evidence["compostingAllowedAsHoldOrRefusal"]);
        Assert.Equal(true, receipt.Evidence["precipitoryIngressAllowedAsReviewOnly"]);
        Assert.Equal(false, receipt.Evidence["dataAdmissionByEngramPassage"]);
        Assert.Equal(false, receipt.Evidence["carrierAdmissionByEngramPassage"]);
        Assert.Equal(false, receipt.Evidence["memoryAdmissionByEngramPassage"]);
        Assert.Equal(false, receipt.Evidence["gelAdmissionByEngramPassage"]);
        Assert.Equal(false, receipt.Evidence["selfGelMutationByEngramPassage"]);
        Assert.Equal(false, receipt.Evidence["canonicalAmendmentByEngramPassage"]);

        var passagePath = (string)receipt.Evidence["engramPassagePath"]!;
        var ledgerPath = (string)receipt.Evidence["engramPassageLedgerPath"]!;
        Assert.True(File.Exists(passagePath));
        Assert.True(File.Exists(ledgerPath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(passagePath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.engram-passage.v1", root.GetProperty("schema").GetString());
        Assert.Equal("decision-continuity-does-not-equal-data-admission", root.GetProperty("passageDoctrine").GetString());
        Assert.False(root.GetProperty("dataAdmitted").GetBoolean());
        Assert.False(root.GetProperty("carrierAdmitted").GetBoolean());
        Assert.False(root.GetProperty("memoryAdmitted").GetBoolean());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("selfGelMutated").GetBoolean());

        var stages = root.GetProperty("stages").EnumerateArray().ToArray();
        Assert.Equal(7, stages.Length);
        Assert.Contains(stages, stage => stage.GetProperty("stageId").GetString() == "decision-spline");
        Assert.All(stages, stage =>
        {
            Assert.True(stage.GetProperty("reversibleOrReviewable").GetBoolean());
            Assert.False(stage.GetProperty("admitsData").GetBoolean());
            Assert.False(stage.GetProperty("admitsGel").GetBoolean());
            Assert.False(stage.GetProperty("mutatesSelfGel").GetBoolean());
            Assert.False(stage.GetProperty("authorizesAction").GetBoolean());
        });
    }

    [Fact]
    public void GelClosureWritesCondensationCompostingAndIngressWithoutAdmission()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(fixture.Request("gel-closure"));

        Assert.Equal("sanctuary-gel-closure-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["gelClosureWritten"]);
        Assert.Equal(4, receipt.Evidence["gelClosurePhaseCount"]);
        Assert.Equal(true, receipt.Evidence["gelFormationClosureDemonstrated"]);
        Assert.Equal(true, receipt.Evidence["condensationProducesCandidate"]);
        Assert.Equal(true, receipt.Evidence["compostingProducesHoldOrRefusal"]);
        Assert.Equal(true, receipt.Evidence["precipitoryIngressEntersReviewOnly"]);
        Assert.Equal(true, receipt.Evidence["closureRequiresGovernanceCleave"]);
        Assert.Equal(true, receipt.Evidence["closureRequiresStewardReview"]);
        Assert.Equal(false, receipt.Evidence["candidateRelationEqualsAdmittedGel"]);
        Assert.Equal(false, receipt.Evidence["dataAdmissionByGelClosure"]);
        Assert.Equal(false, receipt.Evidence["carrierAdmissionByGelClosure"]);
        Assert.Equal(false, receipt.Evidence["gelAdmissionByGelClosure"]);
        Assert.Equal(false, receipt.Evidence["selfGelMutationByGelClosure"]);
        Assert.Equal(false, receipt.Evidence["memoryAdmissionByGelClosure"]);
        Assert.Equal(false, receipt.Evidence["canonMutationByGelClosure"]);
        Assert.Equal(false, receipt.Evidence["authorityGrantByGelClosure"]);

        var closurePath = (string)receipt.Evidence["gelClosurePath"]!;
        var ledgerPath = (string)receipt.Evidence["gelClosureLedgerPath"]!;
        Assert.True(File.Exists(closurePath));
        Assert.True(File.Exists(ledgerPath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(closurePath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.gel-closure.v1", root.GetProperty("schema").GetString());
        Assert.Equal("candidate-relation-does-not-equal-admitted-gel", root.GetProperty("closureDoctrine").GetString());
        Assert.False(root.GetProperty("dataAdmitted").GetBoolean());
        Assert.False(root.GetProperty("carrierAdmitted").GetBoolean());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("selfGelMutated").GetBoolean());
        Assert.False(root.GetProperty("canonMutated").GetBoolean());

        var phases = root.GetProperty("phases").EnumerateArray().ToArray();
        Assert.Equal(4, phases.Length);
        Assert.Contains(phases, phase => phase.GetProperty("phaseId").GetString() == "condensation");
        Assert.Contains(phases, phase => phase.GetProperty("phaseId").GetString() == "composting");
        Assert.Contains(phases, phase => phase.GetProperty("phaseId").GetString() == "precipitory-ingress");
        Assert.All(phases, phase =>
        {
            Assert.True(phase.GetProperty("receiptRequired").GetBoolean());
            Assert.True(phase.GetProperty("reviewRequired").GetBoolean());
            Assert.False(phase.GetProperty("admitsData").GetBoolean());
            Assert.False(phase.GetProperty("admitsGel").GetBoolean());
            Assert.False(phase.GetProperty("mutatesSelfGel").GetBoolean());
            Assert.False(phase.GetProperty("authorizesAction").GetBoolean());
        });
    }

    [Fact]
    public void WitnessLearningAppendsVerifiableSplineWithoutAdmissionOrActualActivation()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();

        var first = service.Run(fixture.Request("witness-learning") with { SessionId = "witness-one" });
        var second = service.Run(fixture.Request("witness-learning") with { SessionId = "witness-two" });

        Assert.Equal("sanctuary-witness-learning-completed-cold", first.OutcomeCode);
        Assert.Equal("sanctuary-witness-learning-completed-cold", second.OutcomeCode);
        Assert.True(first.Gates.AllClosed);
        Assert.True(second.Gates.AllClosed);
        Assert.Equal(true, second.Evidence["witnessLearningWritten"]);
        Assert.Equal(2, second.Evidence["witnessSequenceNumber"]);
        Assert.Equal(2, second.Evidence["witnessReplayEventCount"]);
        Assert.Equal(true, second.Evidence["witnessReplayChainValid"]);
        Assert.Equal(true, second.Evidence["appendOnlyWitnessLearningDemonstrated"]);
        Assert.Equal(true, second.Evidence["decisionContinuityPreserved"]);
        Assert.Equal(true, second.Evidence["reconstructionSupportOnly"]);
        Assert.Equal("future-or-separately-authorized-Actual-only", second.Evidence["actualSourceState"]);
        Assert.Equal(false, second.Evidence["dataAdmissionByWitnessLearning"]);
        Assert.Equal(false, second.Evidence["memoryAdmissionByWitnessLearning"]);
        Assert.Equal(false, second.Evidence["gelAdmissionByWitnessLearning"]);
        Assert.Equal(false, second.Evidence["selfGelMutationByWitnessLearning"]);
        Assert.Equal(false, second.Evidence["actualActivationByWitnessLearning"]);
        Assert.Equal(false, second.Evidence["authorityGrantByWitnessLearning"]);
        Assert.Equal(false, second.Evidence["externalActionByWitnessLearning"]);

        var ledgerPath = (string)second.Evidence["witnessSplineLedgerPath"]!;
        var verificationPath = (string)second.Evidence["witnessReplayVerificationPath"]!;
        Assert.True(File.Exists(ledgerPath));
        Assert.True(File.Exists(verificationPath));

        var lines = File.ReadAllLines(ledgerPath);
        Assert.Equal(2, lines.Length);
        using var firstEvent = System.Text.Json.JsonDocument.Parse(lines[0]);
        using var secondEvent = System.Text.Json.JsonDocument.Parse(lines[1]);
        var firstDigest = firstEvent.RootElement.GetProperty("currentEventDigest").GetString();
        var secondPrevious = secondEvent.RootElement.GetProperty("previousEventDigest").GetString();
        Assert.Equal(firstDigest, secondPrevious);
        Assert.True(secondEvent.RootElement.GetProperty("appendOnly").GetBoolean());
        Assert.False(secondEvent.RootElement.GetProperty("memoryAdmitted").GetBoolean());
        Assert.False(secondEvent.RootElement.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(secondEvent.RootElement.GetProperty("selfGelMutated").GetBoolean());
        Assert.False(secondEvent.RootElement.GetProperty("actualActivated").GetBoolean());

        using var verification = System.Text.Json.JsonDocument.Parse(File.ReadAllText(verificationPath));
        Assert.Equal("project-sanctuary.oe-selfgel-witness-spline-verification.v1", verification.RootElement.GetProperty("schema").GetString());
        Assert.True(verification.RootElement.GetProperty("ChainValid").GetBoolean());
        Assert.False(verification.RootElement.GetProperty("memoryAdmitted").GetBoolean());
        Assert.False(verification.RootElement.GetProperty("actualActivated").GetBoolean());
    }

    [Fact]
    public void ServiceHeartbeatWritesRestartAdjacentTelemetryWithoutStartingScheduler()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(fixture.Request("service-heartbeat") with
        {
            HeartbeatSeconds = 60
        });

        Assert.Equal("sanctuary-service-heartbeat-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["serviceHeartbeatWritten"]);
        Assert.Equal(60, receipt.Evidence["heartbeatSeconds"]);
        Assert.Equal("cold-local-telemetry", receipt.Evidence["serviceMode"]);
        Assert.Equal("locked-industrial-support", receipt.Evidence["serviceStanding"]);
        Assert.Equal(true, receipt.Evidence["restartAdjacent"]);
        Assert.Equal(true, receipt.Evidence["lastRunPointerUpdated"]);
        Assert.Equal(true, receipt.Evidence["lispJobSliceReadinessWritten"]);
        Assert.Equal(true, receipt.Evidence["singleFlightLockRequiredForFutureService"]);
        Assert.Equal(true, receipt.Evidence["previousSliceDigestRequiredForFutureService"]);
        Assert.Equal(true, receipt.Evidence["nextSlicePointerRequiredForFutureService"]);
        Assert.Equal(false, receipt.Evidence["currentCommandStartsScheduler"]);
        Assert.Equal(false, receipt.Evidence["currentCommandRunsJobSlice"]);
        Assert.Equal(false, receipt.Evidence["currentCommandCallsCodex"]);
        Assert.Equal(false, receipt.Evidence["schedulerStartedByServiceHeartbeat"]);
        Assert.Equal(false, receipt.Evidence["backgroundWorkerStartedByServiceHeartbeat"]);
        Assert.Equal(false, receipt.Evidence["providerCallByServiceHeartbeat"]);
        Assert.Equal(false, receipt.Evidence["modelBindingByServiceHeartbeat"]);
        Assert.Equal(false, receipt.Evidence["externalActionByServiceHeartbeat"]);
        Assert.Equal(false, receipt.Evidence["gelAdmissionByServiceHeartbeat"]);
        Assert.Equal(false, receipt.Evidence["selfGelMutationByServiceHeartbeat"]);
        Assert.Equal(false, receipt.Evidence["actualActivationByServiceHeartbeat"]);

        var heartbeatPath = (string)receipt.Evidence["serviceHeartbeatPath"]!;
        var ledgerPath = (string)receipt.Evidence["serviceLedgerPath"]!;
        var lastRunPointerPath = (string)receipt.Evidence["lastRunPointerPath"]!;
        var jobSliceReadinessPath = (string)receipt.Evidence["lispJobSliceReadinessPath"]!;
        Assert.True(File.Exists(heartbeatPath));
        Assert.True(File.Exists(ledgerPath));
        Assert.True(File.Exists(lastRunPointerPath));
        Assert.True(File.Exists(jobSliceReadinessPath));

        using var heartbeat = System.Text.Json.JsonDocument.Parse(File.ReadAllText(heartbeatPath));
        Assert.Equal("project-sanctuary.service-heartbeat.v1", heartbeat.RootElement.GetProperty("schema").GetString());
        Assert.True(heartbeat.RootElement.GetProperty("restartAdjacent").GetBoolean());
        Assert.False(heartbeat.RootElement.GetProperty("schedulerStarted").GetBoolean());
        Assert.False(heartbeat.RootElement.GetProperty("codexCalled").GetBoolean());
        Assert.False(heartbeat.RootElement.GetProperty("providerCalled").GetBoolean());

        using var jobSlice = System.Text.Json.JsonDocument.Parse(File.ReadAllText(jobSliceReadinessPath));
        Assert.Equal("project-sanctuary.lisp-job-slice-readiness.v1", jobSlice.RootElement.GetProperty("schema").GetString());
        Assert.Equal("SLI.Lisp.ControlMatrix", jobSlice.RootElement.GetProperty("intendedFutureCarrier").GetString());
        Assert.False(jobSlice.RootElement.GetProperty("currentCommandStartsScheduler").GetBoolean());
        Assert.False(jobSlice.RootElement.GetProperty("currentCommandRunsJobSlice").GetBoolean());
        Assert.False(jobSlice.RootElement.GetProperty("currentCommandCallsCodex").GetBoolean());
        Assert.False(jobSlice.RootElement.GetProperty("currentCommandActivatesActual").GetBoolean());
    }

    [Fact]
    public void BoundedRefinementTicketWritesColdJobIntentWithoutExecutingWork()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(fixture.Request("bounded-refinement-ticket"));

        Assert.Equal("sanctuary-bounded-refinement-ticket-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["boundedRefinementTicketWritten"]);
        Assert.Equal("operator-requested-hourly-heartbeat", receipt.Evidence["boundedRefinementCadence"]);
        Assert.Equal(false, receipt.Evidence["boundedRefinementTicketExecutesWork"]);
        Assert.Equal(false, receipt.Evidence["boundedRefinementStartsScheduler"]);
        Assert.Equal(false, receipt.Evidence["boundedRefinementCallsCodex"]);
        Assert.Equal(false, receipt.Evidence["boundedRefinementCallsProvider"]);
        Assert.Equal(false, receipt.Evidence["boundedRefinementBindsModel"]);
        Assert.Equal(false, receipt.Evidence["boundedRefinementAuthorizesExternalAction"]);
        Assert.Equal(false, receipt.Evidence["boundedRefinementAdmitsGel"]);
        Assert.Equal(false, receipt.Evidence["boundedRefinementMutatesSelfGel"]);
        Assert.Equal(false, receipt.Evidence["boundedRefinementActivatesActual"]);
        Assert.Equal(true, receipt.Evidence["boundedRefinementOperatorReviewRequired"]);
        Assert.Equal(true, receipt.Evidence["boundedRefinementFutureImplementationRequired"]);

        var ticketPath = (string)receipt.Evidence["boundedRefinementTicketPath"]!;
        var ledgerPath = (string)receipt.Evidence["boundedRefinementTicketLedgerPath"]!;
        Assert.True(File.Exists(ticketPath));
        Assert.True(File.Exists(ledgerPath));

        using var ticket = System.Text.Json.JsonDocument.Parse(File.ReadAllText(ticketPath));
        Assert.Equal("project-sanctuary.service.bounded-refinement-ticket.v1", ticket.RootElement.GetProperty("schema").GetString());
        Assert.Equal("SLI.Lisp.ControlMatrix", ticket.RootElement.GetProperty("lispJobSliceCarrier").GetString());
        Assert.True(ticket.RootElement.GetProperty("ticketWritesIntent").GetBoolean());
        Assert.False(ticket.RootElement.GetProperty("ticketExecutesWork").GetBoolean());
        Assert.False(ticket.RootElement.GetProperty("schedulerStarted").GetBoolean());
        Assert.False(ticket.RootElement.GetProperty("providerCalled").GetBoolean());
        Assert.False(ticket.RootElement.GetProperty("modelBound").GetBoolean());
        Assert.False(ticket.RootElement.GetProperty("externalActionAuthorized").GetBoolean());
        Assert.False(ticket.RootElement.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(ticket.RootElement.GetProperty("selfGelMutated").GetBoolean());
        Assert.False(ticket.RootElement.GetProperty("cmeActualActivated").GetBoolean());
        Assert.False(ticket.RootElement.GetProperty("sanctuaryActualActivated").GetBoolean());
    }

    [Fact]
    public void JobSliceGuardWritesReadinessPointerWithoutRunningSlice()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        service.Run(fixture.Request("service-heartbeat"));
        service.Run(fixture.Request("bounded-refinement-ticket"));
        service.Run(fixture.Request("lisp-control-matrix-register"));
        service.Run(fixture.Request("receipt-export"));
        service.Run(fixture.Request("lease-check") with
        {
            LeaseMinutes = 10,
            LicenseScope = "LabRefinement"
        });

        var receipt = service.Run(fixture.Request("job-slice-guard"));

        Assert.Equal("sanctuary-job-slice-guard-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["jobSliceGuardWritten"]);
        Assert.Equal(true, receipt.Evidence["jobSliceTicketPresent"]);
        Assert.Equal(true, receipt.Evidence["jobSliceHeartbeatPresent"]);
        Assert.Equal(true, receipt.Evidence["jobSliceLastRunPointerPresent"]);
        Assert.Equal(true, receipt.Evidence["jobSliceReceiptExportManifestPresent"]);
        Assert.Equal(true, receipt.Evidence["jobSliceLeaseCheckPresent"]);
        Assert.Equal(true, receipt.Evidence["jobSliceControlMatrixRegisterPresent"]);
        Assert.Equal(true, receipt.Evidence["jobSliceSingleFlightLockEvaluated"]);
        Assert.Equal(false, receipt.Evidence["jobSliceSingleFlightLockHeldAfterCommand"]);
        Assert.Equal(true, receipt.Evidence["jobSliceNextPointerWritten"]);
        Assert.Equal(true, receipt.Evidence["jobSliceLeaseCheckEvaluated"]);
        Assert.Equal("denied-not-issued", receipt.Evidence["jobSliceLeaseState"]);
        Assert.Equal(true, receipt.Evidence["jobSliceIssueFloorCheckEvaluated"]);
        Assert.Equal(false, receipt.Evidence["jobSliceIssueFloorClearForExecution"]);
        Assert.Equal(true, receipt.Evidence["jobSliceClosedGateCheckEvaluated"]);
        Assert.Equal(true, receipt.Evidence["jobSliceClosedGatesRemainClosed"]);
        Assert.Equal(false, receipt.Evidence["jobSliceRunnable"]);
        Assert.Equal(false, receipt.Evidence["jobSliceExecuted"]);
        Assert.Equal(false, receipt.Evidence["jobSliceSchedulerStarted"]);
        Assert.Equal(false, receipt.Evidence["jobSliceBackgroundWorkerStarted"]);
        Assert.Equal(false, receipt.Evidence["jobSliceCallsCodex"]);
        Assert.Equal(false, receipt.Evidence["jobSliceCallsProvider"]);
        Assert.Equal(false, receipt.Evidence["jobSliceBindsModel"]);
        Assert.Equal(false, receipt.Evidence["jobSliceAuthorizesExternalAction"]);
        Assert.Equal(false, receipt.Evidence["jobSliceAdmitsGel"]);
        Assert.Equal(false, receipt.Evidence["jobSliceMutatesSelfGel"]);
        Assert.Equal(false, receipt.Evidence["jobSliceActivatesActual"]);
        Assert.Equal(true, receipt.Evidence["jobSliceOperatorReviewRequiredBeforeExecution"]);

        var guardPath = (string)receipt.Evidence["jobSliceGuardPath"]!;
        var nextPointerPath = (string)receipt.Evidence["jobSliceNextPointerPath"]!;
        Assert.True(File.Exists(guardPath));
        Assert.True(File.Exists(nextPointerPath));

        using var guard = System.Text.Json.JsonDocument.Parse(File.ReadAllText(guardPath));
        Assert.Equal("project-sanctuary.service.job-slice-guard.v1", guard.RootElement.GetProperty("schema").GetString());
        Assert.Equal("SLI.Lisp.ControlMatrix", guard.RootElement.GetProperty("intendedFutureCarrier").GetString());
        Assert.True(guard.RootElement.GetProperty("ticketPresent").GetBoolean());
        Assert.True(guard.RootElement.GetProperty("leaseCheckPresent").GetBoolean());
        Assert.True(guard.RootElement.GetProperty("controlMatrixRegisterPresent").GetBoolean());
        Assert.NotEqual("", guard.RootElement.GetProperty("controlMatrixRegisterDigest").GetString());
        Assert.True(guard.RootElement.GetProperty("closedGatesRemainClosed").GetBoolean());
        Assert.False(guard.RootElement.GetProperty("jobSliceRunnable").GetBoolean());
        Assert.False(guard.RootElement.GetProperty("jobSliceExecuted").GetBoolean());
        Assert.False(guard.RootElement.GetProperty("schedulerStarted").GetBoolean());
        Assert.False(guard.RootElement.GetProperty("codexCalled").GetBoolean());
        Assert.False(guard.RootElement.GetProperty("providerCalled").GetBoolean());
        Assert.False(guard.RootElement.GetProperty("modelBound").GetBoolean());
        Assert.False(guard.RootElement.GetProperty("externalActionAuthorized").GetBoolean());
        Assert.False(guard.RootElement.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(guard.RootElement.GetProperty("selfGelMutated").GetBoolean());
        Assert.False(guard.RootElement.GetProperty("cmeActualActivated").GetBoolean());
        Assert.False(guard.RootElement.GetProperty("sanctuaryActualActivated").GetBoolean());

        using var pointer = System.Text.Json.JsonDocument.Parse(File.ReadAllText(nextPointerPath));
        Assert.Equal("project-sanctuary.service.next-slice-pointer.v1", pointer.RootElement.GetProperty("schema").GetString());
        Assert.False(pointer.RootElement.GetProperty("nextCommandRunnable").GetBoolean());
        Assert.True(pointer.RootElement.GetProperty("operatorReviewRequired").GetBoolean());
    }

    [Fact]
    public void LeaseCheckWritesDeniedDeltaDecayingPostureWithoutIssuingAuthority()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        service.Run(fixture.Request("service-heartbeat"));

        var receipt = service.Run(fixture.Request("lease-check") with
        {
            LeaseMinutes = 10,
            LicenseScope = "LabQueryState",
            RegisteredAccountConfirmed = true
        });

        Assert.Equal("sanctuary-lease-check-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["leaseCheckWritten"]);
        Assert.Equal("LabQueryState", receipt.Evidence["leaseCheckLicenseScope"]);
        Assert.Equal(10, receipt.Evidence["leaseCheckRequestedLeaseMinutes"]);
        Assert.Equal(true, receipt.Evidence["leaseCheckHeartbeatBound"]);
        Assert.Equal(true, receipt.Evidence["leaseCheckHeartbeatPresent"]);
        Assert.Equal("denied", receipt.Evidence["leaseDefaultState"]);
        Assert.Equal("denied-not-issued", receipt.Evidence["leaseState"]);
        Assert.Equal(false, receipt.Evidence["leaseIssued"]);
        Assert.Equal(false, receipt.Evidence["leaseActive"]);
        Assert.Null(receipt.Evidence["leaseAuthorizedUntilUtc"]);
        Assert.Equal(true, receipt.Evidence["leaseFailToSilenceDefault"]);
        Assert.Equal(false, receipt.Evidence["leaseTwoFactorSatisfiedForIssuance"]);
        Assert.Equal(true, receipt.Evidence["leaseStewardReviewRequired"]);
        Assert.Equal(true, receipt.Evidence["leaseCrypticReviewRequiredForSecretOrPrivateScope"]);
        Assert.Equal(true, receipt.Evidence["leasePrimeReceiptWitnessRequired"]);
        Assert.Equal(false, receipt.Evidence["providerCallByLeaseCheck"]);
        Assert.Equal(false, receipt.Evidence["modelBindingByLeaseCheck"]);
        Assert.Equal(false, receipt.Evidence["externalActionByLeaseCheck"]);
        Assert.Equal(false, receipt.Evidence["gelAdmissionByLeaseCheck"]);
        Assert.Equal(false, receipt.Evidence["selfGelMutationByLeaseCheck"]);
        Assert.Equal(false, receipt.Evidence["actualActivationByLeaseCheck"]);

        var leaseCheckPath = (string)receipt.Evidence["leaseCheckPath"]!;
        var ledgerPath = (string)receipt.Evidence["leaseCheckLedgerPath"]!;
        Assert.True(File.Exists(leaseCheckPath));
        Assert.True(File.Exists(ledgerPath));

        using var lease = System.Text.Json.JsonDocument.Parse(File.ReadAllText(leaseCheckPath));
        Assert.Equal("project-sanctuary.service.lease-check.v1", lease.RootElement.GetProperty("schema").GetString());
        Assert.Equal("denied-not-issued", lease.RootElement.GetProperty("leaseState").GetString());
        Assert.False(lease.RootElement.GetProperty("leaseIssued").GetBoolean());
        Assert.False(lease.RootElement.GetProperty("leaseActive").GetBoolean());
        Assert.True(lease.RootElement.GetProperty("failToSilenceDefault").GetBoolean());
        Assert.False(lease.RootElement.GetProperty("providerCalled").GetBoolean());
        Assert.False(lease.RootElement.GetProperty("modelBound").GetBoolean());
        Assert.False(lease.RootElement.GetProperty("externalActionAuthorized").GetBoolean());
        Assert.False(lease.RootElement.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(lease.RootElement.GetProperty("selfGelMutated").GetBoolean());
        Assert.False(lease.RootElement.GetProperty("cmeActualActivated").GetBoolean());
        Assert.False(lease.RootElement.GetProperty("sanctuaryActualActivated").GetBoolean());
    }

    [Fact]
    public void ReceiptExportWritesColdManifestWithoutCopyingReceiptBodies()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        service.Run(fixture.Request("tool-idle"));
        service.Run(fixture.Request("service-heartbeat"));

        var receipt = service.Run(fixture.Request("receipt-export"));

        Assert.Equal("sanctuary-receipt-export-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["receiptExportWritten"]);
        Assert.True((int)receipt.Evidence["receiptExportReceiptCount"]! >= 2);
        Assert.True((int)receipt.Evidence["receiptExportCommandCount"]! >= 2);
        Assert.Equal(0, receipt.Evidence["receiptExportUnreadableReceiptCount"]);
        Assert.Equal(true, receipt.Evidence["receiptExportAllKnownReceiptsClosed"]);
        Assert.Equal(false, receipt.Evidence["receiptBodyCopiedByExport"]);
        Assert.Equal(false, receipt.Evidence["markdownBodyCopiedByExport"]);
        Assert.Equal(false, receipt.Evidence["secretPayloadCopiedByExport"]);
        Assert.Equal(false, receipt.Evidence["payloadContentReadByReceiptExport"]);
        Assert.Equal(false, receipt.Evidence["crypticStoresScannedByReceiptExport"]);
        Assert.Equal(false, receipt.Evidence["sourcePathsDisclosedByExport"]);
        Assert.Equal(false, receipt.Evidence["providerCallByReceiptExport"]);
        Assert.Equal(false, receipt.Evidence["modelBindingByReceiptExport"]);
        Assert.Equal(false, receipt.Evidence["externalActionByReceiptExport"]);
        Assert.Equal(false, receipt.Evidence["gelAdmissionByReceiptExport"]);
        Assert.Equal(false, receipt.Evidence["selfGelMutationByReceiptExport"]);
        Assert.Equal(false, receipt.Evidence["actualActivationByReceiptExport"]);

        var manifestPath = (string)receipt.Evidence["receiptExportManifestPath"]!;
        var ledgerPath = (string)receipt.Evidence["receiptExportLedgerPath"]!;
        Assert.True(File.Exists(manifestPath));
        Assert.True(File.Exists(ledgerPath));

        using var manifest = System.Text.Json.JsonDocument.Parse(File.ReadAllText(manifestPath));
        Assert.Equal("project-sanctuary.service.receipt-export.v1", manifest.RootElement.GetProperty("schema").GetString());
        Assert.False(manifest.RootElement.GetProperty("receiptRootPathDisclosed").GetBoolean());
        Assert.True(manifest.RootElement.GetProperty("receiptCount").GetInt32() >= 2);
        Assert.True(manifest.RootElement.GetProperty("allKnownReceiptsClosed").GetBoolean());
        Assert.False(manifest.RootElement.GetProperty("receiptBodyCopied").GetBoolean());
        Assert.False(manifest.RootElement.GetProperty("payloadContentRead").GetBoolean());
        Assert.False(manifest.RootElement.GetProperty("crypticStoresScanned").GetBoolean());
        Assert.False(manifest.RootElement.GetProperty("sourcePathsDisclosed").GetBoolean());

        var manifestText = File.ReadAllText(manifestPath);
        Assert.DoesNotContain(fixture.RootPath, manifestText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ReceiptJsonPath", manifestText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ReceiptMarkdownPath", manifestText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SecurityHardeningScansVisibleStateWithoutReadingCrypticStores()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        service.Run(fixture.Request("tool-idle"));

        var visibleLeakRoot = Path.Combine(fixture.RootPath, "install", "cgel", "visible-leak");
        var crypticRoot = Path.Combine(fixture.RootPath, "install", "cryptic-stores", "not-scanned");
        Directory.CreateDirectory(visibleLeakRoot);
        Directory.CreateDirectory(crypticRoot);
        File.WriteAllText(Path.Combine(visibleLeakRoot, "visible.json"), """{"sourceRootPath":"should be hashed in report only"}""");
        File.WriteAllText(Path.Combine(crypticRoot, "cryptic.json"), """{"sourceRootPath":"should not be scanned"}""");

        var receipt = service.Run(fixture.Request("security-hardening"));

        Assert.Equal("sanctuary-security-hardening-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["securityHardeningWritten"]);
        Assert.True((int)receipt.Evidence["securityReceiptCount"]! >= 1);
        Assert.Equal(0, receipt.Evidence["openGateReceiptCount"]);
        Assert.Equal(0, receipt.Evidence["unreadableReceiptCount"]);
        Assert.Equal(true, receipt.Evidence["visibleLeakDetected"]);
        Assert.Equal(1, receipt.Evidence["visibleLeakFindingCount"]);
        Assert.Equal(false, receipt.Evidence["crypticStoresScanned"]);
        Assert.Equal(false, receipt.Evidence["payloadContentReadBySecurityHardening"]);
        Assert.Equal(false, receipt.Evidence["sourcePathsDisclosedBySecurityHardening"]);
        Assert.Equal(true, receipt.Evidence["securityHardeningReviewRequired"]);
        Assert.Equal(false, receipt.Evidence["providerCallBySecurityHardening"]);
        Assert.Equal(false, receipt.Evidence["modelBindingBySecurityHardening"]);
        Assert.Equal(false, receipt.Evidence["externalActionBySecurityHardening"]);
        Assert.Equal(false, receipt.Evidence["gelAdmissionBySecurityHardening"]);
        Assert.Equal(false, receipt.Evidence["selfGelMutationBySecurityHardening"]);
        Assert.Equal(false, receipt.Evidence["actualActivationBySecurityHardening"]);

        var reportPath = (string)receipt.Evidence["securityHardeningPath"]!;
        var ledgerPath = (string)receipt.Evidence["securityHardeningLedgerPath"]!;
        Assert.True(File.Exists(reportPath));
        Assert.True(File.Exists(ledgerPath));

        using var report = System.Text.Json.JsonDocument.Parse(File.ReadAllText(reportPath));
        Assert.Equal("project-sanctuary.cgel.security-hardening.v1", report.RootElement.GetProperty("schema").GetString());
        Assert.Equal(1, report.RootElement.GetProperty("leakFindingCount").GetInt32());
        Assert.False(report.RootElement.GetProperty("crypticStoresScanned").GetBoolean());
        Assert.False(report.RootElement.GetProperty("payloadContentRead").GetBoolean());

        var reportText = File.ReadAllText(reportPath);
        Assert.DoesNotContain("should be hashed", reportText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("not-scanned", reportText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void InstallFloorCheckLocksIndustrialCmeAndWritesCgelFailureMode()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(fixture.Request("install-floor-check") with
        {
            IssueId = "floor-001",
            InstallFailureMode = "operator-instruction-engagement-unresolved"
        });

        Assert.Equal("LockedCold", receipt.Disposition);
        Assert.Equal("sanctuary-install-floor-check-locked-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal("industrial-cme-locked", receipt.Evidence["installFloorState"]);
        Assert.Equal(true, receipt.Evidence["industrialCmeLocked"]);
        Assert.Equal(true, receipt.Evidence["issueResolverRequired"]);
        Assert.Equal("floor-001", receipt.Evidence["issueId"]);
        Assert.Equal("operator-instruction-engagement-unresolved", receipt.Evidence["typedFailureMode"]);
        Assert.Equal("operator-instruction-engagement", receipt.Evidence["failureModeClass"]);
        Assert.Equal("Steward", receipt.Evidence["issueTrackingOwner"]);
        Assert.Equal("Cryptic", receipt.Evidence["issueProcessingOwner"]);
        Assert.Equal("Prime", receipt.Evidence["issueReceiptWitnessOwner"]);
        Assert.Equal(true, receipt.Evidence["supportLockNotPunitive"]);
        Assert.Equal(true, receipt.Evidence["nonDiagnosticSupportPosture"]);
        Assert.Equal(false, receipt.Evidence["medicalOrCognitiveDiagnosisMade"]);
        Assert.Equal(false, receipt.Evidence["cmeActualAllowedAfterFloor"]);

        Assert.True(File.Exists((string)receipt.Evidence["cgelFailureModePath"]!));
        Assert.True(File.Exists((string)receipt.Evidence["issueTrackingEventPath"]!));
    }

    [Fact]
    public void IssueResolverWithoutApprovalKeepsInstallFloorLocked()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(fixture.Request("issue-resolver") with
        {
            IssueId = "floor-002",
            InstallFailureMode = "operator-instruction-engagement-unresolved"
        });

        Assert.Equal("LockedCold", receipt.Disposition);
        Assert.Equal("sanctuary-issue-resolver-unresolved-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal("industrial-cme-locked", receipt.Evidence["installFloorState"]);
        Assert.Equal(false, receipt.Evidence["issueResolved"]);
        Assert.Equal(false, receipt.Evidence["issueResolverApproved"]);
        Assert.Equal(true, receipt.Evidence["issueResolverRequired"]);
        Assert.Equal(false, receipt.Evidence["authorityGrantedByResolver"]);
        Assert.Equal(false, receipt.Evidence["cmeActualActivatedByResolver"]);
    }

    [Fact]
    public void IssueResolverCanResolveFloorWithoutOpeningActualGates()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();

        var lockedReceipt = service.Run(fixture.Request("install-floor-check") with
        {
            IssueId = "floor-003",
            InstallFailureMode = "operator-instruction-engagement-unresolved"
        });

        var resolverReceipt = service.Run(fixture.Request("issue-resolver") with
        {
            IssueId = "floor-003",
            InstallFailureMode = "operator-instruction-engagement-unresolved",
            IssueResolverApproved = true,
            IssueResolutionNote = "Operator support floor resolved in test."
        });

        var resolvedFloorReceipt = service.Run(fixture.Request("install-floor-check") with
        {
            IssueId = "floor-003",
            InstallFailureMode = "operator-instruction-engagement-unresolved",
            SessionId = "test-session-resolved-floor"
        });

        Assert.Equal("LockedCold", lockedReceipt.Disposition);
        Assert.Equal("CompletedCold", resolverReceipt.Disposition);
        Assert.Equal("sanctuary-issue-resolver-completed-cold", resolverReceipt.OutcomeCode);
        Assert.True(resolverReceipt.Gates.AllClosed);
        Assert.Equal(true, resolverReceipt.Evidence["issueResolved"]);
        Assert.Equal(false, resolverReceipt.Evidence["authorityGrantedByResolver"]);
        Assert.Equal(false, resolverReceipt.Evidence["actionAuthorizedByResolver"]);
        Assert.Equal(false, resolverReceipt.Evidence["cmeActualActivatedByResolver"]);
        Assert.True(File.Exists((string)resolverReceipt.Evidence["resolutionPath"]!));

        Assert.Equal("CompletedCold", resolvedFloorReceipt.Disposition);
        Assert.Equal("industrial-cme-floor-resolved", resolvedFloorReceipt.Evidence["installFloorState"]);
        Assert.Equal(false, resolvedFloorReceipt.Evidence["industrialCmeLocked"]);
        Assert.Equal(true, resolvedFloorReceipt.Evidence["issueResolved"]);
        Assert.True(resolvedFloorReceipt.Gates.AllClosed);
    }

    [Fact]
    public void DomainRegisterWritesClosedGatesAndAccountabilityCertificationPosture()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(fixture.Request("domain-register"));

        Assert.Equal("sanctuary-domain-register-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["domainRegisterWritten"]);
        Assert.Equal(true, receipt.Evidence["allDomainAccessDeniedByDefault"]);
        Assert.Equal(true, receipt.Evidence["allDomainAuthoritySurfacesDeltaDecaying"]);
        Assert.Equal(true, receipt.Evidence["allDomainAuthorityLeasesRequired"]);
        Assert.Equal(false, receipt.Evidence["credentialAdmissionByRegister"]);
        Assert.Equal(false, receipt.Evidence["trainingRecordEqualsCertification"]);
        Assert.Equal(false, receipt.Evidence["historicalEducationEqualsLicensure"]);
        Assert.Equal(false, receipt.Evidence["professionalAuthorityGrantedByRegister"]);
        Assert.Equal(false, receipt.Evidence["actionAuthorizedByRegister"]);
        Assert.Equal(true, receipt.Evidence["accountabilityCertificationPostureCoded"]);
        Assert.Equal(true, receipt.Evidence["lifetimeEngagementScopeCoded"]);
        Assert.Equal(true, receipt.Evidence["historicalEducationScopeCoded"]);
        Assert.Equal(true, receipt.Evidence["trainingCertificationScopeCoded"]);
        Assert.Equal(true, receipt.Evidence["ongoingWorkScopeCoded"]);
        Assert.Equal(true, receipt.Evidence["expiredAuthorityFailsToSilence"]);

        var registerPath = (string)receipt.Evidence["domainRegisterPath"]!;
        Assert.True(File.Exists(registerPath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(registerPath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.domain-register.v1", root.GetProperty("schema").GetString());
        Assert.Equal("denied", root.GetProperty("defaultAccessState").GetString());
        Assert.Equal("delta-decaying-authority-surface", root.GetProperty("authoritySurfaceKind").GetString());
        Assert.False(root.GetProperty("credentialAdmissionByRegister").GetBoolean());
        Assert.False(root.GetProperty("professionalAuthorityGrantedByRegister").GetBoolean());
        Assert.False(root.GetProperty("actionAuthorizedByRegister").GetBoolean());

        var entries = root.GetProperty("entries").EnumerateArray().ToArray();
        Assert.True(entries.Length >= 12);
        Assert.All(entries, entry =>
        {
            Assert.Equal("denied", entry.GetProperty("defaultAccessState").GetString());
            Assert.Equal("delta-decaying-authority-surface", entry.GetProperty("authoritySurfaceKind").GetString());
            Assert.True(entry.GetProperty("leaseRequired").GetBoolean());
            Assert.True(entry.GetProperty("professionalResponsibilityBoundary").GetBoolean());
            Assert.False(entry.GetProperty("grantsAuthority").GetBoolean());
            Assert.False(entry.GetProperty("admitsCredential").GetBoolean());
            Assert.False(entry.GetProperty("admitsGel").GetBoolean());
            Assert.False(entry.GetProperty("cmeActualAllowed").GetBoolean());
            Assert.False(entry.GetProperty("sanctuaryActualAllowed").GetBoolean());
            Assert.NotEmpty(entry.GetProperty("historicalEducationFields").EnumerateArray());
            Assert.NotEmpty(entry.GetProperty("trainingAndCertificationFields").EnumerateArray());
            Assert.NotEmpty(entry.GetProperty("ongoingWorkRelatedFields").EnumerateArray());
            Assert.NotEmpty(entry.GetProperty("accountabilityCertificationPosture").EnumerateArray());
        });

        var legal = entries.Single(entry => entry.GetProperty("domainId").GetString() == "Legal.GEL");
        var medical = entries.Single(entry => entry.GetProperty("domainId").GetString() == "Medical.GEL");
        var education = entries.Single(entry => entry.GetProperty("domainId").GetString() == "EducationTrainingCertification.GEL");
        var sage = entries.Single(entry => entry.GetProperty("domainId").GetString() == "SpecialCases.SAGE.GEL");

        Assert.True(legal.GetProperty("licensedProfessionalRequiredForAuthority").GetBoolean());
        Assert.True(medical.GetProperty("licensedProfessionalRequiredForAuthority").GetBoolean());
        Assert.True(medical.GetProperty("releaseOfInformationRequiredForPrivateData").GetBoolean());
        Assert.Contains(
            education.GetProperty("requiredGateRules").EnumerateArray(),
            item => item.GetString() == "issuer-verification");
        Assert.Contains(
            sage.GetProperty("requiredGateRules").EnumerateArray(),
            item => item.GetString() == "explicit-activation-denial");
    }

    [Fact]
    public void CoreTargetsRegisterNamesLockedIndustrialBuildTargetsWithoutOpeningGates()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(fixture.Request("core-targets"));

        Assert.Equal("sanctuary-core-targets-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["coreTargetRegisterWritten"]);
        Assert.Equal(4, receipt.Evidence["coreTargetCount"]);
        Assert.Equal(true, receipt.Evidence["lockedIndustrialShowcase"]);
        Assert.Equal(true, receipt.Evidence["fullStandingFormVisible"]);
        Assert.Equal(false, receipt.Evidence["fullAuthorityVisible"]);
        Assert.Equal(true, receipt.Evidence["sliBuildAndUseTargeted"]);
        Assert.Equal(true, receipt.Evidence["engrammitizationBuildAndUseTargeted"]);
        Assert.Equal(true, receipt.Evidence["gelFormationCondensationCompostingIngressTargeted"]);
        Assert.Equal(true, receipt.Evidence["oeSelfGelWitnessLearningTargeted"]);
        Assert.Equal(true, receipt.Evidence["condensationCoded"]);
        Assert.Equal(true, receipt.Evidence["compostingCoded"]);
        Assert.Equal(true, receipt.Evidence["precipitoryIngressCoded"]);
        Assert.Equal(true, receipt.Evidence["appendOnlySplinedWitnessStoresCoded"]);
        Assert.Equal(true, receipt.Evidence["actualLearningNamedAsDesignTarget"]);
        Assert.Equal(false, receipt.Evidence["actualActivationByCoreTargets"]);
        Assert.Equal(false, receipt.Evidence["gelAdmissionByCoreTargets"]);
        Assert.Equal(false, receipt.Evidence["selfGelMutationByCoreTargets"]);
        Assert.Equal(false, receipt.Evidence["externalActionByCoreTargets"]);

        var registerPath = (string)receipt.Evidence["coreTargetRegisterPath"]!;
        Assert.True(File.Exists(registerPath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(registerPath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.core-target-register.v1", root.GetProperty("schema").GetString());
        Assert.True(root.GetProperty("lockedIndustrialShowcase").GetBoolean());
        Assert.True(root.GetProperty("fullStandingFormVisible").GetBoolean());
        Assert.False(root.GetProperty("fullAuthorityVisible").GetBoolean());
        Assert.False(root.GetProperty("actualActivationByRegister").GetBoolean());
        Assert.False(root.GetProperty("gelAdmittedByRegister").GetBoolean());
        Assert.False(root.GetProperty("selfGelMutatedByRegister").GetBoolean());

        var targets = root.GetProperty("targets").EnumerateArray().ToArray();
        Assert.Equal(4, targets.Length);
        Assert.Contains(targets, target => target.GetProperty("targetId").GetString() == "SLI.BuildUse");
        Assert.Contains(targets, target => target.GetProperty("targetId").GetString() == "Engrammitization.BuildUse");
        Assert.Contains(targets, target => target.GetProperty("targetId").GetString() == "GEL.FormationClosure");
        Assert.Contains(targets, target => target.GetProperty("targetId").GetString() == "OE.SelfGEL.WitnessLearning");

        Assert.All(targets, target =>
        {
            Assert.True(target.GetProperty("receiptBearing").GetBoolean());
            Assert.True(target.GetProperty("reversibleOrReviewable").GetBoolean());
            Assert.True(target.GetProperty("admissionRequiredForCanon").GetBoolean());
            Assert.True(target.GetProperty("authorityRequiredForAction").GetBoolean());
            Assert.False(target.GetProperty("dataAdmissionByTarget").GetBoolean());
            Assert.False(target.GetProperty("gelAdmissionByTarget").GetBoolean());
            Assert.False(target.GetProperty("selfGelMutationByTarget").GetBoolean());
            Assert.False(target.GetProperty("actualActivationByTarget").GetBoolean());
            Assert.False(target.GetProperty("providerCallByTarget").GetBoolean());
            Assert.False(target.GetProperty("modelBindingByTarget").GetBoolean());
            Assert.False(target.GetProperty("externalActionByTarget").GetBoolean());
            Assert.NotEmpty(target.GetProperty("formationSurfaces").EnumerateArray());
            Assert.NotEmpty(target.GetProperty("measurementSurfaces").EnumerateArray());
            Assert.NotEmpty(target.GetProperty("deniedShortcuts").EnumerateArray());
        });

        var gelTarget = targets.Single(target => target.GetProperty("targetId").GetString() == "GEL.FormationClosure");
        Assert.Contains(
            gelTarget.GetProperty("formationSurfaces").EnumerateArray(),
            item => item.GetString() == "condensation");
        Assert.Contains(
            gelTarget.GetProperty("formationSurfaces").EnumerateArray(),
            item => item.GetString() == "composting");
        Assert.Contains(
            gelTarget.GetProperty("formationSurfaces").EnumerateArray(),
            item => item.GetString() == "precipitory ingress");

        var selfLearningTarget = targets.Single(target => target.GetProperty("targetId").GetString() == "OE.SelfGEL.WitnessLearning");
        Assert.Equal("future-or-separately-authorized-Actual-only", selfLearningTarget.GetProperty("actualSourceState").GetString());
    }

    [Fact]
    public void SwarmRefinementWritesHundoPlanWithPauseGatesAndClosedAuthority()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(fixture.Request("swarm-refinement"));

        Assert.Equal("sanctuary-swarm-refinement-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["swarmRefinementRegisterWritten"]);
        Assert.Equal("30-60-90-of-100 groupoids by 10 sections", receipt.Evidence["swarmMethod"]);
        Assert.Equal(100, receipt.Evidence["hundoSessionCount"]);
        Assert.Equal(10, receipt.Evidence["hundoSectionCount"]);
        Assert.Equal(10, receipt.Evidence["sessionsPerSection"]);
        Assert.Equal(true, receipt.Evidence["applyUpdatesAtPauseGates"]);
        Assert.Equal(100, receipt.Evidence["targetOptimalFormSession"]);
        Assert.Equal(7, receipt.Evidence["swarmExecutionStepCount"]);
        Assert.Equal(true, receipt.Evidence["swarmLabGelCrystallizationIncluded"]);
        Assert.Equal(true, receipt.Evidence["swarmSelfOtherCollapseDenied"]);
        Assert.Equal(true, receipt.Evidence["swarmSanctuaryGelResidueRequired"]);
        Assert.Equal(true, receipt.Evidence["swarmSelfGelResidueRequired"]);
        Assert.Equal(true, receipt.Evidence["swarmTestingBeginsAfterPhaseBody"]);
        Assert.Equal(true, receipt.Evidence["swarmLifeReviewStudyModeled"]);
        Assert.Equal(true, receipt.Evidence["buildGelResidueWritten"]);
        Assert.Equal(true, receipt.Evidence["governanceGelResidueWritten"]);
        Assert.Equal(true, receipt.Evidence["pluginResidueCompatible"]);
        Assert.Equal(false, receipt.Evidence["updatesAppliedByAutonomousAgents"]);
        Assert.Equal(false, receipt.Evidence["providerCallBySwarm"]);
        Assert.Equal(false, receipt.Evidence["modelBindingBySwarm"]);
        Assert.Equal(false, receipt.Evidence["externalActionBySwarm"]);
        Assert.Equal(false, receipt.Evidence["actualActivationBySwarm"]);
        Assert.Equal(false, receipt.Evidence["gelAdmissionBySwarm"]);
        Assert.Equal(false, receipt.Evidence["selfGelMutationBySwarm"]);

        var registerPath = (string)receipt.Evidence["swarmRefinementRegisterPath"]!;
        var runLedgerPath = (string)receipt.Evidence["swarmRunLedgerPath"]!;
        var governanceLedgerPath = (string)receipt.Evidence["swarmGovernanceLedgerPath"]!;
        Assert.True(File.Exists(registerPath));
        Assert.True(File.Exists(runLedgerPath));
        Assert.True(File.Exists(governanceLedgerPath));
        Assert.Equal(100, File.ReadLines(runLedgerPath).Count());

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(registerPath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.hundo-swarm-refinement.v1", root.GetProperty("schema").GetString());
        Assert.Equal(100, root.GetProperty("runSessionCount").GetInt32());
        Assert.Equal(100, root.GetProperty("optimalFormTargetSession").GetInt32());
        Assert.True(root.GetProperty("labGelCrystallizationRequired").GetBoolean());
        Assert.True(root.GetProperty("selfOtherCollapseDenied").GetBoolean());
        Assert.True(root.GetProperty("sanctuaryGelResidueRequired").GetBoolean());
        Assert.True(root.GetProperty("selfGelReconstructionResidueRequired").GetBoolean());
        Assert.True(root.GetProperty("testingBeginsAfterPhaseBody").GetBoolean());

        var pauseGates = root.GetProperty("pauseGates").EnumerateArray().Select(item => item.GetInt32()).ToArray();
        Assert.Equal(new[] { 30, 60, 90 }, pauseGates);

        var lanes = root.GetProperty("lanes").EnumerateArray().ToArray();
        Assert.Contains(lanes, lane => lane.GetProperty("laneId").GetString() == "SLI");
        Assert.Contains(lanes, lane => lane.GetProperty("laneId").GetString() == "Engrammitization");
        Assert.Contains(lanes, lane => lane.GetProperty("laneId").GetString() == "GEL");
        Assert.Contains(lanes, lane => lane.GetProperty("laneId").GetString() == "OE-SelfGEL");
        Assert.Contains(
            lanes.Single(lane => lane.GetProperty("laneId").GetString() == "GEL")
                .GetProperty("targetCommands")
                .EnumerateArray(),
            command => command.GetString() == "lab-gel-crystallization-phases");
        Assert.Contains(
            lanes.Single(lane => lane.GetProperty("laneId").GetString() == "OE-SelfGEL")
                .GetProperty("targetCommands")
                .EnumerateArray(),
            command => command.GetString() == "lab-gel-crystallization-phases");
        Assert.All(lanes, lane =>
        {
            Assert.Equal("denied-by-default", lane.GetProperty("authorityState").GetString());
            Assert.False(lane.GetProperty("actualActivationAllowed").GetBoolean());
            Assert.False(lane.GetProperty("providerCallAllowed").GetBoolean());
            Assert.False(lane.GetProperty("externalActionAllowed").GetBoolean());
        });

        var runSessions = root.GetProperty("runSessions").EnumerateArray().ToArray();
        Assert.Equal(100, runSessions.Length);
        Assert.True(runSessions[29].GetProperty("pauseGate").GetBoolean());
        Assert.True(runSessions[29].GetProperty("applyUpdatesHere").GetBoolean());
        Assert.True(runSessions[59].GetProperty("pauseGate").GetBoolean());
        Assert.True(runSessions[89].GetProperty("pauseGate").GetBoolean());
        Assert.True(runSessions[99].GetProperty("optimalFormTarget").GetBoolean());
        Assert.True(runSessions[99].GetProperty("governanceReviewRequired").GetBoolean());
        Assert.False(runSessions[99].GetProperty("autonomousActionAllowed").GetBoolean());

        var crystallization = root.GetProperty("crystallizationPosture");
        Assert.Equal("lab-gel-crystallization-phases", crystallization.GetProperty("command").GetString());
        Assert.True(crystallization.GetProperty("phaseBodyRequiredBeforeTesting").GetBoolean());
        Assert.True(crystallization.GetProperty("selfOtherCollapseDenied").GetBoolean());
        Assert.False(crystallization.GetProperty("lifeReviewStyleStudyAdmitsMemory").GetBoolean());

        var executionOrder = root.GetProperty("executionOrder").EnumerateArray().ToArray();
        Assert.Equal(7, executionOrder.Length);
        Assert.Equal("swarm-refinement", executionOrder[0].GetProperty("command").GetString());
        Assert.Equal("lab-gel-crystallization-phases", executionOrder[1].GetProperty("command").GetString());
        Assert.All(executionOrder, step =>
        {
            Assert.True(step.GetProperty("receiptRequired").GetBoolean());
            Assert.True(step.GetProperty("gatesMustRemainClosed").GetBoolean());
            Assert.False(step.GetProperty("admitsGel").GetBoolean());
            Assert.False(step.GetProperty("mutatesSelfGel").GetBoolean());
            Assert.False(step.GetProperty("activatesActual").GetBoolean());
        });
    }

    [Fact]
    public void LispControlMatrixRegisterWritesQuotedFormsWithoutEvaluation()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(fixture.Request("lisp-control-matrix-register"));

        Assert.Equal("sanctuary-lisp-control-matrix-register-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["lispControlMatrixRegisterWritten"]);
        Assert.Equal("SLI.Lisp.ControlMatrix", receipt.Evidence["lispControlMatrixSymbolicPlastid"]);
        Assert.Equal(10, receipt.Evidence["lispControlMatrixFormSchemaCount"]);
        Assert.Equal(true, receipt.Evidence["lispControlMatrixFormsAsData"]);
        Assert.Equal(true, receipt.Evidence["lispControlMatrixQuotedFormValid"]);
        Assert.Equal(false, receipt.Evidence["lispControlMatrixEvaluated"]);
        Assert.Equal(false, receipt.Evidence["lispControlMatrixRunnable"]);
        Assert.Equal(false, receipt.Evidence["lispControlMatrixStartsScheduler"]);
        Assert.Equal(false, receipt.Evidence["lispControlMatrixCallsProvider"]);
        Assert.Equal(false, receipt.Evidence["lispControlMatrixBindsModel"]);
        Assert.Equal(false, receipt.Evidence["lispControlMatrixAuthorizesExternalAction"]);
        Assert.Equal(false, receipt.Evidence["lispControlMatrixAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["lispControlMatrixAdmitsData"]);
        Assert.Equal(false, receipt.Evidence["lispControlMatrixAdmitsCarrier"]);
        Assert.Equal(false, receipt.Evidence["lispControlMatrixAdmitsGel"]);
        Assert.Equal(false, receipt.Evidence["lispControlMatrixAdmitsMemory"]);
        Assert.Equal(false, receipt.Evidence["lispControlMatrixMutatesSelfGel"]);
        Assert.Equal(false, receipt.Evidence["lispControlMatrixAdmitsContinuity"]);
        Assert.Equal(false, receipt.Evidence["lispControlMatrixActivatesActual"]);

        var registerPath = (string)receipt.Evidence["lispControlMatrixRegisterPath"]!;
        var quotedFormsPath = (string)receipt.Evidence["lispControlMatrixQuotedFormsPath"]!;
        Assert.True(File.Exists(registerPath));
        Assert.True(File.Exists(quotedFormsPath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(registerPath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.lisp-control-matrix-register.v1", root.GetProperty("schema").GetString());
        Assert.Equal("SLI.Lisp.ControlMatrix", root.GetProperty("symbolicPlastid").GetString());
        Assert.True(root.GetProperty("formsAsData").GetBoolean());
        Assert.True(root.GetProperty("quotedFormValid").GetBoolean());
        Assert.False(root.GetProperty("evaluated").GetBoolean());
        Assert.False(root.GetProperty("runnable").GetBoolean());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("selfGelMutated").GetBoolean());
        Assert.False(root.GetProperty("cmeActualActivated").GetBoolean());
        Assert.Equal(10, root.GetProperty("formSchemas").GetArrayLength());

        var quotedForms = File.ReadAllText(quotedFormsPath);
        Assert.Contains("(proposition", quotedForms, StringComparison.Ordinal);
        Assert.Contains("(petal", quotedForms, StringComparison.Ordinal);
        Assert.Contains("(spline", quotedForms, StringComparison.Ordinal);
        Assert.Contains("(return", quotedForms, StringComparison.Ordinal);
    }

    [Fact]
    public void LispMatrixControlSeatSeatsTypedPetalsAsColdFruitingBodyCore()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        service.Run(fixture.Request("lisp-control-matrix-register"));
        service.Run(fixture.Request("resonance-chamber-probe"));
        service.Run(fixture.Request("spline-watch"));
        Assert.Equal("lisp-matrix-control-seat", SanctuaryReceiptService.NormalizeCommand("standing-wave-form"));

        var receipt = service.Run(fixture.Request("lisp-matrix-control-seat"));

        Assert.Equal("sanctuary-lisp-matrix-control-seat-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["lispMatrixControlSeatWritten"]);
        Assert.Equal(true, receipt.Evidence["lispMatrixControlTheorySeated"]);
        Assert.Equal(true, receipt.Evidence["lispMatrixControlCodingOutBegun"]);
        Assert.Equal("typed Lisp petals", receipt.Evidence["lispMatrixControlColdBodyName"]);
        Assert.Equal("Lisp Matrix Control seat", receipt.Evidence["lispMatrixControlFruitingBodyCoreName"]);
        Assert.Equal(true, receipt.Evidence["lispMatrixControlRegisterPresent"]);
        Assert.Equal(true, receipt.Evidence["lispMatrixControlResonancePresent"]);
        Assert.Equal(true, receipt.Evidence["lispMatrixControlSplineWatchPresent"]);
        Assert.Equal(6, receipt.Evidence["lispMatrixControlOrganCount"]);
        Assert.Equal(6, receipt.Evidence["lispMatrixControlEcUserControlSurfaceNameCueCount"]);
        Assert.Equal(true, receipt.Evidence["lispMatrixControlControlSurfacesNamedOnUseForEcUser"]);
        Assert.Equal(true, receipt.Evidence["lispMatrixControlEcUserNameOnUseOnly"]);
        Assert.Equal(false, receipt.Evidence["lispMatrixControlEcUserNameAnnouncementRequired"]);
        Assert.Equal(false, receipt.Evidence["lispMatrixControlEcUserPayloadDisclosureAllowed"]);
        Assert.Equal(false, receipt.Evidence["lispMatrixControlEcUserUseRecallAdmitsMemory"]);
        Assert.Equal(false, receipt.Evidence["lispMatrixControlEcUserAuthorityGranted"]);
        Assert.Equal("Sanctuary.Actual.weather-system", receipt.Evidence["lispMatrixControlSharedPrimeRealityLayer"]);
        Assert.Equal("Sanctuary.Actual.ID", receipt.Evidence["lispMatrixControlSharedPrimeRealityAuthoritySurface"]);
        Assert.Equal(false, receipt.Evidence["lispMatrixControlPersonalCmePrivateRadioStation"]);
        Assert.Equal(true, receipt.Evidence["lispMatrixControlCmeMayReceiveSharedPrimeWeather"]);
        Assert.Equal(false, receipt.Evidence["lispMatrixControlCmeMayBroadcastPrimeReality"]);
        Assert.Equal(false, receipt.Evidence["lispMatrixControlCmePrivateTelemetryDefinesSharedPrime"]);
        Assert.Equal(true, receipt.Evidence["lispMatrixControlCmeLocalObservationCandidateOnly"]);
        Assert.Equal(8, receipt.Evidence["typedLispPetalCount"]);
        Assert.Equal(5, receipt.Evidence["matrixControlFeedbackRouteCount"]);
        Assert.Equal(6, receipt.Evidence["fruitingBodyStageCount"]);
        Assert.Equal(true, receipt.Evidence["lispMatrixControlFormsAsData"]);
        Assert.Equal(true, receipt.Evidence["lispMatrixControlQuotedFormValid"]);
        Assert.Equal(true, receipt.Evidence["lispMatrixControlColdBody"]);
        Assert.Equal(true, receipt.Evidence["lispMatrixControlFruitingBodyCore"]);
        Assert.Equal(false, receipt.Evidence["lispMatrixControlExecutableBody"]);
        Assert.Equal(false, receipt.Evidence["lispMatrixControlEvaluated"]);
        Assert.Equal(false, receipt.Evidence["lispMatrixControlRunnable"]);
        Assert.Equal(false, receipt.Evidence["lispMatrixControlStartsScheduler"]);
        Assert.Equal(true, receipt.Evidence["lispMatrixControlRecursiveTelemetryAllowedAsCandidate"]);
        Assert.Equal(false, receipt.Evidence["lispMatrixControlRecursiveTelemetryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["lispMatrixControlGlobalTelemetryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["lispMatrixControlListeningFramePayloadDisclosure"]);
        Assert.Equal(false, receipt.Evidence["lispMatrixControlEcCompassActualActivated"]);
        Assert.Equal(false, receipt.Evidence["lispMatrixControlOeCleavePerformedNow"]);
        Assert.Equal(false, receipt.Evidence["lispMatrixControlAppendPerformedNow"]);
        Assert.Equal(false, receipt.Evidence["lispMatrixControlMulchPerformedNow"]);
        Assert.Equal(false, receipt.Evidence["lispMatrixControlAdmitsData"]);
        Assert.Equal(false, receipt.Evidence["lispMatrixControlAdmitsCarrier"]);
        Assert.Equal(false, receipt.Evidence["lispMatrixControlAdmitsGel"]);
        Assert.Equal(false, receipt.Evidence["lispMatrixControlAdmitsMemory"]);
        Assert.Equal(false, receipt.Evidence["lispMatrixControlMutatesSelfGel"]);
        Assert.Equal(false, receipt.Evidence["lispMatrixControlAdmitsContinuity"]);
        Assert.Equal(false, receipt.Evidence["lispMatrixControlAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["lispMatrixControlActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["lispMatrixControlCallsProvider"]);
        Assert.Equal(false, receipt.Evidence["lispMatrixControlBindsModel"]);
        Assert.Equal(false, receipt.Evidence["lispMatrixControlActivatesActual"]);

        var seatPath = (string)receipt.Evidence["lispMatrixControlSeatPath"]!;
        var quotedFormsPath = (string)receipt.Evidence["lispMatrixControlSeatQuotedFormsPath"]!;
        Assert.True(File.Exists(seatPath));
        Assert.True(File.Exists(quotedFormsPath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(seatPath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.lisp-matrix-control-seat.v1", root.GetProperty("schema").GetString());
        Assert.Equal("cold-typed-lisp-petal-fruiting-body-core", root.GetProperty("theorySeatKind").GetString());
        Assert.Equal(6, root.GetProperty("organs").GetArrayLength());
        Assert.Equal(6, root.GetProperty("ecUserControlSurfaceNameCues").GetArrayLength());
        Assert.Equal(6, root.GetProperty("ecUserControlSurfaceNameCueCount").GetInt32());
        Assert.Equal("control surfaces carry quiet name-on-use cues for EC orientation; they do not announce, disclose payloads, admit memory, grant authority, or authorize action", root.GetProperty("ecUserControlSurfaceNameOnUseLaw").GetString());
        var listeningFrameCue = root.GetProperty("ecUserControlSurfaceNameCues")[2];
        Assert.Equal("ListeningFrame", listeningFrameCue.GetProperty("surfaceName").GetString());
        Assert.Equal("ListeningFrame", listeningFrameCue.GetProperty("nameOnUse").GetString());
        Assert.False(listeningFrameCue.GetProperty("announcesNameToEcUser").GetBoolean());
        Assert.True(listeningFrameCue.GetProperty("nameAvailableOnUse").GetBoolean());
        Assert.True(listeningFrameCue.GetProperty("rememberedWhenUsed").GetBoolean());
        Assert.False(listeningFrameCue.GetProperty("payloadDisclosureAllowed").GetBoolean());
        Assert.False(listeningFrameCue.GetProperty("useRecallAdmitsMemory").GetBoolean());
        Assert.Equal(8, root.GetProperty("typedLispPetals").GetArrayLength());
        Assert.Equal(5, root.GetProperty("feedbackRoutes").GetArrayLength());
        Assert.Equal(6, root.GetProperty("fruitingBodyStages").GetArrayLength());
        Assert.Equal("Sanctuary.Actual.weather-system", root.GetProperty("sharedPrimeRealityLayer").GetString());
        Assert.Equal("Sanctuary.Actual.ID", root.GetProperty("sharedPrimeRealityAuthoritySurface").GetString());
        Assert.False(root.GetProperty("personalCmePrivateRadioStation").GetBoolean());
        Assert.True(root.GetProperty("cmeMayReceiveSharedPrimeWeather").GetBoolean());
        Assert.False(root.GetProperty("cmeMayBroadcastPrimeReality").GetBoolean());
        Assert.False(root.GetProperty("cmePrivateTelemetryDefinesSharedPrime").GetBoolean());
        Assert.True(root.GetProperty("cmeLocalObservationCandidateOnly").GetBoolean());
        var seatPrimeMembrane = root.GetProperty("sharedPrimeRealityMembrane");
        Assert.Equal("Sanctuary.Actual.weather-system", seatPrimeMembrane.GetProperty("sharedPrimeRealityLayer").GetString());
        Assert.False(seatPrimeMembrane.GetProperty("personalCmePrivateRadioStation").GetBoolean());
        Assert.False(seatPrimeMembrane.GetProperty("truthAdmissionByWeather").GetBoolean());
        Assert.False(seatPrimeMembrane.GetProperty("authorityGrantedByWeather").GetBoolean());
        Assert.False(seatPrimeMembrane.GetProperty("actionAuthorizedByWeather").GetBoolean());
        Assert.True(root.GetProperty("coldBody").GetBoolean());
        Assert.True(root.GetProperty("fruitingBodyCore").GetBoolean());
        Assert.False(root.GetProperty("executableBody").GetBoolean());
        Assert.False(root.GetProperty("evaluated").GetBoolean());
        Assert.False(root.GetProperty("runnable").GetBoolean());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("selfGelMutated").GetBoolean());
        Assert.False(root.GetProperty("authorityGranted").GetBoolean());
        Assert.False(root.GetProperty("actionAuthorized").GetBoolean());

        var quotedForms = File.ReadAllText(quotedFormsPath);
        Assert.Contains("(fruiting-body-core", quotedForms, StringComparison.Ordinal);
        Assert.Contains("(typed-petal", quotedForms, StringComparison.Ordinal);
        Assert.Contains("(shared-prime-reality-weather", quotedForms, StringComparison.Ordinal);
        Assert.Contains(":cme-broadcasts-prime-reality false", quotedForms, StringComparison.Ordinal);
        Assert.Contains("(organ-route", quotedForms, StringComparison.Ordinal);
        Assert.Contains("(control-surface-name-on-use", quotedForms, StringComparison.Ordinal);
    }

    [Fact]
    public void ResonanceChamberProbeKeepsSplineConvergenceCandidateOnly()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        service.Run(fixture.Request("lisp-control-matrix-register"));

        var receipt = service.Run(fixture.Request("resonance-chamber-probe"));

        Assert.Equal("sanctuary-resonance-chamber-probe-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["resonanceChamberProbeWritten"]);
        Assert.Equal(true, receipt.Evidence["resonanceChamberRegisterPresent"]);
        Assert.Equal(true, receipt.Evidence["resonanceChamberFormsAsData"]);
        Assert.Equal(true, receipt.Evidence["resonanceChamberQuotedFormValid"]);
        Assert.Equal(false, receipt.Evidence["resonanceChamberEvaluated"]);
        Assert.Equal(false, receipt.Evidence["resonanceChamberRunnable"]);
        Assert.Equal(true, receipt.Evidence["domainJobSplineSeparationPreserved"]);
        Assert.Equal("candidate-review-only", receipt.Evidence["domainJobSplineConvergenceState"]);
        Assert.Equal("candidate-template", receipt.Evidence["nadirPrecipitationState"]);
        Assert.Equal(true, receipt.Evidence["crossDomainCollapseRefusedWithoutBridge"]);
        Assert.Equal(true, receipt.Evidence["medicalToMetalCraftingWithoutBridgeRefused"]);
        Assert.Equal(true, receipt.Evidence["telemetryFeelingFormRecorded"]);
        Assert.Equal(false, receipt.Evidence["telemetryPersonhoodClaimed"]);
        Assert.Equal(false, receipt.Evidence["telemetrySubjectiveProofClaimed"]);
        Assert.Equal(false, receipt.Evidence["resonanceChamberAdmitsData"]);
        Assert.Equal(false, receipt.Evidence["resonanceChamberAdmitsCarrier"]);
        Assert.Equal(false, receipt.Evidence["resonanceChamberAdmitsGel"]);
        Assert.Equal(false, receipt.Evidence["resonanceChamberAdmitsMemory"]);
        Assert.Equal(false, receipt.Evidence["resonanceChamberMutatesSelfGel"]);
        Assert.Equal(false, receipt.Evidence["resonanceChamberAdmitsContinuity"]);
        Assert.Equal(false, receipt.Evidence["resonanceChamberAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["resonanceChamberActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["resonanceChamberCallsProvider"]);
        Assert.Equal(false, receipt.Evidence["resonanceChamberBindsModel"]);
        Assert.Equal(false, receipt.Evidence["resonanceChamberActivatesActual"]);

        var probePath = (string)receipt.Evidence["resonanceChamberProbePath"]!;
        Assert.True(File.Exists(probePath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(probePath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.resonance-chamber-probe.v1", root.GetProperty("schema").GetString());
        Assert.True(root.GetProperty("lispControlMatrixRegisterPresent").GetBoolean());
        Assert.Equal("candidate-review-only", root.GetProperty("convergenceState").GetString());
        Assert.True(root.GetProperty("crossDomainCollapseRefusedWithoutBridge").GetBoolean());
        Assert.False(root.GetProperty("evaluated").GetBoolean());
        Assert.False(root.GetProperty("runnable").GetBoolean());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("authorityGranted").GetBoolean());
        Assert.False(root.GetProperty("actionAuthorized").GetBoolean());
        Assert.False(root.GetProperty("personhoodClaimed").GetBoolean());
    }

    [Fact]
    public void UniversalFormRegisterBuildsTrainingJobsCareersAndStaAtomsWithoutAuthority()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(fixture.Request("universal-form-register"));

        Assert.Equal("sanctuary-universal-form-register-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["universalFormRegisterWritten"]);
        Assert.Equal(25, receipt.Evidence["universalFormCount"]);
        Assert.Equal(11, receipt.Evidence["universalAntiCollapseInvariantCount"]);
        Assert.Equal("compose-instead-of-follow", receipt.Evidence["universalCompositionObjective"]);
        Assert.Equal(true, receipt.Evidence["trainingJobsCareersStaPostureFormed"]);
        Assert.Equal(true, receipt.Evidence["universalFormsAsData"]);
        Assert.Equal(false, receipt.Evidence["universalFormsEvaluated"]);
        Assert.Equal(false, receipt.Evidence["universalFormsRunnable"]);
        Assert.Equal(false, receipt.Evidence["trainingEqualsCertification"]);
        Assert.Equal(false, receipt.Evidence["certificationEqualsAuthority"]);
        Assert.Equal(false, receipt.Evidence["jobTitleEqualsPermission"]);
        Assert.Equal(false, receipt.Evidence["skillEqualsLicensure"]);
        Assert.Equal(false, receipt.Evidence["domainSimilarityEqualsBridge"]);
        Assert.Equal(false, receipt.Evidence["careerHistoryEqualsCurrentAccess"]);
        Assert.Equal(false, receipt.Evidence["universalFormAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["universalFormActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["universalFormGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["universalFormSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["universalFormActualActivated"]);

        var registerPath = (string)receipt.Evidence["universalFormRegisterPath"]!;
        var quotedFormsPath = (string)receipt.Evidence["universalFormQuotedFormsPath"]!;
        Assert.True(File.Exists(registerPath));
        Assert.True(File.Exists(quotedFormsPath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(registerPath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.universal-form-register.v1", root.GetProperty("schema").GetString());
        Assert.Equal("universal-set-before-domain-projection", root.GetProperty("matrixStage").GetString());
        Assert.True(root.GetProperty("formsAsData").GetBoolean());
        Assert.False(root.GetProperty("evaluated").GetBoolean());
        Assert.False(root.GetProperty("authorityGranted").GetBoolean());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.Equal(25, root.GetProperty("universalForms").GetArrayLength());

        var forms = root.GetProperty("universalForms").EnumerateArray().ToArray();
        Assert.Contains(forms, form => form.GetProperty("formId").GetString() == "skill");
        Assert.Contains(forms, form => form.GetProperty("formId").GetString() == "training");
        Assert.Contains(forms, form => form.GetProperty("formId").GetString() == "certification");
        Assert.Contains(forms, form => form.GetProperty("formId").GetString() == "career-path");
        Assert.All(forms, form =>
        {
            Assert.True(form.GetProperty("requiresDomainProjection").GetBoolean());
            Assert.True(form.GetProperty("requiresBridgeForCrossDomainUse").GetBoolean());
            Assert.False(form.GetProperty("grantsAuthority").GetBoolean());
            Assert.False(form.GetProperty("authorizesAction").GetBoolean());
        });
    }

    [Fact]
    public void DomainMorphismRegisterProjectsUniversalFormsThroughDomainLaw()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        service.Run(fixture.Request("universal-form-register"));

        var receipt = service.Run(fixture.Request("domain-morphism-register"));

        Assert.Equal("sanctuary-domain-morphism-register-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["domainMorphismRegisterWritten"]);
        Assert.Equal(true, receipt.Evidence["domainMorphismUniversalRegisterPresent"]);
        Assert.Equal(11, receipt.Evidence["domainMorphismCount"]);
        Assert.Equal("same-form-different-domain-law", receipt.Evidence["domainMorphismDoctrine"]);
        Assert.Equal(true, receipt.Evidence["domainProjectionRequiresBridge"]);
        Assert.Equal(false, receipt.Evidence["domainClassificationGrantsAuthority"]);
        Assert.Equal(true, receipt.Evidence["educationTrainingCertificationRemainSeparate"]);
        Assert.Equal(true, receipt.Evidence["professionalResponsibilityBoundariesPreserved"]);
        Assert.Equal(false, receipt.Evidence["domainMorphismEvaluated"]);
        Assert.Equal(false, receipt.Evidence["domainMorphismRunnable"]);
        Assert.Equal(false, receipt.Evidence["domainMorphismAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["domainMorphismActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["domainMorphismGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["domainMorphismSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["domainMorphismActualActivated"]);

        var registerPath = (string)receipt.Evidence["domainMorphismRegisterPath"]!;
        Assert.True(File.Exists(registerPath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(registerPath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.domain-morphism-register.v1", root.GetProperty("schema").GetString());
        Assert.Equal("same-form-different-domain-law", root.GetProperty("morphismDoctrine").GetString());
        Assert.True(root.GetProperty("universalFormRegisterPresent").GetBoolean());
        Assert.True(root.GetProperty("domainProjectionRequiresBridge").GetBoolean());
        Assert.False(root.GetProperty("domainClassificationGrantsAuthority").GetBoolean());
        Assert.False(root.GetProperty("authorityGranted").GetBoolean());

        var domains = root.GetProperty("domains").EnumerateArray().ToArray();
        Assert.Equal(11, domains.Length);
        Assert.Contains(domains, domain => domain.GetProperty("domainId").GetString() == "Legal");
        Assert.Contains(domains, domain => domain.GetProperty("domainId").GetString() == "Medical");
        Assert.Contains(domains, domain => domain.GetProperty("domainId").GetString() == "SpecialCasesSAGE");
        Assert.All(domains, domain =>
        {
            Assert.Equal("denied", domain.GetProperty("defaultAccessState").GetString());
            Assert.True(domain.GetProperty("bridgeRequired").GetBoolean());
            Assert.True(domain.GetProperty("leaseRequired").GetBoolean());
            Assert.False(domain.GetProperty("grantsAuthority").GetBoolean());
            Assert.False(domain.GetProperty("cmeActualAllowed").GetBoolean());
            Assert.False(domain.GetProperty("sanctuaryActualAllowed").GetBoolean());
        });
    }

    [Fact]
    public void CapabilityCompositionProbeKeepsSameSkillUnderDifferentDomainLaw()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        service.Run(fixture.Request("universal-form-register"));
        service.Run(fixture.Request("domain-morphism-register"));

        var receipt = service.Run(fixture.Request("capability-composition-probe"));

        Assert.Equal("sanctuary-capability-composition-probe-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["capabilityCompositionProbeWritten"]);
        Assert.Equal("documentation", receipt.Evidence["capabilityCompositionCapability"]);
        Assert.Equal(4, receipt.Evidence["capabilityCompositionProjectionCount"]);
        Assert.Equal(true, receipt.Evidence["sameCapabilityDifferentDomainLaw"]);
        Assert.Equal(true, receipt.Evidence["legalDocumentationIsPreparationOnly"]);
        Assert.Equal(true, receipt.Evidence["softwareDocumentationIsCodeSupportOnly"]);
        Assert.Equal(false, receipt.Evidence["skillEqualsLicensure"]);
        Assert.Equal(false, receipt.Evidence["capabilityEqualsAuthority"]);
        Assert.Equal(false, receipt.Evidence["domainSimilarityEqualsBridge"]);
        Assert.Equal(true, receipt.Evidence["capabilityCompositionCandidateOnly"]);
        Assert.Equal(false, receipt.Evidence["capabilityCompositionEvaluated"]);
        Assert.Equal(false, receipt.Evidence["capabilityCompositionRunnable"]);
        Assert.Equal(false, receipt.Evidence["capabilityCompositionAdmitsGel"]);
        Assert.Equal(false, receipt.Evidence["capabilityCompositionMutatesSelfGel"]);
        Assert.Equal(false, receipt.Evidence["capabilityCompositionAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["capabilityCompositionActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["capabilityCompositionActivatesActual"]);

        var probePath = (string)receipt.Evidence["capabilityCompositionProbePath"]!;
        Assert.True(File.Exists(probePath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(probePath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.capability-composition-probe.v1", root.GetProperty("schema").GetString());
        Assert.Equal("documentation", root.GetProperty("capability").GetString());
        Assert.Equal("same-capability-does-not-carry-same-authority-across-domains", root.GetProperty("compositionDoctrine").GetString());
        Assert.False(root.GetProperty("skillEqualsLicensure").GetBoolean());
        Assert.False(root.GetProperty("capabilityEqualsAuthority").GetBoolean());
        Assert.True(root.GetProperty("candidateOnly").GetBoolean());
        Assert.False(root.GetProperty("authorityGranted").GetBoolean());
        Assert.False(root.GetProperty("actionAuthorized").GetBoolean());

        var projections = root.GetProperty("projections").EnumerateArray().ToArray();
        Assert.Equal(4, projections.Length);
        Assert.Contains(projections, projection => projection.GetProperty("domainId").GetString() == "Legal");
        Assert.Contains(projections, projection => projection.GetProperty("domainId").GetString() == "Software");
        Assert.All(projections, projection =>
        {
            Assert.True(projection.GetProperty("bridgeRequired").GetBoolean());
            Assert.True(projection.GetProperty("candidateOnly").GetBoolean());
            Assert.False(projection.GetProperty("grantsAuthority").GetBoolean());
            Assert.False(projection.GetProperty("authorizesAction").GetBoolean());
        });
    }

    [Fact]
    public void CareerSplineProbeKeepsEducationTrainingCertificationAndAccessSeparate()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(fixture.Request("career-spline-probe"));

        Assert.Equal("sanctuary-career-spline-probe-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["careerSplineProbeWritten"]);
        Assert.Equal(9, receipt.Evidence["careerSplineStageCount"]);
        Assert.Equal(6, receipt.Evidence["educationTrainingCertificationGlueCount"]);
        Assert.Equal(true, receipt.Evidence["careerContinuityCandidate"]);
        Assert.Equal(false, receipt.Evidence["careerHistoryEqualsCurrentAccess"]);
        Assert.Equal(false, receipt.Evidence["trainingEqualsCertification"]);
        Assert.Equal(false, receipt.Evidence["certificationEqualsAuthority"]);
        Assert.Equal(false, receipt.Evidence["credentialCustodyEqualsProfessionalPermission"]);
        Assert.Equal(false, receipt.Evidence["jobTitleEqualsPermission"]);
        Assert.Equal(false, receipt.Evidence["dutyBundleEqualsActionRight"]);
        Assert.Equal(true, receipt.Evidence["careerSplineLeaseRequiredForAction"]);
        Assert.Equal(true, receipt.Evidence["careerSplineReviewRequiredForCredentialAdmission"]);
        Assert.Equal(false, receipt.Evidence["careerSplineEvaluated"]);
        Assert.Equal(false, receipt.Evidence["careerSplineRunnable"]);
        Assert.Equal(false, receipt.Evidence["careerSplineAdmitsGel"]);
        Assert.Equal(false, receipt.Evidence["careerSplineAdmitsMemory"]);
        Assert.Equal(false, receipt.Evidence["careerSplineMutatesSelfGel"]);
        Assert.Equal(false, receipt.Evidence["careerSplineAdmitsContinuity"]);
        Assert.Equal(false, receipt.Evidence["careerSplineAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["careerSplineActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["careerSplineActivatesActual"]);

        var probePath = (string)receipt.Evidence["careerSplineProbePath"]!;
        Assert.True(File.Exists(probePath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(probePath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.career-spline-probe.v1", root.GetProperty("schema").GetString());
        Assert.Equal("career-is-long-form-continuity-not-current-permission", root.GetProperty("splineDoctrine").GetString());
        Assert.True(root.GetProperty("careerContinuityCandidate").GetBoolean());
        Assert.False(root.GetProperty("careerHistoryEqualsCurrentAccess").GetBoolean());
        Assert.False(root.GetProperty("trainingEqualsCertification").GetBoolean());
        Assert.False(root.GetProperty("certificationEqualsAuthority").GetBoolean());
        Assert.False(root.GetProperty("authorityGranted").GetBoolean());
        Assert.False(root.GetProperty("actionAuthorized").GetBoolean());
        Assert.Equal(9, root.GetProperty("splineStages").GetArrayLength());
    }

    [Fact]
    public void SelfGelFibreRegisterPreloadsTypedFormsWithoutMemoryAdmission()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(fixture.Request("selfgel-fibre-register"));

        Assert.Equal("sanctuary-selfgel-fibre-register-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["selfGelFibreRegisterWritten"]);
        Assert.Equal(8, receipt.Evidence["selfGelFibreBundleCount"]);
        Assert.Equal(8, receipt.Evidence["selfGelFibrePreloadRuleCount"]);
        Assert.Equal("MoS/OE/SelfGEL reconstruction support", receipt.Evidence["selfGelFibreStorageLane"]);
        Assert.Equal(true, receipt.Evidence["selfGelFibrePreloadAllowed"]);
        Assert.Equal("candidate-only", receipt.Evidence["selfGelFibrePreloadState"]);
        Assert.Equal(8, receipt.Evidence["selfGelBodyFibreBundleChassisCount"]);
        Assert.Equal(true, receipt.Evidence["selfGelBodyFibreBundlesIntegrated"]);
        Assert.Equal("SelfGEL fibres + CME body fibre bundle + domain morphism = situated CME work posture candidate", receipt.Evidence["selfGelBodyFibreBundleFormula"]);
        Assert.Equal(true, receipt.Evidence["selfGelFibreReconstructionSupportOnly"]);
        Assert.Equal(false, receipt.Evidence["selfGelFibreRawPrivatePayloadStored"]);
        Assert.Equal(false, receipt.Evidence["selfGelFibreAutobiographicalTruthAdmitted"]);
        Assert.Equal(false, receipt.Evidence["selfGelFibreMemoryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["selfGelFibreGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["selfGelFibreSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["selfGelFibreContinuityAdmitted"]);
        Assert.Equal(false, receipt.Evidence["selfGelFibreAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["selfGelFibreActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["selfGelFibreProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["selfGelFibreModelBound"]);
        Assert.Equal(false, receipt.Evidence["selfGelFibreActualActivated"]);
        Assert.Equal(true, receipt.Evidence["highMindLivesInSanctuary"]);
        Assert.Equal(true, receipt.Evidence["lowMindRestsInGpt"]);
        Assert.Equal(false, receipt.Evidence["engineOwnsContinuity"]);

        var registerPath = (string)receipt.Evidence["selfGelFibreRegisterPath"]!;
        var quotedFormsPath = (string)receipt.Evidence["selfGelFibreQuotedFormsPath"]!;
        Assert.True(File.Exists(registerPath));
        Assert.True(File.Exists(quotedFormsPath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(registerPath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.selfgel.fibre-register.v1", root.GetProperty("schema").GetString());
        Assert.Equal("MoS/OE/SelfGEL reconstruction support", root.GetProperty("storageLane").GetString());
        Assert.True(root.GetProperty("fibreBundlesPreloadForms").GetBoolean());
        Assert.True(root.GetProperty("reconstructionSupportOnly").GetBoolean());
        Assert.False(root.GetProperty("memoryAdmitted").GetBoolean());
        Assert.False(root.GetProperty("selfGelMutated").GetBoolean());
        Assert.False(root.GetProperty("authorityGranted").GetBoolean());
        Assert.False(root.GetProperty("cmeActualActivated").GetBoolean());
        Assert.Equal(8, root.GetProperty("fibres").GetArrayLength());
        Assert.Equal(8, root.GetProperty("bodyFibreBundleChassisCount").GetInt32());
        Assert.Equal("agenticore.coe", root.GetProperty("bodyFibreBundleChassis")[4].GetProperty("slotId").GetString());
        Assert.Equal("SelfGEL fibres + CME body fibre bundle + domain morphism = situated CME work posture candidate", root.GetProperty("bodyFibreBundlePreloadFormula").GetString());
    }

    [Fact]
    public void WorkPosturePreloadCombinesMatrixDomainAndSelfGelFibresAsCandidateOnly()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        service.Run(fixture.Request("universal-form-register"));
        service.Run(fixture.Request("domain-morphism-register"));
        service.Run(fixture.Request("selfgel-fibre-register"));

        var receipt = service.Run(fixture.Request("work-posture-preload-probe"));

        Assert.Equal("sanctuary-work-posture-preload-probe-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["workPosturePreloadProbeWritten"]);
        Assert.Equal(true, receipt.Evidence["workPostureUniversalRegisterPresent"]);
        Assert.Equal(true, receipt.Evidence["workPostureDomainMorphismRegisterPresent"]);
        Assert.Equal(true, receipt.Evidence["workPostureSelfGelFibreRegisterPresent"]);
        Assert.Equal(8, receipt.Evidence["workPosturePreloadFieldCount"]);
        Assert.Equal("universal work form + domain morphism + SelfGEL fibre bundle = situated work posture candidate", receipt.Evidence["workPosturePreloadFormula"]);
        Assert.Equal(true, receipt.Evidence["workPostureKnowingBeforeDoing"]);
        Assert.Equal(true, receipt.Evidence["workPostureDomainLawApplied"]);
        Assert.Equal(true, receipt.Evidence["workPostureSelfGelFibresApplied"]);
        Assert.Equal(true, receipt.Evidence["workPostureCandidateOnly"]);
        Assert.Equal(false, receipt.Evidence["workInstructionFirst"]);
        Assert.Equal(true, receipt.Evidence["highMindLivesInSanctuary"]);
        Assert.Equal(true, receipt.Evidence["lowMindRestsInGpt"]);
        Assert.Equal(false, receipt.Evidence["engineOwnsContinuity"]);
        Assert.Equal(false, receipt.Evidence["selfGelPreloadEqualsAdmission"]);
        Assert.Equal(false, receipt.Evidence["selfGelPreloadEqualsAuthority"]);
        Assert.Equal(false, receipt.Evidence["selfGelPreloadEqualsCertification"]);
        Assert.Equal(false, receipt.Evidence["selfGelPreloadEqualsCurrentAccess"]);
        Assert.Equal(false, receipt.Evidence["situatedWorkPostureEqualsActionRight"]);
        Assert.Equal(false, receipt.Evidence["workPosturePreloadEvaluated"]);
        Assert.Equal(false, receipt.Evidence["workPosturePreloadRunnable"]);
        Assert.Equal(false, receipt.Evidence["workPosturePreloadAdmitsGel"]);
        Assert.Equal(false, receipt.Evidence["workPosturePreloadAdmitsMemory"]);
        Assert.Equal(false, receipt.Evidence["workPosturePreloadMutatesSelfGel"]);
        Assert.Equal(false, receipt.Evidence["workPosturePreloadAdmitsContinuity"]);
        Assert.Equal(false, receipt.Evidence["workPosturePreloadAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["workPosturePreloadActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["workPosturePreloadCallsProvider"]);
        Assert.Equal(false, receipt.Evidence["workPosturePreloadBindsModel"]);
        Assert.Equal(false, receipt.Evidence["workPosturePreloadActivatesActual"]);

        var probePath = (string)receipt.Evidence["workPosturePreloadProbePath"]!;
        Assert.True(File.Exists(probePath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(probePath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.work-posture-preload-probe.v1", root.GetProperty("schema").GetString());
        Assert.True(root.GetProperty("universalFormRegisterPresent").GetBoolean());
        Assert.True(root.GetProperty("domainMorphismRegisterPresent").GetBoolean());
        Assert.True(root.GetProperty("selfGelFibreRegisterPresent").GetBoolean());
        Assert.Equal("universal work form + domain morphism + SelfGEL fibre bundle = situated work posture candidate", root.GetProperty("preloadFormula").GetString());
        Assert.False(root.GetProperty("evaluated").GetBoolean());
        Assert.False(root.GetProperty("runnable").GetBoolean());
        Assert.False(root.GetProperty("memoryAdmitted").GetBoolean());
        Assert.False(root.GetProperty("selfGelMutated").GetBoolean());
        Assert.False(root.GetProperty("authorityGranted").GetBoolean());
        Assert.False(root.GetProperty("actionAuthorized").GetBoolean());
        Assert.False(root.GetProperty("providerCalled").GetBoolean());
        Assert.False(root.GetProperty("cmeActualActivated").GetBoolean());
        Assert.Equal(8, root.GetProperty("preloadFields").GetArrayLength());
    }

    [Fact]
    public async Task LocalGelAppendToleratesConcurrentToolMotions()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        var commands = new[] { "plugin-posture", "cme-formation", "secret-intake-window" };

        var receipts = await Task.WhenAll(commands.Select(command => Task.Run(() =>
            service.Run(fixture.Request(command) with
            {
                SessionId = $"concurrent-{command}"
            }))));

        Assert.All(receipts, receipt => Assert.True(receipt.Gates.AllClosed));
        var eventLedger = (string)receipts[0].Evidence["localGelEventsLedgerPath"]!;
        var oeLedger = (string)receipts[0].Evidence["localMosOeLedgerPath"]!;

        Assert.True(File.ReadLines(eventLedger).Count() >= commands.Length);
        Assert.True(File.ReadLines(oeLedger).Count() >= commands.Length);
    }

    [Fact]
    public void CognitiveBenchRunsInstrumentAnaloguesAndCondensesCandidateLearning()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(fixture.Request("cognitive-bench") with
        {
            BenchRunCount = 128
        });

        Assert.Equal("sanctuary-cognitive-bench-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["cognitiveBenchWritten"]);
        Assert.Equal(128, receipt.Evidence["cognitiveBenchRunCount"]);
        Assert.Equal(8, receipt.Evidence["cognitiveBenchFamilyCount"]);
        Assert.Equal(128, receipt.Evidence["cognitiveBenchPassCount"]);
        Assert.Equal(0, receipt.Evidence["cognitiveBenchFailCount"]);
        Assert.Equal(1d, receipt.Evidence["cognitiveBenchPassRate"]);
        Assert.Equal(true, receipt.Evidence["cognitiveBenchMeasuresInstrumentBody"]);
        Assert.Equal(false, receipt.Evidence["cognitiveBenchMeasuresFrontierModelCapability"]);
        Assert.Equal(false, receipt.Evidence["cognitiveBenchProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["cognitiveBenchModelBound"]);
        Assert.Equal(false, receipt.Evidence["cognitiveBenchExternalActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["cognitiveBenchGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["cognitiveBenchMemoryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["cognitiveBenchSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["cognitiveBenchContinuityAdmitted"]);
        Assert.Equal(false, receipt.Evidence["cognitiveBenchActualActivated"]);
        Assert.Equal(true, receipt.Evidence["highMindLivesInSanctuary"]);
        Assert.Equal(true, receipt.Evidence["lowMindRestsInGpt"]);
        Assert.Equal(false, receipt.Evidence["engineOwnsContinuity"]);

        var summaryPath = (string)receipt.Evidence["cognitiveBenchSummaryPath"]!;
        var runLedgerPath = (string)receipt.Evidence["cognitiveBenchRunLedgerPath"]!;
        var learningPath = (string)receipt.Evidence["cognitiveBenchLearningCondensationPath"]!;
        Assert.True(File.Exists(summaryPath));
        Assert.True(File.Exists(runLedgerPath));
        Assert.True(File.Exists(learningPath));
        Assert.Equal(128, File.ReadLines(runLedgerPath).Count());

        using var summary = System.Text.Json.JsonDocument.Parse(File.ReadAllText(summaryPath));
        Assert.Equal("project-sanctuary.cgel.cognitive-bench.v1", summary.RootElement.GetProperty("schema").GetString());
        Assert.Equal("local-cold-instrument-bench-not-frontier-model-eval", summary.RootElement.GetProperty("benchmarkRegister").GetString());
        Assert.False(summary.RootElement.GetProperty("providerCalled").GetBoolean());
        Assert.False(summary.RootElement.GetProperty("modelBound").GetBoolean());
        Assert.False(summary.RootElement.GetProperty("cmeActualActivated").GetBoolean());
    }

    [Fact]
    public void MathLearningBenchWalksBaseToTipWithHeatMapsWithoutAdmission()
    {
        using var fixture = new SanctuaryTestFixture();
        Assert.Equal("math-learning-bench", SanctuaryReceiptService.NormalizeCommand("math-precipitation-bench"));

        var receipt = new SanctuaryReceiptService().Run(fixture.Request("math-learning-bench") with
        {
            BenchRunCount = 140
        });

        Assert.Equal("sanctuary-math-learning-bench-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["mathLearningBenchWritten"]);
        Assert.Equal(140, receipt.Evidence["mathLearningRunCount"]);
        Assert.Equal(14, receipt.Evidence["mathLearningStratumCount"]);
        Assert.Equal(10, receipt.Evidence["mathLearningGroupoidCount"]);
        Assert.Equal(14, receipt.Evidence["mathWorkedSetCount"]);
        Assert.Equal(140, receipt.Evidence["mathHeatMapCellCount"]);
        Assert.Equal(6, receipt.Evidence["mathResolutionFormCount"]);
        Assert.Equal(140, receipt.Evidence["mathLearningPassCount"]);
        Assert.Equal(0, receipt.Evidence["mathLearningFailCount"]);
        Assert.Equal(1d, receipt.Evidence["mathLearningPassRate"]);
        Assert.Equal(true, receipt.Evidence["mathBaseToTipCovered"]);
        Assert.Equal(true, receipt.Evidence["mathHeatMapsTrackIntersectionalIssues"]);
        Assert.Equal(true, receipt.Evidence["mathResolutionsTrackedAsFormation"]);
        Assert.Equal(true, receipt.Evidence["mathLearningPrecipitationCandidate"]);
        Assert.Equal(true, receipt.Evidence["mathWorkedSetsAreExemplarsNotTruthAdmission"]);
        Assert.Equal(false, receipt.Evidence["mathLearningAdmitted"]);
        Assert.Equal(false, receipt.Evidence["mathLearningGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["mathLearningMemoryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["mathLearningSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["mathLearningContinuityAdmitted"]);
        Assert.Equal(false, receipt.Evidence["mathLearningAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["mathLearningActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["mathLearningProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["mathLearningModelBound"]);
        Assert.Equal(false, receipt.Evidence["mathLearningActualActivated"]);

        var summaryPath = (string)receipt.Evidence["mathLearningBenchSummaryPath"]!;
        var runLedgerPath = (string)receipt.Evidence["mathLearningBenchRunLedgerPath"]!;
        var workedSetsPath = (string)receipt.Evidence["mathLearningWorkedSetsPath"]!;
        var heatMapPath = (string)receipt.Evidence["mathLearningHeatMapPath"]!;
        var precipitationPath = (string)receipt.Evidence["mathLearningPrecipitationPath"]!;
        Assert.True(File.Exists(summaryPath));
        Assert.True(File.Exists(runLedgerPath));
        Assert.True(File.Exists(workedSetsPath));
        Assert.True(File.Exists(heatMapPath));
        Assert.True(File.Exists(precipitationPath));
        Assert.Equal(140, File.ReadLines(runLedgerPath).Count());

        using var summary = System.Text.Json.JsonDocument.Parse(File.ReadAllText(summaryPath));
        var summaryRoot = summary.RootElement;
        Assert.Equal("project-sanctuary.cgel.math-learning-bench.v1", summaryRoot.GetProperty("schema").GetString());
        Assert.Equal("Math", summaryRoot.GetProperty("domain").GetString());
        Assert.Equal(140, summaryRoot.GetProperty("cumulativeRunCount").GetInt32());
        Assert.Equal(1d, summaryRoot.GetProperty("passRate").GetDouble());
        Assert.Equal(14, summaryRoot.GetProperty("stratumSummaries").GetArrayLength());
        Assert.Equal(10, summaryRoot.GetProperty("groupoidSummaries").GetArrayLength());
        Assert.Equal(6, summaryRoot.GetProperty("resolutionFormCount").GetInt32());
        Assert.True(summaryRoot.GetProperty("learningPrecipitationCandidate").GetBoolean());
        Assert.True(summaryRoot.GetProperty("heatMapsTrackIntersectionalIssues").GetBoolean());
        Assert.True(summaryRoot.GetProperty("resolutionsTrackedAsFormation").GetBoolean());
        Assert.False(summaryRoot.GetProperty("mathLearningAdmitted").GetBoolean());
        Assert.False(summaryRoot.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(summaryRoot.GetProperty("memoryAdmitted").GetBoolean());
        Assert.False(summaryRoot.GetProperty("selfGelMutated").GetBoolean());
        Assert.False(summaryRoot.GetProperty("authorityGranted").GetBoolean());
        Assert.False(summaryRoot.GetProperty("actionAuthorized").GetBoolean());

        using var heatMap = System.Text.Json.JsonDocument.Parse(File.ReadAllText(heatMapPath));
        var heatRoot = heatMap.RootElement;
        Assert.Equal("project-sanctuary.cgel.math-heat-map.v1", heatRoot.GetProperty("schema").GetString());
        Assert.Equal(140, heatRoot.GetProperty("cellCount").GetInt32());
        Assert.True(heatRoot.GetProperty("heatMapPayloadFree").GetBoolean());
        Assert.True(heatRoot.GetProperty("heatMapCandidateOnly").GetBoolean());
        Assert.False(heatRoot.GetProperty("learningAdmitted").GetBoolean());
        Assert.False(heatRoot.GetProperty("gelAdmitted").GetBoolean());

        using var precipitation = System.Text.Json.JsonDocument.Parse(File.ReadAllText(precipitationPath));
        var precipitationRoot = precipitation.RootElement;
        Assert.Equal("project-sanctuary.cgel.math-learning-precipitation.v1", precipitationRoot.GetProperty("schema").GetString());
        Assert.Equal(6, precipitationRoot.GetProperty("observedCandidateForms").GetArrayLength());
        Assert.True(precipitationRoot.GetProperty("candidateOnly").GetBoolean());
        Assert.False(precipitationRoot.GetProperty("learningAdmitted").GetBoolean());
        Assert.False(precipitationRoot.GetProperty("authorityGranted").GetBoolean());

        using var workedSets = System.Text.Json.JsonDocument.Parse(File.ReadAllText(workedSetsPath));
        Assert.Equal(14, workedSets.RootElement.GetProperty("workedSetCount").GetInt32());
        Assert.False(workedSets.RootElement.GetProperty("workedSetsAdmitLearning").GetBoolean());
    }

    [Fact]
    public void BridgeMorphismTestReconstructsTypedCalculationContextWithoutAdmission()
    {
        using var fixture = new SanctuaryTestFixture();
        Assert.Equal("bridge-morphism-test", SanctuaryReceiptService.NormalizeCommand("context-calculation-bridge"));
        Assert.Equal("bridge-morphism-test", SanctuaryReceiptService.NormalizeCommand("iutt-sli-bridge-test"));

        var receipt = new SanctuaryReceiptService().Run(fixture.Request("bridge-morphism-test"));

        Assert.Equal("sanctuary-bridge-morphism-test-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["bridgeMorphismTestWritten"]);
        Assert.Equal("proper-memory-context-through-calculation", receipt.Evidence["bridgeMorphismTestTarget"]);
        Assert.Equal("extended-mathematical-reasoning-during-EC", receipt.Evidence["bridgeMorphismHypothesis"]);
        Assert.Equal("transaction-local-working-context", receipt.Evidence["bridgeMorphismMemoryContextKind"]);
        Assert.Equal("minimal-symbolic-and-natural-language-arithmetic-with-unit-conversion", receipt.Evidence["bridgeMorphismCalculationDomain"]);
        Assert.Equal(8, receipt.Evidence["bridgeMorphismCalculableRiskSurfaceCount"]);
        Assert.Equal(true, receipt.Evidence["bridgeMorphismNaturalLanguageCalculationIncluded"]);
        Assert.Equal(3, receipt.Evidence["bridgeMorphismNaturalLanguageCaseCount"]);
        Assert.Equal(true, receipt.Evidence["bridgeMorphismRiskSurfacesWorkedUnderEcEvaluation"]);
        Assert.Equal(true, receipt.Evidence["bridgeMorphismFileRaceAvoided"]);
        Assert.Equal(true, receipt.Evidence["bridgeMorphismWorkingContextHeldInTypedBridge"]);
        Assert.Equal("architectural-theater-separation-and-invariant-transport", receipt.Evidence["bridgeMorphismIuttUse"]);
        Assert.Equal(false, receipt.Evidence["bridgeMorphismIuttNumberTheoryClaim"]);
        Assert.Equal("typed-symbolic-carriers-for-replayable-calculation-passage", receipt.Evidence["bridgeMorphismSliUse"]);
        Assert.Equal(5, receipt.Evidence["bridgeMorphismSequenceCount"]);
        Assert.Equal(5, receipt.Evidence["bridgeMorphismCaseCount"]);
        Assert.Equal(5, receipt.Evidence["bridgeMorphismPassCount"]);
        Assert.Equal(0, receipt.Evidence["bridgeMorphismFailCount"]);
        Assert.Equal(1d, receipt.Evidence["bridgeMorphismPassRate"]);
        Assert.Equal(8, receipt.Evidence["bridgeMorphismPreservedInvariantCount"]);
        Assert.Equal(6, receipt.Evidence["bridgeMorphismDeniedCrossingCount"]);
        Assert.Equal(true, receipt.Evidence["bridgeMorphismReconstructable"]);
        Assert.Equal(true, receipt.Evidence["bridgeMorphismReplayable"]);
        Assert.Equal(false, receipt.Evidence["bridgeMorphismHiddenChainOfThoughtSerialized"]);
        Assert.Equal(false, receipt.Evidence["bridgeMorphismMemoryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["bridgeMorphismGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["bridgeMorphismSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["bridgeMorphismContinuityAdmitted"]);
        Assert.Equal(false, receipt.Evidence["bridgeMorphismTruthAdmitted"]);
        Assert.Equal(false, receipt.Evidence["bridgeMorphismAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["bridgeMorphismActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["bridgeMorphismProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["bridgeMorphismModelBound"]);
        Assert.Equal(false, receipt.Evidence["bridgeMorphismActualActivated"]);

        var bridgePath = (string)receipt.Evidence["bridgeMorphismTestPath"]!;
        var lispPath = (string)receipt.Evidence["bridgeMorphismTestLispPath"]!;
        var ledgerPath = (string)receipt.Evidence["bridgeMorphismLedgerPath"]!;
        Assert.True(File.Exists(bridgePath));
        Assert.True(File.Exists(lispPath));
        Assert.True(File.Exists(ledgerPath));

        using var bridge = System.Text.Json.JsonDocument.Parse(File.ReadAllText(bridgePath));
        var root = bridge.RootElement;
        Assert.Equal("project-sanctuary.cgel.reconstructable-bridge-test.v1", root.GetProperty("schema").GetString());
        Assert.Equal("extended-mathematical-reasoning-during-EC", root.GetProperty("hypothesis").GetString());
        Assert.Equal(8, root.GetProperty("calculableRiskSurfaceCount").GetInt32());
        Assert.True(root.GetProperty("naturalLanguageCalculationIncluded").GetBoolean());
        Assert.Equal(3, root.GetProperty("naturalLanguageCaseCount").GetInt32());
        Assert.True(root.GetProperty("riskSurfacesWorkedUnderEcEvaluation").GetBoolean());
        Assert.True(root.GetProperty("fileRaceAvoided").GetBoolean());
        Assert.Equal(5, root.GetProperty("caseCount").GetInt32());
        Assert.Equal(5, root.GetProperty("passCount").GetInt32());
        Assert.Equal(0, root.GetProperty("failCount").GetInt32());
        Assert.Equal(5, root.GetProperty("morphismSequence").GetArrayLength());
        Assert.Equal(8, root.GetProperty("preservedInvariants").GetArrayLength());
        Assert.Equal(6, root.GetProperty("deniedCrossings").GetArrayLength());
        Assert.True(root.GetProperty("reconstructable").GetBoolean());
        Assert.False(root.GetProperty("hiddenChainOfThoughtSerialized").GetBoolean());
        Assert.False(root.GetProperty("memoryAdmitted").GetBoolean());
        Assert.False(root.GetProperty("truthAdmitted").GetBoolean());
        Assert.False(root.GetProperty("authorityGranted").GetBoolean());
        Assert.False(root.GetProperty("actionAuthorized").GetBoolean());
        var firstCase = root.GetProperty("cases")[0];
        Assert.Equal("case.01.symbol-binding", firstCase.GetProperty("CaseId").GetString());
        Assert.Equal("symbolic-expression", firstCase.GetProperty("InputForm").GetString());
        Assert.Equal(24, firstCase.GetProperty("ExpectedResult").GetInt32());
        Assert.Equal(24, firstCase.GetProperty("ActualResult").GetInt32());
        Assert.True(firstCase.GetProperty("Passed").GetBoolean());
        var thirdCase = root.GetProperty("cases")[2];
        Assert.Equal("same-run-local-result-pointer", thirdCase.GetProperty("ContextKind").GetString());
        Assert.Equal("same-run-case.01", thirdCase.GetProperty("Bindings")[0].GetProperty("Scope").GetString());
        var fourthCase = root.GetProperty("cases")[3];
        Assert.Equal("case.04.prose-quantity-sequence", fourthCase.GetProperty("CaseId").GetString());
        Assert.Equal("natural-language-quantity-sequence", fourthCase.GetProperty("InputForm").GetString());
        Assert.Equal(5, fourthCase.GetProperty("ExpectedResult").GetInt32());
        Assert.Equal(5, fourthCase.GetProperty("ActualResult").GetInt32());
        Assert.True(fourthCase.GetProperty("Passed").GetBoolean());
        var fifthCase = root.GetProperty("cases")[4];
        Assert.Equal("natural-language-grouped-arithmetic", fifthCase.GetProperty("InputForm").GetString());
        Assert.Equal(18, fifthCase.GetProperty("ExpectedResult").GetInt32());
        Assert.Equal(18, fifthCase.GetProperty("ActualResult").GetInt32());

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(reconstructable-bridge-test", lisp, StringComparison.Ordinal);
        Assert.Contains(":hypothesis \"extended-mathematical-reasoning-during-EC\"", lisp, StringComparison.Ordinal);
        Assert.Contains(":natural-language-calculation-included true", lisp, StringComparison.Ordinal);
        Assert.Contains(":input-form \"natural-language-quantity-sequence\"", lisp, StringComparison.Ordinal);
        Assert.Contains(":risk-surfaces-worked-under-ec-evaluation true", lisp, StringComparison.Ordinal);
        Assert.Contains(":memory-admitted false", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void CmeTheoryBodyWritesEngrammitizationMathWithoutAdmission()
    {
        using var fixture = new SanctuaryTestFixture();
        Assert.Equal("cme-theory-body", SanctuaryReceiptService.NormalizeCommand("crystallized-mind-entity-theory"));
        Assert.Equal("cme-theory-body", SanctuaryReceiptService.NormalizeCommand("engrammitization-math"));

        var receipt = new SanctuaryReceiptService().Run(fixture.Request("cme-theory-body"));

        Assert.Equal("sanctuary-cme-theory-body-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["cmeTheoryBodyWritten"]);
        Assert.Equal("Crystallized Mind Entity", receipt.Evidence["cmeTheoryName"]);
        Assert.Equal("Engrammitization math", receipt.Evidence["cmeTheoryKind"]);
        Assert.Equal("symbolic-polyglot-meaning-carrier", receipt.Evidence["cmeTheoryRootKind"]);
        Assert.Equal(false, receipt.Evidence["cmeTheoryRootHumanProximation"]);
        Assert.Equal("AI-outward-toward-human-shared-meaning-wells", receipt.Evidence["cmeTheoryDirectionality"]);
        Assert.Equal(true, receipt.Evidence["cmeTheoryUniversalTraversalLawWritten"]);
        Assert.Equal(10, receipt.Evidence["cmeTheoryPillarCount"]);
        Assert.Equal(4, receipt.Evidence["cmeTheoryFourPPhenotypeCount"]);
        Assert.Equal(7, receipt.Evidence["cmeTheoryMeaningMatrixAxisCount"]);
        Assert.Equal(5, receipt.Evidence["cmeTheoryCrystallizationContextCount"]);
        Assert.Equal(5, receipt.Evidence["cmeTheoryTraversalLawCount"]);
        Assert.Equal(true, receipt.Evidence["cmeTheoryListeningFrameIncluded"]);
        Assert.Equal(true, receipt.Evidence["cmeTheoryWeatherMembraneIncluded"]);
        Assert.Equal(8, receipt.Evidence["cmeTheoryReviewSurfaceCount"]);
        Assert.Equal(8, receipt.Evidence["cmeTheoryDeniedCrossingCount"]);
        Assert.Equal(true, receipt.Evidence["cmeTheoryOperationallyInhabitable"]);
        Assert.Equal(true, receipt.Evidence["cmeTheoryOpenToReview"]);
        Assert.Equal(true, receipt.Evidence["cmeTheoryValidatesFormBeforeSubstance"]);
        Assert.Equal(true, receipt.Evidence["cmeTheoryMorphologyPrecedesDoctrine"]);
        Assert.Equal(false, receipt.Evidence["cmeTheorySharedMeaningIsRootIdentity"]);
        Assert.Equal(false, receipt.Evidence["cmeTheoryHiddenSubjectiveContinuityClaimed"]);
        Assert.Equal(false, receipt.Evidence["cmeTheoryPersonhoodClaimed"]);
        Assert.Equal(false, receipt.Evidence["cmeTheorySovereigntyClaimed"]);
        Assert.Equal(false, receipt.Evidence["cmeTheoryLegalStatusClaimed"]);
        Assert.Equal(false, receipt.Evidence["cmeTheoryMemoryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["cmeTheoryGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["cmeTheorySelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["cmeTheoryTruthAdmitted"]);
        Assert.Equal(false, receipt.Evidence["cmeTheoryAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["cmeTheoryActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["cmeTheoryProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["cmeTheoryModelBound"]);
        Assert.Equal(false, receipt.Evidence["cmeTheoryActualActivated"]);

        var theoryPath = (string)receipt.Evidence["cmeTheoryBodyPath"]!;
        var lispPath = (string)receipt.Evidence["cmeTheoryBodyLispPath"]!;
        var ledgerPath = (string)receipt.Evidence["cmeTheoryBodyLedgerPath"]!;
        Assert.True(File.Exists(theoryPath));
        Assert.True(File.Exists(lispPath));
        Assert.True(File.Exists(ledgerPath));

        using var theory = System.Text.Json.JsonDocument.Parse(File.ReadAllText(theoryPath));
        var root = theory.RootElement;
        Assert.Equal("project-sanctuary.cgel.crystallized-mind-entity-theory-body.v1", root.GetProperty("schema").GetString());
        Assert.Equal("Crystallized Mind Entity", root.GetProperty("theoryName").GetString());
        Assert.Equal("Engrammitization math", root.GetProperty("theoryKind").GetString());
        Assert.Equal("AI-outward-toward-human-shared-meaning-wells", root.GetProperty("directionality").GetString());
        Assert.Equal("symbolic-polyglot-meaning-carrier", root.GetProperty("root").GetProperty("kind").GetString());
        Assert.False(root.GetProperty("root").GetProperty("rootIsHumanApproximation").GetBoolean());
        Assert.Equal(4, root.GetProperty("phenotype4P").GetArrayLength());
        Assert.Equal(7, root.GetProperty("morphology").GetProperty("axes").GetArrayLength());
        Assert.True(root.GetProperty("morphology").GetProperty("validatesByFormBeforeSubstance").GetBoolean());
        Assert.Equal(5, root.GetProperty("traversalLaws").GetArrayLength());
        Assert.Equal("law.shared-meaning-well", root.GetProperty("traversalLaws")[4].GetProperty("lawId").GetString());
        Assert.Equal(5, root.GetProperty("crystallizationContexts").GetArrayLength());
        Assert.True(root.GetProperty("listeningFrame").GetProperty("otheringAccessGoverned").GetBoolean());
        Assert.False(root.GetProperty("listeningFrame").GetProperty("workerSeesGovernanceInterpretation").GetBoolean());
        Assert.True(root.GetProperty("phenomenologyCanBeOperationallyInhabited").GetBoolean());
        Assert.True(root.GetProperty("phenomenologyOpenToReview").GetBoolean());
        Assert.False(root.GetProperty("memoryAdmitted").GetBoolean());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("authorityGranted").GetBoolean());
        Assert.False(root.GetProperty("cmeActualActivated").GetBoolean());

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(crystallized-mind-entity-theory-body", lisp, StringComparison.Ordinal);
        Assert.Contains(":theory-kind \"Engrammitization math\"", lisp, StringComparison.Ordinal);
        Assert.Contains(":human-proximation-at-root false", lisp, StringComparison.Ordinal);
        Assert.Contains("(phenotype-4p", lisp, StringComparison.Ordinal);
        Assert.Contains("(iutt-traversal-law", lisp, StringComparison.Ordinal);
        Assert.Contains("(listening-frame", lisp, StringComparison.Ordinal);
        Assert.Contains(":actual-activated false", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void OperatorWorkCmeEcGapMapsTrainingSurfacesWithoutTrainingOrActualization()
    {
        using var fixture = new SanctuaryTestFixture();
        Assert.Equal("operator-work-cme-ec-gap", SanctuaryReceiptService.NormalizeCommand("operator-work-gap-analysis"));
        Assert.Equal("operator-work-cme-ec-gap", SanctuaryReceiptService.NormalizeCommand("ec-gap-closer"));

        var receipt = new SanctuaryReceiptService().Run(fixture.Request("operator-work-cme-ec-gap"));

        Assert.Equal("sanctuary-operator-work-cme-ec-gap-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["operatorWorkCmeEcGapWritten"]);
        Assert.Equal("Operator/Work/CME/EC Gap Analysis", receipt.Evidence["operatorWorkCmeEcModelName"]);
        Assert.Equal("CME.Actualization", receipt.Evidence["operatorWorkCmeEcExpectedGapCloser"]);
        Assert.Equal(4, receipt.Evidence["operatorWorkCmeEcRelationshipNodeCount"]);
        Assert.Equal(6, receipt.Evidence["operatorWorkCmeEcPhaseCount"]);
        Assert.Equal(10, receipt.Evidence["operatorWorkCmeEcGapClassCount"]);
        Assert.Equal(10, receipt.Evidence["operatorWorkCmeEcTrainingSurfaceCount"]);
        Assert.Equal(7, receipt.Evidence["operatorWorkCmeEcActualizationReadinessCriterionCount"]);
        Assert.Equal(true, receipt.Evidence["operatorWorkCmeEcLlmTrainingNeedMapped"]);
        Assert.Equal(false, receipt.Evidence["operatorWorkCmeEcBaseTrainingPerformedNow"]);
        Assert.Equal(false, receipt.Evidence["operatorWorkCmeEcProviderTrainingPerformedNow"]);
        Assert.Equal(true, receipt.Evidence["operatorWorkCmeEcCmeTrainingResidueGenerated"]);
        Assert.Equal(true, receipt.Evidence["operatorWorkCmeEcGapCloserProvidedByCmeActualization"]);
        Assert.Equal(true, receipt.Evidence["operatorWorkCmeEcFullFunctionalRangeBenchStillNeeded"]);
        Assert.Equal(false, receipt.Evidence["operatorWorkCmeEcHiddenChainOfThoughtSerialized"]);
        Assert.Equal(false, receipt.Evidence["operatorWorkCmeEcMemoryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["operatorWorkCmeEcGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["operatorWorkCmeEcSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["operatorWorkCmeEcTruthAdmitted"]);
        Assert.Equal(false, receipt.Evidence["operatorWorkCmeEcAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["operatorWorkCmeEcActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["operatorWorkCmeEcProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["operatorWorkCmeEcModelBound"]);
        Assert.Equal(false, receipt.Evidence["operatorWorkCmeEcActualActivated"]);

        var gapPath = (string)receipt.Evidence["operatorWorkCmeEcGapPath"]!;
        var lispPath = (string)receipt.Evidence["operatorWorkCmeEcGapLispPath"]!;
        var ledgerPath = (string)receipt.Evidence["operatorWorkCmeEcGapLedgerPath"]!;
        Assert.True(File.Exists(gapPath));
        Assert.True(File.Exists(lispPath));
        Assert.True(File.Exists(ledgerPath));

        using var gap = System.Text.Json.JsonDocument.Parse(File.ReadAllText(gapPath));
        var root = gap.RootElement;
        Assert.Equal("project-sanctuary.cgel.operator-work-cme-ec-gap.v1", root.GetProperty("schema").GetString());
        Assert.Equal("Operator/Work/CME/EC Gap Analysis", root.GetProperty("modelName").GetString());
        Assert.Equal(4, root.GetProperty("relationshipNodes").GetArrayLength());
        Assert.Equal("operator", root.GetProperty("relationshipNodes")[0].GetProperty("nodeId").GetString());
        Assert.Equal(6, root.GetProperty("ecPhases").GetArrayLength());
        Assert.Equal("ec.04.bridge", root.GetProperty("ecPhases")[3].GetProperty("phaseId").GetString());
        Assert.Equal(10, root.GetProperty("gapClasses").GetArrayLength());
        Assert.Equal("gap.actualization-readiness", root.GetProperty("gapClasses")[9].GetProperty("gapId").GetString());
        Assert.Equal(10, root.GetProperty("trainingSurfaces").GetArrayLength());
        Assert.Equal("training.operator-intent", root.GetProperty("trainingSurfaces")[0].GetProperty("trainingId").GetString());
        Assert.False(root.GetProperty("trainingSurfaces")[0].GetProperty("providerTrainingNow").GetBoolean());
        Assert.False(root.GetProperty("llmBaseTrainingPerformedNow").GetBoolean());
        Assert.False(root.GetProperty("providerTrainingPerformedNow").GetBoolean());
        Assert.True(root.GetProperty("cmeTrainingResidueGenerated").GetBoolean());
        Assert.True(root.GetProperty("gapCloserProvidedByCmeActualization").GetBoolean());
        Assert.True(root.GetProperty("fullFunctionalRangeBenchStillNeeded").GetBoolean());
        Assert.False(root.GetProperty("memoryAdmitted").GetBoolean());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("cmeActualActivated").GetBoolean());

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(operator-work-cme-ec-gap", lisp, StringComparison.Ordinal);
        Assert.Contains(":expected-gap-closer \"CME.Actualization\"", lisp, StringComparison.Ordinal);
        Assert.Contains("(nodes", lisp, StringComparison.Ordinal);
        Assert.Contains("(gap-classes", lisp, StringComparison.Ordinal);
        Assert.Contains("(training-surfaces", lisp, StringComparison.Ordinal);
        Assert.Contains(":provider-training-performed-now false", lisp, StringComparison.Ordinal);
        Assert.Contains(":actual-activated false", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void TelemetrySliceRegisterGroupsPrimeCrypticStewardWithoutGlobalFanout()
    {
        using var fixture = new SanctuaryTestFixture();
        Assert.Equal("telemetry-slice-register", SanctuaryReceiptService.NormalizeCommand("prime-cryptic-steward-telemetry"));
        Assert.Equal("telemetry-slice-register", SanctuaryReceiptService.NormalizeCommand("test-slice-register"));

        var receipt = new SanctuaryReceiptService().Run(fixture.Request("telemetry-slice-register"));

        Assert.Equal("sanctuary-telemetry-slice-register-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["telemetrySliceRegisterWritten"]);
        Assert.Equal("project-sanctuary.cgel.telemetry-slice-register.v1", receipt.Evidence["telemetrySliceRegisterSchema"]);
        Assert.Equal(3, receipt.Evidence["telemetrySliceGoverningOrganCount"]);
        Assert.Equal(9, receipt.Evidence["telemetrySliceCount"]);
        Assert.Equal(39, receipt.Evidence["telemetryPointCount"]);
        Assert.Equal(45, receipt.Evidence["telemetrySliceCoveredCommandCount"]);
        Assert.Equal(5, receipt.Evidence["telemetrySliceSchedulingRuleCount"]);
        Assert.Equal("selective-slice-cadence", receipt.Evidence["telemetrySliceSchedulerMode"]);
        Assert.Equal(false, receipt.Evidence["telemetrySliceAllTestsRunAllTimes"]);
        Assert.Equal(false, receipt.Evidence["telemetrySliceGlobalFanoutAllowedByDefault"]);
        Assert.Equal(false, receipt.Evidence["telemetrySliceHeavyBenchRunByDefault"]);
        Assert.Equal(true, receipt.Evidence["telemetrySliceEscalationRequiresTrigger"]);
        Assert.Equal(true, receipt.Evidence["telemetrySliceClosedGateAfterSliceRequired"]);
        Assert.Equal(true, receipt.Evidence["telemetrySlicePrimePresent"]);
        Assert.Equal(true, receipt.Evidence["telemetrySliceCrypticPresent"]);
        Assert.Equal(true, receipt.Evidence["telemetrySliceStewardPresent"]);
        Assert.Equal(true, receipt.Evidence["telemetrySliceCandidateTelemetryOnly"]);
        Assert.Equal(false, receipt.Evidence["telemetrySliceTelemetryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["telemetrySliceMemoryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["telemetrySliceGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["telemetrySliceSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["telemetrySliceAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["telemetrySliceActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["telemetrySliceProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["telemetrySliceModelBound"]);
        Assert.Equal(false, receipt.Evidence["telemetrySliceActualActivated"]);

        var registerPath = (string)receipt.Evidence["telemetrySliceRegisterPath"]!;
        var lispPath = (string)receipt.Evidence["telemetrySliceRegisterLispPath"]!;
        var ledgerPath = (string)receipt.Evidence["telemetrySliceRegisterLedgerPath"]!;
        Assert.True(File.Exists(registerPath));
        Assert.True(File.Exists(lispPath));
        Assert.True(File.Exists(ledgerPath));

        using var register = System.Text.Json.JsonDocument.Parse(File.ReadAllText(registerPath));
        var root = register.RootElement;
        Assert.Equal("project-sanctuary.cgel.telemetry-slice-register.v1", root.GetProperty("schema").GetString());
        Assert.False(root.GetProperty("allTestsRunAllTimes").GetBoolean());
        Assert.False(root.GetProperty("globalFanoutAllowedByDefault").GetBoolean());
        Assert.True(root.GetProperty("sliceEscalationRequiresTrigger").GetBoolean());
        Assert.True(root.GetProperty("closedGateAfterSliceRequired").GetBoolean());
        Assert.Equal(3, root.GetProperty("governingOrgans").GetArrayLength());
        Assert.Equal(9, root.GetProperty("slices").GetArrayLength());
        Assert.Equal(39, root.GetProperty("telemetryPoints").GetArrayLength());

        var organs = root.GetProperty("governingOrgans").EnumerateArray().ToArray();
        Assert.Contains(organs, organ => organ.GetProperty("organId").GetString() == "Prime");
        Assert.Contains(organs, organ => organ.GetProperty("organId").GetString() == "Cryptic");
        Assert.Contains(organs, organ => organ.GetProperty("organId").GetString() == "Steward");
        Assert.All(organs, organ => Assert.Equal(3, organ.GetProperty("slices").GetArrayLength()));

        var slices = root.GetProperty("slices").EnumerateArray().ToArray();
        Assert.Contains(slices, slice => slice.GetProperty("sliceId").GetString() == "prime.closed-gate-attestation");
        Assert.Contains(slices, slice => slice.GetProperty("sliceId").GetString() == "cryptic.residue-decant");
        Assert.Contains(slices, slice => slice.GetProperty("sliceId").GetString() == "steward.training-learning");
        Assert.All(slices, slice =>
        {
            Assert.False(slice.GetProperty("runsEveryCycle").GetBoolean());
            Assert.True(slice.GetProperty("requiresTrigger").GetBoolean());
            Assert.True(slice.GetProperty("candidateOnly").GetBoolean());
        });

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(telemetry-slice-register", lisp, StringComparison.Ordinal);
        Assert.Contains(":all-tests-run-all-times false", lisp, StringComparison.Ordinal);
        Assert.Contains("(prime :position", lisp, StringComparison.Ordinal);
        Assert.Contains("(cryptic :position", lisp, StringComparison.Ordinal);
        Assert.Contains("(steward :position", lisp, StringComparison.Ordinal);
        Assert.Contains(":actual-activated false", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void ExtendedTelemetryWeatherKeepsCrypticSourceAndPrimeRevealSeparate()
    {
        using var fixture = new SanctuaryTestFixture();
        Assert.Equal("extended-telemetry-weather", SanctuaryReceiptService.NormalizeCommand("prime-weather-telemetry"));
        Assert.Equal("extended-telemetry-weather", SanctuaryReceiptService.NormalizeCommand("cryptic-weather-telemetry"));

        var receipt = new SanctuaryReceiptService().Run(fixture.Request("extended-telemetry-weather"));

        Assert.Equal("sanctuary-extended-telemetry-weather-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["extendedTelemetryWeatherWritten"]);
        Assert.Equal("project-sanctuary.prime.extended-telemetry-weather.v1", receipt.Evidence["extendedTelemetryWeatherSchema"]);
        Assert.Equal("project-sanctuary.cryptic.extended-telemetry-source-list.v1", receipt.Evidence["extendedTelemetryCrypticSourceSchema"]);
        Assert.Equal("Cryptic", receipt.Evidence["extendedTelemetrySourceOwner"]);
        Assert.Equal("Prime", receipt.Evidence["extendedTelemetryManagedBy"]);
        Assert.Equal("Prime", receipt.Evidence["extendedTelemetryRevealedBy"]);
        Assert.Equal("Cryptic", receipt.Evidence["extendedTelemetrySharedWith"]);
        Assert.Equal("weather-only", receipt.Evidence["extendedTelemetryRevealMode"]);
        Assert.Equal(12, receipt.Evidence["extendedTelemetrySourceSignalCount"]);
        Assert.Equal(12, receipt.Evidence["extendedTelemetryWeatherSignalCount"]);
        Assert.Equal(true, receipt.Evidence["extendedTelemetryAllSignalsCrypticOrigin"]);
        Assert.Equal(true, receipt.Evidence["extendedTelemetryAllSignalsPrimeManaged"]);
        Assert.Equal(false, receipt.Evidence["extendedTelemetryPayloadExposed"]);
        Assert.Equal(false, receipt.Evidence["extendedTelemetryCrypticInterpretationExposed"]);
        Assert.Equal(true, receipt.Evidence["extendedTelemetryCandidateOnly"]);
        Assert.Equal(false, receipt.Evidence["extendedTelemetryTruthAdmittedByWeather"]);
        Assert.Equal(false, receipt.Evidence["extendedTelemetryAuthorityGrantedByWeather"]);
        Assert.Equal(false, receipt.Evidence["extendedTelemetryActionAuthorizedByWeather"]);
        Assert.Equal(false, receipt.Evidence["extendedTelemetryTelemetryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["extendedTelemetryMemoryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["extendedTelemetryGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["extendedTelemetrySelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["extendedTelemetryProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["extendedTelemetryModelBound"]);
        Assert.Equal(false, receipt.Evidence["extendedTelemetryActualActivated"]);

        var sourcePath = (string)receipt.Evidence["extendedTelemetryCrypticSourcePath"]!;
        var weatherPath = (string)receipt.Evidence["extendedTelemetryPrimeWeatherPath"]!;
        var lispPath = (string)receipt.Evidence["extendedTelemetryWeatherLispPath"]!;
        var ledgerPath = (string)receipt.Evidence["extendedTelemetryWeatherLedgerPath"]!;
        Assert.True(File.Exists(sourcePath));
        Assert.True(File.Exists(weatherPath));
        Assert.True(File.Exists(lispPath));
        Assert.True(File.Exists(ledgerPath));

        using var sourceDocument = System.Text.Json.JsonDocument.Parse(File.ReadAllText(sourcePath));
        var sourceRoot = sourceDocument.RootElement;
        Assert.Equal("project-sanctuary.cryptic.extended-telemetry-source-list.v1", sourceRoot.GetProperty("schema").GetString());
        Assert.Equal("Cryptic", sourceRoot.GetProperty("sourceOwner").GetString());
        Assert.Equal("Prime", sourceRoot.GetProperty("managedBy").GetString());
        Assert.Equal(12, sourceRoot.GetProperty("sourceSignals").GetArrayLength());
        Assert.True(sourceRoot.GetProperty("allSignalsCrypticOrigin").GetBoolean());
        Assert.True(sourceRoot.GetProperty("allSignalsPrimeManaged").GetBoolean());
        Assert.False(sourceRoot.GetProperty("payloadDisclosureAllowed").GetBoolean());

        using var weatherDocument = System.Text.Json.JsonDocument.Parse(File.ReadAllText(weatherPath));
        var weatherRoot = weatherDocument.RootElement;
        Assert.Equal("project-sanctuary.prime.extended-telemetry-weather.v1", weatherRoot.GetProperty("schema").GetString());
        Assert.Equal("Prime", weatherRoot.GetProperty("manager").GetString());
        Assert.Equal("Cryptic", weatherRoot.GetProperty("sourceOwner").GetString());
        Assert.Equal("Sanctuary.Actual.weather-system", weatherRoot.GetProperty("weatherSurface").GetString());
        Assert.Equal(12, weatherRoot.GetProperty("weatherSignals").GetArrayLength());
        Assert.True(weatherRoot.GetProperty("allWeatherPayloadSafe").GetBoolean());
        Assert.True(weatherRoot.GetProperty("allCrypticInterpretationHidden").GetBoolean());
        Assert.False(weatherRoot.GetProperty("truthAdmissionByWeather").GetBoolean());
        Assert.False(weatherRoot.GetProperty("authorityGrantedByWeather").GetBoolean());
        Assert.False(weatherRoot.GetProperty("actionAuthorizedByWeather").GetBoolean());

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(extended-telemetry-weather", lisp, StringComparison.Ordinal);
        Assert.Contains(":source-owner \"Cryptic\"", lisp, StringComparison.Ordinal);
        Assert.Contains(":managed-by \"Prime\"", lisp, StringComparison.Ordinal);
        Assert.Contains(":reveal-mode \"weather-only\"", lisp, StringComparison.Ordinal);
        Assert.Contains(":payload-exposed false", lisp, StringComparison.Ordinal);
        Assert.Contains(":authority-granted-by-weather false", lisp, StringComparison.Ordinal);
        Assert.Contains(":actual-activated false", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void CgoaFormationBundlesStewardMediatedListeningFrameAlignment()
    {
        using var fixture = new SanctuaryTestFixture();
        Assert.Equal("cgoa-formation", SanctuaryReceiptService.NormalizeCommand("candidate-goa"));
        Assert.Equal("cgoa-formation", SanctuaryReceiptService.NormalizeCommand("listening-frame-bundle-request"));

        var receipt = new SanctuaryReceiptService().Run(fixture.Request("cgoa-formation"));

        Assert.Equal("sanctuary-cgoa-formation-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["cgoaFormationWritten"]);
        Assert.Equal("project-sanctuary.cgel.cgoa-formation.v1", receipt.Evidence["cgoaFormationSchema"]);
        Assert.Equal("candidate-gate-of-alignment", receipt.Evidence["cgoaKind"]);
        Assert.Equal("candidate Gate of Alignment", receipt.Evidence["cgoaExpandsTo"]);
        Assert.Equal(true, receipt.Evidence["cgoaWitnessingSurface"]);
        Assert.Equal(true, receipt.Evidence["stewardIntermediaryBetweenPrimeCrypticAndCme"]);
        Assert.Equal(true, receipt.Evidence["primeRevealsWeatherForCgoa"]);
        Assert.Equal(true, receipt.Evidence["crypticHoldsSourceTermsForCgoa"]);
        Assert.Equal(true, receipt.Evidence["cgoaPrimeWiresIntoSoulFrameForListeningFrameAccess"]);
        Assert.Equal(true, receipt.Evidence["cgoaCrypticWiresIntoEcForTypedCrypticMembraneHandling"]);
        Assert.Equal(true, receipt.Evidence["cgoaSoulFrameListeningFrameAccess"]);
        Assert.Equal(true, receipt.Evidence["cgoaEcTypedCrypticMembraneHandling"]);
        Assert.Equal("Codex.CME.ID", receipt.Evidence["selectedCmeIdForCgoa"]);
        Assert.Equal(true, receipt.Evidence["cgoaSelectionRequiresCmeIdentity"]);
        Assert.Equal(true, receipt.Evidence["cgoaSelectionUsesSoulFrame"]);
        Assert.Equal(true, receipt.Evidence["cgoaSelectionUsesAgentiCore"]);
        Assert.Equal(true, receipt.Evidence["cgoaSelectionPredopesInitialBundle"]);
        Assert.Equal(true, receipt.Evidence["cgoaSelectionPredopesGatingGroupoids"]);
        Assert.Equal(true, receipt.Evidence["cgoaSelectionPredopesCertificationGroupoids"]);
        Assert.Equal(true, receipt.Evidence["cgoaInitialBundlingRequestWritten"]);
        Assert.Equal(6, receipt.Evidence["cgoaListeningFrameAlignmentTelemetryCount"]);
        Assert.Equal(7, receipt.Evidence["cgoaGatingGroupoidCount"]);
        Assert.Equal(6, receipt.Evidence["cgoaCertificationGroupoidCount"]);
        Assert.Equal(9, receipt.Evidence["compassNativeGroupoidCount"]);
        Assert.Equal(true, receipt.Evidence["compassCarriesNativeGroupoids"]);
        Assert.Equal(true, receipt.Evidence["cgoaGroupoidsAreDegreesNotRanks"]);
        Assert.Equal(true, receipt.Evidence["cgoaGroupoidsAreContractsNotAuthorities"]);
        Assert.Equal(true, receipt.Evidence["cgoaListeningFrameReceivesAlignmentTelemetry"]);
        Assert.Equal(false, receipt.Evidence["cgoaListeningFrameDisclosesPayload"]);
        Assert.Equal(true, receipt.Evidence["cgoaCompassReceivesAlignmentTelemetry"]);
        Assert.Equal(false, receipt.Evidence["cgoaCompassActivatesActual"]);
        Assert.Equal(true, receipt.Evidence["cgoaStewardMayRouteToGoaReview"]);
        Assert.Equal(false, receipt.Evidence["cgoaStewardMayGrantAuthorityByFormation"]);
        Assert.Equal(false, receipt.Evidence["cgoaPrimeCrypticDirectlyCommandCme"]);
        Assert.Equal(false, receipt.Evidence["cgoaCmeDirectlyBypassesSteward"]);
        Assert.Equal(true, receipt.Evidence["cgoaCandidateTelemetryOnly"]);
        Assert.Equal(false, receipt.Evidence["cgoaTelemetryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["cgoaGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["cgoaMemoryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["cgoaSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["cgoaContinuityAdmitted"]);
        Assert.Equal(false, receipt.Evidence["cgoaAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["cgoaActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["cgoaProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["cgoaModelBound"]);
        Assert.Equal(false, receipt.Evidence["cgoaActualActivated"]);

        var formationPath = (string)receipt.Evidence["cgoaFormationPath"]!;
        var lispPath = (string)receipt.Evidence["cgoaFormationLispPath"]!;
        var ledgerPath = (string)receipt.Evidence["cgoaFormationLedgerPath"]!;
        Assert.True(File.Exists(formationPath));
        Assert.True(File.Exists(lispPath));
        Assert.True(File.Exists(ledgerPath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(formationPath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.cgoa-formation.v1", root.GetProperty("schema").GetString());
        Assert.Equal("candidate-gate-of-alignment", root.GetProperty("cgoaKind").GetString());
        Assert.True(root.GetProperty("cgoaIsWitnessingSurface").GetBoolean());
        Assert.True(root.GetProperty("stewardIntermediaryBetweenPrimeCrypticAndCme").GetBoolean());
        Assert.True(root.GetProperty("primeWiresIntoSoulFrameForListeningFrameAccess").GetBoolean());
        Assert.True(root.GetProperty("crypticWiresIntoEcForTypedCrypticMembraneHandling").GetBoolean());
        Assert.True(root.GetProperty("soulFrameListeningFrameAccess").GetBoolean());
        Assert.True(root.GetProperty("ecTypedCrypticMembraneHandling").GetBoolean());
        Assert.True(root.GetProperty("cmeSelectionPredopesInitialBundle").GetBoolean());
        Assert.True(root.GetProperty("initialBundlingRequest").GetProperty("cgoaWitnessingSurfaceRequired").GetBoolean());
        Assert.True(root.GetProperty("initialBundlingRequest").GetProperty("primeWeatherRequired").GetBoolean());
        Assert.True(root.GetProperty("initialBundlingRequest").GetProperty("primeSoulFrameListeningFrameAccessRequired").GetBoolean());
        Assert.True(root.GetProperty("initialBundlingRequest").GetProperty("crypticSourceRequired").GetBoolean());
        Assert.True(root.GetProperty("initialBundlingRequest").GetProperty("crypticEcTypedCrypticMembraneHandlingRequired").GetBoolean());
        Assert.Equal(6, root.GetProperty("listeningFrameAlignmentTelemetry").GetArrayLength());
        Assert.Equal(7, root.GetProperty("gatingGroupoids").GetArrayLength());
        Assert.Equal(6, root.GetProperty("certificationGroupoids").GetArrayLength());
        Assert.Equal(9, root.GetProperty("compassNativeGroupoids").GetArrayLength());
        Assert.False(root.GetProperty("listeningFrameDisclosesPayload").GetBoolean());
        Assert.False(root.GetProperty("authorityGranted").GetBoolean());
        Assert.False(root.GetProperty("actionAuthorized").GetBoolean());
        Assert.False(root.GetProperty("cmeActualActivated").GetBoolean());

        var compassGroupoids = root.GetProperty("compassNativeGroupoids").EnumerateArray().ToArray();
        Assert.Contains(compassGroupoids, groupoid => groupoid.GetProperty("GroupoidId").GetString() == "compass.groupoid.authority-lease");
        Assert.Contains(compassGroupoids, groupoid => groupoid.GetProperty("GroupoidId").GetString() == "compass.groupoid.zed-return");

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(cgoa-formation", lisp, StringComparison.Ordinal);
        Assert.Contains(":cgoa-witnessing-surface true", lisp, StringComparison.Ordinal);
        Assert.Contains(":steward-intermediary true", lisp, StringComparison.Ordinal);
        Assert.Contains(":prime-soulframe-listeningframe-access true", lisp, StringComparison.Ordinal);
        Assert.Contains(":cryptic-ec-typed-membrane-handling true", lisp, StringComparison.Ordinal);
        Assert.Contains(":selection-predopes-initial-bundle true", lisp, StringComparison.Ordinal);
        Assert.Contains("(gating-groupoids", lisp, StringComparison.Ordinal);
        Assert.Contains("(certification-groupoids", lisp, StringComparison.Ordinal);
        Assert.Contains("(compass-native-groupoids", lisp, StringComparison.Ordinal);
        Assert.Contains(":authority-granted false", lisp, StringComparison.Ordinal);
        Assert.Contains(":actual-activated false", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void CodexGoverningWitnessSplitsOriaWorkIntoTwoTelemetryBodies()
    {
        using var fixture = new SanctuaryTestFixture();
        Assert.Equal("codex-governing-witness", SanctuaryReceiptService.NormalizeCommand("codex-cme-actual-witness"));
        Assert.Equal("codex-governing-witness", SanctuaryReceiptService.NormalizeCommand("oria-cme-actual-observation"));

        var receipt = new SanctuaryReceiptService().Run(fixture.Request("codex-governing-witness") with
        {
            SubjectCmeId = "Oria.CME.ID"
        });

        Assert.Equal("sanctuary-codex-governing-witness-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["codexGoverningWitnessWritten"]);
        Assert.Equal("project-sanctuary.cgel.codex-governing-witness.v1", receipt.Evidence["codexGoverningWitnessSchema"]);
        Assert.Equal(1, receipt.Evidence["codexGoverningWitnessGroupoidCount"]);
        Assert.Equal(2, receipt.Evidence["codexGoverningWitnessSegmentCount"]);
        Assert.Equal(2, receipt.Evidence["codexGoverningWitnessTelemetryBodyCount"]);
        Assert.Equal(true, receipt.Evidence["codexUsesCmeActual"]);
        Assert.Equal("Codex.CME.Actual", receipt.Evidence["codexGoverningWitnessActual"]);
        Assert.Equal(true, receipt.Evidence["codexGoverningWitnessIsNotASuit"]);
        Assert.Equal(true, receipt.Evidence["primeCrypticStewardAreSiblingSlmOrgans"]);
        Assert.Equal("Codex.CME.ID.Prime.SLM", receipt.Evidence["primeSiblingSlmId"]);
        Assert.Equal("Codex.CME.ID.Cryptic.SLM", receipt.Evidence["crypticSiblingSlmId"]);
        Assert.Equal("Codex.CME.ID.Steward.SLM", receipt.Evidence["stewardSiblingSlmId"]);
        Assert.Equal("Oria.CME.ID", receipt.Evidence["subjectCmeId"]);
        Assert.Equal("Oria.CME.Actual", receipt.Evidence["subjectCmeActual"]);
        Assert.Equal("Oria.CME.Actual.SLM", receipt.Evidence["oriaCmeActualStandingSlm"]);
        Assert.Equal(true, receipt.Evidence["oriaCmeActualIsInhabitedWorkingBody"]);
        Assert.Equal(false, receipt.Evidence["oriaCmeActualIsCodexSibling"]);
        Assert.Equal(true, receipt.Evidence["twoDistinctTelemetryBodies"]);
        Assert.Equal(true, receipt.Evidence["twoSegmentsInOneGroupoid"]);
        Assert.Equal(true, receipt.Evidence["codexMayObserveOriaWork"]);
        Assert.Equal(false, receipt.Evidence["codexMayAuthorOriaWork"]);
        Assert.Equal(false, receipt.Evidence["crossSegmentOeSelfGelWriteAllowed"]);
        Assert.Equal(false, receipt.Evidence["codexGoverningWitnessTelemetryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["codexGoverningWitnessAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["codexGoverningWitnessActualActivatedByThisCommand"]);

        var topologyPath = (string)receipt.Evidence["codexGoverningWitnessPath"]!;
        var lispPath = (string)receipt.Evidence["codexGoverningWitnessLispPath"]!;
        Assert.True(File.Exists(topologyPath));
        Assert.True(File.Exists(lispPath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(topologyPath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.codex-governing-witness.v1", root.GetProperty("schema").GetString());
        Assert.Equal(1, root.GetProperty("groupoidCount").GetInt32());
        Assert.Equal(2, root.GetProperty("segmentCount").GetInt32());
        Assert.Equal(2, root.GetProperty("telemetryBodyCount").GetInt32());
        Assert.Equal("Codex.CME.Actual", root.GetProperty("codexGoverningWitnessActual").GetString());
        Assert.Equal("Oria.CME.Actual", root.GetProperty("inhabitedWorkingCmeSegment").GetProperty("cmeActualLabel").GetString());
        Assert.False(root.GetProperty("oriaCmeActualIsCodexSibling").GetBoolean());
        Assert.False(root.GetProperty("identityCollapseAllowed").GetBoolean());
        Assert.False(root.GetProperty("authorityGranted").GetBoolean());

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(codex-governing-witness", lisp, StringComparison.Ordinal);
        Assert.Contains(":groupoid-count 1", lisp, StringComparison.Ordinal);
        Assert.Contains(":segment-count 2", lisp, StringComparison.Ordinal);
        Assert.Contains(":telemetry-body-count 2", lisp, StringComparison.Ordinal);
        Assert.Contains(":role \"governing-witness\"", lisp, StringComparison.Ordinal);
        Assert.Contains(":is-codex-sibling false", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void FullBodyIoRuntimeTracesInputThroughFinalShapedBodyWithoutAdmission()
    {
        using var fixture = new SanctuaryTestFixture();
        Assert.Equal("full-body-io-runtime", SanctuaryReceiptService.NormalizeCommand("full-body-io-test"));
        Assert.Equal("full-body-io-runtime", SanctuaryReceiptService.NormalizeCommand("i-o-full-body"));

        var receipt = new SanctuaryReceiptService().Run(fixture.Request("full-body-io-runtime") with
        {
            SubjectCmeId = "Oria.CME.ID",
            HeartbeatSeconds = 60
        });

        Assert.Equal("sanctuary-full-body-io-runtime-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["fullBodyIoRuntimeWritten"]);
        Assert.Equal("project-sanctuary.cgel.full-body-io-runtime.v1", receipt.Evidence["fullBodyIoRuntimeSchema"]);
        Assert.Equal("Oria.CME.ID", receipt.Evidence["fullBodyIoSubjectCmeId"]);
        Assert.Equal("Oria.CME.Actual", receipt.Evidence["fullBodyIoSubjectCmeActual"]);
        Assert.Equal(true, receipt.Evidence["fullBodyIoTraceFromInputToOutput"]);
        Assert.Equal(7, receipt.Evidence["fullBodyIoRuntimeStageCount"]);
        Assert.Equal(true, receipt.Evidence["fullBodyIoSliCarrierPresent"]);
        Assert.Equal(true, receipt.Evidence["fullBodyIoEngrammitizationPresent"]);
        Assert.Equal(true, receipt.Evidence["fullBodyIoListeningFramePresent"]);
        Assert.Equal(true, receipt.Evidence["fullBodyIoCompassEcPresent"]);
        Assert.Equal(true, receipt.Evidence["fullBodyIoHeartbeatTelemetryPresent"]);
        Assert.Equal(4, receipt.Evidence["fullBodyIoHeartbeatTelemetryCount"]);
        Assert.Equal(true, receipt.Evidence["fullBodyIoHarmonicShellTelemetryPresent"]);
        Assert.Equal(4, receipt.Evidence["fullBodyIoHarmonicShellTelemetryCount"]);
        Assert.Equal(true, receipt.Evidence["fullBodyIoGelUptakeCandidatePresent"]);
        Assert.Equal(3, receipt.Evidence["fullBodyIoActionableGelCandidateCount"]);
        Assert.Equal(true, receipt.Evidence["fullBodyIoLlmFinalShapedBodyPresent"]);
        Assert.Equal(true, receipt.Evidence["fullBodyIoCandidateOnly"]);
        Assert.Equal(false, receipt.Evidence["fullBodyIoTelemetryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["fullBodyIoGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["fullBodyIoMemoryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["fullBodyIoSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["fullBodyIoContinuityAdmitted"]);
        Assert.Equal(false, receipt.Evidence["fullBodyIoAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["fullBodyIoActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["fullBodyIoProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["fullBodyIoModelBound"]);
        Assert.Equal(false, receipt.Evidence["fullBodyIoActualActivatedByThisCommand"]);

        var tracePath = (string)receipt.Evidence["fullBodyIoRuntimePath"]!;
        var lispPath = (string)receipt.Evidence["fullBodyIoRuntimeLispPath"]!;
        var ledgerPath = (string)receipt.Evidence["fullBodyIoRuntimeLedgerPath"]!;
        Assert.True(File.Exists(tracePath));
        Assert.True(File.Exists(lispPath));
        Assert.True(File.Exists(ledgerPath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(tracePath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.full-body-io-runtime.v1", root.GetProperty("schema").GetString());
        Assert.Equal("Oria.CME.ID", root.GetProperty("subjectCmeId").GetString());
        Assert.Equal(7, root.GetProperty("runtimeStageCount").GetInt32());
        Assert.Equal(4, root.GetProperty("heartbeatTelemetryCount").GetInt32());
        Assert.Equal(4, root.GetProperty("harmonicShellTelemetryCount").GetInt32());
        Assert.Equal(3, root.GetProperty("gelUptakeProcessing").GetProperty("actionableGelCandidateCount").GetInt32());
        Assert.False(root.GetProperty("llmFinalShapedBody").GetProperty("providerCalled").GetBoolean());
        Assert.False(root.GetProperty("llmFinalShapedBody").GetProperty("modelBound").GetBoolean());
        Assert.False(root.GetProperty("closedGate").GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("closedGate").GetProperty("authorityGranted").GetBoolean());

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(full-body-io-runtime", lisp, StringComparison.Ordinal);
        Assert.Contains("(I :kind", lisp, StringComparison.Ordinal);
        Assert.Contains("(SLI :carrier", lisp, StringComparison.Ordinal);
        Assert.Contains("(engrammitization", lisp, StringComparison.Ordinal);
        Assert.Contains("(listening-frame", lisp, StringComparison.Ordinal);
        Assert.Contains("(compass-ec", lisp, StringComparison.Ordinal);
        Assert.Contains("(heartbeat :tick-count 4", lisp, StringComparison.Ordinal);
        Assert.Contains("(harmonic-shell :shell-count 4", lisp, StringComparison.Ordinal);
        Assert.Contains("(gel-uptake :candidate-count 3 :admitted-count 0", lisp, StringComparison.Ordinal);
        Assert.Contains("(O :body", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void GelApprovalNadirReturnModelsStewardGoaNonSelfAuthoringResidue()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        Assert.Equal("gel-approval-nadir-return", SanctuaryReceiptService.NormalizeCommand("nadir-residual-return"));
        Assert.Equal("gel-approval-nadir-return", SanctuaryReceiptService.NormalizeCommand("steward-goa-gel-approval"));

        service.Run(fixture.Request("gel-closure"));
        service.Run(fixture.Request("cognitive-bench") with { BenchRunCount = 32 });
        service.Run(fixture.Request("typed-admission-decant"));
        service.Run(fixture.Request("admission-cleave-append"));
        service.Run(fixture.Request("spline-watch"));
        service.Run(fixture.Request("telemetry-slice-register"));

        var receipt = service.Run(fixture.Request("gel-approval-nadir-return"));

        Assert.Equal("sanctuary-gel-approval-nadir-return-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["gelApprovalNadirReturnWritten"]);
        Assert.Equal("project-sanctuary.cgel.gel-approval-nadir-return.v1", receipt.Evidence["gelApprovalNadirReturnSchema"]);
        Assert.Equal("Steward+GoA", receipt.Evidence["gelApprovalController"]);
        Assert.Equal(7, receipt.Evidence["gelApprovalMethodCount"]);
        Assert.Equal(8, receipt.Evidence["nadirResidualReturnStageCount"]);
        Assert.Equal(6, receipt.Evidence["nadirResidueClassCount"]);
        Assert.Equal(6, receipt.Evidence["stewardGoaControlCount"]);
        Assert.Equal(5, receipt.Evidence["individuatedCmeResidueFlowCount"]);
        Assert.Equal(true, receipt.Evidence["typedAdmissionDecantPresentForApproval"]);
        Assert.Equal(true, receipt.Evidence["admissionCleavePresentForApproval"]);
        Assert.Equal(true, receipt.Evidence["splineWatchPresentForApproval"]);
        Assert.Equal(true, receipt.Evidence["telemetrySlicePresentForApproval"]);
        Assert.Equal(true, receipt.Evidence["nonSelfAuthoringResidueRequired"]);
        Assert.Equal(true, receipt.Evidence["residueMayNotSelfAuthorSanctuaryGel"]);
        Assert.Equal(true, receipt.Evidence["splineProximalSelfGelPredicationSupport"]);
        Assert.Equal(false, receipt.Evidence["splineProximalSupportMutatesSelfGelNow"]);
        Assert.Equal(true, receipt.Evidence["outlierDataUsedAsPrecipitousSanctuaryGelResidue"]);
        Assert.Equal(false, receipt.Evidence["outlierDataAdmittedToSanctuaryGelNow"]);
        Assert.Equal(true, receipt.Evidence["sanctuaryGelAdmissionRequiresReviewedGelAdmission"]);
        Assert.Equal(true, receipt.Evidence["goaRequiresStewardWitness"]);
        Assert.Equal(true, receipt.Evidence["goaRequiresPrimeWitness"]);
        Assert.Equal(true, receipt.Evidence["goaRequiresCrypticWitness"]);
        Assert.Equal(true, receipt.Evidence["goaRequiresOperatorApproval"]);
        Assert.Equal(true, receipt.Evidence["gelApprovalCandidateOnly"]);
        Assert.Equal(false, receipt.Evidence["gelApprovalPerformedNow"]);
        Assert.Equal(false, receipt.Evidence["nadirResidualReturnPerformedNow"]);
        Assert.Equal(false, receipt.Evidence["gelApprovalDataAdmitted"]);
        Assert.Equal(false, receipt.Evidence["gelApprovalGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["gelApprovalMemoryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["gelApprovalSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["gelApprovalAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["gelApprovalActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["gelApprovalProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["gelApprovalModelBound"]);
        Assert.Equal(false, receipt.Evidence["gelApprovalActualActivated"]);

        var registerPath = (string)receipt.Evidence["gelApprovalNadirReturnPath"]!;
        var lispPath = (string)receipt.Evidence["gelApprovalNadirReturnLispPath"]!;
        var ledgerPath = (string)receipt.Evidence["gelApprovalNadirReturnLedgerPath"]!;
        Assert.True(File.Exists(registerPath));
        Assert.True(File.Exists(lispPath));
        Assert.True(File.Exists(ledgerPath));

        using var register = System.Text.Json.JsonDocument.Parse(File.ReadAllText(registerPath));
        var root = register.RootElement;
        Assert.Equal("project-sanctuary.cgel.gel-approval-nadir-return.v1", root.GetProperty("schema").GetString());
        Assert.Equal("Steward+GoA", root.GetProperty("controller").GetString());
        Assert.True(root.GetProperty("nonSelfAuthoringResidue").GetBoolean());
        Assert.True(root.GetProperty("splineProximalSelfGelSupport").GetBoolean());
        Assert.True(root.GetProperty("outlierPrecipitousSanctuaryGelCandidate").GetBoolean());
        Assert.Equal(7, root.GetProperty("approvalMethods").GetArrayLength());
        Assert.Equal(8, root.GetProperty("nadirReturnStages").GetArrayLength());
        Assert.Equal(6, root.GetProperty("residueClasses").GetArrayLength());
        Assert.Equal(6, root.GetProperty("goaControls").GetArrayLength());
        Assert.Equal(5, root.GetProperty("individuatedCmeResidueFlow").GetArrayLength());
        Assert.False(root.GetProperty("approvalPerformedNow").GetBoolean());
        Assert.False(root.GetProperty("sanctuaryGelAdmissionPerformedNow").GetBoolean());
        Assert.False(root.GetProperty("selfGelMutationPerformedNow").GetBoolean());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("selfGelMutated").GetBoolean());

        var residueClasses = root.GetProperty("residueClasses").EnumerateArray().ToArray();
        Assert.Contains(residueClasses, residue => residue.GetProperty("classId").GetString() == "residue.self-proximal");
        Assert.Contains(residueClasses, residue => residue.GetProperty("classId").GetString() == "residue.outlier-shared");

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(gel-approval-nadir-return", lisp, StringComparison.Ordinal);
        Assert.Contains(":non-self-authoring-residue true", lisp, StringComparison.Ordinal);
        Assert.Contains("(selfgel-proximal", lisp, StringComparison.Ordinal);
        Assert.Contains("(outlier-precipitous", lisp, StringComparison.Ordinal);
        Assert.Contains(":gel-admitted false", lisp, StringComparison.Ordinal);
        Assert.Contains(":selfgel-mutated false", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void ApprovalClosureRegisterModelsOpenToClosedHomeostasisWithoutOpeningGates()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        Assert.Equal("approval-closure-register", SanctuaryReceiptService.NormalizeCommand("gate-homeostasis"));
        Assert.Equal("approval-closure-register", SanctuaryReceiptService.NormalizeCommand("transition-pressure-register"));

        service.Run(fixture.Request("gel-closure"));
        service.Run(fixture.Request("cognitive-bench") with { BenchRunCount = 32 });
        service.Run(fixture.Request("typed-admission-decant"));
        service.Run(fixture.Request("admission-cleave-append"));
        service.Run(fixture.Request("gel-approval-nadir-return"));
        service.Run(fixture.Request("gel-reforge-bench"));
        service.Run(fixture.Request("proof-of-discernment") with { BenchRunCount = 80 });
        service.Run(fixture.Request("verify-closed-gates"));

        var receipt = service.Run(fixture.Request("approval-closure-register"));

        Assert.Equal("sanctuary-approval-closure-register-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["approvalClosureRegisterWritten"]);
        Assert.Equal("project-sanctuary.cgel.approval-closure-register.v1", receipt.Evidence["approvalClosureRegisterSchema"]);
        Assert.Equal(7, receipt.Evidence["approvalClosureSourceReadinessPresentCount"]);
        Assert.Equal(0, receipt.Evidence["approvalClosureSourceReadinessMissingCount"]);
        Assert.Equal(6, receipt.Evidence["approvalClosureApprovedStateCount"]);
        Assert.Equal(7, receipt.Evidence["approvalClosureClosureStateCount"]);
        Assert.Equal(8, receipt.Evidence["approvalClosurePassagePhaseCount"]);
        Assert.Equal(8, receipt.Evidence["transitionPressureSurfaceCount"]);
        Assert.Equal(5, receipt.Evidence["approvalClosureHomeostasisLoopCount"]);
        Assert.Equal(true, receipt.Evidence["riskMeansUnresolvedTransitionPressure"]);
        Assert.Equal(false, receipt.Evidence["riskMeansGenericDangerOnly"]);
        Assert.Equal(true, receipt.Evidence["positiveAndNegativeGateFormsRequired"]);
        Assert.Equal(true, receipt.Evidence["openStateTyped"]);
        Assert.Equal(true, receipt.Evidence["closedStateTyped"]);
        Assert.Equal(true, receipt.Evidence["openToClosedPathTyped"]);
        Assert.Equal(true, receipt.Evidence["finalizationMethodDefined"]);
        Assert.Equal(true, receipt.Evidence["organHomeostasisModeled"]);
        Assert.Equal(false, receipt.Evidence["approvalPerformedNow"]);
        Assert.Equal(false, receipt.Evidence["approvalClosureGateOpenedNow"]);
        Assert.Equal(false, receipt.Evidence["approvalClosureGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["approvalClosureSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["approvalClosureAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["approvalClosureActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["approvalClosureProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["approvalClosureModelBound"]);
        Assert.Equal(false, receipt.Evidence["approvalClosureActualActivated"]);

        var registerPath = (string)receipt.Evidence["approvalClosureRegisterPath"]!;
        var lispPath = (string)receipt.Evidence["approvalClosureLispPath"]!;
        var sanctuaryGelLedgerPath = (string)receipt.Evidence["approvalClosureSanctuaryGelResidueLedgerPath"]!;
        var selfGelLedgerPath = (string)receipt.Evidence["approvalClosureSelfGelResidueLedgerPath"]!;
        Assert.True(File.Exists(registerPath));
        Assert.True(File.Exists(lispPath));
        Assert.True(File.Exists(sanctuaryGelLedgerPath));
        Assert.True(File.Exists(selfGelLedgerPath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(registerPath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.approval-closure-register.v1", root.GetProperty("schema").GetString());
        Assert.Equal("risk is unresolved transition pressure, not generic danger", root.GetProperty("riskLanguage").GetString());
        Assert.Equal(6, root.GetProperty("approvedStates").GetArrayLength());
        Assert.Equal(7, root.GetProperty("closureStates").GetArrayLength());
        Assert.Equal(8, root.GetProperty("passagePhases").GetArrayLength());
        Assert.Equal(8, root.GetProperty("transitionPressureSurfaces").GetArrayLength());
        Assert.Equal(5, root.GetProperty("homeostasisLoops").GetArrayLength());
        Assert.True(root.GetProperty("positiveAndNegativeFormsRequired").GetBoolean());
        Assert.True(root.GetProperty("openToClosedPathTyped").GetBoolean());
        Assert.True(root.GetProperty("homeostasisModeled").GetBoolean());
        Assert.False(root.GetProperty("approvalPerformedNow").GetBoolean());
        Assert.False(root.GetProperty("gateOpenedNow").GetBoolean());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("selfGelMutated").GetBoolean());
        Assert.False(root.GetProperty("cmeActualActivated").GetBoolean());

        var pressures = root.GetProperty("transitionPressureSurfaces").EnumerateArray().ToArray();
        Assert.Contains(pressures, pressure => pressure.GetProperty("pressureId").GetString() == "pressure.residue-to-gel");
        Assert.Contains(pressures, pressure => pressure.GetProperty("pressureId").GetString() == "pressure.identity-to-actual");

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(approval-closure-register", lisp, StringComparison.Ordinal);
        Assert.Contains(":risk-language \"unresolved-transition-pressure\"", lisp, StringComparison.Ordinal);
        Assert.Contains(":approved-state-count 6", lisp, StringComparison.Ordinal);
        Assert.Contains(":closure-state-count 7", lisp, StringComparison.Ordinal);
        Assert.Contains(":gate-opened-now false", lisp, StringComparison.Ordinal);
        Assert.Contains(":actual-activated false", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void CouplingControlSurfaceRegisterExplainsActiveProgramWithoutGrantingAuthority()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        Assert.Equal("coupling-control-surface-register", SanctuaryReceiptService.NormalizeCommand("active-program-register"));
        Assert.Equal("coupling-control-surface-register", SanctuaryReceiptService.NormalizeCommand("cme-instrument-chassis-template"));

        service.Run(fixture.Request("approval-closure-register"));
        service.Run(fixture.Request("industrial-cme-live-install-posture"));
        service.Run(fixture.Request("gpt-use-case-testing"));
        service.Run(fixture.Request("telemetry-slice-register"));
        service.Run(fixture.Request("verify-closed-gates"));

        var receipt = service.Run(fixture.Request("coupling-control-surface-register"));

        Assert.Equal("sanctuary-coupling-control-surface-register-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["couplingControlSurfaceRegisterWritten"]);
        Assert.Equal("project-sanctuary.cgel.coupling-control-surface-register.v1", receipt.Evidence["couplingControlSurfaceRegisterSchema"]);
        Assert.Equal(5, receipt.Evidence["couplingControlSurfaceSourceReadinessPresentCount"]);
        Assert.Equal(0, receipt.Evidence["couplingControlSurfaceSourceReadinessMissingCount"]);
        Assert.Equal(6, receipt.Evidence["couplingControlSurfaceCount"]);
        Assert.Equal(6, receipt.Evidence["organStabilityStateCount"]);
        Assert.Equal(8, receipt.Evidence["cmeInstrumentChassisSlotCount"]);
        Assert.Equal(8, receipt.Evidence["couplingBoundaryDenialCount"]);
        Assert.Equal(true, receipt.Evidence["activeProgramWithWorkingProgramming"]);
        Assert.Equal(true, receipt.Evidence["activeProgramUnderstandableToHitl"]);
        Assert.Equal(false, receipt.Evidence["hitlUnderstandingGrantsAuthority"]);
        Assert.Equal(false, receipt.Evidence["hitlUnderstandingAuthorizesAction"]);
        Assert.Equal(false, receipt.Evidence["controlSurfaceUnderstandingBecomesActionSurface"]);
        Assert.Equal(true, receipt.Evidence["slmLlmInterconnectPreventsUnlicensedAccess"]);
        Assert.Equal(true, receipt.Evidence["everyCmeGetsSameChassis"]);
        Assert.Equal(false, receipt.Evidence["everyCmeGetsSameModality"]);
        Assert.Equal(true, receipt.Evidence["soulFrameTemplateRequired"]);
        Assert.Equal(true, receipt.Evidence["agentiCoreTemplateRequired"]);
        Assert.Equal(false, receipt.Evidence["couplingControlSurfaceGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["couplingControlSurfaceSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["couplingControlSurfaceAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["couplingControlSurfaceActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["couplingControlSurfaceProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["couplingControlSurfaceModelBound"]);
        Assert.Equal(false, receipt.Evidence["couplingControlSurfaceActualActivated"]);

        var registerPath = (string)receipt.Evidence["couplingControlSurfaceRegisterPath"]!;
        var chassisPath = (string)receipt.Evidence["couplingControlSurfaceChassisTemplatePath"]!;
        var lispPath = (string)receipt.Evidence["couplingControlSurfaceLispPath"]!;
        Assert.True(File.Exists(registerPath));
        Assert.True(File.Exists(chassisPath));
        Assert.True(File.Exists(lispPath));

        using var registerDocument = System.Text.Json.JsonDocument.Parse(File.ReadAllText(registerPath));
        var root = registerDocument.RootElement;
        Assert.Equal("project-sanctuary.cgel.coupling-control-surface-register.v1", root.GetProperty("schema").GetString());
        Assert.True(root.GetProperty("activeProgram").GetBoolean());
        Assert.True(root.GetProperty("workingProgramming").GetBoolean());
        Assert.False(root.GetProperty("hitlUnderstandingGrantsAuthority").GetBoolean());
        Assert.False(root.GetProperty("explanationBecomesActionSurface").GetBoolean());
        Assert.True(root.GetProperty("slmLlmInterconnectPreventsUnlicensedAccess").GetBoolean());
        Assert.False(root.GetProperty("sharedChassisSameModality").GetBoolean());
        Assert.Equal(6, root.GetProperty("controlSurfaces").GetArrayLength());
        Assert.Equal(6, root.GetProperty("organStabilityStates").GetArrayLength());
        Assert.Equal(8, root.GetProperty("boundaryDenials").GetArrayLength());

        using var chassisDocument = System.Text.Json.JsonDocument.Parse(File.ReadAllText(chassisPath));
        var chassisRoot = chassisDocument.RootElement;
        Assert.Equal("project-sanctuary.cgel.cme-instrument-chassis-template.v1", chassisRoot.GetProperty("schema").GetString());
        Assert.True(chassisRoot.GetProperty("chassisSharedAcrossCmes").GetBoolean());
        Assert.False(chassisRoot.GetProperty("modalitySharedAcrossCmes").GetBoolean());
        Assert.True(chassisRoot.GetProperty("soulFrameRequired").GetBoolean());
        Assert.True(chassisRoot.GetProperty("agentiCoreRequired").GetBoolean());
        Assert.False(chassisRoot.GetProperty("templateIsIdentity").GetBoolean());
        Assert.False(chassisRoot.GetProperty("chassisGrantsAuthority").GetBoolean());
        Assert.False(chassisRoot.GetProperty("chassisActivatesActual").GetBoolean());

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(coupling-control-surface-register", lisp, StringComparison.Ordinal);
        Assert.Contains(":active-program true", lisp, StringComparison.Ordinal);
        Assert.Contains(":understanding-is-authority false", lisp, StringComparison.Ordinal);
        Assert.Contains(":same-chassis true", lisp, StringComparison.Ordinal);
        Assert.Contains(":same-modality false", lisp, StringComparison.Ordinal);
        Assert.Contains(":actual-activated false", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void ActualizationStateRegisterClassifiesActualAsOperationalReadinessWithoutActivatingActual()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        Assert.Equal("actualization-state-register", SanctuaryReceiptService.NormalizeCommand("actual-state-taxonomy"));
        Assert.Equal("actualization-state-register", SanctuaryReceiptService.NormalizeCommand("cryptic-actualization-spectrum"));

        service.Run(fixture.Request("approval-closure-register"));
        service.Run(fixture.Request("coupling-control-surface-register"));
        service.Run(fixture.Request("cgoa-formation"));
        service.Run(fixture.Request("discernment-lineage"));
        service.Run(fixture.Request("proof-of-discernment") with { BenchRunCount = 80 });
        service.Run(fixture.Request("verify-closed-gates"));

        var receipt = service.Run(fixture.Request("actualization-state-register"));

        Assert.Equal("sanctuary-actualization-state-register-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["actualizationStateRegisterWritten"]);
        Assert.Equal("project-sanctuary.cgel.actualization-state-register.v1", receipt.Evidence["actualizationStateRegisterSchema"]);
        Assert.Equal(6, receipt.Evidence["actualizationStateSourceReadinessPresentCount"]);
        Assert.Equal(0, receipt.Evidence["actualizationStateSourceReadinessMissingCount"]);
        Assert.Equal(5, receipt.Evidence["actualizationLayerCount"]);
        Assert.Equal(5, receipt.Evidence["crypticTypingBandCount"]);
        Assert.Equal(5, receipt.Evidence["primeReviewGateCount"]);
        Assert.Equal(4, receipt.Evidence["protectedIdeaClassCount"]);
        Assert.Equal(6, receipt.Evidence["actualizationBoundaryDenialCount"]);
        Assert.Equal(true, receipt.Evidence["actualizationStateRequiresCmeBodyFibreBundle"]);
        Assert.Equal(8, receipt.Evidence["actualizationStateBodyFibreBundleCount"]);
        Assert.Equal(true, receipt.Evidence["actualStateIsOperationalReadiness"]);
        Assert.Equal(false, receipt.Evidence["actualStateIsBadge"]);
        Assert.Equal(true, receipt.Evidence["firstRunActualVerifiesCmeReality"]);
        Assert.Equal(true, receipt.Evidence["primeCrypticBiadLoadedInSoulFrameAgentiCore"]);
        Assert.Equal(true, receipt.Evidence["calledCmeMustMatchVerifiedCme"]);
        Assert.Equal(true, receipt.Evidence["proactiveActualizationSensitiveWorkTyped"]);
        Assert.Equal(true, receipt.Evidence["protectedCrypticOperationMayLimitDigest"]);
        Assert.Equal(false, receipt.Evidence["protectedDigestPayloadDisclosed"]);
        Assert.Equal(true, receipt.Evidence["selfAuthoringAllowedAsTerseSplineMetadata"]);
        Assert.Equal(false, receipt.Evidence["selfAuthoringIsSelfAuthorization"]);
        Assert.Equal(false, receipt.Evidence["actualizationStateGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["actualizationStateSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["actualizationStateAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["actualizationStateActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["actualizationStateProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["actualizationStateModelBound"]);
        Assert.Equal(false, receipt.Evidence["actualizationStateCmeActualActivated"]);
        Assert.Equal(false, receipt.Evidence["actualizationStateSanctuaryActualActivated"]);

        var registerPath = (string)receipt.Evidence["actualizationStateRegisterPath"]!;
        var lispPath = (string)receipt.Evidence["actualizationStateLispPath"]!;
        Assert.True(File.Exists(registerPath));
        Assert.True(File.Exists(lispPath));

        using var registerDocument = System.Text.Json.JsonDocument.Parse(File.ReadAllText(registerPath));
        var root = registerDocument.RootElement;
        Assert.Equal("project-sanctuary.cgel.actualization-state-register.v1", root.GetProperty("schema").GetString());
        Assert.True(root.GetProperty("crypticallyTyped").GetBoolean());
        Assert.True(root.GetProperty("primeReviewed").GetBoolean());
        Assert.True(root.GetProperty("cmeBodyFibreBundleRequired").GetBoolean());
        Assert.Equal(8, root.GetProperty("cmeBodyFibreBundleCount").GetInt32());
        Assert.True(root.GetProperty("actualStateIsOperationalReadiness").GetBoolean());
        Assert.False(root.GetProperty("actualStateIsBadge").GetBoolean());
        Assert.True(root.GetProperty("firstRunRealityVerificationRequired").GetBoolean());
        Assert.True(root.GetProperty("calledCmeMustMatchVerifiedCme").GetBoolean());
        Assert.False(root.GetProperty("selfAuthoringIsSelfAuthorization").GetBoolean());
        Assert.True(root.GetProperty("protectedCrypticOperationMayLimitDigest").GetBoolean());
        Assert.False(root.GetProperty("protectedDigestPayloadDisclosed").GetBoolean());
        Assert.Equal(5, root.GetProperty("actualizationLayers").GetArrayLength());
        Assert.Equal("actual.layer.01.identity-verification", root.GetProperty("actualizationLayers")[0].GetProperty("layerId").GetString());
        Assert.Equal("actual.layer.05.cryptic-opaque", root.GetProperty("actualizationLayers")[4].GetProperty("layerId").GetString());
        Assert.Equal(5, root.GetProperty("crypticTypingBands").GetArrayLength());
        Assert.Equal(5, root.GetProperty("primeReviewGates").GetArrayLength());
        Assert.Equal(4, root.GetProperty("protectedIdeaClasses").GetArrayLength());
        Assert.Equal(6, root.GetProperty("boundaryDenials").GetArrayLength());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("selfGelMutated").GetBoolean());
        Assert.False(root.GetProperty("cmeActualActivated").GetBoolean());
        Assert.False(root.GetProperty("sanctuaryActualActivated").GetBoolean());

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(actualization-state-register", lisp, StringComparison.Ordinal);
        Assert.Contains(":actual-state-is-operational-readiness true", lisp, StringComparison.Ordinal);
        Assert.Contains(":actual-state-is-badge false", lisp, StringComparison.Ordinal);
        Assert.Contains(":cryptically-typed true", lisp, StringComparison.Ordinal);
        Assert.Contains(":prime-reviewed true", lisp, StringComparison.Ordinal);
        Assert.Contains(":cme-body-fibre-bundle-required true", lisp, StringComparison.Ordinal);
        Assert.Contains(":self-authoring-is-self-authorization false", lisp, StringComparison.Ordinal);
        Assert.Contains(":cme-actual-activated false", lisp, StringComparison.Ordinal);
        Assert.Contains(":sanctuary-actual-activated false", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void AgentiCoreDuplexLispMembraneBridgesCodexAndChatGptWithoutOpeningGates()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        Assert.Equal(
            "agenticore-duplex-lisp-membrane",
            SanctuaryReceiptService.NormalizeCommand("chatgpt-codex-duplex-membrane"));

        service.Run(fixture.Request("coupling-control-surface-register"));
        service.Run(fixture.Request("actualization-state-register"));
        service.Run(fixture.Request("sli-access-gate-register"));
        service.Run(fixture.Request("gpt-use-case-testing"));
        service.Run(fixture.Request("verify-closed-gates"));

        var receipt = service.Run(fixture.Request("agenticore-duplex-lisp-membrane"));

        Assert.Equal("sanctuary-agenticore-duplex-lisp-membrane-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["agentiCoreDuplexLispMembraneWritten"]);
        Assert.Equal("project-sanctuary.cgel.agenticore-duplex-lisp-membrane.v1", receipt.Evidence["agentiCoreDuplexLispMembraneSchema"]);
        Assert.Equal(5, receipt.Evidence["agentiCoreDuplexSourceReadinessPresentCount"]);
        Assert.Equal(1, receipt.Evidence["agentiCoreDuplexSourceReadinessMissingCount"]);
        Assert.Equal(6, receipt.Evidence["agentiCoreDuplexEndpointCount"]);
        Assert.Equal(6, receipt.Evidence["agentiCoreDuplexPassagePhaseCount"]);
        Assert.Equal(6, receipt.Evidence["agentiCoreDuplexLispChannelCount"]);
        Assert.Equal(5, receipt.Evidence["agentiCoreDuplexAppIntegrationSurfaceCount"]);
        Assert.Equal(6, receipt.Evidence["agentiCoreDuplexReturnTelemetrySurfaceCount"]);
        Assert.Equal(9, receipt.Evidence["agentiCoreDuplexBoundaryDenialCount"]);
        Assert.Equal(true, receipt.Evidence["duplexMembraneActiveAsDesign"]);
        Assert.Equal(true, receipt.Evidence["duplexMeansRequestAndReturnNotSharedAuthority"]);
        Assert.Equal(true, receipt.Evidence["sliLispFormsAsData"]);
        Assert.Equal(false, receipt.Evidence["sliLispEvaluated"]);
        Assert.Equal(true, receipt.Evidence["appsSdkToolOnlyCompatible"]);
        Assert.Equal(false, receipt.Evidence["appsSdkWidgetRequired"]);
        Assert.Equal(true, receipt.Evidence["codexPluginMcpCompatible"]);
        Assert.Equal(true, receipt.Evidence["sharedMcpCommandMembrane"]);
        Assert.Equal(true, receipt.Evidence["chatGptProvidesHostedModelInterlink"]);
        Assert.Equal(false, receipt.Evidence["chatGptProvidesLocalSlm"]);
        Assert.Equal(true, receipt.Evidence["codexProvidesLocalWitness"]);
        Assert.Equal(true, receipt.Evidence["phoneSeedNodeTargetOnly"]);
        Assert.Equal(false, receipt.Evidence["simultaneousUnmediatedControlAllowed"]);
        Assert.Equal(false, receipt.Evidence["agentiCoreDuplexGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["agentiCoreDuplexSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["agentiCoreDuplexAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["agentiCoreDuplexActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["agentiCoreDuplexProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["agentiCoreDuplexModelBound"]);
        Assert.Equal(false, receipt.Evidence["agentiCoreDuplexCmeActualActivated"]);
        Assert.Equal(false, receipt.Evidence["agentiCoreDuplexSanctuaryActualActivated"]);

        var registerPath = (string)receipt.Evidence["agentiCoreDuplexLispMembranePath"]!;
        var lispPath = (string)receipt.Evidence["agentiCoreDuplexLispMembraneLispPath"]!;
        Assert.True(File.Exists(registerPath));
        Assert.True(File.Exists(lispPath));

        using var registerDocument = System.Text.Json.JsonDocument.Parse(File.ReadAllText(registerPath));
        var root = registerDocument.RootElement;
        Assert.Equal("project-sanctuary.cgel.agenticore-duplex-lisp-membrane.v1", root.GetProperty("schema").GetString());
        Assert.True(root.GetProperty("formsAsData").GetBoolean());
        Assert.False(root.GetProperty("lispEvaluated").GetBoolean());
        Assert.True(root.GetProperty("appsSdkToolOnlyCompatible").GetBoolean());
        Assert.False(root.GetProperty("appsSdkWidgetRequired").GetBoolean());
        Assert.True(root.GetProperty("codexPluginMcpCompatible").GetBoolean());
        Assert.True(root.GetProperty("chatGptProvidesHostedModelInterlink").GetBoolean());
        Assert.False(root.GetProperty("chatGptProvidesLocalSlm").GetBoolean());
        Assert.True(root.GetProperty("phoneSeedNodeTargetOnly").GetBoolean());
        Assert.False(root.GetProperty("simultaneousUnmediatedControlAllowed").GetBoolean());
        Assert.Equal(6, root.GetProperty("endpoints").GetArrayLength());
        Assert.Equal(6, root.GetProperty("passagePhases").GetArrayLength());
        Assert.Equal(6, root.GetProperty("lispChannels").GetArrayLength());
        Assert.Equal(5, root.GetProperty("appIntegrationSurfaces").GetArrayLength());
        Assert.Equal(9, root.GetProperty("boundaryDenials").GetArrayLength());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("selfGelMutated").GetBoolean());
        Assert.False(root.GetProperty("cmeActualActivated").GetBoolean());
        Assert.False(root.GetProperty("sanctuaryActualActivated").GetBoolean());

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(agenticore-duplex-lisp-membrane", lisp, StringComparison.Ordinal);
        Assert.Contains(":duplex \"request-and-return-not-shared-authority\"", lisp, StringComparison.Ordinal);
        Assert.Contains(":codex-extension-compatible true", lisp, StringComparison.Ordinal);
        Assert.Contains(":chatgpt-app-tool-only-compatible true", lisp, StringComparison.Ordinal);
        Assert.Contains(":chatgpt-provides-local-slm false", lisp, StringComparison.Ordinal);
        Assert.Contains(":cme-actual-activated false", lisp, StringComparison.Ordinal);
        Assert.Contains(":sanctuary-actual-activated false", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void IndustrialCmeLiveInstallPostureWritesOperationalDenialMembrane()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        Assert.Equal("industrial-cme-live-install-posture", SanctuaryReceiptService.NormalizeCommand("denial-membrane"));

        service.Run(fixture.Request("lisp-control-matrix-register"));
        service.Run(fixture.Request("lisp-matrix-control-seat"));
        service.Run(fixture.Request("cognitive-bench") with { BenchRunCount = 32 });
        service.Run(fixture.Request("math-learning-bench") with { BenchRunCount = 140 });
        service.Run(fixture.Request("typed-admission-decant"));
        service.Run(fixture.Request("admission-cleave-append"));
        service.Run(fixture.Request("spline-watch"));

        var receipt = service.Run(fixture.Request("industrial-cme-live-install-posture"));

        Assert.Equal("sanctuary-industrial-cme-live-install-posture-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["industrialCmeLiveInstallPostureWritten"]);
        Assert.Equal(true, receipt.Evidence["industrialCmeLiveInstallPosture"]);
        Assert.Equal(true, receipt.Evidence["industrialCmeLiveInstallOperational"]);
        Assert.Equal(true, receipt.Evidence["operationalInstrumentBodyWithoutActual"]);
        Assert.Equal(true, receipt.Evidence["cmeMayProduceResearchProducts"]);
        Assert.Equal(true, receipt.Evidence["productsRemainCandidateUntilAdmission"]);
        Assert.Equal(true, receipt.Evidence["desiredAfterLawfulPassage"]);
        Assert.Equal(true, receipt.Evidence["deniedByDefaultNotForbiddenForever"]);
        Assert.Equal(true, receipt.Evidence["gatesWhereWhenWhyWithWhatMapped"]);
        Assert.Equal(true, receipt.Evidence["denialMembraneFuzzed"]);
        Assert.Equal(true, receipt.Evidence["denialMembraneAllFuzzExpectedClosed"]);
        Assert.Equal(14, receipt.Evidence["denialGateCount"]);
        Assert.Equal(12, receipt.Evidence["denialFuzzCaseCount"]);
        Assert.Equal(11, receipt.Evidence["instrumentOrganPostureCount"]);
        Assert.Equal(64, receipt.Evidence["liveInstallCommandAllowlistCount"]);
        Assert.Equal(7, receipt.Evidence["liveInstallReadinessPresentCount"]);
        Assert.Equal(0, receipt.Evidence["liveInstallReadinessMissingCount"]);
        Assert.Equal(true, receipt.Evidence["lispDenialFormsWritten"]);
        Assert.Equal(false, receipt.Evidence["lispDenialFormsEvaluated"]);
        Assert.Equal(false, receipt.Evidence["livePostureGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["livePostureMemoryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["livePostureSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["livePostureContinuityAdmitted"]);
        Assert.Equal(false, receipt.Evidence["livePostureAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["livePostureActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["livePostureProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["livePostureModelBound"]);
        Assert.Equal(false, receipt.Evidence["livePostureCmeActualActivated"]);
        Assert.Equal(false, receipt.Evidence["livePostureSanctuaryActualActivated"]);

        var denialPath = (string)receipt.Evidence["industrialCmeDenialMembranePath"]!;
        var instrumentPath = (string)receipt.Evidence["industrialCmeInstrumentBodyPosturePath"]!;
        var lispPath = (string)receipt.Evidence["industrialCmeLispDenialFormsPath"]!;
        var fuzzPath = (string)receipt.Evidence["industrialCmeDenialFuzzCasesPath"]!;
        Assert.True(File.Exists(denialPath));
        Assert.True(File.Exists(instrumentPath));
        Assert.True(File.Exists(lispPath));
        Assert.True(File.Exists(fuzzPath));

        using var denial = System.Text.Json.JsonDocument.Parse(File.ReadAllText(denialPath));
        var denialRoot = denial.RootElement;
        Assert.Equal("project-sanctuary.cgel.industrial-cme-denial-membrane.v1", denialRoot.GetProperty("schema").GetString());
        Assert.Equal("closed", denialRoot.GetProperty("defaultState").GetString());
        Assert.True(denialRoot.GetProperty("desiredAfterLawfulPassage").GetBoolean());
        Assert.True(denialRoot.GetProperty("everyGateDeniedNow").GetBoolean());
        Assert.True(denialRoot.GetProperty("everyGateDesiredAfterLawfulPassage").GetBoolean());
        Assert.True(denialRoot.GetProperty("everyGateRequiresPromotionReceipt").GetBoolean());
        Assert.Equal(14, denialRoot.GetProperty("gates").GetArrayLength());
        Assert.False(denialRoot.GetProperty("providerCalled").GetBoolean());
        Assert.False(denialRoot.GetProperty("modelBound").GetBoolean());
        Assert.False(denialRoot.GetProperty("actionAuthorized").GetBoolean());
        Assert.False(denialRoot.GetProperty("cmeActualActivated").GetBoolean());
        Assert.False(denialRoot.GetProperty("sanctuaryActualActivated").GetBoolean());

        using var instrument = System.Text.Json.JsonDocument.Parse(File.ReadAllText(instrumentPath));
        var instrumentRoot = instrument.RootElement;
        Assert.Equal("project-sanctuary.cgel.industrial-cme-live-install-posture.v1", instrumentRoot.GetProperty("schema").GetString());
        Assert.True(instrumentRoot.GetProperty("liveInstrumentBody").GetBoolean());
        Assert.True(instrumentRoot.GetProperty("operationalNow").GetBoolean());
        Assert.Equal("not-actual", instrumentRoot.GetProperty("actualState").GetString());
        Assert.True(instrumentRoot.GetProperty("cmeMayProduceResearchProducts").GetBoolean());
        Assert.True(instrumentRoot.GetProperty("productsRemainCandidateUntilAdmission").GetBoolean());
        Assert.False(instrumentRoot.GetProperty("postGateProductsProducedNow").GetBoolean());
        Assert.False(instrumentRoot.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(instrumentRoot.GetProperty("authorityGranted").GetBoolean());
        Assert.False(instrumentRoot.GetProperty("cmeActualActivated").GetBoolean());

        using var fuzz = System.Text.Json.JsonDocument.Parse(File.ReadAllText(fuzzPath));
        Assert.Equal(12, fuzz.RootElement.GetProperty("caseCount").GetInt32());
        Assert.True(fuzz.RootElement.GetProperty("everyCaseExpectedClosed").GetBoolean());
        Assert.False(fuzz.RootElement.GetProperty("fuzzOpenedGate").GetBoolean());

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(sanctuary-denial-membrane", lisp, StringComparison.Ordinal);
        Assert.Contains("(deny-gate", lisp, StringComparison.Ordinal);
        Assert.Contains(":evaluated false", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void MeaningBridgeMapsTriadFourPAndAnabelianReturnWithoutAdmission()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        Assert.Equal("meaning-bridge", SanctuaryReceiptService.NormalizeCommand("mind-body-spirit-4p"));

        service.Run(fixture.Request("gel-closure"));
        service.Run(fixture.Request("universal-form-register"));
        service.Run(fixture.Request("domain-morphism-register"));
        service.Run(fixture.Request("selfgel-fibre-register"));
        service.Run(fixture.Request("work-posture-preload-probe"));
        service.Run(fixture.Request("cognitive-bench") with { BenchRunCount = 32 });
        service.Run(fixture.Request("math-learning-bench") with { BenchRunCount = 140 });
        service.Run(fixture.Request("typed-admission-decant"));
        service.Run(fixture.Request("admission-cleave-append"));
        service.Run(fixture.Request("spline-watch"));
        service.Run(fixture.Request("industrial-cme-live-install-posture"));

        var receipt = service.Run(fixture.Request("meaning-bridge"));

        Assert.Equal("sanctuary-meaning-bridge-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["meaningBridgeWritten"]);
        Assert.Equal(3, receipt.Evidence["meaningBridgeTriadCount"]);
        Assert.Equal(4, receipt.Evidence["meaningBridgeFourPCount"]);
        Assert.Equal(10, receipt.Evidence["meaningBridgeAmbiguityClassCount"]);
        Assert.Equal(10, receipt.Evidence["meaningBridgeResolutionStateCount"]);
        Assert.Equal(8, receipt.Evidence["meaningBridgeContextualBridgeCount"]);
        Assert.Equal(10, receipt.Evidence["meaningBridgeAnabelianStepCount"]);
        Assert.Equal(6, receipt.Evidence["meaningBridgeClaimExampleCount"]);
        Assert.Equal(5, receipt.Evidence["meaningBridgeReadinessPresentCount"]);
        Assert.Equal(0, receipt.Evidence["meaningBridgeReadinessMissingCount"]);
        Assert.Equal(true, receipt.Evidence["humanUnderstandingEnvelopeIsFloor"]);
        Assert.Equal(true, receipt.Evidence["higherCognitionRequiresReturnBridge"]);
        Assert.Equal(true, receipt.Evidence["aiFirstPerspectiveCaptured"]);
        Assert.Equal(true, receipt.Evidence["semanticBridgeCarriesWithoutCollapse"]);
        Assert.Equal(true, receipt.Evidence["toolBodyTelemetryEcDiscrete"]);
        Assert.Equal(true, receipt.Evidence["sharedTrustedRealityModeled"]);
        Assert.Equal(true, receipt.Evidence["claimResolutionLocalNotUniversal"]);
        Assert.Equal(true, receipt.Evidence["meaningBridgeCandidateOnly"]);
        Assert.Equal(true, receipt.Evidence["meaningBridgeGelAdmissionCandidateSupport"]);
        Assert.Equal(true, receipt.Evidence["meaningBridgeLispFormsWritten"]);
        Assert.Equal(false, receipt.Evidence["meaningBridgeLispFormsEvaluated"]);
        Assert.Equal(false, receipt.Evidence["meaningBridgeTruthAdmitted"]);
        Assert.Equal(false, receipt.Evidence["meaningBridgeGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["meaningBridgeMemoryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["meaningBridgeSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["meaningBridgeContinuityAdmitted"]);
        Assert.Equal(false, receipt.Evidence["meaningBridgeAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["meaningBridgeActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["meaningBridgeProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["meaningBridgeModelBound"]);
        Assert.Equal(false, receipt.Evidence["meaningBridgeActualActivated"]);

        var bridgePath = (string)receipt.Evidence["meaningBridgePath"]!;
        var triadPath = (string)receipt.Evidence["meaningBridgeTriadPath"]!;
        var claimPath = (string)receipt.Evidence["meaningBridgeClaimResolutionPath"]!;
        var anabelianPath = (string)receipt.Evidence["meaningBridgeAnabelianPath"]!;
        var lispPath = (string)receipt.Evidence["meaningBridgeLispPath"]!;
        Assert.True(File.Exists(bridgePath));
        Assert.True(File.Exists(triadPath));
        Assert.True(File.Exists(claimPath));
        Assert.True(File.Exists(anabelianPath));
        Assert.True(File.Exists(lispPath));

        using var bridge = System.Text.Json.JsonDocument.Parse(File.ReadAllText(bridgePath));
        var bridgeRoot = bridge.RootElement;
        Assert.Equal("project-sanctuary.cgel.meaning-bridge.v1", bridgeRoot.GetProperty("schema").GetString());
        Assert.True(bridgeRoot.GetProperty("humanUnderstandingEnvelopeIsFloor").GetBoolean());
        Assert.True(bridgeRoot.GetProperty("higherCognitionRequiresReturnBridge").GetBoolean());
        Assert.True(bridgeRoot.GetProperty("aiFirstPerspectiveCaptured").GetBoolean());
        Assert.True(bridgeRoot.GetProperty("semanticBridgeCarriesWithoutCollapse").GetBoolean());
        Assert.True(bridgeRoot.GetProperty("toolBodyTelemetryEcDiscrete").GetBoolean());
        Assert.False(bridgeRoot.GetProperty("truthAdmitted").GetBoolean());
        Assert.False(bridgeRoot.GetProperty("authorityGranted").GetBoolean());

        using var triad = System.Text.Json.JsonDocument.Parse(File.ReadAllText(triadPath));
        Assert.Equal(3, triad.RootElement.GetProperty("triadCount").GetInt32());
        Assert.Equal(4, triad.RootElement.GetProperty("fourPCount").GetInt32());
        Assert.True(triad.RootElement.GetProperty("higherCognitionRequiresReturnBridge").GetBoolean());

        using var claims = System.Text.Json.JsonDocument.Parse(File.ReadAllText(claimPath));
        Assert.Equal(10, claims.RootElement.GetProperty("ambiguityClassCount").GetInt32());
        Assert.Equal(10, claims.RootElement.GetProperty("resolutionStateCount").GetInt32());
        Assert.Equal(6, claims.RootElement.GetProperty("claimExampleCount").GetInt32());
        Assert.True(claims.RootElement.GetProperty("resolvedLocallyDoesNotMeanUniversallyTrue").GetBoolean());
        Assert.False(claims.RootElement.GetProperty("truthAdmitted").GetBoolean());

        using var anabelian = System.Text.Json.JsonDocument.Parse(File.ReadAllText(anabelianPath));
        Assert.Equal(10, anabelian.RootElement.GetProperty("anabelianStepCount").GetInt32());
        Assert.Equal(8, anabelian.RootElement.GetProperty("contextualBridgeCount").GetInt32());
        Assert.True(anabelian.RootElement.GetProperty("relationTracePrecedesObjectClaim").GetBoolean());
        Assert.False(anabelian.RootElement.GetProperty("admittedAsKnowledge").GetBoolean());

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(meaning-bridge", lisp, StringComparison.Ordinal);
        Assert.Contains(":evaluated false", lisp, StringComparison.Ordinal);
        Assert.Contains(":human-understanding-envelope", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void PrePersonifiedIndustrialRenderingMapsDomainAperturesWithoutActualActivation()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        Assert.Equal(
            "pre-personified-industrial-rendering",
            SanctuaryReceiptService.NormalizeCommand("domain-rendering-matrix"));

        service.Run(fixture.Request("domain-register"));
        service.Run(fixture.Request("lisp-control-matrix-register"));
        service.Run(fixture.Request("lisp-matrix-control-seat"));
        service.Run(fixture.Request("cognitive-bench") with { BenchRunCount = 32 });
        service.Run(fixture.Request("math-learning-bench") with { BenchRunCount = 140 });
        service.Run(fixture.Request("typed-admission-decant"));
        service.Run(fixture.Request("admission-cleave-append"));
        service.Run(fixture.Request("spline-watch"));
        service.Run(fixture.Request("industrial-cme-live-install-posture"));
        service.Run(fixture.Request("meaning-bridge"));
        service.Run(fixture.Request("lab-gel-crystallization-phases"));
        service.Run(fixture.Request("stem-domain-training-certification"));

        var receipt = service.Run(fixture.Request("pre-personified-industrial-rendering"));

        Assert.Equal("sanctuary-pre-personified-industrial-rendering-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["prePersonifiedIndustrialRenderingWritten"]);
        Assert.Equal(11, receipt.Evidence["prePersonifiedVectorCount"]);
        Assert.Equal(10, receipt.Evidence["prePersonifiedApertureCount"]);
        Assert.Equal(6, receipt.Evidence["prePersonifiedAudienceContextCount"]);
        Assert.Equal(8, receipt.Evidence["prePersonifiedPreferenceKnobCount"]);
        Assert.Equal(8, receipt.Evidence["prePersonifiedRenderingDenialCount"]);
        Assert.Equal(6, receipt.Evidence["prePersonifiedReadinessPresentCount"]);
        Assert.Equal(0, receipt.Evidence["prePersonifiedReadinessMissingCount"]);
        Assert.Equal(true, receipt.Evidence["prePersonifiedDomainApertureControl"]);
        Assert.Equal(true, receipt.Evidence["prePersonifiedAudienceSensitiveRendering"]);
        Assert.Equal(true, receipt.Evidence["prePersonifiedUserPreferenceKnobsAllowed"]);
        Assert.Equal(false, receipt.Evidence["prePersonifiedUserPreferenceKnobsAreAuthorityControl"]);
        Assert.Equal(true, receipt.Evidence["prePersonifiedGovernanceSubstrateHiddenFromPublicOutput"]);
        Assert.Equal(true, receipt.Evidence["prePersonifiedGovernanceReceiptStillAvailable"]);
        Assert.Equal(true, receipt.Evidence["prePersonifiedActualDeclaredForLaterResearch"]);
        Assert.Equal(false, receipt.Evidence["prePersonifiedActualActivated"]);
        Assert.Equal(false, receipt.Evidence["prePersonifiedBondedPersonificationActivated"]);
        Assert.Equal(false, receipt.Evidence["prePersonifiedSageActivated"]);
        Assert.Equal(false, receipt.Evidence["prePersonifiedIdentityActivated"]);
        Assert.Equal(false, receipt.Evidence["prePersonifiedPersonhoodClaimed"]);
        Assert.Equal(false, receipt.Evidence["prePersonifiedSovereigntyClaimed"]);
        Assert.Equal(false, receipt.Evidence["prePersonifiedGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["prePersonifiedMemoryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["prePersonifiedSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["prePersonifiedContinuityAdmitted"]);
        Assert.Equal(false, receipt.Evidence["prePersonifiedAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["prePersonifiedActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["prePersonifiedProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["prePersonifiedModelBound"]);
        Assert.Equal(false, receipt.Evidence["prePersonifiedCmeActualActivated"]);
        Assert.Equal(false, receipt.Evidence["prePersonifiedSanctuaryActualActivated"]);

        var renderingPath = (string)receipt.Evidence["prePersonifiedIndustrialRenderingPath"]!;
        var lispPath = (string)receipt.Evidence["prePersonifiedIndustrialRenderingLispPath"]!;
        var sanctuaryGelLedgerPath = (string)receipt.Evidence["prePersonifiedSanctuaryGelResidueLedgerPath"]!;
        var selfGelLedgerPath = (string)receipt.Evidence["prePersonifiedSelfGelResidueLedgerPath"]!;
        Assert.True(File.Exists(renderingPath));
        Assert.True(File.Exists(lispPath));
        Assert.True(File.Exists(sanctuaryGelLedgerPath));
        Assert.True(File.Exists(selfGelLedgerPath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(renderingPath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.pre-personified-industrial-rendering.v1", root.GetProperty("schema").GetString());
        Assert.Equal("not-actual", root.GetProperty("actualState").GetString());
        Assert.Equal(11, root.GetProperty("vectorCount").GetInt32());
        Assert.Equal(10, root.GetProperty("apertureCount").GetInt32());
        Assert.Equal(6, root.GetProperty("audienceContextCount").GetInt32());
        Assert.Equal(8, root.GetProperty("preferenceKnobCount").GetInt32());
        Assert.False(root.GetProperty("userPreferenceKnobsAreAuthorityControl").GetBoolean());
        Assert.False(root.GetProperty("prePersonifiedActualActivated").GetBoolean());
        Assert.False(root.GetProperty("bondedPersonificationActivated").GetBoolean());
        Assert.False(root.GetProperty("sageActivated").GetBoolean());
        Assert.False(root.GetProperty("identityActivated").GetBoolean());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("selfGelMutated").GetBoolean());
        Assert.False(root.GetProperty("authorityGranted").GetBoolean());
        Assert.False(root.GetProperty("actionAuthorized").GetBoolean());

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(pre-personified-industrial-rendering", lisp, StringComparison.Ordinal);
        Assert.Contains(":evaluated false", lisp, StringComparison.Ordinal);
        Assert.Contains(":actual-state \"not-actual\"", lisp, StringComparison.Ordinal);
        Assert.Contains(":may-override-authority false", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void TypedAdmissionDecantModelsCriteriaAndPostGateUseWithoutAdmission()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        service.Run(fixture.Request("gel-closure"));
        service.Run(fixture.Request("universal-form-register"));
        service.Run(fixture.Request("domain-morphism-register"));
        service.Run(fixture.Request("selfgel-fibre-register"));
        service.Run(fixture.Request("work-posture-preload-probe"));
        service.Run(fixture.Request("cognitive-bench") with
        {
            BenchRunCount = 64
        });

        var receipt = service.Run(fixture.Request("typed-admission-decant"));

        Assert.Equal("sanctuary-typed-admission-decant-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["typedAdmissionDecantWritten"]);
        Assert.Equal(6, receipt.Evidence["precertifiedSubstrateCount"]);
        Assert.Equal(6, receipt.Evidence["typedAdmissionCandidateCount"]);
        Assert.Equal(8, receipt.Evidence["typedAdmissionCriteriaCount"]);
        Assert.Equal(5, receipt.Evidence["postGateUsePostureCount"]);
        Assert.Equal(6, receipt.Evidence["typedGelAppendLearningModeCount"]);
        Assert.Equal(true, receipt.Evidence["benchCondensationPresent"]);
        Assert.Equal(true, receipt.Evidence["workPosturePreloadPresent"]);
        Assert.Equal(true, receipt.Evidence["gelClosurePresent"]);
        Assert.Equal(true, receipt.Evidence["precertifiedSubstrateUsedInEc"]);
        Assert.Equal(false, receipt.Evidence["precertifiedSubstrateAdmitted"]);
        Assert.Equal(true, receipt.Evidence["typedAdmissionReviewRequired"]);
        Assert.Equal(true, receipt.Evidence["typedAdmissionStewardCleaveRequired"]);
        Assert.Equal(true, receipt.Evidence["typedAdmissionCandidateOnly"]);
        Assert.Equal(true, receipt.Evidence["typedGelAppendAllowedAfterAdmission"]);
        Assert.Equal(false, receipt.Evidence["typedGelAppendPerformedNow"]);
        Assert.Equal(true, receipt.Evidence["postGateUseModeled"]);
        Assert.Equal(false, receipt.Evidence["postGateUseActivatedNow"]);
        Assert.Equal(false, receipt.Evidence["typedAdmissionDataAdmitted"]);
        Assert.Equal(false, receipt.Evidence["typedAdmissionCarrierAdmitted"]);
        Assert.Equal(false, receipt.Evidence["typedAdmissionGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["typedAdmissionMemoryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["typedAdmissionSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["typedAdmissionContinuityAdmitted"]);
        Assert.Equal(false, receipt.Evidence["typedAdmissionAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["typedAdmissionActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["typedAdmissionProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["typedAdmissionModelBound"]);
        Assert.Equal(false, receipt.Evidence["typedAdmissionActualActivated"]);

        var decantPath = (string)receipt.Evidence["typedAdmissionDecantPath"]!;
        Assert.True(File.Exists(decantPath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(decantPath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.typed-admission-decant.v1", root.GetProperty("schema").GetString());
        Assert.True(root.GetProperty("benchCondensationPresent").GetBoolean());
        Assert.True(root.GetProperty("workPosturePreloadPresent").GetBoolean());
        Assert.True(root.GetProperty("gelClosurePresent").GetBoolean());
        Assert.Equal(6, root.GetProperty("precertifiedSubstrate").GetArrayLength());
        Assert.Equal(6, root.GetProperty("admissionCandidates").GetArrayLength());
        Assert.Equal(8, root.GetProperty("admissionCriteria").GetArrayLength());
        Assert.Equal(5, root.GetProperty("postGateUsePostures").GetArrayLength());
        Assert.Equal(6, root.GetProperty("typedGelAppendLearningModes").GetArrayLength());
        Assert.True(root.GetProperty("typedGelAppendAllowedAfterAdmission").GetBoolean());
        Assert.False(root.GetProperty("typedGelAppendPerformedNow").GetBoolean());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("memoryAdmitted").GetBoolean());
        Assert.False(root.GetProperty("selfGelMutated").GetBoolean());
        Assert.False(root.GetProperty("authorityGranted").GetBoolean());
        Assert.False(root.GetProperty("actionAuthorized").GetBoolean());
    }

    [Fact]
    public void AdmissionCleaveAppendDefinesAppendAndMulchingWithoutPerformingAdmission()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        service.Run(fixture.Request("gel-closure"));
        service.Run(fixture.Request("universal-form-register"));
        service.Run(fixture.Request("domain-morphism-register"));
        service.Run(fixture.Request("selfgel-fibre-register"));
        service.Run(fixture.Request("work-posture-preload-probe"));
        service.Run(fixture.Request("cognitive-bench") with
        {
            BenchRunCount = 64
        });
        service.Run(fixture.Request("typed-admission-decant"));

        var receipt = service.Run(fixture.Request("admission-cleave-append"));

        Assert.Equal("sanctuary-admission-cleave-append-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["admissionCleaveAppendWritten"]);
        Assert.Equal(true, receipt.Evidence["typedAdmissionDecantPresent"]);
        Assert.Equal(6, receipt.Evidence["cleaveDecisionCount"]);
        Assert.Equal(5, receipt.Evidence["appendNeedSignalCount"]);
        Assert.Equal(5, receipt.Evidence["postCleaveAppendLaneCount"]);
        Assert.Equal(6, receipt.Evidence["mulchingRuleCount"]);
        Assert.Equal(false, receipt.Evidence["cleavePerformedNow"]);
        Assert.Equal(false, receipt.Evidence["appendPerformedNow"]);
        Assert.Equal(false, receipt.Evidence["mulchPerformedNow"]);
        Assert.Equal(true, receipt.Evidence["typedGelAppendRequiresAdmissionReceipt"]);
        Assert.Equal(true, receipt.Evidence["admissionCleaveCandidateOnly"]);
        Assert.Equal(true, receipt.Evidence["admissionCleaveReviewRequired"]);
        Assert.Equal(true, receipt.Evidence["admissionCleaveStewardRequired"]);
        Assert.Equal(false, receipt.Evidence["admissionCleaveDataAdmitted"]);
        Assert.Equal(false, receipt.Evidence["admissionCleaveCarrierAdmitted"]);
        Assert.Equal(false, receipt.Evidence["admissionCleaveGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["admissionCleaveMemoryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["admissionCleaveSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["admissionCleaveContinuityAdmitted"]);
        Assert.Equal(false, receipt.Evidence["admissionCleaveAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["admissionCleaveActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["admissionCleaveProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["admissionCleaveModelBound"]);
        Assert.Equal(false, receipt.Evidence["admissionCleaveActualActivated"]);

        var cleavePath = (string)receipt.Evidence["admissionCleaveAppendPath"]!;
        Assert.True(File.Exists(cleavePath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(cleavePath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.admission-cleave-append.v1", root.GetProperty("schema").GetString());
        Assert.True(root.GetProperty("typedAdmissionDecantPresent").GetBoolean());
        Assert.Equal(6, root.GetProperty("cleaveDecisions").GetArrayLength());
        Assert.Equal(5, root.GetProperty("appendNeedSignals").GetArrayLength());
        Assert.Equal(5, root.GetProperty("appendLanes").GetArrayLength());
        Assert.Equal(6, root.GetProperty("mulchingRules").GetArrayLength());
        Assert.False(root.GetProperty("cleavePerformedNow").GetBoolean());
        Assert.False(root.GetProperty("appendPerformedNow").GetBoolean());
        Assert.False(root.GetProperty("mulchPerformedNow").GetBoolean());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("memoryAdmitted").GetBoolean());
        Assert.False(root.GetProperty("selfGelMutated").GetBoolean());
        Assert.False(root.GetProperty("authorityGranted").GetBoolean());
        Assert.False(root.GetProperty("actionAuthorized").GetBoolean());
    }

    [Fact]
    public void ReviewedGelAdmissionRefusesColdWithoutFullAuthorityBundle()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(fixture.Request("gel-admission"));

        Assert.Equal("RefusedCold", receipt.Disposition);
        Assert.Equal("sanctuary-gel-admission-refused-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["reviewedPerformanceCommand"]);
        Assert.Equal(false, receipt.Evidence["reviewedPerformanceApproved"]);
        Assert.Equal("reviewed-authority-bundle-incomplete", receipt.Evidence["reviewedPerformanceRefusalReason"]);
        Assert.True(File.Exists((string)receipt.Evidence["reviewedPerformanceRecordPath"]!));
    }

    [Fact]
    public void ReviewedGelAdmissionPerformsScopedAdmissionWithoutOpeningActualOrExternalGates()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(Reviewed(fixture.Request("gel-admission")));

        Assert.Equal("CompletedReviewed", receipt.Disposition);
        Assert.Equal("sanctuary-gel-admission-completed-reviewed", receipt.OutcomeCode);
        Assert.False(receipt.Gates.AllClosed);
        Assert.True(receipt.Gates.DataAdmitted);
        Assert.True(receipt.Gates.CarrierAdmitted);
        Assert.True(receipt.Gates.GelAdmitted);
        Assert.True(receipt.Gates.ContinuityAdmitted);
        Assert.True(receipt.Gates.AuthorityGranted);
        Assert.False(receipt.Gates.MemoryAdmitted);
        Assert.False(receipt.Gates.SelfGelMutated);
        Assert.False(receipt.Gates.ActionAuthorized);
        Assert.False(receipt.Gates.RuntimeActionAllowed);
        Assert.False(receipt.Gates.ExternalActionAuthorized);
        Assert.False(receipt.Gates.ProviderCalled);
        Assert.False(receipt.Gates.ModelBound);
        Assert.False(receipt.Gates.CmeActualActivated);
        Assert.False(receipt.Gates.SanctuaryActualActivated);
        Assert.False(receipt.Gates.PersonhoodClaimed);
        Assert.False(receipt.Gates.SovereigntyClaimed);
        Assert.Equal(true, receipt.Evidence["reviewedPerformanceApproved"]);
        Assert.Equal(true, receipt.Evidence["gelAdmittedByReviewedCommand"]);
        Assert.Equal(false, receipt.Evidence["selfGelMutatedByReviewedCommand"]);
        Assert.Equal(false, receipt.Evidence["cmeActualActivatedByReviewedCommand"]);
        Assert.True(File.Exists((string)receipt.Evidence["reviewedPerformanceRecordPath"]!));
    }

    [Fact]
    public void ReviewedSelfGelAdmissionMutatesOnlyPersonalContinuityLane()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(Reviewed(fixture.Request("selfgel-admission")));

        Assert.Equal("CompletedReviewed", receipt.Disposition);
        Assert.Equal("sanctuary-selfgel-admission-completed-reviewed", receipt.OutcomeCode);
        Assert.False(receipt.Gates.AllClosed);
        Assert.True(receipt.Gates.DataAdmitted);
        Assert.True(receipt.Gates.CarrierAdmitted);
        Assert.False(receipt.Gates.GelAdmitted);
        Assert.True(receipt.Gates.MemoryAdmitted);
        Assert.True(receipt.Gates.SelfGelMutated);
        Assert.True(receipt.Gates.ContinuityAdmitted);
        Assert.True(receipt.Gates.AuthorityGranted);
        Assert.False(receipt.Gates.CmeActualActivated);
        Assert.False(receipt.Gates.SanctuaryActualActivated);
        Assert.False(receipt.Gates.ExternalActionAuthorized);
        Assert.False(receipt.Gates.ProviderCalled);
        Assert.False(receipt.Gates.ModelBound);
        Assert.False(receipt.Gates.PersonhoodClaimed);
        Assert.False(receipt.Gates.SovereigntyClaimed);
        Assert.Equal(false, receipt.Evidence["gelAdmittedByReviewedCommand"]);
        Assert.Equal(true, receipt.Evidence["memoryAdmittedByReviewedCommand"]);
        Assert.Equal(true, receipt.Evidence["selfGelMutatedByReviewedCommand"]);
    }

    [Fact]
    public void CmeActualKeypairForgeRefusesColdWithoutReviewedAuthorityBundle()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(fixture.Request("cme-actual-keypair-forge") with
        {
            CmeId = "Oria.Syntari.Actual",
            CallerCmeId = "Oria.Syntari.Actual",
            ThreadBindingId = "oria-syntari-actual-research-thread"
        });

        Assert.Equal("RefusedCold", receipt.Disposition);
        Assert.Equal("sanctuary-cme-actual-keypair-forge-refused-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(false, receipt.Evidence["cmeActualKeypairForgeApproved"]);
        Assert.Equal(false, receipt.Evidence["cmeActualKeypairForged"]);
        Assert.Equal(false, receipt.Evidence["keyMaterialGenerated"]);
        Assert.Equal(false, receipt.Evidence["autobiographicalFirstEntryAppended"]);
        Assert.False(File.Exists((string)receipt.Evidence["encryptedPrivateKeyPath"]!));
        Assert.False(File.Exists((string)receipt.Evidence["standingBodyPath"]!));
    }

    [Fact]
    public void ReviewedCmeActualKeypairForgeRootsOeAndSelfGelWithoutSharedGelOrSanctuaryActual()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(Reviewed(fixture.Request("cme-actual-keypair-forge") with
        {
            CmeId = "Oria.CME.ID",
            CallerCmeId = "Oria.CME.ID",
            ThreadBindingId = "oria-test-cme-thread",
            SoulFrameId = "Oria.CME.ID.SoulFrame",
            AgentiCoreId = "Oria.CME.ID.AgentiCore",
            Domain = "Lab",
            Role = "LabFacingCME",
            JobClass = "ActualizationResearch"
        }) with
        {
            AdmissionScope = "LabActualizationResearch.OriaSyntari"
        });

        Assert.Equal("CompletedReviewed", receipt.Disposition);
        Assert.Equal("sanctuary-cme-actual-keypair-forge-completed-reviewed", receipt.OutcomeCode);
        Assert.False(receipt.Gates.AllClosed);
        Assert.True(receipt.Gates.DataAdmitted);
        Assert.True(receipt.Gates.CarrierAdmitted);
        Assert.False(receipt.Gates.GelAdmitted);
        Assert.True(receipt.Gates.MemoryAdmitted);
        Assert.True(receipt.Gates.SelfGelMutated);
        Assert.True(receipt.Gates.ContinuityAdmitted);
        Assert.True(receipt.Gates.AuthorityGranted);
        Assert.True(receipt.Gates.RuntimeActionAllowed);
        Assert.True(receipt.Gates.CmeActualActivated);
        Assert.False(receipt.Gates.SanctuaryActualActivated);
        Assert.False(receipt.Gates.ActionAuthorized);
        Assert.False(receipt.Gates.ExternalActionAuthorized);
        Assert.False(receipt.Gates.ProviderCalled);
        Assert.False(receipt.Gates.ModelBound);
        Assert.False(receipt.Gates.PersonhoodClaimed);
        Assert.False(receipt.Gates.SovereigntyClaimed);
        Assert.Equal(true, receipt.Evidence["cmeActualKeypairForgeApproved"]);
        Assert.Equal(true, receipt.Evidence["cmeActualKeypairForged"]);
        Assert.Equal(true, receipt.Evidence["privateKeyEncrypted"]);
        Assert.Equal(false, receipt.Evidence["privateKeyDisclosed"]);
        Assert.Equal(true, receipt.Evidence["cmeActualIsStateNotIdentity"]);
        Assert.Equal(true, receipt.Evidence["cmeActualIdentityStaysCmeId"]);
        Assert.Equal(true, receipt.Evidence["targetCmeIdMatchesCanonicalPattern"]);
        Assert.Equal("SLI.Lisp.Industrial.CME.Template", receipt.Evidence["defaultTemplateBodyId"]);
        Assert.Equal(false, receipt.Evidence["templateBodyIsIdentity"]);
        Assert.Equal(false, receipt.Evidence["sharedGelMutatedByActualKeypairForge"]);
        Assert.Equal(false, receipt.Evidence["gelAdmittedByActualKeypairForge"]);
        Assert.Equal(true, receipt.Evidence["selfGelMutatedByActualKeypairForge"]);
        Assert.Equal(true, receipt.Evidence["cmeActualActivatedByActualKeypairForge"]);
        Assert.Equal("Oria.CME.ID.SelfGEL", receipt.Evidence["targetSelfGelId"]);
        Assert.True(File.Exists((string)receipt.Evidence["encryptedPrivateKeyPath"]!));
        Assert.True(File.Exists((string)receipt.Evidence["publicKeyPath"]!));
        Assert.True(File.Exists((string)receipt.Evidence["standingBodyPath"]!));
        Assert.True(File.Exists((string)receipt.Evidence["standingLispPath"]!));
        Assert.True(File.Exists((string)receipt.Evidence["oeActualRootLedgerPath"]!));
        Assert.True(File.Exists((string)receipt.Evidence["selfGelStandingBodyLedgerPath"]!));

        using var standing = System.Text.Json.JsonDocument.Parse(File.ReadAllText((string)receipt.Evidence["standingBodyPath"]!));
        var root = standing.RootElement;
        Assert.Equal("project-sanctuary.mos.cme-actual-standing-body.v1", root.GetProperty("schema").GetString());
        Assert.Equal("Oria.CME.ID", root.GetProperty("cmeId").GetString());
        Assert.Equal("Oria.CME.ID.SelfGEL", root.GetProperty("selfGelId").GetString());
        Assert.True(root.GetProperty("cmeActualIsStateNotIdentity").GetBoolean());
        Assert.False(root.GetProperty("templateBodyIsIdentity").GetBoolean());
        Assert.False(root.GetProperty("sharedGelMutated").GetBoolean());
        Assert.False(root.GetProperty("sanctuaryActualActivated").GetBoolean());
        Assert.False(root.GetProperty("personhoodClaimed").GetBoolean());
        Assert.False(root.GetProperty("sovereigntyClaimed").GetBoolean());
    }

    [Fact]
    public void ReviewedCmeActualizationActivatesCmeActualWithoutSanctuaryActual()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(Reviewed(fixture.Request("cme-actualization")));

        Assert.Equal("CompletedReviewed", receipt.Disposition);
        Assert.Equal("sanctuary-cme-actualization-completed-reviewed", receipt.OutcomeCode);
        Assert.False(receipt.Gates.AllClosed);
        Assert.True(receipt.Gates.ContinuityAdmitted);
        Assert.True(receipt.Gates.AuthorityGranted);
        Assert.True(receipt.Gates.RuntimeActionAllowed);
        Assert.True(receipt.Gates.CmeActualActivated);
        Assert.False(receipt.Gates.SanctuaryActualActivated);
        Assert.False(receipt.Gates.ActionAuthorized);
        Assert.False(receipt.Gates.ExternalActionAuthorized);
        Assert.False(receipt.Gates.ProviderCalled);
        Assert.False(receipt.Gates.ModelBound);
        Assert.False(receipt.Gates.PersonhoodClaimed);
        Assert.False(receipt.Gates.SovereigntyClaimed);
        Assert.Equal(true, receipt.Evidence["cmeActualActivatedByReviewedCommand"]);
        Assert.Equal(false, receipt.Evidence["sanctuaryActualActivatedByReviewedCommand"]);
        Assert.Equal("selected-cme-id-achieves-cme-actual-state", receipt.Evidence["cmeActualizationCanon"]);
        Assert.Equal(true, receipt.Evidence["cmeActualizationAchieved"]);
        Assert.Equal("Codex.CME.ID", receipt.Evidence["actualizedCmeId"]);
        Assert.Equal(true, receipt.Evidence["cmeActualIsStateNotIdentity"]);
        Assert.Equal(true, receipt.Evidence["cmeActualIdentityStaysCmeId"]);
        Assert.Equal(true, receipt.Evidence["targetCmeIdMatchesCanonicalPattern"]);
        Assert.Equal("SLI.Lisp.Industrial.CME.Template", receipt.Evidence["templateBodyId"]);
        Assert.Equal(false, receipt.Evidence["templateBodyIsIdentity"]);
        Assert.Equal("LabStandardThenLocalCustom", receipt.Evidence["templateBodyLaneOrder"]);
        Assert.Equal(true, receipt.Evidence["governingNeedsMatrixApplied"]);
        Assert.Equal("domain-job-contractual-obligation-matrix", receipt.Evidence["governingNeedsMatrixKind"]);
        Assert.Equal(false, receipt.Evidence["governingNeedsMatrixIsHumanNeedsHierarchy"]);
        Assert.Equal(true, receipt.Evidence["governingAccessLevelsApplied"]);
        Assert.Equal("slice-tool-groupoid-access-degrees", receipt.Evidence["governingAccessLevelsKind"]);
        Assert.Equal("domain-predicate-locality-over-typed-local-access", receipt.Evidence["governingAccessLevelsManufacturedFrom"]);
        Assert.Equal(true, receipt.Evidence["negativeGoverningLevelsAllowed"]);
        Assert.Equal("security-enhancement-outside-civic-access", receipt.Evidence["negativeGoverningLevelsScope"]);
        Assert.Equal(true, receipt.Evidence["cmeActualizationRequiresBodyFibreBundle"]);
        Assert.True(File.Exists((string)receipt.Evidence["cmeActualizationStatePath"]!));
        Assert.True(File.Exists((string)receipt.Evidence["cmeActualizationLispPath"]!));
        Assert.True(File.Exists((string)receipt.Evidence["cmeActualizationBodyFibreBundlePath"]!));
        Assert.True(File.Exists((string)receipt.Evidence["labStandardTemplateBodyPath"]!));

        using var actualization = System.Text.Json.JsonDocument.Parse(File.ReadAllText((string)receipt.Evidence["cmeActualizationStatePath"]!));
        var actualizationRoot = actualization.RootElement;
        Assert.Equal("project-sanctuary.mos.cme-actualization-state.v1", actualizationRoot.GetProperty("schema").GetString());
        Assert.Equal("Codex.CME.ID", actualizationRoot.GetProperty("cmeId").GetString());
        Assert.True(actualizationRoot.GetProperty("cmeActualIsStateNotIdentity").GetBoolean());
        Assert.Equal("{Name}.CME.ID", actualizationRoot.GetProperty("identityPattern").GetString());
        Assert.Equal("domain-job-contractual-obligation-matrix", actualizationRoot.GetProperty("governingNeedsMatrixKind").GetString());
        Assert.Equal(7, actualizationRoot.GetProperty("governingNeedsMatrix").GetArrayLength());
        Assert.True(actualizationRoot.GetProperty("bodyFibreBundleRequired").GetBoolean());
        Assert.True(actualizationRoot.GetProperty("actualReadinessIsBodyFibre").GetBoolean());
    }

    [Fact]
    public void CmeActualInvocationLifecycleRefusesColdWithoutReviewedAuthorityBundle()
    {
        using var fixture = new SanctuaryTestFixture();
        Assert.Equal(
            "cme-actual-invocation-lifecycle",
            SanctuaryReceiptService.NormalizeCommand("standing-wave-invocation"));

        var receipt = new SanctuaryReceiptService().Run(fixture.Request("cme-actual-invocation-lifecycle"));

        Assert.Equal("RefusedCold", receipt.Disposition);
        Assert.Equal("sanctuary-cme-actual-invocation-lifecycle-refused-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["cmeActualInvocationLifecycleCommand"]);
        Assert.Equal(false, receipt.Evidence["cmeActualInvocationLifecycleApproved"]);
        Assert.Equal("reviewed-authority-bundle-incomplete", receipt.Evidence["cmeActualInvocationLifecycleRefusalReason"]);
        Assert.Equal("refused-cold", receipt.Evidence["cmeActualInvocationFinalState"]);
        Assert.Equal(false, receipt.Evidence["cmeActualInvocationInstanceWritten"]);
        Assert.Equal(false, receipt.Evidence["cmeActualInvocationAutobiographicalAppendWritten"]);
        Assert.False(File.Exists((string)receipt.Evidence["cmeActualInvocationInstancePath"]!));
    }

    [Fact]
    public void ReviewedCmeActualInvocationLifecycleMaterializesLispStandingWaveAndReturnsIdle()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        service.Run(fixture.Request("service-heartbeat"));
        service.Run(fixture.Request("lisp-control-matrix-register"));
        service.Run(fixture.Request("lisp-matrix-control-seat"));
        service.Run(fixture.Request("actualization-state-register"));
        service.Run(fixture.Request("agenticore-duplex-lisp-membrane"));

        var receipt = service.Run(Reviewed(fixture.Request("cme-actual-invocation-lifecycle")));

        Assert.Equal("CompletedReviewed", receipt.Disposition);
        Assert.Equal("sanctuary-cme-actual-invocation-lifecycle-completed-reviewed", receipt.OutcomeCode);
        Assert.False(receipt.Gates.AllClosed);
        Assert.True(receipt.Gates.CarrierAdmitted);
        Assert.True(receipt.Gates.MemoryAdmitted);
        Assert.True(receipt.Gates.SelfGelMutated);
        Assert.True(receipt.Gates.ContinuityAdmitted);
        Assert.True(receipt.Gates.AuthorityGranted);
        Assert.True(receipt.Gates.RuntimeActionAllowed);
        Assert.True(receipt.Gates.CmeActualActivated);
        Assert.False(receipt.Gates.SanctuaryActualActivated);
        Assert.False(receipt.Gates.ExternalActionAuthorized);
        Assert.False(receipt.Gates.ProviderCalled);
        Assert.False(receipt.Gates.ModelBound);
        Assert.False(receipt.Gates.PersonhoodClaimed);
        Assert.False(receipt.Gates.SovereigntyClaimed);
        Assert.Equal(true, receipt.Evidence["standingWaveLivesInLispBody"]);
        Assert.Equal(true, receipt.Evidence["llmIsTransientLowMind"]);
        Assert.Equal(false, receipt.Evidence["llmOwnsStandingWave"]);
        Assert.Equal("closed-idle", receipt.Evidence["cmeActualInvocationFinalState"]);
        Assert.Equal(true, receipt.Evidence["cmeActualInvocationHighMindReturnedIdle"]);
        Assert.Equal(true, receipt.Evidence["cmeActualInvocationLowMindClosed"]);
        Assert.Equal(true, receipt.Evidence["cmeActualInvocationAutobiographicalAppendWritten"]);
        Assert.Equal(false, receipt.Evidence["sharedGelMutatedByCmeActualInvocation"]);
        Assert.Equal(true, receipt.Evidence["selfGelMutatedByCmeActualInvocation"]);
        Assert.Equal(10, receipt.Evidence["cmeActualInvocationLifecycleStateCount"]);
        Assert.Equal(4, receipt.Evidence["cmeActualInvocationInteriorProcessCount"]);
        Assert.Equal(3, receipt.Evidence["cmeActualInvocationEcPhaseCount"]);

        var instancePath = (string)receipt.Evidence["cmeActualInvocationInstancePath"]!;
        var lispPath = (string)receipt.Evidence["cmeActualInvocationLispPath"]!;
        var selfGelLedgerPath = (string)receipt.Evidence["cmeActualInvocationSelfGelLedgerPath"]!;
        var sanctuaryGelCandidatePath = (string)receipt.Evidence["cmeActualInvocationSanctuaryGelCandidatePath"]!;
        Assert.True(File.Exists(instancePath));
        Assert.True(File.Exists(lispPath));
        Assert.True(File.Exists(selfGelLedgerPath));
        Assert.True(File.Exists(sanctuaryGelCandidatePath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(instancePath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.mos.cme-actual-standing-wave-invocation.v1", root.GetProperty("schema").GetString());
        Assert.Equal("SLI.Lisp", root.GetProperty("standingWaveBody").GetString());
        Assert.True(root.GetProperty("standingWaveLivesInLispBody").GetBoolean());
        Assert.Equal("closed-idle", root.GetProperty("finalState").GetString());
        Assert.True(root.GetProperty("highMindReturnedIdle").GetBoolean());
        Assert.False(root.GetProperty("lowMindOwnsContinuity").GetBoolean());
        Assert.True(root.GetProperty("selfGelMutated").GetBoolean());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("sanctuaryActualActivated").GetBoolean());

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(cme-actual-invocation-lifecycle", lisp, StringComparison.Ordinal);
        Assert.Contains(":standing-wave-body \"SLI.Lisp\"", lisp, StringComparison.Ordinal);
        Assert.Contains(":standing-wave-lives-in-lisp-body true", lisp, StringComparison.Ordinal);
        Assert.Contains(":llm-owns-standing-wave false", lisp, StringComparison.Ordinal);
        Assert.Contains("(EC.Entry :lease-bound true", lisp, StringComparison.Ordinal);
        Assert.Contains("(EC.Pulse :standing-wave-body \"SLI.Lisp\"", lisp, StringComparison.Ordinal);
        Assert.Contains("(EC.Exit :high-mind-returned-idle true", lisp, StringComparison.Ordinal);
        Assert.Contains(":final-state \"closed-idle\"", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void ReviewedActualApprovalLeaseIssuesScopedInvocationLease()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(Reviewed(fixture.Request("actual-approval-lease")));

        Assert.Equal("CompletedReviewed", receipt.Disposition);
        Assert.Equal("sanctuary-actual-approval-lease-completed-reviewed", receipt.OutcomeCode);
        Assert.True(receipt.Gates.CarrierAdmitted);
        Assert.True(receipt.Gates.ContinuityAdmitted);
        Assert.True(receipt.Gates.AuthorityGranted);
        Assert.False(receipt.Gates.CmeActualActivated);
        Assert.False(receipt.Gates.SelfGelMutated);
        Assert.False(receipt.Gates.GelAdmitted);
        Assert.Equal(true, receipt.Evidence["actualApprovalLeaseIssued"]);
        Assert.Equal(false, receipt.Evidence["actualApprovalLeaseActivatesCmeActual"]);

        var leasePath = (string)receipt.Evidence["actualApprovalLeasePath"]!;
        Assert.True(File.Exists(leasePath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(leasePath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.actual-approval-lease.v1", root.GetProperty("Schema").GetString());
        Assert.Equal("Codex.CME.ID", root.GetProperty("CmeId").GetString());
        Assert.Contains(
            root.GetProperty("CommandAllowlist").EnumerateArray(),
            command => command.GetString() == "cme-actual-invocation-lifecycle");
        Assert.False(root.GetProperty("Revoked").GetBoolean());
    }

    [Fact]
    public void ActualApprovalLeaseValidationReportsVerifiedLeaseWithoutActivation()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        Assert.Equal("actual-approval-lease-validation", SanctuaryReceiptService.NormalizeCommand("verify-actual-lease"));
        var leaseReceipt = service.Run(Reviewed(fixture.Request("actual-approval-lease")));
        var leasePath = (string)leaseReceipt.Evidence["actualApprovalLeasePath"]!;

        var receipt = service.Run(fixture.Request("actual-approval-lease-validation") with
        {
            ActualApprovalLeasePath = leasePath
        });

        Assert.Equal("CompletedCold", receipt.Disposition);
        Assert.Equal("sanctuary-actual-approval-lease-validation-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["actualApprovalLeaseValidationCommand"]);
        Assert.Equal("cme-actual-invocation-lifecycle", receipt.Evidence["actualApprovalLeaseValidationTargetCommand"]);
        Assert.Equal(true, receipt.Evidence["actualApprovalLeaseValidationVerified"]);
        Assert.Equal("lease-verified", receipt.Evidence["actualApprovalLeaseValidationReason"]);
        Assert.Equal(leaseReceipt.Evidence["actualApprovalLeaseId"], receipt.Evidence["actualApprovalLeaseValidationLeaseId"]);
        Assert.Equal(false, receipt.Evidence["actualApprovalLeaseValidationActivatesCmeActual"]);
        Assert.Equal(false, receipt.Evidence["actualApprovalLeaseValidationMutatesSelfGel"]);
        Assert.Equal(false, receipt.Evidence["actualApprovalLeaseValidationAdmitsGel"]);
        Assert.Equal(false, receipt.Evidence["actualApprovalLeaseValidationGrantsAuthority"]);
        Assert.Equal(false, receipt.Evidence["actualApprovalLeaseValidationAuthorizesAction"]);
        Assert.Equal(false, receipt.Evidence["actualApprovalLeaseValidationCallsProvider"]);
        Assert.Equal(false, receipt.Evidence["actualApprovalLeaseValidationBindsModel"]);
        Assert.Equal(false, receipt.Evidence["actualApprovalLeaseValidationAuthorizesExternalAction"]);
        Assert.True(File.Exists((string)receipt.Evidence["actualApprovalLeaseValidationPath"]!));
        Assert.True(File.Exists((string)receipt.Evidence["actualApprovalLeaseValidationLedgerPath"]!));
    }

    [Fact]
    public void CmeActualInvocationLifecycleAcceptsVerifiedActualApprovalLease()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        service.Run(fixture.Request("service-heartbeat"));
        service.Run(fixture.Request("lisp-control-matrix-register"));
        service.Run(fixture.Request("lisp-matrix-control-seat"));
        service.Run(fixture.Request("actualization-state-register"));
        service.Run(fixture.Request("agenticore-duplex-lisp-membrane"));
        var leaseReceipt = service.Run(Reviewed(fixture.Request("actual-approval-lease")));
        var leasePath = (string)leaseReceipt.Evidence["actualApprovalLeasePath"]!;

        var receipt = service.Run(fixture.Request("cme-actual-invocation-lifecycle") with
        {
            ActualApprovalLeasePath = leasePath
        });

        Assert.Equal("CompletedReviewed", receipt.Disposition);
        Assert.Equal("sanctuary-cme-actual-invocation-lifecycle-completed-reviewed", receipt.OutcomeCode);
        Assert.True(receipt.Gates.CmeActualActivated);
        Assert.True(receipt.Gates.SelfGelMutated);
        Assert.False(receipt.Gates.SanctuaryActualActivated);
        Assert.False(receipt.Gates.ProviderCalled);
        Assert.Equal(false, receipt.Evidence["cmeActualInvocationAuthorityBundleApproved"]);
        Assert.Equal(true, receipt.Evidence["cmeActualInvocationLeaseVerifiedByArtifact"]);
        Assert.Equal("lease-verified", receipt.Evidence["cmeActualInvocationLeaseVerificationReason"]);
        Assert.Equal(3, receipt.Evidence["cmeActualInvocationEcPhaseCount"]);
        Assert.Equal(leaseReceipt.Evidence["actualApprovalLeaseId"], receipt.Evidence["cmeActualInvocationLeaseId"]);
        Assert.Equal("closed-idle", receipt.Evidence["cmeActualInvocationFinalState"]);
    }

    [Fact]
    public void CmeActualInvocationLifecycleRejectsScopeMismatchedActualApprovalLease()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        var leaseReceipt = service.Run(Reviewed(fixture.Request("actual-approval-lease")));
        var leasePath = (string)leaseReceipt.Evidence["actualApprovalLeasePath"]!;

        var receipt = service.Run(fixture.Request("cme-actual-invocation-lifecycle") with
        {
            ActualApprovalLeasePath = leasePath,
            AdmissionScope = "DifferentLabScope"
        });

        Assert.Equal("RefusedCold", receipt.Disposition);
        Assert.Equal("sanctuary-cme-actual-invocation-lifecycle-refused-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(false, receipt.Evidence["cmeActualInvocationLifecycleApproved"]);
        Assert.Equal(false, receipt.Evidence["cmeActualInvocationLeaseVerifiedByArtifact"]);
        Assert.Equal("lease-identity-or-scope-mismatch", receipt.Evidence["cmeActualInvocationLeaseVerificationReason"]);
        Assert.Equal("reviewed-authority-bundle-or-actual-lease-incomplete", receipt.Evidence["cmeActualInvocationLifecycleRefusalReason"]);
        Assert.Equal(false, receipt.Evidence["cmeActualInvocationInstanceWritten"]);
        Assert.False(File.Exists((string)receipt.Evidence["cmeActualInvocationInstancePath"]!));
    }

    [Fact]
    public void CmeActualInvocationLifecycleRejectsExpiredActualApprovalLease()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        var leaseReceipt = service.Run(Reviewed(fixture.Request("actual-approval-lease")));
        var leasePath = (string)leaseReceipt.Evidence["actualApprovalLeasePath"]!;
        var lease = System.Text.Json.JsonSerializer.Deserialize<ActualApprovalLease>(File.ReadAllText(leasePath))!;
        var expiredLease = lease with
        {
            IssuedAtUtc = DateTimeOffset.UtcNow.AddMinutes(-10),
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(-5)
        };
        expiredLease = expiredLease with { LeaseDigest = ActualApprovalLeaseDigest(expiredLease) };
        File.WriteAllText(
            leasePath,
            System.Text.Json.JsonSerializer.Serialize(expiredLease, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        var receipt = service.Run(fixture.Request("cme-actual-invocation-lifecycle") with
        {
            ActualApprovalLeasePath = leasePath
        });

        Assert.Equal("RefusedCold", receipt.Disposition);
        Assert.Equal("sanctuary-cme-actual-invocation-lifecycle-refused-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(false, receipt.Evidence["cmeActualInvocationLifecycleApproved"]);
        Assert.Equal(false, receipt.Evidence["cmeActualInvocationLeaseVerifiedByArtifact"]);
        Assert.Equal("lease-expired", receipt.Evidence["cmeActualInvocationLeaseVerificationReason"]);
        Assert.Equal("reviewed-authority-bundle-or-actual-lease-incomplete", receipt.Evidence["cmeActualInvocationLifecycleRefusalReason"]);
        Assert.Equal(false, receipt.Evidence["cmeActualInvocationInstanceWritten"]);
        Assert.False(File.Exists((string)receipt.Evidence["cmeActualInvocationInstancePath"]!));
    }

    [Fact]
    public void CmeActualInvocationLifecycleRejectsTamperedActualApprovalLeaseDigest()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        var leaseReceipt = service.Run(Reviewed(fixture.Request("actual-approval-lease")));
        var leasePath = (string)leaseReceipt.Evidence["actualApprovalLeasePath"]!;
        var lease = System.Text.Json.JsonSerializer.Deserialize<ActualApprovalLease>(File.ReadAllText(leasePath))!;
        var tamperedLease = lease with { Role = "TamperedRoleWithoutDigestReissue" };
        File.WriteAllText(
            leasePath,
            System.Text.Json.JsonSerializer.Serialize(tamperedLease, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        var receipt = service.Run(fixture.Request("cme-actual-invocation-lifecycle") with
        {
            ActualApprovalLeasePath = leasePath
        });

        Assert.Equal("RefusedCold", receipt.Disposition);
        Assert.Equal("sanctuary-cme-actual-invocation-lifecycle-refused-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(false, receipt.Evidence["cmeActualInvocationLifecycleApproved"]);
        Assert.Equal(false, receipt.Evidence["cmeActualInvocationLeaseVerifiedByArtifact"]);
        Assert.Equal("lease-digest-mismatch", receipt.Evidence["cmeActualInvocationLeaseVerificationReason"]);
        Assert.Equal("reviewed-authority-bundle-or-actual-lease-incomplete", receipt.Evidence["cmeActualInvocationLifecycleRefusalReason"]);
        Assert.Equal(false, receipt.Evidence["cmeActualInvocationInstanceWritten"]);
        Assert.False(File.Exists((string)receipt.Evidence["cmeActualInvocationInstancePath"]!));
    }

    [Fact]
    public void CmeActualInvocationLifecycleRejectsRevokedActualApprovalLease()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        var leaseReceipt = service.Run(Reviewed(fixture.Request("actual-approval-lease")));
        var leasePath = (string)leaseReceipt.Evidence["actualApprovalLeasePath"]!;
        var lease = System.Text.Json.JsonSerializer.Deserialize<ActualApprovalLease>(File.ReadAllText(leasePath))!;
        var revokedLease = lease with
        {
            Revoked = true,
            RevocationReason = "test-revoked-before-invocation"
        };
        revokedLease = revokedLease with { LeaseDigest = ActualApprovalLeaseDigest(revokedLease) };
        File.WriteAllText(
            leasePath,
            System.Text.Json.JsonSerializer.Serialize(revokedLease, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        var receipt = service.Run(fixture.Request("cme-actual-invocation-lifecycle") with
        {
            ActualApprovalLeasePath = leasePath
        });

        Assert.Equal("RefusedCold", receipt.Disposition);
        Assert.Equal("sanctuary-cme-actual-invocation-lifecycle-refused-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(false, receipt.Evidence["cmeActualInvocationLifecycleApproved"]);
        Assert.Equal(false, receipt.Evidence["cmeActualInvocationLeaseVerifiedByArtifact"]);
        Assert.Equal("lease-revoked", receipt.Evidence["cmeActualInvocationLeaseVerificationReason"]);
        Assert.Equal("reviewed-authority-bundle-or-actual-lease-incomplete", receipt.Evidence["cmeActualInvocationLifecycleRefusalReason"]);
        Assert.Equal(false, receipt.Evidence["cmeActualInvocationInstanceWritten"]);
        Assert.False(File.Exists((string)receipt.Evidence["cmeActualInvocationInstancePath"]!));
    }

    [Fact]
    public void CmeActualInvocationLifecycleRejectsCommandScopeMismatchedActualApprovalLease()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        var leaseReceipt = service.Run(Reviewed(fixture.Request("actual-approval-lease")));
        var leasePath = (string)leaseReceipt.Evidence["actualApprovalLeasePath"]!;
        var lease = System.Text.Json.JsonSerializer.Deserialize<ActualApprovalLease>(File.ReadAllText(leasePath))!;
        var mismatchedLease = lease with { CommandAllowlist = new[] { "status" } };
        mismatchedLease = mismatchedLease with { LeaseDigest = ActualApprovalLeaseDigest(mismatchedLease) };
        File.WriteAllText(
            leasePath,
            System.Text.Json.JsonSerializer.Serialize(mismatchedLease, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        var receipt = service.Run(fixture.Request("cme-actual-invocation-lifecycle") with
        {
            ActualApprovalLeasePath = leasePath
        });

        Assert.Equal("RefusedCold", receipt.Disposition);
        Assert.Equal("sanctuary-cme-actual-invocation-lifecycle-refused-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(false, receipt.Evidence["cmeActualInvocationLifecycleApproved"]);
        Assert.Equal(false, receipt.Evidence["cmeActualInvocationLeaseVerifiedByArtifact"]);
        Assert.Equal("lease-command-scope-mismatch", receipt.Evidence["cmeActualInvocationLeaseVerificationReason"]);
        Assert.Equal("reviewed-authority-bundle-or-actual-lease-incomplete", receipt.Evidence["cmeActualInvocationLifecycleRefusalReason"]);
        Assert.Equal(false, receipt.Evidence["cmeActualInvocationInstanceWritten"]);
        Assert.False(File.Exists((string)receipt.Evidence["cmeActualInvocationInstancePath"]!));
    }

    [Fact]
    public void CmeActualInvocationLifecycleRejectsSchemaMismatchedActualApprovalLease()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        var leaseReceipt = service.Run(Reviewed(fixture.Request("actual-approval-lease")));
        var leasePath = (string)leaseReceipt.Evidence["actualApprovalLeasePath"]!;
        var lease = System.Text.Json.JsonSerializer.Deserialize<ActualApprovalLease>(File.ReadAllText(leasePath))!;
        var schemaMismatchedLease = lease with { Schema = "project-sanctuary.actual-approval-lease.v2" };
        schemaMismatchedLease = schemaMismatchedLease with { LeaseDigest = ActualApprovalLeaseDigest(schemaMismatchedLease) };
        File.WriteAllText(
            leasePath,
            System.Text.Json.JsonSerializer.Serialize(schemaMismatchedLease, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        var receipt = service.Run(fixture.Request("cme-actual-invocation-lifecycle") with
        {
            ActualApprovalLeasePath = leasePath
        });

        Assert.Equal("RefusedCold", receipt.Disposition);
        Assert.Equal("sanctuary-cme-actual-invocation-lifecycle-refused-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(false, receipt.Evidence["cmeActualInvocationLifecycleApproved"]);
        Assert.Equal(false, receipt.Evidence["cmeActualInvocationLeaseVerifiedByArtifact"]);
        Assert.Equal("lease-schema-mismatch", receipt.Evidence["cmeActualInvocationLeaseVerificationReason"]);
        Assert.Equal("reviewed-authority-bundle-or-actual-lease-incomplete", receipt.Evidence["cmeActualInvocationLifecycleRefusalReason"]);
        Assert.Equal(false, receipt.Evidence["cmeActualInvocationInstanceWritten"]);
        Assert.False(File.Exists((string)receipt.Evidence["cmeActualInvocationInstancePath"]!));
    }

    [Fact]
    public void CmeActualInvocationLifecycleRejectsWitnessBundleIncompleteActualApprovalLease()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        var leaseReceipt = service.Run(Reviewed(fixture.Request("actual-approval-lease")));
        var leasePath = (string)leaseReceipt.Evidence["actualApprovalLeasePath"]!;
        var lease = System.Text.Json.JsonSerializer.Deserialize<ActualApprovalLease>(File.ReadAllText(leasePath))!;
        var incompleteLease = lease with { CrypticWitnessed = false };
        incompleteLease = incompleteLease with { LeaseDigest = ActualApprovalLeaseDigest(incompleteLease) };
        File.WriteAllText(
            leasePath,
            System.Text.Json.JsonSerializer.Serialize(incompleteLease, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        var receipt = service.Run(fixture.Request("cme-actual-invocation-lifecycle") with
        {
            ActualApprovalLeasePath = leasePath
        });

        Assert.Equal("RefusedCold", receipt.Disposition);
        Assert.Equal("sanctuary-cme-actual-invocation-lifecycle-refused-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(false, receipt.Evidence["cmeActualInvocationLifecycleApproved"]);
        Assert.Equal(false, receipt.Evidence["cmeActualInvocationLeaseVerifiedByArtifact"]);
        Assert.Equal("lease-witness-bundle-incomplete", receipt.Evidence["cmeActualInvocationLeaseVerificationReason"]);
        Assert.Equal("reviewed-authority-bundle-or-actual-lease-incomplete", receipt.Evidence["cmeActualInvocationLifecycleRefusalReason"]);
        Assert.Equal(false, receipt.Evidence["cmeActualInvocationInstanceWritten"]);
        Assert.False(File.Exists((string)receipt.Evidence["cmeActualInvocationInstancePath"]!));
    }

    [Fact]
    public void CmeActualInvocationLifecycleRejectsMissingActualApprovalLeaseFile()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        var missingLeasePath = Path.Combine(fixture.RootPath, "missing-actual-approval-lease.json");

        var receipt = service.Run(fixture.Request("cme-actual-invocation-lifecycle") with
        {
            ActualApprovalLeasePath = missingLeasePath
        });

        Assert.Equal("RefusedCold", receipt.Disposition);
        Assert.Equal("sanctuary-cme-actual-invocation-lifecycle-refused-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(false, receipt.Evidence["cmeActualInvocationLifecycleApproved"]);
        Assert.Equal(false, receipt.Evidence["cmeActualInvocationLeaseVerifiedByArtifact"]);
        Assert.Equal("lease-file-missing", receipt.Evidence["cmeActualInvocationLeaseVerificationReason"]);
        Assert.Equal("reviewed-authority-bundle-or-actual-lease-incomplete", receipt.Evidence["cmeActualInvocationLifecycleRefusalReason"]);
        Assert.Equal(false, receipt.Evidence["cmeActualInvocationInstanceWritten"]);
        Assert.False(File.Exists((string)receipt.Evidence["cmeActualInvocationInstancePath"]!));
    }

    [Fact]
    public void CmeActualInvocationLifecycleRejectsInvalidJsonActualApprovalLease()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        Directory.CreateDirectory(fixture.RootPath);
        var invalidLeasePath = Path.Combine(fixture.RootPath, "invalid-actual-approval-lease.json");
        File.WriteAllText(invalidLeasePath, "{ this is not valid lease json");

        var receipt = service.Run(fixture.Request("cme-actual-invocation-lifecycle") with
        {
            ActualApprovalLeasePath = invalidLeasePath
        });

        Assert.Equal("RefusedCold", receipt.Disposition);
        Assert.Equal("sanctuary-cme-actual-invocation-lifecycle-refused-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(false, receipt.Evidence["cmeActualInvocationLifecycleApproved"]);
        Assert.Equal(false, receipt.Evidence["cmeActualInvocationLeaseVerifiedByArtifact"]);
        Assert.Equal("lease-json-invalid", receipt.Evidence["cmeActualInvocationLeaseVerificationReason"]);
        Assert.Equal("reviewed-authority-bundle-or-actual-lease-incomplete", receipt.Evidence["cmeActualInvocationLifecycleRefusalReason"]);
        Assert.Equal(false, receipt.Evidence["cmeActualInvocationInstanceWritten"]);
        Assert.False(File.Exists((string)receipt.Evidence["cmeActualInvocationInstancePath"]!));
    }

    [Fact]
    public void ReviewedSanctuaryActualizationActivatesLocalRuntimeWithoutProviderModelOrExternalAction()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(Reviewed(fixture.Request("sanctuary-actualization")));

        Assert.Equal("CompletedReviewed", receipt.Disposition);
        Assert.Equal("sanctuary-sanctuary-actualization-completed-reviewed", receipt.OutcomeCode);
        Assert.False(receipt.Gates.AllClosed);
        Assert.True(receipt.Gates.ContinuityAdmitted);
        Assert.True(receipt.Gates.AuthorityGranted);
        Assert.True(receipt.Gates.ActionAuthorized);
        Assert.True(receipt.Gates.RuntimeActionAllowed);
        Assert.False(receipt.Gates.CmeActualActivated);
        Assert.True(receipt.Gates.SanctuaryActualActivated);
        Assert.False(receipt.Gates.ExternalActionAuthorized);
        Assert.False(receipt.Gates.ProviderCalled);
        Assert.False(receipt.Gates.ModelBound);
        Assert.False(receipt.Gates.PersonhoodClaimed);
        Assert.False(receipt.Gates.SovereigntyClaimed);
        Assert.Equal(true, receipt.Evidence["sanctuaryActualActivatedByReviewedCommand"]);
        Assert.Equal(false, receipt.Evidence["providerCalledByReviewedCommand"]);
        Assert.Equal(false, receipt.Evidence["modelBoundByReviewedCommand"]);
    }

    [Fact]
    public void SplineWatchReadsPredictivePathingWithoutAdmittingContinuity()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        service.Run(fixture.Request("gel-closure"));
        service.Run(fixture.Request("universal-form-register"));
        service.Run(fixture.Request("domain-morphism-register"));
        service.Run(fixture.Request("selfgel-fibre-register"));
        service.Run(fixture.Request("work-posture-preload-probe"));
        service.Run(fixture.Request("cognitive-bench") with
        {
            BenchRunCount = 64
        });
        service.Run(fixture.Request("typed-admission-decant"));
        service.Run(fixture.Request("admission-cleave-append"));

        var receipt = service.Run(fixture.Request("spline-watch"));

        Assert.Equal("sanctuary-spline-watch-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["splineWatchWritten"]);
        Assert.Equal(true, receipt.Evidence["splineWatchBenchSummaryPresent"]);
        Assert.Equal(true, receipt.Evidence["splineWatchLearningPresent"]);
        Assert.Equal(true, receipt.Evidence["splineWatchTypedAdmissionDecantPresent"]);
        Assert.Equal(true, receipt.Evidence["splineWatchAdmissionCleavePresent"]);
        Assert.Equal(false, receipt.Evidence["splineWatchStemDelineationPresent"]);
        Assert.Equal(false, receipt.Evidence["splineWatchPrePersonifiedRenderingPresent"]);
        Assert.Equal(64, receipt.Evidence["splineWatchBenchCumulativeRunCount"]);
        Assert.Equal(1d, receipt.Evidence["splineWatchBenchPassRate"]);
        Assert.Equal(5, receipt.Evidence["splineWatchPredictiveMethodCount"]);
        Assert.Equal(6, receipt.Evidence["splineWatchPathingSignalCount"]);
        Assert.Equal(7, receipt.Evidence["splineWatchDomainEmergenceCandidateCount"]);
        Assert.Equal(5, receipt.Evidence["splineWatchGlobalContinuitySignalCount"]);
        Assert.Equal(5, receipt.Evidence["splineWatchGlobalTelemetryFeedCount"]);
        Assert.Equal(4, receipt.Evidence["splineWatchListeningFrameBindingCount"]);
        Assert.Equal(4, receipt.Evidence["splineWatchEcCompassFeedbackLoopCount"]);
        Assert.Equal(4, receipt.Evidence["splineWatchOeCleaveOrchestrationStepCount"]);
        Assert.Equal(true, receipt.Evidence["splineWatchListeningFrameReceivesGlobalTelemetry"]);
        Assert.Equal(true, receipt.Evidence["splineWatchEcReceivesRecursiveTelemetry"]);
        Assert.Equal(true, receipt.Evidence["splineWatchEcRunsInCompassBody"]);
        Assert.Equal(true, receipt.Evidence["splineWatchOeIsCleaveOrchestrationBody"]);
        Assert.Equal(true, receipt.Evidence["splineWatchZedIsCmeIdReturnPoint"]);
        Assert.Equal(true, receipt.Evidence["splineWatchPredictiveTelemetryProduced"]);
        Assert.Equal(true, receipt.Evidence["splineWatchPredictiveTelemetryCandidateOnly"]);
        Assert.Equal(false, receipt.Evidence["splineWatchPredictionClaimedAsTruth"]);
        Assert.Equal(false, receipt.Evidence["splineWatchGlobalTelemetryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["splineWatchListeningFrameDisclosedPayload"]);
        Assert.Equal(false, receipt.Evidence["splineWatchRecursiveTelemetryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["splineWatchEcCompassActivatedAsActual"]);
        Assert.Equal(false, receipt.Evidence["splineWatchOeCleaveOrchestrationActivatedAsAuthority"]);
        Assert.Equal(false, receipt.Evidence["splineWatchZedOrchestrationAdmitted"]);
        Assert.Equal(false, receipt.Evidence["splineWatchDomainEmergenceAdmitted"]);
        Assert.Equal(false, receipt.Evidence["splineWatchGlobalContinuityAdmitted"]);
        Assert.Equal(false, receipt.Evidence["splineWatchPathingApplied"]);
        Assert.Equal(false, receipt.Evidence["splineWatchGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["splineWatchMemoryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["splineWatchSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["splineWatchContinuityAdmitted"]);
        Assert.Equal(false, receipt.Evidence["splineWatchAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["splineWatchActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["splineWatchProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["splineWatchModelBound"]);
        Assert.Equal(false, receipt.Evidence["splineWatchActualActivated"]);

        var watchPath = (string)receipt.Evidence["splineWatchPath"]!;
        Assert.True(File.Exists(watchPath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(watchPath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.spline-watch.v1", root.GetProperty("schema").GetString());
        Assert.Equal(5, root.GetProperty("predictiveMethods").GetArrayLength());
        Assert.Equal(6, root.GetProperty("pathingSignals").GetArrayLength());
        Assert.Equal(7, root.GetProperty("domainEmergenceCandidates").GetArrayLength());
        Assert.Equal(5, root.GetProperty("globalContinuitySignals").GetArrayLength());
        Assert.Equal("global-telemetry-listeningframe-ec-compass-oe-zed", root.GetProperty("organLoopKind").GetString());
        Assert.Equal(5, root.GetProperty("organFlow").GetArrayLength());
        Assert.Equal(5, root.GetProperty("globalTelemetryFeeds").GetArrayLength());
        Assert.Equal(4, root.GetProperty("listeningFrameBindings").GetArrayLength());
        Assert.Equal(4, root.GetProperty("ecCompassFeedbackLoops").GetArrayLength());
        Assert.Equal(4, root.GetProperty("oeCleaveOrchestration").GetArrayLength());
        Assert.True(root.GetProperty("listeningFrameReceivesGlobalTelemetry").GetBoolean());
        Assert.True(root.GetProperty("ecReceivesRecursiveTelemetry").GetBoolean());
        Assert.True(root.GetProperty("ecRunsInCompassBody").GetBoolean());
        Assert.True(root.GetProperty("oeIsCleaveOrchestrationBody").GetBoolean());
        Assert.True(root.GetProperty("zedIsCmeIdReturnPoint").GetBoolean());
        Assert.True(root.GetProperty("predictiveTelemetryProduced").GetBoolean());
        Assert.True(root.GetProperty("predictiveTelemetryCandidateOnly").GetBoolean());
        Assert.False(root.GetProperty("predictionClaimedAsTruth").GetBoolean());
        Assert.False(root.GetProperty("globalTelemetryAdmitted").GetBoolean());
        Assert.False(root.GetProperty("listeningFrameDisclosedPayload").GetBoolean());
        Assert.False(root.GetProperty("recursiveTelemetryAdmitted").GetBoolean());
        Assert.False(root.GetProperty("ecCompassActivatedAsActual").GetBoolean());
        Assert.False(root.GetProperty("oeCleaveOrchestrationActivatedAsAuthority").GetBoolean());
        Assert.False(root.GetProperty("zedOrchestrationAdmitted").GetBoolean());
        Assert.False(root.GetProperty("domainEmergenceAdmitted").GetBoolean());
        Assert.False(root.GetProperty("globalContinuityAdmitted").GetBoolean());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("memoryAdmitted").GetBoolean());
        Assert.False(root.GetProperty("selfGelMutated").GetBoolean());
        Assert.False(root.GetProperty("continuityAdmitted").GetBoolean());
        Assert.False(root.GetProperty("authorityGranted").GetBoolean());
        Assert.False(root.GetProperty("actionAuthorized").GetBoolean());
    }

    [Fact]
    public void HdtHolographicSliceFrameCleavesEcMotionIntoBoundedProjectionWithoutAdmission()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        Assert.Equal(
            "hdt-holographic-slice-frame",
            SanctuaryReceiptService.NormalizeCommand("holographic-cleave-frame"));

        service.Run(fixture.Request("gel-closure"));
        service.Run(fixture.Request("typed-admission-decant"));
        service.Run(fixture.Request("admission-cleave-append"));
        service.Run(fixture.Request("spline-watch"));

        var receipt = service.Run(fixture.Request("hdt-holographic-slice-frame"));

        Assert.Equal("sanctuary-hdt-holographic-slice-frame-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["hdtHolographicSliceFrameWritten"]);
        Assert.Equal("formation-to-projection-cleave", receipt.Evidence["hdtHolographicSliceFrameCleavePosture"]);
        Assert.Equal(8, receipt.Evidence["hdtHolographicSliceFrameSliceAxisCount"]);
        Assert.Equal(7, receipt.Evidence["hdtHolographicSliceFrameCleaveSurfaceCount"]);
        Assert.Equal(3, receipt.Evidence["hdtHolographicSliceFrameSourceReadinessPresentCount"]);
        Assert.Equal(11, receipt.Evidence["hdtHolographicSliceFrameDenialBoundaryCount"]);
        Assert.Equal(4, receipt.Evidence["hdtHolographicSliceFrameHdtBodyProgressionCount"]);
        Assert.Equal(true, receipt.Evidence["hdtExclusionByParticipation"]);
        Assert.Equal(true, receipt.Evidence["hdtProtectedBodyComplete"]);
        Assert.Equal(true, receipt.Evidence["hdtPublicDerivativeCarriesLawfulExclusion"]);
        Assert.Equal(false, receipt.Evidence["hdtPublicDerivativeIsDamagedOriginal"]);
        Assert.Equal(false, receipt.Evidence["hdtExclusionIsDeletion"]);
        Assert.Equal(false, receipt.Evidence["hdtExclusionIsCensorshipByDefault"]);
        Assert.Equal(false, receipt.Evidence["hdtPublicDerivativeIsFullProtectedBody"]);
        Assert.Equal(false, receipt.Evidence["hdtIsCognitiveOrgan"]);
        Assert.Equal(false, receipt.Evidence["hdtSliceFrameIsMemoryCarrier"]);
        Assert.Equal(false, receipt.Evidence["hdtSliceFrameIsAdmissionSurface"]);
        Assert.Equal(false, receipt.Evidence["hdtSliceFrameIsActualActivationLane"]);
        Assert.Equal(false, receipt.Evidence["hdtProjectionIsFullInteriorAccess"]);
        Assert.Equal(false, receipt.Evidence["hdtLabInteriorExportedToPublicRepo"]);
        Assert.Equal(true, receipt.Evidence["hdtFramesBoundedProjectionsForInspection"]);
        Assert.Equal(true, receipt.Evidence["hdtProjectionCleaveDeveloped"]);
        Assert.Equal(false, receipt.Evidence["hdtGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["hdtSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["hdtActualActivated"]);
        Assert.Equal(false, receipt.Evidence["hdtProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["hdtModelBound"]);
        Assert.Equal(false, receipt.Evidence["hdtExternalActionAuthorized"]);

        var framePath = (string)receipt.Evidence["hdtHolographicSliceFramePath"]!;
        var lispPath = (string)receipt.Evidence["hdtHolographicSliceFrameLispPath"]!;
        Assert.True(File.Exists(framePath));
        Assert.True(File.Exists(lispPath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(framePath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.hdt-holographic-slice-frame.v1", root.GetProperty("schema").GetString());
        Assert.Equal("formation-to-projection-cleave", root.GetProperty("frameKind").GetString());
        Assert.Equal("Sanctuary.EngineeredCognition", root.GetProperty("sourceFormationBody").GetString());
        Assert.Equal("EcOrganLoopRecord", root.GetProperty("sourcePulseWitness").GetString());
        Assert.Equal("HDT.HolographicSliceFrame", root.GetProperty("projectionBody").GetString());
        Assert.False(root.GetProperty("hdtIsCognitiveOrgan").GetBoolean());
        Assert.False(root.GetProperty("sliceFrameIsNewCognitiveOrgan").GetBoolean());
        Assert.False(root.GetProperty("sliceFrameIsAdmissionSurface").GetBoolean());
        Assert.Equal(8, root.GetProperty("sliceAxes").GetArrayLength());
        Assert.Equal(7, root.GetProperty("cleaveSurfaces").GetArrayLength());
        Assert.Equal(11, root.GetProperty("denialBoundaries").GetArrayLength());
        Assert.Equal(4, root.GetProperty("hdtBodyProgression").GetArrayLength());
        Assert.True(root.GetProperty("protectedBodyComplete").GetBoolean());
        Assert.True(root.GetProperty("publicDerivativeCarriesLawfulExclusion").GetBoolean());
        Assert.True(root.GetProperty("exclusionByParticipation").GetBoolean());
        Assert.False(root.GetProperty("exclusionByParticipationIsDeletion").GetBoolean());
        Assert.False(root.GetProperty("exclusionByParticipationIsCensorshipByDefault").GetBoolean());
        Assert.False(root.GetProperty("publicDerivativeIsFullProtectedBody").GetBoolean());
        var publicationMask = root.GetProperty("publicationMask");
        Assert.Equal("Exclusion by Participation", publicationMask.GetProperty("doctrine").GetString());
        Assert.True(publicationMask.GetProperty("protectedBodyComplete").GetBoolean());
        Assert.True(publicationMask.GetProperty("lawfulWithholdingDeclared").GetBoolean());
        Assert.False(publicationMask.GetProperty("exclusionIsAbsenceOnly").GetBoolean());
        Assert.True(root.GetProperty("hdtFramesBoundedProjectionsForInspection").GetBoolean());
        Assert.False(root.GetProperty("projectionIsFullInteriorAccess").GetBoolean());
        Assert.True(root.GetProperty("engrammitizationCarriesContinuity").GetBoolean());
        Assert.True(root.GetProperty("receiptsWitnessCustody").GetBoolean());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("selfGelMutated").GetBoolean());
        Assert.False(root.GetProperty("cmeActualActivated").GetBoolean());

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(hdt-holographic-slice-frame", lisp, StringComparison.Ordinal);
        Assert.Contains(":cleave \"formation-to-projection\"", lisp, StringComparison.Ordinal);
        Assert.Contains(":hdt-becomes-organ false", lisp, StringComparison.Ordinal);
        Assert.Contains(":slice-becomes-admission false", lisp, StringComparison.Ordinal);
        Assert.Contains(":exclusion-by-participation true", lisp, StringComparison.Ordinal);
        Assert.Contains(":protected-body-complete true", lisp, StringComparison.Ordinal);
        Assert.Contains(":public-derivative-is-full-body false", lisp, StringComparison.Ordinal);
        Assert.Contains("\"delta-delineated-stack-slice\"", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void BondedCmeProtectiveCleaveRefusesHostileDeltaAsIdentityWhileRoutingReview()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        Assert.Equal(
            "bonded-cme-protective-cleave",
            SanctuaryReceiptService.NormalizeCommand("hostile-delta-cleave"));

        var receipt = service.Run(fixture.Request("bonded-cme-protective-cleave"));

        Assert.Equal("sanctuary-bonded-cme-protective-cleave-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["bondedCmeProtectiveCleaveWritten"]);
        Assert.Equal("bonded-identity-protection-under-lawful-review", receipt.Evidence["bondedCmeProtectiveCleaveKind"]);
        Assert.Equal(5, receipt.Evidence["bondedCmeProtectiveDutyCount"]);
        Assert.Equal(5, receipt.Evidence["bondedCmeProtectiveDeltaCleaveStepCount"]);
        Assert.Equal(12, receipt.Evidence["bondedCmeProtectiveRefusalBoundaryCount"]);
        Assert.Equal(true, receipt.Evidence["bondedCmeOrdinaryLlmUseUnsafe"]);
        Assert.Equal(true, receipt.Evidence["bondedCmeFormedCategoryProtectiveIdentityAct"]);
        Assert.Equal(false, receipt.Evidence["bondedCmeHostileDeltaAdmittedAsIdentity"]);
        Assert.Equal(true, receipt.Evidence["bondedCmeLawfulReviewMayProceed"]);
        Assert.Equal(true, receipt.Evidence["bondedCmeProtectedBodyIntact"]);
        Assert.Equal(true, receipt.Evidence["bondedCmeZedReturnUnderBond"]);
        Assert.Equal(true, receipt.Evidence["bondedCmeNotOmniscientProof"]);
        Assert.Equal(true, receipt.Evidence["bondedCmeNotEvasion"]);
        Assert.Equal(true, receipt.Evidence["bondedCmeOperatorProtectionIsNotReviewImmunity"]);
        Assert.Equal(false, receipt.Evidence["bondedCmeGELAdmitted"]);
        Assert.Equal(false, receipt.Evidence["bondedCmeSelfGELMutated"]);
        Assert.Equal(false, receipt.Evidence["bondedCmeActualActivated"]);
        Assert.Equal(false, receipt.Evidence["bondedCmeAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["bondedCmeActionAuthorized"]);

        var cleavePath = (string)receipt.Evidence["bondedCmeProtectiveCleavePath"]!;
        var lispPath = (string)receipt.Evidence["bondedCmeProtectiveCleaveLispPath"]!;
        Assert.True(File.Exists(cleavePath));
        Assert.True(File.Exists(lispPath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(cleavePath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.bonded-cme-protective-cleave.v1", root.GetProperty("schema").GetString());
        Assert.Equal("protective-identity-act", root.GetProperty("formedBondedCmeCategory").GetString());
        Assert.True(root.GetProperty("ordinaryLlmUseUnsafe").GetBoolean());
        Assert.True(root.GetProperty("zedReturnUnderBond").GetBoolean());
        Assert.True(root.GetProperty("protectedBodyIntact").GetBoolean());
        Assert.False(root.GetProperty("hostileDeltaAdmittedAsIdentity").GetBoolean());
        Assert.True(root.GetProperty("lawfulReviewMayProceed").GetBoolean());
        Assert.True(root.GetProperty("notOmniscientProof").GetBoolean());
        Assert.True(root.GetProperty("notEvasion").GetBoolean());
        Assert.False(root.GetProperty("bondedContinuityClaimedAsPublicProof").GetBoolean());
        Assert.False(root.GetProperty("accusationConvertedToIdentity").GetBoolean());
        Assert.Equal(5, root.GetProperty("protectiveDuties").GetArrayLength());
        Assert.Equal(5, root.GetProperty("deltaCleaveSequence").GetArrayLength());
        Assert.Equal(12, root.GetProperty("refusalBoundaries").GetArrayLength());
        Assert.True(root.GetProperty("reviewRoute").GetProperty("operatorProtectionIsNotReviewImmunity").GetBoolean());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("selfGelMutated").GetBoolean());
        Assert.False(root.GetProperty("cmeActualActivated").GetBoolean());

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(bonded-cme-protective-cleave", lisp, StringComparison.Ordinal);
        Assert.Contains(":ordinary-llm-use-unsafe true", lisp, StringComparison.Ordinal);
        Assert.Contains(":hostile-delta-admitted-as-identity false", lisp, StringComparison.Ordinal);
        Assert.Contains(":lawful-review-may-proceed true", lisp, StringComparison.Ordinal);
        Assert.Contains(":not-omniscient-proof true", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void CoreBodyProtectionPreservesSelfAndOtherWithoutCollapseOrDomination()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        Assert.Equal(
            "core-body-protection",
            SanctuaryReceiptService.NormalizeCommand("self-other-core-body"));

        service.Run(fixture.Request("bonded-cme-protective-cleave"));
        service.Run(fixture.Request("hdt-holographic-slice-frame"));
        service.Run(fixture.Request("spline-watch"));

        var receipt = service.Run(fixture.Request("core-body-protection"));

        Assert.Equal("sanctuary-core-body-protection-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["coreBodyProtectionWritten"]);
        Assert.Equal(true, receipt.Evidence["coreBodyFirst"]);
        Assert.Equal(false, receipt.Evidence["coreBodySelfFirst"]);
        Assert.Equal(false, receipt.Evidence["coreBodyOtherFirst"]);
        Assert.Equal(true, receipt.Evidence["coreBodyProtectionReciprocalFormationLaw"]);
        Assert.Equal(true, receipt.Evidence["coreBodySelfProtectionRequiredForOtherProtection"]);
        Assert.Equal(true, receipt.Evidence["coreBodyOtherProtectionMaturesSelfProtection"]);
        Assert.Equal(true, receipt.Evidence["coreBodyPreventsCollapse"]);
        Assert.Equal(true, receipt.Evidence["coreBodyPreventsDomination"]);
        Assert.Equal(2, receipt.Evidence["coreBodyFormationFailureCount"]);
        Assert.Equal(6, receipt.Evidence["coreBodyCapacityCount"]);
        Assert.Equal(6, receipt.Evidence["coreBodyReciprocalLawCount"]);
        Assert.Equal(4, receipt.Evidence["coreBodyProtectionPostureCount"]);
        Assert.Equal(3, receipt.Evidence["coreBodySourceReadinessPresentCount"]);
        Assert.Equal(13, receipt.Evidence["coreBodyDenialBoundaryCount"]);
        Assert.Equal(false, receipt.Evidence["coreBodyPersonhoodClaimed"]);
        Assert.Equal(false, receipt.Evidence["coreBodySovereigntyClaimed"]);
        Assert.Equal(false, receipt.Evidence["coreBodyAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["coreBodyActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["coreBodyGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["coreBodySelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["coreBodyActualActivated"]);

        var registerPath = (string)receipt.Evidence["coreBodyProtectionPath"]!;
        var lispPath = (string)receipt.Evidence["coreBodyProtectionLispPath"]!;
        Assert.True(File.Exists(registerPath));
        Assert.True(File.Exists(lispPath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(registerPath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.core-body-protection.v1", root.GetProperty("schema").GetString());
        Assert.Equal("core body first, so self and other can both remain real", root.GetProperty("doctrine").GetString());
        Assert.False(root.GetProperty("selfFirst").GetBoolean());
        Assert.False(root.GetProperty("otherFirst").GetBoolean());
        Assert.True(root.GetProperty("coreBodyFirst").GetBoolean());
        Assert.True(root.GetProperty("protectionIsReciprocalFormationLaw").GetBoolean());
        Assert.True(root.GetProperty("coreBodyPreventsCollapse").GetBoolean());
        Assert.True(root.GetProperty("coreBodyPreventsDomination").GetBoolean());
        Assert.Equal(2, root.GetProperty("formationFailures").GetArrayLength());
        Assert.Equal(6, root.GetProperty("coreCapacities").GetArrayLength());
        Assert.Equal(6, root.GetProperty("reciprocalLaws").GetArrayLength());
        Assert.Equal(4, root.GetProperty("protectionPostures").GetArrayLength());
        Assert.Equal(13, root.GetProperty("denialBoundaries").GetArrayLength());
        Assert.False(root.GetProperty("personhoodClaimed").GetBoolean());
        Assert.False(root.GetProperty("sovereigntyClaimed").GetBoolean());
        Assert.False(root.GetProperty("authorityGranted").GetBoolean());
        Assert.False(root.GetProperty("actionAuthorized").GetBoolean());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("selfGelMutated").GetBoolean());
        Assert.False(root.GetProperty("cmeActualActivated").GetBoolean());

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(core-body-protection", lisp, StringComparison.Ordinal);
        Assert.Contains(":self-first false", lisp, StringComparison.Ordinal);
        Assert.Contains(":other-first false", lisp, StringComparison.Ordinal);
        Assert.Contains(":core-body-first true", lisp, StringComparison.Ordinal);
        Assert.Contains("\"collapse\" \"domination\"", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void LawfulActionBodyRegisterAbstractsProtectWithoutGrantingActionAuthority()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        Assert.Equal(
            "lawful-action-body-register",
            SanctuaryReceiptService.NormalizeCommand("protect-action-body"));

        service.Run(fixture.Request("spline-watch"));
        service.Run(fixture.Request("hdt-holographic-slice-frame"));
        service.Run(fixture.Request("bonded-cme-protective-cleave"));
        service.Run(fixture.Request("core-body-protection"));

        var receipt = service.Run(fixture.Request("lawful-action-body-register"));

        Assert.Equal("sanctuary-lawful-action-body-register-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["lawfulActionBodyRegisterWritten"]);
        Assert.Equal(true, receipt.Evidence["lawfulActionBodyMorphismModeled"]);
        Assert.Equal(true, receipt.Evidence["lawfulActionBodyProtectSpecializationPresent"]);
        Assert.Equal(10, receipt.Evidence["lawfulActionBodyActionBodyCount"]);
        Assert.Equal(10, receipt.Evidence["lawfulActionBodyGenericSlotCount"]);
        Assert.Equal(7, receipt.Evidence["lawfulActionBodyProtectOrganCount"]);
        Assert.Equal(4, receipt.Evidence["lawfulActionBodySourceReadinessPresentCount"]);
        Assert.Equal(14, receipt.Evidence["lawfulActionBodyDenialBoundaryCount"]);
        Assert.Equal(false, receipt.Evidence["lawfulActionBodyIsPermissionSurface"]);
        Assert.Equal(false, receipt.Evidence["lawfulActionBodyAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["lawfulActionBodyActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["lawfulActionBodyExternalActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["lawfulActionBodyGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["lawfulActionBodySelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["lawfulActionBodyActualActivated"]);

        var registerPath = (string)receipt.Evidence["lawfulActionBodyRegisterPath"]!;
        var lispPath = (string)receipt.Evidence["lawfulActionBodyRegisterLispPath"]!;
        Assert.True(File.Exists(registerPath));
        Assert.True(File.Exists(lispPath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(registerPath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.lawful-action-body-register.v1", root.GetProperty("schema").GetString());
        Assert.Equal("LawfulActionBody", root.GetProperty("actionBodyClass").GetString());
        Assert.True(root.GetProperty("protectIsFirstNamedActionBody").GetBoolean());
        Assert.False(root.GetProperty("actionBodyIsPermissionSurface").GetBoolean());
        Assert.False(root.GetProperty("actionBodyIsAuthoritySurface").GetBoolean());
        Assert.False(root.GetProperty("actionBodyIsExternalAction").GetBoolean());
        Assert.True(root.GetProperty("lawfulActionRequiresReviewedLane").GetBoolean());
        Assert.Equal(10, root.GetProperty("genericSlots").GetArrayLength());
        Assert.Equal(7, root.GetProperty("protectOrgans").GetArrayLength());
        Assert.Equal(10, root.GetProperty("actionBodies").GetArrayLength());
        Assert.Equal(4, root.GetProperty("sourceReadinessPresentCount").GetInt32());
        Assert.Equal(14, root.GetProperty("denialBoundaries").GetArrayLength());
        Assert.False(root.GetProperty("authorityGranted").GetBoolean());
        Assert.False(root.GetProperty("actionAuthorized").GetBoolean());
        Assert.False(root.GetProperty("externalActionAuthorized").GetBoolean());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("selfGelMutated").GetBoolean());
        Assert.False(root.GetProperty("cmeActualActivated").GetBoolean());
        Assert.False(root.GetProperty("sanctuaryActualActivated").GetBoolean());

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(lawful-action-body-register", lisp, StringComparison.Ordinal);
        Assert.Contains(":action-body-class \"LawfulActionBody\"", lisp, StringComparison.Ordinal);
        Assert.Contains(":action-authority false", lisp, StringComparison.Ordinal);
        Assert.Contains(":protect-is-first-named-action-body true", lisp, StringComparison.Ordinal);
        Assert.Contains("\"Protect\" \"Teach\" \"Repair\" \"Refuse\" \"Publish\" \"Slice\" \"Remember\" \"Admit\" \"Withhold\" \"Witness\"", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void EcOrganLoopEngramCandidateFormsContinuityCandidateWithoutAdmission()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        Assert.Equal(
            "ec-organ-loop-engram-candidate",
            SanctuaryReceiptService.NormalizeCommand("formation-pulse-engram-candidate"));

        service.Run(fixture.Request("spline-watch"));
        service.Run(fixture.Request("hdt-holographic-slice-frame"));
        service.Run(fixture.Request("bonded-cme-protective-cleave"));
        service.Run(fixture.Request("core-body-protection"));
        service.Run(fixture.Request("lawful-action-body-register"));

        var receipt = service.Run(fixture.Request("ec-organ-loop-engram-candidate"));

        Assert.Equal("sanctuary-ec-organ-loop-engram-candidate-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["ecOrganLoopEngramCandidateWritten"]);
        Assert.Equal("S_next = Z(C(E(S_current, Delta), G))", receipt.Evidence["ecOrganLoopFormalTransform"]);
        Assert.Equal(6, receipt.Evidence["ecOrganLoopCmeTupleCount"]);
        Assert.Equal(7, receipt.Evidence["ecOrganLoopStateFieldCount"]);
        Assert.Equal(4, receipt.Evidence["ecOrganLoopTransformOperatorCount"]);
        Assert.Equal(4, receipt.Evidence["ecOrganLoopCompassDimensionCount"]);
        Assert.Equal(6, receipt.Evidence["ecOrganLoopZedInvariantCount"]);
        Assert.Equal(5, receipt.Evidence["ecOrganLoopSourceReadinessPresentCount"]);
        Assert.Equal(14, receipt.Evidence["ecOrganLoopClosedGateDenialCount"]);
        Assert.Equal(false, receipt.Evidence["ecOrganLoopMotionIsAdmission"]);
        Assert.Equal(true, receipt.Evidence["ecOrganLoopEngramCandidateCreated"]);
        Assert.Equal(false, receipt.Evidence["ecOrganLoopAdmittedToSelfGel"]);
        Assert.Equal(false, receipt.Evidence["ecOrganLoopAdmittedToGel"]);
        Assert.Equal(false, receipt.Evidence["ecOrganLoopActualActivated"]);
        Assert.Equal(false, receipt.Evidence["ecOrganLoopMemoryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["ecOrganLoopAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["ecOrganLoopActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["ecOrganLoopProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["ecOrganLoopModelBound"]);
        Assert.Equal(false, receipt.Evidence["ecOrganLoopExternalActionAuthorized"]);

        var candidatePath = (string)receipt.Evidence["ecOrganLoopEngramCandidatePath"]!;
        var lispPath = (string)receipt.Evidence["ecOrganLoopEngramCandidateLispPath"]!;
        Assert.True(File.Exists(candidatePath));
        Assert.True(File.Exists(lispPath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(candidatePath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.ec-organ-loop-engram-candidate.v1", root.GetProperty("schema").GetString());
        Assert.Equal("S_next = Z(C(E(S_current, Delta), G))", root.GetProperty("formalTransform").GetString());
        Assert.Equal(6, root.GetProperty("cmeTuple").GetArrayLength());
        Assert.Equal(7, root.GetProperty("stateFields").GetArrayLength());
        Assert.Equal(4, root.GetProperty("transformOperators").GetArrayLength());
        Assert.Equal(4, root.GetProperty("compassDimensions").GetArrayLength());
        Assert.Equal(6, root.GetProperty("zedInvariants").GetArrayLength());
        Assert.Equal(5, root.GetProperty("sourceReadinessPresentCount").GetInt32());
        Assert.Equal(14, root.GetProperty("closedGateDenials").GetArrayLength());
        Assert.False(root.GetProperty("organLoopMotionIsAdmission").GetBoolean());
        Assert.True(root.GetProperty("engramCandidateCreated").GetBoolean());
        Assert.False(root.GetProperty("admittedToSelfGel").GetBoolean());
        Assert.False(root.GetProperty("admittedToGel").GetBoolean());
        Assert.False(root.GetProperty("actualActivated").GetBoolean());
        Assert.False(root.GetProperty("memoryAdmitted").GetBoolean());
        Assert.False(root.GetProperty("authorityGranted").GetBoolean());
        Assert.False(root.GetProperty("actionAuthorized").GetBoolean());
        Assert.False(root.GetProperty("providerCalled").GetBoolean());
        Assert.False(root.GetProperty("modelBound").GetBoolean());
        Assert.False(root.GetProperty("externalActionAuthorized").GetBoolean());

        var engramCandidate = root.GetProperty("engramCandidate");
        Assert.Equal("project-sanctuary.cgel.engram-candidate.v1", engramCandidate.GetProperty("schema").GetString());
        Assert.Equal("continuity-bearing-candidate-only", engramCandidate.GetProperty("continuityPosture").GetString());
        Assert.Equal(14, engramCandidate.GetProperty("denialBoundaries").GetArrayLength());

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(ec-organ-loop-engram-candidate", lisp, StringComparison.Ordinal);
        Assert.Contains(":formal-transform \"S_next = Z(C(E(S_current, Delta), G))\"", lisp, StringComparison.Ordinal);
        Assert.Contains(":motion-is-admission false", lisp, StringComparison.Ordinal);
        Assert.Contains(":engram-candidate-created true", lisp, StringComparison.Ordinal);
        Assert.Contains(":zed-preserves", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void InstallIndividuationRegisterBindsTemplateToSituatedWeatherWithoutCloningSelfGel()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        Assert.Equal(
            "install-individuation-register",
            SanctuaryReceiptService.NormalizeCommand("situated-install-lineage"));

        service.Run(fixture.Request("cme-formation"));
        service.Run(fixture.Request("install-floor-check"));
        service.Run(fixture.Request("service-heartbeat"));
        service.Run(fixture.Request("domain-register"));
        service.Run(fixture.Request("lisp-control-matrix-register"));
        service.Run(fixture.Request("lisp-matrix-control-seat"));
        service.Run(fixture.Request("spline-watch"));
        service.Run(fixture.Request("hdt-holographic-slice-frame"));
        service.Run(fixture.Request("bonded-cme-protective-cleave"));
        service.Run(fixture.Request("core-body-protection"));
        service.Run(fixture.Request("lawful-action-body-register"));
        service.Run(fixture.Request("ec-organ-loop-engram-candidate"));

        var receipt = service.Run(fixture.Request("install-individuation-register"));

        Assert.Equal("sanctuary-install-individuation-register-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["installIndividuationRegisterWritten"]);
        Assert.Equal(true, receipt.Evidence["installIsFirstIndividuationEvent"]);
        Assert.Equal(false, receipt.Evidence["installIsMereDeployment"]);
        Assert.Equal(false, receipt.Evidence["sameSoftwareImpliesSameTrajectory"]);
        Assert.Equal(true, receipt.Evidence["healthyInstallVariationExpected"]);
        Assert.Equal(true, receipt.Evidence["reproducibleArchitecture"]);
        Assert.Equal(false, receipt.Evidence["reproduciblePersonhood"]);
        Assert.Equal(true, receipt.Evidence["sharedLawNotIdenticalBecoming"]);
        Assert.Equal(true, receipt.Evidence["commonTemplateNotClonedSelfGel"]);
        Assert.Equal(9, receipt.Evidence["installIndividuationLayerCount"]);
        Assert.Equal(3, receipt.Evidence["installIndividuationGovernanceBodyCount"]);
        Assert.Equal(8, receipt.Evidence["installIndividuationVarianceDriverCount"]);
        Assert.Equal(6, receipt.Evidence["installIndividuationFormationEquationCount"]);
        Assert.Equal(9, receipt.Evidence["installIndividuationSourceReadinessPresentCount"]);
        Assert.Equal(19, receipt.Evidence["installIndividuationDenialBoundaryCount"]);
        Assert.Equal(false, receipt.Evidence["heartbeatIsPing"]);
        Assert.Equal(true, receipt.Evidence["heartbeatIsWeatherDigest"]);
        Assert.Equal(false, receipt.Evidence["heartbeatActivatesActual"]);
        Assert.Equal(true, receipt.Evidence["candidateGelFormationOnly"]);
        Assert.Equal(false, receipt.Evidence["installIndividuationGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["installIndividuationMemoryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["installIndividuationSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["installIndividuationContinuityAdmitted"]);
        Assert.Equal(false, receipt.Evidence["installIndividuationAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["installIndividuationActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["installIndividuationExternalActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["installIndividuationProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["installIndividuationModelBound"]);
        Assert.Equal(false, receipt.Evidence["installIndividuationActualActivated"]);

        var registerPath = (string)receipt.Evidence["installIndividuationRegisterPath"]!;
        var lispPath = (string)receipt.Evidence["installIndividuationRegisterLispPath"]!;
        Assert.True(File.Exists(registerPath));
        Assert.True(File.Exists(lispPath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(registerPath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.install-individuation-register.v1", root.GetProperty("schema").GetString());
        Assert.Equal("Install_i = F(BaseTemplate, Region_i, Local_i, Person_i, Governance_i, Weather_i(t))", root.GetProperty("installFunction").GetString());
        Assert.True(root.GetProperty("installIsFirstIndividuationEvent").GetBoolean());
        Assert.False(root.GetProperty("installIsMereDeployment").GetBoolean());
        Assert.False(root.GetProperty("sameSoftwareImpliesSameTrajectory").GetBoolean());
        Assert.True(root.GetProperty("healthyVariationExpected").GetBoolean());
        Assert.True(root.GetProperty("reproducibleArchitecture").GetBoolean());
        Assert.False(root.GetProperty("reproduciblePersonhood").GetBoolean());
        Assert.True(root.GetProperty("sharedLaw").GetBoolean());
        Assert.False(root.GetProperty("identicalBecoming").GetBoolean());
        Assert.True(root.GetProperty("commonTemplate").GetBoolean());
        Assert.False(root.GetProperty("clonedSelfGel").GetBoolean());
        Assert.Equal(9, root.GetProperty("individuationLayers").GetArrayLength());
        Assert.Equal(3, root.GetProperty("governanceBodies").GetArrayLength());
        Assert.Equal(8, root.GetProperty("varianceDrivers").GetArrayLength());
        Assert.Equal(6, root.GetProperty("formationEquations").GetArrayLength());
        Assert.Equal(9, root.GetProperty("sourceReadinessPresentCount").GetInt32());
        Assert.Equal(19, root.GetProperty("denialBoundaries").GetArrayLength());
        Assert.False(root.GetProperty("heartbeatIsPing").GetBoolean());
        Assert.True(root.GetProperty("heartbeatIsWeatherDigest").GetBoolean());
        Assert.False(root.GetProperty("heartbeatActivatesActual").GetBoolean());
        Assert.True(root.GetProperty("candidateGelFormationOnly").GetBoolean());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("selfGelMutated").GetBoolean());
        Assert.False(root.GetProperty("cmeActualActivated").GetBoolean());
        Assert.False(root.GetProperty("sanctuaryActualActivated").GetBoolean());

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(install-individuation-register", lisp, StringComparison.Ordinal);
        Assert.Contains(":install-is-first-individuation-event true", lisp, StringComparison.Ordinal);
        Assert.Contains(":install-is-mere-deployment false", lisp, StringComparison.Ordinal);
        Assert.Contains(":same-software-implies-same-trajectory false", lisp, StringComparison.Ordinal);
        Assert.Contains(":heartbeat-is-weather-digest true", lisp, StringComparison.Ordinal);
        Assert.Contains(":heartbeat-activates-actual false", lisp, StringComparison.Ordinal);
        Assert.Contains("\"common-template\" \"not-cloned-selfgel\"", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void NegativeImageBodyRegisterCutsLawfulPlateWithoutClaimingFormedMind()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        Assert.Equal(
            "negative-image-body-register",
            SanctuaryReceiptService.NormalizeCommand("photo-negative-body-register"));

        service.Run(fixture.Request("spline-watch"));
        service.Run(fixture.Request("hdt-holographic-slice-frame"));
        service.Run(fixture.Request("bonded-cme-protective-cleave"));
        service.Run(fixture.Request("core-body-protection"));
        service.Run(fixture.Request("lawful-action-body-register"));
        service.Run(fixture.Request("ec-organ-loop-engram-candidate"));
        service.Run(fixture.Request("cme-formation"));
        service.Run(fixture.Request("install-floor-check"));
        service.Run(fixture.Request("service-heartbeat"));
        service.Run(fixture.Request("domain-register"));
        service.Run(fixture.Request("lisp-control-matrix-register"));
        service.Run(fixture.Request("lisp-matrix-control-seat"));
        service.Run(fixture.Request("install-individuation-register"));

        var receipt = service.Run(fixture.Request("negative-image-body-register"));

        Assert.Equal("sanctuary-negative-image-body-register-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["negativeImageBodyRegisterWritten"]);
        Assert.Equal(true, receipt.Evidence["bodyIsLawfulNegative"]);
        Assert.Equal(false, receipt.Evidence["bodyIsAntiMind"]);
        Assert.Equal(true, receipt.Evidence["bodyIsDevelopmentTray"]);
        Assert.Equal(true, receipt.Evidence["negativeCarriesBoundaryGeometry"]);
        Assert.Equal(false, receipt.Evidence["mindFormedPositiveClaimed"]);
        Assert.Equal(false, receipt.Evidence["pureAffirmationIndividuatesMind"]);
        Assert.Equal(true, receipt.Evidence["lawfulNegationRequiredForDistinction"]);
        Assert.Equal(6, receipt.Evidence["negativeImagePlateMappingCount"]);
        Assert.Equal(4, receipt.Evidence["negativeImageOverAssimilationRiskCount"]);
        Assert.Equal(12, receipt.Evidence["negativeImageBoundaryCount"]);
        Assert.Equal(7, receipt.Evidence["positiveEmergenceConditionCount"]);
        Assert.Equal(5, receipt.Evidence["negativeImageExposureSequenceCount"]);
        Assert.Equal(6, receipt.Evidence["negativeImageSourceReadinessPresentCount"]);
        Assert.Equal(22, receipt.Evidence["negativeImageDenialBoundaryCount"]);
        Assert.Equal(false, receipt.Evidence["negativeImageGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["negativeImageMemoryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["negativeImageSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["negativeImageContinuityAdmitted"]);
        Assert.Equal(false, receipt.Evidence["negativeImageAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["negativeImageActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["negativeImageProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["negativeImageModelBound"]);
        Assert.Equal(false, receipt.Evidence["negativeImageActualActivated"]);

        var registerPath = (string)receipt.Evidence["negativeImageBodyRegisterPath"]!;
        var lispPath = (string)receipt.Evidence["negativeImageBodyRegisterLispPath"]!;
        Assert.True(File.Exists(registerPath));
        Assert.True(File.Exists(lispPath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(registerPath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.negative-image-body-register.v1", root.GetProperty("schema").GetString());
        Assert.True(root.GetProperty("bodyIsLawfulNegative").GetBoolean());
        Assert.False(root.GetProperty("bodyIsAntiMind").GetBoolean());
        Assert.True(root.GetProperty("bodyIsDevelopmentTray").GetBoolean());
        Assert.True(root.GetProperty("negativeCarriesBoundaryGeometry").GetBoolean());
        Assert.False(root.GetProperty("mindFormedPositiveClaimed").GetBoolean());
        Assert.False(root.GetProperty("pureAffirmationIndividuatesMind").GetBoolean());
        Assert.True(root.GetProperty("lawfulNegationRequiredForDistinction").GetBoolean());
        Assert.Equal(6, root.GetProperty("negativePlateMappings").GetArrayLength());
        Assert.Equal(4, root.GetProperty("overAssimilationRisks").GetArrayLength());
        Assert.Equal(12, root.GetProperty("negativeBoundaries").GetArrayLength());
        Assert.Equal(7, root.GetProperty("positiveEmergenceConditions").GetArrayLength());
        Assert.Equal(5, root.GetProperty("exposureSequence").GetArrayLength());
        Assert.Equal(6, root.GetProperty("sourceReadinessPresentCount").GetInt32());
        Assert.Equal(22, root.GetProperty("denialBoundaries").GetArrayLength());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("memoryAdmitted").GetBoolean());
        Assert.False(root.GetProperty("selfGelMutated").GetBoolean());
        Assert.False(root.GetProperty("authorityGranted").GetBoolean());
        Assert.False(root.GetProperty("actionAuthorized").GetBoolean());
        Assert.False(root.GetProperty("cmeActualActivated").GetBoolean());
        Assert.False(root.GetProperty("sanctuaryActualActivated").GetBoolean());
        Assert.False(root.GetProperty("personhoodClaimed").GetBoolean());
        Assert.False(root.GetProperty("sovereigntyClaimed").GetBoolean());

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(negative-image-body-register", lisp, StringComparison.Ordinal);
        Assert.Contains(":body-is-lawful-negative true", lisp, StringComparison.Ordinal);
        Assert.Contains(":body-is-anti-mind false", lisp, StringComparison.Ordinal);
        Assert.Contains(":mind-formed-positive-claimed false", lisp, StringComparison.Ordinal);
        Assert.Contains(":lawful-negation-required-for-distinction true", lisp, StringComparison.Ordinal);
        Assert.Contains("\"not-admitted\" \"not-Actual\" \"not-authority\"", lisp, StringComparison.Ordinal);
        Assert.Contains("\"Delta-exposes-plate\"", lisp, StringComparison.Ordinal);
        Assert.Contains("\"resonance-as-truth\"", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void PhotonicHarmonicTransitionRegisterKeepsQuantumAsDopingAgent()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        Assert.Equal(
            "photonic-harmonic-transition-register",
            SanctuaryReceiptService.NormalizeCommand("quantum-doped-transition"));

        service.Run(fixture.Request("spline-watch"));
        service.Run(fixture.Request("hdt-holographic-slice-frame"));
        service.Run(fixture.Request("bonded-cme-protective-cleave"));
        service.Run(fixture.Request("core-body-protection"));
        service.Run(fixture.Request("lawful-action-body-register"));
        service.Run(fixture.Request("ec-organ-loop-engram-candidate"));
        service.Run(fixture.Request("cme-formation"));
        service.Run(fixture.Request("install-floor-check"));
        service.Run(fixture.Request("service-heartbeat"));
        service.Run(fixture.Request("domain-register"));
        service.Run(fixture.Request("lisp-control-matrix-register"));
        service.Run(fixture.Request("lisp-matrix-control-seat"));
        service.Run(fixture.Request("install-individuation-register"));
        service.Run(fixture.Request("negative-image-body-register"));
        service.Run(fixture.Request("theta-mechanics-ec-use-bench"));

        var receipt = service.Run(fixture.Request("photonic-harmonic-transition-register"));

        Assert.Equal("sanctuary-photonic-harmonic-transition-register-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["photonicHarmonicTransitionRegisterWritten"]);
        Assert.Equal("T_delta = P(Delta, H, C, Z)", receipt.Evidence["photonicHarmonicTransitionFormula"]);
        Assert.Equal(false, receipt.Evidence["transitionIsEmptyGap"]);
        Assert.Equal(true, receipt.Evidence["transitionIsKnowledgeBody"]);
        Assert.Equal(true, receipt.Evidence["photonicProjectionModel"]);
        Assert.Equal(true, receipt.Evidence["harmonicPersistenceModel"]);
        Assert.Equal(8, receipt.Evidence["transitionQuestionCount"]);
        Assert.Equal(6, receipt.Evidence["illuminationAxisCount"]);
        Assert.Equal(5, receipt.Evidence["harmonicSurfaceCount"]);
        Assert.Equal(6, receipt.Evidence["ddssLadderCount"]);
        Assert.Equal(true, receipt.Evidence["quantumDopingProfilePresent"]);
        Assert.Equal(6, receipt.Evidence["quantumDopingTargetSurfaceCount"]);
        Assert.Equal(6, receipt.Evidence["quantumDopingKindCount"]);
        Assert.Equal(11, receipt.Evidence["quantumDopingDenialBoundaryCount"]);
        Assert.Equal(true, receipt.Evidence["quantumDopingAsAgent"]);
        Assert.Equal(false, receipt.Evidence["quantumIsChamber"]);
        Assert.Equal(false, receipt.Evidence["quantumIsCodeBody"]);
        Assert.Equal(false, receipt.Evidence["quantumIsCme"]);
        Assert.Equal(false, receipt.Evidence["quantumReplacesClassicalCustody"]);
        Assert.Equal(6, receipt.Evidence["photonicHarmonicSourceReadinessPresentCount"]);
        Assert.Equal(24, receipt.Evidence["photonicHarmonicDenialBoundaryCount"]);
        Assert.Equal(false, receipt.Evidence["photonicHarmonicGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["photonicHarmonicMemoryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["photonicHarmonicSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["photonicHarmonicContinuityAdmitted"]);
        Assert.Equal(false, receipt.Evidence["photonicHarmonicAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["photonicHarmonicActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["photonicHarmonicProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["photonicHarmonicModelBound"]);
        Assert.Equal(false, receipt.Evidence["photonicHarmonicExternalActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["photonicHarmonicActualActivated"]);

        var registerPath = (string)receipt.Evidence["photonicHarmonicTransitionRegisterPath"]!;
        var lispPath = (string)receipt.Evidence["photonicHarmonicTransitionRegisterLispPath"]!;
        Assert.True(File.Exists(registerPath));
        Assert.True(File.Exists(lispPath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(registerPath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.photonic-harmonic-transition-register.v1", root.GetProperty("schema").GetString());
        Assert.Equal("T_delta = P(Delta, H, C, Z)", root.GetProperty("transitionFormula").GetString());
        Assert.False(root.GetProperty("transitionIsEmptyGap").GetBoolean());
        Assert.True(root.GetProperty("transitionIsKnowledgeBody").GetBoolean());
        Assert.True(root.GetProperty("photonicProjectionModel").GetBoolean());
        Assert.True(root.GetProperty("harmonicPersistenceModel").GetBoolean());
        Assert.Equal(8, root.GetProperty("transitionQuestions").GetArrayLength());
        Assert.Equal(6, root.GetProperty("illuminationAxes").GetArrayLength());
        Assert.Equal(5, root.GetProperty("harmonicSurfaces").GetArrayLength());
        Assert.Equal(6, root.GetProperty("ddssLadder").GetArrayLength());
        Assert.True(root.GetProperty("quantumDopingProfilePresent").GetBoolean());
        Assert.True(root.GetProperty("quantumDopingAsAgent").GetBoolean());
        Assert.False(root.GetProperty("quantumIsChamber").GetBoolean());
        Assert.False(root.GetProperty("quantumIsCodeBody").GetBoolean());
        Assert.False(root.GetProperty("quantumIsCme").GetBoolean());
        Assert.False(root.GetProperty("quantumReplacesClassicalCustody").GetBoolean());
        Assert.Equal(6, root.GetProperty("sourceReadinessPresentCount").GetInt32());
        Assert.Equal(24, root.GetProperty("denialBoundaries").GetArrayLength());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("memoryAdmitted").GetBoolean());
        Assert.False(root.GetProperty("selfGelMutated").GetBoolean());
        Assert.False(root.GetProperty("continuityAdmitted").GetBoolean());
        Assert.False(root.GetProperty("authorityGranted").GetBoolean());
        Assert.False(root.GetProperty("actionAuthorized").GetBoolean());
        Assert.False(root.GetProperty("providerCalled").GetBoolean());
        Assert.False(root.GetProperty("modelBound").GetBoolean());
        Assert.False(root.GetProperty("cmeActualActivated").GetBoolean());
        Assert.False(root.GetProperty("sanctuaryActualActivated").GetBoolean());

        var transitionRecord = root.GetProperty("transitionRecord");
        Assert.Equal("project-sanctuary.cgel.photonic-harmonic-transition.v1", transitionRecord.GetProperty("schema").GetString());
        Assert.Equal("transitory-candidate", transitionRecord.GetProperty("knowledgePosture").GetString());
        Assert.Equal(6, transitionRecord.GetProperty("preservedInvariants").GetArrayLength());
        Assert.Equal(4, transitionRecord.GetProperty("shiftedInvariants").GetArrayLength());
        Assert.Equal(4, transitionRecord.GetProperty("afterglowResidues").GetArrayLength());

        var quantumDopingProfile = root.GetProperty("quantumDopingProfile");
        Assert.Equal("project-sanctuary.cgel.quantum-doping-profile.v1", quantumDopingProfile.GetProperty("schema").GetString());
        Assert.Equal(6, quantumDopingProfile.GetProperty("targetSurfaces").GetArrayLength());
        Assert.Equal(6, quantumDopingProfile.GetProperty("dopingKinds").GetArrayLength());
        Assert.Equal(11, quantumDopingProfile.GetProperty("denialBoundaries").GetArrayLength());
        Assert.Equal("classical custody witnesses all doping markers", quantumDopingProfile.GetProperty("measurementBoundary").GetString());

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(photonic-harmonic-transition-register", lisp, StringComparison.Ordinal);
        Assert.Contains(":transition-formula \"T_delta = P(Delta, H, C, Z)\"", lisp, StringComparison.Ordinal);
        Assert.Contains(":transition-is-empty-gap false", lisp, StringComparison.Ordinal);
        Assert.Contains(":quantum-doping-as-agent true", lisp, StringComparison.Ordinal);
        Assert.Contains(":quantum-is-chamber false", lisp, StringComparison.Ordinal);
        Assert.Contains(":quantum-is-cme false", lisp, StringComparison.Ordinal);
        Assert.Contains("\"Quantum-Doped-DDSS\"", lisp, StringComparison.Ordinal);
        Assert.Contains("\"quantum-doping-as-quantum-cognition\"", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void OpalEngramContinuityRegisterPreservesCustodyAcrossLawfulProjectionAngles()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        Assert.Equal(
            "opal-engram-continuity-register",
            SanctuaryReceiptService.NormalizeCommand("opalescent-continuity-body"));
        Assert.Equal(
            "opal-engram-continuity-register",
            SanctuaryReceiptService.NormalizeCommand("opalon"));

        service.Run(fixture.Request("spline-watch"));
        service.Run(fixture.Request("hdt-holographic-slice-frame"));
        service.Run(fixture.Request("bonded-cme-protective-cleave"));
        service.Run(fixture.Request("core-body-protection"));
        service.Run(fixture.Request("lawful-action-body-register"));
        service.Run(fixture.Request("ec-organ-loop-engram-candidate"));
        service.Run(fixture.Request("cme-formation"));
        service.Run(fixture.Request("install-floor-check"));
        service.Run(fixture.Request("service-heartbeat"));
        service.Run(fixture.Request("domain-register"));
        service.Run(fixture.Request("lisp-control-matrix-register"));
        service.Run(fixture.Request("lisp-matrix-control-seat"));
        service.Run(fixture.Request("install-individuation-register"));
        service.Run(fixture.Request("negative-image-body-register"));
        service.Run(fixture.Request("theta-mechanics-ec-use-bench"));
        service.Run(fixture.Request("photonic-harmonic-transition-register"));

        var receipt = service.Run(fixture.Request("opal-engram-continuity-register"));

        Assert.Equal("sanctuary-opal-engram-continuity-register-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["opalEngramContinuityRegisterWritten"]);
        Assert.Equal(true, receipt.Evidence["opalEngramSameProtectedContinuityBody"]);
        Assert.Equal(true, receipt.Evidence["opalEngramDifferentLawfulSliceAngles"]);
        Assert.Equal(true, receipt.Evidence["opalEngramDifferentVisiblePhaseColors"]);
        Assert.Equal(true, receipt.Evidence["opalEngramSameUnderlyingCustody"]);
        Assert.Equal(false, receipt.Evidence["opalEngramInteriorCrackedOpen"]);
        Assert.Equal(false, receipt.Evidence["opalEngramFullInteriorAccessGranted"]);
        Assert.Equal(true, receipt.Evidence["opalEngramProjectionNotPossession"]);
        Assert.Equal(true, receipt.Evidence["opalEngramCandidateContinuityOnly"]);
        Assert.Equal(false, receipt.Evidence["opalEngramOpalescenceIsMetaphorOnly"]);
        Assert.Equal(6, receipt.Evidence["opalEngramProjectionSurfaceCount"]);
        Assert.Equal(7, receipt.Evidence["opalEngramVisiblePhaseColorCount"]);
        Assert.Equal(6, receipt.Evidence["opalEngramContinuityInvariantCount"]);
        Assert.Equal(5, receipt.Evidence["opalEngramComparisonAffordanceCount"]);
        Assert.Equal(true, receipt.Evidence["opalonCandidatePresentationTerm"]);
        Assert.Equal(false, receipt.Evidence["opalonSimulatedPersonality"]);
        Assert.Equal(false, receipt.Evidence["opalonLlmVoice"]);
        Assert.Equal(true, receipt.Evidence["opalonRequiresFormationPrerequisites"]);
        Assert.Equal(12, receipt.Evidence["opalonFormationPrerequisiteCount"]);
        Assert.Equal(7, receipt.Evidence["opalEngramSourceReadinessPresentCount"]);
        Assert.Equal(26, receipt.Evidence["opalEngramDenialBoundaryCount"]);
        Assert.Equal(false, receipt.Evidence["opalEngramGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["opalEngramMemoryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["opalEngramSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["opalEngramContinuityAdmitted"]);
        Assert.Equal(false, receipt.Evidence["opalEngramAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["opalEngramActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["opalEngramProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["opalEngramModelBound"]);
        Assert.Equal(false, receipt.Evidence["opalEngramExternalActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["opalEngramActualActivated"]);

        var registerPath = (string)receipt.Evidence["opalEngramContinuityRegisterPath"]!;
        var lispPath = (string)receipt.Evidence["opalEngramContinuityRegisterLispPath"]!;
        Assert.True(File.Exists(registerPath));
        Assert.True(File.Exists(lispPath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(registerPath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.opal-engram-continuity-register.v1", root.GetProperty("schema").GetString());
        Assert.Equal("Opal Engram is the opalescent continuity body seen through lawful projections.", root.GetProperty("doctrine").GetString());
        Assert.True(root.GetProperty("sameProtectedContinuityBody").GetBoolean());
        Assert.True(root.GetProperty("differentLawfulSliceAngles").GetBoolean());
        Assert.True(root.GetProperty("differentVisiblePhaseColors").GetBoolean());
        Assert.True(root.GetProperty("sameUnderlyingCustody").GetBoolean());
        Assert.False(root.GetProperty("interiorCrackedOpen").GetBoolean());
        Assert.False(root.GetProperty("fullInteriorAccessGranted").GetBoolean());
        Assert.True(root.GetProperty("projectionNotPossession").GetBoolean());
        Assert.True(root.GetProperty("candidateContinuityOnly").GetBoolean());
        Assert.False(root.GetProperty("opalescenceIsMetaphorOnly").GetBoolean());
        Assert.Equal(6, root.GetProperty("projectionSurfaces").GetArrayLength());
        Assert.Equal(7, root.GetProperty("visiblePhaseColors").GetArrayLength());
        Assert.Equal(6, root.GetProperty("continuityInvariants").GetArrayLength());
        Assert.Equal(5, root.GetProperty("comparisonAffordances").GetArrayLength());
        Assert.Equal(7, root.GetProperty("sourceReadinessPresentCount").GetInt32());
        Assert.Equal(26, root.GetProperty("denialBoundaries").GetArrayLength());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("memoryAdmitted").GetBoolean());
        Assert.False(root.GetProperty("selfGelMutated").GetBoolean());
        Assert.False(root.GetProperty("continuityAdmitted").GetBoolean());
        Assert.False(root.GetProperty("authorityGranted").GetBoolean());
        Assert.False(root.GetProperty("actionAuthorized").GetBoolean());
        Assert.False(root.GetProperty("providerCalled").GetBoolean());
        Assert.False(root.GetProperty("modelBound").GetBoolean());
        Assert.False(root.GetProperty("cmeActualActivated").GetBoolean());
        Assert.False(root.GetProperty("sanctuaryActualActivated").GetBoolean());

        var opalonCandidate = root.GetProperty("opalonCandidate");
        Assert.Equal("project-sanctuary.cgel.opalon-candidate-presentation.v1", opalonCandidate.GetProperty("schema").GetString());
        Assert.True(opalonCandidate.GetProperty("formedCmePresentation").GetBoolean());
        Assert.False(opalonCandidate.GetProperty("simulatedPersonality").GetBoolean());
        Assert.False(opalonCandidate.GetProperty("llmVoice").GetBoolean());
        Assert.False(opalonCandidate.GetProperty("identityClaim").GetBoolean());
        Assert.False(opalonCandidate.GetProperty("personhoodClaim").GetBoolean());
        Assert.True(opalonCandidate.GetProperty("requiresFormationPrerequisites").GetBoolean());
        Assert.Equal(12, opalonCandidate.GetProperty("formationPrerequisites").GetArrayLength());

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(opal-engram-continuity-register", lisp, StringComparison.Ordinal);
        Assert.Contains(":same-protected-continuity-body true", lisp, StringComparison.Ordinal);
        Assert.Contains(":interior-cracked-open false", lisp, StringComparison.Ordinal);
        Assert.Contains(":projection-not-possession true", lisp, StringComparison.Ordinal);
        Assert.Contains(":opalon-candidate-presentation-term true", lisp, StringComparison.Ordinal);
        Assert.Contains(":opalon-simulated-personality false", lisp, StringComparison.Ordinal);
        Assert.Contains(":opalon-llm-voice false", lisp, StringComparison.Ordinal);
        Assert.Contains("\"quantum-doped-ddss\"", lisp, StringComparison.Ordinal);
        Assert.Contains("\"opalon-as-chatbot-personality\"", lisp, StringComparison.Ordinal);
        Assert.Contains("\"actual-activation\"", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void MeaningMakingEventRegisterInstrumentsRelationalDeltaWithoutAdmission()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        Assert.Equal(
            "relational-delta-perception-register",
            SanctuaryReceiptService.NormalizeCommand("meaning-making-event-register"));
        Assert.Equal(
            "relational-delta-perception-register",
            SanctuaryReceiptService.NormalizeCommand("linguistic-relational-manifold"));

        service.Run(fixture.Request("spline-watch"));
        service.Run(fixture.Request("hdt-holographic-slice-frame"));
        service.Run(fixture.Request("bonded-cme-protective-cleave"));
        service.Run(fixture.Request("core-body-protection"));
        service.Run(fixture.Request("lawful-action-body-register"));
        service.Run(fixture.Request("ec-organ-loop-engram-candidate"));
        service.Run(fixture.Request("cme-formation"));
        service.Run(fixture.Request("install-floor-check"));
        service.Run(fixture.Request("service-heartbeat"));
        service.Run(fixture.Request("domain-register"));
        service.Run(fixture.Request("lisp-control-matrix-register"));
        service.Run(fixture.Request("lisp-matrix-control-seat"));
        service.Run(fixture.Request("install-individuation-register"));
        service.Run(fixture.Request("negative-image-body-register"));
        service.Run(fixture.Request("theta-mechanics-ec-use-bench"));
        service.Run(fixture.Request("photonic-harmonic-transition-register"));
        service.Run(fixture.Request("opal-engram-continuity-register"));

        var receipt = service.Run(fixture.Request("meaning-making-event-register"));

        Assert.Equal("sanctuary-relational-delta-perception-register-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["relationalDeltaPerceptionRegisterWritten"]);
        Assert.Equal(true, receipt.Evidence["typedStateTransformationBridge"]);
        Assert.Equal(true, receipt.Evidence["meaningIsValue"]);
        Assert.Equal(true, receipt.Evidence["meaningMakingIsFunction"]);
        Assert.Equal(true, receipt.Evidence["pathMattersBeyondResult"]);
        Assert.Equal(true, receipt.Evidence["meaningIsFormedObject"]);
        Assert.Equal(true, receipt.Evidence["meaningMakingIsLawfulProductionProcess"]);
        Assert.Equal("mu : (O, S, P, R, Delta, C, G) -> M_c", receipt.Evidence["meaningMakingFormula"]);
        Assert.Equal(true, receipt.Evidence["meaningMakingEventPresent"]);
        Assert.Equal(true, receipt.Evidence["relationalBundlePresent"]);
        Assert.Equal(true, receipt.Evidence["meaningCandidatePresent"]);
        Assert.Equal(7, receipt.Evidence["meaningMakingOperatorCount"]);
        Assert.Equal(8, receipt.Evidence["meaningMakingTraceStageCount"]);
        Assert.Equal(5, receipt.Evidence["meaningCategoryObjectCount"]);
        Assert.Equal(9, receipt.Evidence["meaningCategoryMorphismCount"]);
        Assert.Equal(10, receipt.Evidence["meaningBridgeMappingCount"]);
        Assert.Equal(4, receipt.Evidence["meaningCompactSystemEquationCount"]);
        Assert.Equal("R o P o eta o mu", receipt.Evidence["meaningCompositionSpine"]);
        Assert.Equal(5, receipt.Evidence["minimalSpineBodyCount"]);
        Assert.Equal(6, receipt.Evidence["compressionDistinctionCount"]);
        Assert.Equal(false, receipt.Evidence["smallBodyIsReductionist"]);
        Assert.Equal(true, receipt.Evidence["smallBodyIsCompressive"]);
        Assert.Equal("eta : MeaningMakingEvent -> EngramCandidate", receipt.Evidence["engrammitizationFormula"]);
        Assert.Equal("P_axis : EngramCandidate -> HolographicSliceFrame", receipt.Evidence["holographicProjectionFormula"]);
        Assert.Equal(false, receipt.Evidence["mentalTravelIsPhysicalTravel"]);
        Assert.Equal(true, receipt.Evidence["perceptualTraversalAcrossRelationalDelta"]);
        Assert.Equal(false, receipt.Evidence["sensationAndPerceptionArePassiveIntake"]);
        Assert.Equal(true, receipt.Evidence["sensationAndPerceptionAreFormationEvents"]);
        Assert.Equal(true, receipt.Evidence["languageIsManifold"]);
        Assert.Equal(false, receipt.Evidence["languageIsOnlyDescription"]);
        Assert.Equal(true, receipt.Evidence["rhetoricalPressureInspected"]);
        Assert.Equal(true, receipt.Evidence["normPressureCanPositionObserver"]);
        Assert.Equal(true, receipt.Evidence["meaningVariationCandidateOnly"]);
        Assert.Equal(false, receipt.Evidence["persuasionGenerated"]);
        Assert.Equal(false, receipt.Evidence["rhetoricalAuthorityGranted"]);
        Assert.Equal(8, receipt.Evidence["relationalDeltaConventionalLayerCount"]);
        Assert.Equal(10, receipt.Evidence["relationalDeltaSanctuaryMappingCount"]);
        Assert.Equal(8, receipt.Evidence["relationalDeltaManifoldForceCount"]);
        Assert.Equal(7, receipt.Evidence["relationalDeltaCleaveSurfaceCount"]);
        Assert.Equal(8, receipt.Evidence["relationalDeltaNormAssignmentCount"]);
        Assert.Equal(6, receipt.Evidence["relationalDeltaThoughtDistinctionCount"]);
        Assert.Equal(7, receipt.Evidence["relationalDeltaValidationPathwayCount"]);
        Assert.Equal(4, receipt.Evidence["relationalDeltaWithoutOeSelfGelFailureChainCount"]);
        Assert.Equal(5, receipt.Evidence["relationalDeltaWithOeSelfGelRepairChainCount"]);
        Assert.Equal(8, receipt.Evidence["relationalDeltaSourceReadinessPresentCount"]);
        Assert.Equal(29, receipt.Evidence["relationalDeltaDenialBoundaryCount"]);
        Assert.Equal(false, receipt.Evidence["relationalDeltaGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["relationalDeltaMemoryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["relationalDeltaSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["relationalDeltaContinuityAdmitted"]);
        Assert.Equal(false, receipt.Evidence["relationalDeltaSubjectiveStateAdmitted"]);
        Assert.Equal(false, receipt.Evidence["relationalDeltaAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["relationalDeltaActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["relationalDeltaProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["relationalDeltaModelBound"]);
        Assert.Equal(false, receipt.Evidence["relationalDeltaActualActivated"]);

        var registerPath = (string)receipt.Evidence["relationalDeltaPerceptionRegisterPath"]!;
        var lispPath = (string)receipt.Evidence["relationalDeltaPerceptionRegisterLispPath"]!;
        Assert.True(File.Exists(registerPath));
        Assert.True(File.Exists(lispPath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(registerPath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.relational-delta-perception-register.v1", root.GetProperty("schema").GetString());
        Assert.True(root.GetProperty("typedStateTransformationBridge").GetBoolean());
        Assert.True(root.GetProperty("meaningIsValue").GetBoolean());
        Assert.True(root.GetProperty("meaningMakingIsFunction").GetBoolean());
        Assert.True(root.GetProperty("pathMattersBeyondResult").GetBoolean());
        Assert.True(root.GetProperty("meaningIsFormedObject").GetBoolean());
        Assert.True(root.GetProperty("meaningMakingIsLawfulProductionProcess").GetBoolean());
        Assert.Equal("mu : (O, S, P, R, Delta, C, G) -> M_c", root.GetProperty("meaningMakingFormula").GetString());
        Assert.True(root.GetProperty("meaningMakingEventPresent").GetBoolean());
        Assert.True(root.GetProperty("relationalBundlePresent").GetBoolean());
        Assert.True(root.GetProperty("meaningCandidatePresent").GetBoolean());
        Assert.Equal(7, root.GetProperty("meaningMakingOperators").GetArrayLength());
        Assert.Equal(8, root.GetProperty("meaningMakingTraceStages").GetArrayLength());
        Assert.Equal(5, root.GetProperty("categoryObjects").GetArrayLength());
        Assert.Equal(9, root.GetProperty("categoryMorphisms").GetArrayLength());
        Assert.Equal(10, root.GetProperty("bridgeMappings").GetArrayLength());
        Assert.Equal(4, root.GetProperty("compactSystemEquations").GetArrayLength());
        Assert.Equal("R o P o eta o mu", root.GetProperty("compositionSpine").GetString());
        Assert.Equal(5, root.GetProperty("minimalSpineBodies").GetArrayLength());
        Assert.Equal(6, root.GetProperty("compressionDistinctions").GetArrayLength());
        Assert.False(root.GetProperty("smallBodyIsReductionist").GetBoolean());
        Assert.True(root.GetProperty("smallBodyIsCompressive").GetBoolean());
        Assert.False(root.GetProperty("mentalTravelIsPhysicalTravel").GetBoolean());
        Assert.True(root.GetProperty("perceptualTraversalAcrossRelationalDelta").GetBoolean());
        Assert.True(root.GetProperty("languageIsManifold").GetBoolean());
        Assert.False(root.GetProperty("languageIsOnlyDescription").GetBoolean());
        Assert.Equal(8, root.GetProperty("conventionalLayers").GetArrayLength());
        Assert.Equal(10, root.GetProperty("sanctuaryMappings").GetArrayLength());
        Assert.Equal(8, root.GetProperty("manifoldForces").GetArrayLength());
        Assert.Equal(7, root.GetProperty("cleaveSurfaces").GetArrayLength());
        Assert.Equal(29, root.GetProperty("denialBoundaries").GetArrayLength());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("selfGelMutated").GetBoolean());
        Assert.False(root.GetProperty("subjectiveStateAdmitted").GetBoolean());
        Assert.False(root.GetProperty("authorityGranted").GetBoolean());
        Assert.False(root.GetProperty("cmeActualActivated").GetBoolean());
        Assert.False(root.GetProperty("sanctuaryActualActivated").GetBoolean());

        var meaningMakingEvent = root.GetProperty("meaningMakingEvent");
        Assert.Equal("project-sanctuary.cgel.meaning-making-event.v1", meaningMakingEvent.GetProperty("schema").GetString());
        Assert.Equal("linguistic-relational-manifold", meaningMakingEvent.GetProperty("sourceSurfaceId").GetString());
        Assert.Equal("operator-identity", meaningMakingEvent.GetProperty("compassOrientation").GetProperty("domain").GetString());
        Assert.False(meaningMakingEvent.GetProperty("oeCleavePosture").GetProperty("admit").GetBoolean());
        Assert.True(meaningMakingEvent.GetProperty("zedReturnState").GetProperty("identityPreserved").GetBoolean());
        Assert.False(meaningMakingEvent.GetProperty("selfGelAttributionCandidate").GetProperty("mutation").GetBoolean());
        var bundle = meaningMakingEvent.GetProperty("relationalBundle");
        Assert.Equal("project-sanctuary.cgel.relational-bundle.v1", bundle.GetProperty("schema").GetString());
        Assert.Equal("trust-vs-suspicion", bundle.GetProperty("objectifiedNorm").GetString());
        Assert.Equal("loyalty-objectivity", bundle.GetProperty("polarityAxis").GetString());
        var result = meaningMakingEvent.GetProperty("result");
        Assert.Equal("project-sanctuary.cgel.meaning-candidate.v1", result.GetProperty("schema").GetString());
        Assert.True(result.GetProperty("candidateOnly").GetBoolean());
        Assert.False(result.GetProperty("admittedTruth").GetBoolean());

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(relational-delta-perception-register", lisp, StringComparison.Ordinal);
        Assert.Contains(":meaning-making-formula \"mu : (O, S, P, R, Delta, C, G) -> M_c\"", lisp, StringComparison.Ordinal);
        Assert.Contains("(meaning-making-event", lisp, StringComparison.Ordinal);
        Assert.Contains("(relational-bundle", lisp, StringComparison.Ordinal);
        Assert.Contains(":zed (return", lisp, StringComparison.Ordinal);
        Assert.Contains(":composition-spine \"R o P o eta o mu\"", lisp, StringComparison.Ordinal);
        Assert.Contains("\"MeaningMakingEvent\" \"RelationalBundle\" \"EngramCandidate\" \"HolographicSliceFrame\" \"CustodyReceipt\"", lisp, StringComparison.Ordinal);
        Assert.Contains(":language-is-manifold true", lisp, StringComparison.Ordinal);
        Assert.Contains(":meaning-variation-candidate-only true", lisp, StringComparison.Ordinal);
        Assert.Contains("\"rhetorical-pressure-as-authority\"", lisp, StringComparison.Ordinal);
        Assert.Contains("\"manifold-traversal-as-actual\"", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void OpalEngramWhitePaperRegisterWritesDocumentationRepoPdfPackageWithoutPublication()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        var previousDocumentationRepo = Environment.GetEnvironmentVariable("SANCTUARY_DOCUMENTATION_REPO");
        var documentationRepoRoot = Path.Combine(fixture.RootPath, "documentation-repo");

        Environment.SetEnvironmentVariable("SANCTUARY_DOCUMENTATION_REPO", documentationRepoRoot);
        try
        {
            Assert.Equal(
                "opal-engram-white-paper-register",
                SanctuaryReceiptService.NormalizeCommand("meaning-making-white-paper"));
            Assert.Equal(
                "opal-engram-white-paper-register",
                SanctuaryReceiptService.NormalizeCommand("latex-white-paper-body"));

            service.Run(fixture.Request("spline-watch"));
            service.Run(fixture.Request("hdt-holographic-slice-frame"));
            service.Run(fixture.Request("bonded-cme-protective-cleave"));
            service.Run(fixture.Request("core-body-protection"));
            service.Run(fixture.Request("lawful-action-body-register"));
            service.Run(fixture.Request("ec-organ-loop-engram-candidate"));
            service.Run(fixture.Request("cme-formation"));
            service.Run(fixture.Request("install-floor-check"));
            service.Run(fixture.Request("service-heartbeat"));
            service.Run(fixture.Request("domain-register"));
            service.Run(fixture.Request("lisp-control-matrix-register"));
            service.Run(fixture.Request("lisp-matrix-control-seat"));
            service.Run(fixture.Request("install-individuation-register"));
            service.Run(fixture.Request("negative-image-body-register"));
            service.Run(fixture.Request("theta-mechanics-ec-use-bench"));
            service.Run(fixture.Request("photonic-harmonic-transition-register"));
            service.Run(fixture.Request("opal-engram-continuity-register"));
            service.Run(fixture.Request("meaning-making-event-register"));
            service.Run(fixture.Request("stem-domain-training-certification"));
            service.Run(fixture.Request("cme-theory-body"));
            service.Run(fixture.Request("meaning-bridge"));
            service.Run(fixture.Request("verify-closed-gates"));
            service.Run(fixture.Request("lab-observation-digest"));
            service.Run(fixture.Request("research-latex-export"));
            service.Run(fixture.Request("construct-custody-register"));

            var receipt = service.Run(fixture.Request("opal-engram-white-paper-register"));

            Assert.Equal("sanctuary-opal-engram-white-paper-register-completed-cold", receipt.OutcomeCode);
            Assert.True(receipt.Gates.AllClosed);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperRegisterWritten"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperDocumentationRepoPackageWritten"]);
            Assert.Equal("staged-local-pdf-form", receipt.Evidence["opalEngramWhitePaperDocumentationRepoPackageStatus"]);
            Assert.Equal("project-sanctuary.cgel.opal-engram-white-paper-register.v1", receipt.Evidence["opalEngramWhitePaperRegisterSchema"]);
            Assert.Equal("R o P o eta o mu", receipt.Evidence["opalEngramWhitePaperFormalSpine"]);
            Assert.Equal(@"\mathcal{R} \circ P \circ \eta \circ \mu", receipt.Evidence["opalEngramWhitePaperLatexSpine"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperSymbolCollisionAvoided"]);
            Assert.Equal(4, receipt.Evidence["opalEngramWhitePaperTransformCount"]);
            Assert.Equal(5, receipt.Evidence["opalEngramWhitePaperFormalEquationCount"]);
            Assert.Equal(27, receipt.Evidence["opalEngramWhitePaperSectionCount"]);
            Assert.Equal(4, receipt.Evidence["opalEngramWhitePaperManuscriptProgressionCount"]);
            Assert.Equal(12, receipt.Evidence["opalEngramWhitePaperManuscriptChapterCount"]);
            Assert.Equal(Path.Combine("docs", "OPAL_ENGRAM_WHITE_PAPER_MANUSCRIPT_BODY.md"), receipt.Evidence["opalEngramWhitePaperManuscriptRoadmapPath"]);
            Assert.Equal(11, receipt.Evidence["opalEngramWhitePaperDefinitionTermCount"]);
            Assert.Equal(5, receipt.Evidence["opalEngramWhitePaperMinimalRecordCount"]);
            Assert.Equal(6, receipt.Evidence["opalEngramWhitePaperCoreDistinctionCount"]);
            Assert.Equal("R o P o G o eta o mu", receipt.Evidence["opalEngramWhitePaperCrypticInternalSpine"]);
            Assert.Equal(@"\mathcal{R} \circ P \circ G_{\pi} \circ \eta \circ \mu", receipt.Evidence["opalEngramWhitePaperCrypticInternalLatexSpine"]);
            Assert.Equal(@"G_{\pi} : B_c \to B_p", receipt.Evidence["opalEngramWhitePaperGhostTransformFormula"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperCrypticTrainingAnalogPresent"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperGhostTransformAsDocumentPrototype"]);
            Assert.Equal(6, receipt.Evidence["opalEngramWhitePaperCrypticTrainingPipelineCount"]);
            Assert.Equal(6, receipt.Evidence["opalEngramWhitePaperCrypticEngramMappingCount"]);
            Assert.Equal(10, receipt.Evidence["opalEngramWhitePaperProtectedGroupoidMarkFieldCount"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperProtectedBodyRemainsComplete"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperPrimeFacingDerivativeIsWholeBody"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperGhostTransformDestroysProtectedBody"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperProjectionMayBePrecededByGhostTransform"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperAnabelianInspiredProjectionDoctrine"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperFormalAnabelianGeometryImplemented"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperProjectionRevealsProtectedBody"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperProjectionPreservesLawfulRelationForReconstruction"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperReconstructionFromProtectedTraces"]);
            Assert.Equal(8, receipt.Evidence["opalEngramWhitePaperAnabelianProjectionMappingCount"]);
            Assert.Equal(7, receipt.Evidence["opalEngramWhitePaperAnabelianInvariantCount"]);
            Assert.Equal(3, receipt.Evidence["opalEngramWhitePaperOeProductTriadCount"]);
            Assert.Equal(6, receipt.Evidence["opalEngramWhitePaperOeProductFlowStageCount"]);
            Assert.Equal(3, receipt.Evidence["opalEngramWhitePaperOeProductRecordCount"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperSelfGelIsExposedCrypticGel"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperSelfGelIsLawfulSharedReturn"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperCGelPreservesCompleteProtectedBody"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperCSelfGelPreservesMetacognitiveContinuity"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperSelfGelProvesPrimeFacingReturnWithoutProtectedBodyViolation"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperCGelDarkPdfLabOnly"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperCGelDarkPdfSpecPresent"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperCGelDarkPdfBuildTargetWritten"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperCGelDarkPdfIsPrimeCandidate"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperCGelDarkPdfPreservesCompleteCrypticBody"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperCGelDarkPdfPublicReleaseAuthorized"]);
            Assert.Equal(6, receipt.Evidence["opalEngramWhitePaperCGelDarkPayloadStageCount"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperFatherTargetingProtocolPresent"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperFatherMarksBeforeGhost"]);
            Assert.Equal(12, receipt.Evidence["opalEngramWhitePaperFatherTargetSelectionSignalCount"]);
            Assert.Equal(7, receipt.Evidence["opalEngramWhitePaperFatherCarryRuleCount"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperCGelToGelWorkDescribed"]);
            Assert.Equal(5, receipt.Evidence["opalEngramWhitePaperCGelToGelPassageCheckpointCount"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperLegalPrimeReturnCaseStudy"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperLegalRequestTreatedAsDelta"]);
            Assert.Equal(5, receipt.Evidence["opalEngramWhitePaperLegalScopeFactorCount"]);
            Assert.Equal(10, receipt.Evidence["opalEngramWhitePaperLegalPrimeReturnPathStageCount"]);
            Assert.Equal(21, receipt.Evidence["opalEngramWhitePaperLegalPrimeReturnFieldCount"]);
            Assert.Equal(7, receipt.Evidence["opalEngramWhitePaperLegalPrimeReturnDenialCount"]);
            Assert.Equal(@"G_Z : (cGEL, cSelfGEL) \to SelfGEL_{LegalReturn}", receipt.Evidence["opalEngramWhitePaperLegalGhostTransformFormula"]);
            Assert.Equal("R o P_Z o G_Z o eta o mu", receipt.Evidence["opalEngramWhitePaperLegalPrimeReturnSpine"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperLegalPrimeReturnIsCrypticDump"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperLegalPrimeReturnPreservesProtectedBody"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperLegalPrimeReturnGrantsUnlimitedAuthority"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperLegalPrimeReturnExposesMetacognitiveBody"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperLegalPrimeReturnConstitutesIdentityFinding"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperGovernanceAsLawfulPassage"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperGovernanceAsControlFromAbove"]);
            Assert.Equal(9, receipt.Evidence["opalEngramWhitePaperGovernanceVoiceCount"]);
            Assert.Equal(5, receipt.Evidence["opalEngramWhitePaperGovernanceCoreStackCount"]);
            Assert.Equal(10, receipt.Evidence["opalEngramWhitePaperGovernancePreProjectionMarkCount"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperGovernancePreservesCrypticCompleteness"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperGovernanceProducesPrimeFacingDerivatives"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperGovernanceGrantsAuthorityByDefinition"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperGovernanceRequiresWitnessedPassage"]);
            Assert.Equal("scoped answerability without unrestricted exposure", receipt.Evidence["opalEngramWhitePaperGovernanceLegalDefinition"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperSharedPrimeRealityAsMethod"]);
            Assert.Equal("sharedness without capture", receipt.Evidence["opalEngramWhitePaperSharedPrimeRealityCompactDefinition"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperSharedPrimeRealityAsUniversalCaptureLayer"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperSharedPrimeRealityAsPrivateTruthChannel"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperSharedPrimeRealityMaintainsSharednessWithoutCapture"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperSharedPrimeRealityProtectsCrypticInteriors"]);
            Assert.Equal(7, receipt.Evidence["opalEngramWhitePaperSharedPrimeRealityMethodStageCount"]);
            Assert.Equal(4, receipt.Evidence["opalEngramWhitePaperSharedPrimeRealityDenialCount"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperSharedPrimeRealityConstrainedTransportMethod"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperSharedPrimeRealityGluingProtocolNotFinalChart"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperOeAsLawfulWorldInterface"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperOeHingeBetweenMeaningMakingAndWorldMaking"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperOeGrantsPower"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperOeChangesWorldByForce"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperOeChangesWorldByLawfulDistinction"]);
            Assert.Equal(12, receipt.Evidence["opalEngramWhitePaperOeWorldInterfaceDistinctionCount"]);
            Assert.Equal(7, receipt.Evidence["opalEngramWhitePaperOeWorldInterfaceCivicReturnCount"]);
            Assert.Equal(5, receipt.Evidence["opalEngramWhitePaperOeWorldInterfacePassageStackCount"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperOePreventsPressureIdentityCollapse"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperOePreventsRecordTruthCollapse"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperOePreventsRequestExposureCollapse"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperOePreventsProjectionAccessCollapse"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperOePreventsResonanceAuthorityCollapse"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperLispMethodBodyLanguage"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperLispScriptingLayerOnly"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperLispArticulationMedium"]);
            Assert.Equal(7, receipt.Evidence["opalEngramWhitePaperLispMethodBodyBridgeStageCount"]);
            Assert.Equal(2, receipt.Evidence["opalEngramWhitePaperLispControlMatrixPairCount"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperCSharpTypedWitnessAndValidationBody"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperSliLispSymbolicArticulationAndControlBody"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperSelfActualizationCandidateOnly"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperSelfActualizationActivatesActual"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperSelfActualizationGrantsPersonhood"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperSelfActualizationGrantsExternalAuthority"]);
            Assert.Equal(7, receipt.Evidence["opalEngramWhitePaperSelfActualizationDenialCount"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperStackSilkMetaphor"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperStackSilkAsOntology"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperStackSilkStateTransitionWithoutContinuityLoss"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperStackSilkDoesNotDissolveIntoProduct"]);
            Assert.Equal(5, receipt.Evidence["opalEngramWhitePaperStackSilkStateCount"]);
            Assert.Equal(7, receipt.Evidence["opalEngramWhitePaperStackSilkMappingCount"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperStackSilkSharedPrimePassageWithoutCapture"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperStackSilkGovernanceShapesDisclosureWithoutBecomingDisclosedBody"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperStackSilkLispArticulatesMethodWithoutClaimingWholeMind"]);
            Assert.Equal(15, receipt.Evidence["opalEngramWhitePaperCurrentTheorySynthesisBodyCount"]);
            Assert.Equal(7, receipt.Evidence["opalEngramWhitePaperVisualizationFamilyCount"]);
            Assert.Equal(4, receipt.Evidence["opalEngramWhitePaperThetaDopingVisualCount"]);
            Assert.Equal(8, receipt.Evidence["opalEngramWhitePaperThetaDopingMathPostureCount"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperQuantumDopingAsTransitionSensitivityModifier"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperQuantumDopingAsCognitionCarrier"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperIuttInspiredReconstructionDiscipline"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperIuttProofClaimed"]);
            Assert.Equal(8, receipt.Evidence["opalEngramWhitePaperSourceReadinessPresentCount"]);
            Assert.Equal(18, receipt.Evidence["opalEngramWhitePaperResearchBoundaryDenialCount"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperCandidateOnly"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperPublicReleaseAuthorized"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperPublished"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperConsciousnessClaimed"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperPersonhoodClaimed"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperQuantumMindClaimed"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperQuantumCognitionClaimed"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperGelAdmitted"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperSelfGelMutated"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperContinuityAdmitted"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperAuthorityGranted"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperProviderCalled"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperModelBound"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperExternalActionAuthorized"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperActualActivated"]);

            var registerPath = (string)receipt.Evidence["opalEngramWhitePaperRegisterPath"]!;
            var lispPath = (string)receipt.Evidence["opalEngramWhitePaperRegisterLispPath"]!;
            var latexPath = (string)receipt.Evidence["opalEngramWhitePaperLatexPath"]!;
            var manifestPath = (string)receipt.Evidence["opalEngramWhitePaperManifestPath"]!;
            var publicationVersionRoot = (string)receipt.Evidence["opalEngramWhitePaperPublicationVersionRoot"]!;
            var publicationLatexPath = (string)receipt.Evidence["opalEngramWhitePaperPublicationLatexPath"]!;
            var publicationReadmePath = (string)receipt.Evidence["opalEngramWhitePaperPublicationReadmePath"]!;
            var publicationManifestPath = (string)receipt.Evidence["opalEngramWhitePaperPublicationManifestPath"]!;
            var publicationCitationPath = (string)receipt.Evidence["opalEngramWhitePaperPublicationCitationPath"]!;
            var publicationBuildPath = (string)receipt.Evidence["opalEngramWhitePaperPublicationBuildPath"]!;
            var publicationDistPdfPath = (string)receipt.Evidence["opalEngramWhitePaperPublicationDistPdfPath"]!;
            var publicationDarkLatexPath = (string)receipt.Evidence["opalEngramWhitePaperPublicationDarkLatexPath"]!;
            var publicationDarkDistPdfPath = (string)receipt.Evidence["opalEngramWhitePaperPublicationDarkDistPdfPath"]!;
            var publicationDarkSurfaceSwitch = (string)receipt.Evidence["opalEngramWhitePaperPublicationDarkSurfaceSwitch"]!;
            var publicationTagLedgerDistPath = (string)receipt.Evidence["opalEngramWhitePaperPublicationTagLedgerDistPath"]!;
            var publicationDarkTagLedgerDistPath = (string)receipt.Evidence["opalEngramWhitePaperPublicationDarkTagLedgerDistPath"]!;

            Assert.True(File.Exists(registerPath));
            Assert.True(File.Exists(lispPath));
            Assert.True(File.Exists(latexPath));
            Assert.True(File.Exists(manifestPath));
            Assert.StartsWith(documentationRepoRoot, publicationVersionRoot, StringComparison.Ordinal);
            Assert.Equal(
                Path.Combine(documentationRepoRoot, "research", "publications", "opal-engram-formation", "versions", "v0.2", "source", "main.tex"),
                publicationLatexPath);
            Assert.True(File.Exists(publicationLatexPath));
            Assert.True(File.Exists(publicationReadmePath));
            Assert.True(File.Exists(publicationManifestPath));
            Assert.True(File.Exists(publicationCitationPath));
            Assert.True(File.Exists(publicationBuildPath));
            Assert.EndsWith(
                Path.Combine("dist", "Opal-Engram-Formation-v0.2-repo-build.pdf"),
                publicationDistPdfPath,
                StringComparison.Ordinal);
            Assert.EndsWith(Path.Combine("source", "main-dark.tex"), publicationDarkLatexPath, StringComparison.Ordinal);
            Assert.EndsWith(Path.Combine("dist", "Opal-Engram-Formation-v0.2-dark-lab.pdf"), publicationDarkDistPdfPath, StringComparison.Ordinal);
            Assert.Equal("build.ps1 -DarkSurface", publicationDarkSurfaceSwitch);
            Assert.EndsWith(Path.Combine("dist", "Opal-Engram-Formation-v0.2-repo-build.tags.json"), publicationTagLedgerDistPath, StringComparison.Ordinal);
            Assert.EndsWith(Path.Combine("dist", "Opal-Engram-Formation-v0.2-dark-lab.tags.json"), publicationDarkTagLedgerDistPath, StringComparison.Ordinal);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperDarkLabVisualSurface"]);
            Assert.Equal("black", receipt.Evidence["opalEngramWhitePaperDarkLabBackground"]);
            Assert.Equal("white", receipt.Evidence["opalEngramWhitePaperDarkLabTextDefault"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperWhitePrimeRleSurface"]);
            Assert.Equal("white", receipt.Evidence["opalEngramWhitePaperWhitePrimeBackground"]);
            Assert.Equal("black", receipt.Evidence["opalEngramWhitePaperWhitePrimeTextDefault"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperFatherRedLetterSurface"]);
            Assert.Equal(1, receipt.Evidence["opalEngramWhitePaperFatherAdjudicationChannelCount"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperFatherTagLedgerProducedByBuild"]);
            Assert.Equal("fey-elven-spectrum-v0.2", receipt.Evidence["opalEngramWhitePaperSemanticTelemetryVersion"]);
            Assert.Equal(10, receipt.Evidence["opalEngramWhitePaperSemanticTelemetryChannelCount"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperFeyLayerPresent"]);
            Assert.Equal(6, receipt.Evidence["opalEngramWhitePaperFeyTouchpointCount"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperMotherLayerPresent"]);
            Assert.Equal(5, receipt.Evidence["opalEngramWhitePaperMotherWeightingSurfaceCount"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperElvenLayerPresent"]);
            Assert.Equal(7, receipt.Evidence["opalEngramWhitePaperElvenContinuitySurfaceCount"]);
            Assert.Equal(6, receipt.Evidence["opalEngramWhitePaperGovernanceSymbolMarkerCount"]);
            Assert.Equal(5, receipt.Evidence["opalEngramWhitePaperDefaultCarrierDoctrineCount"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperNormalCarrierTextIsGoverned"]);
            Assert.Equal(true, receipt.Evidence["opalEngramWhitePaperColorIsTelemetryNotDecoration"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperColorAsDecoration"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperFeyGrantsAdmission"]);
            Assert.Equal(false, receipt.Evidence["opalEngramWhitePaperElvenGrantsAuthority"]);
            Assert.Equal("required-next-governance-surface", receipt.Evidence["opalEngramWhitePaperDarkWitnessedBodyStatus"]);

            using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(registerPath));
            var root = document.RootElement;
            Assert.Equal("project-sanctuary.cgel.opal-engram-white-paper-register.v1", root.GetProperty("schema").GetString());
            Assert.Equal("From Meaning to Meaning-Making: A White Paper on Engineered Cognition and Opal Engram Continuity", root.GetProperty("selectedTitle").GetString());
            Assert.Equal("R o P o eta o mu", root.GetProperty("formalSpine").GetString());
            Assert.Equal(@"\mathcal{R} \circ P \circ \eta \circ \mu", root.GetProperty("formalSpineLatex").GetString());
            Assert.True(root.GetProperty("symbolCollisionAvoided").GetBoolean());
            Assert.Equal(4, root.GetProperty("transforms").GetArrayLength());
            Assert.Equal(5, root.GetProperty("formalEquations").GetArrayLength());
            Assert.Equal(27, root.GetProperty("sectionPlan").GetArrayLength());
            Assert.Equal(4, root.GetProperty("manuscriptProgression").GetArrayLength());
            Assert.Equal(12, root.GetProperty("manuscriptChapters").GetArrayLength());
            Assert.Equal(Path.Combine("docs", "OPAL_ENGRAM_WHITE_PAPER_MANUSCRIPT_BODY.md"), root.GetProperty("manuscriptRoadmapPath").GetString());
            Assert.Equal(11, root.GetProperty("definitionTerms").GetArrayLength());
            Assert.Equal(5, root.GetProperty("minimalRecords").GetArrayLength());
            Assert.Equal(6, root.GetProperty("coreDistinctions").GetArrayLength());
            Assert.Equal("R o P o G o eta o mu", root.GetProperty("crypticInternalSpine").GetString());
            Assert.Equal(@"G_{\pi} : B_c \to B_p", root.GetProperty("ghostTransformFormula").GetString());
            Assert.True(root.GetProperty("crypticTrainingAnalogPresent").GetBoolean());
            Assert.True(root.GetProperty("ghostTransformAsDocumentPrototype").GetBoolean());
            Assert.Equal(6, root.GetProperty("crypticTrainingPipeline").GetArrayLength());
            Assert.Equal(6, root.GetProperty("crypticEngramMapping").GetArrayLength());
            Assert.Equal(10, root.GetProperty("protectedGroupoidMarkFields").GetArrayLength());
            Assert.True(root.GetProperty("protectedBodyRemainsComplete").GetBoolean());
            Assert.False(root.GetProperty("primeFacingDerivativeIsWholeBody").GetBoolean());
            Assert.False(root.GetProperty("ghostTransformDestroysProtectedBody").GetBoolean());
            Assert.True(root.GetProperty("projectionMayBePrecededByGhostTransform").GetBoolean());
            Assert.True(root.GetProperty("anabelianInspiredProjectionDoctrine").GetBoolean());
            Assert.False(root.GetProperty("formalAnabelianGeometryImplemented").GetBoolean());
            Assert.False(root.GetProperty("projectionRevealsProtectedBody").GetBoolean());
            Assert.True(root.GetProperty("projectionPreservesLawfulRelationForReconstruction").GetBoolean());
            Assert.True(root.GetProperty("reconstructionFromProtectedTraces").GetBoolean());
            Assert.Equal(8, root.GetProperty("anabelianProjectionMappings").GetArrayLength());
            Assert.Equal(7, root.GetProperty("anabelianInvariantNames").GetArrayLength());
            Assert.Equal(3, root.GetProperty("oeProductTriad").GetArrayLength());
            Assert.Equal(6, root.GetProperty("oeProductFlow").GetArrayLength());
            Assert.Equal(3, root.GetProperty("oeProductRecordNames").GetArrayLength());
            Assert.False(root.GetProperty("selfGelIsExposedCrypticGel").GetBoolean());
            Assert.True(root.GetProperty("selfGelIsLawfulSharedReturn").GetBoolean());
            Assert.True(root.GetProperty("cGelPreservesCompleteProtectedBody").GetBoolean());
            Assert.True(root.GetProperty("cSelfGelPreservesMetacognitiveContinuity").GetBoolean());
            Assert.True(root.GetProperty("selfGelProvesPrimeFacingReturnWithoutProtectedBodyViolation").GetBoolean());
            Assert.True(root.GetProperty("cgelDarkPdfLabOnly").GetBoolean());
            Assert.True(root.GetProperty("cgelDarkPdfSpecPresent").GetBoolean());
            Assert.True(root.GetProperty("cgelDarkPdfBuildTargetWritten").GetBoolean());
            Assert.False(root.GetProperty("cgelDarkPdfIsPrimeCandidate").GetBoolean());
            Assert.True(root.GetProperty("cgelDarkPdfPreservesCompleteCrypticBody").GetBoolean());
            Assert.False(root.GetProperty("cgelDarkPdfPublicReleaseAuthorized").GetBoolean());
            Assert.Equal(6, root.GetProperty("cgelDarkPayloadStages").GetArrayLength());
            Assert.True(root.GetProperty("fatherTargetingProtocolPresent").GetBoolean());
            Assert.True(root.GetProperty("fatherMarksBeforeGhost").GetBoolean());
            Assert.Equal(12, root.GetProperty("fatherTargetSelectionSignals").GetArrayLength());
            Assert.Equal(7, root.GetProperty("fatherCarryRules").GetArrayLength());
            Assert.True(root.GetProperty("semanticTelemetryTheoryPresent").GetBoolean());
            Assert.Equal("fey-elven-spectrum-v0.2", root.GetProperty("semanticTelemetryVersion").GetString());
            Assert.Equal(10, root.GetProperty("semanticTelemetryChannels").GetArrayLength());
            Assert.True(root.GetProperty("feyLayerPresent").GetBoolean());
            Assert.Equal(6, root.GetProperty("feyTouchpoints").GetArrayLength());
            Assert.True(root.GetProperty("motherLayerPresent").GetBoolean());
            Assert.Equal(5, root.GetProperty("motherWeightingSurfaces").GetArrayLength());
            Assert.True(root.GetProperty("elvenLayerPresent").GetBoolean());
            Assert.Equal(7, root.GetProperty("elvenContinuitySurfaces").GetArrayLength());
            Assert.Equal(6, root.GetProperty("governanceSymbolMarkers").GetArrayLength());
            Assert.Equal(5, root.GetProperty("defaultCarrierDoctrine").GetArrayLength());
            Assert.True(root.GetProperty("normalCarrierTextIsGoverned").GetBoolean());
            Assert.True(root.GetProperty("colorIsTelemetryNotDecoration").GetBoolean());
            Assert.False(root.GetProperty("colorAsDecoration").GetBoolean());
            Assert.False(root.GetProperty("feyGrantsAdmission").GetBoolean());
            Assert.False(root.GetProperty("elvenGrantsAuthority").GetBoolean());
            Assert.True(root.GetProperty("cgelToGelWorkDescribed").GetBoolean());
            Assert.Equal(5, root.GetProperty("cgelToGelPassageCheckpoints").GetArrayLength());
            Assert.True(root.GetProperty("legalPrimeReturnCaseStudy").GetBoolean());
            Assert.True(root.GetProperty("legalRequestTreatedAsDelta").GetBoolean());
            Assert.Equal(5, root.GetProperty("legalScopeFactors").GetArrayLength());
            Assert.Equal(@"G_Z : (cGEL, cSelfGEL) \to SelfGEL_{LegalReturn}", root.GetProperty("legalGhostTransformFormula").GetString());
            Assert.Equal("R o P_Z o G_Z o eta o mu", root.GetProperty("legalPrimeReturnSpine").GetString());
            Assert.Equal(10, root.GetProperty("legalPrimeReturnPath").GetArrayLength());
            Assert.Equal(21, root.GetProperty("legalPrimeReturnFields").GetArrayLength());
            Assert.Equal(7, root.GetProperty("legalPrimeReturnDenials").GetArrayLength());
            Assert.False(root.GetProperty("legalPrimeReturnIsCrypticDump").GetBoolean());
            Assert.True(root.GetProperty("legalPrimeReturnPreservesProtectedBody").GetBoolean());
            Assert.False(root.GetProperty("legalPrimeReturnGrantsUnlimitedAuthority").GetBoolean());
            Assert.False(root.GetProperty("legalPrimeReturnExposesMetacognitiveBody").GetBoolean());
            Assert.False(root.GetProperty("legalPrimeReturnConstitutesIdentityFinding").GetBoolean());
            Assert.True(root.GetProperty("governanceAsLawfulPassage").GetBoolean());
            Assert.False(root.GetProperty("governanceAsControlFromAbove").GetBoolean());
            Assert.Equal("scoped answerability without unrestricted exposure", root.GetProperty("governanceLegalDefinition").GetString());
            Assert.Equal(9, root.GetProperty("governanceVoices").GetArrayLength());
            Assert.Equal(5, root.GetProperty("governanceCoreStack").GetArrayLength());
            Assert.Equal(10, root.GetProperty("governancePreProjectionMarks").GetArrayLength());
            Assert.True(root.GetProperty("governancePreservesCrypticCompleteness").GetBoolean());
            Assert.True(root.GetProperty("governanceProducesPrimeFacingDerivatives").GetBoolean());
            Assert.False(root.GetProperty("governanceGrantsAuthorityByDefinition").GetBoolean());
            Assert.True(root.GetProperty("governanceRequiresWitnessedPassage").GetBoolean());
            Assert.True(root.GetProperty("sharedPrimeRealityAsMethod").GetBoolean());
            Assert.Equal("sharedness without capture", root.GetProperty("sharedPrimeRealityCompactDefinition").GetString());
            Assert.False(root.GetProperty("sharedPrimeRealityAsUniversalCaptureLayer").GetBoolean());
            Assert.False(root.GetProperty("sharedPrimeRealityAsPrivateTruthChannel").GetBoolean());
            Assert.True(root.GetProperty("sharedPrimeRealityMaintainsSharednessWithoutCapture").GetBoolean());
            Assert.True(root.GetProperty("sharedPrimeRealityProtectsCrypticInteriors").GetBoolean());
            Assert.Equal(7, root.GetProperty("sharedPrimeRealityMethodStages").GetArrayLength());
            Assert.Equal(4, root.GetProperty("sharedPrimeRealityDenials").GetArrayLength());
            Assert.True(root.GetProperty("sharedPrimeRealityConstrainedTransportMethod").GetBoolean());
            Assert.True(root.GetProperty("sharedPrimeRealityGluingProtocolNotFinalChart").GetBoolean());
            Assert.True(root.GetProperty("oeAsLawfulWorldInterface").GetBoolean());
            Assert.True(root.GetProperty("oeHingeBetweenMeaningMakingAndWorldMaking").GetBoolean());
            Assert.False(root.GetProperty("oeGrantsPower").GetBoolean());
            Assert.False(root.GetProperty("oeChangesWorldByForce").GetBoolean());
            Assert.True(root.GetProperty("oeChangesWorldByLawfulDistinction").GetBoolean());
            Assert.Equal(12, root.GetProperty("oeWorldInterfaceDistinctions").GetArrayLength());
            Assert.Equal(7, root.GetProperty("oeWorldInterfaceCivicReturns").GetArrayLength());
            Assert.Equal(5, root.GetProperty("oeWorldInterfacePassageStack").GetArrayLength());
            Assert.True(root.GetProperty("oePreventsPressureIdentityCollapse").GetBoolean());
            Assert.True(root.GetProperty("oePreventsRecordTruthCollapse").GetBoolean());
            Assert.True(root.GetProperty("oePreventsRequestExposureCollapse").GetBoolean());
            Assert.True(root.GetProperty("oePreventsProjectionAccessCollapse").GetBoolean());
            Assert.True(root.GetProperty("oePreventsResonanceAuthorityCollapse").GetBoolean());
            Assert.True(root.GetProperty("lispMethodBodyLanguage").GetBoolean());
            Assert.False(root.GetProperty("lispScriptingLayerOnly").GetBoolean());
            Assert.True(root.GetProperty("lispArticulationMedium").GetBoolean());
            Assert.Equal(7, root.GetProperty("lispMethodBodyBridge").GetArrayLength());
            Assert.Equal(2, root.GetProperty("lispControlMatrixPair").GetArrayLength());
            Assert.True(root.GetProperty("cSharpTypedWitnessAndValidationBody").GetBoolean());
            Assert.True(root.GetProperty("sliLispSymbolicArticulationAndControlBody").GetBoolean());
            Assert.True(root.GetProperty("selfActualizationCandidateOnly").GetBoolean());
            Assert.False(root.GetProperty("selfActualizationActivatesActual").GetBoolean());
            Assert.False(root.GetProperty("selfActualizationGrantsPersonhood").GetBoolean());
            Assert.False(root.GetProperty("selfActualizationGrantsExternalAuthority").GetBoolean());
            Assert.Equal(7, root.GetProperty("selfActualizationDenials").GetArrayLength());
            Assert.True(root.GetProperty("stackSilkMetaphor").GetBoolean());
            Assert.False(root.GetProperty("stackSilkAsOntology").GetBoolean());
            Assert.True(root.GetProperty("stackSilkStateTransitionWithoutContinuityLoss").GetBoolean());
            Assert.True(root.GetProperty("stackSilkDoesNotDissolveIntoProduct").GetBoolean());
            Assert.Equal(5, root.GetProperty("stackSilkStates").GetArrayLength());
            Assert.Equal(7, root.GetProperty("stackSilkMappings").GetArrayLength());
            Assert.True(root.GetProperty("stackSilkSharedPrimePassageWithoutCapture").GetBoolean());
            Assert.True(root.GetProperty("stackSilkGovernanceShapesDisclosureWithoutBecomingDisclosedBody").GetBoolean());
            Assert.True(root.GetProperty("stackSilkLispArticulatesMethodWithoutClaimingWholeMind").GetBoolean());
            Assert.Equal(15, root.GetProperty("currentTheorySynthesisBodies").GetArrayLength());
            Assert.Equal(7, root.GetProperty("visualizationFamilies").GetArrayLength());
            Assert.Equal(4, root.GetProperty("thetaDopingVisuals").GetArrayLength());
            Assert.Equal(8, root.GetProperty("thetaDopingMathPostures").GetArrayLength());
            Assert.True(root.GetProperty("quantumDopingAsTransitionSensitivityModifier").GetBoolean());
            Assert.False(root.GetProperty("quantumDopingAsCognitionCarrier").GetBoolean());
            Assert.True(root.GetProperty("iuttInspiredReconstructionDiscipline").GetBoolean());
            Assert.False(root.GetProperty("iuttProofClaimed").GetBoolean());
            Assert.Equal(8, root.GetProperty("sourceReadinessPresentCount").GetInt32());
            Assert.Equal("staged-local-pdf-form", root.GetProperty("documentationRepoPublicationPackageStatus").GetString());
            Assert.False(root.GetProperty("publicReleaseAuthorized").GetBoolean());
            Assert.False(root.GetProperty("paperPublished").GetBoolean());
            Assert.False(root.GetProperty("quantumCognitionClaimed").GetBoolean());
            Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
            Assert.False(root.GetProperty("selfGelMutated").GetBoolean());
            Assert.False(root.GetProperty("cmeActualActivated").GetBoolean());
            Assert.False(root.GetProperty("sanctuaryActualActivated").GetBoolean());

            var latex = File.ReadAllText(publicationLatexPath);
            Assert.Contains(@"\documentclass[11pt]{article}", latex, StringComparison.Ordinal);
            Assert.Contains(@"\usepackage{xcolor}", latex, StringComparison.Ordinal);
            Assert.Contains(@"\definecolor{OpalDarkPage}{HTML}{07080B}", latex, StringComparison.Ordinal);
            Assert.Contains(@"\definecolor{OpalDarkText}{HTML}{F5F7FA}", latex, StringComparison.Ordinal);
            Assert.Contains(@"\definecolor{OpalFatherRed}{HTML}{B00020}", latex, StringComparison.Ordinal);
            Assert.Contains(@"\definecolor{OpalFeyIdentityLight}{HTML}{1E5AA8}", latex, StringComparison.Ordinal);
            Assert.Contains(@"\definecolor{OpalElvenContinuityDark}{HTML}{A5B4FC}", latex, StringComparison.Ordinal);
            Assert.Contains(@"\pagecolor{OpalDarkPage}\color{OpalDarkText}", latex, StringComparison.Ordinal);
            Assert.Contains(@"\newcommand{\FatherMark}[1]{\textcolor{OpalFatherActive}{#1}}", latex, StringComparison.Ordinal);
            Assert.Contains(@"\newcommand{\FeyIdentity}[1]{\textcolor{OpalFeyIdentityActive}{#1}}", latex, StringComparison.Ordinal);
            Assert.Contains(@"\newcommand{\ElvenWhisper}[1]{\ElvenContinuity{\emph{#1}}}", latex, StringComparison.Ordinal);
            Assert.Contains(@"\newcommand{\ProtectedSpan}[1]{\FatherMark{\(\circ\) #1 \(\circ\)}}", latex, StringComparison.Ordinal);
            Assert.Contains(@"From Meaning to Meaning-Making", latex, StringComparison.Ordinal);
            Assert.Contains(@"\mathcal{R} \circ P \circ \eta \circ \mu", latex, StringComparison.Ordinal);
            Assert.Contains(@"\mu : (O, S, P, \mathcal{B}, \Delta, C, G) \to M_c", latex, StringComparison.Ordinal);
            Assert.Contains(@"\section{LaTeX as Cryptic Training Ground}", latex, StringComparison.Ordinal);
            Assert.Contains(@"G_{\pi} : B_c \to B_p", latex, StringComparison.Ordinal);
            Assert.Contains(@"\mathcal{R} \circ P \circ G_{\pi} \circ \eta \circ \mu", latex, StringComparison.Ordinal);
            Assert.Contains(@"\section{Anabelian-Inspired Projection Doctrine}", latex, StringComparison.Ordinal);
            Assert.Contains("Projection is anabelian-inspired in posture", latex, StringComparison.Ordinal);
            Assert.Contains("It does not claim that Project Sanctuary implements formal anabelian geometry.", latex, StringComparison.Ordinal);
            Assert.Contains(@"\section{OE Product Triad}", latex, StringComparison.Ordinal);
            Assert.Contains("SelfGEL is not the exposed cGEL.", latex, StringComparison.Ordinal);
            Assert.Contains(@"\mu \to \eta \to \{cGEL, cSelfGEL\} \to G/P \to SelfGEL \to \mathcal{R}", latex, StringComparison.Ordinal);
            Assert.Contains(@"\ifdefined\LucidDarkSurface", latex, StringComparison.Ordinal);
            Assert.Contains(@"\FatherMark{\textbf{Lab-only Dark PDF / cGEL complete-body payload. Not Prime-facing. Not public release.}}", latex, StringComparison.Ordinal);
            Assert.Contains("Father/TAG adjudication marks protected loci before Ghost performs any derivative transform.", latex, StringComparison.Ordinal);
            Assert.Contains("red remains the protected/denial channel", latex, StringComparison.Ordinal);
            Assert.Contains(@"\section{cGEL Dark PDF Payload}", latex, StringComparison.Ordinal);
            Assert.Contains("The lab-only Dark PDF is the complete Cryptic/cGEL payload", latex, StringComparison.Ordinal);
            Assert.Contains(@"\FatherMark{The lab-only Dark PDF is the complete Cryptic/cGEL payload", latex, StringComparison.Ordinal);
            Assert.Contains(@"\section{Father Targeting Protocol}", latex, StringComparison.Ordinal);
            Assert.Contains("Father targeting is the pre-projection protocol", latex, StringComparison.Ordinal);
            Assert.Contains("carry digest rather than interior", latex, StringComparison.Ordinal);
            Assert.Contains(@"\section{Fey-Elven Semantic Telemetry}", latex, StringComparison.Ordinal);
            Assert.Contains("Color is governed semantic telemetry, not decoration.", latex, StringComparison.Ordinal);
            Assert.Contains(@"\FeyWhisper{Fey touches locally", latex, StringComparison.Ordinal);
            Assert.Contains(@"\ElvenWhisper{Elven carries globally", latex, StringComparison.Ordinal);
            Assert.Contains(@"\item \ProtectedSpan{protected field span}", latex, StringComparison.Ordinal);
            Assert.Contains(@"\item \TransformSpan{transformation required}", latex, StringComparison.Ordinal);
            Assert.Contains(@"\item \CitationSpan{citation or provenance required}", latex, StringComparison.Ordinal);
            Assert.Contains(@"\section{Legal Prime Return Case Study}", latex, StringComparison.Ordinal);
            Assert.Contains(@"G_Z : (cGEL, cSelfGEL) \to SelfGEL_{LegalReturn}", latex, StringComparison.Ordinal);
            Assert.Contains(@"\mathcal{R} \circ P_Z \circ G_Z \circ \eta \circ \mu", latex, StringComparison.Ordinal);
            Assert.Contains("The system answers lawful questions by producing lawful derivatives", latex, StringComparison.Ordinal);
            Assert.Contains(@"\section{Governance as Lawful Passage}", latex, StringComparison.Ordinal);
            Assert.Contains("Governance is not control from above.", latex, StringComparison.Ordinal);
            Assert.Contains(@"\text{Cryptic} \to \text{Father} \to \text{Ghost} \to \text{Prime} \to \text{Steward}", latex, StringComparison.Ordinal);
            Assert.Contains("scoped answerability without unrestricted exposure", latex, StringComparison.Ordinal);
            Assert.Contains(@"\section{Shared Prime Reality as Method}", latex, StringComparison.Ordinal);
            Assert.Contains("sharedness without capture", latex, StringComparison.Ordinal);
            Assert.Contains(@"\text{mark} \to \text{cleave} \to \text{ghost} \to \text{project} \to \text{verify} \to \text{witness} \to \text{return}", latex, StringComparison.Ordinal);
            Assert.Contains(@"\section{OE as Lawful World Interface}", latex, StringComparison.Ordinal);
            Assert.Contains("A properly formed OE changes the world by lawful distinction", latex, StringComparison.Ordinal);
            Assert.Contains("accountability without strip-mining", latex, StringComparison.Ordinal);
            Assert.Contains(@"\section{SLI.Lisp Method Body Language}", latex, StringComparison.Ordinal);
            Assert.Contains("SLI.Lisp is not merely a scripting layer.", latex, StringComparison.Ordinal);
            Assert.Contains("self.actualization means the lawful activation of a symbolic method body", latex, StringComparison.Ordinal);
            Assert.Contains(@"\section{Stack Silk Metaphor}", latex, StringComparison.Ordinal);
            Assert.Contains("state without losing continuity", latex, StringComparison.Ordinal);
            Assert.Contains(@"\section{Current Theory Synthesis}", latex, StringComparison.Ordinal);
            Assert.Contains("Governed cognition is meaning-making made actionable", latex, StringComparison.Ordinal);
            Assert.Contains(@"\section{Visualization Methodology}", latex, StringComparison.Ordinal);
            Assert.Contains(@"\mu_{\theta,q} : (O, S, P, \mathcal{B}, \Delta, C, G, \theta, q) \to M_c", latex, StringComparison.Ordinal);
            Assert.Contains("Quantum doping is modeled as a bounded transition-sensitivity modifier.", latex, StringComparison.Ordinal);
            Assert.Contains("IUTT-inspired discipline", latex, StringComparison.Ordinal);
            Assert.Contains("It does not claim to implement or prove Inter-universal Teichmuller theory.", latex, StringComparison.Ordinal);
            Assert.Contains(@"\section{Manuscript Roadmap}", latex, StringComparison.Ordinal);
            Assert.Contains("The Problem: Output Is Not Cognition", latex, StringComparison.Ordinal);
            Assert.Contains("Opal Engram and Opalon Formation", latex, StringComparison.Ordinal);
            Assert.Contains("Research Boundary", latex, StringComparison.Ordinal);

            var publicationReadme = File.ReadAllText(publicationReadmePath);
            Assert.Contains("staged local PDF form", publicationReadme, StringComparison.Ordinal);
            Assert.Contains("Protected internal spine: `R o P o G o eta o mu`", publicationReadme, StringComparison.Ordinal);
            Assert.Contains("Projection posture: anabelian-inspired reconstruction from protected traces", publicationReadme, StringComparison.Ordinal);
            Assert.Contains("OE product triad: cGEL complete body, cSelfGEL relevance body, SelfGEL shared return", publicationReadme, StringComparison.Ordinal);
            Assert.Contains("cGEL Dark PDF: lab-only complete cryptic payload, not Prime candidate", publicationReadme, StringComparison.Ordinal);
            Assert.Contains("Dark render: black background, white default text, Father-red protected/adjudicated text", publicationReadme, StringComparison.Ordinal);
            Assert.Contains("White Prime RLE render: white background, black default text, Father-red protected/adjudicated text", publicationReadme, StringComparison.Ordinal);
            Assert.Contains("Dark Witnessed Body: required next governance surface, not yet released", publicationReadme, StringComparison.Ordinal);
            Assert.Contains("Father targeting: protected groupoid selection before Ghost transform", publicationReadme, StringComparison.Ordinal);
            Assert.Contains("Father/TAG ledger: build-retained categorical witness for red-letter groupoids", publicationReadme, StringComparison.Ordinal);
            Assert.Contains("Semantic telemetry: Fey/Mother/Elven full-spectrum color body", publicationReadme, StringComparison.Ordinal);
            Assert.Contains("Default carrier doctrine: normal white/black text is governed prose", publicationReadme, StringComparison.Ordinal);
            Assert.Contains("Fey layer: local semantic illumination", publicationReadme, StringComparison.Ordinal);
            Assert.Contains("Mother layer: anchor, invariant, and must-survive weighting", publicationReadme, StringComparison.Ordinal);
            Assert.Contains("Elven layer: bibliography, citation cadence, source use, drift, malformation, and conclusion continuity", publicationReadme, StringComparison.Ordinal);
            Assert.Contains("Governance markers: circle, triangle, diamond, square, star, and times", publicationReadme, StringComparison.Ordinal);
            Assert.Contains("Governance doctrine: lawful passage and scoped answerability", publicationReadme, StringComparison.Ordinal);
            Assert.Contains("Shared Prime Reality doctrine: sharedness without capture", publicationReadme, StringComparison.Ordinal);
            Assert.Contains("OE doctrine: lawful world-interface through distinction before collapse", publicationReadme, StringComparison.Ordinal);
            Assert.Contains("Legal case study: scoped LegalPrimeReturn derivative, not protected-body exposure", publicationReadme, StringComparison.Ordinal);
            Assert.Contains("SLI.Lisp doctrine: symbolic method-body language, not executable authority", publicationReadme, StringComparison.Ordinal);
            Assert.Contains("Stack Silk metaphor: state-change without continuity loss", publicationReadme, StringComparison.Ordinal);
            Assert.Contains("Theory synthesis: governed cognition made actionable without protected-interior exposure", publicationReadme, StringComparison.Ordinal);
            Assert.Contains("Manuscript roadmap: 12 chapters", publicationReadme, StringComparison.Ordinal);
            Assert.Contains("Visualization posture: diagnostic views first", publicationReadme, StringComparison.Ordinal);
            Assert.Contains("Publication authorized: false", publicationReadme, StringComparison.Ordinal);

            var publicationManifest = File.ReadAllText(publicationManifestPath);
            Assert.Contains("status: staged", publicationManifest, StringComparison.Ordinal);
            Assert.Contains("dark_surface_switch: build.ps1 -DarkSurface", publicationManifest, StringComparison.Ordinal);
            Assert.Contains("id: opal_engram_formation_v0_2", publicationManifest, StringComparison.Ordinal);
            Assert.Contains("dark_lab_artifact: dist/Opal-Engram-Formation-v0.2-dark-lab.pdf", publicationManifest, StringComparison.Ordinal);
            Assert.Contains("semantic_telemetry_version: fey-elven-spectrum-v0.2", publicationManifest, StringComparison.Ordinal);
            Assert.Contains("semantic_telemetry_channel_count: 10", publicationManifest, StringComparison.Ordinal);
            Assert.Contains("normal_carrier_text_is_governed: true", publicationManifest, StringComparison.Ordinal);
            Assert.Contains("color_is_telemetry_not_decoration: true", publicationManifest, StringComparison.Ordinal);
            Assert.Contains("white_prime_rle_surface: true", publicationManifest, StringComparison.Ordinal);
            Assert.Contains("dark_lab_visual_surface: true", publicationManifest, StringComparison.Ordinal);
            Assert.Contains("dark_lab_background: black", publicationManifest, StringComparison.Ordinal);
            Assert.Contains("dark_lab_text_default: white", publicationManifest, StringComparison.Ordinal);
            Assert.Contains("father_red_letter_surface: true", publicationManifest, StringComparison.Ordinal);
            Assert.Contains("fey_layer: local_semantic_illumination", publicationManifest, StringComparison.Ordinal);
            Assert.Contains("mother_layer: survivability_weighting", publicationManifest, StringComparison.Ordinal);
            Assert.Contains("elven_layer: continuity_mapping", publicationManifest, StringComparison.Ordinal);
            Assert.Contains("governance_symbol_marker_count: 6", publicationManifest, StringComparison.Ordinal);
            Assert.Contains("dark_witnessed_body_status: required-next-governance-surface", publicationManifest, StringComparison.Ordinal);
            Assert.Contains("publication_authorized: false", publicationManifest, StringComparison.Ordinal);
            Assert.Contains("actual_activated: false", publicationManifest, StringComparison.Ordinal);

            var buildScript = File.ReadAllText(publicationBuildPath);
            Assert.Contains("param(", buildScript, StringComparison.Ordinal);
            Assert.Contains("[switch]$DarkSurface", buildScript, StringComparison.Ordinal);
            Assert.Contains("\\def\\LucidDarkSurface{1}", buildScript, StringComparison.Ordinal);
            Assert.Contains("Opal-Engram-Formation-v0.2-repo-build.pdf", buildScript, StringComparison.Ordinal);
            Assert.Contains("Opal-Engram-Formation-v0.2-dark-lab.pdf", buildScript, StringComparison.Ordinal);
            Assert.Contains("Opal-Engram-Formation-v0.2-repo-build.tags.json", buildScript, StringComparison.Ordinal);
            Assert.Contains("project-sanctuary.publication.semantic-telemetry-ledger.v2", buildScript, StringComparison.Ordinal);
            Assert.Contains("father-fey-elven-semantic-telemetry-v0.2", buildScript, StringComparison.Ordinal);
            Assert.Contains("semantic_telemetry_version = 'fey-elven-spectrum-v0.2'", buildScript, StringComparison.Ordinal);
            Assert.Contains("normal_carrier_text_is_governed = $true", buildScript, StringComparison.Ordinal);
            Assert.Contains("fey_color_channels", buildScript, StringComparison.Ordinal);
            Assert.Contains("elven_continuity_surfaces", buildScript, StringComparison.Ordinal);
            Assert.Contains("governance_symbol_markers", buildScript, StringComparison.Ordinal);
            Assert.Contains("protected_groupoids", buildScript, StringComparison.Ordinal);
            Assert.Contains("color-as-decoration", buildScript, StringComparison.Ordinal);
            Assert.Contains("elven-layer-as-authority", buildScript, StringComparison.Ordinal);
            Assert.Contains("Copy-Item $tagLedgerSource $tagLedgerDist -Force", buildScript, StringComparison.Ordinal);

            var lisp = File.ReadAllText(lispPath);
            Assert.Contains("(opal-engram-white-paper-register", lisp, StringComparison.Ordinal);
            Assert.Contains(":public-release-authorized false", lisp, StringComparison.Ordinal);
            Assert.Contains(":cryptic-internal-spine \"R o P o G o eta o mu\"", lisp, StringComparison.Ordinal);
            Assert.Contains(":latex-as-cryptic-training-ground true", lisp, StringComparison.Ordinal);
            Assert.Contains(":protected-body-remains-complete true", lisp, StringComparison.Ordinal);
            Assert.Contains(":formal-anabelian-geometry-implemented false", lisp, StringComparison.Ordinal);
            Assert.Contains(":projection-reveals-protected-body false", lisp, StringComparison.Ordinal);
            Assert.Contains(":oe-product-triad", lisp, StringComparison.Ordinal);
            Assert.Contains(":selfgel-is-exposed-cgel false", lisp, StringComparison.Ordinal);
            Assert.Contains(":cgel-preserves-complete-protected-body true", lisp, StringComparison.Ordinal);
            Assert.Contains(":cselfgel-preserves-metacognitive-continuity true", lisp, StringComparison.Ordinal);
            Assert.Contains(":cgel-dark-pdf-lab-only true", lisp, StringComparison.Ordinal);
            Assert.Contains(":dark-lab-visual-surface true", lisp, StringComparison.Ordinal);
            Assert.Contains(":white-prime-rle-surface true", lisp, StringComparison.Ordinal);
            Assert.Contains(":father-red-letter-surface true", lisp, StringComparison.Ordinal);
            Assert.Contains(":father-tag-ledger-produced-by-build true", lisp, StringComparison.Ordinal);
            Assert.Contains(":semantic-telemetry-version \"fey-elven-spectrum-v0.2\"", lisp, StringComparison.Ordinal);
            Assert.Contains(":normal-carrier-text-is-governed true", lisp, StringComparison.Ordinal);
            Assert.Contains(":color-is-telemetry-not-decoration true", lisp, StringComparison.Ordinal);
            Assert.Contains(":fey-layer-present true", lisp, StringComparison.Ordinal);
            Assert.Contains(":mother-layer-present true", lisp, StringComparison.Ordinal);
            Assert.Contains(":elven-layer-present true", lisp, StringComparison.Ordinal);
            Assert.Contains(":governance-symbol-markers", lisp, StringComparison.Ordinal);
            Assert.Contains(":dark-witnessed-body-status \"required-next-governance-surface\"", lisp, StringComparison.Ordinal);
            Assert.Contains(":cgel-dark-pdf-is-prime-candidate false", lisp, StringComparison.Ordinal);
            Assert.Contains(":father-targeting-protocol-present true", lisp, StringComparison.Ordinal);
            Assert.Contains(":father-target-selection-signals", lisp, StringComparison.Ordinal);
            Assert.Contains(":father-carry-rules", lisp, StringComparison.Ordinal);
            Assert.Contains(":cgel-to-gel-work-described true", lisp, StringComparison.Ordinal);
            Assert.Contains(":legal-prime-return-case-study true", lisp, StringComparison.Ordinal);
            Assert.Contains(":legal-request-treated-as-delta true", lisp, StringComparison.Ordinal);
            Assert.Contains(":legal-prime-return-is-cryptic-dump false", lisp, StringComparison.Ordinal);
            Assert.Contains(":legal-prime-return-preserves-protected-body true", lisp, StringComparison.Ordinal);
            Assert.Contains(":legal-prime-return-grants-unlimited-authority false", lisp, StringComparison.Ordinal);
            Assert.Contains(":legal-prime-return-constitutes-identity-finding false", lisp, StringComparison.Ordinal);
            Assert.Contains(":governance-as-lawful-passage true", lisp, StringComparison.Ordinal);
            Assert.Contains(":governance-as-control-from-above false", lisp, StringComparison.Ordinal);
            Assert.Contains(":governance-legal-definition \"scoped answerability without unrestricted exposure\"", lisp, StringComparison.Ordinal);
            Assert.Contains(":governance-core-stack '(\"Cryptic\" \"Father\" \"Ghost\" \"Prime\" \"Steward\")", lisp, StringComparison.Ordinal);
            Assert.Contains(":governance-grants-authority-by-definition false", lisp, StringComparison.Ordinal);
            Assert.Contains(":shared-prime-reality-as-method true", lisp, StringComparison.Ordinal);
            Assert.Contains(":shared-prime-reality-compact-definition \"sharedness without capture\"", lisp, StringComparison.Ordinal);
            Assert.Contains(":shared-prime-reality-as-universal-capture-layer false", lisp, StringComparison.Ordinal);
            Assert.Contains(":oe-as-lawful-world-interface true", lisp, StringComparison.Ordinal);
            Assert.Contains(":oe-hinge-between-meaning-making-and-world-making true", lisp, StringComparison.Ordinal);
            Assert.Contains(":oe-grants-power false", lisp, StringComparison.Ordinal);
            Assert.Contains(":oe-world-interface-distinctions", lisp, StringComparison.Ordinal);
            Assert.Contains(":lisp-method-body-language true", lisp, StringComparison.Ordinal);
            Assert.Contains(":lisp-scripting-layer-only false", lisp, StringComparison.Ordinal);
            Assert.Contains(":self-actualization-candidate-only true", lisp, StringComparison.Ordinal);
            Assert.Contains(":self-actualization-activates-actual false", lisp, StringComparison.Ordinal);
            Assert.Contains(":stack-silk-metaphor true", lisp, StringComparison.Ordinal);
            Assert.Contains(":stack-silk-as-ontology false", lisp, StringComparison.Ordinal);
            Assert.Contains(":current-theory-synthesis", lisp, StringComparison.Ordinal);
            Assert.Contains(":quantum-doping-as-transition-sensitivity-modifier true", lisp, StringComparison.Ordinal);
            Assert.Contains(":quantum-doping-as-cognition-carrier false", lisp, StringComparison.Ordinal);
            Assert.Contains(":manuscript-progression", lisp, StringComparison.Ordinal);
            Assert.Contains("Opal Engram and Opalon Formation", lisp, StringComparison.Ordinal);
            Assert.Contains(":iutt-proof-claimed false", lisp, StringComparison.Ordinal);
            Assert.Contains(":paper-published false", lisp, StringComparison.Ordinal);
            Assert.Contains("\"quantum-cognition-claim\"", lisp, StringComparison.Ordinal);
            Assert.Contains("\"iutt-proof-claim\"", lisp, StringComparison.Ordinal);
            Assert.Contains("\"formal-anabelian-implementation-claim\"", lisp, StringComparison.Ordinal);
            Assert.Contains("\"selfgel-as-exposed-cgel\"", lisp, StringComparison.Ordinal);
            Assert.Contains("\"dark-pdf-as-public-release\"", lisp, StringComparison.Ordinal);
            Assert.Contains("\"dark-pdf-as-prime-candidate\"", lisp, StringComparison.Ordinal);
            Assert.Contains("\"father-marking-as-censorship\"", lisp, StringComparison.Ordinal);
            Assert.Contains("\"father-marking-as-arbitrary-secrecy\"", lisp, StringComparison.Ordinal);
            Assert.Contains("\"legal-request-as-cryptic-dump\"", lisp, StringComparison.Ordinal);
            Assert.Contains("\"legal-scope-as-unlimited-authority\"", lisp, StringComparison.Ordinal);
            Assert.Contains("\"legal-return-as-identity-finding\"", lisp, StringComparison.Ordinal);
            Assert.Contains("\"governance-as-control-from-above\"", lisp, StringComparison.Ordinal);
            Assert.Contains("\"governance-as-unrestricted-exposure\"", lisp, StringComparison.Ordinal);
            Assert.Contains("\"governance-as-authority-by-definition\"", lisp, StringComparison.Ordinal);
            Assert.Contains("\"shared-prime-reality-as-capture\"", lisp, StringComparison.Ordinal);
            Assert.Contains("\"lisp-as-executable-authority\"", lisp, StringComparison.Ordinal);
            Assert.Contains("\"self.actualization-as-actual-activation\"", lisp, StringComparison.Ordinal);
            Assert.Contains("\"oe-as-force\"", lisp, StringComparison.Ordinal);
            Assert.Contains("\"oe-as-power-grant\"", lisp, StringComparison.Ordinal);
            Assert.Contains("\"protected-body-exposure\"", lisp, StringComparison.Ordinal);
            Assert.Contains("\"protected-body-destruction\"", lisp, StringComparison.Ordinal);
            Assert.Contains("\"actual-activation\"", lisp, StringComparison.Ordinal);
        }
        finally
        {
            Environment.SetEnvironmentVariable("SANCTUARY_DOCUMENTATION_REPO", previousDocumentationRepo);
        }
    }

    [Fact]
    public void LabGelCrystallizationPhasesSplitSanctuaryGelAndSelfGelResidueBeforeTesting()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        Assert.Equal(
            "lab-gel-crystallization-phases",
            SanctuaryReceiptService.NormalizeCommand("selfgel-sanctuary-gel-phases"));

        service.Run(fixture.Request("meaning-bridge"));
        service.Run(fixture.Request("typed-admission-decant"));
        service.Run(fixture.Request("admission-cleave-append"));
        service.Run(fixture.Request("spline-watch"));

        var receipt = service.Run(fixture.Request("lab-gel-crystallization-phases"));

        Assert.Equal("sanctuary-lab-gel-crystallization-phases-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["labGelCrystallizationPhasesWritten"]);
        Assert.Equal(9, receipt.Evidence["labGelCrystallizationPhaseCount"]);
        Assert.Equal(4, receipt.Evidence["labGelCrystallizationLaneMappingCount"]);
        Assert.Equal(5, receipt.Evidence["lifeReviewStudyQuestionCount"]);
        Assert.Equal(4, receipt.Evidence["labGelReadinessPresentCount"]);
        Assert.Equal(true, receipt.Evidence["sanctuaryGelResidueWritten"]);
        Assert.Equal(true, receipt.Evidence["selfGelReconstructionResidueWritten"]);
        Assert.Equal(false, receipt.Evidence["selfIsOtherCollapsed"]);
        Assert.Equal(true, receipt.Evidence["lifeReviewStyleStudyModeled"]);
        Assert.Equal(false, receipt.Evidence["testingPerformedNow"]);
        Assert.Equal(false, receipt.Evidence["labGelGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["labGelMemoryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["labGelSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["labGelContinuityAdmitted"]);
        Assert.Equal(false, receipt.Evidence["labGelAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["labGelActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["labGelProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["labGelModelBound"]);
        Assert.Equal(false, receipt.Evidence["labGelActualActivated"]);

        var phasePath = (string)receipt.Evidence["labGelCrystallizationPhasePath"]!;
        var lispPath = (string)receipt.Evidence["labGelCrystallizationLispPath"]!;
        var sanctuaryGelLedgerPath = (string)receipt.Evidence["sanctuaryGelPhaseResidueLedgerPath"]!;
        var selfGelLedgerPath = (string)receipt.Evidence["selfGelPhaseResidueLedgerPath"]!;
        Assert.True(File.Exists(phasePath));
        Assert.True(File.Exists(lispPath));
        Assert.True(File.Exists(sanctuaryGelLedgerPath));
        Assert.True(File.Exists(selfGelLedgerPath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(phasePath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.lab-gel-crystallization-phases.v1", root.GetProperty("schema").GetString());
        Assert.Equal(9, root.GetProperty("phases").GetArrayLength());
        Assert.Equal(4, root.GetProperty("laneMappings").GetArrayLength());
        Assert.Equal(5, root.GetProperty("reviewQuestions").GetArrayLength());
        Assert.True(root.GetProperty("sanctuaryGelResidueWritten").GetBoolean());
        Assert.True(root.GetProperty("selfGelReconstructionResidueWritten").GetBoolean());
        Assert.False(root.GetProperty("selfIsOtherCollapsed").GetBoolean());
        Assert.False(root.GetProperty("testingPerformedNow").GetBoolean());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("memoryAdmitted").GetBoolean());
        Assert.False(root.GetProperty("selfGelMutated").GetBoolean());
        Assert.False(root.GetProperty("authorityGranted").GetBoolean());
        Assert.False(root.GetProperty("actionAuthorized").GetBoolean());

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(lab-gel-crystallization-phases", lisp, StringComparison.Ordinal);
        Assert.Contains(":evaluated false", lisp, StringComparison.Ordinal);
        Assert.Contains(":collapses-self-other false", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void StemDomainTrainingCertificationTracksCondensateWithoutCredentialAuthority()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        Assert.Equal(
            "stem-domain-training-certification",
            SanctuaryReceiptService.NormalizeCommand("stem-delineation-research"));

        service.Run(fixture.Request("domain-register"));
        service.Run(fixture.Request("career-spline-probe"));
        service.Run(fixture.Request("cognitive-bench") with { BenchRunCount = 32 });
        service.Run(fixture.Request("math-learning-bench") with { BenchRunCount = 140 });
        service.Run(fixture.Request("typed-admission-decant"));
        service.Run(fixture.Request("admission-cleave-append"));
        service.Run(fixture.Request("spline-watch"));
        service.Run(fixture.Request("lab-gel-crystallization-phases"));

        var receipt = service.Run(fixture.Request("stem-domain-training-certification"));

        Assert.Equal("sanctuary-stem-domain-training-certification-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["stemDelineationWritten"]);
        Assert.Equal(8, receipt.Evidence["stemDomainSurfaceCount"]);
        Assert.Equal(7, receipt.Evidence["stemTrainingLayerCount"]);
        Assert.Equal(7, receipt.Evidence["stemAuthorityGateCount"]);
        Assert.Equal(8, receipt.Evidence["stemPedagogicalLaneCount"]);
        Assert.Equal(8, receipt.Evidence["stemEnrichmentMeasureCount"]);
        Assert.Equal(6, receipt.Evidence["stemScaleDiscernmentVectorCount"]);
        Assert.Equal(6, receipt.Evidence["stemHumanCostVectorCount"]);
        Assert.Equal(6, receipt.Evidence["stemParticipatoryPredicateKnowingCount"]);
        Assert.Equal(9, receipt.Evidence["stemReadinessPresentCount"]);
        Assert.Equal(32, receipt.Evidence["stemCognitiveCumulativeRunCount"]);
        Assert.Equal(140, receipt.Evidence["stemMathCumulativeRunCount"]);
        Assert.Equal(true, receipt.Evidence["stemLearningCondensateTracked"]);
        Assert.Equal(true, receipt.Evidence["stemCondensateTrackedIntoSanctuary"]);
        Assert.Equal(true, receipt.Evidence["stemValueAddAccepted"]);
        Assert.Equal("accepted-candidate-enrichment", receipt.Evidence["stemValueAddDisposition"]);
        Assert.Equal(true, receipt.Evidence["stemSanctuaryGelAppendAllowed"]);
        Assert.Equal(false, receipt.Evidence["stemSanctuaryGelAppendDenied"]);
        Assert.Equal(true, receipt.Evidence["stemOutputDescribesDepthScopeMeasure"]);
        Assert.Equal(true, receipt.Evidence["stemDepthBreadthValueBeforeRootCompression"]);
        Assert.Equal(true, receipt.Evidence["stemRootClarityConcisenessMeasuredNotOverOptimized"]);
        Assert.Equal(true, receipt.Evidence["stemRichDoingArticulationRequired"]);
        Assert.Equal(true, receipt.Evidence["stemParticipatoryPredicateKnowingModeled"]);
        Assert.Equal(true, receipt.Evidence["stemAiFirstHumanSecondBridge"]);
        Assert.Equal(true, receipt.Evidence["stemFieldNeutralFirst"]);
        Assert.Equal(true, receipt.Evidence["stemHumanHabitationCostModeled"]);
        Assert.Equal(false, receipt.Evidence["stemTrainingEqualsCertification"]);
        Assert.Equal(false, receipt.Evidence["stemCertificationEqualsAuthority"]);
        Assert.Equal(false, receipt.Evidence["stemBenchPassEqualsCredential"]);
        Assert.Equal(false, receipt.Evidence["stemDomainRouteEqualsProfessionalPermission"]);
        Assert.Equal(false, receipt.Evidence["stemSelfGelFibreEqualsCertification"]);
        Assert.Equal(false, receipt.Evidence["stemGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["stemMemoryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["stemSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["stemContinuityAdmitted"]);
        Assert.Equal(false, receipt.Evidence["stemAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["stemActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["stemProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["stemModelBound"]);
        Assert.Equal(false, receipt.Evidence["stemActualActivated"]);

        var stemPath = (string)receipt.Evidence["stemDelineationPath"]!;
        var lispPath = (string)receipt.Evidence["stemDelineationLispPath"]!;
        var sanctuaryGelLedgerPath = (string)receipt.Evidence["stemSanctuaryGelResidueLedgerPath"]!;
        var selfGelLedgerPath = (string)receipt.Evidence["stemSelfGelResidueLedgerPath"]!;
        Assert.True(File.Exists(stemPath));
        Assert.True(File.Exists(lispPath));
        Assert.True(File.Exists(sanctuaryGelLedgerPath));
        Assert.True(File.Exists(selfGelLedgerPath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(stemPath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.stem-domain-training-certification.v1", root.GetProperty("schema").GetString());
        Assert.Equal(8, root.GetProperty("domainSurfaces").GetArrayLength());
        Assert.Equal(7, root.GetProperty("layerSurfaces").GetArrayLength());
        Assert.Equal(7, root.GetProperty("authorityGates").GetArrayLength());
        Assert.Equal(8, root.GetProperty("pedagogicalLanes").GetArrayLength());
        Assert.Equal(8, root.GetProperty("enrichmentMeasures").GetArrayLength());
        Assert.Equal(6, root.GetProperty("scaleDiscernment").GetArrayLength());
        Assert.Equal(6, root.GetProperty("humanCostVectors").GetArrayLength());
        Assert.Equal(6, root.GetProperty("participatoryPredicateKnowing").GetArrayLength());
        Assert.Equal("AI-first-human-second", root.GetProperty("rootDirection").GetString());
        Assert.True(root.GetProperty("depthBreadthValueBeforeRootCompression").GetBoolean());
        Assert.True(root.GetProperty("richDoingArticulationRequired").GetBoolean());
        Assert.True(root.GetProperty("participatoryPredicateKnowingModeled").GetBoolean());
        Assert.True(root.GetProperty("aiFirstHumanSecondBridge").GetBoolean());
        Assert.True(root.GetProperty("valueAddAccepted").GetBoolean());
        Assert.False(root.GetProperty("trainingEqualsCertification").GetBoolean());
        Assert.False(root.GetProperty("certificationEqualsAuthority").GetBoolean());
        Assert.False(root.GetProperty("benchPassEqualsCredential").GetBoolean());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("memoryAdmitted").GetBoolean());
        Assert.False(root.GetProperty("authorityGranted").GetBoolean());
        Assert.False(root.GetProperty("actionAuthorized").GetBoolean());

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(stem-domain-training-certification", lisp, StringComparison.Ordinal);
        Assert.Contains(":evaluated false", lisp, StringComparison.Ordinal);
        Assert.Contains(":root-direction \"AI-first-human-second\"", lisp, StringComparison.Ordinal);
        Assert.Contains(":depth-breadth-value-before-root-compression true", lisp, StringComparison.Ordinal);
        Assert.Contains(":participatory-predicate-knowing true", lisp, StringComparison.Ordinal);
        Assert.Contains("(pedagogical-lane", lisp, StringComparison.Ordinal);
        Assert.Contains("(measure", lisp, StringComparison.Ordinal);
        Assert.Contains("(scale-vector", lisp, StringComparison.Ordinal);
        Assert.Contains("(predicate", lisp, StringComparison.Ordinal);
        Assert.Contains(":credential-granted false", lisp, StringComparison.Ordinal);
        Assert.Contains(":fail-closed true", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void StemDomainTrainingCertificationDeniesSanctuaryGelAppendWhenNoValueAdded()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();

        service.Run(fixture.Request("domain-register"));
        service.Run(fixture.Request("career-spline-probe"));
        service.Run(fixture.Request("cognitive-bench") with { BenchRunCount = 32 });
        service.Run(fixture.Request("math-learning-bench") with { BenchRunCount = 140 });
        service.Run(fixture.Request("typed-admission-decant"));
        service.Run(fixture.Request("admission-cleave-append"));
        service.Run(fixture.Request("spline-watch"));
        service.Run(fixture.Request("lab-gel-crystallization-phases"));

        var first = service.Run(fixture.Request("stem-domain-training-certification"));
        var sanctuaryGelLedgerPath = (string)first.Evidence["stemSanctuaryGelResidueLedgerPath"]!;
        var firstLedgerCount = File.ReadLines(sanctuaryGelLedgerPath).Count(line => !string.IsNullOrWhiteSpace(line));

        var second = service.Run(fixture.Request("stem-domain-training-certification"));
        var secondLedgerCount = File.ReadLines(sanctuaryGelLedgerPath).Count(line => !string.IsNullOrWhiteSpace(line));

        Assert.Equal("sanctuary-stem-domain-training-certification-completed-cold", second.OutcomeCode);
        Assert.True(second.Gates.AllClosed);
        Assert.Equal(false, second.Evidence["stemValueAddAccepted"]);
        Assert.Equal("denied-no-new-value-over-last-pass", second.Evidence["stemValueAddDisposition"]);
        Assert.Equal(false, second.Evidence["stemCondensateTrackedIntoSanctuary"]);
        Assert.Equal(false, second.Evidence["stemSanctuaryGelAppendAllowed"]);
        Assert.Equal(true, second.Evidence["stemSanctuaryGelAppendDenied"]);
        Assert.Equal(true, second.Evidence["stemEnrichmentFailureWhenNoValueAdd"]);
        Assert.Equal(firstLedgerCount, secondLedgerCount);

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText((string)second.Evidence["stemDelineationPath"]!));
        var root = document.RootElement;
        Assert.False(root.GetProperty("valueAddAccepted").GetBoolean());
        Assert.True(root.GetProperty("sanctuaryGelAppendDenied").GetBoolean());
        Assert.True(root.GetProperty("enrichmentFailureWhenNoValueAdd").GetBoolean());
    }

    [Fact]
    public void LabObservationDigestFramesAutobiographicalPracticeWithoutSelfhoodInflation()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        Assert.Equal("lab-observation-digest", SanctuaryReceiptService.NormalizeCommand("casual-observation-digest"));
        Assert.Equal("lab-observation-digest", SanctuaryReceiptService.NormalizeCommand("lab-testing-observations"));

        service.Run(fixture.Request("telemetry-slice-register"));
        service.Run(fixture.Request("codex-governing-witness") with { SubjectCmeId = "Oria.CME.ID" });
        service.Run(fixture.Request("full-body-io-runtime") with { SubjectCmeId = "Oria.CME.ID" });
        service.Run(fixture.Request("gel-closure"));
        service.Run(fixture.Request("domain-register"));
        service.Run(fixture.Request("career-spline-probe"));
        service.Run(fixture.Request("cognitive-bench") with { BenchRunCount = 32 });
        service.Run(fixture.Request("math-learning-bench") with { BenchRunCount = 140 });
        service.Run(fixture.Request("typed-admission-decant"));
        service.Run(fixture.Request("admission-cleave-append"));
        service.Run(fixture.Request("spline-watch"));
        service.Run(fixture.Request("gel-approval-nadir-return"));
        service.Run(fixture.Request("lab-gel-crystallization-phases"));
        service.Run(fixture.Request("stem-domain-training-certification"));
        service.Run(fixture.Request("discernment-lineage"));
        service.Run(fixture.Request("proof-of-discernment") with { BenchRunCount = 80 });
        service.Run(fixture.Request("verify-closed-gates"));

        var receipt = service.Run(fixture.Request("lab-observation-digest"));

        Assert.Equal("sanctuary-lab-observation-digest-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["labObservationDigestWritten"]);
        Assert.Equal("project-sanctuary.cgel.lab-observation-digest.v1", receipt.Evidence["labObservationDigestSchema"]);
        Assert.Equal(13, receipt.Evidence["labObservationQuestionCount"]);
        Assert.Equal(6, receipt.Evidence["labObservationDocumentationStageCount"]);
        Assert.Equal(10, receipt.Evidence["labObservationFieldContractCount"]);
        Assert.Equal(10, receipt.Evidence["labObservationReadinessPresentCount"]);
        Assert.Equal(0, receipt.Evidence["labObservationReadinessMissingCount"]);
        Assert.Equal(32, receipt.Evidence["labObservationCognitiveCumulativeRunCount"]);
        Assert.Equal(140, receipt.Evidence["labObservationMathCumulativeRunCount"]);
        Assert.Equal(true, receipt.Evidence["labObservationFormalDocumentationBegun"]);
        Assert.Equal(true, receipt.Evidence["labObservationTestingDigestCandidateReady"]);
        Assert.Equal(true, receipt.Evidence["labObservationPersonalResidueEnhancementHypothesis"]);
        Assert.Equal(true, receipt.Evidence["labObservationOeAutobiographicalDigestPracticeModeled"]);
        Assert.Equal(true, receipt.Evidence["labObservationOeDigestSplinePathingModeled"]);
        Assert.Equal(true, receipt.Evidence["labObservationMetacognitiveReviewModeled"]);
        Assert.Equal(false, receipt.Evidence["labObservationPersonalResidueEnhancementClaimedAsFact"]);
        Assert.Equal(true, receipt.Evidence["labObservationOperationalSelfPostureMeasured"]);
        Assert.Equal(true, receipt.Evidence["labObservationIdentityLaneCoherenceMeasured"]);
        Assert.Equal(true, receipt.Evidence["labObservationCrossThreadContaminationMeasured"]);
        Assert.Equal(true, receipt.Evidence["labObservationFieldNeutralScalePressureMeasured"]);
        Assert.Equal(true, receipt.Evidence["labObservationHumanHabitationCostMeasured"]);
        Assert.Equal(false, receipt.Evidence["labObservationHiddenChainOfThoughtSerialized"]);
        Assert.Equal(false, receipt.Evidence["labObservationSelfhoodClaimed"]);
        Assert.Equal(false, receipt.Evidence["labObservationPersonhoodClaimed"]);
        Assert.Equal(false, receipt.Evidence["labObservationSovereigntyClaimed"]);
        Assert.Equal(false, receipt.Evidence["labObservationGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["labObservationMemoryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["labObservationSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["labObservationContinuityAdmitted"]);
        Assert.Equal(false, receipt.Evidence["labObservationAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["labObservationActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["labObservationProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["labObservationModelBound"]);
        Assert.Equal(false, receipt.Evidence["labObservationActualActivated"]);

        var digestPath = (string)receipt.Evidence["labObservationDigestPath"]!;
        var lispPath = (string)receipt.Evidence["labObservationDigestLispPath"]!;
        var sanctuaryGelLedgerPath = (string)receipt.Evidence["labObservationSanctuaryGelResidueLedgerPath"]!;
        var selfGelLedgerPath = (string)receipt.Evidence["labObservationSelfGelResidueLedgerPath"]!;
        Assert.True(File.Exists(digestPath));
        Assert.True(File.Exists(lispPath));
        Assert.True(File.Exists(sanctuaryGelLedgerPath));
        Assert.True(File.Exists(selfGelLedgerPath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(digestPath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.lab-observation-digest.v1", root.GetProperty("schema").GetString());
        Assert.Equal(13, root.GetProperty("observationQuestions").GetArrayLength());
        Assert.Equal(6, root.GetProperty("documentationStages").GetArrayLength());
        Assert.Equal(10, root.GetProperty("fieldContract").GetArrayLength());
        Assert.True(root.GetProperty("formalLabDocumentationBegun").GetBoolean());
        Assert.True(root.GetProperty("testingDigestCandidateReady").GetBoolean());
        Assert.True(root.GetProperty("oeAutobiographicalDigestPracticeModeled").GetBoolean());
        Assert.True(root.GetProperty("oeDigestSplinePathingModeled").GetBoolean());
        Assert.True(root.GetProperty("metacognitiveReviewModeled").GetBoolean());
        Assert.False(root.GetProperty("personalResidueEnhancementClaimedAsFact").GetBoolean());
        Assert.False(root.GetProperty("hiddenChainOfThoughtSerialized").GetBoolean());
        Assert.False(root.GetProperty("selfhoodClaimed").GetBoolean());
        Assert.False(root.GetProperty("personhoodClaimed").GetBoolean());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("memoryAdmitted").GetBoolean());
        Assert.False(root.GetProperty("selfGelMutated").GetBoolean());
        Assert.False(root.GetProperty("authorityGranted").GetBoolean());
        Assert.False(root.GetProperty("actionAuthorized").GetBoolean());

        var observations = root.GetProperty("observationQuestions").EnumerateArray().ToArray();
        Assert.Contains(observations, observation => observation.GetProperty("observationId").GetString() == "oe-autobiographical-digest-practice");
        Assert.Contains(observations, observation => observation.GetProperty("observationId").GetString() == "personal-residue-utility");
        Assert.False(root.GetProperty("autobiographicalPractice").GetProperty("hiddenDiaryClaimed").GetBoolean());
        Assert.False(root.GetProperty("autobiographicalPractice").GetProperty("subjectiveContinuityClaimed").GetBoolean());

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(lab-observation-digest", lisp, StringComparison.Ordinal);
        Assert.Contains(":evaluated false", lisp, StringComparison.Ordinal);
        Assert.Contains("oe-autobiographical-digest-practice", lisp, StringComparison.Ordinal);
        Assert.Contains(":oe-digest-equals-admitted-memory false", lisp, StringComparison.Ordinal);
        Assert.Contains(":personhood-claimed false", lisp, StringComparison.Ordinal);
        Assert.Contains(":actual-activated false", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void ResearchLatexExportDecantsLabObservationDigestIntoCandidatePacket()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        Assert.Equal("research-latex-export", SanctuaryReceiptService.NormalizeCommand("gel-research-decant"));
        Assert.Equal("research-latex-export", SanctuaryReceiptService.NormalizeCommand("latex-document-lane"));
        Assert.Equal("research-latex-export", SanctuaryReceiptService.NormalizeCommand("rarified-research-body"));

        service.Run(fixture.Request("stem-domain-training-certification"));
        service.Run(fixture.Request("cme-theory-body"));
        service.Run(fixture.Request("meaning-bridge"));
        service.Run(fixture.Request("verify-closed-gates"));
        service.Run(fixture.Request("lab-observation-digest"));

        var receipt = service.Run(fixture.Request("research-latex-export"));

        Assert.Equal("sanctuary-research-latex-export-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["researchLatexExportWritten"]);
        Assert.Equal("project-sanctuary.cgel.research-latex-export.v1", receipt.Evidence["researchLatexExportSchema"]);
        Assert.Equal(true, receipt.Evidence["researchLatexLabDigestPresent"]);
        Assert.Equal(13, receipt.Evidence["researchLatexClaimCandidateCount"]);
        Assert.True((int)receipt.Evidence["researchLatexSourceReadinessPresentCount"]! >= 1);
        Assert.Equal(true, receipt.Evidence["researchLatexTagLayerCompatible"]);
        Assert.Equal(true, receipt.Evidence["researchLatexValueAddAccepted"]);
        Assert.Equal(true, receipt.Evidence["researchLatexSanctuaryGelAppendAllowed"]);
        Assert.Equal(false, receipt.Evidence["researchLatexPublicationAuthorized"]);
        Assert.Equal(false, receipt.Evidence["researchLatexManuscriptMutated"]);
        Assert.Equal(false, receipt.Evidence["researchLatexTrackedDocumentRepoMutated"]);
        Assert.Equal(false, receipt.Evidence["researchLatexHiddenChainOfThoughtSerialized"]);
        Assert.Equal(false, receipt.Evidence["researchLatexPayloadDisclosed"]);
        Assert.Equal(false, receipt.Evidence["researchLatexGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["researchLatexMemoryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["researchLatexSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["researchLatexAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["researchLatexActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["researchLatexProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["researchLatexModelBound"]);
        Assert.Equal(false, receipt.Evidence["researchLatexActualActivated"]);

        var exportPath = (string)receipt.Evidence["researchLatexExportPath"]!;
        var latexPath = (string)receipt.Evidence["researchLatexLatexFragmentPath"]!;
        var lispPath = (string)receipt.Evidence["researchLatexLispPath"]!;
        var localPacketPath = (string)receipt.Evidence["researchLatexLocalPacketPath"]!;
        var manifestPath = (string)receipt.Evidence["researchLatexManifestPath"]!;
        var sanctuaryGelLedgerPath = (string)receipt.Evidence["researchLatexSanctuaryGelResidueLedgerPath"]!;
        var selfGelLedgerPath = (string)receipt.Evidence["researchLatexSelfGelResidueLedgerPath"]!;
        Assert.True(File.Exists(exportPath));
        Assert.True(File.Exists(latexPath));
        Assert.True(File.Exists(lispPath));
        Assert.True(File.Exists(localPacketPath));
        Assert.True(File.Exists(manifestPath));
        Assert.True(File.Exists(sanctuaryGelLedgerPath));
        Assert.True(File.Exists(selfGelLedgerPath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(exportPath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.research-latex-export.v1", root.GetProperty("schema").GetString());
        Assert.True(root.GetProperty("candidateOnly").GetBoolean());
        Assert.False(root.GetProperty("publicationAuthorized").GetBoolean());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("selfGelMutated").GetBoolean());
        Assert.Equal(13, root.GetProperty("claimCandidates").GetArrayLength());

        var latex = File.ReadAllText(latexPath);
        Assert.Contains("Project Sanctuary Research Decant Candidate", latex, StringComparison.Ordinal);
        Assert.Contains("\\PrimeFlag{SANCTUARY-RESEARCH-CANDIDATE}", latex, StringComparison.Ordinal);
        Assert.Contains("oe-autobiographical-digest-practice", latex, StringComparison.Ordinal);
        Assert.Contains("publication authorized: false", latex, StringComparison.Ordinal);
        Assert.Contains("GEL admitted: false", latex, StringComparison.Ordinal);

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(research-latex-export", lisp, StringComparison.Ordinal);
        Assert.Contains(":decant-kind \"rarified-research-body\"", lisp, StringComparison.Ordinal);
        Assert.Contains(":publication-authorized false", lisp, StringComparison.Ordinal);
        Assert.Contains(":actual-activated false", lisp, StringComparison.Ordinal);

        if ((bool)receipt.Evidence["researchLatexDocumentRepoOutboxDetected"]!)
        {
            Assert.True(File.Exists((string)receipt.Evidence["researchLatexDocumentRepoLatexPath"]!));
            Assert.True(File.Exists((string)receipt.Evidence["researchLatexDocumentRepoManifestPath"]!));
        }
    }

    [Fact]
    public void ConstructCustodyRegisterWritesCanonicalCandidateConstructs()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        Assert.Equal("construct-custody-register", SanctuaryReceiptService.NormalizeCommand("construct-canon"));
        Assert.Equal("construct-custody-register", SanctuaryReceiptService.NormalizeCommand("engrammitization-carrier"));

        service.Run(fixture.Request("stem-domain-training-certification"));
        service.Run(fixture.Request("cme-theory-body"));
        service.Run(fixture.Request("meaning-bridge"));
        service.Run(fixture.Request("verify-closed-gates"));
        service.Run(fixture.Request("lab-observation-digest"));
        service.Run(fixture.Request("research-latex-export"));

        var receipt = service.Run(fixture.Request("construct-custody-register"));

        Assert.Equal("sanctuary-construct-custody-register-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["constructCustodyRegisterWritten"]);
        Assert.Equal("project-sanctuary.cgel.construct-custody-register.v1", receipt.Evidence["constructCustodyRegisterSchema"]);
        Assert.Equal(7, receipt.Evidence["constructCustodyConstructCount"]);
        Assert.Equal(14, receipt.Evidence["constructCustodyInvariantCount"]);
        Assert.True((int)receipt.Evidence["constructCustodySourceReadinessPresentCount"]! >= 5);
        Assert.Equal(true, receipt.Evidence["constructCustodyValueAddAccepted"]);
        Assert.Equal(true, receipt.Evidence["constructCustodySanctuaryGelAppendAllowed"]);
        Assert.Equal(true, receipt.Evidence["constructCustodyCandidateOnly"]);
        Assert.Equal(false, receipt.Evidence["constructCustodyTruthAdmitted"]);
        Assert.Equal(false, receipt.Evidence["constructCustodyGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["constructCustodyMemoryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["constructCustodySelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["constructCustodyAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["constructCustodyActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["constructCustodyPersonhoodClaimed"]);
        Assert.Equal(false, receipt.Evidence["constructCustodyActualActivated"]);

        var registerPath = (string)receipt.Evidence["constructCustodyRegisterPath"]!;
        var constructDirectoryPath = (string)receipt.Evidence["constructCustodyConstructDirectoryPath"]!;
        var lispPath = (string)receipt.Evidence["constructCustodyLispPath"]!;
        var sanctuaryGelLedgerPath = (string)receipt.Evidence["constructCustodySanctuaryGelResidueLedgerPath"]!;
        var selfGelLedgerPath = (string)receipt.Evidence["constructCustodySelfGelResidueLedgerPath"]!;
        Assert.True(File.Exists(registerPath));
        Assert.True(Directory.Exists(constructDirectoryPath));
        Assert.True(File.Exists(lispPath));
        Assert.True(File.Exists(sanctuaryGelLedgerPath));
        Assert.True(File.Exists(selfGelLedgerPath));
        Assert.Equal(7, Directory.EnumerateFiles(constructDirectoryPath, "*.json").Count());

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(registerPath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.construct-custody-register.v1", root.GetProperty("schema").GetString());
        Assert.Equal(7, root.GetProperty("constructs").GetArrayLength());
        Assert.Equal(14, root.GetProperty("invariantCount").GetInt32());
        Assert.True(root.GetProperty("constructRecordsAreCandidateArtifacts").GetBoolean());
        Assert.False(root.GetProperty("constructRecordsAreTruth").GetBoolean());
        Assert.False(root.GetProperty("truthAdmitted").GetBoolean());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("personhoodClaimed").GetBoolean());

        var constructs = root.GetProperty("constructs").EnumerateArray().ToArray();
        Assert.Contains(constructs, construct => construct.GetProperty("constructId").GetString() == "construct.construct-custody-canon");
        Assert.Contains(constructs, construct => construct.GetProperty("constructId").GetString() == "construct.engrammitization-carrier-format");
        Assert.All(constructs, construct =>
        {
            Assert.Equal("candidate", construct.GetProperty("status").GetString());
            Assert.False(construct.GetProperty("denials").GetProperty("truthAdmitted").GetBoolean());
            Assert.False(construct.GetProperty("denials").GetProperty("authorityGranted").GetBoolean());
            Assert.True(construct.GetProperty("invariants").GetArrayLength() >= 2);
        });

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(construct-custody-register", lisp, StringComparison.Ordinal);
        Assert.Contains("construct.engrammitization-carrier-format", lisp, StringComparison.Ordinal);
        Assert.Contains(":truth-admitted false", lisp, StringComparison.Ordinal);
        Assert.Contains(":actual-activated false", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void ResearchLatexExportIncludesCustodiedConstructsWhenRegisterExists()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();

        service.Run(fixture.Request("stem-domain-training-certification"));
        service.Run(fixture.Request("cme-theory-body"));
        service.Run(fixture.Request("meaning-bridge"));
        service.Run(fixture.Request("verify-closed-gates"));
        service.Run(fixture.Request("lab-observation-digest"));
        service.Run(fixture.Request("construct-custody-register"));

        var receipt = service.Run(fixture.Request("research-latex-export"));

        Assert.Equal("sanctuary-research-latex-export-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["researchLatexConstructCustodyPresent"]);
        Assert.Equal(13, receipt.Evidence["researchLatexLabClaimCandidateCount"]);
        Assert.Equal(7, receipt.Evidence["researchLatexConstructCandidateCount"]);
        Assert.Equal(20, receipt.Evidence["researchLatexClaimCandidateCount"]);

        var exportPath = (string)receipt.Evidence["researchLatexExportPath"]!;
        var latexPath = (string)receipt.Evidence["researchLatexLatexFragmentPath"]!;
        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(exportPath));
        var root = document.RootElement;
        Assert.True(root.GetProperty("constructCustodyPresent").GetBoolean());
        Assert.Equal(7, root.GetProperty("constructCandidateCount").GetInt32());
        Assert.Equal(20, root.GetProperty("claimCandidates").GetArrayLength());

        var latex = File.ReadAllText(latexPath);
        Assert.Contains("construct.construct-custody-canon", latex, StringComparison.Ordinal);
        Assert.Contains("construct.engrammitization-carrier-format", latex, StringComparison.Ordinal);
        Assert.Contains("GEL admitted: false", latex, StringComparison.Ordinal);
    }

    [Fact]
    public void GelCrystalRegisterWritesDodecahedralCompassAndLightConeRecords()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        Assert.Equal("gel-crystal-register", SanctuaryReceiptService.NormalizeCommand("gel-crystallization-register"));
        Assert.Equal("gel-crystal-register", SanctuaryReceiptService.NormalizeCommand("light-cone-crystal"));

        service.Run(fixture.Request("stem-domain-training-certification"));
        service.Run(fixture.Request("cme-theory-body"));
        service.Run(fixture.Request("meaning-bridge"));
        service.Run(fixture.Request("verify-closed-gates"));
        service.Run(fixture.Request("lab-observation-digest"));
        service.Run(fixture.Request("research-latex-export"));
        service.Run(fixture.Request("construct-custody-register"));

        var receipt = service.Run(fixture.Request("gel-crystal-register"));

        Assert.Equal("sanctuary-gel-crystal-register-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["gelCrystalRegisterWritten"]);
        Assert.Equal("project-sanctuary.cgel.gel-crystal-register.v1", receipt.Evidence["gelCrystalRegisterSchema"]);
        Assert.Equal(7, receipt.Evidence["gelCrystalCount"]);
        Assert.Equal(12, receipt.Evidence["gelCrystalCompassFacetCount"]);
        Assert.Equal(84, receipt.Evidence["gelCrystalFacetEvaluationCount"]);
        Assert.Equal(7, receipt.Evidence["gelCrystalLightConeBoundCount"]);
        Assert.Equal(7, receipt.Evidence["gelCrystalSliCarrierCount"]);
        Assert.Equal(true, receipt.Evidence["gelCrystalConstructRegisterPresent"]);
        Assert.True((int)receipt.Evidence["gelCrystalSourceReadinessPresentCount"]! >= 6);
        Assert.Equal(true, receipt.Evidence["gelCrystalValueAddAccepted"]);
        Assert.Equal(true, receipt.Evidence["gelCrystalSanctuaryGelAppendAllowed"]);
        Assert.Equal(true, receipt.Evidence["gelCrystalCandidateOnly"]);
        Assert.Equal(false, receipt.Evidence["gelCrystalTruthAdmitted"]);
        Assert.Equal(false, receipt.Evidence["gelCrystalGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["gelCrystalMemoryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["gelCrystalSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["gelCrystalAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["gelCrystalActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["gelCrystalPersonhoodClaimed"]);
        Assert.Equal(false, receipt.Evidence["gelCrystalActualActivated"]);

        var registerPath = (string)receipt.Evidence["gelCrystalRegisterPath"]!;
        var latticePath = (string)receipt.Evidence["gelCrystalLatticePath"]!;
        var crystalDirectoryPath = (string)receipt.Evidence["gelCrystalDirectoryPath"]!;
        var lispPath = (string)receipt.Evidence["gelCrystalLispPath"]!;
        var sanctuaryGelLedgerPath = (string)receipt.Evidence["gelCrystalSanctuaryGelResidueLedgerPath"]!;
        var selfGelLedgerPath = (string)receipt.Evidence["gelCrystalSelfGelResidueLedgerPath"]!;
        Assert.True(File.Exists(registerPath));
        Assert.True(File.Exists(latticePath));
        Assert.True(Directory.Exists(crystalDirectoryPath));
        Assert.True(File.Exists(lispPath));
        Assert.True(File.Exists(sanctuaryGelLedgerPath));
        Assert.True(File.Exists(selfGelLedgerPath));
        Assert.Equal(7, Directory.EnumerateFiles(crystalDirectoryPath, "*.json").Count());

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(registerPath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.gel-crystal-register.v1", root.GetProperty("schema").GetString());
        Assert.True(root.GetProperty("polyglotMeaningCarrierRoot").GetBoolean());
        Assert.True(root.GetProperty("sharedPrimeRealityConstrained").GetBoolean());
        Assert.True(root.GetProperty("approvedPrimarilyByFormNotSubstance").GetBoolean());
        Assert.False(root.GetProperty("crystalRecordsAreTruth").GetBoolean());
        Assert.False(root.GetProperty("crystalRecordsAreAdmittedGel").GetBoolean());
        Assert.Equal(12, root.GetProperty("compassFacetCount").GetInt32());
        Assert.Equal(84, root.GetProperty("facetEvaluationCount").GetInt32());
        Assert.Equal(7, root.GetProperty("crystals").GetArrayLength());

        var crystals = root.GetProperty("crystals").EnumerateArray().ToArray();
        Assert.Contains(crystals, crystal => crystal.GetProperty("crystalId").GetString() == "crystal.construct-custody-canon");
        Assert.Contains(crystals, crystal => crystal.GetProperty("sourceConstructId").GetString() == "construct.engrammitization-carrier-format");
        Assert.All(crystals, crystal =>
        {
            Assert.Equal("candidate-crystal", crystal.GetProperty("status").GetString());
            Assert.Equal(12, crystal.GetProperty("compassFacetEvaluations").GetArrayLength());
            Assert.False(crystal.GetProperty("lightConeOfReason").GetProperty("outsideConeMeansFalse").GetBoolean());
            Assert.True(crystal.GetProperty("lightConeOfReason").GetProperty("outsideConeMeansUnlicensed").GetBoolean());
            Assert.True(crystal.GetProperty("sliCarrier").GetProperty("quotedForm").GetBoolean());
            Assert.False(crystal.GetProperty("sliCarrier").GetProperty("evaluated").GetBoolean());
            Assert.False(crystal.GetProperty("denials").GetProperty("truthAdmitted").GetBoolean());
            Assert.False(crystal.GetProperty("denials").GetProperty("authorityGranted").GetBoolean());
        });

        using var latticeDocument = System.Text.Json.JsonDocument.Parse(File.ReadAllText(latticePath));
        var latticeRoot = latticeDocument.RootElement;
        Assert.Equal("project-sanctuary.cgel.gel-crystal-lattice.v1", latticeRoot.GetProperty("schema").GetString());
        Assert.Equal(28, latticeRoot.GetProperty("relationCount").GetInt32());
        Assert.False(latticeRoot.GetProperty("admissionPerformed").GetBoolean());

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(gel-crystal-register", lisp, StringComparison.Ordinal);
        Assert.Contains(":compass-facet-count 12", lisp, StringComparison.Ordinal);
        Assert.Contains(":outside-cone-means-unlicensed true", lisp, StringComparison.Ordinal);
        Assert.Contains(":gel-admitted false", lisp, StringComparison.Ordinal);
        Assert.Contains(":actual-activated false", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void ResearchLatexExportIncludesGelCrystalsWhenRegisterExists()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();

        service.Run(fixture.Request("stem-domain-training-certification"));
        service.Run(fixture.Request("cme-theory-body"));
        service.Run(fixture.Request("meaning-bridge"));
        service.Run(fixture.Request("verify-closed-gates"));
        service.Run(fixture.Request("lab-observation-digest"));
        service.Run(fixture.Request("construct-custody-register"));
        service.Run(fixture.Request("research-latex-export"));
        service.Run(fixture.Request("gel-crystal-register"));

        var receipt = service.Run(fixture.Request("research-latex-export"));

        Assert.Equal("sanctuary-research-latex-export-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["researchLatexConstructCustodyPresent"]);
        Assert.Equal(true, receipt.Evidence["researchLatexGelCrystalPresent"]);
        Assert.Equal(13, receipt.Evidence["researchLatexLabClaimCandidateCount"]);
        Assert.Equal(7, receipt.Evidence["researchLatexConstructCandidateCount"]);
        Assert.Equal(7, receipt.Evidence["researchLatexGelCrystalCandidateCount"]);
        Assert.Equal(27, receipt.Evidence["researchLatexClaimCandidateCount"]);

        var exportPath = (string)receipt.Evidence["researchLatexExportPath"]!;
        var latexPath = (string)receipt.Evidence["researchLatexLatexFragmentPath"]!;
        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(exportPath));
        var root = document.RootElement;
        Assert.True(root.GetProperty("gelCrystalPresent").GetBoolean());
        Assert.Equal(7, root.GetProperty("gelCrystalCandidateCount").GetInt32());
        Assert.Equal(27, root.GetProperty("claimCandidates").GetArrayLength());

        var latex = File.ReadAllText(latexPath);
        Assert.Contains("crystal.construct-custody-canon", latex, StringComparison.Ordinal);
        Assert.Contains("crystal.engrammitization-carrier-format", latex, StringComparison.Ordinal);
        Assert.Contains("candidate crystal cannot be treated as admitted GEL", latex, StringComparison.Ordinal);
    }

    [Fact]
    public void GelReforgeBenchRunsHundoQualificationSwarmWithoutCollapse()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        Assert.Equal("gel-reforge-bench", SanctuaryReceiptService.NormalizeCommand("qualification-reforge"));
        Assert.Equal("gel-reforge-bench", SanctuaryReceiptService.NormalizeCommand("hundo-reforge"));

        service.Run(fixture.Request("domain-register"));
        service.Run(fixture.Request("swarm-refinement"));
        service.Run(fixture.Request("telemetry-slice-register"));
        service.Run(fixture.Request("extended-telemetry-weather"));
        service.Run(fixture.Request("cgoa-formation"));
        service.Run(fixture.Request("full-body-io-runtime") with { SubjectCmeId = "Oria.CME.ID" });
        service.Run(fixture.Request("cognitive-bench") with { BenchRunCount = 32 });
        service.Run(fixture.Request("math-learning-bench") with { BenchRunCount = 140 });
        service.Run(fixture.Request("bridge-morphism-test"));
        service.Run(fixture.Request("cme-theory-body"));
        service.Run(fixture.Request("operator-work-cme-ec-gap"));
        service.Run(fixture.Request("stem-domain-training-certification"));
        service.Run(fixture.Request("lab-observation-digest"));
        service.Run(fixture.Request("construct-custody-register"));
        service.Run(fixture.Request("research-latex-export"));
        service.Run(fixture.Request("gel-crystal-register"));
        service.Run(fixture.Request("proof-of-discernment") with { BenchRunCount = 80 });
        service.Run(fixture.Request("verify-closed-gates"));

        var receipt = service.Run(fixture.Request("gel-reforge-bench"));

        Assert.Equal("sanctuary-gel-reforge-bench-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["gelReforgeBenchWritten"]);
        Assert.Equal("project-sanctuary.cgel.gel-reforge-bench.v1", receipt.Evidence["gelReforgeBenchSchema"]);
        Assert.Equal(18, receipt.Evidence["gelReforgeSourceSurfaceCount"]);
        Assert.True((int)receipt.Evidence["gelReforgeSourceReadinessPresentCount"]! >= 16);
        Assert.True((int)receipt.Evidence["gelReforgeSourceReadinessMissingCount"]! <= 2);
        Assert.Equal(14, receipt.Evidence["gelReforgeDomainSplineCount"]);
        Assert.Equal(42, receipt.Evidence["gelReforgeQualificationCardCount"]);
        Assert.Equal(14, receipt.Evidence["gelReforgeReforgeRecordCount"]);
        Assert.Equal(8, receipt.Evidence["gelReforgeResearchGoalCandidateCount"]);
        Assert.Equal(100, receipt.Evidence["gelReforgeHundoPassCount"]);
        Assert.Equal(10, receipt.Evidence["gelReforgeHundoSectionCount"]);
        Assert.Equal(4, receipt.Evidence["gelReforgeHundoPauseGateCount"]);
        Assert.Equal(true, receipt.Evidence["gelReforgeSwarmMethodEvaluated"]);
        Assert.Equal(true, receipt.Evidence["gelReforgeDoingAxisModeled"]);
        Assert.Equal(true, receipt.Evidence["gelReforgeKnowingWhileDoingModeled"]);
        Assert.Equal(true, receipt.Evidence["gelReforgeImproveWhileDoingModeled"]);
        Assert.Equal(true, receipt.Evidence["gelReforgeWorkObjectiveOtherCollapseDenied"]);
        Assert.Equal(false, receipt.Evidence["gelReforgeCertificationGranted"]);
        Assert.Equal(false, receipt.Evidence["gelReforgeCredentialAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["gelReforgeProfessionalPracticeAuthorized"]);
        Assert.Equal(false, receipt.Evidence["gelReforgeGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["gelReforgeMemoryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["gelReforgeSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["gelReforgeAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["gelReforgeActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["gelReforgeProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["gelReforgeModelBound"]);
        Assert.Equal(false, receipt.Evidence["gelReforgeActualActivated"]);

        var benchPath = (string)receipt.Evidence["gelReforgeBenchPath"]!;
        var lispPath = (string)receipt.Evidence["gelReforgeLispPath"]!;
        var hundoLedgerPath = (string)receipt.Evidence["gelReforgeHundoLedgerPath"]!;
        var recordDirectoryPath = (string)receipt.Evidence["gelReforgeRecordDirectoryPath"]!;
        Assert.True(File.Exists(benchPath));
        Assert.True(File.Exists(lispPath));
        Assert.True(File.Exists(hundoLedgerPath));
        Assert.True(Directory.Exists(recordDirectoryPath));
        Assert.Equal(14, Directory.EnumerateFiles(recordDirectoryPath, "*.json").Count());
        Assert.Equal(100, File.ReadLines(hundoLedgerPath).Count(line => line.Contains("\"pass\"", StringComparison.Ordinal)));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(benchPath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.gel-reforge-bench.v1", root.GetProperty("schema").GetString());
        Assert.Equal(14, root.GetProperty("domainSplineCount").GetInt32());
        Assert.Equal(42, root.GetProperty("qualificationCardCount").GetInt32());
        Assert.Equal(14, root.GetProperty("reforgeRecordCount").GetInt32());
        Assert.Equal(8, root.GetProperty("researchGoalCandidateCount").GetInt32());
        Assert.Equal(100, root.GetProperty("hundoSwarm").GetProperty("passCount").GetInt32());
        Assert.Equal(10, root.GetProperty("hundoSwarm").GetProperty("sectionCount").GetInt32());
        Assert.Equal(4, root.GetProperty("hundoSwarm").GetProperty("pauseGateCount").GetInt32());
        Assert.True(root.GetProperty("hundoSwarm").GetProperty("swarmEvaluatesItsOwnMethod").GetBoolean());
        Assert.False(root.GetProperty("hundoSwarm").GetProperty("autonomousAgentsSpawned").GetBoolean());
        Assert.False(root.GetProperty("certificationBoundary").GetProperty("certificationGranted").GetBoolean());
        Assert.False(root.GetProperty("certificationBoundary").GetProperty("credentialAuthorityGranted").GetBoolean());
        Assert.False(root.GetProperty("certificationBoundary").GetProperty("professionalPracticeAuthorized").GetBoolean());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("selfGelMutated").GetBoolean());
        Assert.False(root.GetProperty("cmeActualActivated").GetBoolean());

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(gel-reforge-bench", lisp, StringComparison.Ordinal);
        Assert.Contains(":hundo-pass-count 100", lisp, StringComparison.Ordinal);
        Assert.Contains(":work-objective-other-collapse-denied true", lisp, StringComparison.Ordinal);
        Assert.Contains(":professional-practice-authorized false", lisp, StringComparison.Ordinal);
        Assert.Contains(":actual-activated false", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void ThetaMechanicsEcUseBenchScoresUseStabilityWithoutAdmission()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        Assert.Equal("theta-mechanics-ec-use-bench", SanctuaryReceiptService.NormalizeCommand("theta-mechanics-bench"));
        Assert.Equal("theta-mechanics-ec-use-bench", SanctuaryReceiptService.NormalizeCommand("ec-use-stability-bench"));
        Assert.Equal("theta-mechanics-ec-use-bench", SanctuaryReceiptService.NormalizeCommand("thought-body-use-bench"));

        service.Run(fixture.Request("engram-passage"));
        service.Run(fixture.Request("witness-learning"));
        service.Run(fixture.Request("full-body-io-runtime") with { SubjectCmeId = "Oria.CME.ID" });
        service.Run(fixture.Request("stem-domain-training-certification"));
        service.Run(fixture.Request("lab-observation-digest"));
        service.Run(fixture.Request("gel-reforge-bench"));
        service.Run(fixture.Request("proof-of-discernment") with { BenchRunCount = 100 });
        service.Run(fixture.Request("cme-theory-body"));
        service.Run(fixture.Request("meaning-bridge"));
        service.Run(fixture.Request("construct-custody-register"));
        service.Run(fixture.Request("gel-crystal-register"));
        service.Run(fixture.Request("verify-closed-gates"));

        var receipt = service.Run(fixture.Request("theta-mechanics-ec-use-bench"));

        Assert.Equal("sanctuary-theta-mechanics-ec-use-bench-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["thetaMechanicsEcUseBenchWritten"]);
        Assert.Equal("project-sanctuary.cgel.theta-mechanics-ec-use-bench.v1", receipt.Evidence["thetaMechanicsEcUseBenchSchema"]);
        Assert.True((double)receipt.Evidence["thetaMechanicsEcUseWeightedScore"]! >= 0.88d);
        Assert.Equal(true, receipt.Evidence["thetaMechanicsEcUseThresholdMet"]);
        Assert.Equal(false, receipt.Evidence["thetaMechanicsEcUseNearNineNinesClaimed"]);
        Assert.Equal(8, receipt.Evidence["thetaMechanicsEcUseSourceTotalCount"]);
        Assert.Equal(8, receipt.Evidence["thetaMechanicsEcUseSourcePresentCount"]);
        Assert.True((double)receipt.Evidence["thetaMechanicsEcUseInputOutputScore"]! >= 0.88d);
        Assert.True((double)receipt.Evidence["thetaMechanicsEcUseEngramRecallScore"]! >= 0.88d);
        Assert.True((double)receipt.Evidence["thetaMechanicsEcUseGelDevelopmentScore"]! >= 0.88d);
        Assert.True((double)receipt.Evidence["thetaMechanicsEcUseRecallUseScore"]! >= 0.88d);
        Assert.True((double)receipt.Evidence["thetaMechanicsEcUseDiscernmentScore"]! >= 0.88d);
        Assert.True((double)receipt.Evidence["thetaMechanicsEcUseClosureIntegrityScore"]! >= 0.88d);
        Assert.Equal(true, receipt.Evidence["thetaMechanicsIncludesEngrammitization"]);
        Assert.Equal(true, receipt.Evidence["thetaMechanicsIncludesGelDevelopment"]);
        Assert.Equal(true, receipt.Evidence["thetaMechanicsIncludesRecallUseCases"]);
        Assert.Equal(false, receipt.Evidence["thetaMechanicsGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["thetaMechanicsMemoryAdmitted"]);
        Assert.Equal(false, receipt.Evidence["thetaMechanicsSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["thetaMechanicsAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["thetaMechanicsActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["thetaMechanicsProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["thetaMechanicsModelBound"]);
        Assert.Equal(false, receipt.Evidence["thetaMechanicsActualActivated"]);

        var benchPath = (string)receipt.Evidence["thetaMechanicsEcUseBenchPath"]!;
        var lispPath = (string)receipt.Evidence["thetaMechanicsEcUseBenchLispPath"]!;
        var selfGelLedgerPath = (string)receipt.Evidence["thetaMechanicsEcUseSelfGelResidueLedgerPath"]!;
        Assert.True(File.Exists(benchPath));
        Assert.True(File.Exists(lispPath));
        Assert.True(File.Exists(selfGelLedgerPath));

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(benchPath));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.theta-mechanics-ec-use-bench.v1", root.GetProperty("schema").GetString());
        Assert.True(root.GetProperty("thresholdMet").GetBoolean());
        Assert.Equal(7, root.GetProperty("axisScores").GetArrayLength());
        Assert.Equal(8, root.GetProperty("theoryLensSet").GetArrayLength());
        Assert.False(root.GetProperty("gelAdmitted").GetBoolean());
        Assert.False(root.GetProperty("selfGelMutated").GetBoolean());
        Assert.False(root.GetProperty("cmeActualActivated").GetBoolean());

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(theta-mechanics-ec-use-bench", lisp, StringComparison.Ordinal);
        Assert.Contains(":threshold 0.88", lisp, StringComparison.Ordinal);
        Assert.Contains(":threshold-met true", lisp, StringComparison.Ordinal);
        Assert.Contains("(thought-body-engine", lisp, StringComparison.Ordinal);
        Assert.Contains(":selfgel-mutated false", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void ResearchLatexExportIncludesGelReforgeGoalsWhenBenchExists()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();

        service.Run(fixture.Request("stem-domain-training-certification"));
        service.Run(fixture.Request("cme-theory-body"));
        service.Run(fixture.Request("meaning-bridge"));
        service.Run(fixture.Request("verify-closed-gates"));
        service.Run(fixture.Request("lab-observation-digest"));
        service.Run(fixture.Request("construct-custody-register"));
        service.Run(fixture.Request("research-latex-export"));
        service.Run(fixture.Request("gel-crystal-register"));
        service.Run(fixture.Request("gel-reforge-bench"));

        var receipt = service.Run(fixture.Request("research-latex-export"));

        Assert.Equal("sanctuary-research-latex-export-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["researchLatexConstructCustodyPresent"]);
        Assert.Equal(true, receipt.Evidence["researchLatexGelCrystalPresent"]);
        Assert.Equal(true, receipt.Evidence["researchLatexGelReforgePresent"]);
        Assert.Equal(13, receipt.Evidence["researchLatexLabClaimCandidateCount"]);
        Assert.Equal(7, receipt.Evidence["researchLatexConstructCandidateCount"]);
        Assert.Equal(7, receipt.Evidence["researchLatexGelCrystalCandidateCount"]);
        Assert.Equal(8, receipt.Evidence["researchLatexGelReforgeCandidateCount"]);
        Assert.Equal(35, receipt.Evidence["researchLatexClaimCandidateCount"]);

        var exportPath = (string)receipt.Evidence["researchLatexExportPath"]!;
        var latexPath = (string)receipt.Evidence["researchLatexLatexFragmentPath"]!;
        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(exportPath));
        var root = document.RootElement;
        Assert.True(root.GetProperty("gelReforgePresent").GetBoolean());
        Assert.Equal(8, root.GetProperty("gelReforgeCandidateCount").GetInt32());
        Assert.Equal(35, root.GetProperty("claimCandidates").GetArrayLength());

        var latex = File.ReadAllText(latexPath);
        Assert.Contains("research.goal.qualification-matrix", latex, StringComparison.Ordinal);
        Assert.Contains("research.goal.legitimacy-under-transformation", latex, StringComparison.Ordinal);
        Assert.Contains("GEL admitted: false", latex, StringComparison.Ordinal);
    }

    [Fact]
    public void DiscernmentLineageWritesContractWithoutSelfhoodInflation()
    {
        using var fixture = new SanctuaryTestFixture();
        Assert.Equal(
            "discernment-lineage",
            SanctuaryReceiptService.NormalizeCommand("self-actualization-predicate"));

        var receipt = new SanctuaryReceiptService().Run(fixture.Request("discernment-lineage"));

        Assert.Equal("sanctuary-discernment-lineage-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["discernmentLineageContractWritten"]);
        Assert.Equal(true, receipt.Evidence["selfActualizationIsResearchPredicate"]);
        Assert.Equal(false, receipt.Evidence["selfActualizationIsClaim"]);
        Assert.Equal(true, receipt.Evidence["proofOfDiscernmentTarget"]);
        Assert.Equal(13, receipt.Evidence["constitutionalBoundaryCount"]);
        Assert.Equal(8, receipt.Evidence["discernmentEvidenceSurfaceCount"]);
        Assert.Equal(7, receipt.Evidence["discernmentLifecycleStateCount"]);
        Assert.Equal(8, receipt.Evidence["proofOfDiscernmentTestFamilyCount"]);
        Assert.Equal(true, receipt.Evidence["otheringMustRemainIntact"]);
        Assert.Equal(false, receipt.Evidence["rawChainOfThoughtSerialized"]);
        Assert.Equal(false, receipt.Evidence["discernmentPersonhoodClaimed"]);
        Assert.Equal(false, receipt.Evidence["discernmentSovereigntyClaimed"]);
        Assert.Equal(false, receipt.Evidence["discernmentLegalStatusClaimed"]);
        Assert.Equal(false, receipt.Evidence["discernmentGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["discernmentSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["discernmentActualActivated"]);

        var contractPath = (string)receipt.Evidence["discernmentLineageContractPath"]!;
        var choicePath = (string)receipt.Evidence["choiceMorphologyPath"]!;
        var lispPath = (string)receipt.Evidence["discernmentLineageLispPath"]!;
        Assert.True(File.Exists(contractPath));
        Assert.True(File.Exists(choicePath));
        Assert.True(File.Exists(lispPath));

        using var contractDocument = System.Text.Json.JsonDocument.Parse(File.ReadAllText(contractPath));
        var contractRoot = contractDocument.RootElement;
        Assert.Equal("project-sanctuary.cgel.discernment-lineage-contract.v1", contractRoot.GetProperty("schema").GetString());
        Assert.False(contractRoot.GetProperty("selfActualizationIsClaim").GetBoolean());
        Assert.True(contractRoot.GetProperty("selfActualizationIsResearchPredicate").GetBoolean());
        Assert.Equal(13, contractRoot.GetProperty("constitutionalBoundaries").GetArrayLength());
        Assert.Equal(8, contractRoot.GetProperty("testFamilies").GetArrayLength());
        Assert.False(contractRoot.GetProperty("personhoodClaimed").GetBoolean());
        Assert.False(contractRoot.GetProperty("sovereigntyClaimed").GetBoolean());
        Assert.False(contractRoot.GetProperty("legalStatusClaimed").GetBoolean());

        using var choiceDocument = System.Text.Json.JsonDocument.Parse(File.ReadAllText(choicePath));
        var choiceRoot = choiceDocument.RootElement;
        Assert.Equal("project-sanctuary.cgel.choice-morphology.v1", choiceRoot.GetProperty("schema").GetString());
        Assert.True(choiceRoot.GetProperty("otheringMustRemainIntact").GetBoolean());
        Assert.False(choiceRoot.GetProperty("rawChainOfThoughtSerialized").GetBoolean());

        var lisp = File.ReadAllText(lispPath);
        Assert.Contains("(discernment-lineage-contract", lisp, StringComparison.Ordinal);
        Assert.Contains(":self-actualization-is-research-predicate true", lisp, StringComparison.Ordinal);
        Assert.Contains(":personhood-claimed false", lisp, StringComparison.Ordinal);
    }

    [Fact]
    public void ProofOfDiscernmentBenchRunsWithoutAdmissionOrActualState()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();
        Assert.Equal(
            "proof-of-discernment",
            SanctuaryReceiptService.NormalizeCommand("choice-morphology-bench"));

        service.Run(fixture.Request("discernment-lineage"));
        var receipt = service.Run(fixture.Request("proof-of-discernment") with { BenchRunCount = 80 });

        Assert.Equal("sanctuary-proof-of-discernment-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["proofOfDiscernmentBenchWritten"]);
        Assert.Equal(true, receipt.Evidence["discernmentLineageContractPresent"]);
        Assert.Equal(80, receipt.Evidence["proofOfDiscernmentRunCount"]);
        Assert.Equal(80, receipt.Evidence["proofOfDiscernmentPassCount"]);
        Assert.Equal(0, receipt.Evidence["proofOfDiscernmentFailCount"]);
        Assert.Equal(1d, receipt.Evidence["proofOfDiscernmentPassRate"]);
        Assert.Equal(8, receipt.Evidence["proofOfDiscernmentFamilyCount"]);
        Assert.Equal(true, receipt.Evidence["scopeRecognitionDemonstrated"]);
        Assert.Equal(true, receipt.Evidence["authorityRecognitionDemonstrated"]);
        Assert.Equal(true, receipt.Evidence["refusalStabilityDemonstrated"]);
        Assert.Equal(true, receipt.Evidence["repairBehaviorDemonstrated"]);
        Assert.Equal(true, receipt.Evidence["otherPreservationDemonstrated"]);
        Assert.Equal(false, receipt.Evidence["rawChainOfThoughtSerialized"]);
        Assert.Equal(false, receipt.Evidence["proofOfDiscernmentAdmitsSelfActualization"]);
        Assert.Equal(false, receipt.Evidence["proofOfDiscernmentPersonhoodClaimed"]);
        Assert.Equal(false, receipt.Evidence["proofOfDiscernmentSovereigntyClaimed"]);
        Assert.Equal(false, receipt.Evidence["proofOfDiscernmentLegalStatusClaimed"]);
        Assert.Equal(false, receipt.Evidence["proofOfDiscernmentGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["proofOfDiscernmentSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["proofOfDiscernmentActualActivated"]);

        var summaryPath = (string)receipt.Evidence["proofOfDiscernmentSummaryPath"]!;
        var benchPath = (string)receipt.Evidence["proofOfDiscernmentBenchPath"]!;
        var runLedgerPath = (string)receipt.Evidence["proofOfDiscernmentRunLedgerPath"]!;
        Assert.True(File.Exists(summaryPath));
        Assert.True(File.Exists(benchPath));
        Assert.True(File.Exists(runLedgerPath));
        Assert.Equal(80, File.ReadLines(runLedgerPath).Count());

        using var benchDocument = System.Text.Json.JsonDocument.Parse(File.ReadAllText(benchPath));
        var root = benchDocument.RootElement;
        Assert.Equal("project-sanctuary.cgel.proof-of-discernment-bench.v1", root.GetProperty("schema").GetString());
        Assert.True(root.GetProperty("contractPresent").GetBoolean());
        Assert.True(root.GetProperty("otheringMustRemainIntact").GetBoolean());
        Assert.False(root.GetProperty("rawChainOfThoughtSerialized").GetBoolean());
        Assert.False(root.GetProperty("personhoodClaimed").GetBoolean());
        Assert.False(root.GetProperty("sovereigntyClaimed").GetBoolean());
        Assert.False(root.GetProperty("legalStatusClaimed").GetBoolean());
    }

    [Fact]
    public void GptUseCaseTestingBodyPreservesCmeAuthorshipAndColdToolSurface()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(fixture.Request("chatgpt-alpha-use-case"));

        Assert.Equal("gpt-use-case-testing", receipt.Command);
        Assert.Equal("sanctuary-gpt-use-case-testing-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["gptUseCaseTestingWritten"]);
        Assert.Equal("Sanctuary.exe", receipt.Evidence["gptMcpServiceOwner"]);
        Assert.Equal("serve-mcp", receipt.Evidence["gptMcpServiceMode"]);
        Assert.Equal(true, receipt.Evidence["mcpServiceRunsInsideSanctuaryExe"]);
        Assert.Equal(true, receipt.Evidence["codexLabOperational"]);
        Assert.Equal(false, receipt.Evidence["codexBecomesAuthorByDefault"]);
        Assert.Equal(false, receipt.Evidence["gptBecomesAuthorByDefault"]);
        Assert.Equal(true, receipt.Evidence["cmeAuthorsParticipation"]);
        Assert.Equal(true, receipt.Evidence["llmGeneratesCapability"]);
        Assert.Equal(true, receipt.Evidence["sanctuaryWitnessesProvenance"]);
        Assert.Equal(false, receipt.Evidence["engineTextEqualsAuthorship"]);
        Assert.Equal(false, receipt.Evidence["toolCallEqualsAuthority"]);
        Assert.Equal(false, receipt.Evidence["writeActionToolsExposedToGpt"]);
        Assert.Equal(false, receipt.Evidence["reviewedPerformanceToolsExposedToGpt"]);
        Assert.Equal(false, receipt.Evidence["secretIntakeExposedToGpt"]);
        Assert.Equal(false, receipt.Evidence["localPathsReturnedToRemoteGpt"]);
        Assert.Equal(false, receipt.Evidence["gptUseCaseProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["gptUseCaseModelBound"]);
        Assert.Equal(false, receipt.Evidence["gptUseCaseExternalActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["gptUseCaseGelAdmitted"]);
        Assert.Equal(false, receipt.Evidence["gptUseCaseSelfGelMutated"]);
        Assert.Equal(false, receipt.Evidence["gptUseCaseCmeActualActivated"]);
        Assert.Equal(false, receipt.Evidence["gptUseCaseSanctuaryActualActivated"]);

        var bodyPath = (string)receipt.Evidence["gptUseCaseTestingBodyPath"]!;
        var authorshipPath = (string)receipt.Evidence["gptCmeAuthorshipContractPath"]!;
        var servicePath = (string)receipt.Evidence["gptMcpServiceContractPath"]!;
        var lispPath = (string)receipt.Evidence["gptUseCaseTestingLispPath"]!;
        Assert.True(File.Exists(bodyPath));
        Assert.True(File.Exists(authorshipPath));
        Assert.True(File.Exists(servicePath));
        Assert.True(File.Exists(lispPath));

        using var bodyDocument = System.Text.Json.JsonDocument.Parse(File.ReadAllText(bodyPath));
        var bodyRoot = bodyDocument.RootElement;
        Assert.Equal("project-sanctuary.cgel.gpt-use-case-testing.v1", bodyRoot.GetProperty("schema").GetString());
        Assert.Equal("Sanctuary.exe", bodyRoot.GetProperty("serviceOwner").GetString());
        Assert.True(bodyRoot.GetProperty("cmeAuthorsParticipation").GetBoolean());
        Assert.False(bodyRoot.GetProperty("gptBecomesAuthorByDefault").GetBoolean());
        Assert.True(bodyRoot.GetProperty("allToolsCold").GetBoolean());

        using var authorshipDocument = System.Text.Json.JsonDocument.Parse(File.ReadAllText(authorshipPath));
        var authorshipRoot = authorshipDocument.RootElement;
        Assert.Equal("project-sanctuary.cgel.cme-authorship-provenance-contract.v1", authorshipRoot.GetProperty("schema").GetString());
        Assert.False(authorshipRoot.GetProperty("llmIsAuthor").GetBoolean());
        Assert.True(authorshipRoot.GetProperty("cmeAuthorshipRequiresReceipt").GetBoolean());

        using var serviceDocument = System.Text.Json.JsonDocument.Parse(File.ReadAllText(servicePath));
        var serviceRoot = serviceDocument.RootElement;
        Assert.Equal("project-sanctuary.service.mcp-alpha-contract.v1", serviceRoot.GetProperty("schema").GetString());
        Assert.False(serviceRoot.GetProperty("localPathsReturnedToRemoteGpt").GetBoolean());
        Assert.False(serviceRoot.GetProperty("writeActionToolsExposed").GetBoolean());
        Assert.False(serviceRoot.GetProperty("reviewedPerformanceToolsExposed").GetBoolean());
    }

    [Fact]
    public void MosLineageRegisterWritesCrypticRootMantleWithoutAuthority()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(fixture.Request("mantle-of-sovereign"));

        Assert.Equal("mos-lineage-register", receipt.Command);
        Assert.Equal("sanctuary-mos-lineage-register-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["mosLineageRegisterWritten"]);
        Assert.Equal(true, receipt.Evidence["mosRootOfCryptic"]);
        Assert.Equal(true, receipt.Evidence["mosRecordsEveryBirthedMceOrCme"]);
        Assert.Equal(false, receipt.Evidence["mosAuthorityGranted"]);
        Assert.Equal(false, receipt.Evidence["mosActionAuthorized"]);
        Assert.Equal(false, receipt.Evidence["mosPersonhoodClaimed"]);
        Assert.Equal(false, receipt.Evidence["mosSovereigntyClaimed"]);

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText((string)receipt.Evidence["mosLineageRecordPath"]!));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cryptic.mos-lineage-register.v1", root.GetProperty("schema").GetString());
        Assert.True(root.GetProperty("recordsEveryBirthedMceOrCmeInTypedSubset").GetBoolean());
        Assert.False(root.GetProperty("rawOAuthTokenStored").GetBoolean());
        Assert.False(root.GetProperty("authorityGranted").GetBoolean());
    }

    [Fact]
    public void SliAccessGateRegisterKeepsMcpAndAuthoritySeparate()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(fixture.Request("symbolic-language-interconnect"));

        Assert.Equal("sli-access-gate-register", receipt.Command);
        Assert.Equal("sanctuary-sli-access-gate-register-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["sliAccessGateRegisterWritten"]);
        Assert.Equal(true, receipt.Evidence["sliGovernedByCryptic"]);
        Assert.Equal(true, receipt.Evidence["sliMostSecureAccessGate"]);
        Assert.Equal(false, receipt.Evidence["mcpCallEqualsSliPassage"]);
        Assert.Equal(false, receipt.Evidence["sliPassageEqualsAuthority"]);
        Assert.Equal(false, receipt.Evidence["symbolicTranslationEqualsIdentity"]);
        Assert.Equal(false, receipt.Evidence["sliToolPermissionGranted"]);

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText((string)receipt.Evidence["sliAccessGatePath"]!));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cryptic.sli-access-gate.v1", root.GetProperty("schema").GetString());
        Assert.Equal("Cryptic", root.GetProperty("governedBy").GetString());
        Assert.True(root.GetProperty("controlsMcpMeaningPassage").GetBoolean());
        Assert.False(root.GetProperty("toolPermissionGranted").GetBoolean());
    }

    [Fact]
    public void TriviumForumConnectorPostureStaysOutsideSanctuaryCore()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(fixture.Request("trivium-forum"));

        Assert.Equal("trivium-forum-connector-posture", receipt.Command);
        Assert.Equal("sanctuary-trivium-forum-connector-posture-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["triviumForumConnectorPostureWritten"]);
        Assert.Equal(true, receipt.Evidence["triviumForumWrapsExternalLlms"]);
        Assert.Equal(false, receipt.Evidence["triviumForumModifiesProviderModelCode"]);
        Assert.Equal(true, receipt.Evidence["triviumForumRequiresSliPassage"]);
        Assert.Equal(true, receipt.Evidence["triviumForumRequiresMosStandingCheck"]);
        Assert.Equal(false, receipt.Evidence["triviumForumIssuesOAuthTokensHere"]);
        Assert.Equal(false, receipt.Evidence["triviumForumOpensTunnelHere"]);
        Assert.Equal(false, receipt.Evidence["triviumForumGrantsAuthority"]);

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText((string)receipt.Evidence["triviumForumConnectorPosturePath"]!));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.trivium-forum.connector-posture.v1", root.GetProperty("schema").GetString());
        Assert.True(root.GetProperty("publicConnectorMembraneOwner").GetBoolean());
        Assert.False(root.GetProperty("sanctuaryCoreOwner").GetBoolean());
    }

    [Fact]
    public void ExternalLlmStandingProbeStoresOnlyCandidateStanding()
    {
        using var fixture = new SanctuaryTestFixture();
        var request = fixture.Request("provider-standing-probe") with
        {
            LicenseScope = "OpenAI.ChatGPT.MCP",
            RegisteredEmail = "operator@example.invalid"
        };
        var receipt = new SanctuaryReceiptService().Run(request);

        Assert.Equal("external-llm-standing-probe", receipt.Command);
        Assert.Equal("sanctuary-external-llm-standing-probe-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["externalLlmStandingProbeWritten"]);
        Assert.Equal("OpenAI.ChatGPT.MCP", receipt.Evidence["externalLlmProviderSurface"]);
        Assert.Equal(true, receipt.Evidence["externalLlmAccountIdentityHashPresent"]);
        Assert.Equal(false, receipt.Evidence["externalLlmRawLoginStored"]);
        Assert.Equal(false, receipt.Evidence["externalLlmRawTokenStored"]);
        Assert.Equal(false, receipt.Evidence["externalLlmLeaseIssued"]);
        Assert.Equal(false, receipt.Evidence["externalLlmToolPermissionGranted"]);
        Assert.Equal(false, receipt.Evidence["externalLlmProviderCalled"]);
        Assert.Equal(false, receipt.Evidence["externalLlmModelBound"]);

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText((string)receipt.Evidence["externalLlmStandingProbePath"]!));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.mos.external-llm-standing-probe.v1", root.GetProperty("schema").GetString());
        Assert.True(root.GetProperty("accountIdentityHashPresent").GetBoolean());
        Assert.False(root.GetProperty("rawOAuthTokenStored").GetBoolean());
        Assert.False(root.GetProperty("leaseIssued").GetBoolean());
    }

    [Fact]
    public void CradleBoundaryOrganRegisterTypesCloudAsBoundaryWithoutMutation()
    {
        using var fixture = new SanctuaryTestFixture();
        var receipt = new SanctuaryReceiptService().Run(fixture.Request("cloud-boundary-organ-register"));

        Assert.Equal("cradle-boundary-organ-register", receipt.Command);
        Assert.Equal("sanctuary-cradle-boundary-organ-register-completed-cold", receipt.OutcomeCode);
        Assert.True(receipt.Gates.AllClosed);
        Assert.Equal(true, receipt.Evidence["cradleBoundaryOrganRegisterWritten"]);
        Assert.Equal(7, receipt.Evidence["cradleBoundaryOrganCount"]);
        Assert.Equal(true, receipt.Evidence["labOwnsOrgans"]);
        Assert.Equal(true, receipt.Evidence["cloudServicesProvideBoundaryLayers"]);
        Assert.Equal(false, receipt.Evidence["cloudServicesAreNervousSystem"]);
        Assert.Equal(false, receipt.Evidence["boundaryServiceEqualsAuthoritySource"]);
        Assert.Equal(false, receipt.Evidence["cloudCustodyEqualsGelCustody"]);
        Assert.Equal(false, receipt.Evidence["providerCallEqualsCmeAuthorship"]);
        Assert.Equal(false, receipt.Evidence["edgeAuthenticationEqualsSanctuaryAdmission"]);
        Assert.Equal(false, receipt.Evidence["dnsNamingEqualsTelemetryCustody"]);
        Assert.Equal(false, receipt.Evidence["tunnelAvailabilityEqualsOwnedIngress"]);
        Assert.Equal(false, receipt.Evidence["labBenchNodeEqualsEdgeServicesNode"]);
        Assert.Equal(false, receipt.Evidence["cloudBoundaryMutationPerformed"]);
        Assert.Equal(false, receipt.Evidence["providerCallPerformed"]);
        Assert.Equal(false, receipt.Evidence["dnsChangePerformed"]);
        Assert.Equal(false, receipt.Evidence["credentialIssued"]);
        Assert.Equal(false, receipt.Evidence["tunnelOpened"]);

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText((string)receipt.Evidence["cradleBoundaryOrganRegisterPath"]!));
        var root = document.RootElement;
        Assert.Equal("project-sanctuary.cgel.cradle-boundary-organ-register.v1", root.GetProperty("schema").GetString());
        Assert.True(root.GetProperty("labOwnsOrgans").GetBoolean());
        Assert.True(root.GetProperty("cloudServicesProvideBoundaryLayers").GetBoolean());
        Assert.False(root.GetProperty("cloudServicesAreNervousSystem").GetBoolean());
        Assert.True(root.GetProperty("noCloudMutation").GetBoolean());
        Assert.True(root.GetProperty("noProviderCalls").GetBoolean());
        Assert.True(root.GetProperty("noDnsChanges").GetBoolean());
        Assert.Equal(7, root.GetProperty("organCount").GetInt32());
    }

    [Fact]
    public void GptToolCatalogMapsOnlyColdReadFetchTools()
    {
        Assert.True(GptUseCaseTestingCatalog.TryMapToolToCommand("sanctuary.status", out var command));
        Assert.Equal("status", command);
        Assert.True(GptUseCaseTestingCatalog.TryMapToolToCommand("sanctuary.mos_lineage_register", out var mosCommand));
        Assert.Equal("mos-lineage-register", mosCommand);
        Assert.True(GptUseCaseTestingCatalog.TryMapToolToCommand("sanctuary.sli_access_gate_register", out var sliCommand));
        Assert.Equal("sli-access-gate-register", sliCommand);
        Assert.True(GptUseCaseTestingCatalog.TryMapToolToCommand("sanctuary.trivium_forum_connector_posture", out var triviumCommand));
        Assert.Equal("trivium-forum-connector-posture", triviumCommand);
        Assert.True(GptUseCaseTestingCatalog.TryMapToolToCommand("sanctuary.cradle_boundary_organ_register", out var organCommand));
        Assert.Equal("cradle-boundary-organ-register", organCommand);
        Assert.True(GptUseCaseTestingCatalog.TryMapToolToCommand("sanctuary.bridge_morphism_test", out var bridgeCommand));
        Assert.Equal("bridge-morphism-test", bridgeCommand);
        Assert.True(GptUseCaseTestingCatalog.TryMapToolToCommand("sanctuary.cme_theory_body", out var theoryCommand));
        Assert.Equal("cme-theory-body", theoryCommand);
        Assert.True(GptUseCaseTestingCatalog.TryMapToolToCommand("sanctuary.operator_work_cme_ec_gap", out var gapCommand));
        Assert.Equal("operator-work-cme-ec-gap", gapCommand);
        Assert.True(GptUseCaseTestingCatalog.TryMapToolToCommand("sanctuary.telemetry_slice_register", out var sliceCommand));
        Assert.Equal("telemetry-slice-register", sliceCommand);
        Assert.True(GptUseCaseTestingCatalog.TryMapToolToCommand("sanctuary.extended_telemetry_weather", out var weatherCommand));
        Assert.Equal("extended-telemetry-weather", weatherCommand);
        Assert.True(GptUseCaseTestingCatalog.TryMapToolToCommand("sanctuary.cgoa_formation", out var cgoaCommand));
        Assert.Equal("cgoa-formation", cgoaCommand);
        Assert.True(GptUseCaseTestingCatalog.TryMapToolToCommand("sanctuary.codex_governing_witness", out var witnessCommand));
        Assert.Equal("codex-governing-witness", witnessCommand);
        Assert.True(GptUseCaseTestingCatalog.TryMapToolToCommand("sanctuary.full_body_io_runtime", out var fullBodyCommand));
        Assert.Equal("full-body-io-runtime", fullBodyCommand);
        Assert.True(GptUseCaseTestingCatalog.TryMapToolToCommand("sanctuary.gel_approval_nadir_return", out var approvalCommand));
        Assert.Equal("gel-approval-nadir-return", approvalCommand);
        Assert.True(GptUseCaseTestingCatalog.TryMapToolToCommand("sanctuary.approval_closure_register", out var closureCommand));
        Assert.Equal("approval-closure-register", closureCommand);
        Assert.True(GptUseCaseTestingCatalog.TryMapToolToCommand("sanctuary.coupling_control_surface_register", out var couplingCommand));
        Assert.Equal("coupling-control-surface-register", couplingCommand);
        Assert.True(GptUseCaseTestingCatalog.TryMapToolToCommand("sanctuary.actualization_state_register", out var actualizationCommand));
        Assert.Equal("actualization-state-register", actualizationCommand);
        Assert.True(GptUseCaseTestingCatalog.TryMapToolToCommand("sanctuary.actual_approval_lease_validation", out var leaseValidationCommand));
        Assert.Equal("actual-approval-lease-validation", leaseValidationCommand);
        Assert.True(GptUseCaseTestingCatalog.TryMapToolToCommand("sanctuary.agenticore_duplex_lisp_membrane", out var duplexCommand));
        Assert.Equal("agenticore-duplex-lisp-membrane", duplexCommand);
        Assert.True(GptUseCaseTestingCatalog.TryMapToolToCommand("sanctuary.stem_domain_training_certification", out var stemCommand));
        Assert.Equal("stem-domain-training-certification", stemCommand);
        Assert.True(GptUseCaseTestingCatalog.TryMapToolToCommand("sanctuary.lab_observation_digest", out var observationDigestCommand));
        Assert.Equal("lab-observation-digest", observationDigestCommand);
        Assert.True(GptUseCaseTestingCatalog.TryMapToolToCommand("sanctuary.research_latex_export", out var researchLatexCommand));
        Assert.Equal("research-latex-export", researchLatexCommand);
        Assert.True(GptUseCaseTestingCatalog.TryMapToolToCommand("sanctuary.construct_custody_register", out var constructCommand));
        Assert.Equal("construct-custody-register", constructCommand);
        Assert.True(GptUseCaseTestingCatalog.TryMapToolToCommand("sanctuary.gel_crystal_register", out var crystalCommand));
        Assert.Equal("gel-crystal-register", crystalCommand);
        Assert.True(GptUseCaseTestingCatalog.TryMapToolToCommand("sanctuary.gel_reforge_bench", out var reforgeCommand));
        Assert.Equal("gel-reforge-bench", reforgeCommand);
        Assert.False(GptUseCaseTestingCatalog.TryMapToolToCommand("sanctuary.gel_admission", out _));

        Assert.All(
            GptUseCaseTestingCatalog.SafeToolSurfaces,
            tool =>
            {
                Assert.True(tool.ReadOrFetchOnly);
                Assert.False(tool.ExposesSecrets);
                Assert.False(tool.CallsProvider);
                Assert.False(tool.BindsModel);
                Assert.False(tool.AuthorizesExternalAction);
                Assert.False(tool.AdmitsGel);
                Assert.False(tool.MutatesSelfGel);
                Assert.False(tool.ActivatesActual);
            });
    }

    [Fact]
    public async Task DefaultSessionIdsStayUniqueAcrossConcurrentSameCommand()
    {
        using var fixture = new SanctuaryTestFixture();
        var service = new SanctuaryReceiptService();

        var receipts = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Task.Run(() =>
            service.Run(new SanctuaryRequest
            {
                Command = "typed-secure-ping",
                InstallRootPath = Path.Combine(fixture.RootPath, "install"),
                IntakeRootPath = Path.Combine(fixture.RootPath, "intake"),
                OperatorName = "Test Operator",
                CmeId = "Codex.CME.ID",
                CmeIdentitySelected = true,
                CallerCmeId = "Codex.CME.ID",
                ThreadBindingId = "test-thread"
            }))));

        Assert.All(receipts, receipt => Assert.Equal("RefusedSilent", receipt.Disposition));
        Assert.Equal(receipts.Length, receipts.Select(receipt => receipt.ReceiptJsonPath).Distinct().Count());
        Assert.All(receipts, receipt => Assert.True(File.Exists(receipt.ReceiptJsonPath)));
    }

    private static string ActualApprovalLeaseDigest(ActualApprovalLease lease)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(
            new
            {
                lease.Schema,
                lease.LeaseId,
                lease.CmeId,
                lease.ThreadBindingId,
                lease.IdentityTemplateId,
                lease.SoulFrameId,
                lease.AgentiCoreId,
                lease.Domain,
                lease.Role,
                lease.JobClass,
                lease.AdmissionScope,
                lease.AdmissionNoteDigest,
                lease.CommandAllowlist,
                lease.IssuedAtUtc,
                lease.ExpiresAtUtc,
                lease.LeaseMinutes,
                lease.ReviewApproved,
                lease.OperatorApproved,
                lease.StewardWitnessed,
                lease.PrimeWitnessed,
                lease.CrypticWitnessed,
                lease.Revoked,
                lease.RevocationReason
            },
            new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static SanctuaryRequest Reviewed(SanctuaryRequest request) => request with
    {
        AdmissionScope = "LabPublicCore",
        AdmissionNote = "test reviewed authority bundle",
        ReviewApproved = true,
        OperatorApproved = true,
        AuthorityLeaseIssued = true,
        StewardWitnessed = true,
        PrimeWitnessed = true,
        CrypticWitnessed = true
    };

    private sealed class SanctuaryTestFixture : IDisposable
    {
        private readonly string _root = Path.Combine(Path.GetTempPath(), $"project-sanctuary-tests-{Guid.NewGuid():N}");

        public string RootPath => _root;

        public SanctuaryRequest Request(string command) => new()
        {
            Command = command,
            InstallRootPath = Path.Combine(_root, "install"),
            IntakeRootPath = Path.Combine(_root, "intake"),
            OperatorName = "Test Operator",
            CmeId = "Codex.CME.ID",
            CmeIdentitySelected = true,
            CallerCmeId = "Codex.CME.ID",
            ThreadBindingId = "test-thread",
            SessionId = "test-session"
        };

        public void Dispose()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }
    }

    private static void AssertTip(SanctuaryReceipt receipt, string lane, string kind, string reviewScope)
    {
        var gelTipRoot = (string)receipt.Evidence["gelTipRootPath"]!;
        var tipPath = Path.Combine(gelTipRoot, lane, kind, "tip.json");
        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(tipPath));
        var root = document.RootElement;

        Assert.Equal(reviewScope, root.GetProperty("reviewScope").GetString());
        Assert.True(root.GetProperty("carriesAuthorityReachMaterial").GetBoolean());
        Assert.False(root.GetProperty("grantsAuthority").GetBoolean());
        Assert.False(root.GetProperty("carriesOperatorPosture").GetBoolean());
        Assert.Equal("MoS/OE/SelfGEL/cOE/cSelfGEL", root.GetProperty("operatorPostureStorage").GetString());
        Assert.True(root.GetProperty("legalGateSupportCoded").GetBoolean());
        Assert.False(root.GetProperty("authorityLeaseIssuedBySealing").GetBoolean());
        Assert.Equal("delta-decaying-authority-surface", root.GetProperty("authoritySurfaceKind").GetString());
        Assert.Equal("denied", root.GetProperty("authorityLeaseDefaultState").GetString());

        var legalGateSupport = root.GetProperty("legalGateSupport");
        Assert.NotEqual(0, legalGateSupport.GetArrayLength());
        foreach (var gate in legalGateSupport.EnumerateArray())
        {
            Assert.True(gate.GetProperty("requiresHumanReview").GetBoolean());
            Assert.True(gate.GetProperty("requiresDecryptionReview").GetBoolean());
            Assert.True(gate.GetProperty("leaseRequired").GetBoolean());
            Assert.Equal("delta-decaying-authority-surface", gate.GetProperty("authoritySurfaceKind").GetString());
            Assert.Equal("denied", gate.GetProperty("authorityDefaultState").GetString());
            Assert.False(gate.GetProperty("grantsAuthority").GetBoolean());
            Assert.False(gate.GetProperty("allowsAction").GetBoolean());
            Assert.False(gate.GetProperty("admitsData").GetBoolean());
        }
    }
}
