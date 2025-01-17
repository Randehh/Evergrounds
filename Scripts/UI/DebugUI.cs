using Godot;

[GlobalClass]
public partial class DebugUI : Control
{
    [Export]
    private Label fpsLabel;

    public override void _Process(double delta)
    {
        fpsLabel.Text = $"FPS: {Engine.GetFramesPerSecond()}";
    }
}