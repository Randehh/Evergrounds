using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class PlayerCharacter : CharacterBody3D, IWorldSaveable
{

	public static PlayerCharacter Instance { get; private set; }

    [Export]
	private CharacterBase character;

	[Export]
	private PlayerInteractHandler interactHandler;

    [Export]
	private Area3D vacuumArea;

	[Export]
	private float maxSpeed = 1;

	[Export]
	private float acceleration = 10;

	[Export]
	private float decceleration = 0.01f;

	private Vector3 currentSpeed = Vector3.Zero;
	private Vector3 gravity;

	private List<WorldItem> vacuumItemsInRadius = new();
    private InputState inputState;

    public PlayerCharacter()
	{
		Instance = this;

		gravity = (Vector3)ProjectSettings.GetSetting("physics/3d/default_gravity_vector") * (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

    }

    public override void _Ready()
	{
        ServiceLocator.InventoryService.OnSelectQuickslot += OnItemSelected;

		OnItemSelected(0);

		PlayerCamera.Instance.toFollow = this;

        inputState = ServiceLocator.InputStateService.InputState;
        ServiceLocator.GameNotificationService.OnInputStateChanged.OnFire += OnInputStateChanged;
    }

	private void OnInputStateChanged(InputState state) => inputState = state;

    public override void _ExitTree()
    {
        ServiceLocator.InventoryService.OnSelectQuickslot -= OnItemSelected;
        ServiceLocator.GameNotificationService.OnInputStateChanged.OnFire -= OnInputStateChanged;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{

        if (inputState == InputState.WORLD)
		{
			interactHandler.ProcessInteraction(character.CurrentlyHolding, delta);

			if (Input.IsActionJustPressed("next_day"))
			{
				ServiceLocator.TimeService.TriggerNextDay(true);
			}
		}
	}

    public override void _PhysicsProcess(double delta)
    {
        Vector2 input = Vector2.Zero;

        if (inputState == InputState.WORLD)
        {
            input = new Vector2(
                Input.GetAxis("move_left", "move_right"),
                Input.GetAxis("move_up", "move_down")
                );
        }

        Vector2 acceleratedInput = input * acceleration;
        currentSpeed += new Vector3(acceleratedInput.X, 0, acceleratedInput.Y).Normalized();

        if (input.X < 0.05f && input.X > -0.05f)
        {
            currentSpeed.X *= decceleration;
        }

        if (input.Y < 0.05f && input.Y > -0.05f)
        {
            currentSpeed.Z *= decceleration;
        }

        currentSpeed = currentSpeed.LimitLength(maxSpeed);
        Vector3 finalSpeed = currentSpeed + gravity;

        Velocity = finalSpeed;
        MoveAndSlide();

        character.SetMovementParameters(currentSpeed, currentSpeed.Length() / maxSpeed);

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

    private void OnVacuumAreaEntered(Area3D area)
    {
        if (area is not WorldItem worldItem)
        {
            return;
        }

        vacuumItemsInRadius.Add(worldItem);
    }

    private void OnVacuumAreaExited(Area3D area)
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
