using System.Text.Json;

namespace Sanctuary.Core;

public sealed partial class SanctuaryReceiptService
{
    private static readonly ISet<string> ReviewedPerformanceCommands = new HashSet<string>(StringComparer.Ordinal)
    {
        "gel-admission",
        "selfgel-admission",
        "actual-approval-lease",
        "cme-actual-keypair-forge",
        "cme-actualization",
        "cme-actual-invocation-lifecycle",
        "sanctuary-actualization"
    };

    private static bool IsReviewedPerformanceCommand(string command) =>
        ReviewedPerformanceCommands.Contains(command);

    private static bool HasReviewedAuthorityBundle(SanctuaryRequest request) =>
        request.ReviewApproved &&
        request.OperatorApproved &&
        request.AuthorityLeaseIssued &&
        request.StewardWitnessed &&
        request.PrimeWitnessed &&
        request.CrypticWitnessed &&
        !string.IsNullOrWhiteSpace(request.AdmissionScope);

    private static bool HasReviewedPerformanceAuthority(SanctuaryRequest request) =>
        HasReviewedAuthorityBundle(request) ||
        VerifyActualApprovalLease(
            request,
            NormalizeCommand(request.Command),
            DateTimeOffset.UtcNow).Verified;

    private static ActualApprovalLeaseVerification VerifyActualApprovalLease(
        SanctuaryRequest request,
        string command,
        DateTimeOffset timestamp)
    {
        if (string.IsNullOrWhiteSpace(request.ActualApprovalLeasePath))
        {
            return new(false, "lease-path-missing", "", "", "", null);
        }

        var leasePath = request.ActualApprovalLeasePath;
        if (!File.Exists(leasePath))
        {
            return new(false, "lease-file-missing", "", leasePath, "", null);
        }

        ActualApprovalLease? lease;
        try
        {
            lease = JsonSerializer.Deserialize<ActualApprovalLease>(File.ReadAllText(leasePath));
        }
        catch (JsonException)
        {
            return new(false, "lease-json-invalid", "", leasePath, "", null);
        }

        if (lease is null)
        {
            return new(false, "lease-json-empty", "", leasePath, "", null);
        }

        var computedDigest = ComputeActualApprovalLeaseDigest(lease);
        if (!string.Equals(lease.Schema, "project-sanctuary.actual-approval-lease.v1", StringComparison.Ordinal))
        {
            return new(false, "lease-schema-mismatch", lease.LeaseId, leasePath, computedDigest, lease.ExpiresAtUtc);
        }

        if (!string.Equals(lease.LeaseDigest, computedDigest, StringComparison.Ordinal))
        {
            return new(false, "lease-digest-mismatch", lease.LeaseId, leasePath, computedDigest, lease.ExpiresAtUtc);
        }

        if (lease.Revoked)
        {
            return new(false, "lease-revoked", lease.LeaseId, leasePath, computedDigest, lease.ExpiresAtUtc);
        }

        if (timestamp > lease.ExpiresAtUtc)
        {
            return new(false, "lease-expired", lease.LeaseId, leasePath, computedDigest, lease.ExpiresAtUtc);
        }

        if (!lease.CommandAllowlist.Contains(command, StringComparer.Ordinal))
        {
            return new(false, "lease-command-scope-mismatch", lease.LeaseId, leasePath, computedDigest, lease.ExpiresAtUtc);
        }

        if (!string.Equals(lease.CmeId, request.CmeId, StringComparison.Ordinal) ||
            !string.Equals(lease.ThreadBindingId, request.ThreadBindingId, StringComparison.Ordinal) ||
            !string.Equals(lease.IdentityTemplateId, request.IdentityTemplateId, StringComparison.Ordinal) ||
            !string.Equals(lease.SoulFrameId, EffectiveSoulFrameId(request), StringComparison.Ordinal) ||
            !string.Equals(lease.AgentiCoreId, EffectiveAgentiCoreId(request), StringComparison.Ordinal) ||
            !string.Equals(lease.AdmissionScope, request.AdmissionScope, StringComparison.Ordinal))
        {
            return new(false, "lease-identity-or-scope-mismatch", lease.LeaseId, leasePath, computedDigest, lease.ExpiresAtUtc);
        }

        if (!lease.ReviewApproved ||
            !lease.OperatorApproved ||
            !lease.StewardWitnessed ||
            !lease.PrimeWitnessed ||
            !lease.CrypticWitnessed)
        {
            return new(false, "lease-witness-bundle-incomplete", lease.LeaseId, leasePath, computedDigest, lease.ExpiresAtUtc);
        }

        return new(true, "lease-verified", lease.LeaseId, leasePath, computedDigest, lease.ExpiresAtUtc);
    }

    private static string ComputeActualApprovalLeaseDigest(ActualApprovalLease lease) =>
        Digest(JsonSerializer.Serialize(
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
            JsonOptions));
}
