using Godot;

[GlobalClass]
public partial class InputManager : Node
{
    PlayerInput _lastInput;

    Vector2 _mouseInput;

    float _sprintTimer;

    public static readonly float DashCooldown = 0.75f;
    bool _scheduleDash;
    
    public PlayerInput CollectInputs(float delta)
    {
        PlayerInput playerInput = new()
        {
            NavigationInputs = new()
            {
                MovementInputs = CollectMovement(_lastInput.NavigationInputs.MovementInputs, delta),
                CameraInputs = CameraInput(PlayerSettings.PlayerInputSetting.MouseSensitivity, PlayerSettings.PlayerInputSetting.ControllerSensitivity)
            }
        };

        _lastInput = playerInput;
        return playerInput;
    }
    
    PlayerInput.MovementInput CollectMovement(PlayerInput.MovementInput movementInput, float delta)
    {
        Vector2 MovementInput()
        {
            Vector2 movement = new Vector2(Input.GetAxis("Left", "Right"), Input.GetAxis("Backward", "Forward"));

            if (movement.Length() > 1f)
                movement = movement.Normalized();

            return movement;
        }

        int JumpInput()
        {
            if (Input.IsActionJustPressed("Jump"))
                return 2;
            if (Input.IsActionPressed("Jump"))
                return 1;

            return 0;
        }

        float DashInput()
        {
            bool input = Input.IsActionJustPressed("Dash");

            if (movementInput.Dash <= 0f)
                return input || _scheduleDash ? DashCooldown : 0f;

            _scheduleDash = (_scheduleDash || input) && movementInput.Dash < 0.3f;
            return movementInput.Dash - delta;
        }

        float SprintInput(bool toggle, float padding)
        {
            bool input = Input.IsActionPressed("Dash") || Input.IsActionPressed("Sprint");
            bool held = movementInput.Sprint > 1e-08 && movementInput.Movement != Vector2.Zero && toggle;

            bool sprinting = (input || held) && movementInput.Movement != Vector2.Zero;

            _sprintTimer = sprinting ? padding : Mathf.Max(_sprintTimer - delta, 0f);

            return _sprintTimer > 1e-08 ? Mathf.Min(movementInput.Sprint + delta, movementInput.Dash > DashCooldown - 1e-08 ? 1.5f : 3f) : 0f;
        }

        PlayerInput.MovementInput movementInputs = new()
        {
            Movement = MovementInput(),
            Jump = JumpInput(),
            Dash = DashInput(),
            Sprint = SprintInput(PlayerSettings.PlayerInputSetting.ToggleSprint, PlayerSettings.PlayerInputSetting.SprintPadding)
        };

        return movementInputs;
    }

    Vector2 CameraInput(Vector2 mouseSens, Vector2 controllerSens)
    {
        Vector2 controllerInput = new Vector2(Input.GetAxis("Camera-Left", "Camera-Right"), -Input.GetAxis("Camera-Down", "Camera-Up"));

        Vector2 mouseInput = _mouseInput * mouseSens * 0.01f;
        controllerInput *= controllerSens * 0.5f;

        _mouseInput = Vector2.Zero;

        Vector2 addedInputs = mouseInput + controllerInput;
        // float limit = Mathf.Max(mouseInput.Length(), controllerInput.Length());
        // Vector2 newInput = addedInputs.Normalized() * limit;

        return addedInputs;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion inputEvent)
        {
            _mouseInput += inputEvent.Relative;
        }
    }

    public override void _Notification(int what)
    {
        if (what == NotificationApplicationFocusIn)
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
    }
}

public struct PlayerInput
{
    public NavigationInput NavigationInputs;
    public CombatInput CombatInputs;
    
    public struct NavigationInput
    {
        public MovementInput MovementInputs;
        public Vector2 CameraInputs;
    }

    public struct MovementInput
    {
        public Vector2 Movement;
        public int Jump;
        public float Dash;
        public float Sprint;
    }

    public struct CombatInput
    {
    }
}
