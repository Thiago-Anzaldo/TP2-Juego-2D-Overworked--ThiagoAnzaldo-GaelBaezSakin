using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonoGameLibrary.Input;

public class MouseInfo
{
    public MouseState PreviousState { get; private set; }
    public MouseState CurrentState { get; private set; }

    public Point Position
    {
        get => CurrentState.Position;
        set => SetPosition(value.X, value.Y);
    }

    public int X
    {
        get => CurrentState.X;
        set => SetPosition(value, CurrentState.Y);
    }

    public int Y
    {
        get => CurrentState.Y;
        set => SetPosition(CurrentState.X, value);
    }

    public Point PositionDelta => CurrentState.Position - PreviousState.Position;
    public int XDelta => CurrentState.X - PreviousState.X;
    public int YDelta => CurrentState.Y - PreviousState.Y;
    public bool WasMoved => PositionDelta != Point.Zero;

    public int ScrollWheel => CurrentState.ScrollWheelValue;
    public int ScrollWheelDelta => CurrentState.ScrollWheelValue - PreviousState.ScrollWheelValue;

    public MouseInfo()
    {
        PreviousState = new MouseState();
        CurrentState = Mouse.GetState();
    }

    public void Update()
    {
        PreviousState = CurrentState;
        CurrentState = Mouse.GetState();
    }

    // Traduce nuestro enum al botón correspondiente de un MouseState
    private static ButtonState Estado(MouseState state, MouseButton button) => button switch
    {
        MouseButton.Left => state.LeftButton,
        MouseButton.Middle => state.MiddleButton,
        MouseButton.Right => state.RightButton,
        MouseButton.XButton1 => state.XButton1,
        MouseButton.XButton2 => state.XButton2,
        _ => ButtonState.Released
    };

    public bool IsButtonDown(MouseButton button) => Estado(CurrentState, button) == ButtonState.Pressed;
    public bool IsButtonUp(MouseButton button) => Estado(CurrentState, button) == ButtonState.Released;

    public bool WasButtonJustPressed(MouseButton button)
        => Estado(CurrentState, button) == ButtonState.Pressed && Estado(PreviousState, button) == ButtonState.Released;

    public bool WasButtonJustReleased(MouseButton button)
        => Estado(CurrentState, button) == ButtonState.Released && Estado(PreviousState, button) == ButtonState.Pressed;

    public void SetPosition(int x, int y)
    {
        Mouse.SetPosition(x, y);
        CurrentState = new MouseState(
            x, y,
            CurrentState.ScrollWheelValue,
            CurrentState.LeftButton,
            CurrentState.MiddleButton,
            CurrentState.RightButton,
            CurrentState.XButton1,
            CurrentState.XButton2);
    }
}