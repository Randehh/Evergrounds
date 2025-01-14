using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class WorldMap : Node2D
{
    public const int GRID_SIZE = 16;

    public static WorldMap Instance;

    public AtlasMaterial SelectedTileMaterial { get; private set; }
    public Vector2I MouseGridPosition { get; private set; }

    [Export]
    private TileMapLayer defaultTileMap;

    [Export]
    private TileMapLayer displayTileMap;

    public override void _Ready()
    {
        Instance = this;

        foreach (Vector2I coord in defaultTileMap.GetUsedCells())
        {
            SetDisplayTile(coord);
        }
    }

    public override void _Process(double delta)
    {
        MouseGridPosition = defaultTileMap.LocalToMap(GetMouseCoordinates());

        TileData tileData = defaultTileMap.GetCellTileData(MouseGridPosition);
        if(tileData == null)
        {
            return;
        }

        SelectedTileMaterial = (AtlasMaterial)tileData.GetCustomData("material").AsInt32();
    }

    public bool CanChangeToTileMaterial(AtlasMaterial targetMaterial)
    {
        switch(targetMaterial)
        {
            case AtlasMaterial.TILLABLE:
                return SelectedTileMaterial == AtlasMaterial.TILLED;

            case AtlasMaterial.TILLED:
                return SelectedTileMaterial == AtlasMaterial.TILLABLE;
        }

        return false;
    }

    public void SetSelectedTile(AtlasMaterial material)
    {
        if(!CanChangeToTileMaterial(material))
        {
            return;
        }

        Vector2I atlasCoord = Vector2I.Zero;
        switch(material)
        {
            case AtlasMaterial.TILLABLE: atlasCoord = grassPlaceholderAtlasCoord; break;
            case AtlasMaterial.TILLED: atlasCoord = dirtPlaceholderAtlasCoord; break;
        }

        SetTile(MouseGridPosition, atlasCoord);
    }

    public Vector2 GetMouseCoordinates()
    {
        Vector2 globalMousePosition = GetGlobalMousePosition();
        int xInt = Mathf.FloorToInt(globalMousePosition.X / 16) * 16;
        int yInt = Mathf.FloorToInt(globalMousePosition.Y / 16) * 16;
        Vector2I result = new(xInt, yInt);
        return result + new Vector2(8, 8);
    }

    [Export] 
    private Vector2I grassPlaceholderAtlasCoord;

    [Export] 
    private Vector2I dirtPlaceholderAtlasCoord;

    private readonly Vector2I[] NEIGHBOURS = new Vector2I[] { new(0, 0), new(1, 0), new(0, 1), new(1, 1) };

    private readonly Dictionary<Tuple<AtlasMaterial, AtlasMaterial, AtlasMaterial, AtlasMaterial>, Vector2I> neighboursToAtlasCoord = new() {
        { new(AtlasMaterial.TILLABLE, AtlasMaterial.TILLABLE, AtlasMaterial.TILLABLE, AtlasMaterial.TILLABLE), new Vector2I(2, 1)}, // All corners
        { new(AtlasMaterial.TILLED, AtlasMaterial.TILLED, AtlasMaterial.TILLED, AtlasMaterial.TILLABLE), new Vector2I(1, 3)}, // Outer bottom-right corner
        { new(AtlasMaterial.TILLED, AtlasMaterial.TILLED, AtlasMaterial.TILLABLE, AtlasMaterial.TILLED), new Vector2I(0, 0)}, // Outer bottom-left corner
        { new(AtlasMaterial.TILLED, AtlasMaterial.TILLABLE, AtlasMaterial.TILLED, AtlasMaterial.TILLED), new Vector2I(0, 2)}, // Outer top-right corner
        { new(AtlasMaterial.TILLABLE, AtlasMaterial.TILLED, AtlasMaterial.TILLED, AtlasMaterial.TILLED), new Vector2I(3, 3)}, // Outer top-left corner
        { new(AtlasMaterial.TILLED, AtlasMaterial.TILLABLE, AtlasMaterial.TILLED, AtlasMaterial.TILLABLE), new Vector2I(1, 0)}, // Right edge
        { new(AtlasMaterial.TILLABLE, AtlasMaterial.TILLED, AtlasMaterial.TILLABLE, AtlasMaterial.TILLED), new Vector2I(3, 2)}, // Left edge
        { new(AtlasMaterial.TILLED, AtlasMaterial.TILLED, AtlasMaterial.TILLABLE, AtlasMaterial.TILLABLE), new Vector2I(3, 0)}, // Bottom edge
        { new(AtlasMaterial.TILLABLE, AtlasMaterial.TILLABLE, AtlasMaterial.TILLED, AtlasMaterial.TILLED), new Vector2I(1, 2)}, // Top edge
        { new(AtlasMaterial.TILLED, AtlasMaterial.TILLABLE, AtlasMaterial.TILLABLE, AtlasMaterial.TILLABLE), new Vector2I(1, 1)}, // Inner bottom-right corner
        { new(AtlasMaterial.TILLABLE, AtlasMaterial.TILLED, AtlasMaterial.TILLABLE, AtlasMaterial.TILLABLE), new Vector2I(2, 0)}, // Inner bottom-left corner
        { new(AtlasMaterial.TILLABLE, AtlasMaterial.TILLABLE, AtlasMaterial.TILLED, AtlasMaterial.TILLABLE), new Vector2I(2, 2)}, // Inner top-right corner
        { new(AtlasMaterial.TILLABLE, AtlasMaterial.TILLABLE, AtlasMaterial.TILLABLE, AtlasMaterial.TILLED), new Vector2I(3, 1)}, // Inner top-left corner
        { new(AtlasMaterial.TILLED, AtlasMaterial.TILLABLE, AtlasMaterial.TILLABLE, AtlasMaterial.TILLED), new Vector2I(2, 3)}, // Bottom-left top-right corners
        { new(AtlasMaterial.TILLABLE, AtlasMaterial.TILLED, AtlasMaterial.TILLED, AtlasMaterial.TILLABLE), new Vector2I(0, 1)}, // Top-left down-right corners
		{ new(AtlasMaterial.TILLED, AtlasMaterial.TILLED, AtlasMaterial.TILLED, AtlasMaterial.TILLED), new Vector2I(0, 3)}, // No corners
    };

    public void SetTile(Vector2I coords, Vector2I atlasCoords)
    {
        defaultTileMap.SetCell(coords, 1, atlasCoords);
        SetDisplayTile(coords);
    }

    void SetDisplayTile(Vector2I pos)
    {
        // loop through 4 display neighbours
        for (int i = 0; i < NEIGHBOURS.Length; i++)
        {
            Vector2I newPos = pos + NEIGHBOURS[i];
            Vector2I displayTile = CalculateDisplayTile(newPos);
            displayTileMap.SetCell(newPos, 0, CalculateDisplayTile(newPos));
        }
    }

    private Vector2I CalculateDisplayTile(Vector2I coords)
    {
        // get 4 world tile neighbours
        AtlasMaterial botRight = GetWorldTile(coords - NEIGHBOURS[0]);
        AtlasMaterial botLeft = GetWorldTile(coords - NEIGHBOURS[1]);
        AtlasMaterial topRight = GetWorldTile(coords - NEIGHBOURS[2]);
        AtlasMaterial topLeft = GetWorldTile(coords - NEIGHBOURS[3]);

        // return tile (atlas coord) that fits the neighbour rules
        return neighboursToAtlasCoord[new(topLeft, topRight, botLeft, botRight)];
    }

    private AtlasMaterial GetWorldTile(Vector2I coords)
    {
        Vector2I atlasCoord = defaultTileMap.GetCellAtlasCoords(coords);
        if (atlasCoord == grassPlaceholderAtlasCoord)
        {
            return AtlasMaterial.TILLABLE;
        }
        else
        {
            return AtlasMaterial.TILLED;
        }
    }

    public enum AtlasMaterial
    {
        TILLABLE = 0,
        TILLED = 1,
    }
}