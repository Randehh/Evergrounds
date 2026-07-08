using Evergrounds.Scripts.Utility;
using Godot;
using System;

[GlobalClass]
public partial class EquipmentSlot : Control
{
    [Export]
    private ItemSlotComponent itemSlot;

    [Export]
    private Label equipmentTypeLabel;

    [Export]
    private Label equipmentNameLabel;

    public Action<InventoryItemDefinition> OnItemSet = delegate { };
    public ItemSlotComponent ItemSlot => itemSlot;

    public InventoryEquipmentType EquipmentType => equipmentType;
    private InventoryEquipmentType equipmentType;

    public override void _Ready()
    {
        base._Ready();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
    }

    public void SetEquipmentType(InventoryEquipmentType equipmentType)
    {
        this.equipmentType = equipmentType;
        equipmentTypeLabel.Text = equipmentType.ToString().FirstCharToUpper();
    }

    public void SetItem(InventoryItemDefinition itemDefinition)
    {
        itemSlot.SetItem(itemDefinition, 1);

        equipmentNameLabel.Text = itemDefinition?.displayName ?? "-";

        OnItemSet.Invoke(itemDefinition);
    }
}