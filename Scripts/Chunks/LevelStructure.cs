using Godot;
using Newtonsoft.Json;
using JsonSubTypes;
public class Scene
{
    public Chunk[] Chunks;
    public SceneInterpreter[] Interpreters;
}

public struct Chunk()
{
    public string Name = "Chunk";
    public Vector3 SpawnPoint = default;
    public int[] VisibleChunks = new int[] { };
    public Split[] Splits = new Split[] { };
    public Split[] Blockers = new Split[] { };
    public int Interpreter = 0;
    public int Parameters = 0;
    // public GameEvents Events = new GameEvents();
}

public struct Split
{
    public Vector3 Position;
    public Vector2 Direction;
    public float Width;
    public float Height;
    public int Connected;
}

[JsonConverter(typeof(JsonSubtypes), "TypeName")]
public class SceneInterpreter
{
    public string TypeName;
    public string ParametersJson;
}

public class GameEvents
{
    public void OnEnter()
    {
        
    }

    public void OnExit()
    {
        
    }
}
