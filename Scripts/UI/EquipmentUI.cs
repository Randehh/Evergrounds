using Evergrounds.Scripts.Utility;
using Godot;
using System;

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

        foreach (NumberType numberType in Enum.GetValues(typeof(NumberType))) {
            CreateStatRow(null, numberType);
        }
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustReleased("click")) {
            var draggingItem = DragAndDrop.Instance.DraggingItem;

            if (draggingItem == null && mouseOverSlot != null) {
                if (mouseOverSlot.ItemSlot.currentItem != null) {
                    DragAndDrop.Instance.StartDragging(new InventoryItem(mouseOverSlot.ItemSlot.currentItem));
                    mouseOverSlot.ItemSlot.SetItem(null);

                    ServiceLocator.InventoryService.SetEquipment(mouseOverSlot.EquipmentType, null);
                }
            }

            // Stop dragging
            else if (draggingItem != null && mouseOverSlot != null && draggingItem.definition.inventoryEquipmentType == mouseOverSlot.EquipmentType) {
                mouseOverSlot.SetItem(draggingItem.definition);
                DragAndDrop.Instance.StopDragging();

                ServiceLocator.InventoryService.SetEquipment(mouseOverSlot.EquipmentType, draggingItem.definition);
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

    private void CreateStatRow(Texture2D icon, NumberType numberType)
    {
        StatRow row = statRowScene.Instantiate<StatRow>();
        row.SetData(icon, numberType);
        statRowParent.AddChild(row);
    }
}