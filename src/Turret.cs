using Godot;
#nullable enable

public partial class Turret : Node2D
{

	[Flags]
	public enum TargetingType
	{
		First,
		Last,
		Closest,
		Furthest,
		Strongest,
		Weakest
	}

	[Export] float Cooldown = 100;
	[Export] float Damage = 10;

	[Export] public bool Active = true;

	List<creep> Targets = new();

	creep? Target;
	Castable? Spell;

	DateTime LastFireTime = DateTime.Now;

	[Export(PropertyHint.Enum, nameof(TargetingType))]
	public TargetingType Targeting = TargetingType.First;

	public Turret()
	{
		Spell = (new Fireball(this)) as Castable;
	}

	public override void _Ready()
	{
		var area = GetNode<Area2D>("Area2D");

		area.Connect("body_entered", new Callable(this, "BodyEntered"));
		area.Connect("body_exited", new Callable(this, "BodyExited"));
	}

	public override void _Process(double delta)
	{
		if (!Active)
			return;

		UpdateTarget();

		if (
			Target != null &&
			Spell != null &&
			LastFireTime.AddMilliseconds(Spell.Cooldown) < DateTime.Now
		)
		{
			LastFireTime = DateTime.Now;

			Spell.Cast(Target);
		}

		QueueRedraw();
	}

	private creep GetBestTargetFromTargetingType()
	{
		switch (Targeting)
		{
			case TargetingType.First:
				return Targets.First();
			case TargetingType.Last:
				return Targets.Last();
			case TargetingType.Closest:
				return Targets.OrderBy(x => Position.DistanceTo(x.Position)).First();
			case TargetingType.Furthest:
				return Targets.OrderBy(x => Position.DistanceTo(x.Position)).Last();
			case TargetingType.Strongest:
				return Targets.OrderBy(x => x.Health).Last();
			case TargetingType.Weakest:
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

	public void AddChildAtPosition(Node2D child, Vector2 position)
	{
		AddChild(child);
		child.GlobalPosition = position;
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
