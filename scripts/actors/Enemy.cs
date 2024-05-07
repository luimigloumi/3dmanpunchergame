using Godot;
using System;

public partial class Enemy : Actor
{

	[Export]
	public NodePath navAgentPath;
	public NavigationAgent3D navAgent;

	public Player player;

	[Export]
	public float updateTime = 0.2f;

	public bool ready = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		base._Ready();

		player = (Player)GetTree().GetFirstNodeInGroup("Player");

		navAgent = GetNode<NavigationAgent3D>(navAgentPath);

		CallDeferred("UpdatePosition");

	}	

	public async void UpdatePosition() {

		while(true) {

			navAgent.TargetPosition = player.GlobalPosition;

			await ToSignal(GetTree().CreateTimer(updateTime), "timeout");

		}

	}

}
