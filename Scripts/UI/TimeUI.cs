using Godot;

[GlobalClass]
public partial class TimeUI : Control
{
    [Export]
    private Label dayLabel;

    [Export]
    private Label timeLabel;

    [Export]
    private Control dial;

    public override void _Ready()
    {
        ServiceLocator.GameNotificationService.OnTimeUpdated.OnFire += OnTimeUpdated;
    }

    private void OnTimeUpdated(TimeUpdatePayload payload)
    {
        dayLabel.Text = $"Day {payload.day}";
        timeLabel.Text = payload.displayString;
        dial.RotationDegrees = (float)(360 / payload.normalizedTime) - 180;
    }
}