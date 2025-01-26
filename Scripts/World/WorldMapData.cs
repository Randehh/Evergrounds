using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static WorldMap;

public class WorldMapData : IWorldSaveable
{
    private const string SAVE_KEY_LAYER_DATA = "LayerData";
    private const string SAVE_KEY_WORLD_NODE_DATA = "WorldNodeData";

    private Dictionary<WorldMapDataLayerType, WorldMapDataLayer> layers = new ();
    private HashSet<WorldNodeData> worldNodes = new ();

    private Dictionary<Node2D, WorldNodeData> liveNodes = new ();
    private Dictionary<WorldNodeData, Node2D> liveNodesInverse = new();

    public WorldMapData()
    {
        layers[WorldMapDataLayerType.BASE] = new WorldMapDataLayer(AtlasMaterial.SOFT_SURFACE);
        layers[WorldMapDataLayerType.TILLING] = new WorldMapDataLayer(AtlasMaterial.NONE);

        ServiceLocator.GameNotificationService.OnNodeDestroyed.OnFire += OnNodeDestroyed;
        ServiceLocator.GameNotificationService.OnNodeDespawned.OnFire += OnNodeDespawned;
    }

    ~WorldMapData()
    {
        ServiceLocator.GameNotificationService.OnNodeDestroyed.OnFire -= OnNodeDestroyed;
        ServiceLocator.GameNotificationService.OnNodeDespawned.OnFire -= OnNodeDespawned;
    }

    public bool SetMaterial(Vector2I coords, AtlasMaterial material, WorldMapDataLayerType layer) => layers[layer].SetMaterial(coords, material);
    public AtlasMaterial GetMaterial(Vector2I coords, WorldMapDataLayerType layer) => layers[layer].GetMaterial(coords);

    public IEnumerable<Vector2I> GetAllUsedCoords(WorldMapDataLayerType layer) => layers[layer].GetAllUsedCoords();

    private Dictionary<Vector2I, HashSet<WorldNodeData>> worldNodesInChunk = new();

    public void AddWorldNode(Node2D node)
    {
        if(!WorldNodeData.TryCreateFromLiveInstance(node, out var worldNodeData))
        {
            return;
        }

        worldNodes.Add(worldNodeData);
        liveNodes.Add(node, worldNodeData);
        liveNodesInverse.Add(worldNodeData, node);

        AddNodeToChunk(worldNodeData);
    }

    public void AddWorldNode(WorldNodeData worldNodeData, bool addToNodesInChunk)
    {
        worldNodes.Add(worldNodeData);

        if(addToNodesInChunk)
        {
            AddNodeToChunk(worldNodeData);
        }

        if (!worldNodeData.TrySpawn(out Node2D spawnedNode))
        {
            return;
        }

        liveNodes.Add(spawnedNode, worldNodeData);
        liveNodesInverse.Add(worldNodeData, spawnedNode);
    }

    private void OnNodeDestroyed(Node2D node)
    {
        if(!TryRemoveLiveNode(node, out WorldNodeData worldNodeDataRemoved))
        {
            return;
        }

        Vector2I chunkCoordinate = worldNodeDataRemoved.NodeChunkCoordinate;
        if (worldNodesInChunk.TryGetValue(chunkCoordinate, out HashSet<WorldNodeData> nodeDatas) && nodeDatas.Contains(worldNodeDataRemoved))
        {
            nodeDatas.Remove(worldNodeDataRemoved);
        }
    }

    private void OnNodeDespawned(Node2D node)
    {
        if (!TryRemoveLiveNode(node, out WorldNodeData worldNodeDataRemoved))
        {
            return;
        }

        worldNodeDataRemoved.TrySetSaveData();
    }

    private bool TryRemoveLiveNode(Node2D node, out WorldNodeData worldNodeDataRemoved)
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

    public void SpawnNodes(Vector2I chunkPosition)
    {
        if (!worldNodesInChunk.TryGetValue(chunkPosition, out HashSet<WorldNodeData> nodeDatas))
        {
            return;
        }

        foreach (WorldNodeData nodeData in nodeDatas)
        {
            if(liveNodesInverse.ContainsKey(nodeData))
            {
                continue;
            }

            AddWorldNode(nodeData, false);
        }
    }

    private void AddNodeToChunk(WorldNodeData nodeData)
    {
        Vector2I chunkCoordinate = nodeData.NodeChunkCoordinate;
        if (!worldNodesInChunk.TryGetValue(chunkCoordinate, out HashSet<WorldNodeData> nodeDatas))
        {
            nodeDatas = new HashSet<WorldNodeData>();
            worldNodesInChunk.Add(chunkCoordinate, nodeDatas);
        }

        if (nodeDatas.Contains(nodeData))
        {
            return;
        }

        nodeDatas.Add(nodeData);
    }

    public Godot.Collections.Dictionary<string, Variant> GetSaveData()
    {
        Godot.Collections.Dictionary<string, Variant> mapData = new();

        Godot.Collections.Dictionary<int, Variant> layerSaveData = new();
        mapData.Add(SAVE_KEY_LAYER_DATA, layerSaveData);
        foreach (KeyValuePair<WorldMapDataLayerType, WorldMapDataLayer> layerPair in layers)
        {
            layerSaveData.Add((int)layerPair.Key, layerPair.Value.GetSaveData());
        }

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
        worldNodesInChunk.Clear();

        Godot.Collections.Dictionary<int, Variant> layerSaveData = data[SAVE_KEY_LAYER_DATA].AsGodotDictionary<int, Variant>();
        foreach(KeyValuePair<int, Variant> layerData in layerSaveData)
        {
            WorldMapDataLayerType layerType = (WorldMapDataLayerType)layerData.Key;
            layers[layerType].SetSaveData(layerData.Value.AsGodotArray<Variant>());
        }

        Godot.Collections.Array<Variant> worldNodeDataArray = data[SAVE_KEY_WORLD_NODE_DATA].AsGodotArray<Variant>();
        foreach(Variant worldNodeDataVariant in worldNodeDataArray)
        {
            Godot.Collections.Dictionary<string, Variant> worldNodeSaveData = worldNodeDataVariant.AsGodotDictionary<string, Variant>();
            WorldNodeData worldNodeData = new WorldNodeData();
            worldNodeData.SetSaveData(worldNodeSaveData);
            
            AddWorldNode(worldNodeData, true);
        }
    }

    public enum WorldMapDataLayerType
    {
        BASE,
        TILLING
    }

    private class WorldMapDataLayer
    {
        private readonly AtlasMaterial defaultMaterial;
        private Dictionary<Vector2I, AtlasMaterial> layerValues = new();

        public WorldMapDataLayer(AtlasMaterial defaultMaterial)
        {
            this.defaultMaterial = defaultMaterial;
        }

        public AtlasMaterial GetMaterial(Vector2I coords)
        {
            if(!layerValues.TryGetValue(coords, out var material))
            {
                return defaultMaterial;
            }

            return material;
        }

        public bool SetMaterial(Vector2I coords, AtlasMaterial material)
        {
            if (layerValues.TryGetValue(coords, out var currentMaterial) && material == currentMaterial)
            {
                return false;
            }

            layerValues[coords] = material;
            return true;
        }

        public IEnumerable<Vector2I> GetAllUsedCoords()
        {
            return layerValues.Keys;
        }

        public Godot.Collections.Array<Variant> GetSaveData()
        {
            Godot.Collections.Array<Variant> layerData = new();
            foreach (KeyValuePair<Vector2I, AtlasMaterial> valuePair in layerValues)
            {
                Godot.Collections.Dictionary<string, Variant> valueData = new();
                layerData.Add(valueData);

                valueData.Add($"PosX", valuePair.Key.X);
                valueData.Add($"PosY", valuePair.Key.Y);
                valueData.Add($"Material", (int)valuePair.Value);
            }
            return layerData;
        }

        public void SetSaveData(Godot.Collections.Array<Variant> data)
        {
            layerValues.Clear();

            foreach (Variant tileVariantData in data)
            {
                Godot.Collections.Dictionary<string, Variant> tileDictionary = tileVariantData.AsGodotDictionary<string, Variant>();
                layerValues.Add(new Vector2I(
                    tileDictionary["PosX"].AsInt32(),
                    tileDictionary["PosY"].AsInt32()),
                    (AtlasMaterial)tileDictionary["Material"].AsInt32()
                    );
            }
        }
    }
}