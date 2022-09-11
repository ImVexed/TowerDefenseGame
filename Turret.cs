using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Turret : Sprite
{
	List<Sprite> Targets = new List<Sprite>();
	Sprite? Target;
	float Cooldown = 100;
	DateTime LastFire = DateTime.Now;



	public override void _Ready()
	{
		var area = GetNode<Area2D>("Area2D");
		GD.Print(area);
		area.Connect("area_entered", this,"_on_Area2D_body_entered");
		area.Connect("area_exited", this, "_on_Area2D_body_exited");
	}

	public override void _Process(float delta)
	{
		UpdateTarget();
		
		if (Target != null)
		{
			if (LastFire.AddMilliseconds(Cooldown) < DateTime.Now)
			{
				LastFire = DateTime.Now;
				Target.EmitSignal("take_damage", 10);
			}
		}

		Update();
	}

	public override void _Draw()
	{
		if (Target != null)
		{
			DrawLine(new Vector2(0,0), Target.Position - Position, Color.Color8(255, 0, 0));
		}
	}

	private void _on_Area2D_body_entered(Area2D body)
	{
		Targets.Add(body.GetParent<Sprite>());
	
	}
	
	private void _on_Area2D_body_exited(Area2D body)
	{
		var s = body.GetParent<Sprite>();

		if (s == Target)
		{
			Target = null;
		}

		Targets.Remove(s);
	}

	private void UpdateTarget()
	{
		if (Targets.Count == 0)
		{
			return;
		}

		var closestTarget = Targets.First();

		foreach (var target in Targets)
		{
			if (Position.DistanceTo(target.Position) < Position.DistanceTo(closestTarget.Position))
			{
				closestTarget = target;
			}
		}
		Target = closestTarget;
	}
}
