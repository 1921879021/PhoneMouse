# Security Policy

## 当前安全模型

Phone Mouse 是局域网遥控软件，可以：

- 控制鼠标
- 注入键盘文字
- 触发 Enter / Ctrl+Enter
- 对当前前台窗口进行操作

因此应把它视为具有较高本地权限的工具。

## 配对

当前使用：

```text
One-time Pair Token
→ Device Token
→ WebSocket auth
```

电脑只保存 Device Token 的 SHA-256 哈希。

已授权设备可以在之后重新连接。

## 撤销

Desktop 可：

- 移除单个设备
- 移除全部设备

撤销后对应设备 Token 失效。

## 网络

当前版本：

```text
HTTP
ws://
```

没有 TLS。

因此只建议在可信任局域网使用。

不要在以下环境使用当前 Alpha：

- 公共 Wi-Fi
- 酒店 Wi-Fi
- 商场 Wi-Fi
- 不可信共享网络

## 微信发送

`text_send` 不只依赖手机前端状态。

电脑服务端会再次检查 Windows 当前前台进程是否被识别为微信。

这可以降低误发送风险，但不能保证兼容未来所有微信版本。

## 手机方向设置

竖屏 / 横放口左 / 横放口右属于手机浏览器本地偏好，保存在 `localStorage`。

它不会增加新的电脑权限，但错误的方向选择会造成鼠标方向不符合预期，因此用户应根据手机实际摆放方式选择。

## Reporting

如果发现安全问题，请不要公开发布可直接利用的攻击步骤。

建议先通过 GitHub 私下联系仓库维护者，待修复后再公开讨论。

## Future Work

计划增强：

- HTTPS / WSS
- 更严格的 Session
- Token Rotation
- 细粒度设备权限
- 可选本地 PIN
- 更完善的审计日志
