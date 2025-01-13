using Godot;

[GlobalClass]
public partial class InteractEventPlayAnimation : InteractEvent
{

    [Export]
    private AnimationPlayer player;

    [Export]
    private string animationName;

    public override void Execute()
    {
        player.Play(animationName);
    }
}