using Godot;
using System.Collections.Generic;
using static WorldMap;

public class WorldMapData : IWorldSaveable
{
    private Dictionary<WorldMapDataLayerType, WorldMapDataLayer> layers = new ();
    private List<WorldNodeData> worldNodes = new ();

    private Dictionary<Node2D, WorldNodeData> liveNodes = new ();

    public WorldMapData()
    {
        layers[WorldMapDataLayerType.BASE] = new WorldMapDataLayer(AtlasMaterial.SOFT_SURFACE);
        layers[WorldMapDataLayerType.TILLING] = new WorldMapDataLayer(AtlasMaterial.NONE);
    }

    public bool SetMaterial(Vector2I coords, AtlasMaterial material, WorldMapDataLayerType layer) => layers[layer].SetMaterial(coords, material);
    public AtlasMaterial GetMaterial(Vector2I coords, WorldMapDataLayerType layer) => layers[layer].GetMaterial(coords);

    public IEnumerable<Vector2I> GetAllUsedCoords(WorldMapDataLayerType layer) => layers[layer].GetAllUsedCoords();

    public void AddWorldNode(Node2D node)
    {
        if(!WorldNodeData.TryCreateFromLiveInstance(node, RemoveWorldNode, out var worldNodeData))
        {
            return;
        }

        worldNodes.Add(worldNodeData);
        liveNodes.Add(node, worldNodeData);
    }

    public void RemoveWorldNode(Node2D node)
    {
        if(!liveNodes.ContainsKey(node))
        {
            return;
        }

        liveNodes.Remove(node);
    }

    public Godot.Collections.Dictionary<string, Variant> GetSaveData()
    {
        Godot.Collections.Dictionary<string, Variant> mapData = new();

        Godot.Collections.Dictionary<string, Variant> layerSaveData = new();
        mapData.Add("LayerData", layerSaveData);
        foreach (KeyValuePair<WorldMapDataLayerType, WorldMapDataLayer> layerPair in layers)
        {
            layerSaveData.Add(layerPair.Key.ToString(), layerPair.Value.GetSaveData());
        }

        Godot.Collections.Array<Variant> worldNodeDataArray = new();
        mapData.Add("WorldNodeData", worldNodeDataArray);
        foreach (WorldNodeData worldNodeData in worldNodes)
        {
            Godot.Collections.Dictionary<string, Variant> worldNodeSaveData = worldNodeData.GetSaveData();
            worldNodeDataArray.Add(worldNodeSaveData);
        }
        return mapData;
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
                valueData.Add($"Material", valuePair.Value.ToString());
            }
            return layerData;
        }
    }
}