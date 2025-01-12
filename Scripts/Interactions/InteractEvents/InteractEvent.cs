using Godot;

[GlobalClass]
public abstract partial class InteractEvent : Node2D
{
    public abstract void Execute();
}