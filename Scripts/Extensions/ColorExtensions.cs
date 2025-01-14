using Godot;

public static class ColorExtensions
{

    public static Color WithAlpha(this Color color, float alpha)
    {
        return new Color(color, alpha);
    }
}