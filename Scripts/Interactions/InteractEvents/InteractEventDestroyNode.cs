using Godot;

[GlobalClass]
public partial class InteractEventDestroyNode : InteractEvent
{

    [Export]
    private Node2D toDestroy;

    public override void Execute()
    {
        ServiceLocator.GameNotificationService.OnNodeDestroyed.Execute(toDestroy);
        toDestroy.QueueFree();
    }
}