using Godot;

[GlobalClass]
public partial class LevelNavigationManager : Node
{
    // Player
    Player _player;

    // - - Level - -
    int _currentChunk;
    ChunkInfo _info;
    
    // Data
    Chunk[] _chunks;
    
    InputInterpreter[] _loadedInterpreters;
    InputInterpreterParameters[][] _loadedParameters;


    public void Setup((Chunk[] chunks, int current, InputInterpreter[] interpreters, InputInterpreterParameters[][] parametersArray) scene)
    {
        Vector3 SetupScene()
        {
            _chunks = scene.chunks;
            _currentChunk = scene.current;

            _loadedInterpreters = scene.interpreters;
            _loadedParameters = scene.parametersArray;

            return _chunks[_currentChunk].SpawnPoint;
        }

        void SetupPlayer(Vector3 spawn)
        {
            _player = new()
            {
                CharacterBody = GetNode<CharacterBody3D>("Player"),
                Camera = GetNode<Camera3D>("Camera3D")
            };

            if (_player.CharacterBody == null || _player.Camera == null)
                return;

            _player.CharacterBody.GlobalPosition = spawn;

            _player.CharacterStepCast = _player.CharacterBody.GetNode<RayCast3D>("StepCast");

            _player.CharacterStepCast.Position = new Vector3(0, GameSettings.CameraOffset, 0);
            _player.CharacterStepCast.TargetPosition = new Vector3(0, -GameSettings.CameraOffset + 1e-08f, 0);
        }

        Vector3 spawn = SetupScene();
        SetupPlayer(spawn);
    }

    public void HandleNavigation(PlayerInput.NavigationInput inputs)
    {
        _info = GetChunkInfo(_currentChunk);
        
        void SwitchChunk(int chunk)
        {
            // _chunks[_currentChunk].Events.OnExit();
            _currentChunk = _chunks[_currentChunk].VisibleChunks[chunk];
            // _chunks[_currentChunk].Events.OnEnter();

            GD.Print("Switch to: " + _chunks[_currentChunk].Name + ", With: " + _loadedInterpreters[_chunks[_currentChunk].Interpreter].GetType().Name);
        }

        _info.Interpreter.HandleMovementInputs(inputs, _player, _info.Parameter);

        _info.Interpreter.HandleBlockers(_info.Blockers, _player);
        int chunk = _info.Interpreter.CheckSplits(_info.Splits, _player.CharacterBody);
        
        if (chunk != -1)
            SwitchChunk(chunk);
    }

    public void HandleCamera(PlayerInput.NavigationInput inputs)
    {
        Vector3 interpolated = _player.CharacterBody.GetGlobalTransformInterpolated().Origin;
        
        void Debug(Transform3D transform3D)
        {
            Chunk[] connectedChunks = SplitsFuncs.GetConnectedChunks(_chunks[_currentChunk].Splits, _currentChunk, _chunks);
            
            Debugging.DrawDebugSplits(_chunks[_currentChunk].Splits, connectedChunks);
            Debugging.DrawDebugBlockers(_chunks[_currentChunk].Blockers);
            Debugging.DrawPlayerDebug(transform3D, interpolated);
            Debugging.DrawDebugAxis(transform3D, _player);
        }
        
        _player.Camera.GlobalTransform = _info.Interpreter.HandleCameraInputs(inputs, _player, interpolated, _info.Parameter);
        
        if (PlayerSettings.Misc.DrawDebug)
            Debug(_player.Camera.GlobalTransform);
    }

    ChunkInfo GetChunkInfo(int chunkInd)
    {
        Chunk current = _chunks[chunkInd];
        
        ChunkInfo info = new ChunkInfo
        {
            Interpreter = _loadedInterpreters[current.Interpreter],
            Parameter = _loadedParameters[current.Interpreter][current.Parameters],
            Splits = current.Splits,
            Blockers = current.Blockers
        };

        return info;
    }
}

public class Player
{
    public CharacterBody3D CharacterBody;
    public RayCast3D CharacterStepCast;
    public Camera3D Camera;
}

public class ChunkInfo
{
    public  InputInterpreter Interpreter;
    public  InputInterpreterParameters Parameter;
    public  Split[] Splits;
    public  Split[] Blockers;
}