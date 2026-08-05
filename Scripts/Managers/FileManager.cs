using System;
using Godot;
using Newtonsoft.Json;
using System.Linq;

public static class FileManager
{
    public static (Chunk[] chunks, int current, InputInterpreter[] interpreters) LoadSceneNavigation()
    {
        Scene scene = JsonConvert.DeserializeObject<Scene>(TestJson());

        Chunk[] chunks = scene.Chunks;

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
        
        for (int i = 0; i < chunks.Length; i++)
        {
            if (chunks[i].Splits == null)
                continue;

            foreach (var split in chunks[i].Splits)
            {
                int index = chunks[i].VisibleChunks[split.Connected];
                Chunk chunkAdd = chunks[index];
                int currentChunkInd = Array.IndexOf(chunkAdd.VisibleChunks, i);

                if (currentChunkInd == index)
                    continue;
                
                Split inverseSplit = split with
                {
                    Connected = currentChunkInd,
                    Direction = -split.Direction
                };
                
                chunkAdd.Splits = chunkAdd.Splits == null ? new [] { inverseSplit } : chunkAdd.Splits.Append(inverseSplit).ToArray();
            }
        }
        
        GD.Print(JsonConvert.SerializeObject(chunks, Formatting.Indented));
        return (chunks, 0, loadedInterpreters); // switch to use the scenes startup chunk
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
                    Splits = new[]
                    {
                        new Split
                        {
                            Connected = 0,
                            Direction = new Vector2(1, 0),
                            Height = -3,
                            Width = 2.5f,
                            Position = new Vector3(16, 0, 0)
                        }
                    }
                },
                new Chunk
                {
                    Name = "Corridor",
                    Interpreter = 0,
                    VisibleChunks = [0]
                }
            }
        };

        return JsonConvert.SerializeObject(scene, Formatting.Indented);
    }
}