using Godot;
using Newtonsoft.Json;

[GlobalClass]
public partial class GameManager : Node
{
	PlayerSettings _settings;
	public override void _Ready()
	{
		_settings = new()
		{
			inputSettings = new()
			{
				HoldJump = false,
				ToggleSprint = true,
				SprintPadding = 0.25f
			}
		};
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GameInput gameInput = InputManager.CollectInputs(_settings.inputSettings, (float)delta);
		string inputJson = JsonConvert.SerializeObject(gameInput, Formatting.Indented);
		GD.Print(inputJson);
	}
}
