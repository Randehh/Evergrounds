using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Interactable : Area2D, IWorldGridNode, IWorldDespawnableNode, IWorldSaveable
{
    [Export]
    private Node2D interactPoint;

    [Export]
    private CollisionShape2D interactShape;

    [Export]
    private Node2D selectArrowAnchor;

    [Export]
    private InteractableRequirement interactRequirement;

    [Export]
    private InteractEvent[] interactEvents;

    [Export]
    private InteractEvent[] interactCompleteEvents;

    [Export]
    private int interactCount = 1;

    [Export]
    private Sprite2D mainSprite;

    private int interactCountRemaining;

    public Sprite2D gridPlacementPreviewSprite => mainSprite;

    public Vector2 gridPlacementOffset => (gridPlacementPreviewSprite.GlobalPosition - GlobalPosition) + gridPlacementPreviewSprite.Offset;

    public override void _Ready()
    {
        interactCountRemaining = interactCount;
    }

    public Node2D GetNode() => this;

    public void Interact()
    {
        if(interactCountRemaining <= 0)
        {
            return;
        }

        foreach (var interactEvent in interactEvents)
        {
            interactEvent.Execute();
        }

        interactCountRemaining--;

        if (interactCountRemaining <= 0)
        {
            SetCollisionLayerValue(1, false);

            foreach (var interactEvent in interactCompleteEvents)
            {
                interactEvent.Execute();
            }
        }
    }

    public Vector2 GetArrowAnchor() => selectArrowAnchor.GlobalPosition;

    public InteractResult GetInteractResult(InventoryItem inventoryItem)
    {
        var inventoryItemDefinition = inventoryItem?.definition;

        if (interactCountRemaining <= 0)
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
            [nameof(interactCountRemaining)] = interactCountRemaining
        };
    }
}

