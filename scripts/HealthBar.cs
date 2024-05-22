using Godot;
using System;

public partial class HealthBar : ProgressBar
{

	[Export] public NodePath playerPath;
	public Actor player;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		player = GetNode<Actor>(playerPath);

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		Value = player.health;
		MaxValue = player.maxHealth;

	}
}
