using Godot;
using System;
using System.Linq.Expressions;

public partial class Actor : RigidBody3D
{
	
	[Export] public NodePath groundedCastPath;
	public ShapeCast3D groundedCast;

	public float health = 5;
	[Export]
	public float maxHealth = 5;
	public bool isDead = false;

	public Vector3 gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle() * ProjectSettings.GetSetting("physics/3d/default_gravity_vector").AsVector3();

	public override void _Ready() {

		LockRotation = true;

		health = maxHealth;
		groundedCast = GetNode<ShapeCast3D>(groundedCastPath);

	}

	
	public override void _IntegrateForces(PhysicsDirectBodyState3D state)
	{
		
		gravity = state.TotalGravity;

	}

	public virtual void OnHit(float damage, Vector3 hitPoint, Vector3 hitNormal, Node3D source) 
	{

		health -= damage;
		if (health <= 0 && !isDead) 
		{
			OnDeath();
		}

	}

	public virtual void OnDeath() 
	{

		isDead = true;

		//This is where Death code would go.
		//IF I HAD SOME.
		
		//meowmeowmeowmeowmeowmomeowmeowmeowmeowmeowmeo >w<
	}

	public bool IsOnFloor() {

		return groundedCast.IsColliding();

	}
	
}
