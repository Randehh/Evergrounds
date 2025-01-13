using Godot;

[GlobalClass]
public partial class WorldMap : Node2D
{
    public static WorldMap Instance;

    public override void _Ready()
    {
        Instance = this;
    }
}