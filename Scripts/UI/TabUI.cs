using Godot;

[GlobalClass]
public partial class TabUI : CanvasLayer
{
    public static TabUI Instance { get; private set; }

    [Export]
    private Control expandParent;

    [Export]
    private Control playerUI;

    [Export]
    private CraftingUI shopUI;

    private float foldedY;
    private float expandedY;
    private bool isExpanded = false;

    public override void _Ready()
    {
        base._Ready();

        foldedY = -310;
        expandedY = 100;

        Instance = this;
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("inventory_toggle"))
        {
            if (!isExpanded)
            {
                ShowPlayerUI();
            }
            else
            {
                HideUI();
            }
        }

        expandParent.Position = new Vector2(expandParent.Position.X, (float)Mathf.Lerp(expandParent.Position.Y, isExpanded ? expandedY : foldedY, 20 * delta));
    }

    private void ShowUI()
    {
        isExpanded = true;
        ServiceLocator.InputStateService.PushInputState(InputState.UI);
        ServiceLocator.GameNotificationService.OnFocusUIToggled.Execute(true);
    }

    private void HideUI()
    {
        isExpanded = false;
        ServiceLocator.InputStateService.PopInputState();
        ServiceLocator.GameNotificationService.OnFocusUIToggled.Execute(false);
    }

    public void ShowPlayerUI()
    {
        playerUI.Visible = true;
        shopUI.Visible = false;

        ShowUI();
    }

    public void ShowShopUI(CraftingRecipeContainer itemContainer)
    {
        playerUI.Visible = false;
        shopUI.Visible = true;

        shopUI.SetCraftingContainer(itemContainer);

        ShowUI();
    }
}