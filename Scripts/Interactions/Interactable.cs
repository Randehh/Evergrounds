using Godot;
using Godot.Collections;
using static WorldMap;

[GlobalClass]
public partial class Interactable : Area3D, IWorldGridNode, IWorldDespawnableNode, IWorldSaveable
{
    private const string SAVE_KEY_INTERACTABLE_COUNT = "InteractableCount";

    [Export]
    private Node3D interactPoint;

    [Export]
    private CollisionShape2D interactShape;

    [Export]
    private Node3D selectArrowAnchor;

    [Export]
    private string selectArrowText = string.Empty;

    [Export]
    private InteractableRequirement interactRequirement;

    [Export]
    private InteractEvent[] interactEvents;

    [Export]
    private InteractEvent[] interactCompleteEvents;

    [Export]
    private int interactCount = 1;

    [Export]
    private Node3D previewNode;

    [Export(hintString: "Defines which tiles this item will occupy")]
    public Array<Vector2I> placementTiles;

    [Export(hintString: "Defines on which tiles the item can be placed")]
    public Array<AtlasMaterial> validTilePlacementMaterials;

    private int interactCountRemaining;

    public Node3D placementPreview => previewNode;

    public Vector3 gridPlacementOffset => placementPreview.GlobalPosition - GlobalPosition;

    public Array<Vector2I> gridOffsetPositions => placementTiles;

    public Array<AtlasMaterial> placementMaterials => validTilePlacementMaterials;

    public override void _Ready()
    {
        interactCountRemaining = interactCount;
    }

    public Node3D GetNode() => this;

    public void Interact()
    {
        if(interactCountRemaining == 0)
        {
            return;
        }

        foreach (var interactEvent in interactEvents)
        {
            interactEvent.Execute();
        }

        if (interactCountRemaining != -1)
        {
            interactCountRemaining--;
        }

        if (interactCountRemaining <= 0)
        {
            if (interactCountRemaining != -1)
            {
                SetCollisionLayerValue(1, false);
            }

            foreach (var interactEvent in interactCompleteEvents)
            {
                interactEvent.Execute();
            }
        }
    }

    public Vector3 GetArrowAnchor() => selectArrowAnchor.GlobalPosition;
    public string GetArrowText() => selectArrowText;

    public InteractResult GetInteractResult(InventoryItem inventoryItem)
    {
        var inventoryItemDefinition = inventoryItem?.definition;

        if (interactCountRemaining == 0)
        {
            return InteractResult.NO_INTERACTABLE;
        }

        if(inventoryItemDefinition == null && interactRequirement.interactType != InteractType.NONE)
        {
            return InteractResult.FAULTY_TYPE;
        }

        if(interactRequirement.interactType == InteractType.NONE )
        {
            return InteractResult.OK;
        }

        if(interactRequirement.interactType != inventoryItemDefinition.interactType)
        {
            return InteractResult.FAULTY_TYPE;
        }

        if(interactRequirement.minimumLevel > inventoryItemDefinition.interactLevel)
        {
            return InteractResult.LEVEL_TOO_LOW;
        }

        return InteractResult.OK;
    }

    public Dictionary<string, Variant> GetSaveData()
    {
        return new Dictionary<string, Variant>
        {
            [SAVE_KEY_INTERACTABLE_COUNT] = interactCountRemaining
        };
    }

    public void SetSaveData(Dictionary<string, Variant> data)
    {
        interactCountRemaining = data[SAVE_KEY_INTERACTABLE_COUNT].AsInt32();
        SetCollisionLayerValue(1, interactCountRemaining != 0);
    }
}

