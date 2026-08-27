param(
    [string]$Version = "0.7.2.3-alpha",
    [string]$Runtime = "win-x64",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$ProjectRoot = Split-Path $PSScriptRoot -Parent
$DesktopProject = Join-Path $ProjectRoot "src\PhoneMouse.Desktop\PhoneMouse.Desktop.csproj"
$ArtifactsRoot = Join-Path $ProjectRoot "artifacts"

$SafeVersion = $Version.Trim()
if ([string]::IsNullOrWhiteSpace($SafeVersion)) {
    $SafeVersion = "dev"
}

$SafeVersion = $SafeVersion -replace '[\\/:*?"<>| ]', '-'

$ReleaseName = "PhoneMouse_Alpha_${SafeVersion}_Portable_${Runtime}"
$PublishDir = Join-Path $ArtifactsRoot $ReleaseName
$ZipPath = Join-Path $ArtifactsRoot "${ReleaseName}.zip"
$HashPath = Join-Path $ArtifactsRoot "${ReleaseName}.sha256.txt"

if (-not (Test-Path $DesktopProject)) {
    throw "找不到桌面项目：$DesktopProject"
}

New-Item -ItemType Directory -Path $ArtifactsRoot -Force | Out-Null

Remove-Item $PublishDir -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item $ZipPath -Force -ErrorAction SilentlyContinue
Remove-Item $HashPath -Force -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host " Phone Mouse Portable 发布" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "版本：$SafeVersion"
Write-Host "运行时：$Runtime"
Write-Host "配置：$Configuration"
Write-Host ""

# 自包含 + 单文件。
# PublishTrimmed=false：WPF / ASP.NET Core 当前不做裁剪，避免反射/运行时兼容问题。
# IncludeNativeLibrariesForSelfExtract=true：把 native runtime 一并打进单文件。
# PublishReadyToRun=false：降低构建复杂度和体积，便于 Alpha 阶段分发。
dotnet publish $DesktopProject `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:PublishTrimmed=false `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -p:PublishReadyToRun=false `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    -o $PublishDir

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish 失败，退出代码：$LASTEXITCODE"
}

$OriginalExe = Join-Path $PublishDir "PhoneMouse.Desktop.exe"
$FinalExe = Join-Path $PublishDir "PhoneMouse.exe"

if (-not (Test-Path $OriginalExe)) {
    throw "发布完成，但没有找到：$OriginalExe"
}

Move-Item $OriginalExe $FinalExe -Force

# 删除不需要发给测试用户的调试文件。
Get-ChildItem $PublishDir -Filter "*.pdb" -ErrorAction SilentlyContinue |
    Remove-Item -Force -ErrorAction SilentlyContinue

$FirstRunText = @"
Phone Mouse Portable
====================

版本：$SafeVersion
平台：Windows x64

使用方法
--------
1. 双击 PhoneMouse.exe。
2. 如果 Windows 防火墙弹出提示，请允许“专用网络”访问。
3. 确保电脑和手机连接同一个 Wi-Fi / 局域网。
4. 电脑窗口会显示访问地址和二维码。
5. 手机首次扫描二维码完成配对。
6. 后续可直接访问电脑显示的局域网地址。

当前功能
--------
- 手机触控板控制 Windows 鼠标
- 左键 / 右键 / 双击
- 长按拖拽
- 双指滚轮
- 鼠标灵敏度 / 滚轮速度 / 自然滚动 / 长按时间
- 一次性二维码安全配对
- 已配对设备管理
- 手机语音 / 文字输入
- 写入 VoiceNotes.txt
- 输入到当前 Windows 窗口
- 微信前台检测
- 微信安全发送（Enter / Ctrl+Enter）

安全说明
--------
当前版本仍使用局域网 HTTP / ws://。
请只在可信任的家庭 Wi-Fi、公司内网或个人热点中使用。

Windows SmartScreen
-------------------
本 Alpha 版暂未做商业代码签名。
某些电脑第一次运行可能显示“未知发布者”或 SmartScreen 提示。
这是因为 EXE 没有数字签名，不代表程序自动被判定为恶意软件。

TXT 保存位置
------------
Windows “文档”目录\PhoneMouse\VoiceNotes.txt
"@

Set-Content `
    -Path (Join-Path $PublishDir "首次使用说明.txt") `
    -Value $FirstRunText `
    -Encoding UTF8

# 生成版本信息文本。
$BuildInfo = @"
Phone Mouse Portable Build
Version=$SafeVersion
Runtime=$Runtime
Configuration=$Configuration
BuiltAtUtc=$([DateTime]::UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"))
"@

Set-Content `
    -Path (Join-Path $PublishDir "build-info.txt") `
    -Value $BuildInfo `
    -Encoding UTF8

# ZIP 便于发送和放 GitHub Releases。
Compress-Archive `
    -Path (Join-Path $PublishDir "*") `
    -DestinationPath $ZipPath `
    -CompressionLevel Optimal `
    -Force

$ExeHash = Get-FileHash $FinalExe -Algorithm SHA256
$ZipHash = Get-FileHash $ZipPath -Algorithm SHA256

$HashText = @"
PhoneMouse.exe
SHA256=$($ExeHash.Hash)

$(Split-Path $ZipPath -Leaf)
SHA256=$($ZipHash.Hash)
"@

Set-Content `
    -Path $HashPath `
    -Value $HashText `
    -Encoding ASCII

Write-Host ""
Write-Host "=============================================" -ForegroundColor Green
Write-Host " Portable 发布成功" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""
Write-Host "可直接运行的 EXE：" -ForegroundColor Yellow
Write-Host $FinalExe
Write-Host ""
Write-Host "可直接发送给别人的 ZIP：" -ForegroundColor Yellow
Write-Host $ZipPath
Write-Host ""
Write-Host "SHA256：" -ForegroundColor Yellow
Write-Host $HashPath
Write-Host ""

$Exe = Get-Item $FinalExe
$Zip = Get-Item $ZipPath

Write-Host ("EXE 大小：{0:N1} MB" -f ($Exe.Length / 1MB))
Write-Host ("ZIP 大小：{0:N1} MB" -f ($Zip.Length / 1MB))
