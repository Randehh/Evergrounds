using Godot;

[GlobalClass]
public partial class WorldItem : Area2D
{

    private const float VACUUM_SPEED = 15;

    public bool CanBeVacuumed => currentTime >= 1f;

    [Export]
    private Sprite2D sprite;

    [Export]
    private Node2D floatParent;

    [Export]
    private WorldNodeShadow shadow;

    private WorldItemState itemState = WorldItemState.FLOATING;

    private InventoryItemDefinition definition;
    private int stackSize = 1;
    private double currentTime;
    private Vector2 velocity;
    private float spawnVelocityY;
    private PlayerCharacter vacuumCharacter;

    public void Initialize(InventoryItemDefinition definition, int stackSize = 1)
    {
        this.definition = definition;
        this.stackSize = stackSize;

        sprite.Texture = definition.itemSprite;

        shadow.SetBaseScale(definition.worldShadowScale);
    }

    public override void _Process(double delta)
    {
        currentTime += delta;

        switch (itemState)
        {
            case WorldItemState.SPAWNING:
                Position += velocity;
                velocity *= (float)Mathf.Lerp((float)delta, 1, 0.95f);
                if(velocity.Length() < 0.05f)
                {
                    itemState = WorldItemState.FLOATING;
                }
                break;

            case WorldItemState.FLOATING:
                break;

            case WorldItemState.VACUUMING:
                GlobalPosition = GlobalPosition.Lerp(vacuumCharacter.GlobalPosition, VACUUM_SPEED * (float)delta);
                if(GlobalPosition.DistanceTo(vacuumCharacter.GlobalPosition) < 3.5f) {
                    Inventory.Instance.AddItem(definition, stackSize);
                    QueueFree();
                }
                return;
        }

        floatParent.Position = new Vector2(0, Mathf.Sin((float)currentTime * 3));
        if(sprite.Position.Y < 0 || spawnVelocityY < 0)
        {
            spawnVelocityY += (float)delta * 5;
            sprite.Position = new Vector2(sprite.Position.X, Mathf.Min(0, sprite.Position.Y + spawnVelocityY));
        }
    }

    public void Spawn()
    {
        float randX = (GD.Randf() + 0.2f) * 0.6f * (GD.Randf() > 0.5f ? -1 : 1);
        float randY = (GD.Randf() + 0.2f) * 0.6f * (GD.Randf() > 0.5f ? -1 : 1);
        velocity = new Vector2(randX, randY);

        spawnVelocityY = ((GD.Randf() * 0.5f) + 0.5f) * -1.5f;

        itemState = WorldItemState.SPAWNING;
    }

    public void Vacuum(PlayerCharacter character)
    {
        vacuumCharacter = character;

        itemState = WorldItemState.VACUUMING;
    }

    private enum WorldItemState
    {
        SPAWNING,
        FLOATING,
        VACUUMING
    }
}