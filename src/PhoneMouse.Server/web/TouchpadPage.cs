namespace PhoneMouse.Server.Web;

internal static class TouchpadPage
{
    internal static readonly string Html = """
<!DOCTYPE html>
<html lang="zh-CN">

<head>
    <meta charset="UTF-8">

    <meta
        name="viewport"
        content="width=device-width,
                 initial-scale=1.0,
                 maximum-scale=1.0,
                 user-scalable=no">

    <title>Phone Mouse</title>

    <style>
        * {
            box-sizing: border-box;

            -webkit-tap-highlight-color: transparent;

            -webkit-user-select: none !important;
            user-select: none !important;

            -webkit-touch-callout: none !important;
            -webkit-user-drag: none !important;
        }


        html,
        body {
            margin: 0;
            padding: 0;

            width: 100%;
            height: 100%;

            overflow: hidden;

            background: #ffffff;

            font-family:
                -apple-system,
                BlinkMacSystemFont,
                "Segoe UI",
                sans-serif;

            touch-action: none;

            overscroll-behavior: none;

            -webkit-user-select: none !important;
            user-select: none !important;

            -webkit-touch-callout: none !important;
        }


        #app {
            width: 100%;
            height: 100%;

            display: flex;
            flex-direction: column;

            overflow: hidden;
        }


        #header {
            height: 56px;

            flex-shrink: 0;

            display: flex;
            align-items: center;
            justify-content: space-between;

            padding: 0 18px;

            border-bottom:
                1px solid #eeeeee;

            background:
                #ffffff;
        }


        #title {
            font-size: 17px;
            font-weight: 650;

            color: #222222;

            pointer-events: none;
        }


        #status {
            font-size: 14px;

            pointer-events: none;
        }


        #status.connected {
            color: #159447;
        }


        #status.disconnected {
            color: #d93025;
        }


        #status.waiting {
            color: #b06000;
        }


        #touchpad {
            position: relative;

            flex: 1;

            width: 100%;

            overflow: hidden;

            background: #ffffff;

            touch-action: none !important;

            -webkit-user-select: none !important;
            user-select: none !important;

            -webkit-touch-callout: none !important;
            -webkit-user-drag: none !important;
        }


        #touchpad * {
            -webkit-user-select: none !important;
            user-select: none !important;

            -webkit-touch-callout: none !important;
            -webkit-user-drag: none !important;

            touch-action: none !important;
        }


        #hint {
            position: absolute;

            left: 50%;
            top: 50%;

            transform:
                translate(-50%, -50%);

            width: 90%;

            text-align: center;

            color: #b5b5b5;

            pointer-events: none;
        }


        #hint-main {
            font-size: 25px;
            font-weight: 500;

            margin-bottom: 14px;
        }


        #hint-sub {
            font-size: 14px;

            line-height: 1.9;
        }


        #auth-overlay {
            position: absolute;

            inset: 0;

            z-index: 20;

            display: flex;
            align-items: center;
            justify-content: center;

            padding: 28px;

            background:
                rgba(255,255,255,0.94);

            text-align: center;
        }


        #auth-overlay.hidden {
            display: none;
        }


        #auth-card {
            max-width: 360px;
        }


        #auth-title {
            font-size: 22px;
            font-weight: 650;

            color: #222222;
        }


        #auth-message {
            margin-top: 12px;

            color: #777777;

            font-size: 14px;

            line-height: 1.7;
        }


        #debug {
            position: absolute;

            left: 12px;
            bottom: 10px;

            font-size: 12px;

            color: #bdbdbd;

            pointer-events: none;
        }


        /* =========================================
           模式切换
           ========================================= */

        #mode-tabs {
            height: 50px;

            flex-shrink: 0;

            display: grid;

            grid-template-columns:
                1fr 1fr;

            padding: 7px 12px;

            gap: 8px;

            border-bottom:
                1px solid #eeeeee;

            background:
                #ffffff;
        }


        .mode-tab {
            border: 0;

            border-radius: 10px;

            background:
                #f1f3f4;

            color:
                #666666;

            font-size:
                14px;

            font-weight:
                600;

            touch-action:
                manipulation;
        }


        .mode-tab.active {
            background:
                #202124;

            color:
                #ffffff;
        }


        /* =========================================
           文字 / 语音输入
           ========================================= */

        #text-panel {
            flex: 1;

            overflow-y: auto;

            padding: 18px;

            background:
                #f6f7f8;

            touch-action:
                pan-y;
        }


        #text-panel.hidden,
        #touchpad.hidden,
        #mouse-buttons.hidden {
            display:
                none;
        }


        .text-card {
            max-width:
                760px;

            margin:
                0 auto;

            padding:
                18px;

            border:
                1px solid #e8e8e8;

            border-radius:
                14px;

            background:
                #ffffff;
        }


        .text-card-title {
            font-size:
                20px;

            font-weight:
                650;

            color:
                #222222;
        }


        .text-card-subtitle {
            margin-top:
                8px;

            color:
                #777777;

            font-size:
                13px;

            line-height:
                1.65;
        }


        #text-mouse-pad {
            position:
                relative;

            width:
                100%;

            height:
                190px;

            overflow:
                hidden;

            border:
                1px solid #dddddd;

            border-radius:
                14px;

            background:
                linear-gradient(
                    180deg,
                    #fafafa 0%,
                    #f3f4f5 100%);

            touch-action:
                none !important;

            -webkit-user-select:
                none !important;

            user-select:
                none !important;

            -webkit-touch-callout:
                none !important;

            -webkit-user-drag:
                none !important;
        }


        #text-mouse-pad::before {
            content:
                "";

            position:
                absolute;

            left:
                50%;

            top:
                50%;

            width:
                34px;

            height:
                34px;

            transform:
                translate(-50%, -50%);

            border:
                1px solid #d7d9dc;

            border-radius:
                50%;

            pointer-events:
                none;
        }


        #text-mouse-pad::after {
            content:
                "";

            position:
                absolute;

            left:
                50%;

            top:
                50%;

            width:
                6px;

            height:
                6px;

            transform:
                translate(-50%, -50%);

            border-radius:
                50%;

            background:
                #b7bbc0;

            pointer-events:
                none;
        }


        #text-mouse-pad.active {
            background:
                linear-gradient(
                    180deg,
                    #f5f6f7 0%,
                    #eceef0 100%);
        }


        #open-keyboard-button {
            width:
                100%;

            min-height:
                48px;

            margin-top:
                16px;

            border:
                1px solid #202124;

            border-radius:
                11px;

            background:
                #202124;

            color:
                #ffffff;

            font-size:
                15px;

            font-weight:
                650;

            touch-action:
                manipulation;

            -webkit-user-select:
                none !important;

            user-select:
                none !important;
        }


        #open-keyboard-button:disabled {
            opacity:
                0.45;
        }


        #text-input {
            width:
                100%;

            min-height:
                190px;

            margin-top:
                16px;

            padding:
                14px;

            resize:
                vertical;

            border:
                1px solid #d9d9d9;

            border-radius:
                12px;

            outline:
                none;

            background:
                #ffffff;

            color:
                #222222;

            font-family:
                inherit;

            font-size:
                16px;

            line-height:
                1.6;

            -webkit-user-select:
                text !important;

            user-select:
                text !important;

            -webkit-touch-callout:
                default !important;

            touch-action:
                manipulation !important;
        }


        #text-input:focus {
            border-color:
                #777777;
        }


        #text-meta {
            margin-top:
                8px;

            display:
                flex;

            justify-content:
                space-between;

            color:
                #999999;

            font-size:
                12px;
        }


        #foreground-status {
            margin-top:
                14px;

            padding:
                11px 12px;

            border:
                1px solid #e0e0e0;

            border-radius:
                10px;

            background:
                #f7f7f7;

            color:
                #666666;

            font-size:
                13px;

            line-height:
                1.5;
        }


        #foreground-status.wechat {
            border-color:
                #a9d9ba;

            background:
                #edf8f1;

            color:
                #24693d;
        }


        #foreground-status.blocked {
            border-color:
                #f0d3a2;

            background:
                #fff8eb;

            color:
                #9a6200;
        }


        #text-actions {
            margin-top:
                16px;

            display:
                grid;

            grid-template-columns:
                1fr;

            gap:
                10px;
        }


        .text-action-button {
            min-height:
                46px;

            border:
                1px solid #dddddd;

            border-radius:
                11px;

            background:
                #ffffff;

            color:
                #222222;

            font-size:
                15px;

            font-weight:
                600;

            touch-action:
                manipulation;
        }


        .text-action-button.primary {
            border-color:
                #202124;

            background:
                #202124;

            color:
                #ffffff;
        }


        .text-action-button.send {
            border-color:
                #2f7d4a;

            background:
                #edf8f1;

            color:
                #24693d;
        }


        .text-action-button:disabled {
            opacity:
                0.45;
        }


        #text-result {
            min-height:
                20px;

            margin-top:
                14px;

            color:
                #666666;

            font-size:
                13px;

            line-height:
                1.5;
        }


        #text-result.error {
            color:
                #c62828;
        }


        #text-result.success {
            color:
                #188038;
        }


        #text-clear-button {
            margin-top:
                12px;

            border:
                0;

            background:
                transparent;

            color:
                #777777;

            font-size:
                13px;

            text-decoration:
                underline;

            touch-action:
                manipulation;
        }


        #mouse-buttons {
            height: 78px;

            flex-shrink: 0;

            display: grid;

            grid-template-columns:
                1fr 1fr;

            border-top:
                1px solid #e5e5e5;

            background:
                #fafafa;
        }


        .mouse-button {
            border: 0;

            background: #fafafa;

            font-size: 18px;

            color: #222222;

            touch-action: manipulation;

            -webkit-user-select: none !important;
            user-select: none !important;

            -webkit-touch-callout: none !important;
        }


        .mouse-button:first-child {
            border-right:
                1px solid #e5e5e5;
        }


        .mouse-button:active {
            background:
                #e9e9e9;
        }


        .mouse-button:disabled {
            opacity: 0.45;
        }
    </style>
</head>


<body>

<div id="app">

    <div id="header">

        <div id="title">
            Phone Mouse · Alpha 0.7.2.3
        </div>

        <div
            id="status"
            class="waiting">
            ● 正在检查授权
        </div>

    </div>


    <div id="mode-tabs">

        <button
            id="touch-mode-button"
            class="mode-tab active"
            type="button">
            触控板
        </button>


        <button
            id="text-mode-button"
            class="mode-tab"
            type="button">
            语音 / 文字
        </button>

    </div>


    <div
        id="touchpad"
        unselectable="on">

        <div
            id="hint"
            aria-hidden="true">

            <div id="hint-main">
                触控板
            </div>

            <div id="hint-sub">

                单指滑动：移动鼠标
                <br>

                轻点：左键
                <br>

                连续轻点两次：双击
                <br>

                单指长按后移动：拖拽
                <br>

                双指上下滑：滚轮

            </div>

        </div>


        <div
            id="auth-overlay">

            <div id="auth-card">

                <div id="auth-title">
                    正在验证设备
                </div>

                <div id="auth-message">
                    请稍候…
                </div>

            </div>

        </div>


        <div id="debug">
            Ready
        </div>

    </div>


    <div
        id="text-panel"
        class="hidden">

        <div class="text-card">

            <div
                id="text-mouse-pad"
                aria-label="鼠标控制区">
            </div>


            <button
                id="open-keyboard-button"
                type="button"
                disabled>
                打开手机键盘 / 语音输入
            </button>


            <textarea
                id="text-input"
                maxlength="5000"
                inputmode="text"
                enterkeyhint="done"
                autocomplete="off"
                autocapitalize="sentences"
                spellcheck="true"
                placeholder="点这里输入文字，或点击上方按钮打开手机键盘后使用麦克风…"></textarea>


            <div id="text-meta">

                <span>
                    普通输入：电脑当前窗口必须先处于正确的输入位置
                </span>

                <span id="text-count">
                    0 / 5000
                </span>

            </div>


            <div
                id="foreground-status"
                class="blocked">
                正在检查电脑当前前台窗口…
            </div>


            <div id="text-actions">

                <button
                    id="save-text-button"
                    class="text-action-button"
                    type="button"
                    disabled>
                    写入 VoiceNotes.txt
                </button>


                <button
                    id="type-text-button"
                    class="text-action-button primary"
                    type="button"
                    disabled>
                    输入到电脑当前窗口
                </button>


                <button
                    id="send-text-button"
                    class="text-action-button send"
                    type="button"
                    disabled>
                    微信安全发送
                </button>

            </div>


            <div id="text-result">
                首次使用时：点输入框 → 手机键盘麦克风 → 说话 → 选择操作。
            </div>


            <button
                id="text-clear-button"
                type="button">
                清空文字
            </button>

        </div>

    </div>


    <div id="mouse-buttons">

        <button
            id="left-button"
            class="mouse-button"
            type="button"
            disabled>
            左键
        </button>


        <button
            id="right-button"
            class="mouse-button"
            type="button"
            disabled>
            右键
        </button>

    </div>

</div>


<script>
(() => {

    // =====================================================
    // DOM
    // =====================================================

    const touchpad =
        document.getElementById(
            "touchpad");


    const status =
        document.getElementById(
            "status");


    const debug =
        document.getElementById(
            "debug");


    const leftButton =
        document.getElementById(
            "left-button");


    const rightButton =
        document.getElementById(
            "right-button");


    const mouseButtonsElement =
        document.getElementById(
            "mouse-buttons");


    const authOverlay =
        document.getElementById(
            "auth-overlay");


    const authTitle =
        document.getElementById(
            "auth-title");


    const authMessage =
        document.getElementById(
            "auth-message");


    const touchModeButton =
        document.getElementById(
            "touch-mode-button");


    const textModeButton =
        document.getElementById(
            "text-mode-button");


    const textPanel =
        document.getElementById(
            "text-panel");


    const textInput =
        document.getElementById(
            "text-input");


    const textMousePad =
        document.getElementById(
            "text-mouse-pad");


    const openKeyboardButton =
        document.getElementById(
            "open-keyboard-button");


    const textCount =
        document.getElementById(
            "text-count");


    const saveTextButton =
        document.getElementById(
            "save-text-button");


    const typeTextButton =
        document.getElementById(
            "type-text-button");


    const sendTextButton =
        document.getElementById(
            "send-text-button");


    const textResult =
        document.getElementById(
            "text-result");


    const foregroundStatus =
        document.getElementById(
            "foreground-status");


    const textClearButton =
        document.getElementById(
            "text-clear-button");


    // =====================================================
    // 安全认证状态
    // =====================================================

    const TOKEN_STORAGE_KEY =
        "phonemouse.deviceToken.v1";


    let deviceToken =
        localStorage.getItem(
            TOKEN_STORAGE_KEY);


    let controlEnabled =
        false;


    let currentForegroundIsWeChat =
        false;


    let currentWeChatSendMode =
        "Enter";


    let foregroundWatchTimer =
        null;


    let authenticationRejected =
        false;


    function setLocked(
        title,
        message)
    {
        controlEnabled =
            false;


        authTitle.textContent =
            title;


        authMessage.textContent =
            message;


        authOverlay.classList
            .remove(
                "hidden");


        leftButton.disabled =
            true;


        rightButton.disabled =
            true;


        openKeyboardButton.disabled =
            true;


        saveTextButton.disabled =
            true;


        typeTextButton.disabled =
            true;


        sendTextButton.disabled =
            true;
    }


    function setUnlocked()
    {
        controlEnabled =
            true;


        authOverlay.classList
            .add(
                "hidden");


        leftButton.disabled =
            false;


        rightButton.disabled =
            false;


        openKeyboardButton.disabled =
            false;


        saveTextButton.disabled =
            false;


        typeTextButton.disabled =
            false;


        sendTextButton.disabled =
            !currentForegroundIsWeChat;
    }


    function getDeviceName()
    {
        const userAgent =
            navigator.userAgent ||
            "";


        if (
            /iPhone/i.test(
                userAgent))
        {
            return "iPhone";
        }


        if (
            /iPad/i.test(
                userAgent))
        {
            return "iPad";
        }


        if (
            /Android/i.test(
                userAgent))
        {
            const modelMatch =
                userAgent.match(
                    /Android[^;]*;\s*([^;)]+?)(?:\s+Build\/|;|\))/i);


            if (
                modelMatch &&
                modelMatch[1])
            {
                return `Android · ${modelMatch[1].trim()}`;
            }


            return "Android Phone";
        }


        const platform =
            navigator.userAgentData
                ?.platform ||
            navigator.platform ||
            "Mobile";


        return `${platform} Browser`;
    }


    // =====================================================
    // 首次扫码配对
    // =====================================================

    async function tryPairFromUrl()
    {
        const parameters =
            new URLSearchParams(
                location.search);


        const pairToken =
            parameters.get(
                "pair");


        if (!pairToken)
        {
            return false;
        }


        status.textContent =
            "● 正在配对";


        status.className =
            "waiting";


        setLocked(
            "正在安全配对",
            "正在验证电脑上的一次性配对码…");


        try
        {
            const response =
                await fetch(
                    "/api/pair",
                    {
                        method:
                            "POST",

                        headers:
                        {
                            "Content-Type":
                                "application/json"
                        },

                        body:
                            JSON.stringify(
                                {
                                    pairToken:
                                        pairToken,

                                    deviceName:
                                        getDeviceName()
                                })
                    });


            if (!response.ok)
            {
                deviceToken =
                    null;


                localStorage
                    .removeItem(
                        TOKEN_STORAGE_KEY);


                status.textContent =
                    "● 配对失败";


                status.className =
                    "disconnected";


                setLocked(
                    "配对码已失效",
                    "这个二维码已经被使用或已经过期。请重新扫描电脑当前显示的二维码。");


                return true;
            }


            const data =
                await response.json();


            deviceToken =
                data.deviceToken;


            localStorage
                .setItem(
                    TOKEN_STORAGE_KEY,
                    deviceToken);


            // 配对成功后，从地址栏移除一次性 Token。
            history.replaceState(
                {},
                "",
                location.pathname);


            status.textContent =
                "● 正在认证";


            status.className =
                "waiting";


            setLocked(
                "配对成功",
                "正在建立安全控制连接…");


            return true;
        }
        catch
        {
            status.textContent =
                "● 配对异常";


            status.className =
                "disconnected";


            setLocked(
                "无法完成配对",
                "请确认手机与电脑仍在同一 Wi-Fi，并重新扫描二维码。");


            return true;
        }
    }


    // =====================================================
    // 控制体验设置
    // =====================================================

    let mouseSensitivity =
        1.0;


    let scrollSpeed =
        1.0;


    let naturalScrolling =
        false;


    let longPressMs =
        420;


    function applySettings(
        settings)
    {
        if (!settings)
        {
            return;
        }


        if (
            Number.isFinite(
                settings.mouseSensitivity))
        {
            mouseSensitivity =
                Math.min(
                    3.0,
                    Math.max(
                        0.5,
                        settings.mouseSensitivity));
        }


        if (
            Number.isFinite(
                settings.scrollSpeed))
        {
            scrollSpeed =
                Math.min(
                    3.0,
                    Math.max(
                        0.5,
                        settings.scrollSpeed));
        }


        naturalScrolling =
            settings.naturalScrolling ===
                true;


        if (
            settings.weChatSendMode ===
                "CtrlEnter" ||
            settings.weChatSendMode ===
                "Enter")
        {
            currentWeChatSendMode =
                settings.weChatSendMode;


            updateWeChatSendButton();
        }


        if (
            Number.isFinite(
                settings.longPressMs))
        {
            longPressMs =
                Math.min(
                    900,
                    Math.max(
                        250,
                        Math.round(
                            settings.longPressMs)));
        }


        debug.textContent =
            `Settings · Mouse ${mouseSensitivity.toFixed(1)}× · Scroll ${scrollSpeed.toFixed(1)}×`;
    }


    // =====================================================
    // WebSocket
    // =====================================================

    let socket =
        null;


    let reconnectTimer =
        null;


    function connect()
    {
        if (!deviceToken)
        {
            status.textContent =
                "● 尚未配对";


            status.className =
                "waiting";


            setLocked(
                "此设备尚未配对",
                "请扫描电脑 Phone Mouse 窗口中的一次性二维码完成首次授权。");


            return;
        }


        if (
            socket &&
            (
                socket.readyState ===
                    WebSocket.OPEN ||
                socket.readyState ===
                    WebSocket.CONNECTING
            )
        )
        {
            return;
        }


        authenticationRejected =
            false;


        const protocol =
            location.protocol ===
                "https:"
                ? "wss:"
                : "ws:";


        const url =
            `${protocol}//${location.host}/ws`;


        socket =
            new WebSocket(
                url);


        status.textContent =
            "● 正在连接";


        status.className =
            "waiting";


        socket.addEventListener(
            "open",
            () =>
            {
                // 第一条 WebSocket 消息只做认证。
                socket.send(
                    JSON.stringify(
                        {
                            type:
                                "auth",

                            token:
                                deviceToken
                        }));
            });


        socket.addEventListener(
            "message",
            event =>
            {
                let message;


                try
                {
                    message =
                        JSON.parse(
                            event.data);
                }
                catch
                {
                    return;
                }


                if (
                    message.type ===
                        "foreground_status")
                {
                    updateForegroundStatus(
                        message);


                    return;
                }


                if (
                    message.type ===
                        "action_result")
                {
                    textResult.textContent =
                        message.message ||
                        (
                            message.success
                                ? "操作已完成"
                                : "操作失败"
                        );


                    textResult.className =
                        message.success
                            ? "success"
                            : "error";


                    if (
                        message.success ===
                            true &&
                        (
                            message.action ===
                                "text_save" ||
                            message.action ===
                                "text_type" ||
                            message.action ===
                                "text_send"
                        )
                    )
                    {
                        textInput.value =
                            "";


                        updateTextCount();
                    }


                    return;
                }


                if (
                    message.type ===
                        "settings")
                {
                    applySettings(
                        message);

                    return;
                }


                if (
                    message.type ===
                        "connected" &&
                    message.authenticated ===
                        true)
                {
                    status.textContent =
                        "● 已认证";


                    status.className =
                        "connected";


                    debug.textContent =
                        "Authenticated";


                    setUnlocked();


                    if (
                        !textPanel.classList
                            .contains(
                                "hidden"))
                    {
                        startForegroundWatch();
                    }


                    return;
                }


                if (
                    message.type ===
                        "auth_failed")
                {
                    authenticationRejected =
                        true;


                    deviceToken =
                        null;


                    localStorage
                        .removeItem(
                            TOKEN_STORAGE_KEY);


                    status.textContent =
                        "● 授权失效";


                    status.className =
                        "disconnected";


                    setLocked(
                        "需要重新配对",
                        "此设备的授权已经失效。请重新扫描电脑当前二维码。");


                    try
                    {
                        socket.close();
                    }
                    catch {}
                }
            });


        socket.addEventListener(
            "close",
            () =>
            {
                controlEnabled =
                    false;


                stopForegroundWatch();


                currentForegroundIsWeChat =
                    false;


                updateWeChatSendButton();


                leftButton.disabled =
                    true;


                rightButton.disabled =
                    true;


                openKeyboardButton.disabled =
                    true;


                saveTextButton.disabled =
                    true;


                typeTextButton.disabled =
                    true;


                sendTextButton.disabled =
                    true;


                if (
                    authenticationRejected ||
                    !deviceToken)
                {
                    return;
                }


                status.textContent =
                    "● 已断开";


                status.className =
                    "disconnected";


                setLocked(
                    "连接已断开",
                    "正在尝试重新连接电脑…");


                if (reconnectTimer)
                {
                    clearTimeout(
                        reconnectTimer);
                }


                reconnectTimer =
                    setTimeout(
                        connect,
                        1500);
            });


        socket.addEventListener(
            "error",
            () =>
            {
                status.textContent =
                    "● 连接异常";


                status.className =
                    "disconnected";
            });
    }


    function send(message)
    {
        if (
            !controlEnabled ||
            !socket ||
            socket.readyState !==
                WebSocket.OPEN
        )
        {
            return;
        }


        socket.send(
            JSON.stringify(
                message));
    }


    // =====================================================
    // 禁止文字选择
    // =====================================================

    function isTextEditingActive()
    {
        const active =
            document.activeElement;


        return (
            active === textInput ||
            active?.tagName ===
                "INPUT" ||
            active?.tagName ===
                "TEXTAREA" ||
            active?.isContentEditable ===
                true
        );
    }


    function clearSelection()
    {
        // 触控板需要强制清除文字选择，避免 Android 长按菜单。
        // 但在输入框编辑时绝不能清除 selection：
        // 某些 Android / iOS 浏览器会因此立即关闭软键盘。
        if (
            isTextEditingActive()
        )
        {
            return;
        }


        const selection =
            window.getSelection();


        if (
            selection &&
            selection.rangeCount > 0
        )
        {
            selection.removeAllRanges();
        }
    }


    // =====================================================
    // 鼠标移动队列
    // =====================================================

    let pendingDx = 0;

    let pendingDy = 0;

    let moveFramePending =
        false;


    function queueMove(
        dx,
        dy)
    {
        pendingDx +=
            dx *
            mouseSensitivity;


        pendingDy +=
            dy *
            mouseSensitivity;


        if (!moveFramePending)
        {
            moveFramePending =
                true;


            requestAnimationFrame(
                flushMove);
        }
    }


    function flushMove()
    {
        moveFramePending =
            false;


        const dx =
            Math.round(
                pendingDx);


        const dy =
            Math.round(
                pendingDy);


        pendingDx =
            0;


        pendingDy =
            0;


        if (
            dx === 0 &&
            dy === 0
        )
        {
            return;
        }


        send(
            {
                type:
                    "mouse_move",

                dx:
                    dx,

                dy:
                    dy
            });


        if (dragging)
        {
            debug.textContent =
                `Dragging ${dx}, ${dy}`;
        }
        else
        {
            debug.textContent =
                `Move ${dx}, ${dy}`;
        }
    }


    // =====================================================
    // Pointer 状态
    // =====================================================

    const pointers =
        new Map();


    let primaryPointerId =
        null;


    let startX = 0;

    let startY = 0;


    let lastX = 0;

    let lastY = 0;


    // =====================================================
    // 拖拽状态
    // =====================================================

    let dragging =
        false;


    let longPressTimer =
        null;


    let movedGesture =
        false;


    // =====================================================
    // 双指滚轮
    // =====================================================

    let multiTouchGesture =
        false;


    let lastScrollCenterY =
        null;


    // =====================================================
    // 手势参数
    // =====================================================

    // 长按时间由电脑端设置动态下发。


    const MOVE_CANCEL_DISTANCE =
        18;


    const TAP_MAX_DISTANCE =
        10;


    // =====================================================
    // 长按
    // =====================================================

    function cancelLongPress()
    {
        if (
            longPressTimer !==
                null
        )
        {
            clearTimeout(
                longPressTimer);


            longPressTimer =
                null;
        }
    }


    function scheduleLongPress()
    {
        cancelLongPress();


        longPressTimer =
            setTimeout(
                () =>
                {
                    longPressTimer =
                        null;


                    if (
                        !controlEnabled ||
                        pointers.size !== 1 ||
                        primaryPointerId ===
                            null
                    )
                    {
                        return;
                    }


                    const pointer =
                        pointers.get(
                            primaryPointerId);


                    if (!pointer)
                    {
                        return;
                    }


                    const distance =
                        Math.hypot(
                            pointer.x -
                                startX,

                            pointer.y -
                                startY);


                    if (
                        distance >
                            MOVE_CANCEL_DISTANCE
                    )
                    {
                        return;
                    }


                    clearSelection();


                    dragging =
                        true;


                    send(
                        {
                            type:
                                "mouse_down",

                            button:
                                "left"
                        });


                    debug.textContent =
                        "Dragging";
                },
                longPressMs);
    }


    // =====================================================
    // Pointer Down
    // =====================================================

    touchpad.addEventListener(
        "pointerdown",
        e =>
        {
            if (!controlEnabled)
            {
                e.preventDefault();

                return;
            }


            clearSelection();


            pointers.set(
                e.pointerId,
                {
                    x:
                        e.clientX,

                    y:
                        e.clientY
                });


            try
            {
                touchpad
                    .setPointerCapture(
                        e.pointerId);
            }
            catch {}


            if (
                pointers.size ===
                    1
            )
            {
                primaryPointerId =
                    e.pointerId;


                multiTouchGesture =
                    false;


                dragging =
                    false;


                movedGesture =
                    false;


                startX =
                    e.clientX;


                startY =
                    e.clientY;


                lastX =
                    e.clientX;


                lastY =
                    e.clientY;


                scheduleLongPress();


                debug.textContent =
                    "Pointer Down";
            }
            else if (
                pointers.size ===
                    2
            )
            {
                multiTouchGesture =
                    true;


                cancelLongPress();


                if (dragging)
                {
                    send(
                        {
                            type:
                                "mouse_up",

                            button:
                                "left"
                        });


                    dragging =
                        false;
                }


                const values =
                    [...pointers.values()];


                lastScrollCenterY =
                    (
                        values[0].y +
                        values[1].y
                    ) / 2;


                debug.textContent =
                    "Scroll Mode";
            }


            e.preventDefault();
        },
        {
            passive:
                false
        });


    // =====================================================
    // Pointer Move
    // =====================================================

    touchpad.addEventListener(
        "pointermove",
        e =>
        {
            if (
                !controlEnabled ||
                !pointers.has(
                    e.pointerId)
            )
            {
                return;
            }


            pointers.set(
                e.pointerId,
                {
                    x:
                        e.clientX,

                    y:
                        e.clientY
                });


            if (
                pointers.size >=
                    2
            )
            {
                multiTouchGesture =
                    true;


                cancelLongPress();


                const values =
                    [...pointers.values()];


                const centerY =
                    (
                        values[0].y +
                        values[1].y
                    ) / 2;


                if (
                    lastScrollCenterY !==
                        null
                )
                {
                    const dy =
                        centerY -
                        lastScrollCenterY;


                    const direction =
                        naturalScrolling
                            ? 1
                            : -1;


                    const scrollDelta =
                        Math.round(
                            dy *
                            8 *
                            scrollSpeed *
                            direction);


                    if (
                        scrollDelta !==
                            0
                    )
                    {
                        send(
                            {
                                type:
                                    "mouse_scroll",

                                dy:
                                    scrollDelta
                            });


                        debug.textContent =
                            `Scroll ${scrollDelta}`;
                    }
                }


                lastScrollCenterY =
                    centerY;


                e.preventDefault();

                return;
            }


            if (
                multiTouchGesture
            )
            {
                e.preventDefault();

                return;
            }


            if (
                e.pointerId !==
                    primaryPointerId
            )
            {
                return;
            }


            const dx =
                e.clientX -
                    lastX;


            const dy =
                e.clientY -
                    lastY;


            lastX =
                e.clientX;


            lastY =
                e.clientY;


            const totalDistance =
                Math.hypot(
                    e.clientX -
                        startX,

                    e.clientY -
                        startY);


            if (
                totalDistance >
                    TAP_MAX_DISTANCE
            )
            {
                movedGesture =
                    true;
            }


            if (
                !dragging &&
                totalDistance >
                    MOVE_CANCEL_DISTANCE
            )
            {
                cancelLongPress();
            }


            queueMove(
                dx,
                dy);


            clearSelection();


            e.preventDefault();
        },
        {
            passive:
                false
        });


    // =====================================================
    // Pointer Up / Cancel
    // =====================================================

    function releasePointer(
        e)
    {
        if (
            !pointers.has(
                e.pointerId)
        )
        {
            return;
        }


        const wasPrimary =
            e.pointerId ===
                primaryPointerId;


        const wasDragging =
            dragging;


        const finalX =
            e.clientX;


        const finalY =
            e.clientY;


        pointers.delete(
            e.pointerId);


        try
        {
            touchpad
                .releasePointerCapture(
                    e.pointerId);
        }
        catch {}


        if (
            multiTouchGesture
        )
        {
            cancelLongPress();


            if (wasDragging)
            {
                send(
                    {
                        type:
                            "mouse_up",

                        button:
                            "left"
                    });


                dragging =
                    false;
            }


            lastScrollCenterY =
                null;


            if (
                pointers.size ===
                    0
            )
            {
                multiTouchGesture =
                    false;


                primaryPointerId =
                    null;


                movedGesture =
                    false;


                debug.textContent =
                    "Scroll End";
            }


            clearSelection();


            e.preventDefault();

            return;
        }


        if (wasPrimary)
        {
            cancelLongPress();


            const totalDistance =
                Math.hypot(
                    finalX -
                        startX,

                    finalY -
                        startY);


            if (wasDragging)
            {
                send(
                    {
                        type:
                            "mouse_up",

                        button:
                            "left"
                    });


                dragging =
                    false;


                debug.textContent =
                    "Drag Released";
            }
            else if (
                !movedGesture &&
                totalDistance <=
                    TAP_MAX_DISTANCE
            )
            {
                send(
                    {
                        type:
                            "mouse_click",

                        button:
                            "left"
                    });


                debug.textContent =
                    "Left Click";
            }


            dragging =
                false;


            movedGesture =
                false;


            primaryPointerId =
                null;
        }


        clearSelection();


        e.preventDefault();
    }


    touchpad.addEventListener(
        "pointerup",
        releasePointer,
        {
            passive:
                false
        });


    touchpad.addEventListener(
        "pointercancel",
        releasePointer,
        {
            passive:
                false
        });


    // =====================================================
    // 左 / 右键按钮
    // =====================================================

    leftButton.addEventListener(
        "click",
        () =>
        {
            send(
                {
                    type:
                        "mouse_click",

                    button:
                        "left"
                });


            debug.textContent =
                "Left Button";
        });


    rightButton.addEventListener(
        "click",
        () =>
        {
            send(
                {
                    type:
                        "mouse_click",

                    button:
                        "right"
                });


            debug.textContent =
                "Right Button";
        });


    // =====================================================
    // 阻止手机浏览器默认行为
    // =====================================================

    touchpad.addEventListener(
        "contextmenu",
        e =>
        {
            clearSelection();

            e.preventDefault();

            e.stopPropagation();

            return false;
        },
        true);


    touchpad.addEventListener(
        "selectstart",
        e =>
        {
            clearSelection();

            e.preventDefault();

            e.stopPropagation();

            return false;
        },
        true);


    touchpad.addEventListener(
        "dragstart",
        e =>
        {
            e.preventDefault();

            e.stopPropagation();

            return false;
        },
        true);


    document.addEventListener(
        "selectionchange",
        () =>
        {
            if (
                isTextEditingActive() ||
                !textPanel.classList
                    .contains(
                        "hidden")
            )
            {
                return;
            }


            clearSelection();
        });


    touchpad.addEventListener(
        "touchstart",
        e =>
        {
            clearSelection();

            e.preventDefault();
        },
        {
            passive:
                false
        });


    touchpad.addEventListener(
        "touchmove",
        e =>
        {
            clearSelection();

            e.preventDefault();
        },
        {
            passive:
                false
        });


    touchpad.addEventListener(
        "touchend",
        e =>
        {
            clearSelection();

            e.preventDefault();
        },
        {
            passive:
                false
        });


    // =====================================================
    // 语音 / 文字页内嵌鼠标控制区
    // =====================================================

    const textPadPointers =
        new Map();


    let textPadPrimaryPointerId =
        null;


    let textPadStartX =
        0;


    let textPadStartY =
        0;


    let textPadLastX =
        0;


    let textPadLastY =
        0;


    let textPadDragging =
        false;


    let textPadMoved =
        false;


    let textPadLongPressTimer =
        null;


    let textPadMultiTouch =
        false;


    let textPadLastScrollCenterY =
        null;


    function cancelTextPadLongPress()
    {
        if (
            textPadLongPressTimer !==
                null)
        {
            clearTimeout(
                textPadLongPressTimer);


            textPadLongPressTimer =
                null;
        }
    }


    function scheduleTextPadLongPress()
    {
        cancelTextPadLongPress();


        textPadLongPressTimer =
            setTimeout(
                () =>
                {
                    textPadLongPressTimer =
                        null;


                    if (
                        !controlEnabled ||
                        textPadPointers.size !==
                            1 ||
                        textPadPrimaryPointerId ===
                            null
                    )
                    {
                        return;
                    }


                    const pointer =
                        textPadPointers.get(
                            textPadPrimaryPointerId);


                    if (!pointer)
                    {
                        return;
                    }


                    const distance =
                        Math.hypot(
                            pointer.x -
                                textPadStartX,
                            pointer.y -
                                textPadStartY);


                    if (
                        distance >
                            MOVE_CANCEL_DISTANCE)
                    {
                        return;
                    }


                    textPadDragging =
                        true;


                    textMousePad.classList
                        .add(
                            "active");


                    send(
                        {
                            type:
                                "mouse_down",

                            button:
                                "left"
                        });
                },
                longPressMs);
    }


    function resetTextMousePad(
        releaseMouseButton)
    {
        cancelTextPadLongPress();


        if (
            releaseMouseButton &&
            textPadDragging)
        {
            send(
                {
                    type:
                        "mouse_up",

                    button:
                        "left"
                });
        }


        textPadDragging =
            false;


        textPadMoved =
            false;


        textPadMultiTouch =
            false;


        textPadLastScrollCenterY =
            null;


        textPadPrimaryPointerId =
            null;


        textPadPointers.clear();


        textMousePad.classList
            .remove(
                "active");
    }


    textMousePad.addEventListener(
        "pointerdown",
        e =>
        {
            if (!controlEnabled)
            {
                e.preventDefault();

                return;
            }


            // 如果手机键盘当前打开，触摸鼠标区时先结束文本编辑，
            // 避免软键盘遮挡控制区域。
            if (
                document.activeElement ===
                    textInput)
            {
                textInput.blur();
            }


            clearSelection();


            textPadPointers.set(
                e.pointerId,
                {
                    x:
                        e.clientX,

                    y:
                        e.clientY
                });


            try
            {
                textMousePad
                    .setPointerCapture(
                        e.pointerId);
            }
            catch {}


            if (
                textPadPointers.size ===
                    1)
            {
                textPadPrimaryPointerId =
                    e.pointerId;


                textPadStartX =
                    e.clientX;


                textPadStartY =
                    e.clientY;


                textPadLastX =
                    e.clientX;


                textPadLastY =
                    e.clientY;


                textPadMoved =
                    false;


                textPadMultiTouch =
                    false;


                textPadDragging =
                    false;


                scheduleTextPadLongPress();
            }
            else if (
                textPadPointers.size ===
                    2)
            {
                textPadMultiTouch =
                    true;


                cancelTextPadLongPress();


                if (textPadDragging)
                {
                    send(
                        {
                            type:
                                "mouse_up",

                            button:
                                "left"
                        });


                    textPadDragging =
                        false;
                }


                const values =
                    [...textPadPointers.values()];


                textPadLastScrollCenterY =
                    (
                        values[0].y +
                        values[1].y
                    ) / 2;
            }


            textMousePad.classList
                .add(
                    "active");


            e.preventDefault();
        },
        {
            passive:
                false
        });


    textMousePad.addEventListener(
        "pointermove",
        e =>
        {
            if (
                !controlEnabled ||
                !textPadPointers.has(
                    e.pointerId)
            )
            {
                return;
            }


            textPadPointers.set(
                e.pointerId,
                {
                    x:
                        e.clientX,

                    y:
                        e.clientY
                });


            if (
                textPadPointers.size >=
                    2)
            {
                textPadMultiTouch =
                    true;


                cancelTextPadLongPress();


                const values =
                    [...textPadPointers.values()];


                const centerY =
                    (
                        values[0].y +
                        values[1].y
                    ) / 2;


                if (
                    textPadLastScrollCenterY !==
                        null)
                {
                    const dy =
                        centerY -
                        textPadLastScrollCenterY;


                    const direction =
                        naturalScrolling
                            ? 1
                            : -1;


                    const scrollDelta =
                        Math.round(
                            dy *
                            8 *
                            scrollSpeed *
                            direction);


                    if (
                        scrollDelta !==
                            0)
                    {
                        send(
                            {
                                type:
                                    "mouse_scroll",

                                dy:
                                    scrollDelta
                            });
                    }
                }


                textPadLastScrollCenterY =
                    centerY;


                e.preventDefault();

                return;
            }


            if (
                textPadMultiTouch ||
                e.pointerId !==
                    textPadPrimaryPointerId)
            {
                e.preventDefault();

                return;
            }


            const dx =
                e.clientX -
                textPadLastX;


            const dy =
                e.clientY -
                textPadLastY;


            textPadLastX =
                e.clientX;


            textPadLastY =
                e.clientY;


            const totalDistance =
                Math.hypot(
                    e.clientX -
                        textPadStartX,
                    e.clientY -
                        textPadStartY);


            if (
                totalDistance >
                    TAP_MAX_DISTANCE)
            {
                textPadMoved =
                    true;
            }


            if (
                !textPadDragging &&
                totalDistance >
                    MOVE_CANCEL_DISTANCE)
            {
                cancelTextPadLongPress();
            }


            queueMove(
                dx,
                dy);


            e.preventDefault();
        },
        {
            passive:
                false
        });


    function releaseTextPadPointer(
        e)
    {
        if (
            !textPadPointers.has(
                e.pointerId)
        )
        {
            return;
        }


        const wasPrimary =
            e.pointerId ===
                textPadPrimaryPointerId;


        const wasDragging =
            textPadDragging;


        const finalX =
            e.clientX;


        const finalY =
            e.clientY;


        textPadPointers.delete(
            e.pointerId);


        try
        {
            textMousePad
                .releasePointerCapture(
                    e.pointerId);
        }
        catch {}


        if (textPadMultiTouch)
        {
            cancelTextPadLongPress();


            if (
                textPadPointers.size ===
                    0)
            {
                resetTextMousePad(
                    wasDragging);
            }


            e.preventDefault();

            return;
        }


        if (wasPrimary)
        {
            cancelTextPadLongPress();


            const totalDistance =
                Math.hypot(
                    finalX -
                        textPadStartX,
                    finalY -
                        textPadStartY);


            if (wasDragging)
            {
                send(
                    {
                        type:
                            "mouse_up",

                        button:
                            "left"
                    });
            }
            else if (
                !textPadMoved &&
                totalDistance <=
                    TAP_MAX_DISTANCE)
            {
                send(
                    {
                        type:
                            "mouse_click",

                        button:
                            "left"
                    });
            }


            resetTextMousePad(
                false);
        }


        e.preventDefault();
    }


    textMousePad.addEventListener(
        "pointerup",
        releaseTextPadPointer,
        {
            passive:
                false
        });


    textMousePad.addEventListener(
        "pointercancel",
        releaseTextPadPointer,
        {
            passive:
                false
        });


    textMousePad.addEventListener(
        "contextmenu",
        e =>
        {
            e.preventDefault();

            e.stopPropagation();

            return false;
        },
        true);


    textMousePad.addEventListener(
        "selectstart",
        e =>
        {
            e.preventDefault();

            e.stopPropagation();

            return false;
        },
        true);


    textMousePad.addEventListener(
        "dragstart",
        e =>
        {
            e.preventDefault();

            e.stopPropagation();

            return false;
        },
        true);


    textMousePad.addEventListener(
        "touchstart",
        e =>
        {
            e.preventDefault();
        },
        {
            passive:
                false
        });


    textMousePad.addEventListener(
        "touchmove",
        e =>
        {
            e.preventDefault();
        },
        {
            passive:
                false
        });


    textMousePad.addEventListener(
        "touchend",
        e =>
        {
            e.preventDefault();
        },
        {
            passive:
                false
        });


    // =====================================================
    // 微信前台窗口状态
    // =====================================================

    function updateWeChatSendButton()
    {
        const shortcut =
            currentWeChatSendMode ===
                "CtrlEnter"
                ? "Ctrl+Enter"
                : "Enter";


        sendTextButton.textContent =
            `微信安全发送（${shortcut}）`;


        sendTextButton.disabled =
            !controlEnabled ||
            !currentForegroundIsWeChat;
    }


    function updateForegroundStatus(
        message)
    {
        currentForegroundIsWeChat =
            message.isWeChat ===
                true;


        if (
            message.weChatSendMode ===
                "CtrlEnter" ||
            message.weChatSendMode ===
                "Enter")
        {
            currentWeChatSendMode =
                message.weChatSendMode;
        }


        const shortcut =
            currentWeChatSendMode ===
                "CtrlEnter"
                ? "Ctrl+Enter"
                : "Enter";


        if (currentForegroundIsWeChat)
        {
            foregroundStatus.textContent =
                `✓ 当前电脑前台窗口：微信 · 将使用 ${shortcut} 发送`;


            foregroundStatus.className =
                "wechat";
        }
        else
        {
            const appName =
                message.processName ||
                message.windowTitle ||
                "未知窗口";


            foregroundStatus.textContent =
                `当前电脑前台窗口：${appName} · 微信发送已锁定`;


            foregroundStatus.className =
                "blocked";
        }


        updateWeChatSendButton();
    }


    function requestForegroundStatus()
    {
        if (
            !controlEnabled ||
            textPanel.classList
                .contains(
                    "hidden"))
        {
            return;
        }


        send(
            {
                type:
                    "foreground_query"
            });
    }


    function startForegroundWatch()
    {
        stopForegroundWatch();


        requestForegroundStatus();


        foregroundWatchTimer =
            setInterval(
                requestForegroundStatus,
                1000);
    }


    function stopForegroundWatch()
    {
        if (
            foregroundWatchTimer !==
                null)
        {
            clearInterval(
                foregroundWatchTimer);


            foregroundWatchTimer =
                null;
        }
    }


    // =====================================================
    // 触控板 / 文字输入模式
    // =====================================================

    function showTouchMode()
    {
        stopForegroundWatch();


        resetTextMousePad(
            true);


        if (
            document.activeElement ===
                textInput)
        {
            textInput.blur();
        }


        touchModeButton.classList
            .add(
                "active");


        textModeButton.classList
            .remove(
                "active");


        touchpad.classList
            .remove(
                "hidden");


        textPanel.classList
            .add(
                "hidden");


        mouseButtonsElement.classList
            .remove(
                "hidden");
    }


    function showTextMode()
    {
        emergencyRelease();


        touchModeButton.classList
            .remove(
                "active");


        textModeButton.classList
            .add(
                "active");


        touchpad.classList
            .add(
                "hidden");


        mouseButtonsElement.classList
            .add(
                "hidden");


        textPanel.classList
            .remove(
                "hidden");


        textResult.textContent =
            "点击“打开手机键盘 / 语音输入”，再使用系统键盘麦克风。";


        textResult.className =
            "";


        startForegroundWatch();
    }


    touchModeButton.addEventListener(
        "click",
        showTouchMode);


    textModeButton.addEventListener(
        "click",
        showTextMode);


    // =====================================================
    // 文字输入
    // =====================================================

    function focusTextInput()
    {
        if (!controlEnabled)
        {
            textResult.textContent =
                "当前设备尚未通过认证。";


            textResult.className =
                "error";


            return;
        }


        try
        {
            textInput.focus(
                {
                    preventScroll:
                        true
                });
        }
        catch
        {
            textInput.focus();
        }


        // 将光标放在已有文字末尾。
        // 这一步发生在按钮 click 的用户手势中，
        // 比延迟 setTimeout 自动 focus 稳定得多。
        try
        {
            const length =
                textInput.value.length;


            textInput.setSelectionRange(
                length,
                length);
        }
        catch {}


        textResult.textContent =
            "键盘已请求打开。点击系统键盘上的麦克风即可语音转文字。";


        textResult.className =
            "";
    }


    openKeyboardButton.addEventListener(
        "click",
        focusTextInput);


    function getTextValue()
    {
        return textInput.value.trim();
    }


    function updateTextCount()
    {
        textCount.textContent =
            `${textInput.value.length} / 5000`;
    }


    function submitTextAction(
        type)
    {
        const text =
            getTextValue();


        if (!text)
        {
            textResult.textContent =
                "请先输入文字或使用手机键盘麦克风完成语音转写。";


            textResult.className =
                "error";


            return;
        }


        if (!controlEnabled)
        {
            textResult.textContent =
                "当前设备尚未通过认证。";


            textResult.className =
                "error";


            return;
        }


        send(
            {
                type:
                    type,

                text:
                    text
            });


        textResult.textContent =
            "已发送到电脑，正在执行…";


        textResult.className =
            "";
    }


    textInput.addEventListener(
        "input",
        updateTextCount);


    textInput.addEventListener(
        "focus",
        () =>
        {
            textResult.textContent =
                "可以直接打字，也可以点击手机系统键盘上的麦克风进行语音输入。";


            textResult.className =
                "";
        });


    saveTextButton.addEventListener(
        "click",
        () =>
        {
            submitTextAction(
                "text_save");
        });


    typeTextButton.addEventListener(
        "click",
        () =>
        {
            submitTextAction(
                "text_type");
        });


    sendTextButton.addEventListener(
        "click",
        () =>
        {
            const text =
                getTextValue();


            if (!text)
            {
                submitTextAction(
                    "text_send");

                return;
            }


            if (!currentForegroundIsWeChat)
            {
                textResult.textContent =
                    "已阻止发送：电脑当前前台窗口不是微信。";


                textResult.className =
                    "error";


                requestForegroundStatus();


                return;
            }


            // Alpha 0.7.2.2：
            // 已经有“前台窗口必须是微信”的双重安全检查，
            // 因此点击按钮后直接发送，不再弹确认框。
            submitTextAction(
                "text_send");
        });


    textClearButton.addEventListener(
        "click",
        () =>
        {
            textInput.value =
                "";


            updateTextCount();


            textResult.textContent =
                "已清空。";


            textResult.className =
                "";
        });


    updateTextCount();


    // =====================================================
    // 安全释放
    // =====================================================

    function emergencyRelease()
    {
        cancelLongPress();


        resetTextMousePad(
            true);


        if (dragging)
        {
            send(
                {
                    type:
                        "mouse_up",

                    button:
                        "left"
                });
        }


        dragging =
            false;


        movedGesture =
            false;


        pointers.clear();


        primaryPointerId =
            null;


        multiTouchGesture =
            false;


        lastScrollCenterY =
            null;


        clearSelection();
    }


    window.addEventListener(
        "blur",
        () =>
        {
            emergencyRelease();

            debug.textContent =
                "Reset";
        });


    document.addEventListener(
        "visibilitychange",
        () =>
        {
            if (
                document.visibilityState ===
                    "hidden"
            )
            {
                emergencyRelease();
            }
        });


    // =====================================================
    // 启动
    // =====================================================

    async function initialize()
    {
        clearSelection();


        await tryPairFromUrl();


        if (!deviceToken)
        {
            status.textContent =
                "● 尚未配对";


            status.className =
                "waiting";


            setLocked(
                "此设备尚未配对",
                "请扫描电脑 Phone Mouse 窗口中的一次性二维码完成首次授权。");


            return;
        }


        connect();
    }


    initialize();

})();
</script>

</body>
</html>
""";
}
