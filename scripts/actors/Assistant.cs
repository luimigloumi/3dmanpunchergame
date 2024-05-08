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

	public override void _PhysicsProcess(double delta)
	{

		if (ready) {

			Vector3 velocity = Velocity;

			velocity.Y += gravity * (float)delta;

			Transform3D trans = GlobalTransform.LookingAt(new(player.GlobalPosition.X, GlobalPosition.Y, player.GlobalPosition.Z), Vector3.Up);

			Transform3D rotatedTrans = GlobalTransform;

			if (rotatedTrans.Basis.X != trans.Basis.X) rotatedTrans.Basis.X = rotatedTrans.Basis.X.Slerp(trans.Basis.X, 0.2f);
			if (rotatedTrans.Basis.Z != trans.Basis.Z) rotatedTrans.Basis.Z = rotatedTrans.Basis.Z.Slerp(trans.Basis.Z, 0.2f);

			GlobalTransform = rotatedTrans;
		
			switch (state) {

				case AssistantState.Attacking:

					if (GlobalPosition.DistanceTo(player.GlobalPosition) > desiredDistance) {

						state = AssistantState.Pursuing;
						break;

					}

					velocity = velocity.Lerp(new(0, velocity.Y, 0), 0.05f);

					break;

				case AssistantState.Pursuing:

					Vector3 direction = (navAgent.GetNextPathPosition() - GlobalPosition).Normalized();
					Vector3 flatDir = new Vector3(direction.X, 0, direction.Z).Normalized();

					velocity = velocity.Lerp(new(flatDir.X * speed, velocity.Y, flatDir.Z * speed), 0.05f);

					if (GlobalPosition.DistanceTo(player.GlobalPosition) <= desiredDistance) {

						state = AssistantState.Attacking;
						break;

					}
					break;

			}

			Velocity = velocity;

			MoveAndSlide();

		}

		ready = true;
	}

	public override void OnDeath() {

		QueueFree();

	}

}
