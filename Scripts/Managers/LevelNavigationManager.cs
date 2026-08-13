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
            
            _info = GetChunkInfo(_currentChunk);
            
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

            // Steps

            _player.CharacterFloorCast = _player.CharacterBody.GetNode<ShapeCast3D>("FloorCast");

            _player.CharacterFloorCast.Position = Vector3.Up * (GameSettings.StepUpHeight + 0.1f);
            _player.CharacterFloorCast.TargetPosition = -_player.CharacterFloorCast.Position + (Vector3.Down * GameSettings.StepDownHeight);

            CylinderShape3D shape = new()
            {
                Radius = 0.2f,
                Height = GameSettings.PlayerHeight - GameSettings.StepUpHeight
            };

            CollisionShape3D collider = _player.CharacterBody.GetNode<CollisionShape3D>("Collider");

            collider.Position = Vector3.Up * (GameSettings.PlayerHeight / 2f + GameSettings.StepUpHeight / 2f);
            collider.SetShape(shape);
        }

        Vector3 spawn = SetupScene();
        SetupPlayer(spawn);
    }

    public void HandleNavigation(PlayerInput.NavigationInput inputs)
    {
        void SwitchChunk(int chunk)
        {
            // if new interpreter OR different parameters'
            // interpreter.Events.OnExit()
            // _chunks[_currentChunk].Events.OnExit();
            _currentChunk = _chunks[_currentChunk].VisibleChunks[chunk];
            // _chunks[_currentChunk].Events.OnEnter();
            // interpreter.Events.OnEnter()
            
            _info = GetChunkInfo(_currentChunk);
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

        _player.Camera.GlobalTransform = _info.Interpreter.HandleCameraInputs(inputs, _player, interpolated, _info.Parameter);

        if (!PlayerSettings.Misc.DrawDebug)
            return;

        Debugging.HandleDebug(_player.Camera.GlobalTransform, interpolated, _player, _info, _info.Connected);
    }

    ChunkInfo GetChunkInfo(int chunkInd)
    {
        Chunk current = _chunks[chunkInd];
        ChunkInfo info = new ChunkInfo
        {
            Interpreter = _loadedInterpreters[current.Interpreter],
            Parameter = _loadedParameters[current.Interpreter][current.Parameters],
            Connected = SplitsFuncs.GetConnectedChunks(current.Splits, _currentChunk, _chunks),
            Splits = current.Splits,
            Blockers = current.Blockers
        };

        return info;
    }
}

public class Player
{
    public CharacterBody3D CharacterBody;
    public ShapeCast3D CharacterFloorCast;
    public Camera3D Camera;
}

public class ChunkInfo
{
    public InputInterpreter Interpreter;
    public InputInterpreterParameters? Parameter;
    public Chunk[] Connected;
    public Split[] Splits;
    public Split[] Blockers;
}