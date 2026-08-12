using Godot;

[GlobalClass]
public partial class NieRInterpreter : QuakeInterpreter
{
    public override void HandleMovementInputs(PlayerInput.NavigationInput navigationInput, Player player, InputInterpreterParameters parameter)
    {
        float step = Interpreter.HandleSteps(player);
        player.CharacterBody.Position += Vector3.Up * step;
        bool floored = step != 0;

        Vector3 HandleMovement(PlayerInput.MovementInput movementInput, CharacterBody3D body, Basis forward)
        {
            float moveSpeed = Interpreter.GetSprint(movementInput.Movement,  body.Velocity.Length(), movementInput.Sprint) > 0f && floored ? GameSettings.MaxSprintSpeed : GameSettings.MaxWalkSpeed;
            var velocities = Interpreter.CalculateInput(movementInput.Movement, forward, body.Velocity, floored);

            Vector3 dash = Interpreter.Dash(movementInput.Dash, floored, movementInput.Movement, player.CharacterBody.IsOnWall()) * forward;
            bool canJump = Interpreter.CanJump(movementInput.Jump, floored, dash != Vector3.Zero);

            if (dash != Vector3.Zero)
                return dash + new Vector3(0, Interpreter.Gravity(), 0);

            float friction = Interpreter.CalculateFriction(floored, velocities.newVelocity, moveSpeed, movementInput.Movement.Length(), player.CharacterBody.IsOnWall());
            float speed = Interpreter.CalculateSpeed(velocities.oldVelocity.Length(), moveSpeed, velocities.newVelocity.Length(), friction);

            Vector3 finalVelocity = Interpreter.MovementVelocity(velocities.newVelocity, speed, floored ? 0 : body.Velocity.Y + Interpreter.Gravity());

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
        
        Transform3D HandleCamera(Vector2 cameraInput, Node3D camera, Vector3 bodyPosition, float cameraRot)
        {
            float autoX = cameraRot * parameters.CameraAutoRotation;
            Basis newBasis = Interpreter.InputBasis(Mathf.Clamp(camera.RotationDegrees.X - cameraInput.Y, parameters.MaxPitchAngles.X, parameters.MaxPitchAngles.Y), camera.RotationDegrees.Y - cameraInput.X - autoX, 0);
            
            Transform3D transform = new Transform3D
            {
                Origin = bodyPosition + new Vector3(0, GameSettings.CameraOffset, 0) + newBasis * parameters.CameraOffset,
                Basis = newBasis
            };
            
            return transform;
        }
        
        return HandleCamera(Interpreter.CalcCameraInputs(navigationInput.CameraInputs), player.Camera, interpolated, navigationInput.MovementInputs.Movement.X * 0.1f);
    }
}

public class NieRInterpreterParameters : QuakeInterpreterParameters
{
    public Vector3 CameraOffset = new (0, -0.1f, 2.5f);
    public float CameraAutoRotation = 1f;
}