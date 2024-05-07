using Godot;
using System;

public partial class Enemy : Actor
{

	[Export]
	public NodePath navAgentPath;
	public NavigationAgent3D navAgent;

	Player player;

	[Export]
	public float updateTime = 1f;

	[Export]
	public float speed = 50f;

	bool ready = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		base._Ready();

		player = (Player)GetTree().GetFirstNodeInGroup("Player");

		navAgent = GetNode<NavigationAgent3D>(navAgentPath);

		CallDeferred("UpdatePosition");

	}	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{

		if (ready) {

			Velocity = (navAgent.GetNextPathPosition() - GlobalPosition).Normalized() * speed;

		}

		ready = true;

		MoveAndSlide();

	}

	public async void UpdatePosition() {

		while(true) {

			navAgent.TargetPosition = player.GlobalPosition;

			await ToSignal(GetTree().CreateTimer(updateTime), "timeout");

		}

	}

}
