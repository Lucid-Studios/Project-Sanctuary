using Sanctuary.Core;
using Xunit;

namespace Sanctuary.Core.Tests;

public sealed class SanctuaryReceiptServiceTests
{
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
        Assert.Equal(8, root.GetProperty("typedLispPetals").GetArrayLength());
        Assert.Equal(5, root.GetProperty("feedbackRoutes").GetArrayLength());
        Assert.Equal(6, root.GetProperty("fruitingBodyStages").GetArrayLength());
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
        Assert.Contains("(organ-route", quotedForms, StringComparison.Ordinal);
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
        Assert.Equal(47, receipt.Evidence["liveInstallCommandAllowlistCount"]);
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
        Assert.Equal(9, receipt.Evidence["stemReadinessPresentCount"]);
        Assert.Equal(32, receipt.Evidence["stemCognitiveCumulativeRunCount"]);
        Assert.Equal(140, receipt.Evidence["stemMathCumulativeRunCount"]);
        Assert.Equal(true, receipt.Evidence["stemLearningCondensateTracked"]);
        Assert.Equal(true, receipt.Evidence["stemCondensateTrackedIntoSanctuary"]);
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
        Assert.Contains(":credential-granted false", lisp, StringComparison.Ordinal);
        Assert.Contains(":fail-closed true", lisp, StringComparison.Ordinal);
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
                CmeId = "Codex.CME.ID"
            }))));

        Assert.All(receipts, receipt => Assert.Equal("RefusedSilent", receipt.Disposition));
        Assert.Equal(receipts.Length, receipts.Select(receipt => receipt.ReceiptJsonPath).Distinct().Count());
        Assert.All(receipts, receipt => Assert.True(File.Exists(receipt.ReceiptJsonPath)));
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
