using Godot;

[GlobalClass]
public partial class CharacterBase : Node2D
{

	public InventoryItemDefinition CurrentlyHolding => currentlyHolding;

	[Export]
	private AnimationPlayer characterAnimator;

	[Export]
	private AnimationPlayer holdableAnimator;

	[Export]
	private Sprite2D holdableSprite;

	private Vector2 lastPosition;
	private bool lookRight = true;

	private InventoryItemDefinition currentlyHolding;

    public enum CharacterAnimations
	{
		IDLE,
		WALK
	}

	public enum HoldableAnimations
	{
		SWING,
	}

	public override void _Process(double delta)
	{

		Vector2 movementDelta = GlobalPosition - lastPosition;

		float moveSpeed = movementDelta.Length();
		if (moveSpeed > 0.01f)
		{
            characterAnimator.Play("Walk");
            characterAnimator.SpeedScale = Mathf.Clamp(moveSpeed * 3, 0, 2.5f);
		}
		else
		{
            characterAnimator.Stop();
		}

		lastPosition = GlobalPosition;

		if(Mathf.Abs(movementDelta.X) >= 0.01f)
		{
			lookRight = movementDelta.X > 0;
		}

		Scale = new Vector2(lookRight ? 1 : -1, 1);
	}

	public void SetHoldable(InventoryItemDefinition item)
	{
		holdableSprite.Texture = item != null ? item.itemSprite : null;
		currentlyHolding = item;
    }

	public void UseHoldable()
	{
		if(currentlyHolding == null)
		{
			return;
		}

		holdableAnimator.Play(currentlyHolding.useAnimation.ToString());
    }
}
