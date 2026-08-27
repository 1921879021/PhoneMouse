using PhoneMouse.Core.Native;

namespace PhoneMouse.Core.Input;

public sealed class WindowsKeyboardController :
    IKeyboardController
{
    public void TypeText(
        string text)
    {
        string normalized =
            NormalizeForCurrentWindow(
                text);


        NativeKeyboardInput
            .SendUnicodeText(
                normalized);
    }


    public void TypeTextAndEnter(
        string text)
    {
        string normalized =
            NormalizeForCurrentWindow(
                text);


        NativeKeyboardInput
            .SendUnicodeText(
                normalized);


        NativeKeyboardInput
            .SendEnter();
    }


    public void TypeTextAndCtrlEnter(
        string text)
    {
        string normalized =
            NormalizeForCurrentWindow(
                text);


        NativeKeyboardInput
            .SendUnicodeText(
                normalized);


        NativeKeyboardInput
            .SendCtrlEnter();
    }


    public void PressEnter()
    {
        NativeKeyboardInput
            .SendEnter();
    }


    public void PressCtrlEnter()
    {
        NativeKeyboardInput
            .SendCtrlEnter();
    }


    private static string NormalizeForCurrentWindow(
        string text)
    {
        return text
            .Replace(
                "\r\n",
                " ")
            .Replace(
                '\r',
                ' ')
            .Replace(
                '\n',
                ' ');
    }
}
