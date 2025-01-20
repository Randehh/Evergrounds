using Godot;
using Godot.Collections;
using System;

public class WorldNodeData : IWorldSaveable
{
    public static bool TryCreateFromLiveInstance(Node2D node, Action<Node2D> onReleaseCallback, out WorldNodeData worldNodeData)
    {
        worldNodeData = new WorldNodeData();
        bool succeeded = worldNodeData.SetLiveNodeReference(node, onReleaseCallback);
        return succeeded;
    }

    private const string SAVE_KEY_FILEPATH = "FilePath";
    private const string SAVE_KEY_PARENT = "Parent";
    private const string SAVE_KEY_POS_X = "PosX";
    private const string SAVE_KEY_POS_Y = "PosY";
    private const string SAVE_KEY_CHUNK_X = "ChunkX";
    private const string SAVE_KEY_CHUNK_Y = "ChunkY";
    private const string SAVE_KEY_METADATA = "Metadata";

    private Node2D liveNodeReference;
    private IWorldSaveable liveWorldSaveable;
    private Action<Node2D> onRelease;

    private string sceneFilePath;
    private string parentPath;
    private Vector2 lastKnownPosition;
    private Vector2I lastKnownChunk;
    private Dictionary<string, Variant> metadata;

    public bool SetLiveNodeReference(Node2D liveNode, Action<Node2D> onReleaseCallback)
    {
        if(liveNode is not IWorldSaveable saveable)
        {
            GD.PushWarning($"Cannot save world data that is not implementing {nameof(IWorldSaveable)}: {liveNode.Name}");
            return false;
        }

        liveNodeReference = liveNode;
        liveWorldSaveable = saveable;
        onRelease = onReleaseCallback;

        sceneFilePath = liveNode.SceneFilePath;
        parentPath = liveNode.GetParent().GetPath();

        liveNodeReference.TreeExiting += OnLiveNodeReleased;

        return true;
    }

    public void OnLiveNodeReleased()
    {
        TrySetSaveData();

        onRelease?.Invoke(liveNodeReference);
    }

    private void TrySetSaveData()
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
}