using System;
using Godot;
using Newtonsoft.Json;
using System.Linq;

public static class FileManager
{
    public static (Chunk[] chunks, int current, InputInterpreter[] interpreters, InputInterpreterParameters[][] parametersArray) LoadSceneChunks()
    {
        Scene scene = JsonConvert.DeserializeObject<Scene>(TestJson());

        Chunk[] fileChunks = AddSharedSplits(scene.Chunks);

        InputInterpreter[] loadedInterpreters = LoadInterpreters(scene);

        InputInterpreterParameters[][] parametersArray = LoadParameters(scene);

        return (fileChunks, 0, loadedInterpreters, parametersArray); // Look at /mnt/HHD/Godot/SekiroGameLayout "ConnectedScenes":
    }

    static Chunk[] AddSharedSplits(Chunk[] chunks)
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

        return chunks;
    }

    static InputInterpreter[] LoadInterpreters(Scene scene)
    {
        InputInterpreter[] loadedInterpreters = new InputInterpreter[scene.Interpreters.Length];

        for (int i = 0; i < loadedInterpreters.Length; i++)
        {
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

    static InputInterpreterParameters[][] LoadParameters(Scene scene)
    {
        InputInterpreterParameters[][] parameters = new InputInterpreterParameters[scene.Interpreters.Length][];

        for (int p = 0; p < parameters.Length; p++)
        {
            if (GameSettings.BuiltinInterpreters.Contains(scene.Interpreters[p].TypeName))
            {
                Type type = Type.GetType(scene.Interpreters[p].TypeName + "Parameters");

                if (type != null && scene.Interpreters[p].ParametersJson != null)
                {
                    parameters[p] = (InputInterpreterParameters[])JsonConvert.DeserializeObject(scene.Interpreters[p].ParametersJson, type.MakeArrayType());
                    continue;
                }
            }

            parameters[p] = new InputInterpreterParameters[] { new() };
        }

        return parameters;
    }


    static string TestJson()
    {
        Scene scene = new Scene
        {
            Interpreters =
            [
                new SceneInterpreter
                {
                    TypeName = "QuakeInterpreter",
                    // ParametersJson = JsonConvert.SerializeObject(
                    //     new InputInterpreterParameters[]
                    //     {
                    //         new QuakeInterpreterParameters
                    //         {
                    //             SensitivityMult = new Vector2(1f, 0.5f)
                    //         }
                    //     }
                    // )
                },
                new SceneInterpreter
                {
                    TypeName = "NieRInterpreter",
                    ParametersJson = JsonConvert.SerializeObject( // Serialize to string, loaded later dynamically
                        new InputInterpreterParameters[]
                        {
                            new NieRInterpreterParameters(), // Blank parameters, NOT required or loaded by default
                            new NieRInterpreterParameters
                            {
                                CameraOffset = new Vector3(0, 0.2f, 10f),
                                CameraAutoRotation = 1.2f,
                                MaxPitchAngles = new Vector2(-45f, -45f)
                            }
                        }
                    )
                }
            ],
            Chunks = new[]
            {
                new Chunk
                {
                    Name = "Start Room",
                    SpawnPoint = Vector3.Zero,
                    Interpreter = 1,
                    VisibleChunks = [1],
                    Splits =
                    [
                        new()
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
                        new()
                        {
                            Connected = 0,
                            Direction = new Vector2(1, 0),
                            Height = -5,
                            Width = 6.75f,
                            Position = new Vector3(16, 0, 9.25f)
                        },
                        new()
                        {
                            Connected = 0,
                            Direction = new Vector2(1, 0),
                            Height = -5,
                            Width = 6.75f,
                            Position = new Vector3(16, 0, -9.25f)
                        },
                        new()
                        {
                            Connected = 0,
                            Direction = new Vector2(-1, 0),
                            Height = -5,
                            Width = 16f,
                            Position = new Vector3(-16, 0, 0)
                        },
                        new()
                        {
                            Connected = 0,
                            Direction = new Vector2(0, -1),
                            Height = -5,
                            Width = 16f,
                            Position = new Vector3(0, 0, -16)
                        },
                        new()
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
                    Parameters = 1,
                    VisibleChunks = [2, 0],
                    Splits =
                    [
                        new()
                        {
                            Connected = 0,
                            Direction = new Vector2(1, 0),
                            Height = 3,
                            Width = 2.5f,
                            Position = new Vector3(22, 0, 0)
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
                    Interpreter = 1,
                    Parameters = 0,
                    VisibleChunks = [1, 2],
                    Splits =
                    [
                        new()
                        {
                            Connected = 1,
                            Direction = new Vector2(-1, 0),
                            Height = -5,
                            Width = 2.5f,
                            Position = new Vector3(35, 0, 0)
                        }
                    ]
                }
            }
        };

        GD.Print(JsonConvert.SerializeObject(scene, Formatting.Indented));
        return JsonConvert.SerializeObject(scene, Formatting.Indented);
    }
}