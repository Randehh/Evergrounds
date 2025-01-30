using Godot;

public static class InventoryItemExtensions
{

    private static Color COLOR_TEXT_TRASH = new Color(0.7f, 0.7f, 0.7f);
    private static Color COLOR_TEXT_COMMON = new Color(1, 1, 1);
    private static Color COLOR_TEXT_UNCOMMON = new Color(0.75f, 1f, 0.75f);
    private static Color COLOR_TEXT_RARE = new Color(0.75f, 0.75f, 1);

    public static Color GetTextColor(this InventoryItemRarity rarity)
    {
        switch (rarity)
        {
            case InventoryItemRarity.TRASH:
                return COLOR_TEXT_TRASH;

            case InventoryItemRarity.COMMON:
                return COLOR_TEXT_COMMON;

            case InventoryItemRarity.UNCOMMON:
                return COLOR_TEXT_UNCOMMON;

            case InventoryItemRarity.RARE:
                return COLOR_TEXT_RARE;

            default:
                return COLOR_TEXT_COMMON;
        }
    }

    public static Color GetSlotColor(this InventoryItemRarity rarity)
    {
        Color baseColor = rarity.GetTextColor();
        return baseColor.Lerp(new Color(1, 1, 1), 0.5f);
    }
}