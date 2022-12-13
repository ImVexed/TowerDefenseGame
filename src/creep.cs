using Godot;
using System;

public partial class creep : CharacterBody2D
{
	public Resistances Resistances = new Resistances();
	public BigRational MaxHealth = 1000;
	public BigRational Health;
	[Export] float MovementMultiplier = 100;
	[Export] float NavRadius = 0;
	[Export] bool NavAvoidance = true;
	[Export] bool NavOptimize = true;

	[Signal]
	public delegate void navigation_finishedEventHandler();

	[Signal]
	public delegate void diedEventHandler(creep creep);

	NavigationAgent2D? nav;

	Vector2? NavDestination;
	Vector2? NextNavPosition;

	Vector2[]? NavPath;

	Label? HealthBar;

	PackedScene DamageText = ResourceLoader.Load<PackedScene>("res://scenes/gui/damage_number.tscn");

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Health = MaxHealth;
		HealthBar = GetNode<Label>("Health");
		nav = GetNode<NavigationAgent2D>("NavigationAgent2D");
	// TODO: nav.Connect("path_changed",new Callable(this,"PathChanged")); 
	// TODO: nav.Connect("target_reached",new Callable(this,"TargetReached"));
		nav.Connect("navigation_finished",new Callable(this,"NavigationFinished"));
		nav.Connect("velocity_computed",new Callable(this,"VelocityComputed"));
		nav.MaxSpeed = MovementMultiplier;
		nav.Radius = NavRadius;
		nav.AvoidanceEnabled = NavAvoidance;
	}

	// "velocity_computed" NavAgent2D Signal
	public void VelocityComputed(Vector2 newVelocity)
	{
		Velocity = newVelocity;

		if (!nav!.IsTargetReached())
			MoveAndSlide();
		else
			GlobalPosition = NavigationServer2D.MapGetClosestPoint(nav!.GetNavigationMap(), GlobalPosition);
	}
	public override void _PhysicsProcess(double delta)
	{
		var nextPos = nav!.GetNextLocation();
		NextNavPosition = nextPos;
		var targetVelocity = GlobalPosition.DirectionTo(nextPos) * MovementMultiplier;
		nav?.SetVelocity(targetVelocity);
	}
	public void SetNavigationPosition(Vector2 pos)
	{
		NavDestination = pos;
		nav!.TargetLocation = pos;

		NavPath = NavigationServer2D.MapGetPath(nav!.GetNavigationMap(), GlobalPosition, pos, NavOptimize);
	}

	public void NavigationFinished()
	{
		EmitSignal("navigation_finished");
	}

	public void TakeDamage(Damage damage)
	{
		var resistedDamage = damage / Resistances;

		Health -= damage;

		if (Health <= 0) {
			QueueFree();
			EmitSignal("died", this);
			return;
		}

		var dmg = DamageText.Instantiate<damage_number>();
		dmg.Amount = damage;
		dmg.Type = damage.PrimaryDamageType();

		AddChild(dmg);
		QueueRedraw();
	}

	public override void _Draw()
	{
		HealthBar!.Text = $"{Health}/{MaxHealth}";
	}
}
