
using Godot;

public static class VelocityUtility
{

    public static Vector2 GetUpdatedVelocity(Vector2 currentVelocity, float acceleration, float decceleration, float maxSpeed, Vector2 input)
    {
        currentVelocity += input * acceleration;

        if (input.X < 0.05f && input.X > -0.05f) {
            currentVelocity.X *= decceleration;
        }

        if (input.Y < 0.05f && input.Y > -0.05f) {
            currentVelocity.Y *= decceleration;
        }

        currentVelocity = currentVelocity.LimitLength(maxSpeed);

        return currentVelocity;
    }
}