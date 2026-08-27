using System.ComponentModel;
using System.Runtime.InteropServices;
using PhoneMouse.Core.Native;

namespace PhoneMouse.Core.Input;

public sealed class WindowsMouseController : IMouseController
{
    public void MoveRelative(int dx, int dy)
    {
        SendMouseInput(
            dx,
            dy,
            0,
            NativeInput.MOUSEEVENTF_MOVE);
    }

    public void LeftClick()
    {
        LeftDown();
        LeftUp();
    }

    public void RightClick()
    {
        SendMouseInput(
            0,
            0,
            0,
            NativeInput.MOUSEEVENTF_RIGHTDOWN);

        SendMouseInput(
            0,
            0,
            0,
            NativeInput.MOUSEEVENTF_RIGHTUP);
    }

    public void DoubleClick()
    {
        LeftClick();
        LeftClick();
    }

    public void LeftDown()
    {
        SendMouseInput(
            0,
            0,
            0,
            NativeInput.MOUSEEVENTF_LEFTDOWN);
    }

    public void LeftUp()
    {
        SendMouseInput(
            0,
            0,
            0,
            NativeInput.MOUSEEVENTF_LEFTUP);
    }

    public void Scroll(int delta)
    {
        SendMouseInput(
            0,
            0,
            unchecked((uint)delta),
            NativeInput.MOUSEEVENTF_WHEEL);
    }

    private static void SendMouseInput(
        int dx,
        int dy,
        uint mouseData,
        uint flags)
    {
        var input = new NativeInput.INPUT
        {
            type = NativeInput.INPUT_MOUSE,
            mi = new NativeInput.MOUSEINPUT
            {
                dx = dx,
                dy = dy,
                mouseData = mouseData,
                dwFlags = flags,
                time = 0,
                dwExtraInfo = UIntPtr.Zero
            }
        };

        NativeInput.INPUT[] inputs = [input];

        uint result = NativeInput.SendInput(
            1,
            inputs,
            Marshal.SizeOf<NativeInput.INPUT>());

        if (result == 0)
        {
            throw new Win32Exception(
                Marshal.GetLastWin32Error());
        }
    }
}