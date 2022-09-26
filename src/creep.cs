using Godot;
using System;

public class creep : KinematicBody2D
{
	[Export] public float MaxHealth = 100;
	[Export] public float Health;
	[Export] float MovementMultiplier = 100;
	[Export] float NavRadius = 0;
	[Export] bool NavAvoidance = true;
	[Export] bool NavOptimize = false;

	[Signal]
	public delegate void navigation_finished();

	[Signal]
	public delegate void died(creep creep);

	Vector2 Velocity = Vector2.Zero;
	NavigationAgent2D nav;

	Vector2? NavDestination;
	Vector2? NextNavPosition;

	Vector2[]? NavPath;

	Label HealthBar;

	PackedScene DamageText = ResourceLoader.Load<PackedScene>("res://scenes/gui/damage_number.tscn");

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Health = MaxHealth;
		HealthBar = GetNode<Label>("Health");
		nav = GetNode<NavigationAgent2D>("NavigationAgent2D");
	// TODO: nav.Connect("path_changed", this, "PathChanged"); 
	// TODO: nav.Connect("target_reached", this, "TargetReached");
		nav.Connect("navigation_finished", this, "NavigationFinished");
		nav.Connect("velocity_computed", this, "VelocityComputed");
		nav.MaxSpeed = MovementMultiplier;
		nav.Radius = NavRadius;
		nav.AvoidanceEnabled = NavAvoidance;
	}

	// "velocity_computed" NavAgent2D Signal
	public void VelocityComputed(Vector2 newVelocity)
	{
		Velocity = newVelocity;

		if (!nav.IsTargetReached())
			Velocity = MoveAndSlide(Velocity);
		else
			GlobalPosition = Navigation2DServer.MapGetClosestPoint(nav.GetNavigationMap(), GlobalPosition);
	}
	public override void _PhysicsProcess(float delta)
	{
		var nextPos = nav.GetNextLocation();
		NextNavPosition = nextPos;
		var targetVelocity = GlobalPosition.DirectionTo(nextPos) * MovementMultiplier;
		nav.SetVelocity(targetVelocity);
	}
	public void SetNavigationPosition(Vector2 pos)
	{
		NavDestination = pos;
		nav.SetTargetLocation(pos);

		NavPath = Navigation2DServer.MapGetPath(nav.GetNavigationMap(), GlobalPosition, pos, NavOptimize);
	}

	public void NavigationFinished()
	{
		EmitSignal("navigation_finished");
	}

	public void TakeDamage(float damage)
	{
		Health -= damage;

		if (Health <= 0) {
			QueueFree();
			EmitSignal("died", this);
			return;
		}

		var dmg = DamageText.Instance<damage_number>();
		dmg.Amount = damage;
		dmg.Type = damage_number.DamageType.Void;

		AddChild(dmg);
		Update();
	}

	public override void _Draw()
	{
		HealthBar.Text = $"{Health}/100";
	}
}
