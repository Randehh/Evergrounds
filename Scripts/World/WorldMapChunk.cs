using Godot;
using static WorldMap;
using static WorldMapTileDisplay;

public class WorldMapChunk
{
    private const string DISPLAY_TILE_MAP_SCENE_PATH = @"res://Scenes/World/display_tile_map.tscn";

    public AtlasMaterial SelectedMaterial => selectedMaterial;
    public bool IsMouseInChunk => isMouseInChunk;
    public Vector2I MouseMapPosition => mouseMapPosition;
    public Vector2I ChunkTilePosition => chunkTilePosition;

    public WorldMapTileDisplay standardTileMap;
    private WorldMapTileDisplay tillingTileMap;

    public bool isSet = false;

    private Vector2I chunkTilePosition;
    private Vector2I chunkPixelPosition;
    private AtlasMaterial selectedMaterial;
    private bool isMouseInChunk;
    private Vector2I mouseMapPosition;

    private PackedScene displayTileMapPackedScene;

    public WorldMapChunk(Node2D mapParent, WorldMapData mapData)
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

    public void SetChunkPosition(Vector2I chunkTilePosition, Vector2I chunkPixelPosition)
    {
        this.chunkTilePosition = chunkTilePosition;
        this.chunkPixelPosition = chunkPixelPosition;

        standardTileMap.DisplayLayer.Position = Vector2.Zero;
        tillingTileMap.DisplayLayer.Position = Vector2.Zero;

        standardTileMap.DisplayLayer.Clear();
        tillingTileMap.DisplayLayer.Clear();

        int startIndexX = chunkTilePosition.X;
        int endIndexX = startIndexX + WorldMap.CHUNK_SIZE;
        int startIndexY = chunkTilePosition.Y;
        int endIndexY = startIndexY + WorldMap.CHUNK_SIZE;

        for(int x = chunkTilePosition.X; x < endIndexX; x++)
        {
            for (int y = chunkTilePosition.Y; y < endIndexY; y++)
            {
                Vector2I coord = new Vector2I(x, y);
                standardTileMap.SetDisplayTile(coord);
                tillingTileMap.SetDisplayTile(coord);
            }
        }

        isSet = true;
    }

    public void UpdateSelectedMaterial(Vector2I mouseGridPosition)
    {
        selectedMaterial = AtlasMaterial.NONE;

        isMouseInChunk =
            mouseGridPosition.X >= chunkPixelPosition.X &&
            mouseGridPosition.X < chunkPixelPosition.X + (WorldMap.CHUNK_SIZE * WorldMap.GRID_SIZE) &&
            mouseGridPosition.Y >= chunkPixelPosition.Y &&
            mouseGridPosition.Y < chunkPixelPosition.Y + (WorldMap.CHUNK_SIZE * WorldMap.GRID_SIZE);

        if (!isMouseInChunk)
        {
            return;
        }

        mouseMapPosition = standardTileMap.DisplayLayer.LocalToMap(mouseGridPosition);

        standardTileMap.UpdateSelectedMaterial(mouseMapPosition);
        tillingTileMap.UpdateSelectedMaterial(mouseMapPosition);

        selectedMaterial = GetSelectedTileMaterial();
    }

    public bool TryGetSelectedMaterial(out AtlasMaterial material)
    {
        material = AtlasMaterial.NONE;

        if (!isMouseInChunk)
        {
            return false;
        }

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
        if(!IsMouseInChunk)
        {
            return false;
        }

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

    public void UpdateTileVisuals(Vector2I coord)
    {
        standardTileMap.SetDisplayTile(coord);
        tillingTileMap.SetDisplayTile(coord);
    }

    public void UpdateEdgeVisuals(WorldMapTileDisplayEdge edge)
    {
        standardTileMap.ResetEdge(chunkTilePosition, edge);
        tillingTileMap.ResetEdge(chunkTilePosition, edge);
    }
}