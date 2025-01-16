using Godot;
using System.Collections.Generic;
using static WorldMap;

public class WorldMapData
{
    private Dictionary<WorldMapDataLayerType, WorldMapDataLayer> layers = new ();

    public WorldMapData()
    {
        layers[WorldMapDataLayerType.BASE] = new WorldMapDataLayer(AtlasMaterial.SOFT_SURFACE);
        layers[WorldMapDataLayerType.TILLING] = new WorldMapDataLayer(AtlasMaterial.NONE);
    }

    public bool SetMaterial(Vector2I coords, AtlasMaterial material, WorldMapDataLayerType layer) => layers[layer].SetMaterial(coords, material);
    public AtlasMaterial GetMaterial(Vector2I coords, WorldMapDataLayerType layer) => layers[layer].GetMaterial(coords);

    public IEnumerable<Vector2I> GetAllUsedCoords(WorldMapDataLayerType layer) => layers[layer].GetAllUsedCoords();

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
    }
}