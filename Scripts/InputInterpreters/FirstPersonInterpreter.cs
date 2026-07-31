using Godot;

[GlobalClass]
public partial class FirstPersonInterpreter : InputInterpreter
{
    public override void HandleNavigationInputs(PlayerInput.NavigationInput navigationInput, Player player, SharedVariables variables)
    {
        bool floored = player.CharacterBody.IsOnFloor();
        HandleJumpVars(variables.JumpVars, floored, navigationInput.MovementInputs.Jump);

        void HandleMovement(PlayerInput.MovementInput movementInput, CharacterBody3D body, Basis forward)
        {
            Vector3 Move()
            {
                float moveSpeed = movementInput.Sprint > 0f ? GameSettings.MaxSprintSpeed : GameSettings.MaxWalkSpeed;

                var velocities = CalculateInput(movementInput.Movement, forward, body.Velocity, floored);

                float friction = CalculateFriction(floored, velocities.newVelocity, moveSpeed, movementInput.Movement.Length());
                float speed = CalculateSpeed(velocities.oldVelocity.Length(), moveSpeed, velocities.newVelocity.Length(), friction);

                return MovementVelocity(velocities.newVelocity, speed, body.Velocity.Y);
            }

            Vector3 Jump()
            {
                Vector3 jump = Vector3.Up * GameSettings.JumpHeight;
                variables.JumpVars.JumpTiming = variables.JumpVars.AirTiming = -1f;
                
                return jump;
            }

            body.Velocity = Move();

            if (CanJump(navigationInput.MovementInputs.Jump, variables.JumpVars, floored))
                body.Velocity += Jump();

            body.MoveAndSlide();
        }

        void HandleCamera(Vector2 cameraInput, Node3D camera, Vector3 bodyPosition, float xInput)
        {
            camera.GlobalPosition = bodyPosition + new Vector3(0, 1.57f, 0);

            float roll = float.Lerp(camera.RotationDegrees.Z, -PlayerSettings.PlayerInputSetting.HorizontalCameraTiltFp * xInput,
                GameManager.Delta * PlayerSettings.PlayerInputSetting.HorizontalCameraTiltSpeedFp);

            cameraInput *= PlayerSettings.PlayerInputSetting.SensitivityReductionFp;
            camera.RotationDegrees = new Vector3(Mathf.Clamp(camera.RotationDegrees.X - cameraInput.Y, -90f, 90f), camera.RotationDegrees.Y - cameraInput.X, roll);
        }
        
        HandleCamera(navigationInput.CameraInputs, player.Camera, player.CharacterBody.GlobalPosition, navigationInput.MovementInputs.Movement.X);
        HandleMovement(navigationInput.MovementInputs, player.CharacterBody, new Basis(Vector3.Up, -player.Camera.GlobalRotation.Y));

        GD.Print(variables.JumpVars.AirTiming + "Air");
        GD.Print(variables.JumpVars.JumpTiming + "Jump");
    }
}