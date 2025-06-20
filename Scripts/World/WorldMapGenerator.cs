﻿using Godot;
using Godot.Collections;
using System.Collections.Generic;
using static WorldMap;

[GlobalClass]
public partial class WorldMapGenerator : Node
{
    private const float GRID_SIZE = 1;
    private const int CHUNK_SIZE = 10;
    private const float NOISE_SIZE = 5f;

    [Export]
    private bool randomSeed = true;

    [Export]
    private int seed;

    [Export]
    private Array<WorldMapGeneratorNodePlacementData> placementDatas;

    private FastNoiseLite noise;
    private AtlasMaterial defaultMaterial = AtlasMaterial.SOFT_SURFACE;
    private AtlasMaterial soilMaterial = AtlasMaterial.SOIL;

    private Vector2 gridSize;
    private Vector2 gridSizeHalf;

    public WorldMapGenerator()
    {
        noise = new FastNoiseLite();

        noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Cellular;
    }

    public override void _Ready()
    {
        gridSize = new (GRID_SIZE, GRID_SIZE);
        gridSizeHalf = gridSize / 2;

        noise.Seed = randomSeed ? (int)Time.GetUnixTimeFromSystem() : seed;
    }

    public void GenerateChunk(WorldMapData mapData, Vector2I chunkCoords)
    {
        for (int y = -1; y < CHUNK_SIZE + 1; y++)
        {
            for (int x = -1; x < CHUNK_SIZE + 1; x++)
            {
                GenerateTile(mapData, chunkCoords, new Vector2I(x, y), x != -1 && x != CHUNK_SIZE && y != -1 && y != CHUNK_SIZE);
            }
        }
    }

    private void GenerateTile(WorldMapData mapData, Vector2I chunkCoords, Vector2I tileCoord, bool placeNode)
    {

        Vector2I globalTileCoord = (chunkCoords * CHUNK_SIZE) + tileCoord;

        float noiseValue = (noise.GetNoise2D(globalTileCoord.X * NOISE_SIZE, globalTileCoord.Y * NOISE_SIZE) + 1) * 0.5f * 3;

        int tileValue = Mathf.RoundToInt(noiseValue - 0.4f);
        //mapData.SetMaterial(globalTileCoord, tileValue == 0 ? defaultMaterial : soilMaterial, WorldMapData.WorldMapDataLayerType.BASE, Vector2I.Zero);

        if(!placeNode)
        {
            return;
        }

        foreach (WorldMapGeneratorNodePlacementData placementData in placementDatas)
        {
            Vector2 placementPosition = globalTileCoord * gridSize - gridSizeHalf;
            bool isPlaced = placementData.TryPlace(new Vector3(placementPosition.X, -0.5f, placementPosition.Y), noiseValue);
            if(isPlaced)
            {
                break;
            }
        }
    }
}