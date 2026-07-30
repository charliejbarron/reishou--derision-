using Godot;

public static class PlayerSettings
{
    public static class PlayerInputSetting
    {
        internal static bool HoldJump;
        internal static bool ToggleSprint;
        internal static float SprintPadding = 0.3f;
        internal static Vector2 MouseSensitivity = new (6f, 4f);
        internal static Vector2 ControllerSensitivity = new (7f, 4f);
        internal static float sensitivityReductionFP = 0.6f;
    }
}