using Godot;

[GlobalClass]
public partial class EnableRandomChild : Node3D
{

    public override void _Ready()
    {
        Godot.Collections.Array<Node> children = GetChildren();
        int randomIndex = GD.RandRange(0, children.Count - 1);
        for (int i = 0; i < children.Count; i++)
        {
            if(i != randomIndex)
            {
                children[i].QueueFree();
            }
        }
    }
}