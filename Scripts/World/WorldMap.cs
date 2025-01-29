using Godot;
using System.Collections.Generic;
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
    private WorldMapDisplay worldMapDisplay;
    private HashSet<Vector2I> currentChunks = new ();
    private Queue<Vector2I> chunksToHide = new  ();
    private Queue<Vector2I> chunksToShow = new  ();

    private AtlasMaterial selectedMaterial;

    private Vector2I currentMapCenter;
    private Vector2I chunkGenerationSize;
    private Vector2I baseChunkOffset = new Vector2I(CHUNK_GENERATION_RANGE_X, CHUNK_GENERATION_RANGE_Y);
    private bool initialChunksSet = false;

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
    }

    public override void _Ready()
    {
        AddToGroup(WorldData.RecreateSaveGroup);

        worldMapData = new WorldMapData();
        worldMapDisplay = new WorldMapDisplay(mapChunkParent, worldMapData);
    }

    public override void _Process(double delta)
    {
        Vector2I mouseGridPosition = GetMouseCoordinates(1, true);
        worldMapDisplay.UpdateSelectedMaterial(mouseGridPosition);

        selectedMaterial = GetSelectedMaterial();

        Vector2I nextMapCenter = GetGridChunkPosition(PlayerCharacter.Instance.Position);
        if(currentMapCenter != nextMapCenter || !initialChunksSet)
        {
            RecenterMap(nextMapCenter);
        }

        if(chunksToHide.TryDequeue(out Vector2I toHide))
        {
            worldMapDisplay.HideChunk(toHide);
        }

        if (chunksToShow.TryDequeue(out Vector2I toShow))
        {
            if (!generatedChunkCoords.Contains(toShow))
            {
                generator.GenerateChunk(worldMapData, toShow);
                generatedChunkCoords.Add(toShow);
            }

            worldMapDisplay.DisplayChunk(toShow);
            worldMapData.SpawnNodes(toShow);
        }
    }

    private void RecenterMap(Vector2I position)
    {
        Vector2I mapMovement = (position - currentMapCenter);

        if (initialChunksSet == true && mapMovement.Equals(Vector2I.Zero))
        {
            return;
        }

        HashSet<Vector2I> nextChunks = GetChunksAroundPoint(position);

        // To remove...
        foreach (Vector2I chunk in GetChunkCoordinatesDelta(currentChunks, nextChunks))
        {
            chunksToHide.Enqueue(chunk);
        }

        // To add
        foreach (Vector2I chunk in GetChunkCoordinatesDelta(nextChunks, currentChunks))
        {
            chunksToShow.Enqueue(chunk);
        }

        currentMapCenter = position;
        currentChunks = nextChunks;
        initialChunksSet = true;
    }

    private HashSet<Vector2I> GetChunksAroundPoint(Vector2I point)
    {
        HashSet<Vector2I> chunks = new();
        for (int x = 0; x < chunkGenerationSize.X; x++)
        {
            for (int y = 0; y < chunkGenerationSize.Y; y++)
            {
                Vector2I chunkArrayPosition = new Vector2I(x, y) - baseChunkOffset + point;
                chunks.Add(chunkArrayPosition);
            }
        }

        return chunks;
    }

    private IEnumerable<Vector2I> GetChunkCoordinatesDelta(HashSet<Vector2I> currentChunks, HashSet<Vector2I> nextChunks)
    {
        HashSet<Vector2I> toRemove = new HashSet<Vector2I>(currentChunks);
        toRemove.RemoveWhere(c => nextChunks.Contains(c));
        return toRemove;
    }

    public AtlasMaterial GetSelectedMaterial()
    {
        return worldMapDisplay.SelectedMaterial;
    }

    public AtlasMaterial GetMaterialAt(Vector2I coord)
    {
        return worldMapDisplay.GetMaterialAt(coord);
    }

    public void SetSelectedTile(AtlasMaterial material, bool updateAllChunks = false)
    {
        bool updateVisuals = worldMapDisplay.SetSelectedTile(material);
        if (!updateVisuals)
        {
            return;
        }

        Vector2I mouseMapPosition = worldMapDisplay.MouseMapPosition;

        if (updateAllChunks)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    Vector2I v = new Vector2I(x, y);
                    worldMapDisplay.UpdateTileVisuals(mouseMapPosition + v);
                }
            }
        }
        else
        {
            worldMapDisplay.UpdateTileVisuals(mouseMapPosition);
        }
    }

    public bool CanChangeToTileMaterial(AtlasMaterial targetMaterial)
    {
        return worldMapDisplay.CanChangeToTileMaterial(targetMaterial);
    }


    public Vector2I GetMouseCoordinates(int divisions, bool addHalfSizeOffset)
    {
        Vector2 globalMousePosition = GetGlobalMousePosition();
        return GetGridPosition(globalMousePosition, GRID_SIZE, divisions, addHalfSizeOffset);
    }

    public Vector2I GetGridCoordinates(Vector2 position)
    {
        return GetGridPosition(position, GRID_SIZE, 1, false) / GRID_SIZE;
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

    public void AddWorldNode(Node2D node, bool replaceParent, Vector2 replaceParentPosition)
    {
        if(replaceParent)
        {
            if (node.GetParent() != null)
            {
                node.GetParent().RemoveChild(node);
            }

            AddChild(node);
            node.Position = replaceParentPosition;
        }

        worldMapData.AddWorldNode(node);
    }

    public bool CanPlaceNode(IWorldGridNode worldGridNode, Vector2I gridPosition) => worldMapData.CanPlaceNode(worldGridNode, gridPosition);

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

        worldMapDisplay.ReplaceMapData(worldMapData, GetChunksAroundPoint(Vector2I.Zero));
    }

    public enum AtlasMaterial
    {
        NONE = -1,
        SOFT_SURFACE = 0,
        SOIL = 1,
        TILLED = 2,
    }
}