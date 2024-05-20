using Godot;
using System;

public enum EnemyState {

	Normal,
	Grabbed,
	Thrown

}

public partial class Enemy : Actor
{

	public bool alerted = false;

	[Export(PropertyHint.Layers3DPhysics)] uint losMask = 0;
	[Export] public NodePath headPath;

	public Node3D head;

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
		head = GetNode<Node3D>(headPath);
		defaultMask = CollisionMask;
		defaultLayer = CollisionLayer;

		navAgent = GetNode<NavigationAgent3D>(navAgentPath);

		CallDeferred("UpdatePosition");

	}	

	public override void _PhysicsProcess(double delta) {

		if (ready && alerted) {

			Vector3 velocity = Velocity;

			switch(currentState) {

				case EnemyState.Normal:
					velocity = NormalPhysicsProcess((float)delta, velocity);
				break;

				case EnemyState.Grabbed:
					velocity = GrabbedPhysicsProcess((float)delta, velocity);
				break;

				case EnemyState.Thrown:
					velocity = ThrownPhysicsProcess((float)delta, velocity);
				break;

			}

			Velocity = velocity;

			base._PhysicsProcess((float)delta);

			MoveAndSlide();

		}

		if (LineOfSight(player.GlobalPosition)) {

			alerted = true;

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
		velocity += gravity * (float)delta;

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

	public virtual bool LineOfSight(Vector3 point) {

		PhysicsDirectSpaceState3D state = GetWorld3D().DirectSpaceState;
		PhysicsRayQueryParameters3D parameters = new();
		parameters.From = head.GlobalPosition;
		parameters.To = point;
		parameters.CollisionMask = losMask;
		parameters.HitFromInside = false;
		Godot.Collections.Dictionary result = state.IntersectRay(parameters);
		return !(result.Count > 0);
	}

}
