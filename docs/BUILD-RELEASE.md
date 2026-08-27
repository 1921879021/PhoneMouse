# Build & Release

## Debug Build

```powershell
cd D:\Projects\PhoneMouse

dotnet build PhoneMouse.sln --no-incremental
```

运行：

```powershell
dotnet ".\src\PhoneMouse.Desktop\bin\Debug\net10.0-windows\PhoneMouse.Desktop.dll"
```

## Release Build

```powershell
dotnet build PhoneMouse.sln `
    -c Release `
    --no-incremental
```

## Portable

如果仓库中已经安装：

```text
scripts/Publish-Portable.ps1
```

执行：

```powershell
.\scripts\Publish-Portable.ps1 `
    -Version 0.7.2.3-alpha
```

目标：

```text
artifacts/
└─ PhoneMouse_Alpha_0.7.2.3-alpha_Portable_win-x64.zip
```

Portable 目标：

- Windows x64
- Self-contained
- Single-file
- 用户无需安装 .NET SDK

## GitHub Actions

仓库可使用：

```text
.github/workflows/build-portable.yml
```

进入：

```text
GitHub
→ Actions
→ Build Portable EXE
→ Run workflow
```

## Tag / Release

示例：

```powershell
git tag -a v0.7.2.3-alpha `
    -m "Phone Mouse Alpha 0.7.2.3"

git push origin v0.7.2.3-alpha
```

如果 GitHub Actions Release 工作流已经配置完成，可自动生成 Release Artifact。

## 发布前检查

至少测试：

- Windows 10
- Windows 11
- 干净电脑
- 不安装 Visual Studio
- 不安装 .NET SDK
- 手机与电脑同一 Wi-Fi
- QR 配对
- 鼠标移动
- 左右键
- 拖拽
- 滚轮
- 中文输入
- VoiceNotes
- 微信识别
- Enter
- Ctrl+Enter
- 设备撤销

## SmartScreen

当前 Alpha 版本若没有数字签名，可能看到：

```text
Windows 已保护你的电脑
未知发布者
```

正式公开发布前应考虑代码签名。

## 防火墙

默认端口：

```text
9527/TCP
```

测试用户应该只允许：

```text
专用网络
```

不要要求用户关闭 Windows Defender。
