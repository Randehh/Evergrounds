using Godot;
using Godot.Collections;

[GlobalClass]
public partial class GameSetup : Node
{
    [Export]
    private WorldMap map;

    [Export]
    private PackedScene cameraScene;

    [Export]
    private PackedScene characterScene;

    [Export]
    private PackedScene petScene;

    [Export]
    private Array<InventoryItemDefinition> itemsToGive = new();

    [Export]
    private bool giveItems = true;

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Hidden;

        PlayerCamera camera = cameraScene.Instantiate<PlayerCamera>();
        PlayerCharacter character = characterScene.Instantiate<PlayerCharacter>();
        Pet pet = petScene.Instantiate<Pet>();

        AddChild(camera);
        map.AddWorldNode(character, true, Vector2.Zero);
        map.AddWorldNode(pet, true, Vector2.Zero);

        camera.toFollow = character;
        pet.SetFollowTarget(character);

        if (giveItems) {
            foreach (InventoryItemDefinition item in itemsToGive) {
                ServiceLocator.InventoryService.AddItem(item, 1);
            }
        }
    }
}