using Godot;

[GlobalClass]
public partial class GameManager : Node
{
    public static float Delta;
    
    InputManager _inputManager;
    LevelNavigationManager _levelNavigationManager;
    
    public override void _Ready()
    {
        _inputManager = GetNode<InputManager>("InputManager");
        _levelNavigationManager = GetNode<LevelNavigationManager>("LevelNavigationManager");
        
        _levelNavigationManager.Setup(FileManager.LoadSceneChunks());
        
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _PhysicsProcess(double delta)
    {
        Delta = (float)delta;
        PlayerInput.NavigationInput input = _inputManager.CollectMovementInputs();

        _levelNavigationManager.HandleNavigation(input);
    }
}