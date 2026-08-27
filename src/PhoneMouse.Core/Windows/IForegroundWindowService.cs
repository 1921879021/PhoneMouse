namespace PhoneMouse.Core.Windows;

public interface IForegroundWindowService
{
    ForegroundWindowInfo GetForegroundWindow();
}


public sealed record ForegroundWindowInfo(
    bool IsWeChat,
    string ProcessName,
    string WindowTitle);
