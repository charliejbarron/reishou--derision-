using Godot;

[GlobalClass]
public partial class LevelNavigationManager : Node
{
    // Player
    Player _player;

    // Level
    InputInterpreter _currentInterpreter;
    SharedVariables _interpreterVariables;

    // MovementInterpreter _movementInterpreter;
    // CameraInterpreter _cameraInterpreter;

    public override void _Ready()
    {
        _currentInterpreter = new QuakeInterpreter();
        Startup();
    }

    void Startup()
    {
        _interpreterVariables = new();

        _player = new()
        {
            CharacterBody = ResourceLoader.Load<PackedScene>("res://Scenes/player.tscn").Instantiate() as CharacterBody3D,
            Camera = ResourceLoader.Load<PackedScene>("res://Scenes/camera.tscn").Instantiate() as Camera3D
        };

        AddChild(_player.CharacterBody);
        AddChild(_player.Camera);

        _player.CharacterStepUpCast = _player.CharacterBody?.GetNode<ShapeCast3D>("StepUpCast");

        if (_player.CharacterStepUpCast == null)
            return;

        _player.CharacterStepUpCast.GlobalPosition = new Vector3(0, GameSettings.StepUpHeight, 0);
        _player.CharacterStepUpCast.TargetPosition = new Vector3(0, -GameSettings.StepUpHeight + 0.05f, 0);
    }

    public void HandleNavigation(PlayerInput.NavigationInput navigationInputs)
    {
       _currentInterpreter.HandleNavigationInputs(navigationInputs, _player, _interpreterVariables);
    }
}

public class Player
{
    public CharacterBody3D CharacterBody;
    public ShapeCast3D CharacterStepUpCast;
    public Camera3D Camera;
}

public class SharedVariables
{
    public JumpVariables JumpVars = new();
    public SprintVariables SprintVars = new();
    public DashVariables DashVars = new();

    public class JumpVariables
    {
        internal float AirTiming;
        internal float JumpTiming;
        internal bool DoubleJump;
    }

    public class SprintVariables
    {
        internal float SprintTime;
        internal float PaddingTime;
    }

    public class DashVariables
    {
        internal float DashCooldown;
        internal bool StartCooldown;
        internal float DashTimer;
        internal Vector3 DashDir;
        internal bool ScheduleDash;
    }
}