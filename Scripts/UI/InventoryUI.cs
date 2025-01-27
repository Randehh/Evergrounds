using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class InventoryUI : Control
{
    private readonly Color COLOR_TRANSPARENT = new Color(1, 1, 1, 0);
    private readonly Color COLOR_WHITE = new Color(1, 1, 1, 1);

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

    [Export]
    private Control itemInfoParent;

    [Export]
    private Label itemInfoLabel;

    [Export]
    private TextureRect itemInfoTexture;

    private InventoryService inventory;
    private int currentlyEquipped = 0;
    private List<ButtonData> inventoryButtons = new();
    private int slotMouseOver = -1;

    private float foldedY;
    private float expandedY;
    private bool isExpanded = false;

    public override void _Ready()
    {
        inventory = ServiceLocator.InventoryService;

        ReloadUI();

        EquipSlot(0);

        inventory.OnSelectQuickslot += (slot) => EquipSlot(slot);
        inventory.OnUpdateSlot += SetSlot;

        inventory.UpdateAllSlots();
    }

    public override void _Process(double delta)
    {
        for (int i = 0; i < InventoryService.QUICK_SELECT_COUNT; i++)
        {
            if (Input.IsActionJustPressed($"equip_{i}"))
            {
                int buttonIndex = i - 1;
                if (buttonIndex == -1)
                {
                    buttonIndex = 9;
                }
                ServiceLocator.InventoryService.EquipSlot(buttonIndex);
            }
        }

        if (Input.IsActionJustPressed("equip_next"))
        {
            ServiceLocator.InventoryService.EquipSlotNext();
        }

        if (Input.IsActionJustPressed("equip_previous"))
        {
            ServiceLocator.InventoryService.EquipSlotPrevious();
        }

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
                        if (itemInSlot.definition == draggingItem.definition &&
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
                    WorldItemSpawner itemSpawner = new WorldItemSpawner(new WorldItemSpawnerItemData[] {
                        new WorldItemSpawnerItemData(draggingItem.definition, draggingItem.CurrentStackSize, draggingItem.CurrentStackSize, 1)
                    });
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
        for(int i = 0; i < InventoryService.INVENTORY_SIZE; i++)
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

                SetQuickInfo(slotMouseOver);
            };

            slot.MouseExited += () =>
            {
                slotMouseOver = -1;

                if (slot.Texture != slotBackgroundSelected)
                {
                    slot.Texture = slotBackground;
                }

                SetQuickInfo(currentlyEquipped);
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

        SetQuickInfo(currentlyEquipped);
    }

    private void SetQuickInfo(int slot)
    {
        InventoryItem itemInfo = inventory.GetItem(slot);
        itemInfoParent.Modulate = itemInfo != null ? COLOR_WHITE : COLOR_TRANSPARENT;
        itemInfoTexture.Texture = itemInfo?.definition.itemSprite;
        itemInfoLabel.Text = $"{itemInfo?.definition.displayName} - {itemInfo?.definition.usageText}";
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