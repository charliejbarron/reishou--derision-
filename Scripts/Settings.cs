using Godot;

static class PlayerSettings
{
    public static class PlayerInput
    {
        internal static bool HoldJump = false;
        internal static bool ToggleSprint = true;
        internal static float SprintPadding = 0.1f;
        internal static Vector2 MouseSensitivity = new(6f, 4f);
        internal static Vector2 ControllerSensitivity = new(7f, 4f);
        internal static float SensitivityReductionFp = 0.6f;
        internal static float HorizontalCameraTiltFp = 3f;
        internal static float HorizontalCameraTiltSpeedFp = 1.2f;
    }
    
    public static class Misc
    {
        internal static bool DrawDebug = true;
    }
}

static class GameSettings
{
    internal const float MaxWalkSpeed = 4f;
    internal const float MaxSprintSpeed = 6f;

    internal const float JumpHeight = 7f;

    internal const float DashForce = 32f;
    internal const float DashFloorForceMult = 1.25f;
    internal const float DashCooldown = 0.5f;
    internal const float DashPadding = 0.3f;
    internal const float DashTime = 0.2f;
    internal const float DashFalloff = 0.2f;
    internal const float DashWallLoss = 3f;

    internal const float Friction = 0.9f;
    internal const float Control = 0.2f;
    internal const float SprintDeadzone = 0.2f;

    internal const float StepUpHeight = 0.21f;
    internal const float CameraOffset = 1.57f;

    internal static readonly string[] BuiltinInterpreters =
    {
        "QuakeInterpreter"
    };
}