using Godot;

[GlobalClass]
public partial class CharacterBase : Node3D
{

	public InventoryItem CurrentlyHolding => currentlyHolding;

	[Export]
	private Sprite3D holdableSprite;

	private Vector3 lastPosition;
	private bool lookRight = true;

	private InventoryItem currentlyHolding;

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

	}

	public void SetHoldable(InventoryItem item)
	{
		holdableSprite.Texture = item != null ? item.definition.itemSprite : null;
		currentlyHolding = item;
    }

	public void UseHoldable()
	{
		if(currentlyHolding == null)
		{
			return;
		}
    }
}
