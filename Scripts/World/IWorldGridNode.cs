using Godot;

public interface IWorldGridNode
{
    public Sprite2D gridPlacementPreviewSprite { get; }
    public Vector2 gridPlacementOffset { get; }
}