using Godot;

[GlobalClass]
public partial class QuakeInterpreter : InputInterpreter
{
    public override void HandleNavigationInputs(PlayerInput.NavigationInput navigationInput, Player player)
    {
        bool floored = player.CharacterBody.IsOnFloor();
        SharedFunctions.HandleJumpVars(floored, navigationInput.MovementInputs.Jump);

        Vector3 HandleMovement(PlayerInput.MovementInput movementInput, CharacterBody3D body, Basis forward)
        {
            float moveSpeed = SharedFunctions.GetSprint(movementInput.Movement, movementInput.Sprint) > 0f && floored ? GameSettings.MaxSprintSpeed : GameSettings.MaxWalkSpeed;
            var velocities = SharedFunctions.CalculateInput(movementInput.Movement, forward, body.Velocity, floored);

            body.Position += SharedFunctions.HandleSteps(player, velocities.inputVelocity);

            Vector3 dash = SharedFunctions.Dash(movementInput.Dash, floored, movementInput.Movement, player.CharacterBody.IsOnWall()) * forward;
            bool canJump = SharedFunctions.CanJump(navigationInput.MovementInputs.Jump, floored, dash != Vector3.Zero);

            if (dash != Vector3.Zero)
                return dash + new Vector3(0, SharedFunctions.Gravity(), 0);

            float friction = SharedFunctions.CalculateFriction(floored, velocities.newVelocity, moveSpeed, movementInput.Movement.Length(), player.CharacterBody.IsOnWall());
            float speed = SharedFunctions.CalculateSpeed(velocities.oldVelocity.Length(), moveSpeed, velocities.newVelocity.Length(), friction);

            Vector3 finalVelocity = SharedFunctions.MovementVelocity(velocities.newVelocity, speed, body.Velocity.Y + SharedFunctions.Gravity());

            if (canJump)
                finalVelocity = SharedFunctions.Jump(finalVelocity, velocities.inputVelocity);

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