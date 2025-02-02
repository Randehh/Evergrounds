using Godot;

[GlobalClass]
public partial class ServiceSerializer : Node, IWorldSaveable
{
    private const string SAVE_KEY_INVENTORY = "Inventory";
    private const string SAVE_KEY_TIME = "Time";
    private const string SAVE_KEY_EXPERIENCE = "Experience";

    public override void _Ready()
    {
        AddToGroup(WorldData.ReloadSaveGroup);
    }

    public Godot.Collections.Dictionary<string, Variant> GetSaveData()
    {
        return new Godot.Collections.Dictionary<string, Variant>()
        {
            { SAVE_KEY_INVENTORY, ServiceLocator.InventoryService.GetSaveData() },
            { SAVE_KEY_TIME, ServiceLocator.TimeService.GetSaveData() },
            { SAVE_KEY_EXPERIENCE, ServiceLocator.ExperienceService.GetSaveData() },
        };
    }

    public void SetSaveData(Godot.Collections.Dictionary<string, Variant> data)
    {
        ServiceLocator.InventoryService.SetSaveData(data[SAVE_KEY_INVENTORY].AsGodotDictionary<string, Variant>());
        ServiceLocator.TimeService.SetSaveData(data[SAVE_KEY_TIME].AsGodotDictionary<string, Variant>());
        ServiceLocator.ExperienceService.SetSaveData(data[SAVE_KEY_EXPERIENCE].AsGodotDictionary<string, Variant>());
    }
}
