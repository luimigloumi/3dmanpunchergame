using Godot;
using System;

public enum AssistantState {

	Attacking, Healing, Pursuing, Frenzied

}

public partial class Assistant : Enemy
{

	[Export] public float speed = 7f;
	[Export] public float desiredDistance = 10f;

	public AssistantState state = AssistantState.Attacking;

	public override Vector3 NormalPhysicsProcess(double delta, Vector3 velocity) {
	
		velocity += gravity * (float)delta;

		CollisionMask = defaultMask;
		CollisionLayer = defaultLayer;
	
		switch (state) {

			case AssistantState.Attacking:

				LookAt(new(player.GlobalPosition.X, GlobalPosition.Y, player.GlobalPosition.Z), Vector3.Up);

				if (GlobalPosition.DistanceTo(player.GlobalPosition) > desiredDistance) {

					state = AssistantState.Pursuing;
					break;

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

				if (GlobalPosition.DistanceTo(player.GlobalPosition) <= desiredDistance) {

					state = AssistantState.Attacking;
					break;

				}
			break;

		}

		return velocity;

	}

	public override void OnDeath() {

		QueueFree();

	}

}

