using Godot;
using System;

public partial class Health : Area3D
{
	
	public void OnBodyEntered(Node3D body) {

		if (body is Player) {

			(body as Player).health = Mathf.Min((body as Player).maxHealth, (body as Player).health + 1);

			QueueFree();

		}

	}

}
