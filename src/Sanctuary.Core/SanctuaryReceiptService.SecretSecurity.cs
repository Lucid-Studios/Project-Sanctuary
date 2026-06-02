using System.Text.Json;

namespace Sanctuary.Core;

public sealed partial class SanctuaryReceiptService
{
    private static SecretSourceSpec ParseSecretSourceSpec(string raw)
    {
        var parts = raw.Split('|', 3);
        if (parts.Length != 3 ||
            string.IsNullOrWhiteSpace(parts[0]) ||
            string.IsNullOrWhiteSpace(parts[1]) ||
            string.IsNullOrWhiteSpace(parts[2]))
        {
            throw new ArgumentException("Secret sources must be formatted as Lane|Kind|Path.");
        }

        return new SecretSourceSpec(parts[0].Trim(), parts[1].Trim(), parts[2].Trim());
    }

    private static string ClassifyReviewScope(string lane)
    {
        return lane.Trim().ToLowerInvariant() switch
        {
            "regional" => "jurisdictional-authority-reach",
            "local" => "local-authority-reach",
            "personalized" => "operator-supplied-credential-custody",
            _ => "operator-selected-custody"
        };
    }

    private static bool ShouldFailSilent(string command, SanctuaryRequest request) =>
        string.Equals(command, "typed-secure-ping", StringComparison.OrdinalIgnoreCase) &&
        (string.IsNullOrWhiteSpace(request.RegisteredEmail) ||
            string.IsNullOrWhiteSpace(request.SecurePingNonce) ||
            !request.RegisteredAccountConfirmed);

    private static bool IsLoopbackHost(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            return true;
        }

        return string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(host, "127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(host, "::1", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static string BuildRegisteredAccountEmailChallengeTemplate() =>
        """
        Subject: Verify your Sanctuary access request

        A code was requested by this account, please verify by clicking the button generated below or the link provided here.

        Button: {{verification_button}}
        Link: {{verification_link}}
        Code: {{one_time_code}}

        If you did not request this code, do not click the button or link. The request will expire automatically.
        """;

    private static string NormalizeFailureMode(string failureMode)
    {
        if (string.IsNullOrWhiteSpace(failureMode))
        {
            return "operator-instruction-acknowledgement-missing";
        }

        return SafeSegment(failureMode.Trim().ToLowerInvariant());
    }

    private static string ClassifyFailureMode(string failureMode)
    {
        if (failureMode.Contains("instruction", StringComparison.Ordinal) ||
            failureMode.Contains("acknowledg", StringComparison.Ordinal))
        {
            return "operator-instruction-engagement";
        }

        if (failureMode.Contains("2fa", StringComparison.Ordinal) ||
            failureMode.Contains("account", StringComparison.Ordinal) ||
            failureMode.Contains("registered", StringComparison.Ordinal))
        {
            return "account-access";
        }

        if (failureMode.Contains("secret", StringComparison.Ordinal) ||
            failureMode.Contains("credential", StringComparison.Ordinal) ||
            failureMode.Contains("legal", StringComparison.Ordinal))
        {
            return "custody-or-legal-documentation";
        }

        if (failureMode.Contains("support", StringComparison.Ordinal) ||
            failureMode.Contains("assist", StringComparison.Ordinal))
        {
            return "assisted-support";
        }

        return "install-floor";
    }

    private static string ResolveIssueId(SanctuaryRequest request, string failureMode) =>
        string.IsNullOrWhiteSpace(request.IssueId)
            ? $"issue-{Digest($"{request.CmeId}|{failureMode}")[..16]}"
            : SafeSegment(request.IssueId);

    private static IssueResolutionState ReadIssueResolution(string installRootPath, string issueId)
    {
        var resolutionPath = Path.Combine(installRootPath, "issues", SafeSegment(issueId), "resolution.json");
        if (!File.Exists(resolutionPath))
        {
            return new IssueResolutionState(false, resolutionPath);
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(resolutionPath));
            var resolved = document.RootElement.TryGetProperty("issueResolved", out var property) &&
                property.ValueKind == JsonValueKind.True;
            return new IssueResolutionState(resolved, resolutionPath);
        }
        catch (JsonException)
        {
            return new IssueResolutionState(false, resolutionPath);
        }
    }

    private static string WriteCgelFailureModeRecord(
        SanctuaryRequest request,
        DateTimeOffset timestamp,
        string issueId,
        string failureMode,
        bool locked,
        IssueResolutionState resolution)
    {
        var cgelPath = Path.Combine(
            request.InstallRootPath,
            "cgel",
            "typed-failure-modes",
            $"{failureMode}.json");

        WriteJsonFile(cgelPath, new
        {
            schema = "project-sanctuary.cgel.typed-failure-mode.v1",
            issueId,
            failureMode,
            failureModeClass = ClassifyFailureMode(failureMode),
            installFloorState = locked ? "industrial-cme-locked" : "industrial-cme-floor-resolved",
            industrialCmeLocked = locked,
            issueResolved = resolution.Resolved,
            resolutionPath = resolution.Path,
            protectedIndustrialCmePosture = true,
            supportLockNotPunitive = true,
            nonDiagnosticSupportPosture = true,
            medicalOrCognitiveDiagnosisMade = false,
            issueResolverRequired = locked,
            issueTrackingOwner = "Steward",
            issueProcessingOwner = "Cryptic",
            issueReceiptWitnessOwner = "Prime",
            issueDomainRouting = IssueDomainRouting(),
            segmentedGelDomainRoutingRequired = true,
            crossDomainIssueCollapseAllowed = false,
            realTimeIssueApiIntakeAllowed = false,
            realTimeIssueApiIntakeRequiresLease = true,
            customerServiceIssueCreationRequiresLease = true,
            cmeActualAllowed = false,
            sanctuaryActualAllowed = false,
            updatedAtUtc = timestamp
        });

        return cgelPath;
    }

    private static string AppendIssueTrackingEvent(
        SanctuaryRequest request,
        DateTimeOffset timestamp,
        string issueId,
        string failureMode,
        string eventType,
        bool resolved)
    {
        var issueRoot = Path.Combine(request.InstallRootPath, "issues", SafeSegment(issueId));
        var issueEventPath = Path.Combine(issueRoot, "events.jsonl");
        var globalIssueLedgerPath = Path.Combine(request.InstallRootPath, "issues", "events.jsonl");
        var line = JsonSerializer.Serialize(new
        {
            schema = "project-sanctuary.issue-tracking-event.v1",
            issueId,
            eventType,
            failureMode,
            failureModeClass = ClassifyFailureMode(failureMode),
            cmeId = request.CmeId,
            domain = request.Domain,
            role = request.Role,
            resolved,
            issueTrackingOwner = "Steward",
            issueProcessingOwner = "Cryptic",
            issueReceiptWitnessOwner = "Prime",
            supportLockNotPunitive = true,
            nonDiagnosticSupportPosture = true,
            medicalOrCognitiveDiagnosisMade = false,
            issueDomainRouting = IssueDomainRouting(),
            timestampUtc = timestamp
        });

        AppendJsonLine(issueEventPath, line);
        AppendJsonLine(globalIssueLedgerPath, line);
        return issueEventPath;
    }

    private static IReadOnlyList<string> IssueDomainRouting() => new[]
    {
        "Security.GEL",
        "Install.GEL",
        "Account.GEL",
        "Legal.GEL",
        "Operator.GEL",
        "Product.GEL",
        "Support.GEL"
    };
}
