using Godot;

[GlobalClass]
public partial class WorldMapStatic : Node3D, IWorldSaveable, IWorldMap
{

    public WorldMapStatic()
    {
        AddToGroup("worldmap");
    }

    public override void _Ready()
    {
        AddToGroup(WorldData.RecreateSaveGroup);
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
    }

    public bool CanPlaceNode(IWorldGridNode worldGridNode, Vector2I gridPosition) => false;

    public Godot.Collections.Dictionary<string, Variant> GetSaveData()
    {
        Godot.Collections.Dictionary<string, Variant> data = new();


        return data;
    }

    public void SetSaveData(Godot.Collections.Dictionary<string, Variant> data)
    {

    }
}