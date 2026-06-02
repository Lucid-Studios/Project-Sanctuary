using System.Text.Json;

namespace Sanctuary.Core;

public sealed partial class SanctuaryReceiptService
{
    private static void AddLocalGelEvidence(
        Dictionary<string, object?> evidence,
        SanctuaryRequest request,
        string command,
        string sessionId)
    {
        var localGelRoot = Path.Combine(request.InstallRootPath, "gel");
        var localGelSession = Path.Combine(localGelRoot, "sessions", sessionId);
        var safeCmeId = SafeSegment(request.CmeId);
        var localMosRoot = Path.Combine(localGelRoot, "mos", safeCmeId);
        var localSoulFrameRoot = Path.Combine(localMosRoot, "soulframe");
        var localAgentiCoreRoot = Path.Combine(localMosRoot, "agenticore");
        var localBodyFibreRoot = Path.Combine(localMosRoot, "body-fibres");
        var safeTemplateId = SafeSegment(request.IdentityTemplateId);
        var labTemplateRoot = Path.Combine(localGelRoot, "templates", "lab-standard", safeTemplateId);

        evidence["localInstallRootPath"] = request.InstallRootPath;
        evidence["localGelRootPath"] = localGelRoot;
        evidence["localGelSessionPath"] = localGelSession;
        evidence["localGelResidueJsonPath"] = Path.Combine(localGelSession, "gel-residue.json");
        evidence["localGelEventsLedgerPath"] = Path.Combine(localGelRoot, "events.jsonl");
        evidence["localGelCommandLedgerPath"] = Path.Combine(localGelRoot, "commands", command, "events.jsonl");
        evidence["localMosRootPath"] = localMosRoot;
        evidence["localMosOeLedgerPath"] = Path.Combine(localMosRoot, "oe", "events.jsonl");
        evidence["localMosSelfGelLedgerPath"] = Path.Combine(localMosRoot, "selfgel", "reconstruction-support.jsonl");
        evidence["localMosLaneLedgerPath"] = Path.Combine(localMosRoot, "lanes", command, "events.jsonl");
        evidence["localMosThreadBindingPath"] = Path.Combine(
            request.InstallRootPath,
            "mos",
            "cme-bindings",
            $"{safeCmeId}.json");
        evidence["localMosSoulFrameRootPath"] = localSoulFrameRoot;
        evidence["localMosSoulFramePrimeOeTipPath"] = Path.Combine(localSoulFrameRoot, "prime-oe-tip.json");
        evidence["localMosSoulFramePrimeSelfGelTipPath"] = Path.Combine(localSoulFrameRoot, "prime-selfgel-tip.json");
        evidence["localMosAgentiCoreRootPath"] = localAgentiCoreRoot;
        evidence["localMosAgentiCoreCoeLedgerPath"] = Path.Combine(localAgentiCoreRoot, "coe", "events.jsonl");
        evidence["localMosAgentiCoreCSelfGelLedgerPath"] = Path.Combine(localAgentiCoreRoot, "cselfgel", "events.jsonl");
        evidence["localMosBodyFibreRootPath"] = localBodyFibreRoot;
        evidence["localMosBodyFibreBundlePath"] = Path.Combine(localBodyFibreRoot, "cme-body-fibre-bundle.json");
        evidence["localMosBodyFibreBundleLispPath"] = Path.Combine(localBodyFibreRoot, "cme-body-fibre-bundle.sli.lisp");
        evidence["localMosBodyFibreLedgerPath"] = Path.Combine(localBodyFibreRoot, "events.jsonl");
        evidence["localGelTemplateRegistryPath"] = Path.Combine(localGelRoot, "templates", "registry.json");
        evidence["localGelLabStandardTemplateRootPath"] = labTemplateRoot;
        evidence["localGelLabStandardTemplateBodyJsonPath"] = Path.Combine(labTemplateRoot, "template-body.json");
        evidence["localGelLabStandardTemplateBodyLispPath"] = Path.Combine(labTemplateRoot, "template-body.sli.lisp");
        evidence["localGelLocalCustomTemplateRegistryPath"] = Path.Combine(localGelRoot, "templates", "local", "index.json");
        evidence["localGelTemplateLaneOrder"] = "LabStandardThenLocalCustom";
        if (!string.IsNullOrWhiteSpace(request.ParentCmeId))
        {
            var safeParentCmeId = SafeSegment(request.ParentCmeId);
            var safeSwarmId = string.IsNullOrWhiteSpace(request.SwarmId)
                ? "untyped-swarm"
                : SafeSegment(request.SwarmId);
            var localParentMosRoot = Path.Combine(localGelRoot, "mos", safeParentCmeId);
            evidence["localParentMosRootPath"] = localParentMosRoot;
            evidence["localParentSwarmPrecipitationLedgerPath"] = Path.Combine(
                localParentMosRoot,
                "swarms",
                safeSwarmId,
                "precipitation.jsonl");
        }

        evidence["localGelAppendOnlyPosture"] = true;
        evidence["localGelAdmitsTruth"] = false;
    }

    private static void WriteLocalGelResidue(SanctuaryReceipt receipt)
    {
        var residuePath = (string)receipt.Evidence["localGelResidueJsonPath"]!;
        var eventsLedger = (string)receipt.Evidence["localGelEventsLedgerPath"]!;
        var commandLedger = (string)receipt.Evidence["localGelCommandLedgerPath"]!;
        var oeLedger = (string)receipt.Evidence["localMosOeLedgerPath"]!;
        var selfGelLedger = (string)receipt.Evidence["localMosSelfGelLedgerPath"]!;
        var laneLedger = (string)receipt.Evidence["localMosLaneLedgerPath"]!;
        var threadBindingPath = (string)receipt.Evidence["localMosThreadBindingPath"]!;
        var soulFramePrimeOeTipPath = (string)receipt.Evidence["localMosSoulFramePrimeOeTipPath"]!;
        var soulFramePrimeSelfGelTipPath = (string)receipt.Evidence["localMosSoulFramePrimeSelfGelTipPath"]!;
        var agentiCoreCoeLedger = (string)receipt.Evidence["localMosAgentiCoreCoeLedgerPath"]!;
        var agentiCoreCSelfGelLedger = (string)receipt.Evidence["localMosAgentiCoreCSelfGelLedgerPath"]!;
        var bodyFibreBundlePath = (string)receipt.Evidence["localMosBodyFibreBundlePath"]!;
        var bodyFibreBundleLispPath = (string)receipt.Evidence["localMosBodyFibreBundleLispPath"]!;
        var bodyFibreLedgerPath = (string)receipt.Evidence["localMosBodyFibreLedgerPath"]!;

        WriteGelTemplateBodies(receipt);

        WriteJsonFile(residuePath, new
        {
            schema = "project-sanctuary.local-gel-residue.v1",
            receipt.ReceiptHandle,
            receipt.Command,
            receipt.OutcomeCode,
            receipt.Disposition,
            receipt.SessionId,
            receipt.OperatorName,
            receipt.CmeId,
            receipt.TimestampUtc,
            allGatesClosed = receipt.Gates.AllClosed,
            evidence = receipt.Evidence
        });

        var line = JsonSerializer.Serialize(
            new
            {
                schema = "project-sanctuary.local-gel-event.v1",
                receipt.ReceiptHandle,
                receipt.Command,
                receipt.OutcomeCode,
                receipt.SessionId,
                receipt.CmeId,
                receipt.TimestampUtc,
                allGatesClosed = receipt.Gates.AllClosed
            });

        AppendJsonLine(eventsLedger, line);
        AppendJsonLine(commandLedger, line);
        AppendJsonLine(oeLedger, line);
        AppendJsonLine(laneLedger, line);

        var threadBindingId = receipt.Evidence.GetValueOrDefault("mosThreadBindingId") as string ?? "";
        if (!string.IsNullOrWhiteSpace(threadBindingId) && !File.Exists(threadBindingPath))
        {
            try
            {
                WriteJsonFileCreateNew(threadBindingPath, new
                {
                    schema = "project-sanctuary.mos.cme-thread-binding.v1",
                    cmeId = receipt.CmeId,
                    threadBindingId,
                    domainRole = receipt.Evidence.GetValueOrDefault("mosKnownThreadBindingDomainRole") as string ?? "",
                    soulFrameId = receipt.Evidence.GetValueOrDefault("mosSoulFrameId") as string ?? "",
                    agentiCoreId = receipt.Evidence.GetValueOrDefault("mosAgentiCoreId") as string ?? "",
                    active = true,
                    singleUseCmeLock = true,
                    firstWriterWins = true,
                    crossThreadAccessDenied = true,
                    bindingSource = receipt.Evidence.GetValueOrDefault("mosKnownThreadBindingSource") as string ?? "receipt-write",
                    createdBy = "receipt-write-fallback",
                    createdAtUtc = receipt.TimestampUtc,
                    updatedAtUtc = receipt.TimestampUtc
                });
            }
            catch (IOException)
            {
                // Validation owns the first-use lock. Concurrent receipt writes may observe the same lock appearing.
            }
        }

        WriteJsonFile(soulFramePrimeOeTipPath, new
        {
            schema = "project-sanctuary.soulframe.prime-tip.v1",
            cmeId = receipt.CmeId,
            soulFrameId = receipt.Evidence.GetValueOrDefault("mosSoulFrameId"),
            tipKind = "Prime.OE",
            carriedBy = "SoulFrame",
            canonicalSelfGelMutation = false,
            tipDigest = Digest($"{receipt.CmeId}|{receipt.Evidence.GetValueOrDefault("mosSoulFrameId")}|Prime.OE")
        });

        WriteJsonFile(soulFramePrimeSelfGelTipPath, new
        {
            schema = "project-sanctuary.soulframe.prime-tip.v1",
            cmeId = receipt.CmeId,
            soulFrameId = receipt.Evidence.GetValueOrDefault("mosSoulFrameId"),
            tipKind = "Prime.SelfGEL",
            carriedBy = "SoulFrame",
            canonicalSelfGelMutation = false,
            tipDigest = Digest($"{receipt.CmeId}|{receipt.Evidence.GetValueOrDefault("mosSoulFrameId")}|Prime.SelfGEL")
        });

        var selfGelLine = JsonSerializer.Serialize(
            new
            {
                schema = "project-sanctuary.local-selfgel-reconstruction-support.v1",
                receipt.ReceiptHandle,
                receipt.Command,
                receipt.SessionId,
                receipt.CmeId,
                receipt.TimestampUtc,
                reconstructionSupportOnly = true,
                selfGelMutated = receipt.Gates.SelfGelMutated,
                gelAdmitted = receipt.Gates.GelAdmitted
            });
        AppendJsonLine(selfGelLedger, selfGelLine);

        var agentiCoreLine = JsonSerializer.Serialize(
            new
            {
                schema = "project-sanctuary.agenticore.hot-ec-event.v1",
                receipt.ReceiptHandle,
                receipt.Command,
                receipt.SessionId,
                receipt.CmeId,
                agentiCoreId = receipt.Evidence.GetValueOrDefault("mosAgentiCoreId"),
                receipt.TimestampUtc,
                ecUseSurface = true,
                hotSideOnly = true,
                canonicalSelfGelMutation = false,
                gelAdmitted = receipt.Gates.GelAdmitted,
                selfGelMutated = receipt.Gates.SelfGelMutated,
                allGatesClosed = receipt.Gates.AllClosed
            });
        AppendJsonLine(agentiCoreCoeLedger, agentiCoreLine);
        AppendJsonLine(agentiCoreCSelfGelLedger, agentiCoreLine);

        var bodyFibreBundle = BuildCmeBodyFibreBundleRecord(receipt);
        WriteJsonFile(bodyFibreBundlePath, bodyFibreBundle);
        WriteTextFile(bodyFibreBundleLispPath, BuildCmeBodyFibreBundleLisp(receipt));
        AppendJsonLine(
            bodyFibreLedgerPath,
            JsonSerializer.Serialize(
                new
                {
                    schema = "project-sanctuary.mos.cme-body-fibre-bundle-event.v1",
                    eventType = "cme-body-fibre-bundle-written",
                    receipt.ReceiptHandle,
                    receipt.Command,
                    receipt.SessionId,
                    receipt.CmeId,
                    receipt.TimestampUtc,
                    fibreCount = receipt.Evidence.GetValueOrDefault("mosCmeBodyFibreBundleCount"),
                    cmeActualActivated = receipt.Gates.CmeActualActivated,
                    sanctuaryActualActivated = receipt.Gates.SanctuaryActualActivated,
                    gelAdmitted = receipt.Gates.GelAdmitted,
                    selfGelMutated = receipt.Gates.SelfGelMutated,
                    authorityGranted = receipt.Gates.AuthorityGranted,
                    actionAuthorized = receipt.Gates.ActionAuthorized
                }));

        if (receipt.Evidence.TryGetValue("localParentSwarmPrecipitationLedgerPath", out var precipitationPathValue) &&
            precipitationPathValue is string precipitationPath &&
            !string.IsNullOrWhiteSpace(precipitationPath))
        {
            var precipitationLine = JsonSerializer.Serialize(
                new
                {
                    schema = "project-sanctuary.parent-cme.swarm-precipitation.v1",
                    receipt.ReceiptHandle,
                    receipt.Command,
                    receipt.SessionId,
                    parentCmeId = receipt.Evidence.GetValueOrDefault("mosParentCmeId"),
                    childCmeId = receipt.CmeId,
                    swarmId = receipt.Evidence.GetValueOrDefault("mosSwarmId"),
                    subAgentId = receipt.Evidence.GetValueOrDefault("mosSubAgentId"),
                    identityTemplateId = receipt.Evidence.GetValueOrDefault("mosIdentityTemplateId"),
                    receipt.TimestampUtc,
                    candidateOnly = true,
                    parentDirectOeSelfGelMutationAllowed = false,
                    childKeepsOwnGelLane = true,
                    allGatesClosed = receipt.Gates.AllClosed,
                    selfGelMutated = receipt.Gates.SelfGelMutated,
                    gelAdmitted = receipt.Gates.GelAdmitted
                });
            AppendJsonLine(precipitationPath, precipitationLine);
        }
    }
}
