# Development Guide

本文档面向希望阅读源码、修改功能或进行二次开发的开发者。

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

否则：

```text
SendInput
→ 返回 0
→ Win32 error 87
```

也就是：

```text
ERROR_INVALID_PARAMETER
```

修改 Native Input 代码后，一定要重新测试：

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
src/PhoneMouse.Server/Web/TouchpadPage.cs
```

目前前端 HTML / CSS / JS 被作为 C# 字符串内嵌。

修改手机页面后，建议：

```powershell
dotnet build PhoneMouse.sln --no-incremental
```

并重新启动 Desktop。

如果手机仍显示旧页面：

```text
关闭旧标签页
→ 新开标签页
→ URL 后增加新的 ?v=xxx
```

例如：

```text
http://192.168.1.102:9527/?v=dev2
```

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

Desktop 负责实例化：

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

这样即使互联网不可用，只要局域网可用，二维码仍然能生成。

## TXT

```text
src/PhoneMouse.Server/Notes/VoiceNoteService.cs
```

默认：

```text
Documents\PhoneMouse\VoiceNotes.txt
```

使用：

```text
UTF-8 BOM
```

## 调试建议

### 检查端口

```powershell
Get-NetTCPConnection `
    -LocalPort 9527 `
    -ErrorAction SilentlyContinue
```

### 检查服务器版本

```powershell
$response = Invoke-WebRequest `
    "http://127.0.0.1:9527/?v=debug"

$response.Content |
Select-String "Alpha"
```

### 强制重新编译

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
