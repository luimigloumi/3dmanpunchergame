using Godot;
using System;
using System.ComponentModel;
using System.Net.Mail;


public partial class Drone : Enemy
{

	[Export(PropertyHint.File, "*.tscn")] public string projectileScene;
    [Export(PropertyHint.File, "*.tscn")] public string explodeScene;

    [Export(PropertyHint.File)] public string grabSound;

	[Export] public float speed = 4f;
	[Export] public float desiredDistance = 20f;

    [Export] public float maxAttackDelay = 1;

    [Export] public float tickTockTime = 0.5f;

    [Export] public NodePath meshPath;
    public MeshInstance3D mesh;

    bool exploding = false;

	float attackDelay = 0;


    public override void _Ready() {

        base._Ready();

        mesh = GetNode<MeshInstance3D>(meshPath);

    }

    public override Vector3 ThrownPhysicsProcess(double delta, Vector3 velocity)
	{

        exploding = false;

		CollisionMask = 0;
		CollisionLayer = 0;

		if (projectileCast.IsColliding()) 
		{

			PackedScene e = GD.Load<PackedScene>(explodeScene);
			Node3D ex = e.Instantiate<Node3D>();
			GetParent().AddChild(ex);
			ex.GlobalPosition = head.GlobalPosition;
            EmitSignal(SignalName.OnDeathSignal, this);
            QueueFree();

		}

		projectileCast.Enabled = true;

		return velocity;

	}

    public override Vector3 GrabbedPhysicsProcess(double delta, Vector3 velocity)
	{
		
		CollisionMask = 0;
		CollisionLayer = 0;
		
		velocity = Vector3.Zero;

		LookAt(new(player.GlobalPosition.X, GlobalPosition.Y, player.GlobalPosition.Z), Vector3.Up);

		GlobalPosition = player.grabPoint.GlobalPosition - grabPoint.Position;

        if (!exploding) {
            
            exploding = true;
            ExplodeTimer();
            sm.PlayDirectionlessSound(new Sound(grabSound, 1, 1, GlobalPosition));

        }

		return velocity;


	}

	public override Vector3 NormalPhysicsProcess(double delta, Vector3 velocity) {

		CollisionMask = defaultMask;
		CollisionLayer = defaultLayer;

        attackDelay = Math.Max(0, attackDelay - (float)delta);
	
        if (LineOfSight(player.GlobalPosition) && GlobalPosition.DistanceTo(player.GlobalPosition) > desiredDistance) {
            
            LookAt(player.GlobalPosition);
            velocity += GlobalPosition.DirectionTo(player.GlobalPosition) * (float)speed * (float)delta;

        } else if (LineOfSight(player.GlobalPosition)) {

            LookAt(player.GlobalPosition);
            velocity = velocity.Lerp(Vector3.Zero, 0.01f);

            if (attackDelay <= 0) {

                attackDelay = maxAttackDelay;
                PackedScene p = GD.Load<PackedScene>(projectileScene);
				BitchassOrb proj = p.Instantiate<BitchassOrb>();
				GetParent().AddChild(proj);
				proj.Velocity = GlobalPosition.DirectionTo(player.GlobalPosition) * proj.speed * 2;
				proj.GlobalPosition = head.GlobalPosition;

            }

        }

		return velocity;

	}

    public async void ExplodeTimer() {

        Tween t = GetTree().CreateTween();
        t.TweenProperty(mesh, "mesh:surface_0/material:albedo_color", new Color(1, 0, 0, 1), tickTockTime);

        await ToSignal(GetTree().CreateTimer(tickTockTime), "timeout");
        if (exploding) {
            
            player.OnHit(2, GlobalPosition, player.GlobalPosition - GlobalPosition, this);
            player.heldEnemy = null;
            player.vmState = VMState.Idle;
            OnDeath();

        }

    }

}

