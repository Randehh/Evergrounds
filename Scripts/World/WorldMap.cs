using Godot;

[GlobalClass]
public partial class WorldMap : Node2D
{

    public const int GRID_SIZE = 16;

    public static WorldMap Instance;

    public override void _Ready()
    {
        Instance = this;
    }
}