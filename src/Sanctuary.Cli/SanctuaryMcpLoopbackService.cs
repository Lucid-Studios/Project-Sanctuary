using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Sanctuary.Core;

internal static class SanctuaryMcpLoopbackService
{
    private const string McpProtocolVersion = "2025-03-26";
    private static readonly ConcurrentDictionary<string, SseClientSession> SseSessions = new(StringComparer.Ordinal);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static async Task RunAsync(IReadOnlyList<string> args)
    {
        var host = ReadOption(args, "--host") ?? ReadOption(args, "--http-host") ?? "127.0.0.1";
        var port = ReadInt(args, "--port", ReadInt(args, "--http-port", 8717));
        var scheme = (ReadOption(args, "--scheme") ?? "http").Trim().ToLowerInvariant();
        var publicBaseUrl = ReadOption(args, "--public-base-url") ?? "";
        var publicBindApproved = ReadBool(args, "--public-bind-approved");
        var certPath = ReadOption(args, "--cert-path") ?? "";
        var certPassword = ReadOption(args, "--cert-password")
            ?? ReadEnvironmentOption(args, "--cert-password-env")
            ?? "";
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
        var loopbackHost = IsLoopbackHost(host);

        if (!loopbackHost && !publicBindApproved)
        {
            throw new ArgumentException("Non-loopback Sanctuary MCP binding requires --public-bind-approved.");
        }

        if (!loopbackHost && !string.Equals(scheme, "https", StringComparison.Ordinal))
        {
            throw new ArgumentException("Non-loopback Sanctuary MCP binding requires --scheme https.");
        }

        X509Certificate2? serverCertificate = null;
        if (string.Equals(scheme, "https", StringComparison.Ordinal))
        {
            if (string.IsNullOrWhiteSpace(certPath))
            {
                throw new ArgumentException("HTTPS Sanctuary MCP binding requires --cert-path.");
            }

            serverCertificate = new X509Certificate2(
                certPath,
                certPassword,
                X509KeyStorageFlags.UserKeySet);
        }
        else if (!string.Equals(scheme, "http", StringComparison.Ordinal))
        {
            throw new ArgumentException("Sanctuary MCP binding scheme must be http or https.");
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

        var bindAddress = ResolveBindAddress(host);
        using var listener = new TcpListener(bindAddress, port);
        listener.Start();
        var boundPort = ((IPEndPoint)listener.LocalEndpoint).Port;
        var advertisedHost = string.Equals(host, "0.0.0.0", StringComparison.Ordinal) ||
            string.Equals(host, "::", StringComparison.Ordinal)
                ? "+"
                : host;
        var localBaseUrl = $"{scheme}://{advertisedHost}:{boundPort}";
        var binding = new McpServiceBinding(
            scheme,
            host,
            boundPort,
            publicBaseUrl,
            publicBindApproved,
            serverCertificate);

        Console.WriteLine("Sanctuary MCP alpha service started.");
        Console.WriteLine($"Endpoint: {localBaseUrl}/");
        if (!string.IsNullOrWhiteSpace(publicBaseUrl))
        {
            Console.WriteLine($"Public MCP URL: {publicBaseUrl.TrimEnd('/')}/mcp");
        }

        Console.WriteLine($"Startup receipt handle: {startupReceipt.ReceiptHandle}");
        Console.WriteLine("Routes: GET /health, GET /tools, GET /.well-known/sanctuary-lab.json, GET /app/manifest.json, POST /invoke, POST /mcp, GET /sse, POST /sse/messages");
        Console.WriteLine("All exposed tools remain cold read/fetch candidate surfaces.");

        var handled = 0;
        while (maxRequests <= 0 || handled < maxRequests)
        {
            var client = await listener.AcceptTcpClientAsync();
            handled++;
            _ = Task.Run(async () =>
            {
                using var ownedClient = client;
                try
                {
                    await HandleClientAsync(
                        ownedClient,
                        service,
                        startupReceipt,
                        installRoot,
                        intakeRoot,
                        operatorName,
                        cmeId,
                        domain,
                        role,
                        jobClass,
                        binding);
                }
                catch (IOException)
                {
                    // Client disconnected during a long-lived SSE request.
                }
                catch (ObjectDisposedException)
                {
                    // Client disconnected during a long-lived SSE request.
                }
                catch (AuthenticationException exception)
                {
                    Console.Error.WriteLine($"Sanctuary edge TLS authentication failed: {exception.Message}");
                }
                catch (Exception exception)
                {
                    Console.Error.WriteLine($"Sanctuary MCP request failed closed: {exception.Message}");
                }
            });
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
        string jobClass,
        McpServiceBinding binding)
    {
        await using var networkStream = client.GetStream();
        if (binding.ServerCertificate is null)
        {
            await HandleHttpStreamAsync(
                networkStream,
                service,
                startupReceipt,
                installRoot,
                intakeRoot,
                operatorName,
                defaultCmeId,
                domain,
                role,
                jobClass,
                binding);
            return;
        }

        await using var tlsStream = new SslStream(networkStream, leaveInnerStreamOpen: false);
        await tlsStream.AuthenticateAsServerAsync(new SslServerAuthenticationOptions
        {
            ServerCertificate = binding.ServerCertificate,
            EnabledSslProtocols = SslProtocols.None
        });

        await HandleHttpStreamAsync(
            tlsStream,
            service,
            startupReceipt,
            installRoot,
            intakeRoot,
            operatorName,
            defaultCmeId,
            domain,
            role,
            jobClass,
            binding);
    }

    private static async Task HandleHttpStreamAsync(
        Stream stream,
        SanctuaryReceiptService service,
        SanctuaryReceipt startupReceipt,
        string installRoot,
        string intakeRoot,
        string operatorName,
        string defaultCmeId,
        string domain,
        string role,
        string jobClass,
        McpServiceBinding binding)
    {
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
        var target = parts[1];
        var path = target.Split('?', 2)[0].TrimEnd('/');
        if (path.Length == 0)
        {
            path = "/";
        }

        var contentLength = 0;
        var transferEncodingChunked = false;
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

            if (string.Equals(name, "Transfer-Encoding", StringComparison.OrdinalIgnoreCase) &&
                value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Any(part => string.Equals(part, "chunked", StringComparison.OrdinalIgnoreCase)))
            {
                transferEncodingChunked = true;
            }
        }

        var body = string.Empty;
        if (transferEncodingChunked)
        {
            body = await ReadChunkedBodyAsync(reader);
        }
        else if (contentLength > 0)
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
            await WriteResponseAsync(stream, 200, BuildHealth(startupReceipt, binding));
            return;
        }

        if (method == "GET" && path == "/tools")
        {
            await WriteResponseAsync(stream, 200, BuildToolsPayload(binding));
            return;
        }

        if (method == "GET" && path == "/.well-known/sanctuary-lab.json")
        {
            await WriteResponseAsync(stream, 200, BuildLabWellKnown(startupReceipt, binding));
            return;
        }

        if (method == "GET" && path == "/app/manifest.json")
        {
            await WriteResponseAsync(stream, 200, BuildAppManifest(binding));
            return;
        }

        if (method == "GET" && path == "/")
        {
            await WriteResponseAsync(stream, 200, BuildRootPayload(binding));
            return;
        }

        if (method == "GET" && path == "/sse")
        {
            await HandleSseOpenAsync(stream);
            return;
        }

        if (method == "POST" && path == "/sse/messages")
        {
            await HandleSseMessageAsync(
                stream,
                target,
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
            var response = BuildMcpJsonRpcResponse(
                    service,
                    body,
                    installRoot,
                    intakeRoot,
                    operatorName,
                    defaultCmeId,
                    domain,
                    role,
                    jobClass);

            await WriteResponseAsync(stream, 200, response ?? new { accepted = true });
            return;
        }

        await WriteResponseAsync(stream, 404, new { error = "route-not-found", failClosed = true });
    }

    private static async Task<string> ReadChunkedBodyAsync(StreamReader reader)
    {
        var builder = new StringBuilder();
        while (true)
        {
            var sizeLine = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(sizeLine))
            {
                continue;
            }

            var semicolon = sizeLine.IndexOf(';', StringComparison.Ordinal);
            var sizeText = semicolon >= 0 ? sizeLine[..semicolon] : sizeLine;
            if (!int.TryParse(
                    sizeText.Trim(),
                    System.Globalization.NumberStyles.HexNumber,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var chunkSize))
            {
                throw new InvalidDataException("Malformed chunked request body.");
            }

            if (chunkSize == 0)
            {
                while (!string.IsNullOrEmpty(await reader.ReadLineAsync()))
                {
                    // Drain optional chunk trailers.
                }

                return builder.ToString();
            }

            var buffer = new char[chunkSize];
            var read = 0;
            while (read < chunkSize)
            {
                var count = await reader.ReadAsync(buffer, read, chunkSize - read);
                if (count == 0)
                {
                    throw new EndOfStreamException("Chunked request body ended early.");
                }

                read += count;
            }

            builder.Append(buffer, 0, read);
            await reader.ReadLineAsync();
        }
    }

    private static async Task HandleSseOpenAsync(Stream stream)
    {
        var sessionId = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
        var session = new SseClientSession(sessionId, stream);
        if (!SseSessions.TryAdd(sessionId, session))
        {
            await WriteResponseAsync(stream, 500, new { error = "sse-session-collision", failClosed = true });
            return;
        }

        await WriteRawAsync(
            stream,
            "HTTP/1.1 200 OK\r\n" +
            "Content-Type: text/event-stream; charset=utf-8\r\n" +
            "Cache-Control: no-store\r\n" +
            "Connection: keep-alive\r\n\r\n");

        await WriteSseFrameAsync(session, "endpoint", $"/sse/messages?sessionId={Uri.EscapeDataString(sessionId)}", serializeData: false);

        try
        {
            while (stream.CanWrite)
            {
                await Task.Delay(TimeSpan.FromSeconds(15));
                await WriteSseCommentAsync(session, $"sanctuary-heartbeat {DateTimeOffset.UtcNow:O}");
            }
        }
        finally
        {
            SseSessions.TryRemove(sessionId, out _);
        }
    }

    private static async Task HandleSseMessageAsync(
        Stream stream,
        string target,
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
        var sessionId = ReadQueryValue(target, "sessionId") ?? ReadQueryValue(target, "session_id");
        if (string.IsNullOrWhiteSpace(sessionId) || !SseSessions.TryGetValue(sessionId, out var session))
        {
            await WriteResponseAsync(stream, 404, new
            {
                error = "sse-session-not-found",
                failClosed = true,
                providerCalled = false,
                modelBound = false,
                externalActionAuthorized = false
            });
            return;
        }

        var response = BuildMcpJsonRpcResponse(
            service,
            body,
            installRoot,
            intakeRoot,
            operatorName,
            defaultCmeId,
            domain,
            role,
            jobClass);

        if (response is not null)
        {
            await WriteSseFrameAsync(session, "message", response);
        }

        await WriteResponseAsync(stream, 202, new
        {
            accepted = true,
            sessionId,
            responseEmitted = response is not null,
            failClosed = false
        });
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

    private static object? BuildMcpJsonRpcResponse(
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

        if (string.Equals(method, "notifications/initialized", StringComparison.Ordinal))
        {
            return null;
        }

        if (string.Equals(method, "initialize", StringComparison.Ordinal))
        {
            return new
            {
                jsonrpc = "2.0",
                id,
                result = new
                {
                    protocolVersion = McpProtocolVersion,
                    capabilities = new
                    {
                        tools = new { }
                    },
                    serverInfo = new
                    {
                        name = "Sanctuary Tool",
                        version = "0.1.0-alpha"
                    },
                    instructions = "Cold read/fetch candidate-only Project Sanctuary alpha. Unknown tools fail closed. No provider calls, model binding, external actions, GEL admission, SelfGEL mutation, CME.Actual, or Sanctuary.Actual."
                }
            };
        }

        if (string.Equals(method, "ping", StringComparison.Ordinal))
        {
            return new
            {
                jsonrpc = "2.0",
                id,
                result = new { }
            };
        }

        if (string.Equals(method, "resources/list", StringComparison.Ordinal))
        {
            return new
            {
                jsonrpc = "2.0",
                id,
                result = new
                {
                    resources = Array.Empty<object>()
                }
            };
        }

        if (string.Equals(method, "prompts/list", StringComparison.Ordinal))
        {
            return new
            {
                jsonrpc = "2.0",
                id,
                result = new
                {
                    prompts = Array.Empty<object>()
                }
            };
        }

        if (string.Equals(method, "tools/list", StringComparison.Ordinal))
        {
            return new
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
                            },
                            additionalProperties = false
                        }
                    })
                }
            };
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
                return new
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
                };
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
            return new
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
            };
        }

        return new
        {
            jsonrpc = "2.0",
            id,
            error = new
            {
                code = -32601,
                message = "method-not-supported",
                data = new { failClosed = true }
            }
        };
    }

    private static object BuildHealth(SanctuaryReceipt startupReceipt, McpServiceBinding binding) => new
    {
        schema = "project-sanctuary.gpt-alpha.health.v1",
        service = binding.PublicBindApproved
            ? "Sanctuary.exe MCP Lab edge gateway"
            : "Sanctuary.exe MCP alpha loopback service",
        status = "running",
        owner = "Sanctuary.exe",
        posture = "cold-read-fetch-candidate-only",
        transport = binding.TransportLabel,
        host = binding.Host,
        port = binding.Port,
        publicBaseUrl = binding.PublicBaseUrl,
        mcpServerUrl = binding.McpServerUrl,
        sanctuaryOwnsEdge = binding.PublicBindApproved,
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

    private static object BuildToolsPayload(McpServiceBinding binding) => new
    {
        schema = "project-sanctuary.gpt-alpha.tools.v1",
        serviceOwner = "Sanctuary.exe",
        transport = binding.TransportLabel,
        remoteChatGptUseRequiresHttpsReachableMcp = true,
        thirdPartyTunnelRequired = false,
        mcpServerUrl = binding.McpServerUrl,
        tools = GptUseCaseTestingCatalog.SafeToolSurfaces,
        allToolsReadOrFetchOnly = GptUseCaseTestingCatalog.SafeToolSurfaces.All(tool => tool.ReadOrFetchOnly),
        reviewedPerformanceToolsExposed = false,
        secretIntakeToolsExposed = false,
        providerCallToolsExposed = false,
        modelBindingToolsExposed = false
    };

    private static object BuildLabWellKnown(SanctuaryReceipt startupReceipt, McpServiceBinding binding) => new
    {
        schema = "project-sanctuary.trivium-forum.lab-edge.v1",
        name = "Sanctuary Lab Edge",
        owner = "Lucid Technologies Department of Agentic Research and Development",
        serviceOwner = "Sanctuary.exe",
        transport = binding.TransportLabel,
        publicBaseUrl = binding.PublicBaseUrl,
        mcpServerUrl = binding.McpServerUrl,
        manifestUrl = binding.Url("/app/manifest.json"),
        healthUrl = binding.Url("/health"),
        toolsUrl = binding.Url("/tools"),
        startupReceipt = startupReceipt.ReceiptHandle,
        thirdPartyTunnelRequired = false,
        payloadHostedBySanctuary = true,
        providerCallsAllowed = false,
        modelBindingAllowed = false,
        externalActionsAllowed = false,
        reviewedPerformanceToolsExposed = false,
        secretPayloadRoutesExposed = false
    };

    private static object BuildAppManifest(McpServiceBinding binding) => new
    {
        schema = "project-sanctuary.chatgpt-app-manifest.v1",
        name = "Sanctuary Alpha",
        description = "Lab-facing alpha tool body for Sanctuary receipts and CME provenance.",
        mcpServerUrl = binding.McpServerUrl,
        authentication = "No Auth alpha; cold read/fetch tools only",
        owner = "Lucid Technologies Department of Agentic Research and Development",
        payloadHostedBySanctuary = true,
        csp = new
        {
            connect_domains = Array.Empty<string>(),
            resource_domains = Array.Empty<string>(),
            notes = "No remote UI assets are required for this MCP alpha manifest."
        },
        gates = new
        {
            providerCallsAllowed = false,
            modelBindingAllowed = false,
            externalActionsAllowed = false,
            reviewedPerformanceToolsExposed = false,
            secretPayloadRoutesExposed = false,
            cmeActualToolExposed = false,
            sanctuaryActualToolExposed = false
        }
    };

    private static object BuildRootPayload(McpServiceBinding binding) => new
    {
        schema = "project-sanctuary.edge-root.v1",
        service = "Sanctuary.exe",
        posture = "cold-read-fetch-candidate-only",
        mcpServerUrl = binding.McpServerUrl,
        wellKnownUrl = binding.Url("/.well-known/sanctuary-lab.json"),
        manifestUrl = binding.Url("/app/manifest.json")
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
            "Content-Security-Policy: default-src 'none'; frame-ancestors 'none'; base-uri 'none'; form-action 'none'\r\n" +
            "X-Content-Type-Options: nosniff\r\n" +
            $"Content-Length: {responseBytes.Length}\r\n" +
            "Connection: close\r\n\r\n";
        var headerBytes = Encoding.ASCII.GetBytes(header);
        await stream.WriteAsync(headerBytes);
        await stream.WriteAsync(responseBytes);
    }

    private static async Task WriteRawAsync(Stream stream, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        await stream.WriteAsync(bytes);
        await stream.FlushAsync();
    }

    private static async Task WriteSseFrameAsync(SseClientSession session, string eventName, object payload, bool serializeData = true)
    {
        var data = serializeData ? JsonSerializer.Serialize(payload, JsonOptions) : payload.ToString() ?? "";
        var frame = $"event: {eventName}\n" +
                    $"data: {data.Replace("\r", "", StringComparison.Ordinal).Replace("\n", "\ndata: ", StringComparison.Ordinal)}\n\n";
        await session.WriteLock.WaitAsync();
        try
        {
            await WriteRawAsync(session.Stream, frame);
        }
        finally
        {
            session.WriteLock.Release();
        }
    }

    private static async Task WriteSseCommentAsync(SseClientSession session, string comment)
    {
        await session.WriteLock.WaitAsync();
        try
        {
            await WriteRawAsync(session.Stream, $": {comment.Replace("\r", "", StringComparison.Ordinal).Replace("\n", " ", StringComparison.Ordinal)}\n\n");
        }
        finally
        {
            session.WriteLock.Release();
        }
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

    private static string? ReadEnvironmentOption(IReadOnlyList<string> args, string name)
    {
        var variableName = ReadOption(args, name);
        return string.IsNullOrWhiteSpace(variableName)
            ? null
            : Environment.GetEnvironmentVariable(variableName);
    }

    private static bool ReadBool(IReadOnlyList<string> args, string name)
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

    private static int ReadInt(IReadOnlyList<string> args, string name, int fallback = 0)
    {
        var value = ReadOption(args, name);
        return int.TryParse(value, out var parsed) ? parsed : fallback;
    }

    private static string? ReadQueryValue(string target, string name)
    {
        var question = target.IndexOf('?', StringComparison.Ordinal);
        if (question < 0 || question == target.Length - 1)
        {
            return null;
        }

        foreach (var pair in target[(question + 1)..].Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);
            var key = Uri.UnescapeDataString(parts[0]);
            if (!string.Equals(key, name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return parts.Length == 2 ? Uri.UnescapeDataString(parts[1].Replace('+', ' ')) : "";
        }

        return null;
    }

    private static bool IsLoopbackHost(string host) =>
        string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) ||
        IPAddress.TryParse(host, out var address) && IPAddress.IsLoopback(address);

    private static IPAddress ResolveBindAddress(string host)
    {
        if (string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase))
        {
            return IPAddress.Loopback;
        }

        if (string.Equals(host, "0.0.0.0", StringComparison.Ordinal) ||
            string.Equals(host, "*", StringComparison.Ordinal))
        {
            return IPAddress.Any;
        }

        if (string.Equals(host, "::", StringComparison.Ordinal))
        {
            return IPAddress.IPv6Any;
        }

        return IPAddress.TryParse(host, out var address)
            ? address
            : IPAddress.Any;
    }

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

    private sealed class SseClientSession(string sessionId, Stream stream)
    {
        public string SessionId { get; } = sessionId;
        public Stream Stream { get; } = stream;
        public SemaphoreSlim WriteLock { get; } = new(1, 1);
    }

    private sealed record McpServiceBinding(
        string Scheme,
        string Host,
        int Port,
        string PublicBaseUrl,
        bool PublicBindApproved,
        X509Certificate2? ServerCertificate)
    {
        public string TransportLabel =>
            PublicBindApproved ? "sanctuary-owned-https-edge" : "loopback-http-alpha";

        public string LocalBaseUrl
        {
            get
            {
                var host = string.Equals(Host, "0.0.0.0", StringComparison.Ordinal) ||
                    string.Equals(Host, "*", StringComparison.Ordinal)
                        ? "127.0.0.1"
                        : Host;
                return $"{Scheme}://{host}:{Port}";
            }
        }

        public string EffectiveBaseUrl =>
            string.IsNullOrWhiteSpace(PublicBaseUrl)
                ? LocalBaseUrl
                : PublicBaseUrl.TrimEnd('/');

        public string McpServerUrl => Url("/mcp");

        public string Url(string path) =>
            $"{EffectiveBaseUrl}/{path.TrimStart('/')}";
    }
}
