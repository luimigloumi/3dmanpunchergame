using Godot;
using System;

public enum EnemyState {

	Normal,
	Grabbed,
	Thrown

}

public partial class Enemy : Actor
{

	[Export]
	public NodePath navAgentPath;
	public NavigationAgent3D navAgent;

	[Export] public NodePath projectileCastPath;
	public ShapeCast3D projectileCast;

	[Export] public NodePath grabPointPath;
	public Node3D grabPoint;

	public EnemyState currentState = EnemyState.Normal;

	public Player player;

	[Export] public float throwDamage = 3f;

	[Export]
	public float updateTime = 0.2f;

	public bool ready = false;

	public uint defaultMask;
	public uint defaultLayer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		base._Ready();

		player = (Player)GetTree().GetFirstNodeInGroup("Player");
		projectileCast = GetNode<ShapeCast3D>(projectileCastPath);
		grabPoint = GetNode<Node3D>(grabPointPath);

		grabPoint = GetNode<Node3D>(grabPointPath);
		defaultMask = CollisionMask;
		defaultLayer = CollisionLayer;

		navAgent = GetNode<NavigationAgent3D>(navAgentPath);

		CallDeferred("UpdatePosition");

	}	

	public override void _PhysicsProcess(double delta) {

		if (ready) {

			Vector3 velocity = Velocity;

			switch(currentState) {

				case EnemyState.Normal:
					velocity = NormalPhysicsProcess(delta, velocity);
				break;

				case EnemyState.Grabbed:
					velocity = GrabbedPhysicsProcess(delta, velocity);
				break;

				case EnemyState.Thrown:
					velocity = ThrownPhysicsProcess(delta, velocity);
				break;

			}

			Velocity = velocity;

			MoveAndSlide();

			base._PhysicsProcess(delta);

		}

		ready = true;

	}

	public async void UpdatePosition() {

		while(true) {

			navAgent.TargetPosition = player.GlobalPosition;

			await ToSignal(GetTree().CreateTimer(updateTime), "timeout");

		}

	}

	public virtual Vector3 NormalPhysicsProcess(double delta, Vector3 velocity)
	{
		
		CollisionMask = defaultMask;
		CollisionLayer = defaultLayer;

		return velocity;

	}

	public virtual Vector3 GrabbedPhysicsProcess(double delta, Vector3 velocity)
	{
		
		CollisionMask = 0;
		CollisionLayer = 0;
		
		velocity = Vector3.Zero;

		LookAt(new(player.GlobalPosition.X, GlobalPosition.Y, player.GlobalPosition.Z), Vector3.Up);

		GlobalPosition = player.grabPoint.GlobalPosition - grabPoint.Position;

		return velocity;


	}

	public virtual Vector3 ThrownPhysicsProcess(double delta, Vector3 velocity)
	{

		CollisionMask = 0;
		CollisionLayer = 0;

		velocity = velocity.Lerp(new(0, velocity.Y, 0), 0.005f);
		velocity.Y += gravity * (float)delta;

		if (projectileCast.IsColliding()) 
		{

			if (projectileCast.GetCollider(0) is Enemy) {

				Enemy en = projectileCast.GetCollider(0) as Enemy;
				en.OnHit(throwDamage, projectileCast.GetCollisionPoint(0), projectileCast.GetCollisionNormal(0), this);

			}

			OnHit(1, projectileCast.GetCollisionPoint(0), -projectileCast.GetCollisionNormal(0), this);

			currentState = EnemyState.Normal;

		}

		projectileCast.Enabled = true;

		return velocity;

	}

	public virtual void OnProjectileHit(Node3D col) 
	{

	}

}
