using Godot;

[GlobalClass]
public partial class LevelNavigationManager : Node
{
    // Player
    Player _player;

    // Level
    InputInterpreter _currentInterpreter;

    // MovementInterpreter _movementInterpreter;
    // CameraInterpreter _cameraInterpreter;

    public override void _Ready()
    {
        _currentInterpreter = new QuakeInterpreter();
        Startup();
    }

    void Startup()
    {
        _player = new()
        {
            CharacterBody = ResourceLoader.Load<PackedScene>("res://Scenes/player.tscn").Instantiate() as CharacterBody3D,
            Camera = ResourceLoader.Load<PackedScene>("res://Scenes/camera.tscn").Instantiate() as Camera3D
        };

        AddChild(_player.CharacterBody);
        AddChild(_player.Camera);

        _player.CharacterStepCast = _player.CharacterBody?.GetNode<RayCast3D>("StepCast");

        if (_player.CharacterStepCast == null)
            return;

        _player.CharacterStepCast.Position = new Vector3(0, GameSettings.CameraOffset, 0);
        _player.CharacterStepCast.TargetPosition = new Vector3(0, -GameSettings.CameraOffset + 1e-08f, 0);
    }

    public void HandleNavigation(PlayerInput.NavigationInput navigationInputs)
    {
        _currentInterpreter.HandleNavigationInputs(navigationInputs, _player);
    }
}

public class Player
{
    public CharacterBody3D CharacterBody;
    public RayCast3D CharacterStepCast;
    public Camera3D Camera;
}