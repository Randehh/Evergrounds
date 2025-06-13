using Godot;
using Godot.Collections;
using static WorldMap;

public interface IWorldGridNode
{
    public Node3D placementPreview { get; }
    public Vector3 gridPlacementOffset { get; }
}