using Godot;
using System;

[GlobalClass]
public partial class PlayerInteractHandler : Node2D
{
    public const float INTERACT_RADIUS = 40;

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
    private IWorldGridNode currentlyLoadedWorldGridNode;
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
                HandleGridSelection(itemInHand, 1);

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
            currentInteractResult = currentInteracable.GetInteractResult(character.CurrentlyHolding);
        }
        else
        {
            currentInteractResult = InteractResult.OUT_OF_RANGE;
        }

        float alpha = 1 - ((currentInteracable.Position.DistanceTo(PlayerCharacter.Instance.Position) - INTERACT_RADIUS) / INTERACT_RADIUS * 0.5f);
        alpha = Mathf.Clamp(alpha, 0, 1);
        selectArrow.Modulate = colorWhite.WithAlpha(alpha);

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

        Vector2 gridPosition = WorldMap.Instance.GetMouseCoordinates(1, true);
        gridTileView.GlobalPosition = gridPosition;

        Color previewColorBase = canChangeMaterial ? colorValid : colorInvalid;
        float alphaMin = 0.4f;
        float alphaMax = 0.75f;
        if ((gridPosition).DistanceTo(PlayerCharacter.Instance.Position) >= INTERACT_RADIUS)
        {
            canChangeMaterial = false;

            alphaMin = 0.1f;
            alphaMax = 0.25f;
        }

        Color previewColor = previewColorBase.WithAlpha(SinBetween(alphaMin, alphaMax));
        gridTileView.Modulate = previewColor;

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

            currentlyLoadedWorldGridNode = gridNode;

            previewSprite.Texture = currentlyLoadedWorldGridNode.gridPlacementPreviewSprite.Texture;
            gridPreviewNodeOffset = currentlyLoadedWorldGridNode.gridPlacementOffset;
        }

        Vector2I worldPosition = WorldMap.Instance.GetMouseCoordinates(gridDivision, true);
        currentGridPosition = worldPosition;
        previewSprite.GlobalPosition = currentGridPosition + gridPreviewNodeOffset;

        Vector2I gridPosition = WorldMap.Instance.GetGridCoordinates(currentGridPosition);
        bool canPlace = WorldMap.Instance.CanPlaceNode(currentlyLoadedWorldGridNode, gridPosition);

        Color previewColorBase = canPlace ? colorValid : colorInvalid;
        float alphaMin = 0.4f;
        float alphaMax = 0.75f;
        if(((Vector2)currentGridPosition).DistanceTo(PlayerCharacter.Instance.Position) >= INTERACT_RADIUS)
        {
            canPlace = false;

            alphaMin = 0.1f;
            alphaMax = 0.25f;
        }

        Color previewColor = previewColorBase.WithAlpha(SinBetween(alphaMin, alphaMax));

        previewSprite.Modulate = previewColor;

        if (IsUseHoldablePressed() && canPlace)
        {
            Node2D placedNode = currentlyLoadedPackedScene.Instantiate<Node2D>();
            WorldMap.Instance.AddWorldNode(placedNode, true, currentGridPosition);

            ServiceLocator.InventoryService.RemoveItem(itemInHand, 1);
        }
    }

    private bool IsUseHoldablePressed() => Input.IsActionJustPressed("use_holdable");
}
