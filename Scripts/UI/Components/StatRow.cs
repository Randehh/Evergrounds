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

    public void SetData(Texture2D iconTexture, string typeName, string typeValue)
    {
        icon.Texture = iconTexture;
        typeLabel.Text = typeName;
        typeValueLabel.Text = typeValue;
    }
}