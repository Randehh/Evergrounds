using Godot;

[GlobalClass]
public partial class InteractEventTriggerNextDay : InteractEvent
{
    public override void Execute()
    {
        ServiceLocator.TimeService.TriggerNextDay(true);
    }
}