using System.Text.Json;

namespace Sanctuary.Core;

public sealed partial class SanctuaryReceiptService
{
    private static string ReadStringProperty(JsonElement root, string propertyName) =>
        TryGetPropertyIgnoreCase(root, propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? ""
            : "";

    private static IReadOnlyList<string> BuildVisibleSecurityRoots(string installRootPath) => new[]
    {
        Path.Combine(installRootPath, "receipts"),
        Path.Combine(installRootPath, "gel"),
        Path.Combine(installRootPath, "cgel"),
        Path.Combine(installRootPath, "service"),
        Path.Combine(installRootPath, "access"),
        Path.Combine(installRootPath, "issues")
    };

    private static IReadOnlyList<string> SecurityLeakTokens() => new[]
    {
        @"\OneDrive\Documents\Personal",
        @"Personal MISC Legal",
        "\"sourceRootPath\"",
        "\"originalFileName\"",
        "\"relativePath\"",
        "private sample payload"
    };

    private static bool IsTextLikeSecuritySurface(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        return string.Equals(extension, ".json", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(extension, ".jsonl", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(extension, ".md", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(extension, ".txt", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsUnderSkippedRoot(string filePath, IReadOnlyList<string> skippedRoots)
    {
        var fullPath = Path.GetFullPath(filePath);
        return skippedRoots.Any(root =>
            fullPath.StartsWith(Path.GetFullPath(root), StringComparison.OrdinalIgnoreCase));
    }

    private static bool TryGetBooleanProperty(JsonElement root, string objectName, string propertyName, out bool value)
    {
        value = false;
        if (!TryGetPropertyIgnoreCase(root, objectName, out var nested) ||
            !TryGetPropertyIgnoreCase(nested, propertyName, out var property) ||
            (property.ValueKind != JsonValueKind.True && property.ValueKind != JsonValueKind.False))
        {
            return false;
        }

        value = property.GetBoolean();
        return true;
    }

    private static bool IsReviewedPerformanceOpenReceipt(JsonElement root)
    {
        var command = ReadStringProperty(root, "Command");
        var disposition = ReadStringProperty(root, "Disposition");
        if (!ReviewedPerformanceCommands.Contains(command) ||
            !string.Equals(disposition, "CompletedReviewed", StringComparison.Ordinal))
        {
            return false;
        }

        return TryGetBooleanProperty(root, "Evidence", "reviewedPerformanceApproved", out var approved) &&
            approved &&
            TryGetBooleanProperty(root, "Gates", "ExternalActionAuthorized", out var externalActionAuthorized) &&
            !externalActionAuthorized &&
            TryGetBooleanProperty(root, "Gates", "ProviderCalled", out var providerCalled) &&
            !providerCalled &&
            TryGetBooleanProperty(root, "Gates", "ModelBound", out var modelBound) &&
            !modelBound &&
            TryGetBooleanProperty(root, "Gates", "PersonhoodClaimed", out var personhoodClaimed) &&
            !personhoodClaimed &&
            TryGetBooleanProperty(root, "Gates", "SovereigntyClaimed", out var sovereigntyClaimed) &&
            !sovereigntyClaimed;
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string propertyName, out JsonElement property)
    {
        foreach (var candidate in element.EnumerateObject())
        {
            if (string.Equals(candidate.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                property = candidate.Value;
                return true;
            }
        }

        property = default;
        return false;
    }
}
