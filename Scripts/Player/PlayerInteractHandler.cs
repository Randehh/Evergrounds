using Godot;

[GlobalClass]
public partial class PlayerInteractHandler : Node3D
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
    private Sprite3D selectArrow;

    [Export]
    private Label selectArrowLabel;

    // Default interactions
    private Interactable currentInteracable;
    private InteractResult currentInteractResult;
    private InteractType previousInteractType;
    private double alphaTimer = 0;

    public override void _Ready()
    {
        base._Ready();

        //(selectArrow.FindChild("ArrowAnimator") as AnimationPlayer).Play("Highlight");
    }

    public void ProcessInteraction(InventoryItem itemInHand, double delta)
    {
        InteractType interactType = itemInHand?.definition.interactType ?? InteractType.NONE;

        // Check if we need to reset indicators
        bool activateIndicator = false;
        if(interactType != previousInteractType)
        {
            selectArrow.Visible = false;

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

        string arrowText = currentInteracable.GetArrowText();
        bool hasArrowText = string.IsNullOrWhiteSpace(arrowText);

        selectArrow.GlobalPosition = currentInteracable.GetArrowAnchor() + (hasArrowText ? new Vector3(0, 0, 0) : new Vector3()) + new Vector3(0, 1.25f, 0);
        selectArrow.Visible = true;
        selectArrowLabel.Text = arrowText;

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


    private float SinBetween(float min, float max)
    {
        return ((max - min) * (float)Mathf.Sin(alphaTimer * 5) + max + min) / 2;
    }

    private bool IsUseHoldablePressed() => Input.IsActionJustPressed("use_holdable");
}
