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

            _player.Marker = _player.Camera.GetNode<Node3D>("Marker");
            _player.CharacterStepCast = _player.CharacterBody.GetNode<RayCast3D>("StepCast");

            _player.CharacterStepCast.Position = new Vector3(0, GameSettings.CameraOffset, 0);
            _player.CharacterStepCast.TargetPosition = new Vector3(0, -GameSettings.CameraOffset + 1e-08f, 0);
        }

        Vector3 spawn = SetupScene();
        SetupPlayer(spawn);
    }

    public void HandleNavigation(PlayerInput.NavigationInput inputs)
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

        currentInterpreter.HandleMovementInputs(inputs, _player, _loadedParameters[_chunks[_currentChunk].Interpreter][_chunks[_currentChunk].Parameters]);

        currentInterpreter.HandleBlockers(_chunks[_currentChunk].Blockers, _player);
        SwitchChunk(currentInterpreter.HandleSplits(_chunks[_currentChunk].Splits, _player.CharacterBody));

        _player.Marker.GlobalTransform =
            currentInterpreter.HandleCameraInputs(inputs, _player, _loadedParameters[_chunks[_currentChunk].Interpreter][_chunks[_currentChunk].Parameters]);
    }

    public override void _Process(double delta)
    {
        void Debug(Transform3D transform3D)
        {
            Chunk[] connectedChunks = SplitsFuncs.GetConnectedChunks(_chunks[_currentChunk].Splits, _currentChunk, _chunks);
            SplitsFuncs.DrawDebugSplits(_chunks[_currentChunk].Splits, connectedChunks);
            SplitsFuncs.DrawDebugBlockers(_chunks[_currentChunk].Blockers);

            GameFuncs.DrawPlayerDebug(transform3D, _player);
        }

        Transform3D headTrans = _player.Marker.GetGlobalTransformInterpolated();
        _player.Camera.GlobalTransform = headTrans;

        if (PlayerSettings.Misc.DrawDebug)
            Debug(headTrans);
    }
}

public class Player
{
    public CharacterBody3D CharacterBody;
    public RayCast3D CharacterStepCast;
    public Camera3D Camera;
    public Node3D Marker;
}