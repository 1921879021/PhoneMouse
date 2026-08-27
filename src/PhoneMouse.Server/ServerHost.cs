using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using PhoneMouse.Core.Input;
using PhoneMouse.Core.Windows;
using PhoneMouse.Server.Notes;
using PhoneMouse.Server.Security;
using PhoneMouse.Server.Settings;
using PhoneMouse.Server.Web;

namespace PhoneMouse.Server;

public sealed class ServerHost : IAsyncDisposable
{
    private readonly IMouseController _mouseController;
    private readonly IKeyboardController _keyboardController;
    private readonly IForegroundWindowService _foregroundWindowService;
    private readonly VoiceNoteService _voiceNoteService;
    private readonly PairingService _pairingService;
    private readonly TrustedDeviceStore _trustedDeviceStore;
    private readonly ControlSettingsService _controlSettingsService;

    private readonly ConcurrentDictionary<Guid, ActiveConnection>
        _activeConnections = new();

    private WebApplication? _app;
    private int _connectedClientCount;


    public ServerHost(
        IMouseController mouseController,
        IKeyboardController keyboardController,
        IForegroundWindowService foregroundWindowService,
        VoiceNoteService voiceNoteService,
        PairingService pairingService,
        TrustedDeviceStore trustedDeviceStore,
        ControlSettingsService controlSettingsService)
    {
        _mouseController = mouseController;
        _keyboardController = keyboardController;
        _foregroundWindowService = foregroundWindowService;
        _voiceNoteService = voiceNoteService;
        _pairingService = pairingService;
        _trustedDeviceStore = trustedDeviceStore;
        _controlSettingsService = controlSettingsService;

        _trustedDeviceStore.DeviceRevoked +=
            TrustedDeviceStore_DeviceRevoked;

        _controlSettingsService.SettingsChanged +=
            ControlSettingsService_SettingsChanged;
    }


    public bool IsRunning =>
        _app is not null;


    public int ConnectedClientCount =>
        Volatile.Read(
            ref _connectedClientCount);


    public event EventHandler<int>?
        ConnectedClientCountChanged;


    public bool IsDeviceConnected(
        string deviceId)
    {
        return
            _activeConnections
                .Values
                .Any(
                    x =>
                        string.Equals(
                            x.DeviceId,
                            deviceId,
                            StringComparison.Ordinal));
    }


    public async Task StartAsync(
        CancellationToken cancellationToken =
            default)
    {
        if (_app is not null)
        {
            return;
        }

        WebApplicationBuilder builder =
            WebApplication.CreateBuilder();

        builder.WebHost.UseUrls(
            "http://0.0.0.0:9527");

        WebApplication app =
            builder.Build();

        app.UseWebSockets();

        app.MapGet(
            "/",
            (HttpContext context) =>
            {
                context.Response.Headers.CacheControl =
                    "no-store, no-cache, must-revalidate, max-age=0";

                context.Response.Headers.Pragma =
                    "no-cache";

                context.Response.Headers.Expires =
                    "0";

                return Results.Content(
                    TouchpadPage.Html,
                    "text/html; charset=utf-8");
            });

        app.MapPost(
            "/api/pair",
            (Func<HttpContext, Task<IResult>>)
                HandlePairAsync);

        app.Map(
            "/ws",
            HandleWebSocketAsync);

        _app =
            app;

        await app.StartAsync(
            cancellationToken);
    }


    private async Task<IResult> HandlePairAsync(
        HttpContext context)
    {
        PairRequest? request;

        try
        {
            request =
                await context.Request
                    .ReadFromJsonAsync<PairRequest>(
                        cancellationToken:
                            context.RequestAborted);
        }
        catch (JsonException)
        {
            return
                Results.BadRequest(
                    new
                    {
                        error =
                            "invalid_json"
                    });
        }

        if (
            request is null ||
            string.IsNullOrWhiteSpace(
                request.PairToken))
        {
            return
                Results.BadRequest(
                    new
                    {
                        error =
                            "missing_pair_token"
                    });
        }

        if (
            !_pairingService
                .TryConsumePairingToken(
                    request.PairToken))
        {
            return
                Results.Json(
                    new
                    {
                        error =
                            "pair_token_invalid_or_expired"
                    },
                    statusCode:
                        StatusCodes.Status401Unauthorized);
        }

        IssuedDeviceToken issued =
            _trustedDeviceStore
                .IssueDeviceToken(
                    request.DeviceName);

        return
            Results.Json(
                new
                {
                    paired =
                        true,

                    deviceId =
                        issued.DeviceId,

                    deviceToken =
                        issued.Token
                });
    }


    private async Task HandleWebSocketAsync(
        HttpContext context)
    {
        if (
            !context.WebSockets
                .IsWebSocketRequest)
        {
            context.Response.StatusCode =
                StatusCodes.Status400BadRequest;

            return;
        }

        using WebSocket socket =
            await context.WebSockets
                .AcceptWebSocketAsync();

        bool countedAsConnected =
            false;

        Guid connectionId =
            Guid.Empty;

        ActiveConnection? activeConnection =
            null;

        try
        {
            string? authenticationMessage =
                await ReceiveTextMessageAsync(
                    socket,
                    context.RequestAborted);

            string? deviceToken =
                null;

            string? deviceId =
                null;

            bool authenticated =
                authenticationMessage is not null &&
                TryReadAuthToken(
                    authenticationMessage,
                    out deviceToken) &&
                _trustedDeviceStore
                    .TryValidateAndTouch(
                        deviceToken,
                        out deviceId) &&
                !string.IsNullOrWhiteSpace(
                    deviceId);

            if (!authenticated)
            {
                await SendTextAsync(
                    socket,
                    """
                    {"type":"auth_failed"}
                    """,
                    CancellationToken.None);

                if (
                    socket.State ==
                        WebSocketState.Open)
                {
                    await socket.CloseAsync(
                        WebSocketCloseStatus.PolicyViolation,
                        "Authentication required",
                        CancellationToken.None);
                }

                return;
            }

            string authenticatedDeviceId =
                deviceId!;

            connectionId =
                Guid.NewGuid();

            activeConnection =
                new ActiveConnection(
                    authenticatedDeviceId,
                    socket);

            _activeConnections[
                connectionId] =
                    activeConnection;

            countedAsConnected =
                true;

            int count =
                Interlocked.Increment(
                    ref _connectedClientCount);

            ConnectedClientCountChanged?
                .Invoke(
                    this,
                    count);

            await SendConnectionTextAsync(
                activeConnection,
                """
                {"type":"connected","authenticated":true}
                """,
                context.RequestAborted);

            await SendSettingsAsync(
                activeConnection,
                _controlSettingsService
                    .GetSnapshot(),
                context.RequestAborted);

            while (
                socket.State ==
                    WebSocketState.Open &&
                !context.RequestAborted
                    .IsCancellationRequested)
            {
                string? message =
                    await ReceiveTextMessageAsync(
                        socket,
                        context.RequestAborted);

                if (message is null)
                {
                    break;
                }

                string? response =
                    HandleMessage(
                        message);


                if (
                    response is not null &&
                    activeConnection is not null)
                {
                    await SendConnectionTextAsync(
                        activeConnection,
                        response,
                        context.RequestAborted);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (WebSocketException)
        {
        }
        finally
        {
            if (
                connectionId !=
                    Guid.Empty)
            {
                _activeConnections.TryRemove(
                    connectionId,
                    out _);
            }

            if (countedAsConnected)
            {
                try
                {
                    _mouseController.LeftUp();
                }
                catch
                {
                }

                int count =
                    Interlocked.Decrement(
                        ref _connectedClientCount);

                if (count < 0)
                {
                    Interlocked.Exchange(
                        ref _connectedClientCount,
                        0);

                    count =
                        0;
                }

                ConnectedClientCountChanged?
                    .Invoke(
                        this,
                        count);
            }
        }
    }


    private void ControlSettingsService_SettingsChanged(
        object? sender,
        ControlSettingsSnapshot settings)
    {
        _ =
            BroadcastSettingsAsync(
                settings);
    }


    private async Task BroadcastSettingsAsync(
        ControlSettingsSnapshot settings)
    {
        ActiveConnection[] connections =
            _activeConnections
                .Values
                .ToArray();

        foreach (
            ActiveConnection connection
            in connections)
        {
            try
            {
                if (
                    connection.Socket.State ==
                        WebSocketState.Open)
                {
                    await SendSettingsAsync(
                        connection,
                        settings,
                        CancellationToken.None);
                }
            }
            catch
            {
                // 某台手机正在断线时，不影响其他设备。
            }
        }
    }


    private static Task SendSettingsAsync(
        ActiveConnection connection,
        ControlSettingsSnapshot settings,
        CancellationToken cancellationToken)
    {
        string json =
            JsonSerializer.Serialize(
                new
                {
                    type =
                        "settings",

                    mouseSensitivity =
                        settings.MouseSensitivity,

                    scrollSpeed =
                        settings.ScrollSpeed,

                    naturalScrolling =
                        settings.NaturalScrolling,

                    longPressMs =
                        settings.LongPressMs,

                    weChatSendMode =
                        settings.WeChatSendMode ==
                            1
                            ? "CtrlEnter"
                            : "Enter"
                });

        return
            SendConnectionTextAsync(
                connection,
                json,
                cancellationToken);
    }


    private static async Task SendConnectionTextAsync(
        ActiveConnection connection,
        string text,
        CancellationToken cancellationToken)
    {
        await connection.SendLock
            .WaitAsync(
                cancellationToken);

        try
        {
            if (
                connection.Socket.State !=
                    WebSocketState.Open)
            {
                return;
            }

            await SendTextAsync(
                connection.Socket,
                text,
                cancellationToken);
        }
        finally
        {
            connection.SendLock
                .Release();
        }
    }


    private void TrustedDeviceStore_DeviceRevoked(
        object? sender,
        string deviceId)
    {
        _ =
            DisconnectDeviceAsync(
                deviceId);
    }


    private async Task DisconnectDeviceAsync(
        string deviceId)
    {
        ActiveConnection[] connections =
            _activeConnections
                .Values
                .Where(
                    x =>
                        string.Equals(
                            x.DeviceId,
                            deviceId,
                            StringComparison.Ordinal))
                .ToArray();

        foreach (
            ActiveConnection connection
            in connections)
        {
            try
            {
                if (
                    connection.Socket.State ==
                        WebSocketState.Open)
                {
                    await connection.SendLock
                        .WaitAsync();

                    try
                    {
                        if (
                            connection.Socket.State ==
                                WebSocketState.Open)
                        {
                            await connection.Socket
                                .CloseAsync(
                                    WebSocketCloseStatus.PolicyViolation,
                                    "Device authorization revoked",
                                    CancellationToken.None);
                        }
                    }
                    finally
                    {
                        connection.SendLock
                            .Release();
                    }
                }
            }
            catch
            {
                try
                {
                    connection.Socket
                        .Abort();
                }
                catch
                {
                }
            }
        }
    }


    private static bool TryReadAuthToken(
        string message,
        out string? token)
    {
        token =
            null;

        try
        {
            using JsonDocument document =
                JsonDocument.Parse(
                    message);

            JsonElement root =
                document.RootElement;

            if (
                !root.TryGetProperty(
                    "type",
                    out JsonElement typeElement) ||
                typeElement.GetString() !=
                    "auth")
            {
                return false;
            }

            if (
                !root.TryGetProperty(
                    "token",
                    out JsonElement tokenElement))
            {
                return false;
            }

            token =
                tokenElement.GetString();

            return
                !string.IsNullOrWhiteSpace(
                    token);
        }
        catch (JsonException)
        {
            return false;
        }
    }


    private static async Task<string?>
        ReceiveTextMessageAsync(
            WebSocket socket,
            CancellationToken cancellationToken)
    {
        byte[] buffer =
            new byte[4096];

        using MemoryStream stream =
            new();

        while (true)
        {
            WebSocketReceiveResult result =
                await socket.ReceiveAsync(
                    new ArraySegment<byte>(
                        buffer),
                    cancellationToken);

            if (
                result.MessageType ==
                    WebSocketMessageType.Close)
            {
                if (
                    socket.State ==
                        WebSocketState.CloseReceived)
                {
                    await socket.CloseOutputAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Client closed",
                        CancellationToken.None);
                }

                return null;
            }

            if (
                result.MessageType !=
                    WebSocketMessageType.Text)
            {
                return null;
            }

            stream.Write(
                buffer,
                0,
                result.Count);

            if (
                stream.Length >
                    64 * 1024)
            {
                throw new WebSocketException(
                    "Message too large.");
            }

            if (result.EndOfMessage)
            {
                break;
            }
        }

        return
            Encoding.UTF8.GetString(
                stream.ToArray());
    }


    private string? HandleMessage(
        string message)
    {
        try
        {
            using JsonDocument document =
                JsonDocument.Parse(
                    message);

            JsonElement root =
                document.RootElement;

            if (
                !root.TryGetProperty(
                    "type",
                    out JsonElement typeElement))
            {
                return null;
            }

            string? type =
                typeElement.GetString();

            switch (type)
            {
                case "mouse_move":
                {
                    double dx =
                        root.GetProperty(
                                "dx")
                            .GetDouble();

                    double dy =
                        root.GetProperty(
                                "dy")
                            .GetDouble();

                    _mouseController.MoveRelative(
                        (int)Math.Round(dx),
                        (int)Math.Round(dy));

                    break;
                }

                case "mouse_click":
                {
                    string button =
                        root.TryGetProperty(
                                "button",
                                out JsonElement buttonElement)
                            ? buttonElement.GetString()
                                ?? "left"
                            : "left";

                    if (
                        button ==
                            "right")
                    {
                        _mouseController.RightClick();
                    }
                    else
                    {
                        _mouseController.LeftClick();
                    }

                    break;
                }

                case "mouse_double_click":
                {
                    _mouseController.DoubleClick();

                    break;
                }

                case "mouse_down":
                {
                    _mouseController.LeftDown();

                    break;
                }

                case "mouse_up":
                {
                    _mouseController.LeftUp();

                    break;
                }

                case "mouse_scroll":
                {
                    int delta =
                        root.GetProperty(
                                "dy")
                            .GetInt32();

                    _mouseController.Scroll(
                        delta);

                    break;
                }


                case "foreground_query":
                {
                    return
                        CreateForegroundStatusMessage();
                }


                case "text_save":
                {
                    string text =
                        ReadTextPayload(
                            root);


                    _voiceNoteService.Append(
                        text);


                    return JsonSerializer.Serialize(
                        new
                        {
                            type =
                                "action_result",

                            action =
                                "text_save",

                            success =
                                true,

                            message =
                                "已写入 VoiceNotes.txt"
                        });
                }


                case "text_type":
                {
                    string text =
                        ReadTextPayload(
                            root);


                    _keyboardController.TypeText(
                        text);


                    return JsonSerializer.Serialize(
                        new
                        {
                            type =
                                "action_result",

                            action =
                                "text_type",

                            success =
                                true,

                            message =
                                "已输入到电脑当前窗口"
                        });
                }


                case "text_send":
                {
                    string text =
                        ReadTextPayload(
                            root);


                    ForegroundWindowInfo foreground =
                        _foregroundWindowService
                            .GetForegroundWindow();


                    if (!foreground.IsWeChat)
                    {
                        return JsonSerializer.Serialize(
                            new
                            {
                                type =
                                    "action_result",

                                action =
                                    "text_send",

                                success =
                                    false,

                                message =
                                    "已阻止发送：电脑当前前台窗口不是微信。"
                            });
                    }


                    ControlSettingsSnapshot settings =
                        _controlSettingsService
                            .GetSnapshot();


                    if (
                        settings.WeChatSendMode ==
                            1)
                    {
                        _keyboardController
                            .TypeTextAndCtrlEnter(
                                text);
                    }
                    else
                    {
                        _keyboardController
                            .TypeTextAndEnter(
                                text);
                    }


                    return JsonSerializer.Serialize(
                        new
                        {
                            type =
                                "action_result",

                            action =
                                "text_send",

                            success =
                                true,

                            message =
                                settings.WeChatSendMode ==
                                    1
                                    ? "已输入并使用 Ctrl+Enter 发送"
                                    : "已输入并使用 Enter 发送"
                        });
                }
            }


            return null;
        }
        catch (JsonException)
        {
            return CreateActionError(
                "消息格式不正确");
        }
        catch (InvalidOperationException ex)
        {
            return CreateActionError(
                ex.Message);
        }
        catch (KeyNotFoundException)
        {
            return CreateActionError(
                "消息缺少必要字段");
        }
        catch (ArgumentException ex)
        {
            return CreateActionError(
                ex.Message);
        }
        catch (IOException ex)
        {
            return CreateActionError(
                $"写入 TXT 失败：{ex.Message}");
        }
    }


    private string CreateForegroundStatusMessage()
    {
        ForegroundWindowInfo foreground =
            _foregroundWindowService
                .GetForegroundWindow();


        ControlSettingsSnapshot settings =
            _controlSettingsService
                .GetSnapshot();


        return JsonSerializer.Serialize(
            new
            {
                type =
                    "foreground_status",

                isWeChat =
                    foreground.IsWeChat,

                processName =
                    foreground.ProcessName,

                windowTitle =
                    foreground.WindowTitle,

                weChatSendMode =
                    settings.WeChatSendMode ==
                        1
                        ? "CtrlEnter"
                        : "Enter"
            });
    }


    private static string ReadTextPayload(
        JsonElement root)
    {
        if (
            !root.TryGetProperty(
                "text",
                out JsonElement textElement))
        {
            throw new ArgumentException(
                "缺少文字内容。");
        }


        string text =
            textElement.GetString()
            ?? string.Empty;


        if (
            string.IsNullOrWhiteSpace(
                text))
        {
            throw new ArgumentException(
                "文字内容不能为空。");
        }


        if (
            text.Length >
                5000)
        {
            throw new ArgumentException(
                "单次最多发送 5000 个字符。");
        }


        return text;
    }


    private static string CreateActionError(
        string message)
    {
        return JsonSerializer.Serialize(
            new
            {
                type =
                    "action_result",

                success =
                    false,

                message =
                    message
            });
    }


    private static async Task SendTextAsync(
        WebSocket socket,
        string text,
        CancellationToken cancellationToken)
    {
        byte[] data =
            Encoding.UTF8.GetBytes(
                text);

        await socket.SendAsync(
            new ArraySegment<byte>(
                data),
            WebSocketMessageType.Text,
            true,
            cancellationToken);
    }


    public async ValueTask DisposeAsync()
    {
        _trustedDeviceStore.DeviceRevoked -=
            TrustedDeviceStore_DeviceRevoked;

        _controlSettingsService.SettingsChanged -=
            ControlSettingsService_SettingsChanged;

        foreach (
            ActiveConnection connection
            in _activeConnections.Values)
        {
            try
            {
                connection.Socket.Abort();
            }
            catch
            {
            }
        }

        _activeConnections.Clear();

        if (_app is null)
        {
            return;
        }

        await _app.StopAsync();

        await _app.DisposeAsync();

        _app =
            null;
    }


    private sealed record PairRequest(
        string? PairToken,
        string? DeviceName);


    private sealed class ActiveConnection
    {
        public ActiveConnection(
            string deviceId,
            WebSocket socket)
        {
            DeviceId =
                deviceId;

            Socket =
                socket;
        }


        public string DeviceId
        {
            get;
        }


        public WebSocket Socket
        {
            get;
        }


        public SemaphoreSlim SendLock
        {
            get;
        } =
            new(
                1,
                1);
    }
}
