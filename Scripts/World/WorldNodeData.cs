using Godot;
using Godot.Collections;

public class WorldNodeData : IWorldSaveable
{
    public static bool TryCreateFromLiveInstance(Node2D node, out WorldNodeData worldNodeData)
    {
        worldNodeData = new WorldNodeData();
        bool succeeded = worldNodeData.SetLiveNodeReference(node);
        return succeeded;
    }

    private const string SAVE_KEY_FILEPATH = "FilePath";
    private const string SAVE_KEY_PARENT = "Parent";
    private const string SAVE_KEY_POS_X = "PosX";
    private const string SAVE_KEY_POS_Y = "PosY";
    private const string SAVE_KEY_CHUNK_X = "ChunkX";
    private const string SAVE_KEY_CHUNK_Y = "ChunkY";
    private const string SAVE_KEY_METADATA = "Metadata";

    public Node2D LiveNode => liveNodeReference;
    public Vector2I NodeChunkCoordinate => liveNodeReference != null ? WorldMap.Instance.GetGridChunkPosition(liveNodeReference.Position) : lastKnownChunk;

    private Node2D liveNodeReference;
    private IWorldSaveable liveWorldSaveable;

    private string sceneFilePath;
    private string parentPath;
    private Vector2 lastKnownPosition;
    private Vector2I lastKnownChunk;
    private Dictionary<string, Variant> metadata;

    public bool TrySpawn(out Node2D spawnedNode)
    {
        spawnedNode = null;

        if (!PlayerDespawnableHandler.Instance.IsPointInArea(lastKnownPosition))
        {
            return false;
        }

        var newObjectScene = GD.Load<PackedScene>(sceneFilePath);
        spawnedNode = newObjectScene.Instantiate<Node2D>();
        WorldMap.Instance.AddChild(spawnedNode);
        spawnedNode.Set(Node2D.PropertyName.Position, lastKnownPosition);

        (spawnedNode as IWorldSaveable).SetSaveData(metadata);

        SetLiveNodeReference(spawnedNode);

        return true;
    }

    public bool SetLiveNodeReference(Node2D liveNode)
    {
        if(liveNode is not IWorldSaveable saveable)
        {
            GD.PushWarning($"Cannot save world data that is not implementing {nameof(IWorldSaveable)}: {liveNode.Name}");
            return false;
        }

        liveNodeReference = liveNode;
        liveWorldSaveable = saveable;

        sceneFilePath = liveNode.SceneFilePath;
        parentPath = liveNode.GetParent().GetPath();

        return true;
    }

    public void TrySetSaveData()
    {
        if(liveNodeReference == null || !GodotObject.IsInstanceValid(liveNodeReference))
        {
            return;
        }

        lastKnownPosition = liveNodeReference.Position;
        lastKnownChunk = WorldMap.Instance.GetGridChunkPosition(lastKnownPosition);
        metadata = liveWorldSaveable.GetSaveData();
    }

    public Godot.Collections.Dictionary<string, Variant> GetSaveData()
    {
        TrySetSaveData();

        Godot.Collections.Dictionary<string, Variant> saveDataDictionary = new();

        if (string.IsNullOrEmpty(sceneFilePath))
        {
            GD.Print($"Cannot save world data when the scene file path is empty");
            return null;
        }

        saveDataDictionary.Add(SAVE_KEY_FILEPATH, sceneFilePath);
        saveDataDictionary.Add(SAVE_KEY_PARENT, parentPath);
        saveDataDictionary.Add(SAVE_KEY_CHUNK_X, lastKnownChunk.X);
        saveDataDictionary.Add(SAVE_KEY_CHUNK_Y, lastKnownChunk.Y);
        saveDataDictionary.Add(SAVE_KEY_POS_X, lastKnownPosition.X);
        saveDataDictionary.Add(SAVE_KEY_POS_Y, lastKnownPosition.Y);
        saveDataDictionary.Add(SAVE_KEY_METADATA, metadata);

        return saveDataDictionary;
    }

    public void SetSaveData(Dictionary<string, Variant> data)
    {
        sceneFilePath = data[SAVE_KEY_FILEPATH].AsString();
        parentPath = data[SAVE_KEY_PARENT].AsString();

        lastKnownChunk = new Vector2I(
            data[SAVE_KEY_CHUNK_X].AsInt32(),
            data[SAVE_KEY_CHUNK_Y].AsInt32()
            );

        lastKnownPosition = new Vector2(
            (float)data[SAVE_KEY_POS_X].AsDouble(),
            (float)data[SAVE_KEY_POS_Y].AsDouble()
            );

        metadata = data[SAVE_KEY_METADATA].AsGodotDictionary<string, Variant>();
    }
}