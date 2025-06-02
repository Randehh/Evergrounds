using Godot;

[GlobalClass]
public partial class InteractEventGiveExperience : InteractEvent
{

    [Export]
    private ExperienceType experienceType;

    [Export]
    private int experienceAmount = 1;

    public override void Execute()
    {
        ServiceLocator.ExperienceService.AddExperience(experienceType, experienceAmount);
    }
}