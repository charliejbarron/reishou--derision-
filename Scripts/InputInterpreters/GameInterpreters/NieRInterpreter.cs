using Godot;

[GlobalClass]
public partial class NieRInterpreter : QuakeInterpreter
{
    public override void HandleMovementInputs(PlayerInput.NavigationInput navigationInput, Player player, InputInterpreterParameters parameters)
    {
        bool floored = player.CharacterBody.IsOnFloor();

        Vector3 HandleMovement(PlayerInput.MovementInput movementInput, CharacterBody3D body, Basis forward)
        {
            float moveSpeed = Interpreter.GetSprint(movementInput.Movement,  body.Velocity.Length(), movementInput.Sprint) > 0f && floored ? GameSettings.MaxSprintSpeed : GameSettings.MaxWalkSpeed;
            var velocities = Interpreter.CalculateInput(movementInput.Movement, forward, body.Velocity, floored);

            body.Position += Interpreter.HandleSteps(player, velocities.inputVelocity);

            Vector3 dash = Interpreter.Dash(movementInput.Dash, floored, movementInput.Movement, player.CharacterBody.IsOnWall()) * forward;
            bool canJump = Interpreter.CanJump(movementInput.Jump, floored, dash != Vector3.Zero);

            if (dash != Vector3.Zero)
                return dash + new Vector3(0, Interpreter.Gravity(), 0);

            float friction = Interpreter.CalculateFriction(floored, velocities.newVelocity, moveSpeed, movementInput.Movement.Length(), player.CharacterBody.IsOnWall());
            float speed = Interpreter.CalculateSpeed(velocities.oldVelocity.Length(), moveSpeed, velocities.newVelocity.Length(), friction);

            Vector3 finalVelocity = Interpreter.MovementVelocity(velocities.newVelocity, speed, body.Velocity.Y + Interpreter.Gravity());

            if (canJump)
                finalVelocity = Interpreter.Jump(finalVelocity, velocities.inputVelocity);

            return finalVelocity;
        }
        
        player.CharacterBody.Velocity = HandleMovement(navigationInput.MovementInputs, player.CharacterBody, new Basis(Vector3.Up, -player.Camera.GlobalRotation.Y));
        player.CharacterBody.MoveAndSlide();
    }
    
    public override Transform3D HandleCameraInputs(PlayerInput.NavigationInput navigationInput, Player player, Vector3 interpolated, InputInterpreterParameters parameters)
    {
        NieRInterpreterParameters nierParameters = parameters as NieRInterpreterParameters ?? new NieRInterpreterParameters();
        
        Transform3D HandleCamera(Vector2 cameraInput, Node3D camera, Vector3 bodyPosition)
        {
            Basis newBasis = Interpreter.InputBasis(Mathf.Clamp(camera.RotationDegrees.X - cameraInput.Y, -90f, 90f), camera.RotationDegrees.Y - cameraInput.X, 0);
            
            Transform3D transform = new Transform3D
            {
                Origin = bodyPosition + new Vector3(0, GameSettings.CameraOffset, 0) + newBasis * nierParameters.CameraOffset,
                Basis = newBasis
            };
            
            return transform;
        }
        
        return HandleCamera(Interpreter.CalcCameraInputs(navigationInput.CameraInputs), player.Camera, interpolated);
    }
}

public class NieRInterpreterParameters : InputInterpreterParameters
{
    public Vector3 CameraOffset = new (0, 0.2f, 6f);
}