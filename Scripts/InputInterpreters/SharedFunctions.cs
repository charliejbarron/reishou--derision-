using System.Collections.Generic;
using Godot;

internal static class Interpreter
{
    internal static T ConvertParameters<T>(InputInterpreterParameters parameter) where T : InputInterpreterParameters, new()
    {
        T param = parameter as T ?? new T();

        return param;
    }

    internal static Vector3 HeadPosition(float yPos)
    {
        SharedVariables.CameraVariables vars = SharedVariables.CameraVars;
        
        Vector3 head = new Vector3(0, GameSettings.CameraOffset  - vars.StepOffset, 0);

        float increase = Mathf.Max(1, vars.StepOffset);
        
        vars.StepOffset = float.Lerp(vars.StepOffset, 0, GameManager.Delta * GameSettings.CameraYSmoothing * increase);
        
        return head;
    }

    internal static bool HandleFloor(Player player)
    {
        player.CharacterFloorCast.ForceShapecastUpdate();

        if (!player.CharacterFloorCast.IsColliding())
            return false;

        float offset = (1 - player.CharacterFloorCast.GetClosestCollisionSafeFraction()) * (GameSettings.StepUpHeight + GameSettings.StepDownHeight) - GameSettings.StepDownHeight;

        // GD.Print(offset);

        if (player.CharacterBody.Velocity.Y > 0f && offset <= 0f)
            return false;
        
        player.CharacterBody.GlobalPosition += Vector3.Up * offset;
        
        if (Mathf.Abs(offset) > 3e-02f)
        {
            player.CharacterBody.ResetPhysicsInterpolation();
            
            SharedVariables.CameraVariables vars = SharedVariables.CameraVars;
            vars.StepOffset += offset;
        }

        return true;
    }

    internal static Vector3 Dash(bool dash, bool floored, Vector2 moveInput, bool isOnWall)
    {
        SharedVariables.DashVariables dashVars = SharedVariables.DashVars;

        if (floored != dashVars.TouchedFloor)
            dashVars.DashCooldown = 0;

        if (floored)
            dashVars.TouchedFloor = true;

        dashVars.DashTimer -= GameManager.Delta * (isOnWall ? GameSettings.DashWallLoss : 1);

        if (dashVars.DashTimer > GameSettings.DashFalloff)
        {
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

    internal static float GetSprint(Vector2 moveInput, float speed, bool sprintInput)
    {
        SharedVariables.SprintVariables sprintVars = SharedVariables.SprintVars;

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

    internal static bool CanJump(int jump, bool floored, bool dash = false, bool dbJump = true)
    {
        SharedVariables.JumpVariables jumpVars = SharedVariables.JumpVars;
        HandleJumpVars();

        bool nearFloor = jumpVars.AirTiming > 0f || floored;
        bool jumpInput = jump >= (PlayerSettings.PlayerInput.HoldJump ? 1 : 2);

        if (dash)
        {
            if (jumpInput)
                jumpVars.ScheduleJump = true;

            return false;
        }

        if (jumpVars.ScheduleJump)
        {
            jumpVars.ScheduleJump = false;
            jumpInput = true;
        }

        if (dbJump && !nearFloor && !jumpVars.DoubleJump && jumpInput)
            jumpVars.DoubleJump = nearFloor = true;

        return (jumpInput || jumpVars.JumpTiming >= 0) && nearFloor;

        void HandleJumpVars()
        {
            jumpVars.AirTiming -= GameManager.Delta;
            jumpVars.JumpTiming = jump >= (PlayerSettings.PlayerInput.HoldJump ? 1 : 2) ? 0.15f : jumpVars.JumpTiming - GameManager.Delta;

            if (!floored)
                return;

            jumpVars.AirTiming = GameSettings.JumpAirTiming;
            jumpVars.DoubleJump = false;
        }
    }

    internal static Vector3 Jump(Vector3 finalVelocity, Vector3 inputVelocity)
    {
        SharedVariables.JumpVars.JumpTiming = SharedVariables.JumpVars.AirTiming = -1f;

        Vector3 dir = finalVelocity.Lerp(inputVelocity, 0.75f).Normalized();
        Vector3 newVelocity = dir * (finalVelocity * new Vector3(1, 0, 1)).Length();

        newVelocity.Y = GameSettings.JumpHeight;

        return newVelocity;
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

    internal static float Gravity(bool floored, float yVel = 0f)
    {
        if (floored)
            return 0;

        return yVel + -9.8f * 2f * GameManager.Delta;
    }

    internal static (Vector3 inputVelocity, Vector3 oldVelocity, Vector3 newVelocity) CalculateInput(Vector2 input, Basis forward, Vector3 bodyVelocity, bool floored)
    {
        Vector3 movementDir = new Vector3(input.X, 0, -input.Y) * forward;
        Vector3 velocity = bodyVelocity * new Vector3(1, 0, 1);
        Vector3 inputVelocity = velocity + movementDir * (floored ? GameSettings.Control * (velocity.Length() * 0.2f + 1) : GameSettings.Control);

        return (movementDir, velocity, inputVelocity);
    }

    internal static Basis InputBasis(float x, float y, float z)
    {
        Basis degBasis = Basis.FromEuler(new Vector3(x, y, z) * (Mathf.Pi / 180));
        return degBasis;
    }

    internal static Basis InputBasis(Vector3 input)
    {
        Basis degBasis = Basis.FromEuler(input * (Mathf.Pi / 180));
        return degBasis;
    }

    internal static Vector2 CalcCameraInputs(PlayerInput.CameraInput camera, bool fp = false)
    {
        Vector2 input = camera.Controller * (fp ? PlayerSettings.PlayerInput.ControllerSensitivityReductionFp : 1);
        input += camera.Mouse * (fp ? PlayerSettings.PlayerInput.MouseSensitivityReductionFp : 1);

        return input;
    }
}

public static class SplitsFuncs
{
    internal static (Vector3 newPosition, Vector3 newVelocity) BlockPlayer(Split[] splits, Player player)
    {
        Vector3 position = player.CharacterBody.GlobalPosition;
        Vector3 velocity = player.CharacterBody.Velocity;

        int[] candidates = IsInfront(splits, player.CharacterBody.GlobalPosition);

        if (candidates.Length == 0)
            return (position, velocity);

        for (int i = 0; i < candidates.Length; i++)
        {
            Split split = splits[candidates[i]];

            bool matchHeight = split.Height < 0f || CheckHeight(split, position.Y);

            float width = GetWidth(split, new Vector2(position.X, position.Z));
            // Bug !!
            // player slides through corners D:
            bool matchWidth = split.Width < 0f || width <= 0f;

            bool match = matchWidth && matchHeight;

            if (!match)
                continue;

            Vector2 length = new Vector2(-split.Direction.Y, split.Direction.X);

            float offset = HorizontalPos(length, new Vector2(position.X, position.Z), new Vector2(split.Position.X, split.Position.Z));
            Vector3 clamped = split.Position + new Vector3(length.X, 0, length.Y) * offset;
            position = new Vector3(clamped.X, position.Y, clamped.Z);

            float velOffset = HorizontalPos(length, new Vector2(velocity.X, velocity.Z), Vector2.Zero);
            Vector3 velClamped = new Vector3(length.X, 0, length.Y) * velOffset;
            velocity = new Vector3(velClamped.X, velocity.Y, velClamped.Z);
        }

        return (position, velocity);
    }

    internal static int CheckNavigationSplits(Split[] splits, Vector3 playerPos)
    {
        int[] candidates = IsInfront(splits, playerPos);

        switch (candidates.Length)
        {
            case 0:
                return -1;
            case 1 when Ignore(splits[candidates[0]]):
                return candidates[0];
        }

        int pick = -1;
        for (int i = 0; i < candidates.Length; i++)
        {
            bool matchHeight = CheckHeight(splits[candidates[i]], playerPos.Y);

            float width = GetWidth(splits[candidates[i]], new Vector2(playerPos.X, playerPos.Z));
            bool matchWidth = width < 0f;

            bool match = matchWidth && matchHeight;

            if (match)
            {
                pick = splits[candidates[i]].Connected;
                break;
            }

            if (pick == -1 && (matchWidth || splits[candidates[i]].Width < 0f) && (matchHeight || splits[candidates[i]].Height < 0f))
                pick = splits[candidates[i]].Connected;
        }

        return pick;
    }

    internal static int[] IsInfront(Split[] splits, Vector3 playerPos)
    {
        List<int> candidates = new List<int>();
        for (int i = 0; i < splits.Length; i++)
        {
            Vector3 localPos = playerPos - splits[i].Position;
            float dot = splits[i].Direction.Dot(new Vector2(localPos.X, localPos.Z).Normalized());

            if (dot <= 0f)
                continue;

            candidates.Add(i);
        }

        return candidates.ToArray();
    }

    internal static bool Ignore(Split split)
    {
        return split.Width < 0f && split.Height < 0f;
    }

    internal static bool CheckHeight(Split split, float playerPosY)
    {
        return playerPosY > split.Position.Y - 1e-02 && playerPosY < split.Position.Y + Mathf.Abs(split.Height);
    }

    internal static float GetWidth(Split split, Vector2 playerPos)
    {
        Vector2 length = new Vector2(-split.Direction.Y, split.Direction.X);

        float offset = Mathf.Abs(HorizontalPos(length, playerPos, new Vector2(split.Position.X, split.Position.Z))) - Mathf.Abs(split.Width);

        return offset;
    }

    internal static float HorizontalPos(Vector2 length, Vector2 playerPos, Vector2 split)
    {
        return length.Dot(playerPos - new Vector2(split.X, split.Y));
    }

    internal static Chunk[] GetConnectedChunks(Split[] splits, int currentChunk, Chunk[] allChunks)
    {
        Chunk[] chunks = new Chunk[splits.Length];

        for (int i = 0; i < splits.Length; i++)
        {
            chunks[i] = allChunks[allChunks[currentChunk].VisibleChunks[splits[i].Connected]];
        }

        return chunks;
    }
}