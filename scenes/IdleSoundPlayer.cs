using Godot;
using System;

public partial class IdleSoundPlayer : AudioStreamPlayer3D
{

	[Export] public float lowDelay = 1f;
	[Export] public float highDelay = 5f;
	[Export] public Godot.Collections.Array<AudioStream> sounds = new();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		PlaySounds();

	}

	async void PlaySounds() {

		while(true) {

			await ToSignal(GetTree().CreateTimer(GD.RandRange(lowDelay, highDelay)), "timeout");

			Stream = sounds[GD.RandRange(0, sounds.Count - 1)];
			Play(0);

		}

	}

}
