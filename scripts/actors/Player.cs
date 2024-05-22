using Godot;
using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

public enum PlayerState
{
	Idle,
	Walking,
	Jumping,
	Falling
}

public enum VMState {

	Idle, 
	Punching, 
	PunchingSecond,
	PunchingFinish,
	Grabbing,
	Holding,
	Throwing,
	Charging,
	ChargePunching

}

public partial class Player : Actor
{

	#region References

	[ExportCategory("References")]

	[Export(PropertyHint.File, "*.tscn")] public string deathScenePath;

	[Export] public NodePath screenOverlayPath;
	public TextureRect screenOverlay;

	public SoundManager sm;

	[Export] public NodePath cameraPath;
	public Camera3D camera;

	[Export]
	public NodePath vmAnimatorPath;
	public AnimationPlayer vmAnimator;

	[Export] public NodePath grabPointPath;
	public Node3D grabPoint;

	[Export]
	public NodePath punchCastPath;
	public ShapeCast3D punchCast;
	[Export]
	public NodePath chargePunchCastPath;
	public ShapeCast3D chargePunchCast;
	[Export] public NodePath vmPath;
	public MeshInstance3D vm;

	[Export] public PackedScene smackScene;

	[ExportCategory("Sounds")]

	[Export(PropertyHint.File)] public string punchSound;
	[Export(PropertyHint.File)] public string jumpSound;
	[Export(PropertyHint.File)] public string whooshSound;
	[Export(PropertyHint.File)] public string chargePunchSound;
	[Export(PropertyHint.File)] public string grabSound;
	[Export(PropertyHint.File)] public string throwSound;
	[Export(PropertyHint.File)] public string hurtSound;
	

	#endregion

	#region Variables

	[Export] public float maxInvincibility = 1f;
	public float invincibility = 0f;

	[Export]
	public PlayerState currentState = PlayerState.Idle;
	
	public VMState vmState = VMState.Idle;

	[ExportCategory("Movement")]
	[ExportGroup("Horizontal Movement")]

	[Export]
	public float movementSpeed = 20f;
	[Export]
	public float groundTraction = 1f;
	[Export]
	public float airTraction = 0.3f;
	[Export]
	public float highSpeedTractionMultiplier = 0.1f;

	[ExportGroup("Vertical Movement")]

	[ExportSubgroup("Jumping")]
	[Export]
	public int maxJumps = 2;
	int jumps = 2;
	[Export]
	public float jumpHeight = 50f;
	[Export]
	public float maximumCoyoteTime = 0.2f;
	[Export]
	public float maximumJumpBuffer = 0.2f;
	float coyoteTime = 0f;
	float jumpBuffer = 0f;

	[ExportGroup("Combat")]
	[ExportSubgroup("Punch")]

	[Export]
	public Godot.Collections.Array<string> prioritizedVMAnimations = new Godot.Collections.Array<string>();

	[Export]
	public float maxPunchBuffer = 0.2f;
	public float punchBuffer = 0f;

	[Export]
	public float punchTimeMaximum = 0.2f;
	public float punchTimer = 0;
	[Export]
	public float punchDamage = 1f;

	[ExportSubgroup("Grab")]

	[Export]
	public float maxGrabBuffer = 0.2f;
	public float grabBuffer = 0f;
	[Export]
	public float grabTimeMaximum = 0.5f;
	public float grabTimer = 0;

	public Enemy heldEnemy;

	[ExportSubgroup("ChargePunch")]
	[Export] public float chargePunchDamage = 5;
	[Export] public float chargePunchKickback = 5;
	[Export] public float chargePunchTime = 1f;
	public float chargePunchTimer = 0;

	#endregion

	public override void _Ready()
	{

		base._Ready();

		vmAnimator = GetNode<AnimationPlayer>(vmAnimatorPath);
		punchCast = GetNode<ShapeCast3D>(punchCastPath);
		grabPoint = GetNode<Node3D>(grabPointPath);
		camera = GetNode<Camera3D>(cameraPath);
		chargePunchCast = GetNode<ShapeCast3D>(chargePunchCastPath);
		vm = GetNode<MeshInstance3D>(vmPath);
		screenOverlay = GetNode<TextureRect>(screenOverlayPath);
		sm = GetNode<SoundManager>("/root/SoundManager");

		coyoteTime = 0f;
		jumpBuffer = 0f;
		grabBuffer = 0f;
		punchBuffer = 0f;
		Velocity = Vector3.Zero;

	}

	public override void _Process(double delta)
	{

		coyoteTime = Mathf.Max(0f, coyoteTime - (float)delta);
		jumpBuffer = Mathf.Max(0f, jumpBuffer - (float)delta);
		punchBuffer = Mathf.Max(0f, punchBuffer - (float)delta);
		punchTimer = Mathf.Max(0f, punchTimer - (float)delta);
		grabBuffer = Mathf.Max(0f, grabBuffer - (float)delta);
		grabTimer = Mathf.Max(0f, grabTimer - (float)delta);
		invincibility = Mathf.Max(0f, invincibility - (float)delta);

		if (Input.IsActionJustPressed("Jump")) jumpBuffer = maximumJumpBuffer;

		if (Input.IsActionJustPressed("Attack")) punchBuffer = maxPunchBuffer;

		if (Input.IsActionJustPressed("Grab")) grabBuffer = maxGrabBuffer;

		for (int i = 0; i < vm.Mesh.GetSurfaceCount(); i++) {

			StandardMaterial3D mat = (StandardMaterial3D)vm.Mesh.SurfaceGetMaterial(i);

			mat.AlbedoColor = new(mat.AlbedoColor.R, 1 - chargePunchTimer / chargePunchTime, 1 - chargePunchTimer / chargePunchTime);

			vm.Mesh.SurfaceSetMaterial(i, mat);

		}

		switch(vmState) {

			case VMState.Idle:

				if (!vmAnimator.IsPlaying() || !prioritizedVMAnimations.Contains(vmAnimator.CurrentAnimation)) {

					vmAnimator.Play("Idle");

				}

				if (Input.IsActionJustPressed("Attack")) {

					vmState = VMState.Charging;
					vmAnimator.Play("PunchChargeup");

				}

				if (grabBuffer > 0) { 

					grabBuffer = 0;
					vmState = VMState.Grabbing;
					sm.PlayDirectionlessSound(new Sound(throwSound, 1, 0.5f + GD.Randf(), Vector3.Zero));
					vmAnimator.Play("Grab");
					vmAnimator.Seek(0);
					grabTimer = grabTimeMaximum;
					break;

				}

			break;

			case VMState.Punching:

				if (punchTimer <= 0) {
					
					vmState = VMState.Idle;
					if (punchBuffer > 0) {

						Punch(1);

					}
					
				}

			break;
			case VMState.PunchingSecond:

				if (punchTimer <= 0) {
					
					vmState = VMState.Idle;
					if (punchBuffer > 0) {

						Punch(2);

					}
					
				}

			break;

			case VMState.PunchingFinish:

				if (punchTimer <= 0) {
					
					vmState = VMState.Idle;
					
				}

			break;

			case VMState.Grabbing:	

				if (grabTimer <= 0) {
					
					vmState = VMState.Idle;

				}

				if (punchCast.IsColliding()) {

					Enemy en = punchCast.GetCollider(0) as Enemy;
					
					if (en.electrified) {

						OnHit(1, punchCast.GetCollisionPoint(0), -punchCast.GetCollisionNormal(0), en);
						

					} else {

						sm.PlayDirectionlessSound(new Sound(grabSound, 1, 0.5f + GD.Randf(), Vector3.Zero));

						en.currentState = EnemyState.Grabbed;

						vmState = VMState.Holding;

						heldEnemy = en;

					}

				}

			break;

			case VMState.Holding:

				if (!vmAnimator.IsPlaying() || !prioritizedVMAnimations.Contains(vmAnimator.CurrentAnimation)) {

					vmAnimator.Play("Idle");

				}

				if (punchBuffer > 0 || grabBuffer > 0) {

					sm.PlayDirectionlessSound(new Sound(throwSound, 1, 0.5f + GD.Randf(), Vector3.Zero));

					punchBuffer = 0;
					grabBuffer = 0;	
					
					heldEnemy.Velocity = -camera.GlobalTransform.Basis.Z * 20f + Velocity;
					heldEnemy.GlobalPosition = grabPoint.GlobalPosition - Vector3.Up * 0.5f;
					heldEnemy.currentState = EnemyState.Thrown;
					heldEnemy.projectileCast.Enabled = false;

					heldEnemy = null;

					vmAnimator.Play("Grab");

					vmState = VMState.Idle;

				}

			break;

			case VMState.Charging:

				if (!vmAnimator.IsPlaying() || !prioritizedVMAnimations.Contains(vmAnimator.CurrentAnimation)) {

					vmAnimator.Play("PunchChargeupLoop");

				}

				Vector3 velocity = Velocity;

				chargePunchTimer = Mathf.Min(chargePunchTime, chargePunchTimer + (float)delta);

				if (!Input.IsActionPressed("Attack")) {

					if (chargePunchTimer >= chargePunchTime) {
						
						velocity = ChargePunch(velocity);

					} else {

						Punch(0);
						chargePunchTimer = 0;

					}

				}

				Velocity = velocity;

			break;

			case VMState.ChargePunching:

				if (punchTimer <= 0) {
					
					vmState = VMState.Idle;
					
				}

			break;

		}

	}

	public override void _PhysicsProcess(double delta)
	{

		base._PhysicsProcess(delta);
		
		Vector3 velocity = Velocity;
		Vector2 inputVector = Input.GetVector("Left", "Right", "Forward", "Back").Normalized();

		Vector3 movementDirection = inputVector.X * Transform.Basis.X + inputVector.Y * Transform.Basis.Z;

		velocity += gravity * (float)delta;

		switch(currentState) {

			case PlayerState.Idle:
			{

				jumps = maxJumps;

				if (IsOnFloor()) {

					coyoteTime = maximumCoyoteTime;

				}

				if (coyoteTime <= 0f) {

					currentState = PlayerState.Falling;

				} else {

					Vector3 flatVel = new(velocity.X, 0, velocity.Z);

					if (jumpBuffer > 0f) {

						jumps--;
						velocity.Y = jumpHeight;
						jumpBuffer = 0f;
						coyoteTime = 0f;
						currentState = PlayerState.Jumping;
						sm.PlayDirectionlessSound(new Sound(jumpSound, 1, 0.5f + GD.Randf(), Vector3.Zero));

					} else if (inputVector != Vector2.Zero) {

						currentState = PlayerState.Walking;

					} else {

						velocity = velocity.Lerp(new(0, velocity.Y, 0), 0.1f * groundTraction * (flatVel.Length() > movementSpeed * 1.5f ? highSpeedTractionMultiplier : 1));

					}

				}				
				
				break;
			}
			case PlayerState.Walking:
			{

				jumps = maxJumps;

				if (IsOnFloor()) coyoteTime = maximumCoyoteTime;

				if (coyoteTime <= 0f) {

					currentState = PlayerState.Falling;

				} else {

					Vector3 flatVel = new(velocity.X, 0, velocity.Z);

					if (jumpBuffer > 0f) {

						jumps--;
						velocity.Y = jumpHeight;
						jumpBuffer = 0f;
						coyoteTime = 0f;
						currentState = PlayerState.Jumping;
						sm.PlayDirectionlessSound(new Sound(jumpSound, 1, 0.5f + GD.Randf(), Vector3.Zero));
					
					} else if (inputVector == Vector2.Zero) {

						currentState = PlayerState.Idle;

					} else {

						velocity = velocity.Lerp(new(movementDirection.X * movementSpeed, velocity.Y, movementDirection.Z * movementSpeed), 0.1f * groundTraction * (flatVel.Length() > movementSpeed * 1.5f ? highSpeedTractionMultiplier : 1));

					}

				} 

				break;
			}
			case PlayerState.Jumping:
			{

				if (IsOnFloor()) {

					currentState = PlayerState.Idle;

				} else {

					Vector3 flatVel = new(velocity.X, 0, velocity.Z);
					if (jumps > 0 && jumpBuffer > 0f) {
						
						jumps--;
						Vector3 maxVel = movementDirection.Normalized() * Mathf.Max(flatVel.Length(), movementSpeed);
						velocity = new(maxVel.X, jumpHeight * 0.7f, maxVel.Z);
						jumpBuffer = 0f;
						currentState = PlayerState.Jumping;

					};

					velocity = velocity.Lerp(new(movementDirection.X * movementSpeed, velocity.Y, movementDirection.Z * movementSpeed), 0.1f * airTraction * (flatVel.Length() > movementSpeed * 1.5f ? highSpeedTractionMultiplier : 1));

					if (velocity.Y <= 0f)
					{

						currentState = PlayerState.Falling;

					} else if (Input.IsActionJustReleased("Jump")) {


						velocity.Y *= 0.5f;
						currentState = PlayerState.Falling;

					}

				}

				break;
			}
			case PlayerState.Falling:
			{

				if (IsOnFloor()) {

					currentState = PlayerState.Idle;

				} else {

					Vector3 flatVel = new(velocity.X, 0, velocity.Z);
					if (jumps > 0 && jumpBuffer > 0f) {
						
						jumps--;
						Vector3 maxVel = movementDirection.Normalized() * Mathf.Max(flatVel.Length(), movementSpeed);
						velocity = new(maxVel.X, jumpHeight * 0.7f, maxVel.Z);
						jumpBuffer = 0f;
						currentState = PlayerState.Jumping;

					};

					velocity = velocity.Lerp(new(movementDirection.X * movementSpeed, velocity.Y, movementDirection.Z * movementSpeed), 0.1f * airTraction * (flatVel.Length() > movementSpeed * 1.5f ? highSpeedTractionMultiplier : 1));

				}

				break;
			}

			default:
			{
				GD.PrintErr("Invalid player state!");
				currentState = PlayerState.Idle;
				break;
			}

		}

		Velocity = velocity;

		MoveAndSlide();

	}

	public Vector3 ChargePunch(Vector3 velocity) {

		chargePunchTimer = 0;
		vmState = VMState.ChargePunching;
		vmAnimator.Play("ChargedPunch");
		if (chargePunchCast.IsColliding()) {

			for (int i = 0; i < chargePunchCast.GetCollisionCount(); i++) 
			{
				if (chargePunchCast.GetCollider(i) is Actor) {
					
					Actor en = chargePunchCast.GetCollider(i) as Actor;
					en.OnHit(chargePunchDamage, chargePunchCast.GetCollisionPoint(i), chargePunchCast.GetCollisionNormal(i), this);

				} else {

					velocity = camera.Transform.Basis.Z * chargePunchKickback;

				}

				OneShotParticles p = smackScene.Instantiate<OneShotParticles>();
				GetParent().AddChild(p);
				p.GlobalPosition = punchCast.GetCollisionPoint(i);

			}

			sm.PlayDirectionlessSound(new Sound(chargePunchSound, 1, 0.5f + GD.Randf(), Vector3.Zero));

		} else {

			sm.PlayDirectionlessSound(new Sound(whooshSound, 1, 0.5f + GD.Randf(), Vector3.Zero));

		}
		punchTimer = punchTimeMaximum * 3;
		return velocity;

	}

	void Punch(int number) {

		punchBuffer = 0;
		switch (number) {
			
			case 0:
			
				vmState = VMState.Punching;
				vmAnimator.Play("Punch");

			break;
			case 1:
			
				vmState = VMState.PunchingSecond;
				vmAnimator.Play("Punch2");
				
			break;

			case 2:
			
				vmState = VMState.PunchingFinish;
				vmAnimator.Play("Punch3");
			
			break;

		}
		vmAnimator.Seek(0);
		punchTimer = number == 2 ? punchTimeMaximum * 3f : punchTimeMaximum;
		if (punchCast.IsColliding()) {

			sm.PlayDirectionlessSound(new Sound(punchSound, 1, 0.5f + GD.Randf(), Vector3.Zero));

			for (int i = 0; i < punchCast.GetCollisionCount(); i++) {

				Actor en = punchCast.GetCollider(i) as Actor;
				OneShotParticles p = smackScene.Instantiate<OneShotParticles>();
				GetParent().AddChild(p);
				p.GlobalPosition = punchCast.GetCollisionPoint(i);
				en.OnHit(punchDamage, punchCast.GetCollisionPoint(0), punchCast.GetCollisionNormal(0), this);

			}

		} else {

			sm.PlayDirectionlessSound(new Sound(whooshSound, 1, 0.5f + GD.Randf(), Vector3.Zero));

		}

	}

	public override void OnHit(float damage, Vector3 hitPoint, Vector3 hitNormal, Node3D source)
	{
		if (invincibility <= 0) 
		{

			base.OnHit(damage, hitPoint, hitNormal, source);
			HitFlash();
			invincibility = maxInvincibility;
			sm.PlayDirectionlessSound(new Sound(hurtSound, 1, 0.75f + GD.Randf() * 0.5f, Vector3.Zero));

		}
	}

	async void HitFlash() {

		screenOverlay.Modulate = new(1,0,0,0.5f);

		Tween twee = GetTree().CreateTween();
		twee.TweenProperty(screenOverlay, "modulate", new Color(0,1,1,0.5f), 0.5f);

		await ToSignal(GetTree().CreateTimer(maxInvincibility), "timeout");

		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(screenOverlay, "modulate", new Color(0,1,1,0), 0.5f);

	}

	public override void OnDeath()
	{
		GetTree().ChangeSceneToFile(deathScenePath);
	}

}
