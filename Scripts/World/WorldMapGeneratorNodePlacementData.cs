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

    internal void TryPlace(Vector2I globalTileCoord, float noiseValue)
    {
        noiseValue = Mathf.Pow(noiseValue, 2);
        if(noiseValue < minThreshold || noiseValue > maxThreshold)
        {
            return;
        }

        GD.Seed((ulong)(globalTileCoord.X * 1000000 + globalTileCoord.Y + noiseValue));
        if(GD.Randf() >= chance)
        {
            return;
        }

        Node2D placedNode = scene.Instantiate<Node2D>();
        WorldMap.Instance.AddChild(placedNode);
        placedNode.GlobalPosition = globalTileCoord;
    }
}