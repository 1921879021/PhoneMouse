# WebSocket Protocol

Phone Mouse 使用 WebSocket：

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

认证成功：

```json
{
  "type": "connected",
  "authenticated": true
}
```

认证失败：

```json
{
  "type": "auth_failed"
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

## Save Text

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

不是微信：

```json
{
  "type": "action_result",
  "action": "text_send",
  "success": false,
  "message": "已阻止发送：电脑当前前台窗口不是微信。"
}
```

## Foreground Query

```json
{
  "type": "foreground_query"
}
```

响应示例：

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

服务端会向已认证设备推送：

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

## 协议扩展建议

后续增加新消息类型时建议：

- `type` 保持唯一。
- 不修改已有字段语义。
- 新字段优先做可选。
- 服务端必须忽略未知消息类型。
- 高权限行为必须放在认证后执行。
