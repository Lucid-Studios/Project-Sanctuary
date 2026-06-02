using System.Text.Json;
using Sanctuary.Core;

var command = args.Length > 0 && !args[0].StartsWith("--", StringComparison.Ordinal)
    ? args[0]
    : "status";

if (string.Equals(command, "serve-mcp", StringComparison.OrdinalIgnoreCase) ||
    string.Equals(command, "serve-http", StringComparison.OrdinalIgnoreCase))
{
    await SanctuaryMcpLoopbackService.RunAsync(args);
    return;
}

var selectedCmeId = ReadOption(args, "--cme-id");
var request = new SanctuaryRequest
{
    Command = command,
    InstallRootPath = ReadOption(args, "--install-root")
        ?? Path.Combine(Environment.CurrentDirectory, ".local", "install"),
    IntakeRootPath = ReadOption(args, "--intake-root"),
    OperatorName = ReadOption(args, "--operator-name") ?? "Operator",
    CmeId = selectedCmeId ?? "",
    CmeIdentitySelected = !string.IsNullOrWhiteSpace(selectedCmeId),
    ServiceIdentityId = ReadOption(args, "--service-id") ?? "Sanctuary.Actual.ID",
    CallerCmeId = selectedCmeId ?? "",
    ThreadBindingId = ReadOption(args, "--thread-binding-id") ?? "",
    IdentityTemplateId = ReadOption(args, "--identity-template-id") ?? "SLI.Lisp.Industrial.CME.Template",
    SoulFrameId = ReadOption(args, "--soulframe-id") ?? "",
    AgentiCoreId = ReadOption(args, "--agenticore-id") ?? "",
    ParentCmeId = ReadOption(args, "--parent-cme-id") ?? "",
    SubjectCmeId = ReadOption(args, "--subject-cme-id") ?? "",
    SwarmId = ReadOption(args, "--swarm-id") ?? "",
    SubAgentId = ReadOption(args, "--sub-agent-id") ?? "",
    Domain = ReadOption(args, "--domain") ?? "Lab",
    Role = ReadOption(args, "--role") ?? "IndustrialCME",
    JobClass = ReadOption(args, "--job-class") ?? "ColdBench",
    SessionId = ReadOption(args, "--session-id") ?? "",
    SecretLane = ReadOption(args, "--secret-lane") ?? "Regional",
    SecretKind = ReadOption(args, "--secret-kind") ?? "BusinessLicenseWashingtonState",
    SecretSourceSpecs = ReadOptions(args, "--secret-source"),
    RegisteredEmail = ReadOption(args, "--registered-email") ?? "",
    SecurePingNonce = ReadOption(args, "--secure-ping-nonce") ?? "",
    LicenseScope = ReadOption(args, "--license-scope") ?? "LabQueryState",
    AdmissionScope = ReadOption(args, "--admission-scope") ?? "LabPublicCore",
    AdmissionNote = ReadOption(args, "--admission-note") ?? "",
    ActualApprovalLeasePath = ReadOption(args, "--actual-approval-lease-path") ?? "",
    RegisteredAccountConfirmed = ReadBool(args, "--registered-account-confirmed"),
    ReviewApproved = ReadBool(args, "--review-approved"),
    OperatorApproved = ReadBool(args, "--operator-approved"),
    AuthorityLeaseIssued = ReadBool(args, "--authority-lease-issued"),
    StewardWitnessed = ReadBool(args, "--steward-witnessed"),
    PrimeWitnessed = ReadBool(args, "--prime-witnessed"),
    CrypticWitnessed = ReadBool(args, "--cryptic-witnessed"),
    InstallFailureMode = ReadOption(args, "--install-failure-mode") ?? "",
    IssueId = ReadOption(args, "--issue-id") ?? "",
    IssueResolutionNote = ReadOption(args, "--issue-resolution-note") ?? "",
    HttpHost = ReadOption(args, "--http-host") ?? "127.0.0.1",
    HttpPort = ReadInt(args, "--http-port"),
    LeaseMinutes = ReadInt(args, "--lease-minutes", 15),
    HeartbeatSeconds = ReadInt(args, "--heartbeat-seconds", 60),
    BenchRunCount = ReadInt(args, "--bench-run-count", 3000),
    OpenReceivingWindow = ReadBool(args, "--open-receiving-window"),
    SearchMyPc = ReadBool(args, "--search-my-pc"),
    ChatSecretPassageRequested = ReadBool(args, "--chat-secret-passage"),
    RoamingHttpRequested = ReadBool(args, "--roaming-http"),
    IssueResolverApproved = ReadBool(args, "--issue-resolver-approved"),
    OperatorInstructionAcknowledged = ReadBool(args, "--operator-instruction-acknowledged")
};

try
{
    var receipt = new SanctuaryReceiptService().Run(request);
    var jsonRequested = ReadBool(args, "--json");

    Console.WriteLine($"Sanctuary command: {receipt.Command}");
    Console.WriteLine($"Outcome: {receipt.OutcomeCode}");
    Console.WriteLine($"Disposition: {receipt.Disposition}");
    Console.WriteLine($"Receipt JSON: {receipt.ReceiptJsonPath}");
    Console.WriteLine($"Receipt Markdown: {receipt.ReceiptMarkdownPath}");
    Console.WriteLine($"All gates closed: {receipt.Gates.AllClosed}");
    Console.WriteLine($"Provider called: {receipt.Gates.ProviderCalled}");
    Console.WriteLine($"Model bound: {receipt.Gates.ModelBound}");
    Console.WriteLine($"External action authorized: {receipt.Gates.ExternalActionAuthorized}");
    Console.WriteLine($"CME.Actual activated: {receipt.Gates.CmeActualActivated}");
    Console.WriteLine($"Sanctuary.Actual activated: {receipt.Gates.SanctuaryActualActivated}");

    if (jsonRequested)
    {
        Console.WriteLine(JsonSerializer.Serialize(receipt, new JsonSerializerOptions { WriteIndented = true }));
    }
}
catch (Exception exception) when (exception is ArgumentException or ArgumentOutOfRangeException or IOException)
{
    Console.Error.WriteLine(exception.Message);
    Environment.ExitCode = 1;
}

static string? ReadOption(IReadOnlyList<string> args, string name)
{
    for (var index = 0; index < args.Count - 1; index++)
    {
        if (string.Equals(args[index], name, StringComparison.OrdinalIgnoreCase))
        {
            var value = args[index + 1];
            return string.IsNullOrWhiteSpace(value) || value.StartsWith("--", StringComparison.Ordinal)
                ? null
                : value;
        }
    }

    return null;
}

static IReadOnlyList<string> ReadOptions(IReadOnlyList<string> args, string name)
{
    var values = new List<string>();
    for (var index = 0; index < args.Count - 1; index++)
    {
        if (string.Equals(args[index], name, StringComparison.OrdinalIgnoreCase))
        {
            values.Add(args[index + 1]);
        }
    }

    return values;
}

static bool ReadBool(IReadOnlyList<string> args, string name)
{
    for (var index = 0; index < args.Count; index++)
    {
        if (!string.Equals(args[index], name, StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        if (index + 1 < args.Count && bool.TryParse(args[index + 1], out var parsed))
        {
            return parsed;
        }

        return true;
    }

    return false;
}

static int ReadInt(IReadOnlyList<string> args, string name, int fallback = 0)
{
    var value = ReadOption(args, name);
    return int.TryParse(value, out var parsed) ? parsed : fallback;
}
