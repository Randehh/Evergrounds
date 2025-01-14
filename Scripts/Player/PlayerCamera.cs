using Godot;

[GlobalClass]
public partial class PlayerCamera : Camera2D
{

	[Export]
	private PlayerCharacter character;

	[Export]
	private float followSpeed;

	[Export]
	private float mousePullPower = 5;

	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Vector2 mouseNormalized = GetViewport().GetMousePosition() / GetViewportRect().Size;
		mouseNormalized = mouseNormalized - (Vector2.One * 0.5f);
		Vector2 targetPosition = character.Position + (mouseNormalized * mousePullPower);
        Position = Position.Lerp(targetPosition, (float)(followSpeed * delta));
	}
}
