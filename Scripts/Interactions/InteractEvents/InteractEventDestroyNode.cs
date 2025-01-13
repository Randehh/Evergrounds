using Godot;

[GlobalClass]
public partial class InteractEventDestroyNode : InteractEvent
{

    [Export]
    private Node toDestroy;

    public override void Execute()
    {
        toDestroy.QueueFree();
    }
}