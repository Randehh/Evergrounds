using Godot;

[GlobalClass]
public partial class InteractEventDestroyNode : InteractEvent
{

    [Export]
    private Node3D toDestroy;

    public override void Execute()
    {
        ServiceLocator.GameNotificationService.OnNodeDestroyed.Execute(toDestroy);
        toDestroy.QueueFree();
    }
}