using Godot;

public static class PlayerSettings
{
    public static class PlayerInputSetting
    {
        internal static bool HoldJump = false;
        internal static bool ToggleSprint = true;
        internal static float SprintPadding = 0.3f;
        internal static Vector2 MouseSensitivity = new (6f, 4f);
        internal static Vector2 ControllerSensitivity = new (7f, 4f);
        internal static float SensitivityReductionFp = 0.6f;
        internal static float HorizontalCameraTiltFp = 2f;
        internal static float HorizontalCameraTiltSpeedFp = 1.2f;
    }
}

public static class GameSettings
{
    internal const float MaxWalkSpeed = 4f;
    internal const float MaxSprintSpeed = 6f;
    internal const float Friction = 0.9f;
    internal const float JumpHeight = 6f;
    internal const float Control= 0.2f;
}