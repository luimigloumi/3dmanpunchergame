using Godot;
using System;

public partial class Exlpod : Area3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		Explode();

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	async void Explode() {

		await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
		foreach (Node n in GetOverlappingBodies()) {
			
			if (n is Actor) {

				(n as Actor).OnHit(2, GlobalPosition, Vector3.Zero, null);
				(n as Actor).Velocity += GlobalPosition.DirectionTo((n as Actor).GlobalPosition) * 10;

			}

		}

	}

}
