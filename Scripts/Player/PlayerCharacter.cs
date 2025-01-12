using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class PlayerCharacter : Node2D
{
    private readonly Color colorWhite = new Color(1, 1, 1, 1);
    private readonly Color colorHalfAlpha = new Color(1, 1, 1, 0.5f);

    [Export]
	private CharacterBase character;

	[Export]
	private Node2D selectArrow;

	[Export]
	private PlayerInteractDetector interactDetector;

	[Export]
	private Area2D vacuumArea;

	[Export]
	private float maxSpeed = 1;

	[Export]
	private float acceleration = 10;

	[Export]
	private float decceleration = 0.01f;

	private Vector2 currentSpeed = Vector2.Zero;
	private Interactable currentInteracable;
	private InteractResult currentInteractResult;

	private List<WorldItem> vacuumItemsInRadius = new();

	public override void _Ready()
	{
		Inventory.Instance.OnSelectQuickslot += OnItemSelected;
		(selectArrow.FindChild("ArrowAnimator") as AnimationPlayer).Play("Highlight");

		OnItemSelected(0);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Vector2 input = new Vector2(
			Input.GetAxis("move_left", "move_right"),
			Input.GetAxis("move_up", "move_down")
			).Normalized();

		currentSpeed += input * acceleration * (float)delta;

		if (input.X < 0.05f && input.X > -0.05f)
		{
			currentSpeed.X *= decceleration * (float)delta;
		}

		if (input.Y < 0.05f && input.Y > -0.05f)
		{
			currentSpeed.Y *= decceleration * (float)delta;
		}

		currentSpeed.X = Mathf.Clamp(currentSpeed.X, -maxSpeed, maxSpeed);
		currentSpeed.Y = Mathf.Clamp(currentSpeed.Y, -maxSpeed, maxSpeed);

		Position += currentSpeed;

		ProcessSelection();

		if (Input.IsActionJustPressed("use_holdable"))
		{
			if (currentInteractResult == InteractResult.OK)
			{
				character.UseHoldable();
				interactDetector.SelectedInteractable.Interact();
			}
			else if (currentInteractResult == InteractResult.NO_INTERACTABLE)
			{
				character.UseHoldable();
			}
		}
	}

    public override void _PhysicsProcess(double delta)
    {
        for (int i = vacuumItemsInRadius.Count - 1; i >= 0; i--)
        {
			WorldItem item = vacuumItemsInRadius[i];
            if (!item.CanBeVacuumed)
            {
				continue;
            }

			item.Vacuum(this);
			vacuumItemsInRadius.RemoveAt(i);
        }
    }

    private void ProcessSelection()
	{
		Interactable currentInteracable = interactDetector.SelectedInteractable;

		if (currentInteracable == null)
		{
			selectArrow.Visible = false;
			currentInteractResult = InteractResult.NO_INTERACTABLE;
            return;
		}

		if (interactDetector.IsInRange)
		{
			selectArrow.Modulate = colorWhite;
			currentInteractResult = currentInteracable.GetInteractResult(character.CurrentlyHolding);
		}
		else
		{
			selectArrow.Modulate = colorHalfAlpha;
			currentInteractResult = InteractResult.OUT_OF_RANGE;
		}

		selectArrow.GlobalPosition = currentInteracable.GetArrowAnchor();
		selectArrow.Visible = true;
	}

	private void OnItemSelected(int quickslot)
	{
		var item = Inventory.Instance.GetItemFromQuickslot(quickslot);
		character.SetHoldable(item?.definition);
	}

    private void OnVacuumAreaEntered(Area2D area)
    {
        if (area is not WorldItem worldItem)
        {
            return;
        }

        vacuumItemsInRadius.Add(worldItem);
    }

    private void OnVacuumAreaExited(Area2D area)
    {
        if (area is not WorldItem worldItem || !vacuumItemsInRadius.Contains(worldItem))
        {
            return;
        }

        vacuumItemsInRadius.Remove(worldItem);
    }
}
