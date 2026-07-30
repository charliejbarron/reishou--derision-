using Godot;

[GlobalClass]
public partial class MovementManager : Node
{
	CharacterBody3D _player;
	
	Camera3D _camera;
	Node3D _cameraMarker;
	
	MovementInterpreter _movementInterpreter;
	CameraInterpreter _cameraInterpreter;

	public override void _Ready()
	{
		_cameraInterpreter = new FirstPerson();
		_movementInterpreter = new MovementInterpreter();

		_player = ResourceLoader.Load<PackedScene>("res://Scenes/player.tscn").Instantiate() as CharacterBody3D;
		_camera = ResourceLoader.Load<PackedScene>("res://Scenes/camera.tscn").Instantiate() as Camera3D;
		
		AddChild(_player);
		AddChild(_camera);

		_cameraMarker = _camera?.GetNode<Node3D>("Marker");
	}

	public void HandleNavigation(NavigationInput navigationInputs)
	{
		_movementInterpreter.Handle(navigationInputs.MovementInputs, _player);
		_cameraInterpreter.Handle(navigationInputs.CameraInputs, _cameraMarker, _player.GlobalPosition);
	}

	public override void _Process(double delta)
	{
		_camera.GlobalTransform = _cameraMarker.GetGlobalTransformInterpolated();
	}
}
