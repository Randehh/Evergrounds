using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class PlayerDespawnableHandler : Area3D
{
    public static PlayerDespawnableHandler Instance;

    [Export]
    private CollisionShape3D collisionShape;

    private Node3D parent;
	private List<IWorldDespawnableNode> despawnables = new();

    private float squaredInteractableRange;

    public PlayerDespawnableHandler()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        parent = GetParent<Node3D>();

        float circleRadius = (collisionShape.Shape as SphereShape3D).Radius;
        squaredInteractableRange = new Vector2(circleRadius, circleRadius).DistanceSquaredTo(Vector2.Zero);
    }

    private void OnAreaEntered(Area3D area) {
        if(area is not IWorldDespawnableNode interactable)
        {
            return;
        }

        despawnables.Add(interactable);
    }

    private void OnAreaExited(Area3D area) {
        if (area is not IWorldDespawnableNode interactable || !despawnables.Contains(interactable) || area.IsQueuedForDeletion())
        {
            return;
        }

        despawnables.Remove(interactable);
        ServiceLocator.GameNotificationService.OnNodeDespawned.Execute(area);
        area.QueueFree();
    }

    public bool IsPointInArea(Vector3 point)
    {
        return parent.GlobalPosition.DistanceSquaredTo(point) <= squaredInteractableRange;
    }
}
