using Godot;

[GlobalClass]
public partial class EquipmentUI : Control
{

    [Export]
    private EquipmentSlot slotHead;

    [Export]
    private EquipmentSlot slotChest;

    [Export]
    private EquipmentSlot slotFeet;

    [Export]
    private EquipmentSlot slotTrinket;

    [Export]
    private PackedScene statRowScene;

    [Export]
    private Control statRowParent;

    private InventoryService inventory;
    private EquipmentSlot mouseOverSlot;

    public override void _Ready()
    {
        inventory = ServiceLocator.InventoryService;

        SetEquipmentSlot(slotHead, InventoryEquipmentType.HEAD);
        SetEquipmentSlot(slotChest, InventoryEquipmentType.CHEST);
        SetEquipmentSlot(slotFeet, InventoryEquipmentType.FEET);
        SetEquipmentSlot(slotTrinket, InventoryEquipmentType.TRINKET);

        CreateStatRow(null, "Movement speed", "2");
        CreateStatRow(null, "Attack speed", "1 per second");
        CreateStatRow(null, "Charisma", "12");
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustReleased("click")) {
            var draggingItem = DragAndDrop.Instance.DraggingItem;

            // Stop dragging
            if (draggingItem != null && mouseOverSlot != null && draggingItem.definition.inventoryEquipmentType == mouseOverSlot.EquipmentType) {
                mouseOverSlot.SetItem(draggingItem.definition);
                DragAndDrop.Instance.StopDragging();
            }
        }
    }

    private void SetEquipmentSlot(EquipmentSlot slot, InventoryEquipmentType slotType)
    {
        slot.SetEquipmentType(slotType);
        slot.SetItem(inventory.GetEquipment(slotType));

        slot.ItemSlot.MouseEntered += () => {
            mouseOverSlot = slot;
            slot.ItemSlot.SetBackgroundStateHover();
        };

        slot.ItemSlot.MouseExited += () => {
            mouseOverSlot = null;
            slot.ItemSlot.SetBackgroundStateDefault();
        };
    }

    private void CreateStatRow(Texture2D icon, string statDisplayName, string statValue)
    {
        StatRow row = statRowScene.Instantiate<StatRow>();
        row.SetData(icon, statDisplayName, statValue);
        statRowParent.AddChild(row);
    }
}