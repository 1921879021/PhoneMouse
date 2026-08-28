# Phone Mouse

> 用手机浏览器在局域网内控制 Windows 鼠标、输入文字，并支持微信安全发送。

**当前源码版本：Alpha 0.7.2.6.1**

Phone Mouse 是一个 Windows + 手机浏览器的局域网遥控工具。电脑端运行 WPF 桌面程序并启动本地 ASP.NET Core 服务，手机通过浏览器连接电脑，无需安装手机 App。

## 功能

- 手机触控板控制 Windows 鼠标
  - 单指移动
  - 左键 / 右键
  - 双击
  - 长按拖拽
  - 双指滚轮
- 手机摆放方向切换
  - 竖屏
  - 横放 · 充电口朝左
  - 横放 · 充电口朝右
  - 方向选择自动保存在手机浏览器中
- 双指滚动
  - 默认：双指向上滑 → 页面向下滚动
  - 默认：双指向下滑 → 页面向上滚动
  - 可通过电脑端“自然滚动”设置再次反转
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
  - 使用手机系统键盘麦克风进行语音转文字
  - 输入到电脑当前窗口
  - 输入框内直接提供发送按钮，软键盘打开时也方便操作
- 微信安全发送
  - 自动检测电脑当前前台窗口是否为微信
  - 非微信窗口时阻止发送
  - 支持 `Enter` / `Ctrl + Enter`
- 语音 / 文字页面内嵌小型触控板
- 后端仍保留 `VoiceNotes.txt` 写入能力，当前手机 UI 默认不显示该按钮

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
│  │  ├─ web
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
- TXT 写入能力
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

仓库中的 `global.json` 用于固定 .NET SDK 版本。

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

## 竖屏 / 横放模式

主触控板提供三种手机摆放模式：

```text
竖屏
横放 · 口左
横放 · 口右
```

其中“口左 / 口右”指手机充电口朝向。

之所以采用用户手动选择，而不是完全自动判断，是因为手机网页和部分内置 WebView 在自动旋转关闭、方向锁定或不同厂商系统下，不一定能可靠获得真实的设备摆放方向。

选择结果保存在手机浏览器的 `localStorage` 中，下次打开会继续使用上一次选择。

## 手机语音输入

Phone Mouse 当前不直接调用浏览器 Speech API。

使用流程：

```text
进入“语音 / 文字”
→ 点击文字输入框
→ 手机系统键盘弹出
→ 点击系统键盘麦克风
→ 系统完成语音转文字
→ 点击输入框底部操作按钮发送到电脑
```

这样可以避开局域网 HTTP 页面在不同 Android / iOS 浏览器中的麦克风权限兼容问题。

## 微信安全发送

手机端会持续检查电脑当前前台窗口。

检测到微信时允许“微信发送”；如果电脑当前不是微信，手机按钮会被锁定，并且服务端还会再次检查，降低误发送风险。

支持：

```text
Enter
Ctrl + Enter
```

可在电脑端 Phone Mouse 设置中切换。

## VoiceNotes.txt

后端仍保留 `text_save` / `VoiceNotes.txt` 能力，默认路径：

```text
Windows 文档目录\PhoneMouse\VoiceNotes.txt
```

例如：

```text
C:\Users\<User>\Documents\PhoneMouse\VoiceNotes.txt
```

当前 Alpha 0.7.2.6.1 的手机界面已经取消“写入 VoiceNotes.txt”按钮，以减少界面占用，但协议能力仍保留用于兼容和二次开发。

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

第一次运行时 Windows 可能询问是否允许网络访问。建议只允许：

```text
专用网络
```

不建议关闭 Windows Defender 或整个防火墙。

## Portable 版本

项目已配置 GitHub Actions Portable 发布流程。

目标发布形式：

```text
PhoneMouse.exe
```

测试用户无需安装 Visual Studio 或 .NET SDK。

构建说明见：

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
- [更新记录](CHANGELOG.md)

## 已知限制

- 当前只支持 Windows。
- 当前主要面向局域网使用。
- HTTP / WebSocket 尚未加密。
- 微信检测依赖 Windows 前台进程识别。
- 不同版本微信的进程名和快捷键行为可能存在差异。
- 当前语音识别依赖手机系统键盘提供的语音输入能力。
- 横放方向由用户手动选择。
- 仍处于 Alpha 阶段，不建议用于关键业务。

## Roadmap

计划中的方向：

- 正式 Windows 安装包
- HTTPS / WSS
- 更完善的输入法与快捷键支持
- 文件传输
- 媒体控制
- 演示文稿控制
- 自定义快捷键
- 更完善的设备权限模型
- 正式应用图标与代码签名

## License

当前仓库尚未指定开源许可证。

在正式选择许可证之前，请不要默认该项目已经允许任意商业使用、再发布或闭源集成。

如果计划开放二次开发，建议后续明确选择 MIT / Apache-2.0 / GPL 等许可证之一。
