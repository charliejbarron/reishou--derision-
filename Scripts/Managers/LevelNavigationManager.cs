using Godot;

[GlobalClass]
public partial class LevelNavigationManager : Node
{
    // Player
    Player _player;

    // Level
    int _currentChunk;
    Chunk[] _chunks;

    InputInterpreter[] _loadedInterpreters;


    public void Setup((Chunk[] chunks, int current, InputInterpreter[] interpreters) scene)
    {
        void SetupScene()
        {
            _chunks = scene.chunks;
            _currentChunk = scene.current;

            _loadedInterpreters = scene.interpreters;
        }

        void SetupPlayer()
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

        SetupScene();
        SetupPlayer();
    }

    public void HandleNavigation(PlayerInput inputs)
    {
        InputInterpreter currentInterpreter = _loadedInterpreters[_chunks[_currentChunk].Interpreter];

        void SwitchChunk(int chunk)
        {
            if (PlayerSettings.Misc.DrawDebug)
            {
                foreach (var split in _chunks[_currentChunk].Splits)
                {
                    ChunkFunctions.DrawDebug(split, _chunks[_chunks[_currentChunk].VisibleChunks[split.Connected]]);
                }
            }

            if (chunk == -1)
                return;

            GD.Print(chunk);
            _currentChunk = _chunks[_currentChunk].VisibleChunks[chunk];
            GD.Print(_chunks[_currentChunk].Name);
        }

        currentInterpreter.HandleNavigationInputs(inputs.NavigationInputs, _player);
        SwitchChunk(currentInterpreter.HandleChunkNavigation(_chunks[_currentChunk].Splits, _player.CharacterBody.GlobalPosition));
    }
}

public class Player
{
    public CharacterBody3D CharacterBody;
    public RayCast3D CharacterStepCast;
    public Camera3D Camera;
}