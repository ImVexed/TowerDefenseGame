using Godot;
using System;

public partial class projectile : Area2D
{
	[Export] public float Speed = 600;
	[Export] public bool FreeAfterHit = true;

	public BigRational RemainingChains = 0;
	public BigRational RemainingForks = 0;
	public BigRational RemainingPierces = 0;

	public OnHitCallback? OnHitCallback;
	public SpawnProjectileCallback? SpawnProjectileCallback;

	// Holds a list of creeps that have already been hit by this projectile
	public List<creep> CollidedCreeps = new List<creep>();

	Area2D? ChainArea;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<VisibleOnScreenNotifier2D>("VisibleOnScreenNotifier2D").Connect("screen_exited", new Callable(this, "OnViewportExit"));
		Connect("body_entered", new Callable(this, "BodyEntered"));
		ChainArea = GetNode<Area2D>("ChainArea2D");
	}

	public override void _PhysicsProcess(double delta)
	{
		var direction = Vector2.Right.Rotated(Rotation);
		GlobalPosition = GlobalPosition + direction * (Speed * (float)delta);
	}

	public void OnViewportExit()
	{
		CallDeferred("queue_free");
	}

	public creep? GetChainTarget() {
		var bodies = ChainArea!.GetOverlappingBodies().Cast<creep>();
		
		return bodies.Where(x => !CollidedCreeps.Contains(x)).OrderBy((x) => x.GlobalPosition.DistanceSquaredTo(GlobalPosition)).FirstOrDefault(defaultValue:null);
	}

	public void CopyState(projectile other)
	{
		Speed = other.Speed;
		FreeAfterHit = other.FreeAfterHit;
		RemainingChains = other.RemainingChains;
		RemainingForks = other.RemainingForks;
		RemainingPierces = other.RemainingPierces;
		OnHitCallback = other.OnHitCallback;
		SpawnProjectileCallback = other.SpawnProjectileCallback;
	}

	public new void BodyEntered(Node body)
	{
		var c = (creep)body;

		if (CollidedCreeps.Contains(c))
			return;

		CollidedCreeps.Add(c);

		var HadEffectRemaining = false;

		if (RemainingPierces > 0) {
			RemainingPierces -= 1;
			HadEffectRemaining = true;
		} else if (RemainingChains > 0) {
			RemainingChains -= 1;

			var target = GetChainTarget();

			if (target != null) {
				this.SetTarget(target.GlobalPosition, target.Velocity);
			}
			HadEffectRemaining = true;
		} else if (RemainingForks > 0) {
			RemainingForks -= 1;

			var leftForkAngle = Rotation - Mathf.Pi / 8;
			var rightForkAngle = Rotation + Mathf.Pi / 8;

			var leftFork = SpawnProjectileCallback!(c.GlobalPosition);
			leftFork.CopyState(this);
			leftFork.CollidedCreeps.Add(c); // We would instantly hit the same creep again if we didn't do this
			leftFork.Rotation = leftForkAngle;

			var rightFork = SpawnProjectileCallback!(c.GlobalPosition);
			rightFork.CopyState(this);
			rightFork.CollidedCreeps.Add(c); // We would instantly hit the same creep again if we didn't do this
			rightFork.Rotation = rightForkAngle;
		}

		OnHitCallback?.Invoke(c!);

		if (!HadEffectRemaining && FreeAfterHit)
		{
			CallDeferred("queue_free");
		}
	}

	public void SetTarget(Vector2 target, Vector2 target_velocity)
	{
		var delta = target - GlobalPosition;

		var deltaTime = AimAhead(delta, target_velocity, Speed);

		if (deltaTime > 0)
		{
			var aimPoint = target + target_velocity * deltaTime;
			LookAt(aimPoint);
		}
		else
		{
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
