using Godot;
using System;
using System.ComponentModel;

public enum AssistantState {

	Attacking, Stunned, Pursuing, Frenzied

}

public partial class Assistant : Enemy
{

	[Export] public NodePath sparkleSoundPath;
	public AudioStreamPlayer3D sparkleSound;

	[Export(PropertyHint.File, "*.tscn")] public string projectileScene;

	[Export] public NodePath sparksPath;
	public GpuParticles3D sparks;

	[Export] public float speed = 7f;
	[Export] public float desiredDistance = 10f;

	bool canAttack = true;

	public AssistantState state = AssistantState.Attacking;


	public override void _Ready() {

		base._Ready();

		sparks = GetNode<GpuParticles3D>(sparksPath);
		sparkleSound = GetNode<AudioStreamPlayer3D>(sparkleSoundPath);

	}

	public override Vector3 NormalPhysicsProcess(double delta, Vector3 velocity) {
	
		velocity += gravity * (float)delta;

		CollisionMask = defaultMask;
		CollisionLayer = defaultLayer;
	
		switch (state) {

			case AssistantState.Attacking:

				LookAt(new(player.GlobalPosition.X, GlobalPosition.Y, player.GlobalPosition.Z), Vector3.Up);

				if (GlobalPosition.DistanceTo(player.GlobalPosition) > desiredDistance || !LineOfSight(player.GlobalPosition)) {

					state = AssistantState.Pursuing;
					break;

				} else {

					if (canAttack) 
					{

						canAttack = false;
						LaserBarrage();

					}

				}

				velocity = velocity.Lerp(new(0, velocity.Y, 0), 0.05f);

				break;

			case AssistantState.Pursuing:

				if (!(new Vector3(velocity.X, 0, velocity.Z).IsEqualApprox(Vector3.Zero))) 
				{
					LookAt(GlobalPosition + new Vector3(velocity.X, 0, velocity.Z), Vector3.Up);
				}

				Vector3 direction = (navAgent.GetNextPathPosition() - GlobalPosition).Normalized();
				Vector3 flatDir = new Vector3(direction.X, 0, direction.Z).Normalized();

				velocity = velocity.Lerp(new(flatDir.X * speed, velocity.Y, flatDir.Z * speed), 0.05f);

				if (GlobalPosition.DistanceTo(player.GlobalPosition) <= desiredDistance && LineOfSight(player.GlobalPosition)) {

					state = AssistantState.Attacking;
					break;

				}
			break;

		}

		return velocity;

	}
	
	public async void LaserBarrage() {

		electrified = true;
		sparks.Emitting = true;

		sparkleSound.Play();

		for(int i = 0; i < 5; i++) {
			
			await ToSignal(GetTree().CreateTimer(0.4), "timeout");
			if (state == AssistantState.Attacking) {

				PackedScene p = GD.Load<PackedScene>(projectileScene);
				BitchassOrb proj = p.Instantiate<BitchassOrb>();
				GetParent().AddChild(proj);
				proj.Velocity = GlobalPosition.DirectionTo(player.GlobalPosition) * proj.speed;
				proj.GlobalPosition = head.GlobalPosition;

			} else {

				break;

			}

		}

		sparks.Emitting = false;
		electrified = false;

		sparkleSound.Stop();

		await ToSignal(GetTree().CreateTimer(3), "timeout");

		canAttack = true;

	}

}

