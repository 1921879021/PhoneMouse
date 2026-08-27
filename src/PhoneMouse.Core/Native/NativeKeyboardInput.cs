using System.Runtime.InteropServices;

namespace PhoneMouse.Core.Native;

/// <summary>
/// Windows SendInput 键盘注入。
///
/// 重要：INPUT 的 union 必须包含 MOUSEINPUT / KEYBDINPUT / HARDWAREINPUT，
/// 这样在 x64 Windows 上 Marshal.SizeOf&lt;INPUT&gt;() 才会得到 Win32
/// SendInput 要求的正确结构大小（40 bytes）。
///
/// 如果 union 只定义 KEYBDINPUT，cbSize 会过小，SendInput 会返回 0，
/// GetLastWin32Error() == 87 (ERROR_INVALID_PARAMETER)。
/// </summary>
internal static class NativeKeyboardInput
{
    private const uint INPUT_KEYBOARD =
        1;

    private const uint KEYEVENTF_KEYUP =
        0x0002;

    private const uint KEYEVENTF_UNICODE =
        0x0004;

    private const ushort VK_RETURN =
        0x0D;


    private const ushort VK_CONTROL =
        0x11;


    [DllImport(
        "user32.dll",
        SetLastError = true)]
    private static extern uint SendInput(
        uint numberOfInputs,
        INPUT[] inputs,
        int sizeOfInput);


    internal static void SendUnicodeText(
        string text)
    {
        if (
            string.IsNullOrEmpty(
                text))
        {
            return;
        }


        List<INPUT> inputs =
            new(
                text.Length * 2);


        foreach (
            char character
            in text)
        {
            inputs.Add(
                CreateUnicodeInput(
                    character,
                    keyUp: false));


            inputs.Add(
                CreateUnicodeInput(
                    character,
                    keyUp: true));
        }


        SendInChunks(
            inputs);
    }


    internal static void SendEnter()
    {
        INPUT[] inputs =
        [
            CreateVirtualKeyInput(
                VK_RETURN,
                keyUp: false),

            CreateVirtualKeyInput(
                VK_RETURN,
                keyUp: true)
        ];


        SendInputChecked(
            inputs);
    }


    internal static void SendCtrlEnter()
    {
        INPUT[] inputs =
        [
            CreateVirtualKeyInput(
                VK_CONTROL,
                keyUp: false),

            CreateVirtualKeyInput(
                VK_RETURN,
                keyUp: false),

            CreateVirtualKeyInput(
                VK_RETURN,
                keyUp: true),

            CreateVirtualKeyInput(
                VK_CONTROL,
                keyUp: true)
        ];


        SendInputChecked(
            inputs);
    }


    private static INPUT CreateUnicodeInput(
        char character,
        bool keyUp)
    {
        return
            new INPUT
            {
                type =
                    INPUT_KEYBOARD,

                U =
                    new InputUnion
                    {
                        ki =
                            new KEYBDINPUT
                            {
                                wVk =
                                    0,

                                wScan =
                                    character,

                                dwFlags =
                                    KEYEVENTF_UNICODE |
                                    (
                                        keyUp
                                            ? KEYEVENTF_KEYUP
                                            : 0
                                    ),

                                time =
                                    0,

                                dwExtraInfo =
                                    UIntPtr.Zero
                            }
                    }
            };
    }


    private static INPUT CreateVirtualKeyInput(
        ushort virtualKey,
        bool keyUp)
    {
        return
            new INPUT
            {
                type =
                    INPUT_KEYBOARD,

                U =
                    new InputUnion
                    {
                        ki =
                            new KEYBDINPUT
                            {
                                wVk =
                                    virtualKey,

                                wScan =
                                    0,

                                dwFlags =
                                    keyUp
                                        ? KEYEVENTF_KEYUP
                                        : 0,

                                time =
                                    0,

                                dwExtraInfo =
                                    UIntPtr.Zero
                            }
                    }
            };
    }


    private static void SendInChunks(
        List<INPUT> inputs)
    {
        const int chunkSize =
            512;


        for (
            int offset = 0;
            offset < inputs.Count;
            offset += chunkSize)
        {
            int count =
                Math.Min(
                    chunkSize,
                    inputs.Count - offset);


            INPUT[] chunk =
                inputs
                    .GetRange(
                        offset,
                        count)
                    .ToArray();


            SendInputChecked(
                chunk);
        }
    }


    private static void SendInputChecked(
        INPUT[] inputs)
    {
        if (
            inputs.Length == 0)
        {
            return;
        }


        int inputSize =
            Marshal.SizeOf<INPUT>();


        uint sent =
            SendInput(
                (uint)inputs.Length,
                inputs,
                inputSize);


        if (
            sent !=
                (uint)inputs.Length)
        {
            int error =
                Marshal.GetLastWin32Error();


            throw new InvalidOperationException(
                $"SendInput 只发送了 {sent}/{inputs.Length} 个键盘事件。Win32={error}，INPUT_SIZE={inputSize}");
        }
    }


    // Win32 INPUT:
    // https://learn.microsoft.com/windows/win32/api/winuser/ns-winuser-input
    //
    // x64:
    // type 4 bytes + padding 4 bytes + union 32 bytes = 40 bytes.
    [StructLayout(
        LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;

        public InputUnion U;
    }


    [StructLayout(
        LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        public MOUSEINPUT mi;

        [FieldOffset(0)]
        public KEYBDINPUT ki;

        [FieldOffset(0)]
        public HARDWAREINPUT hi;
    }


    [StructLayout(
        LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx;

        public int dy;

        public uint mouseData;

        public uint dwFlags;

        public uint time;

        public UIntPtr dwExtraInfo;
    }


    [StructLayout(
        LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;

        public ushort wScan;

        public uint dwFlags;

        public uint time;

        public UIntPtr dwExtraInfo;
    }


    [StructLayout(
        LayoutKind.Sequential)]
    private struct HARDWAREINPUT
    {
        public uint uMsg;

        public ushort wParamL;

        public ushort wParamH;
    }
}
