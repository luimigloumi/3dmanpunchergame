using Godot;
using System;

public partial class PlayerCamera : Node3D
{

	[Export]
	public NodePath bodyPath;
	public Node3D body;
	[Export]
	public NodePath playerPath;
	public Player player;
	[Export]
	public bool lockInAir = true;

	[Export]
	public float verticalSensitivity = 0.85f;
	[Export]
	public float horizontalSensitivity = 1f;
	[Export]
	public float angleLimit = 80f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		body = GetNode<Node3D>(bodyPath);
		player = GetNode<Player>(playerPath);
		Input.MouseMode = Input.MouseModeEnum.Captured;

	}

    public override void _UnhandledInput(InputEvent @event)
    {

		if (@event is InputEventMouseMotion)
		{

			InputEventMouseMotion mouseEvent = @event as InputEventMouseMotion;

			RotateX(-mouseEvent.Relative.Y * verticalSensitivity * 0.01f);
			body.RotateY(-mouseEvent.Relative.X * horizontalSensitivity * 0.01f);

		}

    }

    public override void _Process(double delta)
    {

		if (player.IsOnFloor() || lockInAir) {

			RotationDegrees = new(Mathf.Lerp(RotationDegrees.X, Mathf.Clamp(RotationDegrees.X, -angleLimit, 90), 0.5f), RotationDegrees.Y, RotationDegrees.Z);

		}

		if (Input.IsActionJustPressed("Pause"))
		{

			Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;

		}

    }

}