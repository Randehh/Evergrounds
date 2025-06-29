using Godot;
using Godot.Collections;

public interface IWorldMap
{
    void AddWorldNode(Node3D node, bool replaceParent, Vector3 replaceParentPosition);
    Dictionary<string, Variant> GetSaveData();
    void SetSaveData(Dictionary<string, Variant> data);
}