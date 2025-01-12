using Godot;

[GlobalClass]
public partial class PlayerCamera : Camera2D
{

	[Export]
	private PlayerCharacter character;

	[Export]
	private float followSpeed;

	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Position = Position.Lerp(character.Position, (float)(followSpeed * delta));
	}
}
