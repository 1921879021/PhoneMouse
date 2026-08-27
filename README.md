# Phone Mouse

> 用手机浏览器在局域网内控制 Windows 鼠标、输入文字，并支持微信安全发送。

**当前源码版本：Alpha 0.7.2.3**

Phone Mouse 是一个 Windows + 手机浏览器的局域网遥控工具。电脑端运行 WPF 桌面程序并启动本地 ASP.NET Core 服务，手机通过浏览器连接电脑，无需安装手机 App。

## 功能

- 手机触控板控制 Windows 鼠标
  - 单指移动
  - 左键 / 右键
  - 双击
  - 长按拖拽
  - 双指滚轮
- 控制体验可调
  - 鼠标灵敏度
  - 滚轮速度
  - 自然滚动
  - 长按时间
- 安全配对
  - 一次性 Pair Token
  - 长期 Device Token
  - 已配对设备管理
  - 在线 / 离线状态
  - 单设备撤销 / 全部撤销
- 本地二维码连接
- 手机语音 / 文字输入
  - 使用手机系统键盘的麦克风进行语音转文字
  - 写入 `VoiceNotes.txt`
  - 输入到电脑当前窗口
- 微信安全发送
  - 自动检测电脑当前前台窗口是否为微信
  - 非微信窗口时阻止发送
  - 支持 `Enter` / `Ctrl + Enter`
- 语音 / 文字页面内嵌小型触控板

## 项目架构

```text
PhoneMouse
├─ src
│  ├─ PhoneMouse.Core
│  │  ├─ Input
│  │  ├─ Native
│  │  └─ Windows
│  │
│  ├─ PhoneMouse.Server
│  │  ├─ Network
│  │  ├─ Notes
│  │  ├─ Security
│  │  ├─ Settings
│  │  ├─ Web
│  │  └─ ServerHost.cs
│  │
│  └─ PhoneMouse.Desktop
│     ├─ Services
│     ├─ MainWindow.xaml
│     └─ MainWindow.xaml.cs
│
├─ PhoneMouse.sln
└─ global.json
```

### PhoneMouse.Core

负责 Windows 本地输入与窗口能力：

- `WindowsMouseController`
- `WindowsKeyboardController`
- `NativeInput`
- `NativeKeyboardInput`
- `WindowsForegroundWindowService`

底层使用 Win32 `SendInput`。

### PhoneMouse.Server

负责局域网 HTTP / WebSocket 服务：

- Kestrel
- WebSocket 控制通道
- 设备认证
- 配对 Token
- 控制设置同步
- TXT 写入
- 手机网页

默认监听：

```text
http://0.0.0.0:9527
```

### PhoneMouse.Desktop

Windows WPF 控制中心：

- 显示局域网访问地址
- 显示二维码
- 管理已配对设备
- 调整鼠标与滚轮参数
- 设置微信发送快捷键

## 开发环境

推荐：

- Windows 10 / 11 x64
- .NET SDK 10.0.400
- Visual Studio 2026 或支持 .NET 10 的 Visual Studio
- Git

仓库中的 `global.json` 会固定 .NET SDK 版本。

查看 SDK：

```powershell
dotnet --list-sdks
```

## 编译

```powershell
git clone https://github.com/1921879021/PhoneMouse.git

cd PhoneMouse

dotnet build PhoneMouse.sln --no-incremental
```

运行：

```powershell
dotnet ".\src\PhoneMouse.Desktop\bin\Debug\net10.0-windows\PhoneMouse.Desktop.dll"
```

或者：

```powershell
dotnet run --project src\PhoneMouse.Desktop
```

## 使用

1. 在 Windows 电脑运行 Phone Mouse。
2. 确保手机和电脑处于同一个 Wi-Fi / 局域网。
3. 电脑窗口会显示类似：

```text
http://192.168.1.102:9527
```

4. 手机扫描电脑显示的一次性二维码。
5. 首次完成安全配对后，即可控制鼠标。
6. 后续已授权设备可直接访问普通局域网地址。

## 手机语音输入

Phone Mouse 当前不直接调用浏览器 Speech API。

使用流程：

```text
手机打开“语音 / 文字”
→ 点击“打开手机键盘 / 语音输入”
→ 点击手机系统键盘自带麦克风
→ 系统完成语音转文字
→ Phone Mouse 将文字发送给电脑
```

这样可以避免 HTTP 局域网页在不同 Android / iOS 浏览器中的麦克风权限兼容问题。

## VoiceNotes.txt

手机点击：

```text
写入 VoiceNotes.txt
```

默认写入：

```text
Windows 文档目录\PhoneMouse\VoiceNotes.txt
```

例如：

```text
C:\Users\<User>\Documents\PhoneMouse\VoiceNotes.txt
```

文件使用 UTF-8 BOM，方便 Windows PowerShell 5.1 和记事本正确识别中文。

## 微信安全发送

手机端会持续检查电脑当前前台窗口。

检测到微信：

```text
✓ 当前电脑前台窗口：微信
```

此时允许：

```text
微信安全发送
```

如果电脑当前不是微信，则前端按钮锁定，并且服务端也会再次检查，防止误发送。

支持：

```text
Enter
Ctrl + Enter
```

可在电脑端 Phone Mouse 设置中切换。

## 安全说明

当前版本使用：

```text
HTTP
ws://
```

还没有 HTTPS / WSS。

因此只建议在以下环境使用：

- 家庭 Wi-Fi
- 可信任公司内网
- 个人热点

不要在不可信公共 Wi-Fi 上使用。

电脑端保存的是 Device Token 的 SHA-256 哈希，而不是原始 Token。

设备数据位于：

```text
%LOCALAPPDATA%\PhoneMouse\
```

主要包括：

```text
trusted-devices.json
control-settings.json
```

## Windows 防火墙

Phone Mouse 默认监听 TCP `9527`。

第一次运行时 Windows 可能询问是否允许网络访问。

建议只允许：

```text
专用网络
```

不建议关闭 Windows Defender 或整个防火墙。

## Portable 版本

项目已经预留 Portable 发布流程。

目标发布形式：

```text
PhoneMouse.exe
```

测试用户无需安装 Visual Studio 或 .NET SDK。

发布相关说明见：

```text
docs/BUILD-RELEASE.md
```

## 二次开发

请先阅读：

- [开发指南](docs/DEVELOPMENT.md)
- [WebSocket 协议](docs/PROTOCOL.md)
- [构建与发布](docs/BUILD-RELEASE.md)
- [安全说明](SECURITY.md)
- [贡献指南](CONTRIBUTING.md)

## 已知限制

- 当前只支持 Windows。
- 当前为局域网模式。
- HTTP / WebSocket 尚未加密。
- 微信检测依赖 Windows 前台进程识别。
- 不同版本微信的进程名和快捷键行为可能存在差异。
- 当前语音识别依赖手机系统键盘提供的语音输入能力。
- 仍处于 Alpha 阶段，不建议用于生产环境或关键业务。

## Roadmap

计划中的方向：

- Portable 单文件发布
- 正式 Windows 安装包
- HTTPS / WSS
- 更完善的输入法与快捷键支持
- 文件传输
- 媒体控制
- 演示文稿控制
- 自定义快捷键
- 更完善的设备权限模型

## License

当前仓库尚未指定开源许可证。

在正式选择许可证之前，请不要默认该项目已经允许任意商业使用、再发布或闭源集成。

如果计划开放二次开发，建议后续明确选择 MIT / Apache-2.0 / GPL 等许可证之一。
