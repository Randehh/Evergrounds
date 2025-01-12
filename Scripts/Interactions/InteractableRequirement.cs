using Godot;

[GlobalClass]
public partial class InteractableRequirement : Resource
{
    [Export]
    public InteractType interactType;

    [Export]
    public int minimumLevel = 0;

    public InteractableRequirement() { }
}