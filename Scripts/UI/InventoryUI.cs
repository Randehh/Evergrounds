using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class InventoryUI : Control
{

    [Export]
    private PackedScene inventorySlotScene;

    [Export]
    private VBoxContainer inventoryRowContainer;

    [Export]
    private Texture2D slotBackground;

    [Export]
    private Texture2D slotBackgroundHover;

    [Export]
    private Texture2D slotBackgroundSelected;

    private Inventory inventory;
    private int currentlyEquipped = 0;
    private List<ButtonData> inventoryButtons = new();
    private int slotMouseOver = -1;

    public override void _Ready()
    {
        inventory = Inventory.Instance;

        ReloadUI();

        EquipSlot(0);

        inventory.OnSelectQuickslot += (slot) => EquipSlot(slot);
        inventory.OnUpdateSlot += SetSlot;

        inventory.EmitQuickslotCallbacks();
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustReleased("click"))
        {
            var draggingItem = DragAndDrop.Instance.DraggingItem;

            // Start dragging
            if (draggingItem == null && slotMouseOver != -1)
            {
                DragAndDrop.Instance.StartDragging(inventory.GetItem(slotMouseOver));
                inventory.RemoveItem(slotMouseOver);
            }
            else if (draggingItem != null)
            {
                if (slotMouseOver != -1)
                {
                    inventory.SetItem(slotMouseOver, draggingItem.definition, draggingItem.currentStackSize);
                }
                else
                {
                    WorldItemSpawner itemSpawner = new WorldItemSpawner(new WorldItemSpawnerItemData(draggingItem.definition, draggingItem.currentStackSize, draggingItem.currentStackSize, 1));
                    PlayerCharacter.Instance.AddChild(itemSpawner);
                    itemSpawner.GlobalPosition = PlayerCharacter.Instance.GlobalPosition;
                }

                DragAndDrop.Instance.StopDragging();
            }
        }
    }

    private void ReloadUI()
    {
        inventoryButtons.Clear();

        var children = inventoryRowContainer.GetChildren();
        foreach(var child in children)
        {
            inventoryRowContainer.RemoveChild(child);
            child.Free();
        }

        HBoxContainer row = null;
        for(int i = 0; i < Inventory.INVENTORY_SIZE; i++)
        {
            if(i % 10 == 0)
            {
                row = new HBoxContainer();
                row.Alignment = BoxContainer.AlignmentMode.Center;
                inventoryRowContainer.AddChild(row);
            }

            TextureRect slot = inventorySlotScene.Instantiate<TextureRect>();
            row.AddChild(slot);

            ButtonData buttonData = new ButtonData(slot);
            inventoryButtons.Add(buttonData);

            buttonData.stackCountLabel.Visible = false;

            if (i < 10)
            {
                buttonData.label.Text = GetEquipmentSlotDisplayIndex(i).ToString();

                int slotIndex = i;
                slot.MouseEntered += () =>
                {
                    slotMouseOver = slotIndex;

                    if(slot.Texture != slotBackgroundSelected)
                    {
                        slot.Texture = slotBackgroundHover;
                    }
                };

                slot.MouseExited += () =>
                {
                    slotMouseOver = -1;

                    if (slot.Texture != slotBackgroundSelected)
                    {
                        slot.Texture = slotBackground;
                    }
                };
            }
            else
            {
                buttonData.label.Visible = false;
            }
        }
    }

    private void SetSlot(int slot)
    {
        ButtonData buttonData = inventoryButtons[slot];
        InventoryItem inventoryItem = inventory.GetItem(slot);
        buttonData.textureRect.Texture = inventoryItem?.definition.itemSprite;
        buttonData.stackCountLabel.Text = inventoryItem?.currentStackSize.ToString();
        buttonData.stackCountLabel.Visible = inventoryItem?.definition.isStackable ?? false;
    }

    private void EquipSlot(int slot)
    {
        inventoryButtons[currentlyEquipped].buttonRect.Texture = slotBackground;
        inventoryButtons[slot].buttonRect.Texture = slotBackgroundSelected;

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
        public TextureRect buttonRect;
        public TextureRect textureRect;
        public Label label;
        public Label stackCountLabel;

        public ButtonData(TextureRect buttonRect)
        {
            this.buttonRect = buttonRect;
            textureRect = buttonRect.GetNode("ItemSprite") as TextureRect;
            label = buttonRect.GetNode("NumberLabel") as Label;
            stackCountLabel = buttonRect.GetNode("StackCountLabel") as Label;
        }
    }
}