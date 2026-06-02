using System.Text;
using System.Text.Json;

namespace Sanctuary.Core;

public sealed partial class SanctuaryReceiptService
{
    private static void ValidateCmeIdentitySelection(SanctuaryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CmeId))
        {
            throw new ArgumentException(
                "CME identity selection is required before Sanctuary can write receipts, GEL, OE, SelfGEL, or MoS residue. Select or provide a CME identity first.",
                nameof(request));
        }

        if (!request.CmeIdentitySelected)
        {
            throw new ArgumentException(
                "CME identity must be explicitly selected or resolved by MoS before first tool use; silent default identity fallback is not allowed.",
                nameof(request));
        }

        if (!string.IsNullOrWhiteSpace(request.CallerCmeId) &&
            !string.Equals(request.CallerCmeId, request.CmeId, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Caller CME identity must match the receipt/OE/SelfGEL CME identity. Cross-CME receipt writes require a parent precipitation lane, not caller identity substitution.",
                nameof(request));
        }

        ValidateCmeThreadBinding(request);
    }

    private static void ValidateCmeThreadBinding(SanctuaryRequest request)
    {
        if (IsServiceIdentityProcessRequest(request))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(request.ThreadBindingId))
        {
            throw new ArgumentException(
                "CME thread binding id is required before Sanctuary can write receipts, GEL, OE, SelfGEL, cOE, cSelfGEL, or MoS residue for a participant CME.",
                nameof(request));
        }

        lock (AppendLock)
        {
            var knownBinding = ResolveKnownCmeThreadBinding(request);
            if (string.IsNullOrWhiteSpace(knownBinding.BindingId))
            {
                WriteCmeThreadBindingFile(request, "", "first-use-cme-lock");
                return;
            }

            if (!string.Equals(request.ThreadBindingId, knownBinding.BindingId, StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    $"CME identity '{request.CmeId}' is bound to thread '{knownBinding.BindingId}' by {knownBinding.Source}; request attempted '{request.ThreadBindingId}'. Cross-thread CME identity access is denied.",
                    nameof(request));
            }

            WriteCmeThreadBindingFile(request, knownBinding.DomainRole, knownBinding.Source);
        }
    }

    private static bool IsServiceIdentityProcessRequest(SanctuaryRequest request) =>
        !string.IsNullOrWhiteSpace(request.ServiceIdentityId) &&
        string.Equals(request.CmeId, request.ServiceIdentityId, StringComparison.Ordinal) &&
        !request.ServiceIdentityId.EndsWith(".CME.ID", StringComparison.Ordinal);

    private static (string BindingId, string DomainRole, string Source) ResolveKnownCmeThreadBinding(SanctuaryRequest request)
    {
        var safeCmeId = SafeSegment(request.CmeId);
        var cmeBindingPath = Path.Combine(request.InstallRootPath, "mos", "cme-bindings", $"{safeCmeId}.json");
        if (File.Exists(cmeBindingPath))
        {
            using var bindingDocument = JsonDocument.Parse(File.ReadAllText(cmeBindingPath, Encoding.UTF8));
            var root = bindingDocument.RootElement;
            var bindingId = ReadStringProperty(root, "threadBindingId") ?? "";
            var domainRole = ReadStringProperty(root, "domainRole") ?? "";
            if (!string.IsNullOrWhiteSpace(bindingId))
            {
                return (bindingId, domainRole, "mos-cme-binding");
            }
        }

        var identityCandidatesPath = Path.Combine(request.InstallRootPath, "mos", "identity-candidates.json");
        if (!File.Exists(identityCandidatesPath))
        {
            return ("", "", "");
        }

        using var document = JsonDocument.Parse(File.ReadAllText(identityCandidatesPath, Encoding.UTF8));
        if (!document.RootElement.TryGetProperty("candidates", out var candidates) ||
            candidates.ValueKind != JsonValueKind.Array)
        {
            return ("", "", "");
        }

        foreach (var candidate in candidates.EnumerateArray())
        {
            var cmeId = ReadStringProperty(candidate, "cmeId") ?? "";
            if (!string.Equals(cmeId, request.CmeId, StringComparison.Ordinal))
            {
                continue;
            }

            return (
                ReadStringProperty(candidate, "lane") ?? "",
                ReadStringProperty(candidate, "domainRole") ?? "",
                "mos-identity-candidates");
        }

        return ("", "", "");
    }

    private static void WriteCmeThreadBindingFile(
        SanctuaryRequest request,
        string domainRole,
        string bindingSource)
    {
        var safeCmeId = SafeSegment(request.CmeId);
        var cmeBindingPath = Path.Combine(request.InstallRootPath, "mos", "cme-bindings", $"{safeCmeId}.json");
        var timestamp = DateTimeOffset.UtcNow;
        var payload = BuildCmeThreadBindingPayload(request, domainRole, bindingSource, timestamp, timestamp);

        Directory.CreateDirectory(Path.GetDirectoryName(cmeBindingPath)!);
        if (File.Exists(cmeBindingPath))
        {
            EnsureExistingCmeBindingMatchesRequest(request, cmeBindingPath);
            return;
        }

        try
        {
            WriteJsonFileCreateNew(cmeBindingPath, payload);
        }
        catch (IOException)
        {
            EnsureExistingCmeBindingMatchesRequest(request, cmeBindingPath);
        }
    }

    private static object BuildCmeThreadBindingPayload(
        SanctuaryRequest request,
        string domainRole,
        string bindingSource,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc) =>
        new
        {
            schema = "project-sanctuary.mos.cme-thread-binding.v1",
            cmeId = request.CmeId,
            threadBindingId = request.ThreadBindingId,
            domainRole,
            soulFrameId = EffectiveSoulFrameId(request),
            agentiCoreId = EffectiveAgentiCoreId(request),
            active = true,
            singleUseCmeLock = true,
            firstWriterWins = true,
            crossThreadAccessDenied = true,
            bindingSource,
            createdBy = string.Equals(bindingSource, "first-use-cme-lock", StringComparison.Ordinal)
                ? "first-use-core-validation"
                : "mos-core-validation",
            createdAtUtc,
            updatedAtUtc
        };

    private static void EnsureExistingCmeBindingMatchesRequest(SanctuaryRequest request, string cmeBindingPath)
    {
        var knownBinding = ResolveKnownCmeThreadBinding(request);
        if (string.IsNullOrWhiteSpace(knownBinding.BindingId))
        {
            throw new IOException($"CME binding lock path '{cmeBindingPath}' could not be verified after a create race.");
        }

        if (!string.Equals(request.ThreadBindingId, knownBinding.BindingId, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                $"CME identity '{request.CmeId}' is bound to thread '{knownBinding.BindingId}' by {knownBinding.Source}; request attempted '{request.ThreadBindingId}'. Cross-thread CME identity access is denied.",
                nameof(request));
        }
    }

    private static string EffectiveSoulFrameId(SanctuaryRequest request) =>
        string.IsNullOrWhiteSpace(request.SoulFrameId)
            ? $"{request.CmeId}.SoulFrame"
            : request.SoulFrameId;

    private static string EffectiveAgentiCoreId(SanctuaryRequest request) =>
        string.IsNullOrWhiteSpace(request.AgentiCoreId)
            ? $"{request.CmeId}.AgentiCore"
            : request.AgentiCoreId;

    private static bool IsCanonicalParticipantCmeId(string cmeId) =>
        !string.IsNullOrWhiteSpace(cmeId) &&
        cmeId.EndsWith(".CME.ID", StringComparison.Ordinal);

    private static string BuildCmeActualStateId(string cmeId) =>
        $"urn:sanctuary:cme-actual-state:{SafeSegment(cmeId)}";

    private static string BuildCmeActualLabel(string cmeId) =>
        cmeId.EndsWith(".CME.ID", StringComparison.Ordinal)
            ? $"{cmeId[..^".CME.ID".Length]}.CME.Actual"
            : $"{cmeId}.Actual";

    private static object BuildSharedPrimeRealityMembrane(string? serviceIdentityId) => new
    {
        schema = "project-sanctuary.shared-prime-reality-membrane.v1",
        sharedPrimeRealityLayer = "Sanctuary.Actual.weather-system",
        sharedPrimeRealityAuthoritySurface = string.IsNullOrWhiteSpace(serviceIdentityId)
            ? "Sanctuary.Actual.ID"
            : serviceIdentityId,
        sharedPrimeRealityOwnedByPersonalCme = false,
        personalCmePrivateRadioStation = false,
        cmeMayReceiveSharedPrimeWeather = true,
        cmeMayBroadcastPrimeReality = false,
        cmeLocalObservationCandidateOnly = true,
        cmePrivateTelemetryDefinesSharedPrime = false,
        listeningFrameReceivesWeather = true,
        listeningFrameDoesNotOwnWeather = true,
        globalTelemetryIsWeatherSignal = true,
        payloadDisclosureAllowed = false,
        truthAdmissionByWeather = false,
        authorityGrantedByWeather = false,
        actionAuthorizedByWeather = false
    };

    private static string BuildLabStandardTemplateBodyPath(string installRootPath, string templateId) =>
        Path.Combine(
            installRootPath,
            "gel",
            "templates",
            "lab-standard",
            SafeSegment(templateId),
            "template-body.json");
}
