# Build & Release

当前参考版本：**Alpha 0.7.2.6.1**

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

仓库已经配置：

```text
scripts/Publish-Portable.ps1
```

本机发布：

```powershell
.\scripts\Publish-Portable.ps1 `
    -Version 0.7.2.6.1-alpha
```

Portable 目标：

- Windows x64
- Self-contained
- Single-file
- 用户无需安装 .NET SDK

## GitHub Actions

工作流：

```text
.github/workflows/build-portable.yml
```

手动运行：

```text
GitHub
→ Actions
→ Build Portable EXE
→ Run workflow
```

## Tag / Release

当前标签示例：

```powershell
git tag -a v0.7.2.6.1-alpha `
    -m "Phone Mouse Alpha 0.7.2.6.1"

git push origin v0.7.2.6.1-alpha
```

标签推送后，工作流可自动：

```text
编译
→ 生成 Portable ZIP
→ 生成 SHA256
→ 创建 GitHub Release
```

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
- 双指滚轮
- 竖屏模式
- 横放 · 充电口朝左
- 横放 · 充电口朝右
- 中文输入
- 微信识别
- Enter
- Ctrl+Enter
- 设备撤销

## SmartScreen

Alpha 版本若没有数字签名，可能看到：

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
