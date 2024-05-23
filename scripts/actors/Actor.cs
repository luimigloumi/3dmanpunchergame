using Godot;
using System;
using System.Linq.Expressions;

public partial class Actor : CharacterBody3D
{

	public float health = 5;
	[Export]
	public float maxHealth = 5;
	public bool isDead = false;

	[Export(PropertyHint.File)] public string hurtSound;

	public SoundManager sm;

	public Vector3 gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle() * ProjectSettings.GetSetting("physics/3d/default_gravity_vector").AsVector3();

	public override void _Ready() {

		health = maxHealth;
		sm = GetNode<SoundManager>("/root/SoundManager");

	}

	
	public override void _PhysicsProcess(double delta)
	{
		
		PhysicsDirectBodyState3D state = PhysicsServer3D.BodyGetDirectState(GetRid());
		gravity = state.TotalGravity;

	}

	public virtual void OnHit(float damage, Vector3 hitPoint, Vector3 hitNormal, Node3D source) 
	{

		health -= damage;
		sm.PlaySound(new Sound(hurtSound, 1, 0.75f + GD.Randf() * 0.5f, GlobalPosition));
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
	
}
