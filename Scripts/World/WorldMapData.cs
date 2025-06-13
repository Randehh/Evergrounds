using Godot;
using System.Collections.Generic;
using static WorldMap;

public class WorldMapData : IWorldSaveable
{
    private const string SAVE_KEY_LAYER_DATA = "LayerData";
    private const string SAVE_KEY_WORLD_NODE_DATA = "WorldNodeData";

    private HashSet<WorldNodeData> worldNodes = new();

    private Dictionary<Node3D, WorldNodeData> liveNodes = new();
    private Dictionary<WorldNodeData, Node3D> liveNodesInverse = new();

    public void AddWorldNode(Node3D node)
    {
        if (!WorldNodeData.TryCreateFromLiveInstance(node, out var worldNodeData))
        {
            return;
        }

        worldNodes.Add(worldNodeData);
        liveNodes.Add(node, worldNodeData);
        liveNodesInverse.Add(worldNodeData, node);
    }

    public void AddWorldNode(WorldNodeData worldNodeData)
    {
        worldNodes.Add(worldNodeData);

        if (!worldNodeData.TrySpawn(out Node3D spawnedNode))
        {
            return;
        }

        liveNodes.Add(spawnedNode, worldNodeData);
        liveNodesInverse.Add(worldNodeData, spawnedNode);
    }

    private void OnNodeDestroyed(Node3D node)
    {
        if (!TryRemoveLiveNode(node, out WorldNodeData worldNodeDataRemoved))
        {
            return;
        }
    }

    private void OnNodeDespawned(Node3D node)
    {
        if (!TryRemoveLiveNode(node, out WorldNodeData worldNodeDataRemoved))
        {
            return;
        }

        worldNodeDataRemoved.TrySetSaveData();
    }

    private bool TryRemoveLiveNode(Node3D node, out WorldNodeData worldNodeDataRemoved)
    {
        worldNodeDataRemoved = null;
        if (!liveNodes.ContainsKey(node))
        {
            return false;
        }

        worldNodeDataRemoved = liveNodes[node];

        worldNodes.Remove(worldNodeDataRemoved);
        liveNodes.Remove(node);
        liveNodesInverse.Remove(worldNodeDataRemoved);
        return true;
    }

    public Godot.Collections.Dictionary<string, Variant> GetSaveData()
    {
        Godot.Collections.Dictionary<string, Variant> mapData = new();

        Godot.Collections.Dictionary<int, Variant> layerSaveData = new();
        mapData.Add(SAVE_KEY_LAYER_DATA, layerSaveData);

        Godot.Collections.Array<Variant> worldNodeDataArray = new();
        mapData.Add(SAVE_KEY_WORLD_NODE_DATA, worldNodeDataArray);
        foreach (WorldNodeData worldNodeData in worldNodes)
        {
            Godot.Collections.Dictionary<string, Variant> worldNodeSaveData = worldNodeData.GetSaveData();
            worldNodeDataArray.Add(worldNodeSaveData);
        }
        return mapData;
    }

    public void SetSaveData(Godot.Collections.Dictionary<string, Variant> data)
    {
        liveNodes.Clear();
        liveNodesInverse.Clear();

        Godot.Collections.Array<Variant> worldNodeDataArray = data[SAVE_KEY_WORLD_NODE_DATA].AsGodotArray<Variant>();
        foreach (Variant worldNodeDataVariant in worldNodeDataArray)
        {
            Godot.Collections.Dictionary<string, Variant> worldNodeSaveData = worldNodeDataVariant.AsGodotDictionary<string, Variant>();
            WorldNodeData worldNodeData = new WorldNodeData();
            worldNodeData.SetSaveData(worldNodeSaveData);

            AddWorldNode(worldNodeData);
        }
    }

    public enum WorldMapDataLayerType
    {
        BASE,
        TILLING,
        WALLS,
        FLOORS
    }
}