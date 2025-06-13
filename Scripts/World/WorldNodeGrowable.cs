using Godot;
using Godot.Collections;
using static WorldMap;

[GlobalClass]
public partial class WorldNodeGrowable : Area3D, IWorldGridNode, IWorldDespawnableNode, IWorldSaveable, IWorldTimeListener
{
    private const string SAVE_KEY_START_GROWING = "Start";

    [Export]
    private int daysRequired = 5;

    [Export]
    private PackedScene onFinishedGrowingScene;

    [Export]
    private Node3D placementPreviewNode;

    [Export(hintString: "Defines which tiles this item will occupy")]
    public Array<Vector2I> placementTiles;

    [Export(hintString: "Defines on which tiles the item can be placed")]
    public Array<AtlasMaterial> validTilePlacementMaterials;

    private int startedGrowingOn;

    public Node3D placementPreview => placementPreviewNode;

    public Vector3 gridPlacementOffset => placementPreviewNode.GlobalPosition - GlobalPosition;

    public Array<Vector2I> gridOffsetPositions => placementTiles;

    public Array<AtlasMaterial> placementMaterials => validTilePlacementMaterials;

    public override void _Ready()
    {
        startedGrowingOn = ServiceLocator.TimeService.currentDay;
    }

    public Node3D GetNode() => this;

    public void SetTime(int day)
    {
        if(day - startedGrowingOn < daysRequired)
        {
            return;
        }

        ServiceLocator.GameNotificationService.OnNodeDestroyed.Execute(this);
        QueueFree();

        Node3D grownNode = onFinishedGrowingScene.Instantiate<Node3D>();
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

