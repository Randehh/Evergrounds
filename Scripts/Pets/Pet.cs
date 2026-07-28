using Godot;

[GlobalClass]
public partial class Pet : Node2D
{
    [Export]
    private float maxSpeed = 1;

    [Export]
    private float acceleration = 10;

    [Export]
    private float decceleration = 0.01f;

    [Export]
    private float wanderRange = 4;

    [Export]
    private float wanderWaitPeriod = 4;

    [Export]
    private float wanderWalkPeriod = 3;

    [Export]
    private Node2D initialFollowTarget;

    private Vector2 currentSpeed = Vector2.Zero;
    private Node2D followTarget;
    private Vector2 offsetPoint = Vector2.Zero;

    private PetState state = PetState.IDLE;
    private double currentStateTime = 0;
    private float endStateTime = 0;

    public override void _Ready()
    {
        base._Ready();

        SetFollowTarget(initialFollowTarget);
    }

    public override void _PhysicsProcess(double delta)
    {
        // Moving
        Vector2 input = Vector2.Zero;

        Vector2 currentPosition = GlobalPosition;
        Vector2 targetPosition = offsetPoint;
        float distanceToOffsetPoint = currentPosition.DistanceTo(targetPosition);
        float distanceToTarget = currentPosition.DistanceTo(followTarget.GlobalPosition);

        currentStateTime += delta;

        if (distanceToTarget > wanderRange) {
            SetState(PetState.FOLLOWING);
        }

        switch (state) {
            case PetState.IDLE:
                if (currentStateTime > endStateTime) {
                    SetState(PetState.WANDERING);
                }
                break;
            case PetState.WANDERING:
                if (currentStateTime > endStateTime) {
                    SetState(PetState.IDLE);
                } else if (distanceToOffsetPoint > maxSpeed) {
                    input = targetPosition - currentPosition;
                }
                break;
            case PetState.FOLLOWING:
                if (distanceToOffsetPoint <= wanderRange) {
                    SetState(PetState.WANDERING);
                } else if (distanceToOffsetPoint > maxSpeed) {
                    input = targetPosition - currentPosition;
                }
                break;
        }

        currentSpeed = VelocityUtility.GetUpdatedVelocity(currentSpeed, acceleration, decceleration, maxSpeed, input);

        GlobalPosition += currentSpeed;
    }

    public void SetFollowTarget(Node2D target)
    {
        followTarget = target;
        PickNewOffsetPoint();
    }

    private void SetState(PetState nextState)
    {
        if(nextState == state) {
            return;
        }

        currentStateTime = 0;
        state = nextState;

        if (nextState == PetState.WANDERING || nextState == PetState.FOLLOWING) {
            PickNewOffsetPoint();
        }

        switch (nextState) {
            case PetState.IDLE:
                endStateTime = wanderWaitPeriod;
                break;

            case PetState.WANDERING:
                endStateTime = wanderWalkPeriod;
                break;
        }
    }

    private void PickNewOffsetPoint()
    {
        if(followTarget == null) {
            return;
        }

        offsetPoint = followTarget.GlobalPosition + new Vector2(
            ((GD.Randf() * 2) - 1) * wanderRange,
            ((GD.Randf() * 2) - 1) * wanderRange
        );
    }

    private enum PetState
    {
        IDLE,
        WANDERING,
        FOLLOWING
    }
}