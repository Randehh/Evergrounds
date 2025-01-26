using Godot;

[GlobalClass]
public partial class GameSetup : Node
{
    [Export]
    private WorldMap map;

    [Export]
    private PackedScene cameraScene;

    [Export]
    private PackedScene characterScene;

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Hidden;

        PlayerCamera camera = cameraScene.Instantiate<PlayerCamera>();
        PlayerCharacter character = characterScene.Instantiate<PlayerCharacter>();

        AddChild(camera);
        map.AddWorldNode(character, true, Vector2.Zero);

        camera.toFollow = character;
    }
}