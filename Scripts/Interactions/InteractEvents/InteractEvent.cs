using Godot;

[GlobalClass]
public abstract partial class InteractEvent : Node3D
{
    public abstract void Execute();
}