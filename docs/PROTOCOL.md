# WebSocket Protocol

当前参考版本：**Alpha 0.7.2.6.1**

Phone Mouse 使用：

```text
ws://<PC-IP>:9527/ws
```

当前协议为 JSON 文本消息。

## 认证

连接建立后，手机发送的第一条消息必须是：

```json
{
  "type": "auth",
  "token": "<device-token>"
}
```

认证成功示例：

```json
{
  "type": "connected",
  "authenticated": true
}
```

未经认证的连接不能控制电脑。

## Mouse Move

```json
{
  "type": "mouse_move",
  "dx": 12,
  "dy": -5
}
```

说明：手机端的竖屏 / 横放方向映射在发送前完成，服务端继续接收标准的相对 `dx / dy`。

## Mouse Click

左键：

```json
{
  "type": "mouse_click",
  "button": "left"
}
```

右键：

```json
{
  "type": "mouse_click",
  "button": "right"
}
```

## Double Click

```json
{
  "type": "mouse_double_click"
}
```

## Mouse Down

```json
{
  "type": "mouse_down",
  "button": "left"
}
```

## Mouse Up

```json
{
  "type": "mouse_up",
  "button": "left"
}
```

## Scroll

```json
{
  "type": "mouse_scroll",
  "dy": -120
}
```

手机端负责根据摆放方向和滚动设置计算最终 `dy`。

## Save Text

后端仍支持：

```json
{
  "type": "text_save",
  "text": "测试文字"
}
```

成功示例：

```json
{
  "type": "action_result",
  "action": "text_save",
  "success": true,
  "message": "已写入 VoiceNotes.txt"
}
```

当前手机 UI 默认不显示 VoiceNotes 按钮，但协议仍保留。

## Type Text

```json
{
  "type": "text_type",
  "text": "测试文字"
}
```

该消息只向当前 Windows 焦点窗口输入文字，不主动发送。

## WeChat Safe Send

```json
{
  "type": "text_send",
  "text": "今天吃什么？"
}
```

服务端会重新检查当前 Windows 前台窗口。

不是微信时应返回失败结果，防止误发送。

## Foreground Query

```json
{
  "type": "foreground_query"
}
```

响应包含：

```json
{
  "type": "foreground_status",
  "isWeChat": true,
  "processName": "WeChat",
  "windowTitle": "微信",
  "weChatSendMode": "Enter"
}
```

`weChatSendMode`：

```text
Enter
CtrlEnter
```

## Settings

服务端会向已认证设备推送控制设置，例如：

```json
{
  "type": "settings",
  "mouseSensitivity": 1.0,
  "scrollSpeed": 1.0,
  "naturalScrolling": false,
  "longPressMs": 420,
  "weChatSendMode": "Enter"
}
```

手机的 `portrait / landscape-left / landscape-right` 属于单设备浏览器侧偏好，不通过该 WebSocket 设置消息同步。

## 协议扩展建议

后续增加新消息类型时建议：

- `type` 保持唯一。
- 不修改已有字段语义。
- 新字段优先做可选。
- 服务端应安全忽略未知消息类型。
- 高权限行为必须放在认证后执行。
