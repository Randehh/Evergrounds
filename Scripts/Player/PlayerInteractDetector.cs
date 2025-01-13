using Godot;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class PlayerInteractDetector : Area2D
{
    public Interactable SelectedInteractable => IsInstanceValid(selectedInteractable) ? selectedInteractable : null;
    public bool IsInRange { get; private set; }

    [Export]
    private float interactRadius = 25;

    private Node2D parent;
    private Interactable selectedInteractable;
	private List<Interactable> interactables = new();

    public override void _Ready()
    {
        parent = GetParent<Node2D>();
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition = GetGlobalMousePosition();

        if (interactables.Count == 0)
        {
            selectedInteractable = null;
            return;
        }

        Vector2 position = GlobalPosition;
        int closestIndex = -1;
        float closestDistance = float.MaxValue;
        for (int i = interactables.Count - 1; i >= 0; i--)
        {
            Interactable interactable = interactables[i];
            if(interactable == null || interactable.IsQueuedForDeletion() || !IsInstanceValid(interactable))
            {
                interactables.RemoveAt(i);
                continue;
            }

            float distance = position.DistanceSquaredTo(interactable.GlobalPosition);
            if(distance < closestDistance )
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        if(closestIndex == -1)
        {
            selectedInteractable = null;
            return;
        }

        selectedInteractable = interactables[closestIndex];
        IsInRange = SelectedInteractable.GlobalPosition.DistanceTo(parent.GlobalPosition) <= interactRadius;
    }

    private void OnAreaEntered(Area2D area) {
        if(area is not Interactable interactable)
        {
            return;
        }

        interactables.Add(interactable);
    }

    private void OnAreaExited(Area2D area) {
        if (area is not Interactable interactable || !interactables.Contains(interactable))
        {
            return;
        }

        interactables.Remove(interactable);
    }
}
