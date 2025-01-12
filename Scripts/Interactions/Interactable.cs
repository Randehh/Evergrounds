using Godot;

[GlobalClass]
public partial class Interactable : Area2D
{
    [Export]
    private Node2D interactPoint;

    [Export]
    private Node2D selectArrowAnchor;

    [Export]
    private InteractableRequirement interactRequirement;

    [Export]
    private InteractEvent[] interactEvents;

    public void Interact()
    {
        foreach (var interactEvent in interactEvents)
        {
            interactEvent.Execute();
        }
    }

    public Vector2 GetArrowAnchor() => selectArrowAnchor.GlobalPosition;

    public InteractResult GetInteractResult(InventoryItemDefinition inventoryItemDefinition)
    {
        if(inventoryItemDefinition == null && interactRequirement.interactType != InteractType.ANY)
        {
            return InteractResult.FAULTY_TYPE;
        }

        if(interactRequirement.interactType == InteractType.ANY )
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

