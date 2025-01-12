using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class InventoryUI : Control
{

    [Export]
    private HBoxContainer inventorySlotsParent;

    private Inventory inventory;
    private int currentlyEquipped = 0;
    private List<ButtonData> inventoryButtons = new();

    public override void _Ready()
    {
        inventory = Inventory.Instance;

        int buttonNumber = 0;
        foreach (var child in inventorySlotsParent.GetChildren())
        {
            if (child is not Button inventoryButton)
            {
                continue;
            }

            ButtonData buttonData = new ButtonData(inventoryButton);
            inventoryButtons.Add(buttonData);
            buttonData.label.Text = GetEquipmentSlotDisplayIndex(buttonNumber).ToString();

            int slotIndex = buttonNumber;
            inventoryButton.ButtonDown += () => EquipSlot(slotIndex);

            buttonNumber++;
        }

        EquipSlot(0);

        inventory.OnSelectQuickslot += (slot) => EquipSlot(slot);
        inventory.OnSetQuickslot += SetSlot;

        inventory.EmitQuickslotCallbacks();
    }

    private void SetSlot(int slot, int inventoryIndex)
    {
        ButtonData buttonData = inventoryButtons[slot];
        InventoryItem inventoryItem = inventory.GetItem(inventoryIndex);
        buttonData.textureRect.Texture = inventoryItem?.definition.itemSprite;

        GD.Print($"{slot} = {inventoryIndex} -> {inventoryItem?.definition.itemType}");
    }

    private void EquipSlot(int slot)
    {
        inventoryButtons[currentlyEquipped].button.SetPressedNoSignal(false);
        inventoryButtons[slot].button.EmitSignal(Button.SignalName.Pressed, true);
        inventoryButtons[slot].button.EmitSignal(Button.SignalName.FocusEntered, true);
        inventoryButtons[slot].button.SetPressedNoSignal(true);

        currentlyEquipped = slot;
    }

    private int GetEquipmentSlotDisplayIndex(int slot)
    {
        int buttonIndex = slot + 1;
        if (buttonIndex == 10)
        {
            buttonIndex = 0;
        }

        return buttonIndex;
    }

    private struct ButtonData
    {
        public Button button;
        public TextureRect textureRect;
        public Label label;

        public ButtonData(Button button)
        {
            this.button = button;
            textureRect = button.GetNode("ItemSprite") as TextureRect;
            label = button.GetNode("NumberLabel") as Label;
        }
    }
}