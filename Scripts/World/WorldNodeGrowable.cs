using Godot;
using Godot.Collections;
using static WorldMap;

[GlobalClass]
public partial class WorldNodeGrowable : Area2D, IWorldGridNode, IWorldDespawnableNode, IWorldSaveable, IWorldTimeListener
{
    private const string SAVE_KEY_START_GROWING = "Start";

    [Export]
    private int daysRequired = 5;

    [Export]
    private PackedScene onFinishedGrowingScene;

    [Export]
    private Sprite2D mainSprite;

    [Export(hintString: "Defines which tiles this item will occupy")]
    public Array<Vector2I> placementTiles = new() { Vector2I.Zero };

    [Export(hintString: "Defines on which tiles the item can be placed")]
    public Array<AtlasMaterial> validTilePlacementMaterials = new();

    private int startedGrowingOn;

    public Sprite2D gridPlacementPreviewSprite => mainSprite;

    public Vector2 gridPlacementOffset => (gridPlacementPreviewSprite.GlobalPosition - GlobalPosition) + gridPlacementPreviewSprite.Offset;

    public Array<Vector2I> gridOffsetPositions => placementTiles;

    public Array<AtlasMaterial> placementMaterials => validTilePlacementMaterials;

    public override void _Ready()
    {
        startedGrowingOn = ServiceLocator.TimeService.currentDay;
    }

    public Node2D GetNode() => this;

    public void SetTime(int day)
    {
        if(day - startedGrowingOn < daysRequired)
        {
            return;
        }

        ServiceLocator.GameNotificationService.OnNodeDestroyed.Execute(this);
        QueueFree();

        Node2D grownNode = onFinishedGrowingScene.Instantiate<Node2D>();
        WorldMap.Instance.AddWorldNode(grownNode, true, Position);
    }

    public Dictionary<string, Variant> GetSaveData()
    {
        return new Dictionary<string, Variant>
        {
            [SAVE_KEY_START_GROWING] = startedGrowingOn
        };
    }

    public void SetSaveData(Dictionary<string, Variant> data)
    {
        startedGrowingOn = data[SAVE_KEY_START_GROWING].AsInt32();
    }
}

