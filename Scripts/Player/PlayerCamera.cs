using Godot;

[GlobalClass]
public partial class PlayerCamera : Camera2D
{
	public static PlayerCamera Instance { get; private set; }

	[Export]
	private float followSpeed;

	[Export]
	private float mousePullPower = 5;

	public Node2D toFollow;

    public override void _Ready()
    {
        Instance = this;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
		if (toFollow == null)
		{
			return;
		}

		Vector2 mouseNormalized = GetViewport().GetMousePosition() / GetViewportRect().Size;
		mouseNormalized = mouseNormalized - (Vector2.One * 0.5f);
		Vector2 targetPosition = toFollow.Position + (mouseNormalized * mousePullPower);
		Position = Position.Lerp(targetPosition, (float)(followSpeed * delta));
	}
}
