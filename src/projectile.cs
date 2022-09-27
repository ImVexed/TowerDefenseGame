using Godot;
using System;

public partial class projectile : Area2D
{
	[Export] public float Speed = 600;
	[Export] public float Damage = 100;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<VisibleOnScreenNotifier2D>("VisibleOnScreenNotifier2D").Connect("screen_exited",new Callable(this,"OnViewportExit"));
		Connect("body_entered",new Callable(this,"BodyEntered"));
	}

	public override void _PhysicsProcess(double delta)
	{
		var direction = Vector2.Right.Rotated(Rotation);
		GlobalPosition = GlobalPosition + direction * (Speed * (float)delta);
	}

	public void OnViewportExit()
	{
		QueueFree();
	}

	public new void BodyEntered(Node body) 
	{
		if (body is not creep)
			return;

		var c = body as creep;

		c!.TakeDamage(Damage);
		QueueFree();
	}

	public void SetTarget(creep c)
	{
		var delta = c.GlobalPosition - GlobalPosition;
		var velocity = c.Velocity;

		var deltaTime = AimAhead(delta, velocity, Speed);

		if (deltaTime > 0) {
			var aimPoint = c.GlobalPosition + velocity *deltaTime;
			LookAt(aimPoint);
		} else {
			// nothing?
		}
	}

	// delta: relative position
	// vr: relative velocity
	// muzzleV: Speed of the bullet (muzzle velocity)
	// returns: Delta time when the projectile will hit, or -1 if impossible
	float AimAhead(Vector2 delta, Vector2 vr, float muzzleV)
	{
		// Quadratic equation coefficients a*t^2 + b*t + c = 0
		float a = vr.Dot(vr) - muzzleV * muzzleV;
		float b = 2f * vr.Dot(delta);
		float c = delta.Dot(delta);

		float desc = b * b - 4f * a * c;

		// If the discriminant is negative, then there is no solution
		if (desc > 0f)
		{
			return 2f * c / (Mathf.Sqrt(desc) - b);
		}
		else
		{
			return -1f;
		}
	}
}
