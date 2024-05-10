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
	
		velocity.Y += gravity * (float)delta;

		CollisionMask = defaultMask;
		CollisionLayer = defaultLayer;
	
		switch (state) {

			case AssistantState.Attacking:

				Transform3D trans2 = GlobalTransform.LookingAt(new(player.GlobalPosition.X, GlobalPosition.Y, player.GlobalPosition.Z), Vector3.Up);

				Transform3D rotatedTrans2 = GlobalTransform;

				if (rotatedTrans2.Basis.X != trans2.Basis.X) rotatedTrans2.Basis.X = rotatedTrans2.Basis.X.Normalized().Slerp(trans2.Basis.X.Normalized(), 0.2f);
				if (rotatedTrans2.Basis.Z != trans2.Basis.Z) rotatedTrans2.Basis.Z = rotatedTrans2.Basis.Z.Normalized().Slerp(trans2.Basis.Z.Normalized(), 0.2f);

				GlobalTransform = rotatedTrans2;

				if (GlobalPosition.DistanceTo(player.GlobalPosition) > desiredDistance) {

					state = AssistantState.Pursuing;
					break;

				}

				velocity = velocity.Lerp(new(0, velocity.Y, 0), 0.05f);

				break;

			case AssistantState.Pursuing:

				if (!(Velocity.IsEqualApprox(Vector3.Zero))) 
				{
					Transform3D trans = GlobalTransform.LookingAt(GlobalPosition + new Vector3(Velocity.X, 0f, Velocity.Z), Vector3.Up);

					Transform3D rotatedTrans = GlobalTransform;

					if (rotatedTrans.Basis.X != trans.Basis.X) rotatedTrans.Basis.X = rotatedTrans.Basis.X.Slerp(trans.Basis.X, 0.2f);
					if (rotatedTrans.Basis.Z != trans.Basis.Z) rotatedTrans.Basis.Z = rotatedTrans.Basis.Z.Slerp(trans.Basis.Z, 0.2f);

					GlobalTransform = rotatedTrans;
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

