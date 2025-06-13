using Godot;
using Godot.Collections;
using System;

[GlobalClass]
public partial class WorldItem : Area3D, IWorldDespawnableNode, IWorldSaveable
{
    private const string SAVE_KEY_ITEM_DEFINITION_PATH = "ItemDefinitionPath";
    private const string SAVE_KEY_STACK_SIZE = "StackSize";

    private const float VACUUM_SPEED = 15f;

    public bool CanBeVacuumed => currentTime >= 1.5f;

    [Export]
    private Sprite3D itemVisual;

    [Export]
    private Node3D floatParent;

    private WorldItemState itemState = WorldItemState.FLOATING;

    private InventoryItemDefinition definition;
    private int stackSize = 1;
    private double currentTime;
    private Vector3 velocity;
    private PlayerCharacter vacuumCharacter;

    public void Initialize(InventoryItemDefinition definition, int stackSize = 1)
    {
        this.definition = definition;
        this.stackSize = stackSize;

        itemVisual.Texture = definition.itemSprite;

        currentTime = GD.Randf();
    }

    public override void _Process(double delta)
    {
        currentTime += delta;

        switch (itemState)
        {
            case WorldItemState.SPAWNING:
                Position += velocity;
                velocity *= (float)Mathf.Lerp((float)delta, 1, 0.98f);
                if(velocity.Length() < 0.001f)
                {
                    itemState = WorldItemState.FLOATING;
                }
                break;

            case WorldItemState.FLOATING:
                break;

            case WorldItemState.VACUUMING:
                GlobalPosition = GlobalPosition.Lerp(vacuumCharacter.GlobalPosition, VACUUM_SPEED * (float)delta);
                if(GlobalPosition.DistanceTo(vacuumCharacter.GlobalPosition) < 0.05f) {
                    ServiceLocator.InventoryService.AddItem(definition, stackSize);
                    ServiceLocator.GameNotificationService.OnNodeDestroyed.Execute(this);
                    QueueFree();
                }
                return;
        }

        floatParent.Position = new Vector3(0, Mathf.Sin((float)currentTime * 3) * 0.1f, 0);
        Position = new Vector3(Position.X, Mathf.Min(0, Position.Y), Position.Z);
    }

    public void Spawn()
    {
        float randX = (GD.Randf() + 0.2f) * (GD.Randf() > 0.5f ? -1 : 1);
        float randZ = (GD.Randf() + 0.2f) * (GD.Randf() > 0.5f ? -1 : 1);
        velocity = new Vector3(randX, 0, randZ) * 0.01f;

        itemState = WorldItemState.SPAWNING;
    }

    public void Vacuum(PlayerCharacter character)
    {
        vacuumCharacter = character;

        itemState = WorldItemState.VACUUMING;
    }

    public Node3D GetNode()
    {
        return this;
    }

    public Dictionary<string, Variant> GetSaveData()
    {
        Dictionary<string, Variant> data = new();
        data.Add(SAVE_KEY_STACK_SIZE, stackSize);
        data.Add(SAVE_KEY_ITEM_DEFINITION_PATH, definition.ResourcePath);
        return data;
    }

    public void SetSaveData(Dictionary<string, Variant> data)
    {
        InventoryItemDefinition definition = GD.Load<InventoryItemDefinition>(data[SAVE_KEY_ITEM_DEFINITION_PATH].AsString());
        Initialize(definition, data[SAVE_KEY_STACK_SIZE].AsInt32());
    }

    private enum WorldItemState
    {
        SPAWNING,
        FLOATING,
        VACUUMING
    }
}