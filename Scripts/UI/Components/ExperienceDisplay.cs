using Godot;

[GlobalClass]
public partial class ExperienceDisplay : Control
{
    [Export]
    private ProgressBar progressBar;

    [Export]
    private Label progressLabel;

    [Export]
    private ExperienceType trackingType;

    public override void _Ready()
    {
        ServiceLocator.GameNotificationService.OnExperienceUpdated.OnFire += OnExperienceUpdated;

        SetTypeToTrack(trackingType);
    }

    public override void _ExitTree()
    {
        ServiceLocator.GameNotificationService.OnExperienceUpdated.OnFire -= OnExperienceUpdated;
    }

    public void SetTypeToTrack(ExperienceType type)
    {
        trackingType = type;

        ExperienceUpdatePayload payload = ServiceLocator.ExperienceService.GetPayload(type);
        OnExperienceUpdated(payload);
    }

    private void OnExperienceUpdated(ExperienceUpdatePayload payload)
    {
        if(payload.experienceType != trackingType)
        {
            return;
        }

        progressBar.Value = payload.normalizedExperience;
        progressLabel.Text = $"{payload.experienceType} Lv{payload.animatedLevel} - {payload.animatedExperience}/{payload.targetAnimatedExperience}";
    }
}