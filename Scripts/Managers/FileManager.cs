using System;
using Godot;
using Newtonsoft.Json;
using System.Linq;

public static class FileManager
{
    public static (Chunk[] chunks, int current, InputInterpreter[] interpreters) LoadSceneChunks()
    {
        Scene scene = JsonConvert.DeserializeObject<Scene>(TestJson());

        Chunk[] fileChunks = SceneFunctions.AddSharedSplits(scene.Chunks);

        InputInterpreter[] loadedInterpreters = SceneFunctions.LoadInterpreters(scene);

        // GD.Print(JsonConvert.SerializeObject(scene, Formatting.Indented));
        return (fileChunks, scene.StartingChunk, loadedInterpreters); // switch to use the scenes startup chunk
    }

    static class SceneFunctions
    {
        internal static Chunk[] AddSharedSplits(Chunk[] chunks)
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
                    if (oldSplits[i][s].Connected < 0)
                        continue;

                    Split split = oldSplits[i][s];
                    chunks[i].Splits[s].Direction = split.Direction.Normalized();

                    int index = chunks[i].VisibleChunks[split.Connected];
                    int currentChunkInd = Array.IndexOf(chunks[index].VisibleChunks, i);

                    Split inverseSplit = split with
                    {
                        Connected = currentChunkInd,
                        Direction = -split.Direction.Normalized()
                    };

                    chunks[index].Splits = chunks[index].Splits == null ? new[] { inverseSplit } : chunks[index].Splits.Append(inverseSplit).ToArray();
                }
            }

            GD.Print(JsonConvert.SerializeObject(chunks, Formatting.Indented));

            return chunks;
        }

        internal static InputInterpreter[] LoadInterpreters(Scene scene)
        {
            InputInterpreter[] loadedInterpreters = new InputInterpreter[scene.Interpreters.Length];

            for (int i = 0; i < loadedInterpreters.Length; i++)
            {
                GD.Print(scene.Interpreters[i].TypeName);
                if (GameSettings.BuiltinInterpreters.Contains(scene.Interpreters[i].TypeName))
                {
                    Type type = Type.GetType(scene.Interpreters[i].TypeName);

                    if (type != null)
                        loadedInterpreters[i] = (InputInterpreter)Activator.CreateInstance(type);
                }
                // Custom interpreters loaded via .pkc, then pulled in the same way (may need reference to assembly)
                // https://docs.godotengine.org/en/stable/tutorials/export/exporting_pcks.html#modding-considerations
            }

            return loadedInterpreters;
        }
    }

    static string TestJson()
    {
        Scene scene = new Scene
        {
            StartingChunk = 0,
            Interpreters =
            [
                new SceneInterpreter
                {
                    TypeName = "QuakeInterpreter",
                    Parameters = []
                },
                new SceneInterpreter
                {
                    TypeName = "NieRInterpreter",
                    Parameters = []
                }
            ],
            Chunks = new[]
            {
                new Chunk
                {
                    Name = "Start Room",
                    SpawnPoint = Vector3.Zero,
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
                    ],
                    Blockers =
                    [
                        new Split
                        {
                            Connected = 0,
                            Direction = new Vector2(1, 0),
                            Height = -5,
                            Width = 6.75f,
                            Position = new Vector3(16, 0, 9.25f)
                        },
                        new Split
                        {
                            Connected = 0,
                            Direction = new Vector2(1, 0),
                            Height = -5,
                            Width = 6.75f,
                            Position = new Vector3(16, 0, -9.25f)
                        },
                        new Split
                        {
                            Connected = 0,
                            Direction = new Vector2(-1, 0),
                            Height = -5,
                            Width = 16f,
                            Position = new Vector3(-16, 0, 0)
                        },
                        new Split
                        {
                            Connected = 0,
                            Direction = new Vector2(0, -1),
                            Height = -5,
                            Width = 16f,
                            Position = new Vector3(0, 0, -16)
                        },
                        new Split
                        {
                            Connected = 0,
                            Direction = new Vector2(0, 1),
                            Height = -5,
                            Width = 16f,
                            Position = new Vector3(0, 0, 16)
                        }
                    ]
                },
                new Chunk
                {
                    Name = "Corridor",
                    SpawnPoint = new Vector3(21, 0, 0),
                    Interpreter = 1,
                    VisibleChunks = [2, 0],
                    Splits =
                    [
                        new Split
                        {
                            Connected = 0,
                            Direction = new Vector2(1, 0),
                            Height = 3,
                            Width = 2.5f,
                            Position = new Vector3(25, 0, 0)
                        }
                    ]
                },
                new Chunk
                {
                    Name = "CorridorAAA",
                    SpawnPoint = new Vector3(30, 0, 0),
                    Interpreter = 0,
                    VisibleChunks = [1, 0, 3]
                },
                new Chunk
                {
                    Name = "CorridorEnd",
                    SpawnPoint = new Vector3(40, 0, 1.5f),
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