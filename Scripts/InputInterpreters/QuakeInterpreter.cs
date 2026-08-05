using Godot;

[GlobalClass]
public partial class QuakeInterpreter : InputInterpreter
{
    public override void HandleNavigationInputs(PlayerInput.NavigationInput navigationInput, Player player)
    {
        bool floored = player.CharacterBody.IsOnFloor();
        NavigationFunctions.HandleJumpVars(floored, navigationInput.MovementInputs.Jump);

        Vector3 HandleMovement(PlayerInput.MovementInput movementInput, CharacterBody3D body, Basis forward)
        {
            float moveSpeed = NavigationFunctions.GetSprint(movementInput.Movement, movementInput.Sprint) > 0f && floored ? GameSettings.MaxSprintSpeed : GameSettings.MaxWalkSpeed;
            var velocities = NavigationFunctions.CalculateInput(movementInput.Movement, forward, body.Velocity, floored);

            body.Position += NavigationFunctions.HandleSteps(player, velocities.inputVelocity);

            Vector3 dash = NavigationFunctions.Dash(movementInput.Dash, floored, movementInput.Movement, player.CharacterBody.IsOnWall()) * forward;
            bool canJump = NavigationFunctions.CanJump(navigationInput.MovementInputs.Jump, floored, dash != Vector3.Zero);

            if (dash != Vector3.Zero)
                return dash + new Vector3(0, NavigationFunctions.Gravity(), 0);

            float friction = NavigationFunctions.CalculateFriction(floored, velocities.newVelocity, moveSpeed, movementInput.Movement.Length(), player.CharacterBody.IsOnWall());
            float speed = NavigationFunctions.CalculateSpeed(velocities.oldVelocity.Length(), moveSpeed, velocities.newVelocity.Length(), friction);

            Vector3 finalVelocity = NavigationFunctions.MovementVelocity(velocities.newVelocity, speed, body.Velocity.Y + NavigationFunctions.Gravity());

            if (canJump)
                finalVelocity = NavigationFunctions.Jump(finalVelocity, velocities.inputVelocity);

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