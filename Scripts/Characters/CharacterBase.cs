using Godot;

[GlobalClass]
public partial class CharacterBase : Node3D
{

	public InventoryItem CurrentlyHolding => currentlyHolding;

	[Export]
	private AnimationTree animationTree;

	[Export]
	private AnimationPlayer player;

	[Export]
	private Node3D characterParent;

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

	public void SetMovementParameters(Vector3 direction, float runBlend)
	{
		if (direction.Length() > 0.1f)
		{
			characterParent.LookAt(characterParent.GlobalPosition - direction);
		}

        animationTree.Set("parameters/RunBlend/blend_amount", runBlend);
        animationTree.Set("parameters/TimeScale/scale", runBlend * 0.6f);
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
