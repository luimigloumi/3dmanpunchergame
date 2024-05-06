using Godot;
using System;

public partial class Enemy : Actor
{

	[Export]
	public NodePath navAgentPath;
	public NavigationAgent3D navAgent;

	Player player;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		base._Ready();

		player = (Player)GetTree().GetFirstNodeInGroup("Player");

		navAgent = GetNode<NavigationAgent3D>(navAgentPath);

	}	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{

		base._PhysicsProcess(delta);

		navAgent.TargetPosition = player.GlobalPosition;

		Velocity = (GlobalPosition - navAgent.GetNextPathPosition()).Normalized() * 5;

	}
}
