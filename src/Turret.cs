using Godot;
#nullable enable

public partial class turret : Node2D
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

	[Export] public bool Placed = false;
	[Export] public bool PlacementValid = true;
	Area2D? PlacementArea;

	List<creep> Targets = new();

	creep? Target;
	Castable? Spell;

	DateTime LastFireTime = DateTime.Now;

	[Export(PropertyHint.Enum, nameof(TargetingType))]
	public TargetingType Targeting = TargetingType.First;



	public turret()
	{
		Spell = (new Fireball(this)) as Castable;
	}

	public override void _Ready()
	{
		PlacementArea = GetNode<Area2D>("PlacementArea");
		var area = GetNode<Area2D>("RangeArea");

		area.BodyEntered += RangeEntered;
		area.BodyExited += RangeExited;

		PlacementArea.BodyEntered += PlacementEntered;
		PlacementArea.BodyExited += PlacementExited;
		PlacementArea.AreaEntered += PlacementEntered;
		PlacementArea.AreaExited += PlacementExited;

	}

	public override void _Process(double delta)
	{
		if (!Placed)
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
		// if (Target != null)
		// 	DrawLine(new Vector2(0, 0), Target.Position - Position, Color.Color8(255, 0, 0));
	}

	public void AddChildAtPosition(Node2D child, Vector2 position)
	{
		AddChild(child);
		child.GlobalPosition = position;
	}

	private void RangeEntered(Node body)
	{
		var s = (creep)body;

		Targets.Add(s);
		UpdateTarget();
	}

	private void RangeExited(Node body)
	{
		var s = (creep)body;

		if (s == Target)
			Target = null;


		Targets.Remove(s);
		UpdateTarget();
	}

	private void PlacementEntered(Node body)
	{
		if (Placed)
			return;

		PlacementValid = false;
		Modulate = new Color(1, 0, 0);
	}
	
	private void PlacementExited(Node body)
	{
		if (Placed)
			return;
			
		PlacementValid = !PlacementArea!.HasOverlappingBodies() && !PlacementArea.HasOverlappingAreas();
		if (PlacementValid)
			Modulate = new Color(1, 1, 1);
	}

	private void UpdateTarget()
	{
		if (Targets.Count == 0)
			return;

		Target = GetBestTargetFromTargetingType();
	}
}
