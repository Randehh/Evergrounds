using Godot;
using Godot.Collections;

public interface IWorldSaveable
{

    public Dictionary<string, Variant> GetSaveData();
}