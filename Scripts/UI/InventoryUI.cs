using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class InventoryUI : Control
{

    private const int slotSize = 34;
    private const int separatorSize = 4;

    [Export]
    private PackedScene inventorySlotScene;

    [Export]
    private VBoxContainer inventoryRowContainer;

    [Export]
    private Control expandParent;

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

    private float foldedY;
    private float expandedY;
    private bool isExpanded = false;

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

            // Stop dragging
            else if (draggingItem != null)
            {
                if (slotMouseOver != -1)
                {
                    var itemInSlot = inventory.GetItem(slotMouseOver);
                    if (itemInSlot != null)
                    {
                        if (itemInSlot.definition.itemType == draggingItem.definition.itemType &&
                            itemInSlot.CurrentStackSize < itemInSlot.definition.stackSize)
                        {
                            int spaceRemaining = itemInSlot.definition.stackSize - itemInSlot.CurrentStackSize;

                            // If dragging item count is more than space remaining, split
                            if (draggingItem.CurrentStackSize >= spaceRemaining)
                            {
                                itemInSlot.CurrentStackSize = itemInSlot.definition.stackSize;
                                draggingItem.CurrentStackSize -= spaceRemaining;
                                DragAndDrop.Instance.StartDragging(draggingItem);
                            }
                            else
                            {
                                itemInSlot.CurrentStackSize += draggingItem.CurrentStackSize;
                                DragAndDrop.Instance.StopDragging();
                            }
                        }
                        else
                        {
                            inventory.RemoveItem(slotMouseOver);
                            DragAndDrop.Instance.StopDragging();

                            inventory.SetItem(slotMouseOver, draggingItem.definition, draggingItem.CurrentStackSize);
                            DragAndDrop.Instance.StartDragging(itemInSlot);
                        }

                    }
                    else
                    {
                        inventory.SetItem(slotMouseOver, draggingItem.definition, draggingItem.CurrentStackSize);
                        DragAndDrop.Instance.StopDragging();
                    }
                }
                else
                {
                    WorldItemSpawner itemSpawner = new WorldItemSpawner(new WorldItemSpawnerItemData(draggingItem.definition, draggingItem.CurrentStackSize, draggingItem.CurrentStackSize, 1));
                    PlayerCharacter.Instance.AddChild(itemSpawner);
                    itemSpawner.GlobalPosition = PlayerCharacter.Instance.GlobalPosition;
                    DragAndDrop.Instance.StopDragging();
                }
            }
        }

        if(Input.IsActionJustPressed("inventory_toggle"))
        {
            isExpanded = !isExpanded;
        }

        expandParent.Position = new Vector2(expandParent.Position.X, (float)Mathf.Lerp(expandParent.Position.Y, isExpanded ? expandedY : foldedY, 20 * delta));
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
            }
            else
            {
                buttonData.label.Visible = false;
            }

            int slotIndex = i;
            slot.MouseEntered += () =>
            {
                slotMouseOver = slotIndex;

                if (slot.Texture != slotBackgroundSelected)
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

        foldedY = expandParent.Position.Y - slotSize - separatorSize;
        expandedY = foldedY - (inventoryRowContainer.GetChildren().Count * slotSize) - separatorSize;
    }

    private void SetSlot(int slot)
    {
        ButtonData buttonData = inventoryButtons[slot];
        InventoryItem inventoryItem = inventory.GetItem(slot);
        buttonData.textureRect.Texture = inventoryItem?.definition.itemSprite;
        buttonData.stackCountLabel.Text = inventoryItem?.CurrentStackSize.ToString();
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