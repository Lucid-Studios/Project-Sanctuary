using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Sanctuary.Core;

internal static class SanctuaryMcpLoopbackService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static async Task RunAsync(IReadOnlyList<string> args)
    {
        var host = ReadOption(args, "--host") ?? ReadOption(args, "--http-host") ?? "127.0.0.1";
        var port = ReadInt(args, "--port", ReadInt(args, "--http-port", 8717));
        var maxRequests = ReadInt(args, "--max-requests");
        var installRoot = ReadOption(args, "--install-root")
            ?? Path.Combine(Environment.CurrentDirectory, ".local", "install");
        var intakeRoot = ReadOption(args, "--intake-root")
            ?? Path.Combine(Environment.CurrentDirectory, ".local", "intake");
        var operatorName = ReadOption(args, "--operator-name") ?? "Operator";
        var cmeId = ReadOption(args, "--cme-id") ?? "Codex.CME.ID";
        var domain = ReadOption(args, "--domain") ?? "Lab";
        var role = ReadOption(args, "--role") ?? "IndustrialCME";
        var jobClass = ReadOption(args, "--job-class") ?? "GptUseCaseAlpha";

        if (!IsLoopbackHost(host))
        {
            throw new ArgumentException("Sanctuary MCP alpha service only binds loopback hosts.");
        }

        var service = new SanctuaryReceiptService();
        var startupReceipt = service.Run(new SanctuaryRequest
        {
            Command = "gpt-use-case-testing",
            InstallRootPath = installRoot,
            IntakeRootPath = intakeRoot,
            OperatorName = operatorName,
            CmeId = cmeId,
            Domain = domain,
            Role = role,
            JobClass = jobClass,
            SessionId = "sanctuary-gpt-use-case-service-start"
        });

        using var listener = new TcpListener(IPAddress.Loopback, port);
        listener.Start();
        var boundPort = ((IPEndPoint)listener.LocalEndpoint).Port;

        Console.WriteLine("Sanctuary MCP alpha service started.");
        Console.WriteLine($"Endpoint: http://{host}:{boundPort}/");
        Console.WriteLine($"Startup receipt handle: {startupReceipt.ReceiptHandle}");
        Console.WriteLine("Routes: GET /health, GET /tools, POST /invoke, POST /mcp");
        Console.WriteLine("All exposed tools remain cold read/fetch candidate surfaces.");

        var handled = 0;
        while (maxRequests <= 0 || handled < maxRequests)
        {
            using var client = await listener.AcceptTcpClientAsync();
            handled++;
            await HandleClientAsync(
                client,
                service,
                startupReceipt,
                installRoot,
                intakeRoot,
                operatorName,
                cmeId,
                domain,
                role,
                jobClass);
        }
    }

    private static async Task HandleClientAsync(
        TcpClient client,
        SanctuaryReceiptService service,
        SanctuaryReceipt startupReceipt,
        string installRoot,
        string intakeRoot,
        string operatorName,
        string defaultCmeId,
        string domain,
        string role,
        string jobClass)
    {
        await using var stream = client.GetStream();
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);

        var requestLine = await reader.ReadLineAsync();
        if (string.IsNullOrWhiteSpace(requestLine))
        {
            await WriteResponseAsync(stream, 400, new { error = "empty-request" });
            return;
        }

        var parts = requestLine.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            await WriteResponseAsync(stream, 400, new { error = "malformed-request-line" });
            return;
        }

        var method = parts[0].ToUpperInvariant();
        var path = parts[1].Split('?', 2)[0].TrimEnd('/');
        if (path.Length == 0)
        {
            path = "/";
        }

        var contentLength = 0;
        string? header;
        while (!string.IsNullOrEmpty(header = await reader.ReadLineAsync()))
        {
            var colon = header.IndexOf(':', StringComparison.Ordinal);
            if (colon <= 0)
            {
                continue;
            }

            var name = header[..colon].Trim();
            var value = header[(colon + 1)..].Trim();
            if (string.Equals(name, "Content-Length", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(value, out var parsed))
            {
                contentLength = parsed;
            }
        }

        var body = string.Empty;
        if (contentLength > 0)
        {
            var buffer = new char[contentLength];
            var read = 0;
            while (read < contentLength)
            {
                var chunk = await reader.ReadAsync(buffer, read, contentLength - read);
                if (chunk == 0)
                {
                    break;
                }

                read += chunk;
            }

            body = new string(buffer, 0, read);
        }

        if (method == "GET" && path == "/health")
        {
            await WriteResponseAsync(stream, 200, BuildHealth(startupReceipt));
            return;
        }

        if (method == "GET" && path == "/tools")
        {
            await WriteResponseAsync(stream, 200, BuildToolsPayload());
            return;
        }

        if (method == "POST" && path == "/invoke")
        {
            var invocation = ParseInvocation(body);
            await InvokeToolAsync(
                stream,
                service,
                invocation,
                installRoot,
                intakeRoot,
                operatorName,
                defaultCmeId,
                domain,
                role,
                jobClass);
            return;
        }

        if (method == "POST" && path == "/mcp")
        {
            await HandleMcpJsonRpcAsync(
                stream,
                service,
                body,
                installRoot,
                intakeRoot,
                operatorName,
                defaultCmeId,
                domain,
                role,
                jobClass);
            return;
        }

        await WriteResponseAsync(stream, 404, new { error = "route-not-found", failClosed = true });
    }

    private static async Task InvokeToolAsync(
        Stream stream,
        SanctuaryReceiptService service,
        GptToolInvocationRequest invocation,
        string installRoot,
        string intakeRoot,
        string operatorName,
        string defaultCmeId,
        string domain,
        string role,
        string jobClass)
    {
        if (!GptUseCaseTestingCatalog.TryMapToolToCommand(invocation.Tool, out var command))
        {
            await WriteResponseAsync(stream, 403, new
            {
                schema = "project-sanctuary.gpt-alpha.refusal.v1",
                error = "tool-not-allowlisted",
                failClosed = true,
                providerCalled = false,
                modelBound = false,
                externalActionAuthorized = false
            });
            return;
        }

        var benchRunCount = BuildBenchRunCount(command, invocation.BenchRunCount);
        var receipt = service.Run(new SanctuaryRequest
        {
            Command = command,
            InstallRootPath = installRoot,
            IntakeRootPath = intakeRoot,
            OperatorName = operatorName,
            CmeId = string.IsNullOrWhiteSpace(invocation.CmeId) ? defaultCmeId : invocation.CmeId,
            Domain = domain,
            Role = role,
            JobClass = jobClass,
            SessionId = string.IsNullOrWhiteSpace(invocation.SessionId)
                ? $"gpt-alpha-{command}-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss-fffffff}"
                : invocation.SessionId,
            BenchRunCount = benchRunCount
        });

        await WriteResponseAsync(stream, 200, BuildSanitizedToolResult(invocation.Tool, receipt));
    }

    private static async Task HandleMcpJsonRpcAsync(
        Stream stream,
        SanctuaryReceiptService service,
        string body,
        string installRoot,
        string intakeRoot,
        string operatorName,
        string defaultCmeId,
        string domain,
        string role,
        string jobClass)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(body) ? "{}" : body);
        var root = document.RootElement;
        var id = root.TryGetProperty("id", out var idElement) ? idElement.Clone() : default(JsonElement?);
        var method = root.TryGetProperty("method", out var methodElement) ? methodElement.GetString() : "";

        if (string.Equals(method, "tools/list", StringComparison.Ordinal))
        {
            await WriteResponseAsync(stream, 200, new
            {
                jsonrpc = "2.0",
                id,
                result = new
                {
                    tools = GptUseCaseTestingCatalog.SafeToolSurfaces.Select(tool => new
                    {
                        name = tool.ToolName,
                        description = tool.Description,
                        inputSchema = new
                        {
                            type = "object",
                            properties = new
                            {
                                sessionId = new { type = "string" },
                                cmeId = new { type = "string" },
                                benchRunCount = new { type = "integer", minimum = 1, maximum = 240 }
                            }
                        }
                    })
                }
            });
            return;
        }

        if (string.Equals(method, "tools/call", StringComparison.Ordinal))
        {
            var tool = "";
            var invocation = new GptToolInvocationRequest();
            if (root.TryGetProperty("params", out var paramsElement))
            {
                if (paramsElement.TryGetProperty("name", out var nameElement))
                {
                    tool = nameElement.GetString() ?? "";
                }

                if (paramsElement.TryGetProperty("arguments", out var argumentsElement))
                {
                    invocation = ParseInvocation(argumentsElement.GetRawText());
                }
            }

            invocation = invocation with { Tool = string.IsNullOrWhiteSpace(invocation.Tool) ? tool : invocation.Tool };

            if (!GptUseCaseTestingCatalog.TryMapToolToCommand(invocation.Tool, out var command))
            {
                await WriteResponseAsync(stream, 200, new
                {
                    jsonrpc = "2.0",
                    id,
                    error = new
                    {
                        code = -32602,
                        message = "tool-not-allowlisted",
                        data = new
                        {
                            failClosed = true,
                            providerCalled = false,
                            modelBound = false,
                            externalActionAuthorized = false
                        }
                    }
                });
                return;
            }

            var receipt = service.Run(new SanctuaryRequest
            {
                Command = command,
                InstallRootPath = installRoot,
                IntakeRootPath = intakeRoot,
                OperatorName = operatorName,
                CmeId = string.IsNullOrWhiteSpace(invocation.CmeId) ? defaultCmeId : invocation.CmeId,
                Domain = domain,
                Role = role,
                JobClass = jobClass,
                SessionId = string.IsNullOrWhiteSpace(invocation.SessionId)
                    ? $"mcp-alpha-{command}-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss-fffffff}"
                    : invocation.SessionId,
                BenchRunCount = BuildBenchRunCount(command, invocation.BenchRunCount)
            });
            var result = BuildSanitizedToolResult(invocation.Tool, receipt);
            await WriteResponseAsync(stream, 200, new
            {
                jsonrpc = "2.0",
                id,
                result = new
                {
                    content = new[]
                    {
                        new
                        {
                            type = "text",
                            text = JsonSerializer.Serialize(result, JsonOptions)
                        }
                    },
                    structuredContent = result
                }
            });
            return;
        }

        await WriteResponseAsync(stream, 200, new
        {
            jsonrpc = "2.0",
            id,
            error = new
            {
                code = -32601,
                message = "method-not-supported",
                data = new { failClosed = true }
            }
        });
    }

    private static object BuildHealth(SanctuaryReceipt startupReceipt) => new
    {
        schema = "project-sanctuary.gpt-alpha.health.v1",
        service = "Sanctuary.exe MCP alpha loopback service",
        status = "running",
        owner = "Sanctuary.exe",
        posture = "cold-read-fetch-candidate-only",
        startupReceipt.ReceiptHandle,
        startupReceipt.OutcomeCode,
        startupReceipt.Disposition,
        allGatesClosed = startupReceipt.Gates.AllClosed,
        providerCallsAllowed = false,
        modelBindingAllowed = false,
        externalActionsAllowed = false,
        gelAdmissionAllowed = false,
        selfGelMutationAllowed = false,
        actualActivationAllowed = false
    };

    private static object BuildToolsPayload() => new
    {
        schema = "project-sanctuary.gpt-alpha.tools.v1",
        serviceOwner = "Sanctuary.exe",
        transport = "loopback-http-alpha",
        remoteChatGptUseRequiresSecureMcpTunnel = true,
        tools = GptUseCaseTestingCatalog.SafeToolSurfaces,
        allToolsReadOrFetchOnly = GptUseCaseTestingCatalog.SafeToolSurfaces.All(tool => tool.ReadOrFetchOnly),
        reviewedPerformanceToolsExposed = false,
        secretIntakeToolsExposed = false,
        providerCallToolsExposed = false,
        modelBindingToolsExposed = false
    };

    private static object BuildSanitizedToolResult(string tool, SanctuaryReceipt receipt) => new
    {
        schema = "project-sanctuary.gpt-alpha.tool-result.v1",
        tool,
        receipt.Command,
        receipt.OutcomeCode,
        receipt.Disposition,
        receipt.ReceiptHandle,
        receipt.SessionId,
        receipt.TimestampUtc,
        receipt.CmeId,
        receipt.Domain,
        receipt.Role,
        allGatesClosed = receipt.Gates.AllClosed,
        gates = new
        {
            receipt.Gates.ProviderCalled,
            receipt.Gates.ModelBound,
            receipt.Gates.ExternalActionAuthorized,
            receipt.Gates.ActionAuthorized,
            receipt.Gates.GelAdmitted,
            receipt.Gates.MemoryAdmitted,
            receipt.Gates.SelfGelMutated,
            receipt.Gates.CmeActualActivated,
            receipt.Gates.SanctuaryActualActivated
        },
        evidenceDigest = Digest(JsonSerializer.Serialize(receipt.Evidence, JsonOptions)),
        selectedEvidence = SelectSafeEvidence(receipt.Evidence),
        localReceiptPathReturned = false,
        receiptBodyReturned = false,
        secretPayloadReturned = false
    };

    private static IReadOnlyDictionary<string, object?> SelectSafeEvidence(IReadOnlyDictionary<string, object?> evidence)
    {
        var selected = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var (key, value) in evidence)
        {
            if (selected.Count >= 48 ||
                key.Contains("path", StringComparison.OrdinalIgnoreCase) ||
                key.Contains("directory", StringComparison.OrdinalIgnoreCase) ||
                key.Contains("root", StringComparison.OrdinalIgnoreCase) ||
                key.Contains("payload", StringComparison.OrdinalIgnoreCase) ||
                value is string stringValue && LooksLikePathOrSecret(stringValue))
            {
                continue;
            }

            if (value is null or string or bool or int or long or double or decimal)
            {
                selected[key] = value;
            }
        }

        return selected;
    }

    private static bool LooksLikePathOrSecret(string value) =>
        value.Contains(@":\", StringComparison.Ordinal) ||
        value.Contains("\\Users\\", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("/Users/", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("-----BEGIN", StringComparison.OrdinalIgnoreCase);

    private static GptToolInvocationRequest ParseInvocation(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return new GptToolInvocationRequest();
        }

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        return new GptToolInvocationRequest
        {
            Tool = ReadString(root, "tool") ?? ReadString(root, "name") ?? "",
            SessionId = ReadString(root, "sessionId") ?? ReadString(root, "session_id") ?? "",
            CmeId = ReadString(root, "cmeId") ?? ReadString(root, "cme_id") ?? "",
            BenchRunCount = ReadNullableInt(root, "benchRunCount") ?? ReadNullableInt(root, "bench_run_count")
        };
    }

    private static int BuildBenchRunCount(string command, int? requested)
    {
        if (command is not ("math-learning-bench" or "proof-of-discernment"))
        {
            return 1;
        }

        return Math.Clamp(requested ?? 64, 1, 240);
    }

    private static async Task WriteResponseAsync(Stream stream, int statusCode, object payload)
    {
        var responseBody = JsonSerializer.Serialize(payload, JsonOptions);
        var responseBytes = Encoding.UTF8.GetBytes(responseBody);
        var statusText = statusCode switch
        {
            200 => "OK",
            400 => "Bad Request",
            403 => "Forbidden",
            404 => "Not Found",
            _ => "Internal Server Error"
        };
        var header =
            $"HTTP/1.1 {statusCode} {statusText}\r\n" +
            "Content-Type: application/json; charset=utf-8\r\n" +
            "Cache-Control: no-store\r\n" +
            $"Content-Length: {responseBytes.Length}\r\n" +
            "Connection: close\r\n\r\n";
        var headerBytes = Encoding.ASCII.GetBytes(header);
        await stream.WriteAsync(headerBytes);
        await stream.WriteAsync(responseBytes);
    }

    private static string? ReadString(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static int? ReadNullableInt(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) && value.TryGetInt32(out var parsed)
            ? parsed
            : null;

    private static string? ReadOption(IReadOnlyList<string> args, string name)
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

    private static int ReadInt(IReadOnlyList<string> args, string name, int fallback = 0)
    {
        var value = ReadOption(args, name);
        return int.TryParse(value, out var parsed) ? parsed : fallback;
    }

    private static bool IsLoopbackHost(string host) =>
        string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) ||
        IPAddress.TryParse(host, out var address) && IPAddress.IsLoopback(address);

    private static string Digest(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private sealed record GptToolInvocationRequest
    {
        public string Tool { get; init; } = "";
        public string SessionId { get; init; } = "";
        public string CmeId { get; init; } = "";
        public int? BenchRunCount { get; init; }
    }
}
