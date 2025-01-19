using Godot;
using System;

[GlobalClass]
public partial class PlayerInteractHandler : Node2D
{
    private readonly Color colorWhite = new Color(1, 1, 1, 1);
    private readonly Color colorHalfAlpha = new Color(1, 1, 1, 0.5f);

    private readonly Color colorValid = new Color(0.25f, 1, 0.25f, 0.5f);
    private readonly Color colorInvalid = new Color(1, 0.25f, 0.25f, 0.5f);

    [Export]
    private CharacterBase character;

    [Export]
    private PlayerInteractDetector interactDetector;

    [Export]
    private Node2D selectArrow;

    [Export]
    private Sprite2D previewSprite;

    [Export]
    private Sprite2D gridTileView;

    // Default interactions
    private Interactable currentInteracable;
    private InteractResult currentInteractResult;

    // Grid interactions
    private Vector2I currentGridPosition;
    private string currentLoadedScenePath = string.Empty;
    private PackedScene currentlyLoadedPackedScene;
    private Node2D currentLoadedScene;
    private Vector2 gridPreviewNodeOffset = Vector2.Zero;
    private double alphaTimer;

    private InteractType previousInteractType;

    public override void _Ready()
    {
        base._Ready();

        (selectArrow.FindChild("ArrowAnimator") as AnimationPlayer).Play("Highlight");
    }

    public void ProcessInteraction(InventoryItem itemInHand, double delta)
    {
        InteractType interactType = itemInHand?.definition.interactType ?? InteractType.NONE;

        // Check if we need to reset indicators
        bool activateIndicator = false;
        if(interactType != previousInteractType)
        {
            selectArrow.Visible = false;
            previewSprite.Visible = false;
            gridTileView.Visible = false;

            alphaTimer = 0;

            activateIndicator = true;
        }

        switch (interactType)
        {
            case InteractType.NONE:
            case InteractType.CHOP_TREE:
            case InteractType.MINE_ORE:
            case InteractType.HARVEST_PLANTS:
                HandleDefaultInteract(itemInHand);
                break;

            case InteractType.SET_TILE_MATERIAL:
                HandleSetTileMaterial(itemInHand, delta);

                if (activateIndicator)
                {
                    gridTileView.Visible = true;
                }
                break;

            case InteractType.GRID_SELECT:
                HandleGridSelection(itemInHand, 1);

                if (activateIndicator)
                {
                    previewSprite.Visible = true;
                }
                break;

            case InteractType.GRID_SELECT_HALF:
                HandleGridSelection(itemInHand, 2);

                if (activateIndicator)
                {
                    previewSprite.Visible = true;
                }
                break;
        }

        previousInteractType = interactType;

        alphaTimer += delta;
    }

    private void HandleDefaultInteract(InventoryItem itemInHand)
    {
        // Update the current interactable and the arrow
        Interactable currentInteracable = interactDetector.SelectedInteractable;

        if (currentInteracable == null)
        {
            selectArrow.Visible = false;
            currentInteractResult = InteractResult.NO_INTERACTABLE;
            return;
        }

        if (interactDetector.IsInRange)
        {
            selectArrow.Modulate = colorWhite;
            currentInteractResult = currentInteracable.GetInteractResult(character.CurrentlyHolding);
        }
        else
        {
            selectArrow.Modulate = colorHalfAlpha;
            currentInteractResult = InteractResult.OUT_OF_RANGE;
        }

        selectArrow.GlobalPosition = currentInteracable.GetArrowAnchor();
        selectArrow.Visible = true;

        // Execute interact
        if (IsUseHoldablePressed())
        {
            if (currentInteractResult == InteractResult.OK)
            {
                character.UseHoldable();
                interactDetector.SelectedInteractable.Interact();
            }
            else if (currentInteractResult == InteractResult.NO_INTERACTABLE)
            {
                character.UseHoldable();
            }
        }
    }

    private void HandleSetTileMaterial(InventoryItem itemInHand, double delta)
    {
        bool canChangeMaterial = WorldMap.Instance.CanChangeToTileMaterial(itemInHand.definition.tileMaterial);

        gridTileView.GlobalPosition = WorldMap.Instance.GetMouseCoordinates(1, true);
        gridTileView.Modulate = (canChangeMaterial ? colorValid : colorInvalid).WithAlpha(SinBetween(0.25f, 0.75f));

        if (canChangeMaterial && IsUseHoldablePressed())
        {
            WorldMap.Instance.SetSelectedTile(itemInHand.definition.tileMaterial, true);
            character.UseHoldable();
        }
    }

    private float SinBetween(float min, float max)
    {
        return ((max - min) * (float)Mathf.Sin(alphaTimer * 5) + max + min) / 2;
    }

    private void HandleGridSelection(InventoryItem itemInHand, int gridDivision)
    {
        string scenePath = itemInHand.definition.gridPlacementScene;
        if (string.IsNullOrWhiteSpace(scenePath))
        {
            GD.PrintErr($"Scene path for item {itemInHand.definition.ResourceName} is null or empty");
            return;
        }

        if(!currentLoadedScenePath.Equals(scenePath))
        {
            if(currentLoadedScene != null)
            {
                currentLoadedScene.QueueFree();
            }

            currentlyLoadedPackedScene = GD.Load<PackedScene>(scenePath);
            currentLoadedScene = currentlyLoadedPackedScene.Instantiate<Node2D>();
            currentLoadedScenePath = scenePath;

            if(currentLoadedScene is not IWorldGridNode gridNode) {
                GD.PrintErr($"Scene at path {scenePath} does not inherit from IWorldGridNode");
                return;
            }

            previewSprite.Texture = gridNode.gridPlacementPreviewSprite.Texture;
            gridPreviewNodeOffset = gridNode.gridPlacementOffset;
        }

        Vector2I gridPosition = WorldMap.Instance.GetMouseCoordinates(gridDivision, true);
        currentGridPosition = gridPosition;
        previewSprite.GlobalPosition = currentGridPosition + gridPreviewNodeOffset;

        bool canPlace = itemInHand.definition.validTilePlacementMaterials.Contains(WorldMap.Instance.GetSelectedMaterial());
        previewSprite.Modulate = (canPlace ? colorValid : colorInvalid).WithAlpha(SinBetween(0.25f, 0.75f));

        if(IsUseHoldablePressed() && canPlace)
        {
            Node2D placedNode = currentlyLoadedPackedScene.Instantiate<Node2D>();
            WorldMap.Instance.AddChild(placedNode);
            placedNode.GlobalPosition = currentGridPosition;

            Inventory.Instance.RemoveItem(itemInHand, 1);
        }
    }

    private bool IsUseHoldablePressed() => Input.IsActionJustPressed("use_holdable");
}
