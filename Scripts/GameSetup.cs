using Godot;
using Godot.Collections;

[GlobalClass]
public partial class GameSetup : Node
{

    [Export]
    private PackedScene cameraScene;

    [Export]
    private PackedScene characterScene;

    [Export]
    private Array<InventoryItemDefinition> itemsToGive;

    public override void _Ready()
    {
        GroupUtility.SetTreeRoot(this);

        Input.MouseMode = Input.MouseModeEnum.Hidden;

        PlayerCamera camera = cameraScene.Instantiate<PlayerCamera>();
        PlayerCharacter character = characterScene.Instantiate<PlayerCharacter>();

        AddChild(camera);
        GroupUtility.GetNodeFromGroup<IWorldMap>(GroupUtility.GROUP_WORLD_MAP).AddWorldNode(character, true, Vector3.Zero);

        camera.toFollow = character;

        foreach (InventoryItemDefinition item in itemsToGive)
        {
            ServiceLocator.InventoryService.AddItem(item, 1);
        }
    }
}