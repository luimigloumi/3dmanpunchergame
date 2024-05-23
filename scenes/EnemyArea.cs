using Godot;
using System;

public partial class EnemyArea : Area3D
{

	bool hasTriggered = false;
	[Export] public bool endGame = false;
	[Export] public PackedScene endScene;
	public Godot.Collections.Array<Enemy> enemies = new Godot.Collections.Array<Enemy>();
	public Godot.Collections.Array<EnemySpawner> spawners = new Godot.Collections.Array<EnemySpawner>();
	[Export]public Godot.Collections.Array<NodePath> doorPaths = new Godot.Collections.Array<NodePath>();
	public Godot.Collections.Array<StaticBody3D> doors = new Godot.Collections.Array<StaticBody3D>();
	public Godot.Collections.Array<Vector3> doorPositions = new Godot.Collections.Array<Vector3>();

	public void OnBodyEntered(Node3D body) {

		if (!hasTriggered) {
			foreach (Node n in GetChildren()) {
			
				if (n is EnemySpawner) {

					spawners.Add(n as EnemySpawner);

				}

			}

			foreach (EnemySpawner s	 in spawners) {
				
				Enemy e = s.enemy.Instantiate<Enemy>();
				enemies.Add(e);
				GetParent().AddChild(e);
				e.GlobalPosition = s.GlobalPosition;
				e.OnDeathSignal += (Enemy en) => enemies.Remove(en);
				s.QueueFree();

			}

			hasTriggered = true;
		}

	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		for (int i = 0; i < doorPaths.Count; i++) {
			
			doors.Add(GetNode<StaticBody3D>(doorPaths[i]));
			doorPositions.Add(doors[i].GlobalPosition);

		}

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		if (enemies.Count == 0) {
			
			if (!endGame || !hasTriggered) {

				foreach (StaticBody3D d in doors) {
				
				d.GlobalPosition = Vector3.Up * 1000;

				}

			} else if (hasTriggered) {

				GetTree().ChangeSceneToPacked(endScene);

			}

		} else {

			for (int i = 0; i < doors.Count; i++) {
				
				doors[i].GlobalPosition = doorPositions[i];

			}
		}

	}
}
