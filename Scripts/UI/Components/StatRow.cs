using Evergrounds.Scripts.Utility;
using Godot;

[GlobalClass]
public partial class StatRow : Control
{
    [Export]
    private TextureRect icon;

    [Export]
    private Label typeLabel;

    [Export]
    private Label typeValueLabel;

    private NumberType numberType;

    public override void _ExitTree()
    {
        ServiceLocator.NumberService.onNumberUpdated -= OnNumberUpdated;
    }

    public void SetData(Texture2D iconTexture, NumberType numberType)
    {
        this.numberType = numberType;

        float currentValue = ServiceLocator.NumberService.GetCalculatedValue(numberType);

        icon.Texture = iconTexture;
        typeLabel.Text = numberType.ToString().FirstCharToUpper().Replace('_', ' ');
        typeValueLabel.Text = currentValue.ToString();

        ServiceLocator.NumberService.onNumberUpdated += OnNumberUpdated;
    }

    private void OnNumberUpdated(NumberType numberType, float value)
    {
        if (this.numberType != numberType) {
            return;
        }

        typeValueLabel.Text = value.ToString();
    }
}