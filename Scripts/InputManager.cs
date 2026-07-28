using Godot;

static class InputManager
{
	static GameInput _lastInput;

	static float _sprintTimer;

	public static readonly float DashPadding = 0.75f; 
	static bool _scheduleDash;
	public static GameInput CollectInputs(PlayerSettings.PlayerInputSetting settings, float delta)
	{
		Vector2 MovementInput()
		{
			Vector2 movement = new Vector2(Input.GetAxis("Left", "Right"), Input.GetAxis("Backward", "Forward"));

			if (movement.Length() > 1f)
				movement = movement.Normalized();

			return movement;
		}

		bool JumpInput(bool hold)
		{
			return hold ? Input.IsActionJustPressed("Jump") : Input.IsActionPressed("Jump");
		}

		float DashInput()
		{
			bool input = Input.IsActionJustPressed("Dash");
			
			if (_lastInput.Dash <= 0f)
				return input || _scheduleDash ? DashPadding : 0f;
			
			_scheduleDash = (_scheduleDash || input) && _lastInput.Dash < 0.3f;
			return _lastInput.Dash - delta;
		}
		
		float SprintInput(bool toggle, float padding)
		{
			bool input = Input.IsActionPressed("Dash") || Input.IsActionPressed("Sprint");
			bool held = _lastInput.Sprint > 1e-08f && _lastInput.Movement != Vector2.Zero && toggle;

			bool dashing = (input || held) && _lastInput.Movement != Vector2.Zero;
			bool dashed = _lastInput.Dash > DashPadding - 1e-08;

			_sprintTimer = dashing ? padding : Mathf.Max(_sprintTimer - delta, 0f);
			
			return _sprintTimer > 1e-08 ? Mathf.Min(_lastInput.Sprint + delta, dashed ? 1.5f : 3f) : 0f;
		}
		
		GameInput gameInput = new ()
		{
			Movement = MovementInput(),
			Jumped = JumpInput(settings.HoldJump),
			Dash = DashInput(),
			Sprint = SprintInput(settings.ToggleSprint, settings.SprintPadding)
		};

		_lastInput = gameInput;
		return gameInput;
	}
}

public struct GameInput
{
	public Vector2 Movement;
	public bool Jumped;
	public float Dash;
	public float Sprint;
}
