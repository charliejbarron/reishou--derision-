using Godot;

[GlobalClass]
public partial class QuakeInterpreter : InputInterpreter
{
    public override void HandleNavigationInputs(PlayerInput.NavigationInput navigationInput, Player player, SharedVariables variables)
    {
        bool floored = player.CharacterBody.IsOnFloor();
        HandleJumpVars(variables.JumpVars, floored, navigationInput.MovementInputs.Jump);

        Vector3 HandleMovement(PlayerInput.MovementInput movementInput, CharacterBody3D body, Basis forward)
        {
            
            float moveSpeed = movementInput.Sprint && floored ? GameSettings.MaxSprintSpeed : GameSettings.MaxWalkSpeed;
            var velocities = CalculateInput(movementInput.Movement, forward, body.Velocity, floored);
            
            Vector3 dash = Dash(variables.DashVars, movementInput.Dash, floored, movementInput.Movement) * forward;
            
            if (dash != Vector3.Zero)
            {
                return dash + new Vector3(0, Gravity(), 0);
            }

            float friction = CalculateFriction(floored, velocities.newVelocity, moveSpeed, movementInput.Movement.Length());
            float speed = CalculateSpeed(velocities.oldVelocity.Length(), moveSpeed, velocities.newVelocity.Length(), friction);
            
            Vector3 finalVelocity = MovementVelocity(velocities.newVelocity, speed, body.Velocity.Y + Gravity());

            bool canJump = CanJump(navigationInput.MovementInputs.Jump, variables.JumpVars, floored, true);
            finalVelocity = canJump ? Jump(variables.JumpVars, finalVelocity, velocities.inputVelocity) : finalVelocity;

            body.Position += HandleSteps(player, velocities.inputVelocity);
            
            return finalVelocity;
        }

        void HandleCamera(Vector2 cameraInput, Node3D camera, Vector3 bodyPosition, float xInput)
        {
            camera.GlobalPosition = bodyPosition + new Vector3(0, GameSettings.CameraOffset, 0);

            float roll = float.Lerp(camera.RotationDegrees.Z, -PlayerSettings.PlayerInput.HorizontalCameraTiltFp * xInput,
                GameManager.Delta * PlayerSettings.PlayerInput.HorizontalCameraTiltSpeedFp);

            cameraInput *= PlayerSettings.PlayerInput.SensitivityReductionFp;
            camera.RotationDegrees = new Vector3(Mathf.Clamp(camera.RotationDegrees.X - cameraInput.Y, -90f, 90f), camera.RotationDegrees.Y - cameraInput.X, roll);
        }

        HandleCamera(navigationInput.CameraInputs, player.Camera, player.CharacterBody.GlobalPosition, navigationInput.MovementInputs.Movement.X);

        player.CharacterBody.Velocity = HandleMovement(navigationInput.MovementInputs, player.CharacterBody, new Basis(Vector3.Up, -player.Camera.GlobalRotation.Y));
        player.CharacterBody.MoveAndSlide();
    }
}