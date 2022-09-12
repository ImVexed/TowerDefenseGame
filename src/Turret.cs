using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#nullable enable

public class Turret : Node2D
{
	[Export] float Cooldown = 200;
	[Export] float Damage = 10;
	
	List<creep> Targets = new();
	creep? Target;
	DateTime LastFire = DateTime.Now;



	public override void _Ready()
	{
		var area = GetNode<Area2D>("Area2D");

		area.Connect("body_entered", this,"BodyEntered");
		area.Connect("body_exited", this, "BodyExited");
	}

	public override void _Process(float delta)
	{
		UpdateTarget();
		
		if (Target != null && LastFire.AddMilliseconds(Cooldown) < DateTime.Now)
		{
			LastFire = DateTime.Now;
			Target.TakeDamage(Damage + (int)GD.RandRange(0, 100));
		}

		Update();
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
