using Godot;

[GlobalClass]
public partial class InputManager : Node
{
    Vector2 _mouseInput;
    float _sprintTimer;
    bool _scheduleDash;
    
    public PlayerInput.NavigationInput CollectMovementInputs()
    {
        PlayerInput.NavigationInput movementInput = CollectMovement();

        return movementInput;
    }
    
    PlayerInput.NavigationInput CollectMovement()
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

        PlayerInput.CameraInput GetCameraInput(Vector2 mouseSens, Vector2 controllerSens)
        {
            Vector2 controllerInput = new Vector2(Input.GetAxis("Camera-Left", "Camera-Right"), -Input.GetAxis("Camera-Down", "Camera-Up"));

            Vector2 mouseInput = _mouseInput * mouseSens * 0.01f;
            controllerInput *= controllerSens * 15f * GameManager.Delta;

            _mouseInput = Vector2.Zero;

            PlayerInput.CameraInput input = new()
            {
                Mouse = mouseInput,
                Controller = controllerInput
            };

            return input;
        }
        
        PlayerInput.NavigationInput navigationInput = new()
        {
            MovementInputs = new ()
            {
                Movement = MovementInput(),
                Jump = JumpInput(),
                Dash = DashInput(),
                Sprint = SprintInput()
            },
            CameraInputs = GetCameraInput(PlayerSettings.PlayerInput.MouseSensitivity, PlayerSettings.PlayerInput.ControllerSensitivity)
        };

        return navigationInput;
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
        public CameraInput CameraInputs;
    }

    public struct MovementInput
    {
        public Vector2 Movement;
        public int Jump;
        public bool Dash;
        public bool Sprint;
    }
    
    public struct CameraInput
    {
        public Vector2 Mouse;
        public Vector2 Controller;
    }

    public struct CombatInput
    {
    }
}
