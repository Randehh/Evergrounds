using Godot;

[GlobalClass]
public partial class CursorUI : Control
{
    [Export]
    private Control mouseParent;

    [Export]
    private TextureRect cursorTextureRect;

    [Export]
    private Texture2D cursorTexture;

    [Export]
    private Texture2D cursorTexturePressed;

    public override void _Ready()
    {
        cursorTextureRect.Texture = cursorTexture;
    }

    public override void _Process(double delta)
    {
        mouseParent.Position = GetLocalMousePosition();
        cursorTextureRect.Texture = Input.IsMouseButtonPressed(MouseButton.Left) ? cursorTexturePressed : cursorTexture;
    }
}