using System.Collections.Generic;
using Godot;

internal static class Interpreter
{
    internal static T ConvertParameters<T>(InputInterpreterParameters parameter) where T : InputInterpreterParameters, new()
    {
        T param = parameter as T ?? new T();

        return param;
    }
    
    internal static Vector3 HandleMovement(PlayerInput.MovementInput movementInput, Player player, Basis forward)
    {
        bool floored = Interpreter.HandleFloor(player);
        
        float moveSpeed = Interpreter.GetSprint(movementInput.Movement, player.CharacterBody.Velocity.Length(), movementInput.Sprint) > 0f && floored ? GameSettings.MaxSprintSpeed : GameSettings.MaxWalkSpeed;
        var velocities = Interpreter.CalculateInput(movementInput.Movement, forward, player.CharacterBody.Velocity, floored);

        Vector3 dash = Interpreter.ClampToSlope(Interpreter.Dash(movementInput.Dash, floored, movementInput.Movement, player.CharacterBody.IsOnWall()) * forward);
        bool canJump = Interpreter.CanJump(movementInput.Jump, floored, dash != Vector3.Zero);

        if (dash != Vector3.Zero)
            return dash + new Vector3(0, Interpreter.Gravity(floored), 0);

        float friction = Interpreter.CalculateFriction(floored, velocities.newVelocity, moveSpeed, movementInput.Movement.Length(), player.CharacterBody.IsOnWall());
        float speed = Interpreter.CalculateSpeed(velocities.oldVelocity.Length(), moveSpeed, velocities.newVelocity.Length(), friction);

        Vector3 finalVelocity = Interpreter.MovementVelocity(velocities.newVelocity, speed, Interpreter.Gravity(floored, player.CharacterBody.Velocity.Y));

        finalVelocity = Interpreter.HandleSlopes(finalVelocity);
            
        if (canJump)
            finalVelocity = Interpreter.Jump(finalVelocity, velocities.inputVelocity);

        return finalVelocity;
    }

    internal static Vector3 HeadPosition()
    {
        SharedVariables.PhysicVariables vars = SharedVariables.PhysicsVars;

        Vector3 head = new Vector3(0, GameSettings.CameraOffset - vars.StepOffset, 0);

        float increase = Mathf.Max(1, vars.StepOffset);

        vars.StepOffset = float.Lerp(vars.StepOffset, 0, GameManager.Delta * GameSettings.CameraYSmoothing * increase);

        return head;
    }

    internal static bool HandleFloor(Player player, bool forceSlope = false)
    {
        SharedVariables.PhysicVariables vars = SharedVariables.PhysicsVars;
        player.FloorCast.ForceShapecastUpdate();

        if (!player.FloorCast.IsColliding() || player.CharacterBody.Velocity.Y > 0f)
        {
            vars.OnSlope = false;
            return false;
        }

        vars.LimitNormal = vars.SlopeNormal = Vector3.Zero;

        float offset = (1 - player.FloorCast.GetClosestCollisionSafeFraction()) * (GameSettings.StepUpHeight + GameSettings.StepDownHeight);
        offset -= GameSettings.StepDownHeight + 0.005f;


        player.CharacterBody.GlobalPosition += Vector3.Up * offset;

        Vector3 normal = player.FloorCast.GetCollisionNormal(0);
        float angle = 90f - float.RadiansToDegrees(Mathf.Asin(normal.Y));

        if (Mathf.Abs(offset) > 5e-02f && angle < GameSettings.MaxSlopeAngle && !forceSlope)
        {
            player.CharacterBody.ResetPhysicsInterpolation();
            vars.StepOffset += offset;
        }

        if (angle < GameSettings.MaxSlopeAngle && !forceSlope)
            return true;

        SharedVariables.PhysicsVars.OnSlope = false;
        bool collision = player.SlopeCast.IsColliding();

        // slope normal should only be from raycast, limit normal should be shapecast
        if (collision)
        {
            Vector3 rayNormal = player.SlopeCast.GetCollisionNormal();

            if (90f - float.RadiansToDegrees(Mathf.Asin(rayNormal.Y)) > GameSettings.MaxSlopeAngle)
            {
                SharedVariables.PhysicsVars.OnSlope = true;
                SharedVariables.PhysicsVars.SlopeNormal = rayNormal.Cross(new Vector3(-rayNormal.Z, 0, rayNormal.X).Normalized());
            }
        }
        else
        {
            SharedVariables.PhysicsVars.OnSlope = false;
        }

        SharedVariables.PhysicsVars.LimitNormal = new Vector3(normal.X, 0, normal.Z).Normalized();
        return true;
    }

    internal static Vector3 Dash(bool dash, bool floored, Vector2 moveInput, bool isOnWall)
    {
        SharedVariables.DashVariables dashVars = SharedVariables.DashVars;
        bool onSlope = SharedVariables.PhysicsVars.OnSlope;
        
        if (floored != dashVars.TouchedFloor)
            dashVars.DashCooldown = 0;

        if (floored && !onSlope)
            dashVars.TouchedFloor = true;

        dashVars.DashTimer -= GameManager.Delta * (isOnWall ? GameSettings.DashWallLoss : 1);

        if (dashVars.DashTimer > GameSettings.DashFalloff)
        {
            SharedVariables.PhysicsVars.LimitNormal = SharedVariables.PhysicsVars.SlopeNormal = Vector3.Zero;
            return onSlope ? Vector3.Zero : dashVars.DashDir * GameSettings.DashForce * dashVars.DashTimer;
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
        return onSlope ? Vector3.Zero : dashVars.DashDir * GameSettings.DashForce * GameManager.Delta * dashVars.DashCooldown;
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

        Vector3 inputDir = SharedVariables.PhysicsVars.SlopeNormal != Vector3.Zero ? SharedVariables.PhysicsVars.SlopeNormal : inputVelocity;

        Vector3 dir = finalVelocity.Lerp(inputDir, 1f / Mathf.Max(finalVelocity.Length(), 1.45f)).Normalized();
        Vector3 newVelocity = dir * (finalVelocity * new Vector3(1, 0, 1)).Length();

        newVelocity.Y = GameSettings.JumpHeight;

        return newVelocity;
    }

    internal static float CalculateFriction(bool floored, Vector3 newVelocity, float targetSpeed, float input, bool isOnWall)
    {
        isOnWall = isOnWall || !SharedVariables.PhysicsVars.OnSlope && SharedVariables.PhysicsVars.LimitNormal != Vector3.Zero; 
        
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
        Vector3 velocity = bodyVelocity * new Vector3(1, 1, 1);
        
        movementDir = ClampToSlope(movementDir, 1f);
        
        Vector3 inputVelocity = velocity + movementDir * (floored ? GameSettings.Control * (velocity.Length() * 0.2f + 1) : GameSettings.Control);

        return (movementDir, velocity, inputVelocity);
    }

    internal static Vector3 HandleSlopes(Vector3 velocity)
    {
        SharedVariables.PhysicVariables vars = SharedVariables.PhysicsVars;

        velocity = ClampToSlope(velocity);
        
        if (vars.SlopeNormal == Vector3.Zero)
            return velocity;

        if (vars.OnSlope)
            velocity += (vars.SlopeNormal + Vector3.Down * velocity.Length()) * (Mathf.Abs(vars.SlopeNormal.Y) * Mathf.Min(velocity.Length(), 2f));

        return velocity;
    }

    internal static Vector3 ClampToSlope(Vector3 clamp, float? movement = null)
    {
        float velocity = movement ?? clamp.Length();
        
        SharedVariables.PhysicVariables vars = SharedVariables.PhysicsVars;

        if (vars.LimitNormal == Vector3.Zero) 
            return clamp;
        
        float dot = clamp.Dot(vars.LimitNormal.Normalized());

        if (dot >= 0f) 
            return clamp;
        
        clamp -= dot * vars.LimitNormal.Normalized() * Mathf.Min(1f, velocity);

        return clamp;
    }
    
    static Vector3 ClampToSlope(Vector2 clamp, float? movement = null)
    {
        float velocity = movement ?? clamp.Length();
        
        Vector3 newClamp = new Vector3(clamp.X, 0, clamp.Y);
        SharedVariables.PhysicVariables vars = SharedVariables.PhysicsVars;

        if (vars.SlopeNormal == Vector3.Zero) 
            return newClamp;
        
        float dot = newClamp.Dot(new Vector3(vars.SlopeNormal.X, 0, vars.SlopeNormal.Z).Normalized());

        if (dot >= 0f) 
            return newClamp;
        
        newClamp -= dot * new Vector3(vars.SlopeNormal.X, 0, vars.SlopeNormal.Z).Normalized() * Mathf.Min(1f, velocity);

        return newClamp;
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

        bool hasMatched = false;
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

            hasMatched = true;

            Vector2 length = new Vector2(-split.Direction.Y, split.Direction.X);

            float offset = HorizontalPos(length, new Vector2(position.X, position.Z), new Vector2(split.Position.X, split.Position.Z));
            Vector3 clamped = split.Position + new Vector3(length.X, 0, length.Y) * offset;
            position = new Vector3(clamped.X, position.Y, clamped.Z);

            float velOffset = HorizontalPos(length, new Vector2(velocity.X, velocity.Z), Vector2.Zero);
            Vector3 velClamped = new Vector3(length.X, 0, length.Y) * velOffset;
            velocity = new Vector3(velClamped.X, velocity.Y, velClamped.Z);
        }

        if (hasMatched)
            SharedVariables.DashVars.DashTimer -= GameManager.Delta * GameSettings.DashWallLoss;

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