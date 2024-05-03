using Godot;
using System;

public partial class CameraFollow : Camera3D
{

	[Export]
	public NodePath targetPath;
	[Export]
	public float lerpValue = 0.3f;

	public Node3D target;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		target = GetNode<Node3D>(targetPath);

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		GlobalRotation = target.GlobalRotation;

		GlobalPosition = new(target.GlobalPosition.X, Mathf.Lerp(GlobalPosition.Y, target.GlobalPosition.Y, lerpValue), target.GlobalPosition.Z);

	}
}
