using System;
using Godot;
using Newtonsoft.Json;
using System.Linq;

public static class FileManager
{
    public static (Chunk[] chunks, int current, InputInterpreter[] interpreters) LoadSceneChunks()
    {
        Scene scene = JsonConvert.DeserializeObject<Scene>(TestJson());

        Chunk[] fileChunks = scene.Chunks;

        InputInterpreter[] loadedInterpreters = LoadInterpreters(scene);
        AddSplits(fileChunks);

        GD.Print(JsonConvert.SerializeObject(fileChunks, Formatting.Indented));
        return (fileChunks, 0, loadedInterpreters); // switch to use the scenes startup chunk
    }

    static void AddSplits(Chunk[] chunks)
    {
        Split[][] oldSplits = new Split[chunks.Length][];

        for (int i = 0; i < chunks.Length; i++)
        {
            oldSplits[i] = chunks[i].Splits;
        }
        
        for (int i = 0; i < chunks.Length; i++)
        {
            if (oldSplits[i] == null)
                continue;

            for (int s = 0; s < oldSplits[i].Length; s++)
            {
                Split split = oldSplits[i][s];
                chunks[i].Splits[s].Direction = split.Direction.Normalized();

                int index = chunks[i].VisibleChunks[split.Connected];
                int currentChunkInd = Array.IndexOf(chunks[index].VisibleChunks, i);
                
                Split inverseSplit = split with
                {
                    Connected = currentChunkInd,
                    Direction = -split.Direction.Normalized()
                };

                chunks[index].Splits = chunks[index].Splits == null ? new [] { inverseSplit } : chunks[index].Splits.Append(inverseSplit).ToArray();
            }
        }
    }

    static InputInterpreter[] LoadInterpreters(Scene scene)
    {
        InputInterpreter[] loadedInterpreters = new InputInterpreter[1];

        for (int i = 0; i < loadedInterpreters.Length; i++)
        {
            if (GameSettings.BuiltinInterpreters.Contains(scene.Interpreters[i]))
            {
                Type type = Type.GetType(scene.Interpreters[i]);

                if (type != null)
                    loadedInterpreters[i] = (InputInterpreter)Activator.CreateInstance(type);
            }
            // Custom interpreters loaded via .pkc, then pulled in the same way (may need reference to assembly)
            // https://docs.godotengine.org/en/stable/tutorials/export/exporting_pcks.html#modding-considerations
        }

        return loadedInterpreters;
    }

    static string TestJson()
    {
        Scene scene = new Scene
        {
            Interpreters = new[]
            {
                "QuakeInterpreter"
            },
            Chunks = new[]
            {
                new Chunk
                {
                    Name = "Start Room",
                    Interpreter = 0,
                    VisibleChunks = [1],
                    Splits =
                    [
                        new Split
                        {
                            Connected = 0,
                            Direction = new Vector2(1, 0).Normalized(),
                            Height = -3,
                            Width = 2.5f,
                            Position = new Vector3(16, 0, 0)
                        }
                    ]
                },
                new Chunk
                {
                    Name = "Corridor",
                    Interpreter = 0,
                    VisibleChunks = [2, 0],
                    Splits =
                    [
                        new Split
                        {
                            Connected = 0,
                            Direction = new Vector2(1, 0),
                            Height = -3,
                            Width = 2.5f,
                            Position = new Vector3(25, 0, 0)
                        }
                    ]
                },
                new Chunk
                {
                    Name = "CorridorAAA",
                    Interpreter = 0,
                    VisibleChunks = [1, 0, 3]
                },
                new Chunk
                {
                    Name = "CorridorEnd",
                    Interpreter = 0,
                    VisibleChunks = [1, 2],
                    Splits =
                    [
                        new Split
                        {
                            Connected = 1,
                            Direction = new Vector2(-1, 1),
                            Height = -5,
                            Width = 2.5f,
                            Position = new Vector3(35, 0, 0)
                        }
                    ]
                }
            }
        };

        return JsonConvert.SerializeObject(scene, Formatting.Indented);
    }
}