using Godot;

[GlobalClass]
public partial class QuakeInterpreter : InputInterpreter
{
    public override void HandleMovementInputs(PlayerInput.NavigationInput navigationInput, Player player, InputInterpreterParameters parameter)
    {
        player.CharacterBody.Velocity = Interpreter.HandleMovement(navigationInput.MovementInputs, player, new Basis(Vector3.Up, -player.Camera.GlobalRotation.Y));
        player.CharacterBody.MoveAndSlide();
    }

    public override Transform3D HandleCameraInputs(PlayerInput.NavigationInput navigationInput, Player player, Vector3 interpolated,  InputInterpreterParameters? inputParameters)
    {
        var parameters = Interpreter.ConvertParameters<QuakeInterpreterParameters>(inputParameters);
        
        Transform3D HandleCamera(Vector2 cameraInput, Node3D camera, float xInput)
        {
            float roll = float.Lerp(camera.RotationDegrees.Z, -PlayerSettings.PlayerInput.HorizontalCameraTiltFp * xInput,
                GameManager.Delta * PlayerSettings.PlayerInput.HorizontalCameraTiltSpeedFp);
            cameraInput *= parameters.SensitivityMult;

            Transform3D transform = new Transform3D
            {
                Origin = interpolated + Interpreter.HeadPosition(),
                Basis = Interpreter.InputBasis(Mathf.Clamp(camera.RotationDegrees.X - cameraInput.Y, parameters.MaxPitchAngles.X, parameters.MaxPitchAngles.Y), camera.RotationDegrees.Y - cameraInput.X, roll)
            };
            
            return transform;
        }
        
        return HandleCamera(Interpreter.CalcCameraInputs(navigationInput.CameraInputs, true), player.Camera, navigationInput.MovementInputs.Movement.X);
    }
}

public class QuakeInterpreterParameters : InputInterpreterParameters
{
    public Vector2 SensitivityMult = Vector2.One;
    public Vector2 MaxPitchAngles = new Vector2(-90f, 90f);
}