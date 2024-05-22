using Godot;
using System;

public partial class BitchassOrb : CharacterBody3D
{

    [Export] public float damage = 1f;
    [Export] public float speed = 6f;
    [Export] public float maxTime = 15;

    public override void _Ready()
    {
    
        Kaboom();

    }

    public override void _PhysicsProcess(double delta)
    {

        var col = MoveAndCollide(Velocity * (float)delta);
        if (col != null && col.GetCollider(0) is Player) {

            ((Player)col.GetCollider(0)).OnHit(damage, GlobalPosition, ((Player)col.GetCollider(0)).GlobalPosition - GlobalPosition, this);
            QueueFree();
        
        }

    }

    public async void Kaboom() {

        await ToSignal(GetTree().CreateTimer(maxTime), "timeout");

        QueueFree();

    }

}
