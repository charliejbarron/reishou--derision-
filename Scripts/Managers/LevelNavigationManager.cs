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

        void SwitchChunk()
        {
            if (PlayerSettings.Misc.DrawDebug)
                foreach (var split in _chunks[_currentChunk].Splits)
                    ChunkFunctions.DrawDebug(split, _chunks[_chunks[_currentChunk].VisibleChunks[split.Connected]]);

            int chunk = currentInterpreter.HandleChunkNavigation(_chunks[_currentChunk].Splits, _player.CharacterBody.GlobalPosition);

            if (chunk == -1)
                return;

            // _chunks[_currentChunk].Events.OnExit();
            _currentChunk = _chunks[_currentChunk].VisibleChunks[chunk];
            // _chunks[_currentChunk].Events.OnEnter();
            
            GD.Print("Switch to: " + _chunks[_currentChunk].Name + ", With: " + _loadedInterpreters[_chunks[_currentChunk].Interpreter].GetType().Name);
        }

        currentInterpreter.HandleMovementInputs(inputs.NavigationInputs, _player);
        SwitchChunk();
    }
}

public class Player
{
    public CharacterBody3D CharacterBody;
    public RayCast3D CharacterStepCast;
    public Camera3D Camera;
}