using Godot;
using System.Collections.Generic;
using static WorldMap;
using static WorldMapTileDisplay;

public class WorldMapDisplay
{
    private const string DISPLAY_TILE_MAP_SCENE_PATH = @"res://Scenes/World/display_tile_map.tscn";

    public AtlasMaterial SelectedMaterial => selectedMaterial;
    public Vector2I MouseMapPosition => mouseMapPosition;

    public WorldMapTileDisplay standardTileMap;
    private WorldMapTileDisplay tillingTileMap;

    public bool isSet = false;

    private AtlasMaterial selectedMaterial;
    private Vector2I mouseMapPosition;

    private PackedScene displayTileMapPackedScene;

    public WorldMapDisplay(Node2D mapParent, WorldMapData mapData)
    {
        displayTileMapPackedScene = GD.Load<PackedScene>(DISPLAY_TILE_MAP_SCENE_PATH);

        standardTileMap = CreateTileDisplay(mapParent, mapData, AtlasMaterial.SOFT_SURFACE, AtlasMaterial.SOIL, WorldMapData.WorldMapDataLayerType.BASE, 0);
        tillingTileMap = CreateTileDisplay(mapParent, mapData, AtlasMaterial.NONE, AtlasMaterial.TILLED, WorldMapData.WorldMapDataLayerType.TILLING, 2);
    }

    private WorldMapTileDisplay CreateTileDisplay(Node2D mapParent, WorldMapData mapData, AtlasMaterial materialOne,  AtlasMaterial materialTwo, WorldMapData.WorldMapDataLayerType layerType, int atlasId)
    {
        TileMapLayer mapLayer = displayTileMapPackedScene.Instantiate<TileMapLayer>();
        mapParent.AddChild(mapLayer);
        mapLayer.Position = Vector2.Zero;

        return new WorldMapTileDisplay(mapData, mapLayer, materialOne, materialTwo, layerType, atlasId);
    }

    public void UpdateSelectedMaterial(Vector2I mouseGridPosition)
    {
        selectedMaterial = AtlasMaterial.NONE;

        mouseMapPosition = standardTileMap.DisplayLayer.LocalToMap(mouseGridPosition);

        standardTileMap.UpdateSelectedMaterial(mouseMapPosition);
        tillingTileMap.UpdateSelectedMaterial(mouseMapPosition);

        selectedMaterial = GetSelectedTileMaterial();
    }

    public bool TryGetSelectedMaterial(out AtlasMaterial material)
    {
        material = selectedMaterial;
        return true;
    }

    private AtlasMaterial GetSelectedTileMaterial()
    {
        if (tillingTileMap.SelectedTileMaterial != AtlasMaterial.NONE)
        {
            return tillingTileMap.SelectedTileMaterial;
        }
        else
        {
            return standardTileMap.SelectedTileMaterial;
        }
    }

    public bool CanChangeToTileMaterial(AtlasMaterial targetMaterial)
    {
        switch (targetMaterial)
        {
            case AtlasMaterial.SOFT_SURFACE:
                return standardTileMap.SelectedTileMaterial == AtlasMaterial.SOIL;

            case AtlasMaterial.SOIL:
                return standardTileMap.SelectedTileMaterial == AtlasMaterial.SOFT_SURFACE || tillingTileMap.SelectedTileMaterial == AtlasMaterial.TILLED;

            case AtlasMaterial.TILLED:
                return standardTileMap.SelectedTileMaterial == AtlasMaterial.SOIL && tillingTileMap.SelectedTileMaterial == AtlasMaterial.NONE;
        }

        return false;
    }

    public bool SetSelectedTile(AtlasMaterial targetMaterial)
    {
        return SetTile(mouseMapPosition, targetMaterial);
    }

    public bool SetTile(Vector2I mouseGridPosition, AtlasMaterial material)
    {
        if (!CanChangeToTileMaterial(material))
        {
            return false;
        }

        switch (material)
        {
            case AtlasMaterial.SOFT_SURFACE:
                standardTileMap.SetTile(mouseGridPosition, asTypeOne: true);
                break;

            case AtlasMaterial.SOIL:
                standardTileMap.SetTile(mouseGridPosition, asTypeOne: false);
                tillingTileMap.SetTile(mouseGridPosition, asTypeOne: true);
                break;

            case AtlasMaterial.TILLED:
                tillingTileMap.SetTile(mouseGridPosition, asTypeOne: false);
                break;
        }

        return true;
    }

    public void DisplayChunk(Vector2I chunkCoord)
    {
        standardTileMap.DisplayChunk(chunkCoord);
        tillingTileMap.DisplayChunk(chunkCoord);
    }

    public void HideChunk(Vector2I chunkCoord)
    {
        standardTileMap.HideChunk(chunkCoord);
        tillingTileMap.HideChunk(chunkCoord);
    }

    public void UpdateTileVisuals(Vector2I coord)
    {
        standardTileMap.SetDisplayTile(coord);
        tillingTileMap.SetDisplayTile(coord);
    }

    public void UpdateEdgeVisuals(WorldMapTileDisplayEdge edge)
    {
        //standardTileMap.ResetEdge(chunkTilePosition, edge);
        //tillingTileMap.ResetEdge(chunkTilePosition, edge);
    }

    public void ReplaceMapData(WorldMapData worldMapData, HashSet<Vector2I> chunks)
    {
        standardTileMap.ReplaceMapData(worldMapData, chunks);
        tillingTileMap.ReplaceMapData(worldMapData, chunks);
    }
}