# Development Guide

本文档面向希望阅读源码、修改功能或进行二次开发的开发者。

当前参考版本：**Alpha 0.7.2.6.1**

## 技术栈

| 模块 | 技术 |
|---|---|
| Desktop | WPF |
| Runtime | .NET 10 |
| Local Server | ASP.NET Core / Kestrel |
| Realtime | WebSocket |
| Mouse / Keyboard | Win32 SendInput |
| QR | 本地 C# 实现 |
| Mobile UI | HTML + CSS + JavaScript |
| Persistence | JSON / TXT |

## 解决方案

```text
PhoneMouse.sln

PhoneMouse.Core
PhoneMouse.Server
PhoneMouse.Desktop
```

### Core

职责：

```text
Windows 系统能力
```

包括：

- 鼠标移动
- 左右键
- 拖拽
- 滚轮
- Unicode 键盘输入
- Enter / Ctrl+Enter
- 前台窗口检测

重要文件：

```text
src/PhoneMouse.Core/Input/IMouseController.cs
src/PhoneMouse.Core/Input/IKeyboardController.cs
src/PhoneMouse.Core/Input/WindowsMouseController.cs
src/PhoneMouse.Core/Input/WindowsKeyboardController.cs
src/PhoneMouse.Core/Native/NativeInput.cs
src/PhoneMouse.Core/Native/NativeKeyboardInput.cs
src/PhoneMouse.Core/Windows/WindowsForegroundWindowService.cs
```

## SendInput 注意事项

Windows x64 下 `INPUT` 结构尺寸必须正确。

`INPUT` union 不能只定义 `KEYBDINPUT`，还要正确覆盖：

```text
MOUSEINPUT
KEYBDINPUT
HARDWAREINPUT
```

否则 `SendInput` 可能返回 0，并出现 Win32 error 87：

```text
ERROR_INVALID_PARAMETER
```

修改 Native Input 后至少重新测试：

- 英文
- 中文
- Enter
- Ctrl+Enter

## Server

入口：

```text
src/PhoneMouse.Server/ServerHost.cs
```

默认监听：

```text
0.0.0.0:9527
```

主要路由：

```text
GET  /
POST /api/pair
GET  /ws
```

### GET /

返回手机端页面：

```text
src/PhoneMouse.Server/web/TouchpadPage.cs
```

目前前端 HTML / CSS / JS 被作为 C# 字符串内嵌。

修改手机页面后建议：

```powershell
dotnet build PhoneMouse.sln --no-incremental
```

并重新启动 Desktop。

如果手机仍显示旧页面，可以关闭旧标签页，重新访问并增加版本查询参数，例如：

```text
http://192.168.1.102:9527/?v=dev2
```

## 手机方向映射

Alpha 0.7.2.6 起，手机触控板支持：

```text
portrait
landscape-left
landscape-right
```

选择值保存在手机浏览器：

```text
localStorage
```

当前键名：

```text
PhoneMouse.OrientationMode
```

方向映射同时影响：

- 主触控板单指移动
- 主触控板长按拖拽
- 主触控板双指滚轮
- 语音 / 文字页的小触控板

二次开发时，如果新增其他基于相对位移的手势，应复用统一的方向变换逻辑，避免各模块方向不一致。

## 双指滚动

Alpha 0.7.2.6 默认滚动语义：

```text
双指向上 -> 页面向下
双指向下 -> 页面向上
```

电脑端的 `naturalScrolling` 设置仍保留，用于再次反转。

## Security

配对相关：

```text
src/PhoneMouse.Server/Security/PairingService.cs
src/PhoneMouse.Server/Security/TrustedDeviceStore.cs
```

流程：

```text
电脑生成 Pair Token
→ 二维码包含 Pair Token
→ 手机 POST /api/pair
→ Pair Token 成功使用后立即失效
→ 服务端签发 Device Token
→ 手机 localStorage 保存 Device Token
→ 电脑只保存 SHA-256(Token)
→ WebSocket 首包必须 auth
```

未通过认证的 WebSocket 不进入控制消息处理。

## Settings

控制设置：

```text
src/PhoneMouse.Server/Settings/ControlSettingsService.cs
```

保存：

```text
%LOCALAPPDATA%\PhoneMouse\control-settings.json
```

设置变化会通过 WebSocket 实时同步到已连接手机。

## Desktop

主要文件：

```text
src/PhoneMouse.Desktop/MainWindow.xaml
src/PhoneMouse.Desktop/MainWindow.xaml.cs
```

Desktop 负责组合：

```text
WindowsMouseController
WindowsKeyboardController
WindowsForegroundWindowService
VoiceNoteService
PairingService
TrustedDeviceStore
ControlSettingsService
ServerHost
```

## QR Code

```text
src/PhoneMouse.Desktop/Services/QrCodeService.cs
```

当前 QR 为纯本地实现，不依赖第三方在线 API。

## TXT

```text
src/PhoneMouse.Server/Notes/VoiceNoteService.cs
```

默认：

```text
Documents\PhoneMouse\VoiceNotes.txt
```

当前手机 UI 不显示写入按钮，但后端能力保留。

## 调试建议

检查端口：

```powershell
Get-NetTCPConnection `
    -LocalPort 9527 `
    -ErrorAction SilentlyContinue
```

检查服务器页面版本：

```powershell
$response = Invoke-WebRequest `
    "http://127.0.0.1:9527/?v=debug"

$response.Content |
Select-String "Alpha"
```

强制重新编译：

```powershell
dotnet build PhoneMouse.sln --no-incremental
```

## 代码修改原则

建议保持：

1. Core 不直接依赖 Desktop。
2. Server 只依赖 Core。
3. Desktop 组合各服务。
4. WebSocket 消息尽量保持向后兼容。
5. 所有高权限操作必须经过设备认证。
6. 发送类操作必须考虑误触和前台窗口安全检查。
7. 新增手势时统一经过方向变换逻辑。
8. 手机端全局 `preventDefault()` 要注意不要拦截真正的按钮控件。
