using Godot;

[GlobalClass]
public partial class PlayerCamera : Camera3D
{
	public static PlayerCamera Instance { get; private set; }

	[Export]
	private float followSpeed;

	[Export]
	private float mousePullPower = 5;

	public Node3D toFollow;
	private Vector3 raycastHit;

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

		Vector2 mouseNormalized = GetViewport().GetMousePosition() / GetViewport().GetVisibleRect().Size;
		mouseNormalized = mouseNormalized - (Vector2.One * 0.5f);
		mouseNormalized *= mousePullPower;
		Vector3 targetPosition = toFollow.Position + new Vector3(mouseNormalized.X, 0, mouseNormalized.Y) + (this.Transform.Basis.Z * 10);
		Position = Position.Lerp(targetPosition, (float)(followSpeed * delta));
	}

    public override void _PhysicsProcess(double delta)
    {
        if (toFollow != null)
        {
            Vector2 mouseNormalized = GetViewport().GetMousePosition() / GetViewport().GetVisibleRect().Size;
            mouseNormalized = mouseNormalized - (Vector2.One * 0.5f);
            mouseNormalized *= mousePullPower;
            Vector3 targetPosition = toFollow.Position + new Vector3(mouseNormalized.X, 0, mouseNormalized.Y) + (this.Transform.Basis.Z * 10);
            Position = Position.Lerp(targetPosition, (float)(followSpeed * delta));
        }

        Vector2 mousePosition = GetViewport().GetMousePosition();
        Vector3 from = ProjectRayOrigin(mousePosition);
        Vector3 to = ProjectRayNormal(mousePosition) * 1000;
        Plane plane = new Plane(Vector3.Up, 0);
        Vector3? hit = plane.IntersectsRay(from, to);
		if(hit.HasValue)
		{
            raycastHit = plane.IntersectsRay(from, to).Value;
        }
    }

	public Vector3 GetGroundRaycastPosition() => raycastHit;
}
