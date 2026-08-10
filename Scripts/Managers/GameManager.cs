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
    }

    public override void _PhysicsProcess(double delta)
    {
        Delta = (float)delta;
        PlayerInput playerInput = _inputManager.CollectInputs();

        _levelNavigationManager.HandleNavigation(playerInput);
    }

    public override void _Process(double delta)
    {
        _levelNavigationManager.HandleCamera();
    }
}