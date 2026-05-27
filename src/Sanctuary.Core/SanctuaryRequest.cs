namespace Sanctuary.Core;

public sealed record SanctuaryRequest
{
    public required string Command { get; init; }
    public required string InstallRootPath { get; init; }
    public string? IntakeRootPath { get; init; }
    public string OperatorName { get; init; } = "Operator";
    public string CmeId { get; init; } = "Codex.CME.ID";
    public string Domain { get; init; } = "Lab";
    public string Role { get; init; } = "IndustrialCME";
    public string JobClass { get; init; } = "ColdBench";
    public string SessionId { get; init; } = "";
    public string SecretLane { get; init; } = "Regional";
    public string SecretKind { get; init; } = "BusinessLicenseWashingtonState";
    public IReadOnlyList<string> SecretSourceSpecs { get; init; } = Array.Empty<string>();
    public string RegisteredEmail { get; init; } = "";
    public string SecurePingNonce { get; init; } = "";
    public string LicenseScope { get; init; } = "LabQueryState";
    public string AdmissionScope { get; init; } = "LabPublicCore";
    public string AdmissionNote { get; init; } = "";
    public bool RegisteredAccountConfirmed { get; init; }
    public bool ReviewApproved { get; init; }
    public bool OperatorApproved { get; init; }
    public bool AuthorityLeaseIssued { get; init; }
    public bool StewardWitnessed { get; init; }
    public bool PrimeWitnessed { get; init; }
    public bool CrypticWitnessed { get; init; }
    public string InstallFailureMode { get; init; } = "";
    public string IssueId { get; init; } = "";
    public string IssueResolutionNote { get; init; } = "";
    public string HttpHost { get; init; } = "127.0.0.1";
    public int HttpPort { get; init; }
    public int LeaseMinutes { get; init; } = 15;
    public int HeartbeatSeconds { get; init; } = 60;
    public int BenchRunCount { get; init; } = 3000;
    public bool OpenReceivingWindow { get; init; }
    public bool SearchMyPc { get; init; }
    public bool ChatSecretPassageRequested { get; init; }
    public bool RoamingHttpRequested { get; init; }
    public bool IssueResolverApproved { get; init; }
    public bool OperatorInstructionAcknowledged { get; init; }
}
