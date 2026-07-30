using System;
using Godot;
using Newtonsoft.Json;

[GlobalClass]
public partial class GameManager : Node
{
    InputManager _inputManager;
    MovementManager _movementManager;
    
    readonly string _playerScene = "";

    TESTINGGG _testinggg;

    public override void _Ready()
    {
        _inputManager = GetNode<InputManager>("InputManager");
        _movementManager = GetNode<MovementManager>("MovementManager");

        // _testinggg = new()
        // {
        //     bleh = 2,
        //     BLAHHH = "djasksda",
        //     _tests = new[]
        //     {
        //         new Test
        //         {
        //             Type = Test.Types.test3,
        //             AAA = new test3()
        //             {
        //                 BBB = 3,
        //                 GGG = "hgdsf"
        //             }
        //         },
        //         new Test
        //         {
        //             Type = Test.Types.test4,
        //             AAA = new test4()
        //             {
        //                 BBB = 6,
        //                 GHH = true,
        //             }
        //         },
        //         new Test
        //         {
        //             Type = Test.Types.test3,
        //             AAA = new test3()
        //             {
        //                 BBB = 3,
        //                 GGG = "hgdsf"
        //             }
        //         }
        //     }
        // };
        //
        // string json = JsonConvert.SerializeObject(_testinggg, Formatting.Indented);
        //
        // GD.Print(json);
        //
        // TESTINGGG testJson = JsonConvert.DeserializeObject<TESTINGGG>(json);
        //
        // foreach (var test in testJson._tests)
        // {
        //     string parameters = JsonConvert.SerializeObject(test);
        //     GD.Print(test.Type);
        //     switch (test.Type)
        //     {
        //         case Test.Types.test3:
        //             GD.Print(JsonConvert.DeserializeObject<test3>(parameters));
        //             break;
        //         case Test.Types.test4:
        //             GD.Print(JsonConvert.DeserializeObject<test4>(parameters));
        //             break;
        //     }
        // }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
        GameInput gameInput = _inputManager.CollectInputs((float)delta);
        string inputJson = JsonConvert.SerializeObject(gameInput, Formatting.Indented);
        GD.Print(inputJson);
        
        _movementManager.HandleNavigation(gameInput.NavigationInputs);
    }
}



public class TESTINGGG
{
    public Test[] _tests;
    public int bleh;
    public string BLAHHH;
}

public class Test
{
    public Types Type;
    [JsonIgnore]
    public object AAA;
    
    public enum Types
    {
        test3,
        test4
    }
}

public class Test2
{
    public int BBB = 2;
}

public class test3 : Test2
{
    public string GGG;
}

public class test4 : Test2
{
    public bool GHH;
}