using Godot;

[GlobalClass]
public partial class CharacterBase : Node2D
{

	[Export]
	private AnimationPlayer animationPlayer;

	[Export]
	private Sprite2D holdableSprite;

	private Vector2 lastPosition;
	private bool lookRight = true;

	public enum CharacterAnimations
	{
		IDLE,
		WALK
	}

	public override void _Process(double delta)
	{

		Vector2 movementDelta = GlobalPosition - lastPosition;

		float moveSpeed = movementDelta.Length();
		if (moveSpeed > 0.01f)
		{
			animationPlayer.Play("Walk");
			animationPlayer.SpeedScale = Mathf.Clamp(moveSpeed * 3, 0, 2.5f);
		}
		else
		{
			animationPlayer.Stop();
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
    }
}
