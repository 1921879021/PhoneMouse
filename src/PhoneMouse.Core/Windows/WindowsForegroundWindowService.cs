using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace PhoneMouse.Core.Windows;

public sealed class WindowsForegroundWindowService :
    IForegroundWindowService
{
    private const int MaxWindowTitleLength =
        512;


    [DllImport(
        "user32.dll",
        EntryPoint = "GetForegroundWindow")]
    private static extern nint NativeGetForegroundWindow();


    [DllImport(
        "user32.dll",
        CharSet = CharSet.Unicode,
        SetLastError = true)]
    private static extern int GetWindowText(
        nint hWnd,
        StringBuilder lpString,
        int nMaxCount);


    [DllImport(
        "user32.dll",
        SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(
        nint hWnd,
        out uint processId);


    public ForegroundWindowInfo GetForegroundWindow()
    {
        nint handle =
            NativeGetForegroundWindow();


        if (
            handle ==
                nint.Zero)
        {
            return
                new ForegroundWindowInfo(
                    IsWeChat: false,
                    ProcessName: string.Empty,
                    WindowTitle: string.Empty);
        }


        string windowTitle =
            ReadWindowTitle(
                handle);


        string processName =
            ReadProcessName(
                handle);


        bool isWeChat =
            IsWeChatProcess(
                processName);


        return
            new ForegroundWindowInfo(
                isWeChat,
                processName,
                windowTitle);
    }


    private static string ReadWindowTitle(
        nint handle)
    {
        StringBuilder builder =
            new(
                MaxWindowTitleLength);


        _ =
            GetWindowText(
                handle,
                builder,
                builder.Capacity);


        return
            builder
                .ToString()
                .Trim();
    }


    private static string ReadProcessName(
        nint handle)
    {
        try
        {
            _ =
                GetWindowThreadProcessId(
                    handle,
                    out uint processId);


            if (
                processId ==
                    0)
            {
                return
                    string.Empty;
            }


            using Process process =
                Process.GetProcessById(
                    unchecked(
                        (int)processId));


            return
                process.ProcessName
                    .Trim();
        }
        catch
        {
            return
                string.Empty;
        }
    }


    private static bool IsWeChatProcess(
        string processName)
    {
        if (
            string.IsNullOrWhiteSpace(
                processName))
        {
            return false;
        }


        return
            processName.StartsWith(
                "WeChat",
                StringComparison.OrdinalIgnoreCase) ||
            processName.StartsWith(
                "Weixin",
                StringComparison.OrdinalIgnoreCase);
    }
}
