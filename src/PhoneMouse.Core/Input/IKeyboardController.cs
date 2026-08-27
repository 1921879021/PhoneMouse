namespace PhoneMouse.Core.Input;

public interface IKeyboardController
{
    void TypeText(string text);

    void TypeTextAndEnter(string text);

    void TypeTextAndCtrlEnter(string text);

    void PressEnter();

    void PressCtrlEnter();
}
