using Godot;
using System;
using System.Linq;

public class Character : Sprite
{
	private readonly Font Font = new DynamicFont
	{
		FontData = ResourceLoader.Load<DynamicFontData>("res://Roboto-Regular.ttf")
	};

	[Export] float Health = 100;
	[Export] float MovementSpeed = 100;
	
	Vector2[]? Path;

	[Signal]
	public delegate void take_damage(float damage);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Connect("take_damage", this, "TakeDamage");
	}
	
	public void TakeDamage(float damage) 
	{
		Health -= damage;
		
		if (Health <=0) {
			QueueFree();
		}
		Update();
	}

	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		Update();

		if (Path != null)
		{
			var distance = MovementSpeed * delta;
			var lastPoint = Position;

			while (Path.Length > 0)
			{
				var distanceBetween = lastPoint.DistanceTo(Path[0]);

				if (distance <= distanceBetween)
				{
					Position = lastPoint.LinearInterpolate(Path[0], distance / distanceBetween);
					return;
				}

				distance -= distanceBetween;
				lastPoint = Path[0];
				Path = Path[1..];
			}

			Position = lastPoint;
			SetProcess(false);
		}
	}

	public void MoveTo(Vector2 pos)
	{
		Path = GetParent<Navigation2D>().GetSimplePath(GlobalPosition, pos)[1..];
		SetProcess(true);
	}
	
	public override void _Draw()
	{
		DrawString(Font, new Vector2(-30,-70), $"{Health}/100", Color.Color8(255,0,0));
	}
}
