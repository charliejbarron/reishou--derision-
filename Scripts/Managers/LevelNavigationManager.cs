using Godot;

[GlobalClass]
public partial class LevelNavigationManager : Node
{
    // Player
    Player _player;

    // - - Level - -
    // Chunks
    int _currentChunk;
    Chunk[] _chunks;

    //Interpreters
    InputInterpreter[] _loadedInterpreters;
    InputInterpreter.Parameters[][] _loadedParameters;


    public void Setup((Chunk[] chunks, int current, InputInterpreter[] interpreters) scene)
    {
        Vector3 SetupScene()
        {
            _chunks = scene.chunks;
            _currentChunk = scene.current;

            _loadedInterpreters = scene.interpreters;
            
            return _chunks[_currentChunk].SpawnPoint;
        }

        void SetupPlayer(Vector3 spawn)
        {
            _player = new()
            {
                CharacterBody = ResourceLoader.Load<PackedScene>("res://Scenes/player.tscn").Instantiate() as CharacterBody3D,
                Camera = ResourceLoader.Load<PackedScene>("res://Scenes/camera.tscn").Instantiate() as Camera3D
            };

            AddChild(_player.CharacterBody);
            AddChild(_player.Camera);

            if (_player.CharacterBody == null)
                return;
            
            _player.CharacterBody.GlobalPosition = spawn;
            _player.CharacterStepCast = _player.CharacterBody.GetNode<RayCast3D>("StepCast");

            if (_player.CharacterStepCast == null)
                return;

            _player.CharacterStepCast.Position = new Vector3(0, GameSettings.CameraOffset, 0);
            _player.CharacterStepCast.TargetPosition = new Vector3(0, -GameSettings.CameraOffset + 1e-08f, 0);
        }

        Vector3 spawn = SetupScene();
        SetupPlayer(spawn);
    }

    public void HandleNavigation(PlayerInput inputs)
    {
        InputInterpreter currentInterpreter = _loadedInterpreters[_chunks[_currentChunk].Interpreter];

        void SwitchChunk(int chunk)
        {
            if (chunk == -1)
                return;

            // _chunks[_currentChunk].Events.OnExit();
            _currentChunk = _chunks[_currentChunk].VisibleChunks[chunk];
            // _chunks[_currentChunk].Events.OnEnter();
            
            GD.Print("Switch to: " + _chunks[_currentChunk].Name + ", With: " + _loadedInterpreters[_chunks[_currentChunk].Interpreter].GetType().Name);
        }

        currentInterpreter.HandleMovementInputs(inputs.NavigationInputs, _player);
        currentInterpreter.HandleBlockers(_chunks[_currentChunk].Blockers, _player);
        SwitchChunk(currentInterpreter.HandleSplits(_chunks[_currentChunk].Splits, _player.CharacterBody));
        
        if (PlayerSettings.Misc.DrawDebug)
        {
            Chunk[] connectedChunks = SplitsFuncs.GetConnectedChunks(_chunks[_currentChunk].Splits, _currentChunk, _chunks);
            SplitsFuncs.DrawDebugSplits(_chunks[_currentChunk].Splits, connectedChunks);
            SplitsFuncs.DrawDebugBlockers(_chunks[_currentChunk].Blockers);
        }
    }

    public void HandleCamera()
    {
        
    }
}

public class Player
{
    public CharacterBody3D CharacterBody;
    public RayCast3D CharacterStepCast;
    public Camera3D Camera;
}