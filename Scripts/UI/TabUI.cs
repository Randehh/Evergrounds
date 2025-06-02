using Godot;

[GlobalClass]
public partial class TabUI : Control
{

    [Export]
    private Control expandParent;

    private float foldedY;
    private float expandedY;
    private bool isExpanded = false;

    public override void _Ready()
    {
        base._Ready();

        foldedY = -310;
        expandedY = 100;
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("inventory_toggle"))
        {
            isExpanded = !isExpanded;

            if (isExpanded)
            {
                ServiceLocator.InputStateService.PushInputState(InputState.UI);
            }
            else
            {
                ServiceLocator.InputStateService.PopInputState();
            }
        }

        expandParent.Position = new Vector2(expandParent.Position.X, (float)Mathf.Lerp(expandParent.Position.Y, isExpanded ? expandedY : foldedY, 20 * delta));
    }
}