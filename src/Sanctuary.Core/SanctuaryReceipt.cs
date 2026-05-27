using System.Text.Json.Serialization;

namespace Sanctuary.Core;

public sealed record SanctuaryReceipt
{
    public string Schema { get; init; } = "project-sanctuary.receipt.v1";
    public required string ReceiptHandle { get; init; }
    public required string Command { get; init; }
    public required string OutcomeCode { get; init; }
    public required string Disposition { get; init; }
    public required string GovernanceTrace { get; init; }
    public required string SessionId { get; init; }
    public required string OperatorName { get; init; }
    public required string CmeId { get; init; }
    public required string Domain { get; init; }
    public required string Role { get; init; }
    public required string JobClass { get; init; }
    public required DateTimeOffset TimestampUtc { get; init; }
    public required string InstallRootPath { get; init; }
    public required string ReceiptJsonPath { get; init; }
    public required string ReceiptMarkdownPath { get; init; }
    public required SanctuaryGates Gates { get; init; }
    public Dictionary<string, object?> Evidence { get; init; } = new(StringComparer.Ordinal);

    [JsonIgnore]
    public bool IsClosedGateReceipt => Gates.AllClosed;
}
