using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class PlayerDespawnableHandler : Area2D
{
    private Node2D parent;
	private List<IWorldDespawnableNode> despawnables = new();

    private float squaredInteractableRange;

    public override void _Ready()
    {
        parent = GetParent<Node2D>();
    }

    private void OnAreaEntered(Area2D area) {
        if(area is not IWorldDespawnableNode interactable)
        {
            return;
        }

        despawnables.Add(interactable);
    }

    private void OnAreaExited(Area2D area) {
        if (area is not IWorldDespawnableNode interactable || !despawnables.Contains(interactable))
        {
            return;
        }

        despawnables.Remove(interactable);
        area.QueueFree();
    }
}
