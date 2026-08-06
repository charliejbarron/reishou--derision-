using Godot;

[GlobalClass]
public partial class NieRInterpreter : QuakeInterpreter
{
    public override void HandleMovementInputs(PlayerInput.NavigationInput navigationInput, Player player)
    {
        bool floored = player.CharacterBody.IsOnFloor();

        Vector3 HandleMovement(PlayerInput.MovementInput movementInput, CharacterBody3D body, Basis forward)
        {
            float moveSpeed = Interpreter.GetSprint(movementInput.Movement, movementInput.Sprint) > 0f && floored ? GameSettings.MaxSprintSpeed : GameSettings.MaxWalkSpeed;
            var velocities = Interpreter.CalculateInput(movementInput.Movement, forward, body.Velocity, floored);

            body.Position += Interpreter.HandleSteps(player, velocities.inputVelocity);

            Vector3 dash = Interpreter.Dash(movementInput.Dash, floored, movementInput.Movement, player.CharacterBody.IsOnWall()) * forward;
            bool canJump = Interpreter.CanJump(navigationInput.MovementInputs.Jump, floored, dash != Vector3.Zero);

            if (dash != Vector3.Zero)
                return dash + new Vector3(0, Interpreter.Gravity(), 0);

            float friction = Interpreter.CalculateFriction(floored, velocities.newVelocity, moveSpeed, movementInput.Movement.Length(), player.CharacterBody.IsOnWall());
            float speed = Interpreter.CalculateSpeed(velocities.oldVelocity.Length(), moveSpeed, velocities.newVelocity.Length(), friction);

            Vector3 finalVelocity = Interpreter.MovementVelocity(velocities.newVelocity, speed, body.Velocity.Y + Interpreter.Gravity());

            if (canJump)
                finalVelocity = Interpreter.Jump(finalVelocity, velocities.inputVelocity);

            return finalVelocity;
        }

        Transform3D HandleCamera(Vector2 cameraInput, Node3D camera, Vector3 bodyPosition)
        {
            Transform3D transform = new Transform3D();

            transform.Origin = bodyPosition + new Vector3(0, GameSettings.CameraOffset, 0) + camera.Basis.Z * 4f;
            transform.Basis = Interpreter.InputBasis(Mathf.Clamp(camera.RotationDegrees.X - cameraInput.Y, -90f, 90f), camera.RotationDegrees.Y - cameraInput.X, 0);

            GD.Print(cameraInput);
            
            return transform;
        }

        player.Camera.GlobalTransform = HandleCamera(navigationInput.CameraInputs, player.Camera, player.CharacterBody.GlobalPosition);

        player.CharacterBody.Velocity = HandleMovement(navigationInput.MovementInputs, player.CharacterBody, new Basis(Vector3.Up, -player.Camera.GlobalRotation.Y));
        player.CharacterBody.MoveAndSlide();
    }
}
