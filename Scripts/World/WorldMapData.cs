using Godot;
using System.Collections.Generic;
using static WorldMap;

public class WorldMapData : IWorldSaveable
{
    private const string SAVE_KEY_LAYER_DATA = "LayerData";
    private const string SAVE_KEY_WORLD_NODE_DATA = "WorldNodeData";

    private Dictionary<WorldMapDataLayerType, WorldMapDataLayer> layers = new();
    private HashSet<WorldNodeData> worldNodes = new();

    private Dictionary<Node2D, WorldNodeData> liveNodes = new();
    private Dictionary<WorldNodeData, Node2D> liveNodesInverse = new();

    private Dictionary<Vector2I, WorldNodeData> occupiedGridSpaces = new();
    private Dictionary<WorldNodeData, List<Vector2I>> occupiedGridSpacesInverse = new();

    private Dictionary<Vector2I, HashSet<WorldNodeData>> worldNodesInChunk = new();

    public WorldMapData()
    {
        layers[WorldMapDataLayerType.BASE] = new WorldMapDataLayer(AtlasMaterial.SOFT_SURFACE);
        layers[WorldMapDataLayerType.TILLING] = new WorldMapDataLayer(AtlasMaterial.NONE);
        layers[WorldMapDataLayerType.WALLS] = new WorldMapDataLayer(AtlasMaterial.NONE);
        layers[WorldMapDataLayerType.FLOORS] = new WorldMapDataLayer(AtlasMaterial.NONE);

        ServiceLocator.GameNotificationService.OnNodeDestroyed.OnFire += OnNodeDestroyed;
        ServiceLocator.GameNotificationService.OnNodeDespawned.OnFire += OnNodeDespawned;
    }

    ~WorldMapData()
    {
        ServiceLocator.GameNotificationService.OnNodeDestroyed.OnFire -= OnNodeDestroyed;
        ServiceLocator.GameNotificationService.OnNodeDespawned.OnFire -= OnNodeDespawned;
    }

    public bool SetMaterial(Vector2I coords, AtlasMaterial material, WorldMapDataLayerType layer, Vector2I subTileSet) => layers[layer].SetMaterial(coords, material, subTileSet);
    public AtlasMaterial GetMaterial(Vector2I coords, WorldMapDataLayerType layer) => layers[layer].GetTileData(coords);

    public Vector2I GetSubTileSet(Vector2I coords, WorldMapDataLayerType layer) => layers[layer].GetSubTileSet(coords);

    public void AddWorldNode(Node2D node)
    {
        if (!WorldNodeData.TryCreateFromLiveInstance(node, out var worldNodeData))
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

        if (addToNodesInChunk)
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
        if (!TryRemoveLiveNode(node, out WorldNodeData worldNodeDataRemoved))
        {
            return;
        }


        Vector2I chunkCoordinate = worldNodeDataRemoved.NodeChunkCoordinate;
        if (worldNodesInChunk.TryGetValue(chunkCoordinate, out HashSet<WorldNodeData> nodeDatas) && nodeDatas.Contains(worldNodeDataRemoved))
        {
            nodeDatas.Remove(worldNodeDataRemoved);
        }
        else
        {
            worldNodeDataRemoved.Invalidate();
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

        if (occupiedGridSpacesInverse.TryGetValue(worldNodeDataRemoved, out List<Vector2I> worldNodeSpaces))
        {
            foreach (Vector2I nodeSpace in worldNodeSpaces)
            {
                if (occupiedGridSpaces.ContainsKey(nodeSpace))
                {
                    occupiedGridSpaces.Remove(nodeSpace);
                }
            }

            occupiedGridSpacesInverse.Remove(worldNodeDataRemoved);
        }
        return true;
    }

    public void SpawnNodes(Vector2I chunkPosition)
    {
        if (!worldNodesInChunk.TryGetValue(chunkPosition, out HashSet<WorldNodeData> nodeDatas))
        {
            return;
        }

        List<WorldNodeData> invalidNodes = new();
        foreach (WorldNodeData nodeData in nodeDatas)
        {
            if (!nodeData.IsValid)
            {
                invalidNodes.Add(nodeData);
                continue;
            }

            if (liveNodesInverse.ContainsKey(nodeData))
            {
                continue;
            }

            AddWorldNode(nodeData, false);
        }

        foreach (WorldNodeData nodeData in invalidNodes)
        {
            nodeDatas.Remove(nodeData);
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

        if (nodeData.LiveNode != null && nodeData.LiveNode is IWorldGridNode gridNode)
        {
            Vector2I nodeGridCoordinate = WorldMap.Instance.GetGridCoordinates(nodeData.LiveNode.Position);
            List<Vector2I> nodeSpaces = new();
            foreach (Vector2I gridOffsetPosition in gridNode.gridOffsetPositions)
            {
                Vector2I nodeSpace = nodeGridCoordinate + gridOffsetPosition;

                occupiedGridSpaces.Add(nodeSpace, nodeData);
                nodeSpaces.Add(nodeSpace);
            }
            occupiedGridSpacesInverse.Add(nodeData, nodeSpaces);
        }
    }

    public bool CanPlaceNode(IWorldGridNode worldGridNode, Vector2I gridPosition)
    {
        foreach (Vector2I tileOffset in worldGridNode.gridOffsetPositions)
        {
            Vector2I gridOffsetPosition = gridPosition + tileOffset;

            if (occupiedGridSpaces.ContainsKey(gridOffsetPosition))
            {
                return false;
            }

            if (worldGridNode.placementMaterials.Count > 0)
            {
                AtlasMaterial material = WorldMap.Instance.GetMaterialAt(gridOffsetPosition);
                if (!worldGridNode.placementMaterials.Contains(material))
                {
                    return false;
                }
            }
        }

        return true;
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
        foreach (KeyValuePair<int, Variant> layerData in layerSaveData)
        {
            WorldMapDataLayerType layerType = (WorldMapDataLayerType)layerData.Key;
            layers[layerType].SetSaveData(layerData.Value.AsGodotDictionary<string, Variant>());
        }

        Godot.Collections.Array<Variant> worldNodeDataArray = data[SAVE_KEY_WORLD_NODE_DATA].AsGodotArray<Variant>();
        foreach (Variant worldNodeDataVariant in worldNodeDataArray)
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
        TILLING,
        WALLS,
        FLOORS
    }
}