using Godot;
using System;
using System.Linq.Expressions;

public partial class Actor : CharacterBody3D
{

	[Export] public NodePath stepCastPath;
	public ShapeCast3D stepCast;

	public float health = 5;
	[Export]
	public float maxHealth = 5;
	public bool isDead = false;

	public Vector3 gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle() * ProjectSettings.GetSetting("physics/3d/default_gravity_vector").AsVector3();

	public override void _Ready() {

		health = maxHealth;
		stepCast = GetNode<ShapeCast3D>(stepCastPath);

	}

	
    public override void _PhysicsProcess(double delta)
    {
		
		PhysicsDirectBodyState3D state = PhysicsServer3D.BodyGetDirectState(GetRid());
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

	public Vector3 StepStairs(Vector3 velocity, float delta) 
	{
		
		stepCast.Position = new Vector3(velocity.X, 0.3f, velocity.Z);
		stepCast.TargetPosition = new(0, -0.25f, 0);
		stepCast.ForceShapecastUpdate();
		if (stepCast.IsColliding() && stepCast.GetCollisionPoint(0).Y - GlobalPosition.Y <= 0.3f) {
			
			GlobalPosition = new(GlobalPosition.X, stepCast.GetCollisionPoint(0).Y, GlobalPosition.Z);
		}

		return velocity;

	}
	
}
