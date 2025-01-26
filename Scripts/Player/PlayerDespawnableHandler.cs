using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class PlayerDespawnableHandler : Area2D
{
    public static PlayerDespawnableHandler Instance;

    [Export]
    private CollisionShape2D collisionShape;

    private Node2D parent;
	private List<IWorldDespawnableNode> despawnables = new();

    private float squaredInteractableRange;

    public PlayerDespawnableHandler()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        parent = GetParent<Node2D>();

        float circleRadius = (collisionShape.Shape as CircleShape2D).Radius;
        squaredInteractableRange = new Vector2(circleRadius, circleRadius).DistanceSquaredTo(Vector2.Zero);
    }

    private void OnAreaEntered(Area2D area) {
        if(area is not IWorldDespawnableNode interactable)
        {
            return;
        }

        despawnables.Add(interactable);
    }

    private void OnAreaExited(Area2D area) {
        if (area is not IWorldDespawnableNode interactable || !despawnables.Contains(interactable) || area.IsQueuedForDeletion())
        {
            return;
        }

        despawnables.Remove(interactable);
        ServiceLocator.GameNotificationService.OnNodeDespawned.Execute(area);
        area.QueueFree();
    }

    public bool IsPointInArea(Vector2 point)
    {
        return parent.GlobalPosition.DistanceSquaredTo(point) <= squaredInteractableRange;
    }
}
