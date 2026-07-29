using Godot;
using System;

[GlobalClass]
public partial class CharacterBase : Node2D
{

	public InventoryItem CurrentlyHolding => currentlyHolding;
	public bool IsUsingHoldable => isUsingHoldable;

	[Export]
	private AnimationPlayer characterAnimator;

	[Export]
	private AnimationPlayer holdableAnimator;

	[Export]
	private Sprite2D holdableSprite;

	private Vector2 lastPosition;
	private bool lookRight = true;
	private bool isUsingHoldable = false;
	private Action useCallback;

	private InventoryItem currentlyHolding;

    public enum CharacterAnimations
	{
		IDLE,
		WALK
	}

	public enum HoldableAnimations
	{
		SWING,
		SHOVEL,
		SHAKE_OUT
	}

	public override void _PhysicsProcess(double delta)
	{

		Vector2 movementDelta = GlobalPosition - lastPosition;

        float moveSpeed = movementDelta.Length();
		if (moveSpeed > 0.01f)
		{
            characterAnimator.Play("Walk");
            characterAnimator.SpeedScale = Mathf.Clamp(moveSpeed * 15, 0, 2.5f);
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


    public void SetHoldable(InventoryItem item)
	{
		holdableSprite.Texture = item != null ? item.definition.itemSprite : null;
		currentlyHolding = item;
    }

	public void UseHoldable(Action useCallback)
	{
		if(currentlyHolding == null)
		{
			return;
		}

		this.useCallback = useCallback;

		holdableAnimator.Stop();
        holdableAnimator.Play(currentlyHolding.definition.useAnimation.ToString(), 0.2f);
    }

	public void SetIsUsingHoldable(bool isUsing) => isUsingHoldable = isUsing;
	public void ExecuteHoldableAction() => useCallback?.Invoke();
}
