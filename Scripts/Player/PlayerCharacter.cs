using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class PlayerCharacter : Node2D, IWorldSaveable
{

	public static PlayerCharacter Instance { get; private set; }

    [Export]
	private CharacterBase character;

	[Export]
	private PlayerInteractHandler interactHandler;

    [Export]
	private Area2D vacuumArea;

	[Export]
	private float maxSpeed = 1;

	[Export]
	private float acceleration = 10;

	[Export]
	private float decceleration = 0.01f;

	private Vector2 currentSpeed = Vector2.Zero;

	private List<WorldItem> vacuumItemsInRadius = new();

	public PlayerCharacter()
	{
		Instance = this;
	}

    public override void _Ready()
	{
        ServiceLocator.InventoryService.OnSelectQuickslot += OnItemSelected;

		OnItemSelected(0);

		PlayerCamera.Instance.toFollow = this;
    }

    public override void _ExitTree()
    {
        ServiceLocator.InventoryService.OnSelectQuickslot -= OnItemSelected;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
		Vector2 input = new Vector2(
			Input.GetAxis("move_left", "move_right"),
			Input.GetAxis("move_up", "move_down")
			).Normalized();

		currentSpeed += input * acceleration;

		if (input.X < 0.05f && input.X > -0.05f)
		{
			currentSpeed.X *= decceleration;
		}

		if (input.Y < 0.05f && input.Y > -0.05f)
		{
			currentSpeed.Y *= decceleration;
		}

		currentSpeed.X = Mathf.Clamp(currentSpeed.X, -maxSpeed, maxSpeed);
		currentSpeed.Y = Mathf.Clamp(currentSpeed.Y, -maxSpeed, maxSpeed);

		Position += currentSpeed * (float)delta;

		interactHandler.ProcessInteraction(character.CurrentlyHolding, delta);
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

	private void OnItemSelected(int quickslot)
	{
		var item = ServiceLocator.InventoryService.GetItem(quickslot);
		character.SetHoldable(item);
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

    public Godot.Collections.Dictionary<string, Variant> GetSaveData()
    {
        return new Godot.Collections.Dictionary<string, Variant>();
    }

	public void SetSaveData(Godot.Collections.Dictionary<string, Variant> data)
	{

	}
}
