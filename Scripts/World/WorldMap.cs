using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class WorldMap : Node2D
{
    public const int GRID_SIZE = 16;

    public static WorldMap Instance;

    public Vector2I MouseGridPosition { get; private set; }
    public bool IsSelectedTileTilled => tillingTileMap.SelectedTileMaterial == AtlasMaterial.TILLED;

    [Export]
    private TileMapLayer defaultTileMapLayer;

    [Export]
    private TileMapLayer displayTileMapLayer;

    [Export]
    private TileMapLayer tillingTileMapLayer;

    [Export]
    private TileMapLayer tillingDisplayTileMapLayer;

    private WorldMapTileSet standardTileMap;
    private WorldMapTileSet tillingTileMap;

    public override void _Ready()
    {
        Instance = this;
        standardTileMap = CreateTileSet(defaultTileMapLayer, displayTileMapLayer, AtlasMaterial.SOIL, AtlasMaterial.SOFT_SURFACE, 1, 0);
        tillingTileMap = CreateTileSet(tillingTileMapLayer, tillingDisplayTileMapLayer, AtlasMaterial.TILLED, AtlasMaterial.NONE, 1, 2);
    }

    public override void _Process(double delta)
    {
        MouseGridPosition = defaultTileMapLayer.LocalToMap(GetMouseCoordinates(1));

        standardTileMap.Process(MouseGridPosition, delta);
        tillingTileMap.Process(MouseGridPosition, delta);
    }

    private WorldMapTileSet CreateTileSet(TileMapLayer dataLayer, TileMapLayer displayLayer, AtlasMaterial materialOne, AtlasMaterial materialTwo, int dataAtlasId, int displayAtlasId)
    {
        return new WorldMapTileSet(dataLayer, displayLayer, materialOne, materialTwo, dataAtlasId, displayAtlasId);
    }

    public bool CanChangeToTileMaterial(AtlasMaterial targetMaterial)
    {
        switch(targetMaterial)
        {
            case AtlasMaterial.SOFT_SURFACE:
                return standardTileMap.SelectedTileMaterial == AtlasMaterial.SOIL;

            case AtlasMaterial.SOIL:
                return standardTileMap.SelectedTileMaterial == AtlasMaterial.SOFT_SURFACE;

            case AtlasMaterial.TILLED:
                return standardTileMap.SelectedTileMaterial == AtlasMaterial.SOIL && tillingTileMap.SelectedTileMaterial == AtlasMaterial.NONE;
        }

        return false;
    }

    public void SetSelectedTile(AtlasMaterial material)
    {
        if(!CanChangeToTileMaterial(material))
        {
            return;
        }

        switch(material)
        {
            case AtlasMaterial.SOFT_SURFACE:
                standardTileMap.SetTile(MouseGridPosition, asTypeOne: true);
                break;

            case AtlasMaterial.SOIL:
                standardTileMap.SetTile(MouseGridPosition, asTypeOne: false);
                break;

            case AtlasMaterial.TILLED:
                tillingTileMap.SetTile(MouseGridPosition, asTypeOne: false);
                break;
        }
    }

    public Vector2I GetMouseCoordinates(int divisions)
    {
        int size = 16 / divisions;
        Vector2 globalMousePosition = GetGlobalMousePosition();
        int xInt = Mathf.FloorToInt(globalMousePosition.X / size) * size;
        int yInt = Mathf.FloorToInt(globalMousePosition.Y / size) * size;
        Vector2I result = new(xInt, yInt);
        return new Vector2I(xInt, yInt) + new Vector2I(size / 2, size / 2);
    }

    public enum AtlasMaterial
    {
        NONE = -1,
        SOFT_SURFACE = 0,
        SOIL = 1,
        TILLED = 2,
    }
}