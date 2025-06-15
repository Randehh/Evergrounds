using Godot;
using Godot.Collections;

[GlobalClass]
public partial class WorldMapGeneratorNodePlacementData : Resource
{
    private const int COORD_X_SEED_OFFSET = 1000000000;
    private const int COORD_Y_SEED_OFFSET = 1000000;
    private const int COORD_Z_SEED_OFFSET = 1000;

    [Export]
    public Array<PackedScene> scenes;

    [Export]
    public float minThreshold = 0;

    [Export]
    public float maxThreshold = 1;

    [Export]
    public float chance = 0.2f;

    [Export]
    public float offsetVariance = 0;

    [Export]
    public bool randomRotation = false;

    public WorldMapGeneratorNodePlacementData() { }

    internal bool TryPlace(Vector3 globalTileCoord, float noiseValue)
    {
        noiseValue = Mathf.Pow(noiseValue, 2);
        if(noiseValue < minThreshold || noiseValue > maxThreshold)
        {
            return false;
        }

        GD.Seed(GetSeed(globalTileCoord, noiseValue));
        if(GD.Randf() >= chance)
        {
            return false;
        }

        Node3D placedNode = scenes.PickRandom().Instantiate<Node3D>();
        Vector3 placementPosition = globalTileCoord + GetRandomOffset();
        WorldMap.Instance.AddWorldNode(placedNode, true, placementPosition);

        if(randomRotation)
        {
            placedNode.RotateY(Mathf.Pi * 2 * GD.Randf());
        }
        return true;
    }

    private ulong GetSeed(Vector3 position, float noiseValue)
    {
        return (ulong)(
            Mathf.Abs(position.X * COORD_X_SEED_OFFSET) +
            Mathf.Abs(position.Y * COORD_Y_SEED_OFFSET) +
            Mathf.Abs(position.Z * COORD_Z_SEED_OFFSET) +
            noiseValue
       );
    }

    private Vector3 GetRandomOffset()
    {
        return new Vector3(
            ((GD.Randf() * 2) - 1) * offsetVariance,
            0,
            ((GD.Randf() * 2) - 1) * offsetVariance);
    }
}