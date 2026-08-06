using Godot;

public class Scene
{
    public Chunk[] Chunks;
    public string[] Interpreters;
}

public class Chunk
{
    public string Name = "Chunk";
    public int[] VisibleChunks;
    public Split[] Splits;
    public int Interpreter = 0;
    public int Parameters = 0;
}

public struct Split
{
    public Vector3 Position;
    public Vector2 Direction;
    public float Width;
    public float Height;
    public int Connected;
}
