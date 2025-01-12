using Godot;

[GlobalClass]
public partial class PlayerAttachment : Node2D
{
    [Export]
    private Node2D targetNode;

    [Export]
    private Node2D attachmentNode;

    [Export]
    private float followSpeed = 10;

    [Export]
    private float rotationSpeed = 10;

    [Export]
    private float rotationMultiplier = 2;

    private Vector2 lagPosition;
    private float startRotation;
    private float lagRotation;

    public override void _Ready()
    {
        startRotation = targetNode.Rotation;
    }

    public override void _Process(double delta)
	{
        lagPosition = lagPosition.Lerp(GlobalPosition, followSpeed * (float)delta);
        lagRotation = Mathf.Lerp(lagRotation, (startRotation * 0.5f) + ((targetNode.Rotation - startRotation) * rotationMultiplier), rotationSpeed * (float)delta);

        attachmentNode.GlobalPosition = lagPosition;
        attachmentNode.Rotation = lagRotation;
    }
}
