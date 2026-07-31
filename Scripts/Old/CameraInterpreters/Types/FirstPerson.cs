using Godot;

[GlobalClass]
public partial class FirstPerson : CameraInterpreter
{
    public override void Handle(Vector2 cameraInputs, Node3D camera, Vector3 playerPos)
    {
        camera.GlobalPosition = playerPos + new Vector3(0, 1.57f, 0);

        cameraInputs *= PlayerSettings.PlayerInputSetting.SensitivityReductionFp;
        camera.RotationDegrees = new Vector3(Mathf.Clamp(camera.RotationDegrees.X - cameraInputs.Y, -90f, 90f), camera.RotationDegrees.Y - cameraInputs.X, 0);
    }
}
