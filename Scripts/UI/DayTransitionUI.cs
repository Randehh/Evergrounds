using Godot;
using System;

[GlobalClass]
public partial class DayTransitionUI : Control
{
    [Export]
    private Label dayLabel;

    [Export]
    private AnimationPlayer animationPlayer;

    private Action onCompleteCallback;

    public override void _Ready()
    {
        ServiceLocator.GameNotificationService.OnTriggerDayTransition.OnFire += StartTransition;
    }

    private void StartTransition(Action onCompleteCallback)
    {
        this.onCompleteCallback = onCompleteCallback;

        dayLabel.Text = $"Day {ServiceLocator.TimeService.currentDay}";

        animationPlayer.Play("transition");
    }

    public void TriggerNextDay()
    {
        onCompleteCallback.Invoke();

        dayLabel.Text = $"Day {ServiceLocator.TimeService.currentDay}";
    }
}