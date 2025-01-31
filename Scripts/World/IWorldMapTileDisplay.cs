using Godot;
using System.Collections.Generic;

public interface IWorldMapTileDisplay
{
    public TileMapLayer DisplayLayer { get; }
    public WorldMap.AtlasMaterial SelectedTileData { get; }
    public void DisplayChunk(Vector2I chunkCoord);
    public WorldMap.AtlasMaterial GetMaterialAt(Vector2I gridPosition);
    public void HideChunk(Vector2I chunkCoord);
    public void ReplaceMapData(WorldMapData worldMapData, HashSet<Vector2I> chunks);
    public void SetDisplayTile(Vector2I pos);
    public void UpdateSelectedMaterial(Vector2I mouseGridPosition);
}