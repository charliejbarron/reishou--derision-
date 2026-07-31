using Godot;

[GlobalClass]
public partial class InputInterpreter : Resource
{
    public virtual void HandleNavigationInputs(PlayerInput.NavigationInput navigationInput, Player player, SharedVariables variables)
    {
    }

    internal bool CanJump(int jump, SharedVariables.JumpVariables jumpVars, bool floored)
    {
        return (jump >= (PlayerSettings.PlayerInputSetting.HoldJump ? 1 : 2) || jumpVars.JumpTiming >= 0) && !(!floored && jumpVars.AirTiming <= 0f);
    }

    internal void HandleJumpVars(SharedVariables.JumpVariables variables, bool floored, int Jump)
    {
        variables.AirTiming = floored ? 0.15f : variables.AirTiming - GameManager.Delta;
        variables.JumpTiming = Jump >= (PlayerSettings.PlayerInputSetting.HoldJump ? 1 : 2) ? 0.15f : variables.JumpTiming - GameManager.Delta;
    }

    internal float CalculateFriction(bool floored, Vector3 newVelocity, float targetSpeed, float input)
    {
        if (!floored)
            return 1f;

        float t = Mathf.Clamp(targetSpeed / newVelocity.Length(), 0, 1) * input;
        
        return Mathf.Max(GameSettings.Friction, t);
    }

    internal float CalculateSpeed(float oldVelocity, float maxSpeed, float newVelocity, float friction)
    {
        return Mathf.Min(Mathf.Max(oldVelocity, maxSpeed), newVelocity * friction);
    }

    internal Vector3 MovementVelocity(Vector3 newVelocity, float newSpeed, float yVelocity)
    {
        float Gravity()
        {
            return -9.8f * 2f * GameManager.Delta;
        }

        newVelocity = newVelocity.Normalized() * newSpeed;
        return new Vector3(newVelocity.X, yVelocity + Gravity(), newVelocity.Z);
    }

    internal (Vector3 inputVelocity, Vector3 oldVelocity, Vector3 newVelocity) CalculateInput(Vector2 input, Basis forward, Vector3 bodyVelocity, bool floored)
    {
        Vector3 movementDir = new Vector3(input.X, 0, -input.Y) * forward;
        Vector3 velocity = bodyVelocity * new Vector3(1, 0, 1);
        Vector3 inputVelocity = velocity + movementDir * (floored ? GameSettings.Control * (velocity.Length() * 0.2f + 1) : GameSettings.Control);

        return (movementDir, velocity, inputVelocity);
    }
}