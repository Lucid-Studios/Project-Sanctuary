using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Sanctuary.Core;

public sealed partial class SanctuaryReceiptService
{
    private static byte[] LoadOrCreateMasterKey(string keyCustodyPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(keyCustodyPath)!);
        if (File.Exists(keyCustodyPath))
        {
            var protectedKey = File.ReadAllBytes(keyCustodyPath);
            return OperatingSystem.IsWindows()
                ? ProtectedData.Unprotect(protectedKey, KeyEntropy(), DataProtectionScope.CurrentUser)
                : protectedKey;
        }

        var key = RandomNumberGenerator.GetBytes(32);
        var protectedBytes = OperatingSystem.IsWindows()
            ? ProtectedData.Protect(key, KeyEntropy(), DataProtectionScope.CurrentUser)
            : key;
        File.WriteAllBytes(keyCustodyPath, protectedBytes);
        return key;
    }

    private static SealedBytes EncryptBytes(byte[] key, byte[] plaintext)
    {
        var nonce = RandomNumberGenerator.GetBytes(12);
        var tag = new byte[16];
        var ciphertext = new byte[plaintext.Length];
        using var aes = new AesGcm(key, tag.Length);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);
        return new SealedBytes(
            Convert.ToBase64String(nonce),
            Convert.ToBase64String(tag),
            Convert.ToBase64String(ciphertext));
    }

    private static string ToMarkdown(SanctuaryReceipt receipt)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Sanctuary Receipt");
        builder.AppendLine();
        builder.AppendLine($"- command: `{receipt.Command}`");
        builder.AppendLine($"- outcome: `{receipt.OutcomeCode}`");
        builder.AppendLine($"- disposition: `{receipt.Disposition}`");
        builder.AppendLine($"- session: `{receipt.SessionId}`");
        builder.AppendLine($"- CME ID: `{receipt.CmeId}`");
        builder.AppendLine($"- all gates closed: `{receipt.Gates.AllClosed}`");
        builder.AppendLine();
        builder.AppendLine(receipt.GovernanceTrace);
        return builder.ToString();
    }

    private static InstallLocalLabCmeContext ReadInstallLocalLabCmeContext(SanctuaryRequest request)
    {
        var path = Path.Combine(request.InstallRootPath, "mos", "lab-cme-context.json");
        if (!File.Exists(path))
        {
            return new InstallLocalLabCmeContext(
                Path: path,
                Present: false,
                Schema: "project-sanctuary.install.lab-cme-context.v1",
                Active: false,
                LabActorCmeId: "",
                TelemetrySubjectCmeId: "",
                ServiceIdentityId: "",
                IdentityTemplateId: "",
                ResidueCapturePolicy: "undeclared-install-local-context",
                GovernanceSimulationBodies: Array.Empty<string>());
        }

        using var document = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
        var root = document.RootElement;
        var governanceBodies = ReadStringArrayProperty(root, "governanceSimulationBodies");
        return new InstallLocalLabCmeContext(
            Path: path,
            Present: true,
            Schema: ReadStringProperty(root, "schema") ?? "project-sanctuary.install.lab-cme-context.v1",
            Active: ReadBoolProperty(root, "active") ?? true,
            LabActorCmeId: ReadStringProperty(root, "labActorCmeId") ?? "",
            TelemetrySubjectCmeId: ReadStringProperty(root, "telemetrySubjectCmeId") ?? "",
            ServiceIdentityId: ReadStringProperty(root, "serviceIdentityId") ?? "",
            IdentityTemplateId: ReadStringProperty(root, "identityTemplateId") ?? "",
            ResidueCapturePolicy: ReadStringProperty(root, "residueCapturePolicy") ?? "candidate-gel-residue-only",
            GovernanceSimulationBodies: governanceBodies);
    }

    private static IReadOnlyList<string> ReadStringArrayProperty(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value) ||
            value.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<string>();
        }

        return value
            .EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString() ?? "")
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToArray();
    }

    private static bool? ReadBoolProperty(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null
        };
    }

    private sealed record InstallLocalLabCmeContext(
        string Path,
        bool Present,
        string Schema,
        bool Active,
        string LabActorCmeId,
        string TelemetrySubjectCmeId,
        string ServiceIdentityId,
        string IdentityTemplateId,
        string ResidueCapturePolicy,
        IReadOnlyList<string> GovernanceSimulationBodies);

    private static string SafeSegment(string value)
    {
        var builder = new StringBuilder();
        foreach (var character in value)
        {
            builder.Append(char.IsLetterOrDigit(character) || character is '-' or '_' or '.'
                ? character
                : '-');
        }

        return builder.Length == 0 ? "default" : builder.ToString();
    }

    private static string LispString(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);

    private static string LatexString(string value)
    {
        var builder = new StringBuilder();
        foreach (var character in value)
        {
            builder.Append(character switch
            {
                '\\' => "\\textbackslash{}",
                '{' => "\\{",
                '}' => "\\}",
                '$' => "\\$",
                '&' => "\\&",
                '%' => "\\%",
                '#' => "\\#",
                '_' => "\\_",
                '~' => "\\textasciitilde{}",
                '^' => "\\textasciicircum{}",
                '\r' => "",
                '\n' => "\\par ",
                _ => character.ToString()
            });
        }

        return builder.ToString();
    }

    private static string Digest16(string value) => Digest(value)[..16];

    private static string Digest(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string DigestBytes(byte[] value)
    {
        var bytes = SHA256.HashData(value);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static byte[] KeyEntropy() =>
        Encoding.UTF8.GetBytes("ProjectSanctuary.LocalLabGelTips.v1");

    private static void WriteJsonFile(string path, object payload)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        lock (AppendLock)
        {
            File.WriteAllText(path, JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8);
        }
    }

    private static void WriteJsonFileCreateNew(string path, object payload)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        lock (AppendLock)
        {
            using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
            using var writer = new StreamWriter(stream, Encoding.UTF8);
            writer.Write(JsonSerializer.Serialize(payload, JsonOptions));
        }
    }

    private static void WriteTextFile(string path, string payload)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        lock (AppendLock)
        {
            File.WriteAllText(path, payload, Encoding.UTF8);
        }
    }

    private static void AppendJsonLine(string path, string line)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        lock (AppendLock)
        {
            for (var attempt = 0; attempt < 10; attempt++)
            {
                try
                {
                    using var stream = new FileStream(
                        path,
                        FileMode.Append,
                        FileAccess.Write,
                        FileShare.ReadWrite);
                    using var writer = new StreamWriter(stream, Encoding.UTF8);
                    writer.WriteLine(line);
                    return;
                }
                catch (IOException) when (attempt < 9)
                {
                    Thread.Sleep(25 * (attempt + 1));
                }
            }
        }
    }
}
