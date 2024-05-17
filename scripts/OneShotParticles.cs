using Godot;
using System;

public partial class OneShotParticles : GpuParticles3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		Emitting=true;
		DestroyWhenDone(Lifetime);

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	async void DestroyWhenDone(double lifetime) 
	{

		await ToSignal(GetTree().CreateTimer(lifetime), "timeout");
		QueueFree();

	}

}
