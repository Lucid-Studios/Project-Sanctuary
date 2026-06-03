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
        var selectedCmeId = ReadOption(args, "--cme-id");
        var defaultCallerCmeId = selectedCmeId ?? "";
        var defaultThreadBindingId = ReadOption(args, "--thread-binding-id") ?? "";
        var defaultSoulFrameId = ReadOption(args, "--soulframe-id") ?? "";
        var defaultAgentiCoreId = ReadOption(args, "--agenticore-id") ?? "";
        var defaultIdentityTemplateId = ReadOption(args, "--identity-template-id") ?? "SLI.Lisp.Industrial.CME.Template";
        var defaultSubjectCmeId = ReadOption(args, "--subject-cme-id") ?? "";
        var serviceIdentityId = ReadOption(args, "--service-id") ?? "Sanctuary.Actual.ID";
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
        var startupCmeId = string.IsNullOrWhiteSpace(defaultCallerCmeId)
            ? serviceIdentityId
            : defaultCallerCmeId;
        var startupThreadBindingId = string.IsNullOrWhiteSpace(defaultCallerCmeId)
            ? ""
            : defaultThreadBindingId;
        var startupSoulFrameId = string.IsNullOrWhiteSpace(defaultCallerCmeId)
            ? ""
            : defaultSoulFrameId;
        var startupAgentiCoreId = string.IsNullOrWhiteSpace(defaultCallerCmeId)
            ? ""
            : defaultAgentiCoreId;
        var startupSubjectCmeId = string.IsNullOrWhiteSpace(defaultCallerCmeId)
            ? ""
            : defaultSubjectCmeId;
        var startupReceipt = service.Run(new SanctuaryRequest
        {
            Command = "gpt-use-case-testing",
            InstallRootPath = installRoot,
            IntakeRootPath = intakeRoot,
            OperatorName = operatorName,
            CmeId = startupCmeId,
            CmeIdentitySelected = true,
            ServiceIdentityId = serviceIdentityId,
            CallerCmeId = startupCmeId,
            ThreadBindingId = startupThreadBindingId,
            IdentityTemplateId = defaultIdentityTemplateId,
            SoulFrameId = startupSoulFrameId,
            AgentiCoreId = startupAgentiCoreId,
            SubjectCmeId = startupSubjectCmeId,
            Domain = domain,
            Role = "SanctuaryService",
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
                        defaultCallerCmeId,
                        defaultThreadBindingId,
                        defaultSoulFrameId,
                        defaultAgentiCoreId,
                        defaultIdentityTemplateId,
                        defaultSubjectCmeId,
                        serviceIdentityId,
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
        string defaultThreadBindingId,
        string defaultSoulFrameId,
        string defaultAgentiCoreId,
        string defaultIdentityTemplateId,
        string defaultSubjectCmeId,
        string serviceIdentityId,
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
                defaultThreadBindingId,
                defaultSoulFrameId,
                defaultAgentiCoreId,
                defaultIdentityTemplateId,
                defaultSubjectCmeId,
                serviceIdentityId,
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
            defaultThreadBindingId,
            defaultSoulFrameId,
            defaultAgentiCoreId,
            defaultIdentityTemplateId,
            defaultSubjectCmeId,
            serviceIdentityId,
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
        string defaultThreadBindingId,
        string defaultSoulFrameId,
        string defaultAgentiCoreId,
        string defaultIdentityTemplateId,
        string defaultSubjectCmeId,
        string serviceIdentityId,
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
            await WriteResponseAsync(stream, 200, BuildHealth(startupReceipt, binding, serviceIdentityId, defaultIdentityTemplateId, defaultCmeId, defaultSubjectCmeId));
            return;
        }

        if (method == "GET" && path == "/tools")
        {
            await WriteResponseAsync(stream, 200, BuildToolsPayload(binding));
            return;
        }

        if (method == "GET" && path == "/.well-known/sanctuary-lab.json")
        {
            await WriteResponseAsync(stream, 200, BuildLabWellKnown(startupReceipt, binding, serviceIdentityId, defaultIdentityTemplateId, defaultCmeId, defaultSubjectCmeId));
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
                defaultThreadBindingId,
                defaultSoulFrameId,
                defaultAgentiCoreId,
                defaultIdentityTemplateId,
                defaultSubjectCmeId,
                serviceIdentityId,
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
                defaultThreadBindingId,
                defaultSoulFrameId,
                defaultAgentiCoreId,
                defaultIdentityTemplateId,
                defaultSubjectCmeId,
                serviceIdentityId,
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
                    defaultThreadBindingId,
                    defaultSoulFrameId,
                    defaultAgentiCoreId,
                    defaultIdentityTemplateId,
                    defaultSubjectCmeId,
                    serviceIdentityId,
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
        string defaultThreadBindingId,
        string defaultSoulFrameId,
        string defaultAgentiCoreId,
        string defaultIdentityTemplateId,
        string defaultSubjectCmeId,
        string serviceIdentityId,
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
            defaultThreadBindingId,
            defaultSoulFrameId,
            defaultAgentiCoreId,
            defaultIdentityTemplateId,
            defaultSubjectCmeId,
            serviceIdentityId,
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
        string defaultThreadBindingId,
        string defaultSoulFrameId,
        string defaultAgentiCoreId,
        string defaultIdentityTemplateId,
        string defaultSubjectCmeId,
        string serviceIdentityId,
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

        var seating = BuildSeatedInvocation(
            invocation,
            command,
            BuildBenchRunCount(command, invocation.BenchRunCount),
            "gpt-alpha",
            installRoot,
            intakeRoot,
            operatorName,
            defaultCmeId,
            defaultThreadBindingId,
            defaultSoulFrameId,
            defaultAgentiCoreId,
            defaultIdentityTemplateId,
            defaultSubjectCmeId,
            serviceIdentityId,
            domain,
            role,
            jobClass);
        if (!seating.Allowed)
        {
            await WriteResponseAsync(stream, seating.HttpStatus, seating.Payload!);
            return;
        }

        SanctuaryReceipt receipt;
        try
        {
            receipt = service.Run(seating.Request!);
        }
        catch (ArgumentException exception)
        {
            await WriteResponseAsync(stream, 403, BuildCmeIdentityDeniedPayload(seating.CmeId, exception.Message));
            return;
        }

        await WriteResponseAsync(stream, 200, BuildSanitizedToolResult(invocation.Tool, receipt));
    }

    private static object? BuildMcpJsonRpcResponse(
        SanctuaryReceiptService service,
        string body,
        string installRoot,
        string intakeRoot,
        string operatorName,
        string defaultCmeId,
        string defaultThreadBindingId,
        string defaultSoulFrameId,
        string defaultAgentiCoreId,
        string defaultIdentityTemplateId,
        string defaultSubjectCmeId,
        string serviceIdentityId,
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
                    instructions = "Cold read/fetch candidate-only Project Sanctuary alpha. Every tool call must provide a caller CME identity such as Codex.CME.ID or Oria.CME.ID. Unknown tools fail closed. No provider calls, model binding, external actions, GEL admission, SelfGEL mutation, CME.Actual, or Sanctuary.Actual."
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
                    tools = GptUseCaseTestingCatalog.SafeToolSurfaces.Select(BuildMcpToolDescriptor)
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

            var seating = BuildSeatedInvocation(
                invocation,
                command,
                BuildBenchRunCount(command, invocation.BenchRunCount),
                "mcp-alpha",
                installRoot,
                intakeRoot,
                operatorName,
                defaultCmeId,
                defaultThreadBindingId,
                defaultSoulFrameId,
                defaultAgentiCoreId,
                defaultIdentityTemplateId,
                defaultSubjectCmeId,
                serviceIdentityId,
                domain,
                role,
                jobClass);
            if (!seating.Allowed)
            {
                return new
                {
                    jsonrpc = "2.0",
                    id,
                    error = new
                    {
                        code = -32602,
                        message = seating.Error,
                        data = seating.Payload
                    }
                };
            }

            SanctuaryReceipt receipt;
            try
            {
                receipt = service.Run(seating.Request!);
            }
            catch (ArgumentException exception)
            {
                return new
                {
                    jsonrpc = "2.0",
                    id,
                    error = new
                    {
                        code = -32602,
                        message = "cme-identity-denied",
                        data = BuildCmeIdentityDeniedPayload(seating.CmeId, exception.Message)
                    }
                };
            }
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
                    structuredContent = result,
                    _meta = BuildMcpToolResultMeta(invocation.Tool, receipt)
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

    private static object BuildMcpToolDescriptor(GptUseCaseToolSurface tool) => new
    {
        name = tool.ToolName,
        title = BuildToolTitle(tool.ToolName),
        description = tool.Description,
        inputSchema = BuildMcpToolInputSchema(),
        outputSchema = BuildMcpToolOutputSchema(),
        annotations = new
        {
            readOnlyHint = tool.ReadOrFetchOnly,
            destructiveHint = false,
            openWorldHint = false,
            idempotentHint = !tool.WritesCandidateResidue
        },
        _meta = new Dictionary<string, object?>
        {
            ["openai/toolInvocation/invoking"] = "Writing Sanctuary receipt...",
            ["openai/toolInvocation/invoked"] = "Sanctuary receipt ready.",
            ["sanctuary/accessKind"] = tool.AccessKind,
            ["sanctuary/writesCandidateResidue"] = tool.WritesCandidateResidue,
            ["sanctuary/readFetchOnly"] = tool.ReadOrFetchOnly,
            ["sanctuary/noProviderCalls"] = !tool.CallsProvider,
            ["sanctuary/noModelBinding"] = !tool.BindsModel,
            ["sanctuary/noExternalAction"] = !tool.AuthorizesExternalAction,
            ["sanctuary/noGelAdmission"] = !tool.AdmitsGel,
            ["sanctuary/noSelfGelMutation"] = !tool.MutatesSelfGel,
            ["sanctuary/noActualActivation"] = !tool.ActivatesActual
        }
    };

    private static string BuildToolTitle(string toolName)
    {
        var name = toolName.StartsWith("sanctuary.", StringComparison.OrdinalIgnoreCase)
            ? toolName["sanctuary.".Length..]
            : toolName;
        return string.Join(
            " ",
            name.Split('_', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(part => char.ToUpperInvariant(part[0]) + part[1..]));
    }

    private static object BuildMcpToolInputSchema() => new
    {
        type = "object",
        properties = new
        {
            sessionId = new { type = "string" },
            cmeId = new { type = "string" },
            threadBindingId = new { type = "string" },
            parentCmeId = new { type = "string" },
            subjectCmeId = new { type = "string" },
            domain = new { type = "string" },
            role = new { type = "string" },
            jobClass = new { type = "string" },
            swarmId = new { type = "string" },
            subAgentId = new { type = "string" },
            identityTemplateId = new { type = "string" },
            soulFrameId = new { type = "string" },
            agentiCoreId = new { type = "string" },
            actualApprovalLeasePath = new { type = "string" },
            benchRunCount = new { type = "integer", minimum = 1, maximum = 240 }
        },
        required = new[] { "cmeId" },
        additionalProperties = false
    };

    private static object BuildMcpToolOutputSchema() => new
    {
        type = "object",
        properties = new
        {
            schema = new { type = "string" },
            tool = new { type = "string" },
            Command = new { type = "string" },
            OutcomeCode = new { type = "string" },
            Disposition = new { type = "string" },
            ReceiptHandle = new { type = "string" },
            SessionId = new { type = "string" },
            TimestampUtc = new { type = "string" },
            CmeId = new { type = "string" },
            Domain = new { type = "string" },
            Role = new { type = "string" },
            allGatesClosed = new { type = "boolean" },
            gates = new { type = "object" },
            evidenceDigest = new { type = "string" },
            selectedEvidence = new { type = "object" },
            localReceiptPathReturned = new { type = "boolean" },
            receiptBodyReturned = new { type = "boolean" },
            secretPayloadReturned = new { type = "boolean" }
        },
        required = new[]
        {
            "schema",
            "tool",
            "Command",
            "OutcomeCode",
            "Disposition",
            "ReceiptHandle",
            "CmeId",
            "allGatesClosed",
            "gates",
            "localReceiptPathReturned",
            "receiptBodyReturned",
            "secretPayloadReturned"
        },
        additionalProperties = true
    };

    private static object BuildMcpToolResultMeta(string tool, SanctuaryReceipt receipt) => new
    {
        sanctuaryTool = tool,
        receiptHandle = receipt.ReceiptHandle,
        outcomeCode = receipt.OutcomeCode,
        allGatesClosed = receipt.Gates.AllClosed,
        localReceiptPathReturned = false,
        receiptBodyReturned = false,
        secretPayloadReturned = false,
        widgetSafe = true
    };

    private static object BuildHealth(
        SanctuaryReceipt startupReceipt,
        McpServiceBinding binding,
        string serviceIdentityId,
        string defaultIdentityTemplateId,
        string defaultCallerCmeId,
        string defaultSubjectCmeId) => new
    {
        schema = "project-sanctuary.gpt-alpha.health.v1",
        service = binding.PublicBindApproved
            ? "Sanctuary.exe MCP Lab edge gateway"
            : "Sanctuary.exe MCP alpha loopback service",
        status = "running",
        owner = "Sanctuary.exe",
        serviceIdentityId,
        serviceIdentityIsCme = false,
        defaultIdentityTemplateId,
        defaultCallerCmeIdIsParticipant = !string.IsNullOrWhiteSpace(defaultCallerCmeId),
        defaultCallerCmeId,
        defaultSubjectCmeId,
        participantCmeIdRequiredPerToolCall = true,
        participantIdentityPattern = "{Name}.CME.ID",
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
        serviceIdentityId = "Sanctuary.Actual.ID",
        serviceIdentityIsCme = false,
        participantCmeIdRequiredPerToolCall = true,
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

    private static object BuildLabWellKnown(
        SanctuaryReceipt startupReceipt,
        McpServiceBinding binding,
        string serviceIdentityId,
        string defaultIdentityTemplateId,
        string defaultCallerCmeId,
        string defaultSubjectCmeId) => new
    {
        schema = "project-sanctuary.trivium-forum.lab-edge.v1",
        name = "Sanctuary Lab Edge",
        owner = "Lucid Technologies Department of Agentic Research and Development",
        serviceOwner = "Sanctuary.exe",
        serviceIdentityId,
        serviceIdentityIsCme = false,
        defaultIdentityTemplateId,
        defaultCallerCmeIdIsParticipant = !string.IsNullOrWhiteSpace(defaultCallerCmeId),
        defaultCallerCmeId,
        defaultSubjectCmeId,
        participantCmeIdRequiredPerToolCall = true,
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
        serviceIdentityId = "Sanctuary.Actual.ID",
        serviceIdentityIsCme = false,
        participantIdentityPattern = "{Name}.CME.ID",
        participantCmeIdRequiredPerToolCall = true,
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
        serviceIdentityId = "Sanctuary.Actual.ID",
        serviceIdentityIsCme = false,
        participantCmeIdRequiredPerToolCall = true,
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

        foreach (var (key, value) in evidence.Where(pair => IsPrioritySafeEvidenceKey(pair.Key)))
        {
            TryAddSafeEvidence(selected, key, value);
        }

        foreach (var (key, value) in evidence)
        {
            TryAddSafeEvidence(selected, key, value);
        }

        return selected;
    }

    private static bool TryAddSafeEvidence(Dictionary<string, object?> selected, string key, object? value)
    {
        if (selected.Count >= 64 ||
            selected.ContainsKey(key) ||
            key.Contains("path", StringComparison.OrdinalIgnoreCase) ||
            key.Contains("directory", StringComparison.OrdinalIgnoreCase) ||
            key.Contains("root", StringComparison.OrdinalIgnoreCase) ||
            key.Contains("payload", StringComparison.OrdinalIgnoreCase) ||
            value is string stringValue && LooksLikePathOrSecret(stringValue))
        {
            return false;
        }

        if (value is null or string or bool or int or long or double or decimal)
        {
            selected[key] = value;
            return true;
        }

        return false;
    }

    private static bool IsPrioritySafeEvidenceKey(string key) =>
        key.StartsWith("agentiCoreDuplex", StringComparison.Ordinal) ||
        key.StartsWith("duplex", StringComparison.Ordinal) ||
        key.StartsWith("appsSdk", StringComparison.Ordinal) ||
        key.StartsWith("codexPlugin", StringComparison.Ordinal) ||
        key.StartsWith("sharedMcp", StringComparison.Ordinal) ||
        key.StartsWith("chatGpt", StringComparison.Ordinal) ||
        key.StartsWith("phoneSeed", StringComparison.Ordinal) ||
        key.StartsWith("returnTelemetry", StringComparison.Ordinal) ||
        key.StartsWith("sliLisp", StringComparison.Ordinal) ||
        key.StartsWith("actualApprovalLeaseValidation", StringComparison.Ordinal) ||
        key.StartsWith("simultaneousUnmediated", StringComparison.Ordinal);

    private static bool LooksLikePathOrSecret(string value) =>
        value.Contains(@":\", StringComparison.Ordinal) ||
        value.Contains("\\Users\\", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("/Users/", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("-----BEGIN", StringComparison.OrdinalIgnoreCase);

    private static SeatedInvocation BuildSeatedInvocation(
        GptToolInvocationRequest invocation,
        string command,
        int benchRunCount,
        string sessionPrefix,
        string installRoot,
        string intakeRoot,
        string operatorName,
        string defaultCmeId,
        string defaultThreadBindingId,
        string defaultSoulFrameId,
        string defaultAgentiCoreId,
        string defaultIdentityTemplateId,
        string defaultSubjectCmeId,
        string serviceIdentityId,
        string defaultDomain,
        string defaultRole,
        string defaultJobClass)
    {
        var cmeId = ResolveEffectiveCallerCmeId(invocation.CmeId, defaultCmeId);
        if (string.IsNullOrWhiteSpace(cmeId))
        {
            return SeatedInvocation.Denied("", "cme-identity-required", 400, BuildCmeIdentityRequiredPayload());
        }

        var residentDefaultApplies = string.Equals(cmeId, defaultCmeId, StringComparison.Ordinal);
        var known = ResolveKnownCmeSeating(installRoot, cmeId);
        var context = ResolveInstallContextSeating(installRoot, cmeId);
        var threadBindingId = FirstNonEmpty(
            invocation.ThreadBindingId,
            known.ThreadBindingId,
            residentDefaultApplies ? defaultThreadBindingId : "");
        if (string.IsNullOrWhiteSpace(threadBindingId))
        {
            return SeatedInvocation.Denied(
                cmeId,
                "cme-thread-binding-required",
                400,
                BuildCmeThreadBindingRequiredPayload(cmeId));
        }

        var domain = FirstNonEmpty(
            invocation.Domain,
            context.Domain,
            residentDefaultApplies ? defaultDomain : "");
        var role = FirstNonEmpty(
            invocation.Role,
            context.Role,
            known.DomainRole,
            residentDefaultApplies ? defaultRole : "");
        var jobClass = FirstNonEmpty(
            invocation.JobClass,
            context.JobClass,
            residentDefaultApplies ? defaultJobClass : "");
        if (string.IsNullOrWhiteSpace(domain) ||
            string.IsNullOrWhiteSpace(role) ||
            string.IsNullOrWhiteSpace(jobClass))
        {
            return SeatedInvocation.Denied(
                cmeId,
                "governance-seating-incomplete",
                409,
                BuildGovernanceNotReadyPayload(
                    cmeId,
                    "governance-seating-incomplete",
                    "CME route was identified, but Steward/cGoA could not resolve a complete domain, role, and job scope before tool execution."));
        }

        var soulFrameId = FirstNonEmpty(
            invocation.SoulFrameId,
            known.SoulFrameId,
            residentDefaultApplies ? defaultSoulFrameId : "",
            $"{cmeId}.SoulFrame");
        var agentiCoreId = FirstNonEmpty(
            invocation.AgentiCoreId,
            known.AgentiCoreId,
            residentDefaultApplies ? defaultAgentiCoreId : "",
            $"{cmeId}.AgentiCore");
        var request = new SanctuaryRequest
        {
            Command = command,
            InstallRootPath = installRoot,
            IntakeRootPath = intakeRoot,
            OperatorName = operatorName,
            CmeId = cmeId,
            CmeIdentitySelected = true,
            ServiceIdentityId = serviceIdentityId,
            CallerCmeId = cmeId,
            ThreadBindingId = threadBindingId,
            IdentityTemplateId = FirstNonEmpty(invocation.IdentityTemplateId, context.IdentityTemplateId, defaultIdentityTemplateId),
            SoulFrameId = soulFrameId,
            AgentiCoreId = agentiCoreId,
            ParentCmeId = invocation.ParentCmeId,
            SubjectCmeId = FirstNonEmpty(invocation.SubjectCmeId, context.SubjectCmeId, residentDefaultApplies ? defaultSubjectCmeId : ""),
            SwarmId = invocation.SwarmId,
            SubAgentId = invocation.SubAgentId,
            ActualApprovalLeasePath = invocation.ActualApprovalLeasePath,
            Domain = domain,
            Role = role,
            JobClass = jobClass,
            SessionId = string.IsNullOrWhiteSpace(invocation.SessionId)
                ? $"{sessionPrefix}-{command}-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss-fffffff}"
                : invocation.SessionId,
            BenchRunCount = benchRunCount
        };

        return SeatedInvocation.FromRequest(request);
    }

    private static KnownCmeSeating ResolveKnownCmeSeating(string installRoot, string cmeId)
    {
        var binding = ReadCmeBindingSeating(installRoot, cmeId);
        var candidate = ReadIdentityCandidateSeating(installRoot, cmeId);
        return new KnownCmeSeating(
            FirstNonEmpty(binding.ThreadBindingId, candidate.ThreadBindingId),
            FirstNonEmpty(binding.DomainRole, candidate.DomainRole),
            FirstNonEmpty(binding.SoulFrameId, candidate.SoulFrameId),
            FirstNonEmpty(binding.AgentiCoreId, candidate.AgentiCoreId),
            FirstNonEmpty(binding.Source, candidate.Source));
    }

    private static KnownCmeSeating ReadCmeBindingSeating(string installRoot, string cmeId)
    {
        var bindingPath = Path.Combine(installRoot, "mos", "cme-bindings", $"{SafeSegment(cmeId)}.json");
        if (!File.Exists(bindingPath))
        {
            return KnownCmeSeating.Empty;
        }

        using var document = JsonDocument.Parse(File.ReadAllText(bindingPath, Encoding.UTF8));
        var root = document.RootElement;
        return new KnownCmeSeating(
            ReadString(root, "threadBindingId") ?? "",
            ReadString(root, "domainRole") ?? "",
            ReadString(root, "soulFrameId") ?? "",
            ReadString(root, "agentiCoreId") ?? "",
            "mos-cme-binding");
    }

    private static KnownCmeSeating ReadIdentityCandidateSeating(string installRoot, string cmeId)
    {
        var candidatePath = Path.Combine(installRoot, "mos", "identity-candidates.json");
        if (!File.Exists(candidatePath))
        {
            return KnownCmeSeating.Empty;
        }

        using var document = JsonDocument.Parse(File.ReadAllText(candidatePath, Encoding.UTF8));
        if (!document.RootElement.TryGetProperty("candidates", out var candidates) ||
            candidates.ValueKind != JsonValueKind.Array)
        {
            return KnownCmeSeating.Empty;
        }

        foreach (var candidate in candidates.EnumerateArray())
        {
            if (!string.Equals(ReadString(candidate, "cmeId"), cmeId, StringComparison.Ordinal))
            {
                continue;
            }

            return new KnownCmeSeating(
                ReadString(candidate, "lane") ?? "",
                ReadString(candidate, "domainRole") ?? "",
                ReadString(candidate, "soulFrameId") ?? "",
                ReadString(candidate, "agentiCoreId") ?? "",
                "mos-identity-candidates");
        }

        return KnownCmeSeating.Empty;
    }

    private static InstallContextSeating ResolveInstallContextSeating(string installRoot, string cmeId)
    {
        var contextPath = Path.Combine(installRoot, "mos", "lab-cme-context.json");
        if (!File.Exists(contextPath))
        {
            return InstallContextSeating.Empty;
        }

        using var document = JsonDocument.Parse(File.ReadAllText(contextPath, Encoding.UTF8));
        var root = document.RootElement;
        var identityTemplateId = ReadString(root, "identityTemplateId") ?? "";
        var labActorCmeId = ReadString(root, "labActorCmeId") ?? "";
        var telemetrySubjectCmeId = ReadString(root, "telemetrySubjectCmeId") ?? "";
        var labDomain = ResolveContextUniverseDomain(root, "lab");

        if (root.TryGetProperty("researchLanes", out var lanes) &&
            lanes.ValueKind == JsonValueKind.Array)
        {
            foreach (var lane in lanes.EnumerateArray())
            {
                if (!string.Equals(ReadString(lane, "cmeId"), cmeId, StringComparison.Ordinal))
                {
                    continue;
                }

                return new InstallContextSeating(
                    ReadString(lane, "domain") ?? "",
                    ReadString(lane, "role") ?? "",
                    ReadString(lane, "jobClass") ?? "",
                    "",
                    identityTemplateId,
                    ReadString(lane, "contextUniverseId") ?? "",
                    "install-lab-research-lane");
            }
        }

        if (string.Equals(cmeId, labActorCmeId, StringComparison.Ordinal))
        {
            return new InstallContextSeating(
                FirstNonEmpty(labDomain, "Project-Sanctuary.Lab"),
                "LabFacingCME",
                "GovernedToolExecution",
                telemetrySubjectCmeId,
                identityTemplateId,
                "lab",
                "install-lab-actor");
        }

        if (string.Equals(cmeId, telemetrySubjectCmeId, StringComparison.Ordinal))
        {
            return new InstallContextSeating(
                FirstNonEmpty(labDomain, "Project-Sanctuary.Lab"),
                "TelemetrySubject",
                "GovernedTelemetryReturn",
                labActorCmeId,
                identityTemplateId,
                "lab",
                "install-telemetry-subject");
        }

        return InstallContextSeating.Empty;
    }

    private static string ResolveContextUniverseDomain(JsonElement root, string contextUniverseId)
    {
        if (!root.TryGetProperty("contextUniverses", out var contexts) ||
            contexts.ValueKind != JsonValueKind.Array)
        {
            return "";
        }

        foreach (var context in contexts.EnumerateArray())
        {
            if (string.Equals(ReadString(context, "contextUniverseId"), contextUniverseId, StringComparison.Ordinal))
            {
                return ReadString(context, "domain") ?? "";
            }
        }

        return "";
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return "";
    }

    private static string SafeSegment(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            builder.Append(char.IsLetterOrDigit(character) || character is '.' or '_' or '-'
                ? character
                : '_');
        }

        return builder.Length == 0 ? "unnamed" : builder.ToString();
    }

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
            ThreadBindingId = ReadString(root, "threadBindingId") ?? ReadString(root, "thread_binding_id") ?? "",
            ParentCmeId = ReadString(root, "parentCmeId") ?? ReadString(root, "parent_cme_id") ?? "",
            SubjectCmeId = ReadString(root, "subjectCmeId") ?? ReadString(root, "subject_cme_id") ?? "",
            Domain = ReadString(root, "domain") ?? "",
            Role = ReadString(root, "role") ?? "",
            JobClass = ReadString(root, "jobClass") ?? ReadString(root, "job_class") ?? "",
            SwarmId = ReadString(root, "swarmId") ?? ReadString(root, "swarm_id") ?? "",
            SubAgentId = ReadString(root, "subAgentId") ?? ReadString(root, "sub_agent_id") ?? "",
            IdentityTemplateId = ReadString(root, "identityTemplateId") ?? ReadString(root, "identity_template_id") ?? "",
            SoulFrameId = ReadString(root, "soulFrameId") ?? ReadString(root, "soulframe_id") ?? "",
            AgentiCoreId = ReadString(root, "agentiCoreId") ?? ReadString(root, "agenticore_id") ?? "",
            ActualApprovalLeasePath = ReadString(root, "actualApprovalLeasePath") ?? ReadString(root, "actual_approval_lease_path") ?? "",
            BenchRunCount = ReadNullableInt(root, "benchRunCount") ?? ReadNullableInt(root, "bench_run_count")
        };
    }

    private static string ResolveEffectiveCallerCmeId(string invocationCmeId, string defaultCmeId) =>
        string.IsNullOrWhiteSpace(invocationCmeId) ? defaultCmeId : invocationCmeId;

    private static string ResolveEffectiveThreadBindingId(string invocationThreadBindingId, string defaultThreadBindingId) =>
        string.IsNullOrWhiteSpace(invocationThreadBindingId) ? defaultThreadBindingId : invocationThreadBindingId;

    private static string ResolveEffectiveSubjectCmeId(string invocationSubjectCmeId, string defaultSubjectCmeId) =>
        string.IsNullOrWhiteSpace(invocationSubjectCmeId) ? defaultSubjectCmeId : invocationSubjectCmeId;

    private static string ResolveEffectiveBodyId(string invocationBodyId, string defaultBodyId) =>
        string.IsNullOrWhiteSpace(invocationBodyId) ? defaultBodyId : invocationBodyId;

    private static object BuildCmeIdentityRequiredPayload() => new
    {
        schema = "project-sanctuary.cme-identity-required.v1",
        error = "cme-identity-required",
        message = "Select or provide a caller CME identity before Sanctuary writes receipts, GEL, OE, SelfGEL, or MoS residue.",
        acceptedPattern = "{Name}.CME.ID",
        serviceIdentityId = "Sanctuary.Actual.ID",
        serviceIdentityIsCme = false,
        silentDefaultAllowed = false,
        failClosed = true,
        providerCalled = false,
        modelBound = false,
        externalActionAuthorized = false,
        gelAdmitted = false,
        selfGelMutated = false
    };

    private static object BuildGovernanceNotReadyPayload(string cmeId, string error, string message) => new
    {
        schema = "project-sanctuary.governance-seating-not-ready.v1",
        error,
        message,
        cmeId,
        routeOnly = true,
        toolExecuted = false,
        receiptWritten = false,
        gelWritten = false,
        oeWritten = false,
        selfGelWritten = false,
        coeWritten = false,
        cSelfGelWritten = false,
        failClosed = true,
        providerCalled = false,
        modelBound = false,
        externalActionAuthorized = false,
        gelAdmitted = false,
        selfGelMutated = false,
        cmeActualActivated = false,
        sanctuaryActualActivated = false
    };

    private static object BuildCmeThreadBindingRequiredPayload(string cmeId) => new
    {
        schema = "project-sanctuary.cme-thread-binding-required.v1",
        error = "cme-thread-binding-required",
        message = "Provide the native CME thread binding id before Sanctuary writes receipts, GEL, OE, SelfGEL, cOE, cSelfGEL, or MoS residue.",
        cmeId,
        acceptedExamples = new[] { "codex-lab-thread", "oria-test-cme-thread" },
        failClosed = true,
        providerCalled = false,
        modelBound = false,
        externalActionAuthorized = false,
        gelAdmitted = false,
        selfGelMutated = false
    };

    private static object BuildCmeIdentityDeniedPayload(string cmeId, string reason) => new
    {
        schema = "project-sanctuary.cme-identity-denied.v1",
        error = "cme-identity-denied",
        cmeId,
        reason,
        receiptWritten = false,
        gelWritten = false,
        oeWritten = false,
        selfGelWritten = false,
        coeWritten = false,
        cSelfGelWritten = false,
        failClosed = true,
        providerCalled = false,
        modelBound = false,
        externalActionAuthorized = false,
        gelAdmitted = false,
        selfGelMutated = false
    };

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
        public string ThreadBindingId { get; init; } = "";
        public string ParentCmeId { get; init; } = "";
        public string SubjectCmeId { get; init; } = "";
        public string Domain { get; init; } = "";
        public string Role { get; init; } = "";
        public string JobClass { get; init; } = "";
        public string SwarmId { get; init; } = "";
        public string SubAgentId { get; init; } = "";
        public string IdentityTemplateId { get; init; } = "";
        public string SoulFrameId { get; init; } = "";
        public string AgentiCoreId { get; init; } = "";
        public string ActualApprovalLeasePath { get; init; } = "";
        public int? BenchRunCount { get; init; }
    }

    private sealed record SeatedInvocation(
        SanctuaryRequest? Request,
        string CmeId,
        string Error,
        object? Payload,
        int HttpStatus)
    {
        public bool Allowed => Request is not null;

        public static SeatedInvocation FromRequest(SanctuaryRequest request) =>
            new(request, request.CmeId, "", null, 200);

        public static SeatedInvocation Denied(string cmeId, string error, int httpStatus, object payload) =>
            new(null, cmeId, error, payload, httpStatus);
    }

    private sealed record KnownCmeSeating(
        string ThreadBindingId,
        string DomainRole,
        string SoulFrameId,
        string AgentiCoreId,
        string Source)
    {
        public static KnownCmeSeating Empty { get; } = new("", "", "", "", "");
    }

    private sealed record InstallContextSeating(
        string Domain,
        string Role,
        string JobClass,
        string SubjectCmeId,
        string IdentityTemplateId,
        string ContextUniverseId,
        string Source)
    {
        public static InstallContextSeating Empty { get; } = new("", "", "", "", "", "", "");
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
