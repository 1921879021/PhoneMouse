# Contributing

感谢关注 Phone Mouse。

当前项目仍处于 Alpha 阶段，接口和 UI 可能继续变化。

## 开始之前

建议先阅读：

```text
README.md
docs/DEVELOPMENT.md
docs/PROTOCOL.md
SECURITY.md
```

## 提交 Issue

请尽量包含：

- Windows 版本
- 手机系统
- 浏览器
- Phone Mouse 版本
- 完整错误信息
- 复现步骤
- 是否能稳定复现

如果是编译问题，请附：

```powershell
dotnet --info
dotnet build PhoneMouse.sln --no-incremental
```

的相关输出。

## Pull Request

建议：

1. 一个 PR 只解决一类问题。
2. 不要把 `bin/`、`obj/`、`.vs/` 提交到仓库。
3. 修改 WebSocket 协议时同步更新 `docs/PROTOCOL.md`。
4. 修改设置项时考虑旧配置文件兼容。
5. 修改 Native Input 时在 Windows x64 真机验证。
6. 高权限操作必须保留认证和安全检查。

## Commit Message

示例：

```text
Fix Android soft keyboard focus
Add WeChat foreground detection
Improve WebSocket device authentication
Add Portable release pipeline
```

## 测试

PR 至少应保证：

```powershell
dotnet build PhoneMouse.sln --no-incremental
```

成功。

涉及鼠标、键盘、微信或移动端手势的修改，应进行真机测试。
