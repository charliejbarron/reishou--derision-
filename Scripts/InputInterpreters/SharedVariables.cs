using Godot;

static class SharedVariables
{
    public static JumpVariables JumpVars = new();
    public static SprintVariables SprintVars = new();
    public static DashVariables DashVars = new();
    public static CameraVariables CameraVars = new();

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

    public class CameraVariables
    {
        internal float StepOffset;
    }
}