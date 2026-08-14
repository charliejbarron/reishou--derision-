using Godot;

[GlobalClass]
public partial class NieRInterpreter : QuakeInterpreter
{
    public override void HandleMovementInputs(PlayerInput.NavigationInput navigationInput, Player player, InputInterpreterParameters parameter)
    {
        bool floored = Interpreter.HandleFloor(player);

        Vector3 HandleMovement(PlayerInput.MovementInput movementInput, CharacterBody3D body, Basis forward)
        {
            float moveSpeed = Interpreter.GetSprint(movementInput.Movement,  body.Velocity.Length(), movementInput.Sprint) > 0f && floored ? GameSettings.MaxSprintSpeed : GameSettings.MaxWalkSpeed;
            var velocities = Interpreter.CalculateInput(movementInput.Movement, forward, body.Velocity, floored);

            Vector3 dash = Interpreter.Dash(movementInput.Dash, floored, movementInput.Movement, body.IsOnWall()) * forward;
            bool canJump = Interpreter.CanJump(movementInput.Jump, floored, dash != Vector3.Zero);

            if (dash != Vector3.Zero)
                return dash + new Vector3(0, Interpreter.Gravity(floored), 0);

            float friction = Interpreter.CalculateFriction(floored, velocities.newVelocity, moveSpeed, movementInput.Movement.Length(), body.IsOnWall());
            float speed = Interpreter.CalculateSpeed(velocities.oldVelocity.Length(), moveSpeed, velocities.newVelocity.Length(), friction);

            Vector3 finalVelocity = Interpreter.MovementVelocity(velocities.newVelocity, speed,  Interpreter.Gravity(floored, body.Velocity.Y));

            if (canJump)
                finalVelocity = Interpreter.Jump(finalVelocity, velocities.inputVelocity);

            return finalVelocity;
        }
        
        player.CharacterBody.Velocity = HandleMovement(navigationInput.MovementInputs, player.CharacterBody, new Basis(Vector3.Up, -player.Camera.GlobalRotation.Y));
        player.CharacterBody.MoveAndSlide();
    }
    
    public override Transform3D HandleCameraInputs(PlayerInput.NavigationInput navigationInput, Player player, Vector3 interpolated, InputInterpreterParameters inputParameters)
    {
        var parameters = Interpreter.ConvertParameters<NieRInterpreterParameters>(inputParameters);
        
        Transform3D HandleCamera(Vector2 cameraInput, Node3D camera, float cameraRot)
        {
            float autoX = cameraRot * parameters.CameraAutoRotation;
            Basis newBasis = Interpreter.InputBasis(Mathf.Clamp(camera.RotationDegrees.X - cameraInput.Y, parameters.MaxPitchAngles.X, parameters.MaxPitchAngles.Y), camera.RotationDegrees.Y - cameraInput.X - autoX, 0);
            
            Transform3D transform = new Transform3D
            {
                Origin = interpolated + Interpreter.HeadPosition() + newBasis * parameters.CameraOffset,
                Basis = newBasis
            };

            // GD.Print("origin: " + transform.Origin.Y);
            
            return transform;
        }
        
        return HandleCamera(Interpreter.CalcCameraInputs(navigationInput.CameraInputs), player.Camera, navigationInput.MovementInputs.Movement.X * 0.1f);
    }
}

public class NieRInterpreterParameters : QuakeInterpreterParameters
{
    public Vector3 CameraOffset = new (0, -0.1f, 2.5f);
    public float CameraAutoRotation = 1f;
}