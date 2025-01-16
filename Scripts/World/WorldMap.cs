using Godot;

[GlobalClass]
public partial class WorldMap : Node2D
{
    public const int GRID_SIZE = 16;

    public static WorldMap Instance;

    public Vector2I MouseGridPosition { get; private set; }
    public bool IsSelectedTileTilled => tillingTileMap.SelectedTileMaterial == AtlasMaterial.TILLED;

    [Export]
    private TileMapLayer displayTileMapLayer;

    [Export]
    private TileMapLayer tillingDisplayTileMapLayer;

    [Export]
    private WorldMapGenerator generator;

    private WorldMapData worldMapData;
    private WorldMapTileSet standardTileMap;
    private WorldMapTileSet tillingTileMap;

    public override void _Ready()
    {
        Instance = this;

        worldMapData = new WorldMapData();

        generator.GenerateInitialChunks(worldMapData);

        standardTileMap = CreateTileSet(worldMapData, displayTileMapLayer, AtlasMaterial.SOFT_SURFACE, AtlasMaterial.SOIL, WorldMapData.WorldMapDataLayerType.BASE, 0);
        tillingTileMap = CreateTileSet(worldMapData, tillingDisplayTileMapLayer, AtlasMaterial.NONE, AtlasMaterial.TILLED, WorldMapData.WorldMapDataLayerType.TILLING, 2);
    }

    public override void _Process(double delta)
    {
        MouseGridPosition = displayTileMapLayer.LocalToMap(GetMouseCoordinates(1));

        standardTileMap.Process(MouseGridPosition, delta);
        tillingTileMap.Process(MouseGridPosition, delta);
    }

    private WorldMapTileSet CreateTileSet(WorldMapData mapData, TileMapLayer displayLayer, AtlasMaterial materialOne, AtlasMaterial materialTwo, WorldMapData.WorldMapDataLayerType layerType, int displayAtlasId)
    {
        return new WorldMapTileSet(mapData, displayLayer, materialOne, materialTwo, layerType, displayAtlasId);
    }

    public AtlasMaterial GetSelectedTileMaterial()
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