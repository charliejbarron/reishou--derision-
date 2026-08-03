using Godot;

[GlobalClass]
public partial class InputInterpreter : Resource
{
    public virtual void HandleNavigationInputs(PlayerInput.NavigationInput navigationInput, Player player, SharedVariables variables)
    {
    }

    internal bool CanJump(int jump, SharedVariables.JumpVariables jumpVars, bool floored, bool dbJump)
    {
        bool nearFloor = jumpVars.AirTiming > 0f || floored;
        bool jumpInput = jump >= (PlayerSettings.PlayerInput.HoldJump ? 1 : 2);

        if (dbJump && !nearFloor && !jumpVars.DoubleJump && jumpInput)
            jumpVars.DoubleJump = nearFloor = true;

        return (jumpInput || jumpVars.JumpTiming >= 0) && nearFloor;
    }

    internal Vector3 HandleSteps(Player player, Vector3 input)
    {
        if (input == Vector3.Zero || !player.CharacterBody.IsOnFloor() || !player.CharacterBody.IsOnWall())
            return Vector3.Zero;

        player.CharacterStepUpCast.ForceShapecastUpdate();

        if (!player.CharacterStepUpCast.IsColliding())
            return Vector3.Zero;

        float step = player.CharacterStepUpCast.GetClosestCollisionSafeFraction();

        Vector3 point = player.CharacterStepUpCast.GetCollisionPoint(0);
        Vector3 localPoint = player.CharacterBody.ToLocal(point) * new Vector3(1, 0, 1);

        float alignment = localPoint.Normalized().Dot(input);

        if (step < 1 && alignment > 0)
        {
            return new Vector3(input.X * 0.01f, (1f - step) * GameSettings.StepUpHeight, input.Z * 0.01f);
        }

        return Vector3.Zero;
    }

    internal Vector3 Dash(SharedVariables.DashVariables dashVars, bool dash, bool floored, Vector2 moveInput)
    {
        GD.Print(dashVars.DashCooldown);

        if (dashVars.DashTimer > GameSettings.DashFalloff)
        {
            dashVars.DashTimer -= GameManager.Delta;

            if (floored)
                dashVars.StartCooldown = true;
            
            return dashVars.DashDir * GameSettings.DashForce * dashVars.DashTimer;
        }

        if (dashVars.DashCooldown < GameSettings.DashCooldown - 1e-08 || floored || dashVars.StartCooldown)
        {
            dashVars.StartCooldown = false;
            dashVars.DashCooldown -= GameManager.Delta;

            if (dashVars.DashCooldown < GameSettings.DashPadding && dash)
            {
                dashVars.ScheduleDash = true;
            }
            
            if (!floored)
            {
                dashVars.DashCooldown = 0;
            }
        }

        if (!(dash || dashVars.ScheduleDash) || dashVars.DashCooldown > 0f)
            return Vector3.Zero;

        dashVars.ScheduleDash = false;
        dashVars.DashCooldown = GameSettings.DashCooldown;
        dashVars.DashTimer = GameSettings.DashTime + GameSettings.DashFalloff;
        dashVars.DashDir = moveInput == Vector2.Zero ? new Vector3(0, 0, -1) : new Vector3(moveInput.X, 0, -moveInput.Y).Normalized();
        return dashVars.DashDir * GameSettings.DashForce * GameManager.Delta * dashVars.DashCooldown;
    }

    internal float GetSprint(SharedVariables.SprintVariables sprintVars, Vector2 moveInput, bool sprintInput)
    {
        if (sprintInput)
        {
            sprintVars.PaddingTime = PlayerSettings.PlayerInput.SprintPadding;
            sprintVars.SprintTime = Mathf.Min(sprintVars.SprintTime + GameManager.Delta, 3f);
            return sprintVars.SprintTime;
        }

        if (!PlayerSettings.PlayerInput.ToggleSprint)
        {
            sprintVars.SprintTime = 0f;
            return 0f;
        }

        if (moveInput.Length() >= GameSettings.SprintDeadzone && sprintVars.PaddingTime > 0)
        {
            sprintVars.PaddingTime = PlayerSettings.PlayerInput.SprintPadding;
        }

        sprintVars.SprintTime = sprintVars.PaddingTime <= 0f ? 0f : Mathf.Min(sprintVars.SprintTime + GameManager.Delta, 3f);
        sprintVars.PaddingTime -= GameManager.Delta;

        return sprintVars.SprintTime;
    }

    internal Vector3 Jump(SharedVariables.JumpVariables jumpVars, Vector3 finalVelocity, Vector3 inputVelocity)
    {
        jumpVars.JumpTiming = jumpVars.AirTiming = -1f;

        Vector3 dir = finalVelocity.Lerp(inputVelocity, 0.75f).Normalized();
        Vector3 newVelocity = dir * (finalVelocity * new Vector3(1, 0, 1)).Length();

        newVelocity.Y = GameSettings.JumpHeight;

        return newVelocity;
    }

    internal void HandleJumpVars(SharedVariables.JumpVariables variables, bool floored, int jump)
    {
        if (floored)
            variables.DoubleJump = false;

        variables.AirTiming = floored ? 0.2f : variables.AirTiming - GameManager.Delta;
        variables.JumpTiming = jump >= (PlayerSettings.PlayerInput.HoldJump ? 1 : 2) ? 0.15f : variables.JumpTiming - GameManager.Delta;
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
        newVelocity = newVelocity.Normalized() * newSpeed;
        return new Vector3(newVelocity.X, yVelocity, newVelocity.Z);
    }

    internal float Gravity()
    {
        return -9.8f * 2f * GameManager.Delta;
    }

    internal (Vector3 inputVelocity, Vector3 oldVelocity, Vector3 newVelocity) CalculateInput(Vector2 input, Basis forward, Vector3 bodyVelocity, bool floored)
    {
        Vector3 movementDir = new Vector3(input.X, 0, -input.Y) * forward;
        Vector3 velocity = bodyVelocity * new Vector3(1, 0, 1);
        Vector3 inputVelocity = velocity + movementDir * (floored ? GameSettings.Control * (velocity.Length() * 0.2f + 1) : GameSettings.Control);

        return (movementDir, velocity, inputVelocity);
    }
}