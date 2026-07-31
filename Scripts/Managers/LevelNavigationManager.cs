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
        _currentInterpreter = new FirstPersonInterpreter();
        _interpreterVariables = new();
        
        _player = new()
        {
            CharacterBody = ResourceLoader.Load<PackedScene>("res://Scenes/player.tscn").Instantiate() as CharacterBody3D,
            Camera = ResourceLoader.Load<PackedScene>("res://Scenes/camera.tscn").Instantiate() as Camera3D
        };

        AddChild(_player.CharacterBody);
        AddChild(_player.Camera);
    }

    public void HandleNavigation(PlayerInput.NavigationInput navigationInputs)
    {
        _currentInterpreter.HandleNavigationInputs(navigationInputs, _player, _interpreterVariables);
    }
}

public class Player
{
    public CharacterBody3D CharacterBody;
    public Camera3D Camera;
}

public class SharedVariables
{
    public JumpVariables JumpVars = new ();
    public class JumpVariables
    {
        internal float AirTiming;
        internal float JumpTiming;       
    }
}