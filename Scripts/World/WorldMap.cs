using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;
using static WorldMapTileDisplay;

[GlobalClass]
public partial class WorldMap : Node2D, IWorldSaveable
{
    private const string SAVE_KEY_MAP_DATA = "MapData";
    private const string SAVE_KEY_GENERATED_CHUNKS = "GeneratedChunks";
    private const string SAVE_KEY_X = "X";
    private const string SAVE_KEY_Y = "Y";

    public const int GRID_SIZE = 16;
    public const int CHUNK_SIZE = 5;
    public const int CHUNK_GENERATION_RANGE_X = 4;
    public const int CHUNK_GENERATION_RANGE_Y = 3;

    public static WorldMap Instance;

    public Vector2I MouseGridPosition { get; private set; }
    public bool IsSelectedTileTilled => selectedMaterial == AtlasMaterial.TILLED;

    [Export]
    private WorldMapGenerator generator;

    [Export]
    private Node2D mapChunkParent;

    private WorldMapData worldMapData;
    private List<WorldMapChunk> chunks = new();
    private System.Collections.Generic.Dictionary<Vector2I, WorldMapChunk> chunkLookup = new();

    private AtlasMaterial selectedMaterial;

    private Vector2I currentMapCenter;
    private Vector2I chunkGenerationSize;
    private Vector2I baseChunkOffset = new Vector2I(CHUNK_GENERATION_RANGE_X, CHUNK_GENERATION_RANGE_Y);
    private bool initialChunksSet = false;

    private Queue<(WorldMapChunk, Vector2I, Vector2I)> chunkMovementQueue = new();
    private List<WorldMapChunk> lockedChunks = new();

    private HashSet<Vector2I> generatedChunkCoords = new();

    private System.Collections.Generic.Dictionary<Vector2I, WorldMapTileDisplayEdge> edgeLookup = new()
    {
        { new(-1, 0), WorldMapTileDisplayEdge.LEFT },
        { new(1, 0), WorldMapTileDisplayEdge.RIGHT },
        { new(0, -1), WorldMapTileDisplayEdge.UP },
        { new(0, 1), WorldMapTileDisplayEdge.DOWN }
    };

    public WorldMap()
    {
        Instance = this;

        chunkGenerationSize = new Vector2I(1 + (CHUNK_GENERATION_RANGE_X * 2), 1 + (CHUNK_GENERATION_RANGE_Y * 2));

        worldMapData = new WorldMapData();
    }

    public override void _Ready()
    {
        AddToGroup(WorldData.RecreateSaveGroup);

        CreateChunks();
    }

    public override void _Process(double delta)
    {

        Vector2I mouseGridPosition = GetMouseCoordinates(1, true);
        foreach (WorldMapChunk chunk in chunks)
        {
            chunk.UpdateSelectedMaterial(mouseGridPosition);
        }

        selectedMaterial = GetSelectedMaterial();

        Vector2I nextMapCenter = GetGridChunkPosition(PlayerCharacter.Instance.Position);
        if(currentMapCenter != nextMapCenter || !initialChunksSet)
        {
            RecenterMap(nextMapCenter);
        }

        if(chunkMovementQueue.TryDequeue(out var chunkData))
        {
            var nextChunk = chunkData.Item1;
            var chunkArrayPosition = chunkData.Item2;
            var movementDirection = chunkData.Item3;

            Vector2I chunkPosition = (chunkArrayPosition * CHUNK_SIZE) / CHUNK_SIZE;
            chunkLookup[chunkPosition] = nextChunk;

            if(!generatedChunkCoords.Contains(chunkArrayPosition))
            {
                generator.GenerateChunk(worldMapData, chunkArrayPosition);
                generatedChunkCoords.Add(chunkArrayPosition);
            }

            Vector2I chunkTilePosition = chunkArrayPosition * CHUNK_SIZE;
            Vector2I chunkPixelPosition = chunkTilePosition * GRID_SIZE;
            nextChunk.SetChunkPosition(chunkTilePosition, chunkPixelPosition);


            if(edgeLookup.TryGetValue(movementDirection, out WorldMapTileDisplayEdge edge) &&
                chunkLookup.TryGetValue(chunkPosition - movementDirection, out WorldMapChunk edgeChunk))
            {
                edgeChunk.UpdateEdgeVisuals(edge);
            }

            if (lockedChunks.Contains(nextChunk))
            {
                lockedChunks.Remove(nextChunk);
            }
        }
    }

    private void CreateChunks()
    {
        for (int x = 0; x < chunkGenerationSize.X; x++)
        {
            for (int y = 0; y < chunkGenerationSize.Y; y++)
            {
                Vector2I chunkArrayPosition = new Vector2I(x, y) - baseChunkOffset;

                WorldMapChunk newChunk = new WorldMapChunk(mapChunkParent, worldMapData);
                chunks.Add(newChunk);
                chunkLookup.Add(chunkArrayPosition, newChunk);

                generator.GenerateChunk(worldMapData, chunkArrayPosition);
            }
        }
    }

    private void RecenterMap(Vector2I position)
    {
        List<WorldMapChunk> freeChunks = new List<WorldMapChunk>(chunks);
        HashSet<Vector2I> filledSpaces = new HashSet<Vector2I>();

        foreach(WorldMapChunk chunk in chunks)
        {
            Vector2I chunkPosition = chunk.ChunkTilePosition / CHUNK_SIZE;
            bool isInRange =
                chunkPosition.X >= position.X - baseChunkOffset.X &&
                chunkPosition.X <= position.X + baseChunkOffset.X &&
                chunkPosition.Y >= position.Y - baseChunkOffset.Y &&
                chunkPosition.Y <= position.Y + baseChunkOffset.Y;

            if(isInRange && chunk.isSet)
            {
                freeChunks.Remove(chunk);
            }

            if (initialChunksSet)
            {
                filledSpaces.Add(chunkPosition);
            }
        }

        Vector2I movementDirection = (position - currentMapCenter);

        for (int x = 0; x < chunkGenerationSize.X; x++)
        {
            for (int y = 0; y < chunkGenerationSize.Y; y++)
            {
                Vector2I chunkArrayPosition = new Vector2I(x, y) - baseChunkOffset + position;

                if (filledSpaces.Contains(chunkArrayPosition))
                {
                    continue;
                }

                WorldMapChunk nextChunk = freeChunks[0];
                if (lockedChunks.Contains(nextChunk))
                {
                    continue;
                }

                freeChunks.RemoveAt(0);

                chunkMovementQueue.Enqueue((nextChunk, chunkArrayPosition, movementDirection));

                Vector2I chunkPosition = nextChunk.ChunkTilePosition / CHUNK_SIZE;
                chunkLookup.Remove(chunkPosition);

                lockedChunks.Add(nextChunk);
            }
        }

        currentMapCenter = position;
        initialChunksSet = true;
    }

    public AtlasMaterial GetSelectedMaterial()
    {
        return GetSelectedChunk()?.SelectedMaterial ?? AtlasMaterial.NONE;
    }

    public void SetSelectedTile(AtlasMaterial material, bool updateAllChunks = false)
    {
        WorldMapChunk selectedChunk = GetSelectedChunk();
        bool updateVisuals = selectedChunk?.SetSelectedTile(material) ?? false;
        if (!updateVisuals)
        {
            return;
        }

        Vector2I mouseMapPosition = selectedChunk.MouseMapPosition;


        if (updateAllChunks)
        {
            foreach (var item in chunks)
            {

                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        Vector2I v = new Vector2I(x, y);
                        item.UpdateTileVisuals(mouseMapPosition + v);
                    }
                }
            }
        }
        else
        {
            selectedChunk.UpdateTileVisuals(mouseMapPosition);
        }
    }

    public bool CanChangeToTileMaterial(AtlasMaterial targetMaterial)
    {
        return GetSelectedChunk()?.CanChangeToTileMaterial(targetMaterial) ?? false;
    }

    public WorldMapChunk GetSelectedChunk()
    {
        foreach (WorldMapChunk chunk in chunks)
        {
            if (!chunk.IsMouseInChunk)
            {
                continue;
            }

            return chunk;
        }

        return null;
    }


    public Vector2I GetMouseCoordinates(int divisions, bool addHalfSizeOffset)
    {
        Vector2 globalMousePosition = GetGlobalMousePosition();
        return GetGridPosition(globalMousePosition, GRID_SIZE, divisions, addHalfSizeOffset);
    }

    private Vector2I GetGridPosition(Vector2 position, int gridSize, int divisions, bool addHalfSizeOffset)
    {
        int size = gridSize / divisions;
        int xInt = Mathf.FloorToInt(position.X / size) * size;
        int yInt = Mathf.FloorToInt(position.Y / size) * size;

        if(addHalfSizeOffset)
        {
            return new Vector2I(xInt, yInt) + new Vector2I(size / 2, size / 2);
        } else
        {
            return new Vector2I(xInt, yInt);
        }
    }

    public Vector2I GetGridChunkPosition(Vector2 position) => GetGridPosition(position, GRID_SIZE * CHUNK_SIZE, 1, false) / (GRID_SIZE * CHUNK_SIZE);

    public void AddWorldNode(Node2D node, bool replaceParent)
    {
        if(replaceParent)
        {
            if (node.GetParent() != null)
            {
                node.GetParent().RemoveChild(node);
            }

            AddChild(node);
        }

        worldMapData.AddWorldNode(node);
    }

    public Godot.Collections.Dictionary<string, Variant> GetSaveData()
    {
        Godot.Collections.Dictionary<string, Variant> data = new();
        
        data.Add(SAVE_KEY_MAP_DATA, worldMapData.GetSaveData());

        Godot.Collections.Array<Variant> chunkCoordsGenerated = new();
        data.Add(SAVE_KEY_GENERATED_CHUNKS, chunkCoordsGenerated);

        foreach (Vector2I coords in generatedChunkCoords)
        {
            Godot.Collections.Dictionary<string, Variant> chunkCoordDict = new()
            {
                { SAVE_KEY_X, coords.X },
                { SAVE_KEY_Y, coords.Y },
            };
            chunkCoordsGenerated.Add(chunkCoordDict);
        }

        return data;
    }

    public void SetSaveData(Godot.Collections.Dictionary<string, Variant> data)
    {
        worldMapData.SetSaveData(data[SAVE_KEY_MAP_DATA].AsGodotDictionary<string, Variant>());

        foreach (Godot.Collections.Dictionary<string, Variant> chunkDict in data[SAVE_KEY_GENERATED_CHUNKS].AsGodotArray<Godot.Collections.Dictionary<string, Variant>>())
        {
            generatedChunkCoords.Add(new Vector2I(
                chunkDict[SAVE_KEY_X].AsInt32(),
                chunkDict[SAVE_KEY_Y].AsInt32()
                ));
        }

        foreach (var chunk in chunks)
        {
            chunk.ReplaceMapData(worldMapData);
        }
    }

    public enum AtlasMaterial
    {
        NONE = -1,
        SOFT_SURFACE = 0,
        SOIL = 1,
        TILLED = 2,
    }
}