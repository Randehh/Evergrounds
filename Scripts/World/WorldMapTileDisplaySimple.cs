using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using static WorldMap;
using static WorldMapData;

public class WorldMapTileDisplaySimple : IWorldMapTileDisplay
{
    public TileMapLayer DisplayLayer { get; private set; }
    public AtlasMaterial SelectedTileData { get; private set; }

    private WorldMapData mapData;
    private WorldMapDataLayerType layerType;
    private readonly int displayAtlasId;

    public WorldMapTileDisplaySimple(WorldMapData mapData, TileMapLayer displayLayer, WorldMapDataLayerType layerType, int displayAtlasId)
    {
        DisplayLayer = displayLayer;

        this.mapData = mapData;
        this.layerType = layerType;
        this.displayAtlasId = displayAtlasId;
    }

    public void ReplaceMapData(WorldMapData worldMapData, HashSet<Vector2I> chunks)
    {
        mapData = worldMapData;

        foreach (Vector2I chunk in chunks)
        {
            DisplayChunk(chunk);
        }
    }

    public void DisplayChunk(Vector2I chunkCoord)
    {
        foreach (Vector2I tileCoord in GetTilesOfChunk(chunkCoord))
        {
            SetDisplayTile(tileCoord);
        }
    }

    public void HideChunk(Vector2I chunkCoord)
    {
        foreach (Vector2I tileCoord in GetTilesOfChunk(chunkCoord))
        {
            DisplayLayer.EraseCell(tileCoord);
        }
    }

    private IEnumerable<Vector2I> GetTilesOfChunk(Vector2I chunkCoord)
    {
        Vector2I chunkWorldPosition = chunkCoord * WorldMap.CHUNK_SIZE;
        for (int x = 0; x < WorldMap.CHUNK_SIZE; x++)
        {
            for (int y = 0; y < WorldMap.CHUNK_SIZE; y++)
            {
                Vector2I coord = new Vector2I(chunkWorldPosition.X + x, chunkWorldPosition.Y + y);
                yield return coord;
            }
        }
    }

    public void UpdateSelectedMaterial(Vector2I mouseGridPosition)
    {
        SelectedTileData = mapData.GetMaterial(mouseGridPosition, layerType);
    }

    public AtlasMaterial GetMaterialAt(Vector2I gridPosition)
    {
        return mapData.GetMaterial(gridPosition, layerType);
    }

    public bool SetTile(Vector2I coords, AtlasMaterial material, Vector2I subTileSet)
    {
        bool isUpdated = mapData.SetMaterial(coords, material, layerType, subTileSet);
        return isUpdated;
    }

    public void SetDisplayTile(Vector2I pos)
    {
        AtlasMaterial material = mapData.GetMaterial(pos, layerType);
        DisplayLayer.SetCell(pos, displayAtlasId, GetAtlasPosition(material));
    }

    private Vector2I GetAtlasPosition(AtlasMaterial material)
    {
        switch(material)
        {
            case AtlasMaterial.FLOOR_WOOD:
                return new Vector2I(1, 0);

            case AtlasMaterial.FLOOR_STONE:
                return new Vector2I(2, 0);

            default:
                return Vector2I.Zero;
        }
    }

    public enum WorldMapTileDisplayEdge
    {
        LEFT,
        RIGHT,
        UP,
        DOWN
    }
}