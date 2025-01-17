﻿using Godot;
using Godot.Collections;
using System.Collections.Generic;
using static WorldMap;

[GlobalClass]
public partial class WorldMapGenerator : Node
{
    private const float NOISE_SIZE = 5f;

    [Export]
    private int seed;

    [Export]
    private Array<WorldMapGeneratorNodePlacementData> placementDatas;

    private FastNoiseLite noise;
    private AtlasMaterial defaultMaterial = AtlasMaterial.SOFT_SURFACE;
    private AtlasMaterial soilMaterial = AtlasMaterial.SOIL;

    private HashSet<Vector2I> generatedChunkCoords = new();

    private Vector2I gridSize;
    private Vector2I gridSizeHalf;

    public WorldMapGenerator()
    {
        noise = new FastNoiseLite();
        noise.Seed = seed;
        noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Cellular;
    }

    public override void _Ready()
    {
        gridSize = new (WorldMap.GRID_SIZE, WorldMap.GRID_SIZE);
        gridSizeHalf = gridSize / 2;
    }

    public void GenerateChunk(WorldMapData mapData, Vector2I chunkCoords)
    {
        if(generatedChunkCoords.Contains(chunkCoords))
        {
            return;
        }

        for (int y = 0; y < WorldMap.CHUNK_SIZE; y++)
        {
            for (int x = 0; x < WorldMap.CHUNK_SIZE; x++)
            {
                GenerateTile(mapData, chunkCoords, new Vector2I(x, y));
            }
        }
    }

    private void GenerateTile(WorldMapData mapData, Vector2I chunkCoords, Vector2I tileCoord)
    {
        Vector2I globalTileCoord = (chunkCoords * CHUNK_SIZE) + tileCoord;

        float noiseValue = (noise.GetNoise2D(globalTileCoord.X * NOISE_SIZE, globalTileCoord.Y * NOISE_SIZE) + 1) * 0.5f * 3;

        int tileValue = Mathf.RoundToInt(noiseValue - 0.3f);
        mapData.SetMaterial(globalTileCoord, tileValue == 0 ? defaultMaterial : soilMaterial, WorldMapData.WorldMapDataLayerType.BASE);

        foreach (WorldMapGeneratorNodePlacementData placementData in placementDatas)
        {
            bool isPlaced = placementData.TryPlace(globalTileCoord * gridSize - gridSizeHalf, noiseValue);
            if(isPlaced)
            {
                break;
            }
        }
    }
}