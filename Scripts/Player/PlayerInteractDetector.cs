using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class PlayerInteractDetector : Area3D
{
    public Interactable SelectedInteractable => IsInstanceValid(selectedInteractable) ? selectedInteractable : null;
    public bool IsInRange { get; private set; }

    private Node3D parent;
    private Interactable selectedInteractable;
	private List<Interactable> interactables = new();

    public override void _Ready()
    {
        parent = GetParent<Node3D>();
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition = PlayerCamera.Instance.GetGroundRaycastPosition();

        if (interactables.Count == 0)
        {
            selectedInteractable = null;
            return;
        }

        Vector3 position = GlobalPosition;
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
        IsInRange = SelectedInteractable.GlobalPosition.DistanceTo(parent.GlobalPosition) <= PlayerInteractHandler.INTERACT_RADIUS;
    }

    private void OnAreaEntered(Area3D area) {
        if(area is not Interactable interactable)
        {
            return;
        }

        interactables.Add(interactable);
        GD.Print("Interactable entered");
    }

    private void OnAreaExited(Area3D area) {
        if (area is not Interactable interactable || !interactables.Contains(interactable))
        {
            return;
        }

        interactables.Remove(interactable);
        GD.Print("Interactable exited");
    }
}
