using Godot;

[GlobalClass]
public partial class GameManager : Node
{
    public static float Delta;
    
    InputManager _inputManager;
    LevelNavigationManager _levelNavigationManager;

    PlayerInput _input;
    
    public override void _Ready()
    {
        _inputManager = GetNode<InputManager>("InputManager");
        _levelNavigationManager = GetNode<LevelNavigationManager>("LevelNavigationManager");
        
        _levelNavigationManager.LevelSetup(FileManager.LoadSceneChunks());
        
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _PhysicsProcess(double delta)
    {
        Delta = (float)delta;
        _input.NavigationInputs.MovementInputs = _inputManager.CollectMovementInputs();

        _levelNavigationManager.HandleNavigation(_input.NavigationInputs);
    }

    public override void _Process(double delta)
    {
        _input.NavigationInputs.CameraInputs = _inputManager.CollectCameraInputs((float)delta);
        
        _levelNavigationManager.HandleCamera(_input.NavigationInputs);
    }
}