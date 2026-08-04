using Godot;

internal static class SharedFunctions
{
    internal static bool CanJump(int jump, SharedVariables.JumpVariables jumpVars, bool floored, bool dbJump)
    {
        bool nearFloor = jumpVars.AirTiming > 0f || floored;
        bool jumpInput = jump >= (PlayerSettings.PlayerInput.HoldJump ? 1 : 2);

        if (dbJump && !nearFloor && !jumpVars.DoubleJump && jumpInput)
            jumpVars.DoubleJump = nearFloor = true;

        return (jumpInput || jumpVars.JumpTiming >= 0) && nearFloor;
    }

    internal static Vector3 HandleSteps(Player player, Vector3 input)
    {
        if (input == Vector3.Zero || !player.CharacterBody.IsOnFloor() || !player.CharacterBody.IsOnWall() || player.CharacterBody.Velocity.Y > 0f)
            return Vector3.Zero;

        player.CharacterStepCast.Position = new Vector3(input.X * 0.21f, GameSettings.StepUpHeight, input.Z * 0.21f);
        player.CharacterStepCast.ForceRaycastUpdate();

        if (!player.CharacterStepCast.IsColliding())
            return Vector3.Zero;

        Vector3 point = player.CharacterBody.ToLocal(player.CharacterStepCast.GetCollisionPoint()) * new Vector3(0.1f, 1f, 0.1f);

        if (point.Y < 1e-08f)
            return Vector3.Zero;

        return point;
    }

    internal static Vector3 Dash(SharedVariables.DashVariables dashVars, bool dash, bool floored, Vector2 moveInput, bool isOnWall)
    {
        if (floored != dashVars.TouchedFloor)
            dashVars.DashCooldown = 0;

        if (floored)
            dashVars.TouchedFloor = true;

        if (dashVars.DashTimer > GameSettings.DashFalloff)
        {
            dashVars.DashTimer -= GameManager.Delta * (isOnWall ? GameSettings.DashWallLoss : 1);
            return dashVars.DashDir * GameSettings.DashForce * dashVars.DashTimer;
        }

        dashVars.DashCooldown -= GameManager.Delta;
        dashVars.ScheduleDash -= GameManager.Delta;

        if (dash)
            dashVars.ScheduleDash = GameSettings.DashPadding;

        if (dashVars.ScheduleDash < 0f || dashVars.DashCooldown > 0f || !dashVars.TouchedFloor)
            return Vector3.Zero;

        dashVars.ScheduleDash = 0f;
        dashVars.TouchedFloor = floored;
        dashVars.DashCooldown = GameSettings.DashCooldown;
        dashVars.DashTimer = GameSettings.DashTime + GameSettings.DashFalloff;
        dashVars.DashDir = moveInput == Vector2.Zero ? new Vector3(0, 0, -1) : new Vector3(moveInput.X, 0, -moveInput.Y).Normalized();
        return dashVars.DashDir * GameSettings.DashForce * GameManager.Delta * dashVars.DashCooldown;
    }

    internal static float GetSprint(SharedVariables.SprintVariables sprintVars, Vector2 moveInput, bool sprintInput)
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

    internal static Vector3 Jump(SharedVariables.JumpVariables jumpVars, Vector3 finalVelocity, Vector3 inputVelocity)
    {
        jumpVars.JumpTiming = jumpVars.AirTiming = -1f;

        Vector3 dir = finalVelocity.Lerp(inputVelocity, 0.75f).Normalized();
        Vector3 newVelocity = dir * (finalVelocity * new Vector3(1, 0, 1)).Length();

        newVelocity.Y = GameSettings.JumpHeight;

        return newVelocity;
    }

    internal static void HandleJumpVars(SharedVariables.JumpVariables variables, bool floored, int jump)
    {
        if (floored)
            variables.DoubleJump = false;

        variables.AirTiming = floored ? 0.2f : variables.AirTiming - GameManager.Delta;
        variables.JumpTiming = jump >= (PlayerSettings.PlayerInput.HoldJump ? 1 : 2) ? 0.15f : variables.JumpTiming - GameManager.Delta;
    }

    internal static float CalculateFriction(bool floored, Vector3 newVelocity, float targetSpeed, float input, bool isOnWall)
    {
        if (!floored)
            return 1f;

        float t = Mathf.Clamp(targetSpeed / newVelocity.Length(), 0, 1) * input;

        return Mathf.Max(GameSettings.Friction, t * (isOnWall ? 0 : 1));
    }

    internal static float CalculateSpeed(float oldVelocity, float maxSpeed, float newVelocity, float friction)
    {
        return Mathf.Min(Mathf.Max(oldVelocity, maxSpeed), newVelocity * friction);
    }

    internal static Vector3 MovementVelocity(Vector3 newVelocity, float newSpeed, float yVelocity)
    {
        newVelocity = newVelocity.Normalized() * newSpeed;
        return new Vector3(newVelocity.X, yVelocity, newVelocity.Z);
    }

    internal static float Gravity()
    {
        return -9.8f * 2f * GameManager.Delta;
    }

    internal static (Vector3 inputVelocity, Vector3 oldVelocity, Vector3 newVelocity) CalculateInput(Vector2 input, Basis forward, Vector3 bodyVelocity, bool floored)
    {
        Vector3 movementDir = new Vector3(input.X, 0, -input.Y) * forward;
        Vector3 velocity = bodyVelocity * new Vector3(1, 0, 1);
        Vector3 inputVelocity = velocity + movementDir * (floored ? GameSettings.Control * (velocity.Length() * 0.2f + 1) : GameSettings.Control);

        return (movementDir, velocity, inputVelocity);
    }
}