using Godot;
using System;

public class projectile : Area2D
{
	[Export] public float Speed = 100;
	[Export] public float Damage = 100;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<VisibilityNotifier2D>("VisibilityNotifier2D").Connect("viewport_exited", this, "OnViewportExit");
		Connect("body_entered", this, "BodyEntered");
	}

	public override void _PhysicsProcess(float delta)
	{
		var direction = Vector2.Right.Rotated(Rotation);
		GlobalPosition += Speed * direction * delta;
	}

	public void OnViewportExit(Viewport vp)
	{
		QueueFree();
	}

	public void BodyEntered(Node body) 
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
		var velocity = c.GetFloorVelocity();

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
