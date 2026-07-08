using Godot;

[GlobalClass]
public partial class NumberMod : Resource
{
    [Export]
    public string id;

    [Export]
    public NumberType numberType;

    [Export]
    public NumberModType modType;

    [Export]
    public float modValue;

    public NumberMod() { }
}
