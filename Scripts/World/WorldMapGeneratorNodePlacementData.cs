using Godot;

[GlobalClass]
public partial class WorldMapGeneratorNodePlacementData : Resource
{
    [Export]
    public PackedScene scene;

    [Export]
    public float minThreshold = 0;

    [Export]
    public float maxThreshold = 1;

    [Export]
    public float chance = 0.2f;

    public WorldMapGeneratorNodePlacementData() { }

    internal bool TryPlace(Vector2I globalTileCoord, float noiseValue)
    {
        noiseValue = Mathf.Pow(noiseValue, 2);
        if(noiseValue < minThreshold || noiseValue > maxThreshold)
        {
            return false;
        }

        GD.Seed((ulong)(globalTileCoord.X * 1000000 + globalTileCoord.Y * 1000 + noiseValue));
        if(GD.Randf() >= chance)
        {
            return false;
        }

        Node2D placedNode = scene.Instantiate<Node2D>();
        WorldMap.Instance.AddWorldNode(placedNode, true, globalTileCoord);
        return true;
    }
}