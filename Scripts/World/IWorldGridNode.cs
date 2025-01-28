using Godot;
using Godot.Collections;
using static WorldMap;

public interface IWorldGridNode
{
    public Sprite2D gridPlacementPreviewSprite { get; }
    public Vector2 gridPlacementOffset { get; }
    public Array<AtlasMaterial> placementMaterials { get; }
    public Array<Vector2I> gridOffsetPositions { get; }
}