using Godot;

[GlobalClass]
public partial class WorldNodeShadow : Node2D
{
    private static Color halfAlpha = new Color(1, 1, 1, 0.5f);

    [Export]
    private Node2D targetNode;

    [Export]
    private Sprite2D shadowSprite;

    [Export]
    private float maxHeight = 20;

    [Export]
    private float baseScaleMultiplier = 1;

    private Vector2 baseScale;

    public override void _Ready()
    {
        SetBaseScale(baseScaleMultiplier);
        shadowSprite.Modulate = halfAlpha;
    }

    public override void _Process(double delta)
    {
        if(GlobalPosition.Y - targetNode.GlobalPosition.Y > maxHeight)
        {
            shadowSprite.Scale = Vector2.Zero;
        } else
        {
            shadowSprite.Scale = baseScale.Lerp(Vector2.Zero, Mathf.Max(0, GlobalPosition.Y - targetNode.GlobalPosition.Y) / maxHeight);
        }

    }

    public void SetBaseScale(float scale)
    {
        baseScale = new Vector2(scale, scale);
    }
}