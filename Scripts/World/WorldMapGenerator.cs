using Godot;
using Godot.Collections;
using static WorldMap;

[GlobalClass]
public partial class WorldMapGenerator : Node
{
    private const int CHUNK_SIZE = 10;
    private const int INITIAL_CHUNKS_SIZE = 5;
    private const float NOISE_SIZE = 5f;

    [Export]
    private int seed;

    [Export]
    private Array<WorldMapGeneratorNodePlacementData> placementDatas;

    private WorldMapData mapData;
    private FastNoiseLite noise;
    private AtlasMaterial defaultMaterial = AtlasMaterial.SOFT_SURFACE;
    private AtlasMaterial soilMaterial = AtlasMaterial.SOIL;

    private Vector2I gridSize;
    private Vector2I gridSizeHalf;

    public WorldMapGenerator()
    {
        noise = new FastNoiseLite();
        noise.Seed = seed;
        noise.NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth;
        noise.FractalOctaves = 2;
    }

    public override void _Ready()
    {
        gridSize = new (WorldMap.GRID_SIZE, WorldMap.GRID_SIZE);
        gridSizeHalf = gridSize / 2;
    }

    public void GenerateInitialChunks(WorldMapData mapData)
    {
        this.mapData = mapData;

        for (int x = -INITIAL_CHUNKS_SIZE; x < INITIAL_CHUNKS_SIZE; x++)
        {
            for (int y = -INITIAL_CHUNKS_SIZE; y < INITIAL_CHUNKS_SIZE; y++)
            {
                GenerateChunk(new Vector2I(x, y));
            }
        }
    }

    public void GenerateChunk(Vector2I chunkCoords)
    {
        for (int y = 0; y < CHUNK_SIZE; y++)
        {
            for (int x = 0; x < CHUNK_SIZE; x++)
            {
                GenerateTile(chunkCoords, new Vector2I(x, y));
            }
        }
    }

    private void GenerateTile(Vector2I chunkCoords, Vector2I tileCoord)
    {
        Vector2I globalTileCoord = (chunkCoords * CHUNK_SIZE) + tileCoord;

        float noiseValue = (noise.GetNoise2D(globalTileCoord.X * NOISE_SIZE, globalTileCoord.Y * NOISE_SIZE) + 1) * 0.5f;

        int tileValue = Mathf.RoundToInt(noiseValue - 0.3f);
        mapData.SetMaterial(globalTileCoord, tileValue == 0 ? defaultMaterial : soilMaterial, WorldMapData.WorldMapDataLayerType.BASE);

        foreach (WorldMapGeneratorNodePlacementData placementData in placementDatas)
        {
            placementData.TryPlace(globalTileCoord * gridSize - gridSizeHalf, noiseValue);
        }
    }
}