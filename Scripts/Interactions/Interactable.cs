using Godot;

[GlobalClass]
public partial class Interactable : Area2D
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

    private int interactCountRemaining;

    public override void _Ready()
    {
        interactCountRemaining = interactCount;
    }

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
            interactShape.Disabled = true;

            foreach (var interactEvent in interactCompleteEvents)
            {
                interactEvent.Execute();
            }
        }
    }

    public Vector2 GetArrowAnchor() => selectArrowAnchor.GlobalPosition;

    public InteractResult GetInteractResult(InventoryItemDefinition inventoryItemDefinition)
    {
        if(interactCountRemaining <= 0)
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
}

