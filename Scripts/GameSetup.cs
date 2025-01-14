using Godot;

[GlobalClass]
public partial class GameSetup : Node
{

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Hidden;
    }
}