using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#nullable enable

public partial class Turret : Node2D
{
	[Export] float Cooldown = 500;
	[Export] float Damage = 10;
	
	List<creep> Targets = new();
	creep? Target;
	DateTime LastFire = DateTime.Now;

	PackedScene ProjectileBase = ResourceLoader.Load<PackedScene>("res://scenes/entities/projectile.tscn");

	public override void _Ready()
	{
		var area = GetNode<Area2D>("Area2D");

		area.Connect("body_entered",new Callable(this,"BodyEntered"));
		area.Connect("body_exited",new Callable(this,"BodyExited"));
	}

	public override void _Process(double delta)
	{
		UpdateTarget();
		
		if (Target != null && LastFire.AddMilliseconds(Cooldown) < DateTime.Now)
		{
			LastFire = DateTime.Now;
			var proj = ProjectileBase.Instantiate<projectile>();
			proj.Speed = 600;
			proj.Damage = Damage + (int)GD.RandRange(0, 10);
			AddChild(proj);
			proj.SetTarget(Target);
		}

		QueueRedraw();
	}

	public override void _Draw()
	{
		if (Target != null)
			DrawLine(new Vector2(0,0), Target.Position - Position, Color.Color8(255, 0, 0));	
	}

	private void BodyEntered(Node body)
	{
		if (body is not creep)
			return;
		
		Targets.Add((creep)body);
	}
	
	private void BodyExited(Node body)
	{
		if (body is not creep)
			return;
		
		var s = (creep)body;

		if (s == Target)
			Target = null;


		Targets.Remove(s);
	}

	private void UpdateTarget()
	{
		if (Targets.Count == 0)
			return;


		var closestTarget = Targets.First();

		foreach (var target in Targets)
			if (Position.DistanceTo(target.Position) < Position.DistanceTo(closestTarget.Position))
				closestTarget = target;

		Target = closestTarget;
	}
}
