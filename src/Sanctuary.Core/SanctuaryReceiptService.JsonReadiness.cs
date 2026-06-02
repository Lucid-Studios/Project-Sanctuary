using System.Text.Json;

namespace Sanctuary.Core;

public sealed partial class SanctuaryReceiptService
{
    private static int ReadPreviousBenchRunCount(string summaryPath)
    {
        if (!File.Exists(summaryPath))
        {
            return 0;
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(summaryPath));
            return document.RootElement.TryGetProperty("cumulativeRunCount", out var cumulativeRunCount) &&
                cumulativeRunCount.ValueKind == JsonValueKind.Number &&
                cumulativeRunCount.TryGetInt32(out var parsed)
                    ? parsed
                    : 0;
        }
        catch (JsonException)
        {
            return 0;
        }
        catch (IOException)
        {
            return 0;
        }
    }

    private static int ReadJsonInt(string path, string propertyName)
    {
        if (!File.Exists(path))
        {
            return 0;
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            return document.RootElement.TryGetProperty(propertyName, out var value) &&
                value.ValueKind == JsonValueKind.Number &&
                value.TryGetInt32(out var parsed)
                    ? parsed
                    : 0;
        }
        catch (JsonException)
        {
            return 0;
        }
        catch (IOException)
        {
            return 0;
        }
    }

    private static int ReadJsonIntAt(string path, params string[] propertyPath)
    {
        if (!File.Exists(path) || propertyPath.Length == 0)
        {
            return 0;
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            var current = document.RootElement;
            foreach (var propertyName in propertyPath)
            {
                if (current.ValueKind != JsonValueKind.Object ||
                    !current.TryGetProperty(propertyName, out current))
                {
                    return 0;
                }
            }

            return current.ValueKind == JsonValueKind.Number &&
                current.TryGetInt32(out var parsed)
                    ? parsed
                    : 0;
        }
        catch (JsonException)
        {
            return 0;
        }
        catch (IOException)
        {
            return 0;
        }
    }

    private static double ReadJsonDouble(string path, string propertyName)
    {
        if (!File.Exists(path))
        {
            return 0d;
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            return document.RootElement.TryGetProperty(propertyName, out var value) &&
                value.ValueKind == JsonValueKind.Number &&
                value.TryGetDouble(out var parsed)
                    ? Math.Round(parsed, 4)
                    : 0d;
        }
        catch (JsonException)
        {
            return 0d;
        }
        catch (IOException)
        {
            return 0d;
        }
    }

    private static int ReadJsonArrayCount(string path, string propertyName)
    {
        if (!File.Exists(path))
        {
            return 0;
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            return document.RootElement.TryGetProperty(propertyName, out var value) &&
                value.ValueKind == JsonValueKind.Array
                    ? value.GetArrayLength()
                    : 0;
        }
        catch (JsonException)
        {
            return 0;
        }
        catch (IOException)
        {
            return 0;
        }
    }

    private static bool ReadJsonBool(string path, string propertyName) =>
        ReadJsonBoolAt(path, propertyName);

    private static bool ReadJsonBoolAt(string path, params string[] propertyPath)
    {
        if (!File.Exists(path) || propertyPath.Length == 0)
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            var current = document.RootElement;
            foreach (var propertyName in propertyPath)
            {
                if (current.ValueKind != JsonValueKind.Object ||
                    !current.TryGetProperty(propertyName, out current))
                {
                    return false;
                }
            }

            return current.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => false
            };
        }
        catch (JsonException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }

    private static string ReadJsonString(string path, string propertyName)
    {
        if (!File.Exists(path))
        {
            return "";
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            return document.RootElement.TryGetProperty(propertyName, out var value) &&
                value.ValueKind == JsonValueKind.String
                    ? value.GetString() ?? ""
                    : "";
        }
        catch (JsonException)
        {
            return "";
        }
        catch (IOException)
        {
            return "";
        }
    }

    private static string ReadOptionalJsonString(JsonElement element, string propertyName) =>
        element.ValueKind == JsonValueKind.Object &&
        element.TryGetProperty(propertyName, out var value) &&
        value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? ""
            : "";

    private static int CountJsonlLines(string path)
    {
        if (!File.Exists(path))
        {
            return 0;
        }

        try
        {
            return File.ReadLines(path).Count(line => !string.IsNullOrWhiteSpace(line));
        }
        catch (IOException)
        {
            return 0;
        }
    }

    private static string DigestLastJsonlLine(string path)
    {
        if (!File.Exists(path))
        {
            return "";
        }

        try
        {
            var last = File.ReadLines(path).LastOrDefault(line => !string.IsNullOrWhiteSpace(line));
            return string.IsNullOrWhiteSpace(last) ? "" : Digest(last);
        }
        catch (IOException)
        {
            return "";
        }
    }
}
