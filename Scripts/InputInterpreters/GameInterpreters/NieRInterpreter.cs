using Godot;

[GlobalClass]
public partial class NieRInterpreter : QuakeInterpreter
{
    public override void HandleMovementInputs(PlayerInput.NavigationInput navigationInput, Player player, InputInterpreterParameters parameter)
    {
        player.CharacterBody.Velocity = Interpreter.HandleMovement(navigationInput.MovementInputs, player, new Basis(Vector3.Up, -player.Camera.GlobalRotation.Y));
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