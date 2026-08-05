using Godot;

[GlobalClass]
public partial class InputManager : Node
{
    Vector2 _mouseInput;
    float _sprintTimer;
    bool _scheduleDash;
    
    public PlayerInput CollectInputs()
    {
        PlayerInput playerInput = new()
        {
            NavigationInputs = new()
            {
                MovementInputs = CollectMovement(),
                CameraInputs = CameraInput(PlayerSettings.PlayerInput.MouseSensitivity, PlayerSettings.PlayerInput.ControllerSensitivity)
            }
        };

        return playerInput;
    }
    
    PlayerInput.MovementInput CollectMovement()
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

        bool DashInput()
        {
            bool input = Input.IsActionJustPressed("Dash");
            return input;
        }

        bool SprintInput()
        {
            return Input.IsActionPressed("Dash") || Input.IsActionPressed("Sprint");
        }

        PlayerInput.MovementInput movementInputs = new()
        {
            Movement = MovementInput(),
            Jump = JumpInput(),
            Dash = DashInput(),
            Sprint = SprintInput()
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
        public bool Dash;
        public bool Sprint;
    }

    public struct CombatInput
    {
    }
}
