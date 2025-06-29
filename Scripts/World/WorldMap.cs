using Godot;

[GlobalClass]
public partial class WorldMap : Node3D, IWorldSaveable, IWorldMap
{
    private const string SAVE_KEY_MAP_DATA = "MapData";

    [Export]
    private WorldMapGenerator generator;

    private WorldMapData worldMapData = new WorldMapData();

    public WorldMap()
    {
        AddToGroup("worldmap");
    }

    public override void _Ready()
    {
        AddToGroup(WorldData.RecreateSaveGroup);

        for (int x = -2; x <= 2; x++)
        {
            for (int z = -2; z <= 2; z++)
            {
                generator.GenerateChunk(worldMapData, new Vector2I(x, z));
            }
        }
    }

    public void AddWorldNode(Node3D node, bool replaceParent, Vector3 replaceParentPosition)
    {
        if (replaceParent)
        {
            if (node.GetParent() != null)
            {
                node.GetParent().RemoveChild(node);
            }

            AddChild(node);
            node.Position = replaceParentPosition;
        }

        worldMapData.AddWorldNode(node);
    }

    // TODO: replace with bounding box check
    public bool CanPlaceNode(IWorldGridNode worldGridNode, Vector2I gridPosition) => true;

    public Godot.Collections.Dictionary<string, Variant> GetSaveData()
    {
        Godot.Collections.Dictionary<string, Variant> data = new();

        data.Add(SAVE_KEY_MAP_DATA, worldMapData.GetSaveData());

        return data;
    }

    public void SetSaveData(Godot.Collections.Dictionary<string, Variant> data)
    {
        worldMapData.SetSaveData(data[SAVE_KEY_MAP_DATA].AsGodotDictionary<string, Variant>());
    }

    public enum AtlasMaterial
    {
        NONE = -1,
        SOFT_SURFACE = 0,
        SOIL = 1,
        TILLED = 2,
        WALL = 3,

        FLOOR_WOOD = 1000,
        FLOOR_STONE = 1001
    }
}