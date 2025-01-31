using Godot;
using System.Collections.Generic;
using static WorldMap;

public class WorldMapDataLayer
{
    private readonly AtlasMaterial defaultMaterial;
    private Dictionary<Vector2I, AtlasMaterial> layerValues = new();
    private Dictionary<Vector2I, Vector2I> tileSubSetValues = new();

    public WorldMapDataLayer(AtlasMaterial defaultMaterial)
    {
        this.defaultMaterial = defaultMaterial;
    }

    public AtlasMaterial GetTileData(Vector2I coords)
    {
        if (!layerValues.TryGetValue(coords, out var tileData))
        {
            return defaultMaterial;
        }

        return tileData;
    }

    public bool SetMaterial(Vector2I coords, AtlasMaterial material, Vector2I subTileSet)
    {

        foreach (Vector2I neighbour in WorldMapConstants.GridNeighbours)
        {
            tileSubSetValues[coords + neighbour] = subTileSet;
        }

        if (layerValues.TryGetValue(coords, out var layerAtlasValue) &&
            material == layerAtlasValue)
        {
            return false;
        }

        layerValues[coords] = material;
        return true;
    }

    public Vector2I GetSubTileSet(Vector2I coords)
    {
        if (tileSubSetValues.TryGetValue(coords, out var subTileSet))
        {
            return subTileSet;
        }

        return Vector2I.Zero;
    }

    public IEnumerable<Vector2I> GetAllUsedCoords()
    {
        return layerValues.Keys;
    }

    public Godot.Collections.Dictionary<string, Variant> GetSaveData()
    {
        Godot.Collections.Dictionary<string, Variant> layerData = new();

        Godot.Collections.Array<Variant> valueDataParent = new();
        layerData.Add("Values", valueDataParent);

        foreach (KeyValuePair<Vector2I, AtlasMaterial> valuePair in layerValues)
        {
            Godot.Collections.Dictionary<string, Variant> valueData = new();
            valueDataParent.Add(valueData);

            valueData.Add($"PosX", valuePair.Key.X);
            valueData.Add($"PosY", valuePair.Key.Y);
            valueData.Add($"Material", (int)valuePair.Value);
        }

        Godot.Collections.Array<Variant> tileSubSetParent = new();
        layerData.Add("TileSubSets", tileSubSetParent);

        foreach (KeyValuePair<Vector2I, Vector2I> valuePair in tileSubSetValues)
        {
            Godot.Collections.Dictionary<string, Variant> valueData = new();
            tileSubSetParent.Add(valueData);

            valueData.Add($"SubTileSetX", valuePair.Key.X);
            valueData.Add($"SubTileSetY", valuePair.Key.Y);
            valueData.Add($"SubTileSetValX", valuePair.Value.X);
            valueData.Add($"SubTileSetValY", valuePair.Value.Y);
        }
        return layerData;
    }

    public void SetSaveData(Godot.Collections.Dictionary<string, Variant> data)
    {
        layerValues.Clear();
        tileSubSetValues.Clear();

        foreach (Godot.Collections.Dictionary<string, Variant> tileDictionary in data["Values"].AsGodotArray<Godot.Collections.Dictionary<string, Variant>>())
        {
            layerValues.Add(new Vector2I(
                tileDictionary["PosX"].AsInt32(),
                tileDictionary["PosY"].AsInt32()),
                (AtlasMaterial)tileDictionary["Material"].AsInt32());
        }


        foreach (Godot.Collections.Dictionary<string, Variant> subSetDictionary in data["TileSubSets"].AsGodotArray<Godot.Collections.Dictionary<string, Variant>>())
        {
            tileSubSetValues.Add(
                new Vector2I(subSetDictionary["SubTileSetX"].AsInt32(), subSetDictionary["SubTileSetY"].AsInt32()),
                new Vector2I(subSetDictionary["SubTileSetValX"].AsInt32(), subSetDictionary["SubTileSetValY"].AsInt32()));
        }
    }
}

public class WorldMapDataLayerTile
{
    public AtlasMaterial material;
    public Vector2I subTileSet = Vector2I.Zero;
}