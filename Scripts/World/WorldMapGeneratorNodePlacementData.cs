﻿using Godot;

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

    [Export]
    public float offsetVariance = 0;

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
        Vector2 placementPosition = globalTileCoord + GetRandomOffset();
        WorldMap.Instance.AddWorldNode(placedNode, true, placementPosition);
        return true;
    }

    private Vector2 GetRandomOffset()
    {
        return new Vector2I(
            Mathf.RoundToInt(((GD.Randf() * 2) - 1) * offsetVariance),
            Mathf.RoundToInt(((GD.Randf() * 2) - 1) * offsetVariance));
    }
}