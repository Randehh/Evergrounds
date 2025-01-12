using Godot;

[GlobalClass]
public partial class PlayerCharacter : Node2D
{

	[Export]
	private CharacterBase character;

	[Export]
	private float maxSpeed = 1;

	[Export]
	private float acceleration = 10;

	[Export]
	private float decceleration = 0.01f;

	private Vector2 currentSpeed = Vector2.Zero;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Inventory.Instance.OnSelectQuickslot += OnItemSelected;

    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Vector2 input = new Vector2(
			Input.GetAxis("move_left", "move_right"),
			Input.GetAxis("move_up", "move_down")
			).Normalized();

		currentSpeed += input * acceleration * (float)delta;

		if(input.X < 0.05f && input.X > -0.05f) 
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
    }

	private void OnItemSelected(int quickslot)
	{
		var item = Inventory.Instance.GetItemFromQuickslot(quickslot);
		character.SetHoldable(item?.definition);

    }
}
