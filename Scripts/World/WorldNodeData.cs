using Godot;
using Godot.Collections;

public class WorldNodeData : IWorldSaveable
{
    public static bool TryCreateFromLiveInstance(Node3D node, out WorldNodeData worldNodeData)
    {
        worldNodeData = new WorldNodeData();
        bool succeeded = worldNodeData.SetLiveNodeReference(node);
        return succeeded;
    }

    private const string SAVE_KEY_FILEPATH = "FilePath";
    private const string SAVE_KEY_PARENT = "Parent";
    private const string SAVE_KEY_POS_X = "PosX";
    private const string SAVE_KEY_POS_Y = "PosY";
    private const string SAVE_KEY_POS_Z = "PosY";
    private const string SAVE_KEY_METADATA = "Metadata";

    public Node3D LiveNode => liveNodeReference;
    public bool IsValid => isValid;

    private Node3D liveNodeReference;
    private IWorldSaveable liveWorldSaveable;

    private string sceneFilePath;
    private string parentPath;
    private Vector3 lastKnownPosition;
    private Dictionary<string, Variant> metadata;
    private bool isValid = true;

    public bool TrySpawn(out Node3D spawnedNode)
    {
        spawnedNode = null;

        if (!PlayerDespawnableHandler.Instance.IsPointInArea(lastKnownPosition))
        {
            return false;
        }

        var newObjectScene = GD.Load<PackedScene>(sceneFilePath);
        spawnedNode = newObjectScene.Instantiate<Node3D>();
        GroupUtility.GetWorldMap<WorldMap>().AddChild(spawnedNode);
        spawnedNode.Set(Node2D.PropertyName.Position, lastKnownPosition);

        (spawnedNode as IWorldSaveable).SetSaveData(metadata);

        SetLiveNodeReference(spawnedNode);

        return true;
    }

    public bool SetLiveNodeReference(Node3D liveNode)
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

        ServiceLocator.GameNotificationService.OnNodeSpawned.Execute(this);

        return true;
    }

    public void Invalidate()
    {
        isValid = false;
    }

    public void TrySetSaveData()
    {
        if (liveNodeReference == null || !GodotObject.IsInstanceValid(liveNodeReference))
        {
            return;
        }

        lastKnownPosition = liveNodeReference.Position;

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
        saveDataDictionary.Add(SAVE_KEY_POS_X, lastKnownPosition.X);
        saveDataDictionary.Add(SAVE_KEY_POS_Y, lastKnownPosition.Y);
        saveDataDictionary.Add(SAVE_KEY_POS_Z, lastKnownPosition.Z);
        saveDataDictionary.Add(SAVE_KEY_METADATA, metadata);

        return saveDataDictionary;
    }

    public void SetSaveData(Dictionary<string, Variant> data)
    {
        sceneFilePath = data[SAVE_KEY_FILEPATH].AsString();
        parentPath = data[SAVE_KEY_PARENT].AsString();

        lastKnownPosition = new Vector3(
            (float)data[SAVE_KEY_POS_X].AsDouble(),
            (float)data[SAVE_KEY_POS_Y].AsDouble(),
            (float)data[SAVE_KEY_POS_Z].AsDouble()
            );

        metadata = data[SAVE_KEY_METADATA].AsGodotDictionary<string, Variant>();
    }
}