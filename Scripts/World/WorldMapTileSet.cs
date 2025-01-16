﻿using Godot;
using System;
using System.Collections.Generic;
using static WorldMap;
using static WorldMapData;

public class WorldMapTileSet
{
    public AtlasMaterial SelectedTileMaterial { get; private set; }

    private readonly Dictionary<Tuple<AtlasMaterial, AtlasMaterial, AtlasMaterial, AtlasMaterial>, Vector2I> neighboursToAtlasCoord;
    private readonly Vector2I[] neighbours = new Vector2I[] { new(0, 0), new(1, 0), new(0, 1), new(1, 1) };
    private readonly WorldMapData mapData;
    private readonly TileMapLayer displayLayer;
    private readonly AtlasMaterial typeOne;
    private readonly AtlasMaterial typeTwo;
    private readonly WorldMapDataLayerType layerType;
    private readonly int displayAtlasId;

    public WorldMapTileSet(WorldMapData mapData, TileMapLayer displayLayer, AtlasMaterial typeOne, AtlasMaterial typeTwo, WorldMapDataLayerType layerType, int displayAtlasId)
    {
        this.mapData = mapData;
        this.displayLayer = displayLayer;
        this.typeOne = typeOne;
        this.typeTwo = typeTwo;
        this.layerType = layerType;
        this.displayAtlasId = displayAtlasId;

        neighboursToAtlasCoord = new() {
            { new(typeOne, typeOne, typeOne, typeOne), new Vector2I(2, 1)}, // All corners
            { new(typeTwo, typeTwo, typeTwo, typeOne), new Vector2I(1, 3)}, // Outer bottom-right corner
            { new(typeTwo, typeTwo, typeOne, typeTwo), new Vector2I(0, 0)}, // Outer bottom-left corner
            { new(typeTwo, typeOne, typeTwo, typeTwo), new Vector2I(0, 2)}, // Outer top-right corner
            { new(typeOne, typeTwo, typeTwo, typeTwo), new Vector2I(3, 3)}, // Outer top-left corner
            { new(typeTwo, typeOne, typeTwo, typeOne), new Vector2I(1, 0)}, // Right edge
            { new(typeOne, typeTwo, typeOne, typeTwo), new Vector2I(3, 2)}, // Left edge
            { new(typeTwo, typeTwo, typeOne, typeOne), new Vector2I(3, 0)}, // Bottom edge
            { new(typeOne, typeOne, typeTwo, typeTwo), new Vector2I(1, 2)}, // Top edge
            { new(typeTwo, typeOne, typeOne, typeOne), new Vector2I(1, 1)}, // Inner bottom-right corner
            { new(typeOne, typeTwo, typeOne, typeOne), new Vector2I(2, 0)}, // Inner bottom-left corner
            { new(typeOne, typeOne, typeTwo, typeOne), new Vector2I(2, 2)}, // Inner top-right corner
            { new(typeOne, typeOne, typeOne, typeTwo), new Vector2I(3, 1)}, // Inner top-left corner
            { new(typeTwo, typeOne, typeOne, typeTwo), new Vector2I(2, 3)}, // Bottom-left top-right corners
            { new(typeOne, typeTwo, typeTwo, typeOne), new Vector2I(0, 1)}, // Top-left down-right corners
	    	{ new(typeTwo, typeTwo, typeTwo, typeTwo), new Vector2I(0, 3)}, // No corners
        };

        foreach (Vector2I coord in mapData.GetAllUsedCoords(layerType))
        {
            SetDisplayTile(coord);
        }
    }

    public void Process(Vector2I mouseGridPosition, double delta)
    {
        SelectedTileMaterial = mapData.GetMaterial(mouseGridPosition, layerType);
    }

    public void SetTile(Vector2I coords, bool asTypeOne)
    {
        mapData.SetMaterial(coords, asTypeOne ? typeOne : typeTwo, layerType);
        SetDisplayTile(coords);
    }

    private void SetDisplayTile(Vector2I pos)
    {
        // loop through 4 display neighbours
        for (int i = 0; i < neighbours.Length; i++)
        {
            Vector2I newPos = pos + neighbours[i];
            Vector2I displayTile = CalculateDisplayTile(newPos);
            displayLayer.SetCell(newPos, displayAtlasId, CalculateDisplayTile(newPos));
        }
    }

    private Vector2I CalculateDisplayTile(Vector2I coords)
    {
        // get 4 world tile neighbours
        AtlasMaterial botRight = GetWorldTile(coords - neighbours[0]);
        AtlasMaterial botLeft = GetWorldTile(coords - neighbours[1]);
        AtlasMaterial topRight = GetWorldTile(coords - neighbours[2]);
        AtlasMaterial topLeft = GetWorldTile(coords - neighbours[3]);

        // return tile (atlas coord) that fits the neighbour rules
        return neighboursToAtlasCoord[new(topLeft, topRight, botLeft, botRight)];
    }

    private AtlasMaterial GetWorldTile(Vector2I coords)
    {
        return mapData.GetMaterial(coords, layerType);
    }
}