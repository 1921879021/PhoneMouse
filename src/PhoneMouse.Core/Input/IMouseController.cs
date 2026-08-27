namespace PhoneMouse.Core.Input;

public interface IMouseController
{
    void MoveRelative(int dx, int dy);

    void LeftClick();

    void RightClick();

    void DoubleClick();

    void LeftDown();

    void LeftUp();

    void Scroll(int delta);
}