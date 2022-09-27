using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#nullable enable

public class Turret : Node2D
{
    [Flags]
    public enum TargetingType
    {
        First = 0,
        Last = 1,
        Closest = 2,
        Furthest = 3,
        Strongest = 4,
        Weakest = 5
    }

    [Export] float Cooldown = 100;
    [Export] float Damage = 10;

    List<creep> Targets = new();
    creep? Target;
    DateTime LastFire = DateTime.Now;

    [Export(PropertyHint.Enum, "First,Last,Closest,Furthest,Strongest,Weakest")]
    public int Targeting = (int)TargetingType.First;

    PackedScene ProjectileBase = ResourceLoader.Load<PackedScene>("res://scenes/entities/projectile.tscn");

    public override void _Ready()
    {
        var area = GetNode<Area2D>("Area2D");

        area.Connect("body_entered", this, "BodyEntered");
        area.Connect("body_exited", this, "BodyExited");
    }

    public override void _Process(float delta)
    {
        UpdateTarget();

        if (Target != null && LastFire.AddMilliseconds(Cooldown) < DateTime.Now)
        {
            LastFire = DateTime.Now;
            var proj = ProjectileBase.Instance<projectile>();
            proj.Speed = 600;
            proj.Damage = Damage + (int)GD.RandRange(0, 10);
            AddChild(proj);
            proj.SetTarget(Target);
        }

        Update();
    }

    private creep GetBestTargetFromTargetingType()
    {
        switch (Targeting)
        {
            case (int)TargetingType.First:
                return Targets.First();
            case (int)TargetingType.Last:
                return Targets.Last();
            case (int)TargetingType.Closest:
                return Targets.OrderBy(x => Position.DistanceTo(x.Position)).First();
            case (int)TargetingType.Furthest:
                return Targets.OrderBy(x => Position.DistanceTo(x.Position)).Last();
            case (int)TargetingType.Strongest:
                return Targets.OrderBy(x => x.Health).Last();
            case (int)TargetingType.Weakest:
                return Targets.OrderBy(x => x.Health).First();
            default:
                throw new ArgumentOutOfRangeException($"Invalid Targeting Type: {Targeting}");
        }
    }

    public override void _Draw()
    {
        if (Target != null)
            DrawLine(new Vector2(0, 0), Target.Position - Position, Color.Color8(255, 0, 0));
    }

    private void BodyEntered(Node body)
    {
		if (body is not creep)
			return;

		var s = (creep)body;

		Targets.Add(s);
		UpdateTarget();
    }

    private void BodyExited(Node body)
    {
        if (body is not creep)
            return;

        var s = (creep)body;

        if (s == Target)
            Target = null;


        Targets.Remove(s);
		UpdateTarget();
    }

    private void UpdateTarget()
    {
        if (Targets.Count == 0)
            return;

        Target = GetBestTargetFromTargetingType();
    }
}
