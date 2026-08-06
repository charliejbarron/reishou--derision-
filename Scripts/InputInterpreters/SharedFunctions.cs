using System.Collections.Generic;
using Godot;

internal static class NavigationFunctions
{
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
            return dashVars.DashDir * GameSettings.DashForce * dashVars.DashTimer * (floored ? GameSettings.DashFloorForceMult : 1f);
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

    internal static float GetSprint(Vector2 moveInput, bool sprintInput)
    {
        if (sprintInput)
        {
            SharedVariables.SprintVars.PaddingTime = PlayerSettings.PlayerInput.SprintPadding;
            SharedVariables.SprintVars.SprintTime = Mathf.Min(SharedVariables.SprintVars.SprintTime + GameManager.Delta, 3f);
            return SharedVariables.SprintVars.SprintTime;
        }

        if (!PlayerSettings.PlayerInput.ToggleSprint)
        {
            SharedVariables.SprintVars.SprintTime = 0f;
            return 0f;
        }

        if (moveInput.Length() >= GameSettings.SprintDeadzone && SharedVariables.SprintVars.PaddingTime > 0)
        {
            SharedVariables.SprintVars.PaddingTime = PlayerSettings.PlayerInput.SprintPadding;
        }

        SharedVariables.SprintVars.SprintTime = SharedVariables.SprintVars.PaddingTime <= 0f ? 0f : Mathf.Min(SharedVariables.SprintVars.SprintTime + GameManager.Delta, 3f);
        SharedVariables.SprintVars.PaddingTime -= GameManager.Delta;

        return SharedVariables.SprintVars.SprintTime;
    }

    internal static bool CanJump(int jump, bool floored, bool dash = false, bool dbJump = true)
    {
        bool nearFloor = SharedVariables.JumpVars.AirTiming > 0f || floored;
        bool jumpInput = jump >= (PlayerSettings.PlayerInput.HoldJump ? 1 : 2);

        if (dash)
        {
            if (jumpInput)
                SharedVariables.JumpVars.ScheduleJump = true;
            
            return false;
        }

        if (SharedVariables.JumpVars.ScheduleJump)
        {
            SharedVariables.JumpVars.ScheduleJump = false;
            jumpInput = true;
        }
        
        if (dbJump && !nearFloor && !SharedVariables.JumpVars.DoubleJump && jumpInput)
            SharedVariables.JumpVars.DoubleJump = nearFloor = true;

        return (jumpInput || SharedVariables.JumpVars.JumpTiming >= 0) && nearFloor;
    }

    internal static Vector3 Jump(Vector3 finalVelocity, Vector3 inputVelocity)
    {
        SharedVariables.JumpVars.JumpTiming = SharedVariables.JumpVars.AirTiming = -1f;

        Vector3 dir = finalVelocity.Lerp(inputVelocity, 0.75f).Normalized();
        Vector3 newVelocity = dir * (finalVelocity * new Vector3(1, 0, 1)).Length();

        newVelocity.Y = GameSettings.JumpHeight;

        return newVelocity;
    }

    internal static void HandleJumpVars(bool floored, int jump)
    {
        if (floored)
            SharedVariables.JumpVars.DoubleJump = false;

        SharedVariables.JumpVars.AirTiming = floored ? 0.2f : SharedVariables.JumpVars.AirTiming - GameManager.Delta;
        SharedVariables.JumpVars.JumpTiming = jump >= (PlayerSettings.PlayerInput.HoldJump ? 1 : 2) ? 0.15f : SharedVariables.JumpVars.JumpTiming - GameManager.Delta;
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

public static class ChunkFunctions
{
    internal static int CheckSplits(Split[] splits, Vector3 playerPos)
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
            float dot = splits[i].Direction.Normalized().Dot(new Vector2(localPos.X, localPos.Z).Normalized());
            
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
        
        float offset = Mathf.Abs(length.Dot(playerPos - new Vector2(split.Position.X, split.Position.Z))) - Mathf.Abs(split.Width);

        return offset;
    }
    
    internal static void DrawDebug(Split split, Chunk connection)
    {
        DebugDraw3D.DrawArrowRay(split.Position, new Vector3(split.Direction.X, 0, split.Direction.Y), 1f, Colors.MediumSpringGreen, 0.3f);

        Vector3 widthOffset = new Vector3(-split.Direction.Y, 0, split.Direction.X) * Mathf.Abs(split.Width);
        Vector3 heightOffset = Vector3.Up * Mathf.Abs(split.Height);

        Color heightDCol = split.Height < 0f ? Colors.GreenYellow : Colors.MediumSpringGreen;
        Color heightUCol = split.Height < 0f ? Colors.MediumVioletRed : Colors.PaleVioletRed;
        Color widthLCol = split.Width < 0f ? Colors.BlueViolet : Colors.MediumPurple;
        Color widthRCol = split.Width < 0f ? Colors.Blue : Colors.MediumTurquoise;
        
        DebugDraw3D.DrawLine(split.Position + widthOffset, split.Position - widthOffset, heightDCol);
        DebugDraw3D.DrawLine(split.Position + widthOffset + heightOffset, split.Position - widthOffset + heightOffset, heightUCol);
        DebugDraw3D.DrawLine(split.Position - widthOffset, split.Position - widthOffset + heightOffset, widthLCol);
        DebugDraw3D.DrawLine(split.Position + widthOffset, split.Position + widthOffset + heightOffset, widthRCol);
        
        DebugDraw3D.DrawText(split.Position + heightOffset * 1.5f, connection.Name, 48);
    }
}

static class SharedVariables
{
    public static JumpVariables JumpVars = new();
    public static SprintVariables SprintVars = new();
    public static DashVariables DashVars = new();

    public class JumpVariables
    {
        internal float AirTiming;
        internal float JumpTiming;
        internal bool DoubleJump;
        internal bool ScheduleJump;
    }

    public class SprintVariables
    {
        internal float SprintTime;
        internal float PaddingTime;
    }

    public class DashVariables
    {
        internal float DashCooldown;
        internal float DashTimer;
        internal Vector3 DashDir;
        internal float ScheduleDash;
        internal bool TouchedFloor;
    }
}