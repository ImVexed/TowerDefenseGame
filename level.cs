using Godot;
using System;

public class level : Navigation2D
{
	[Export] float SpawnRate = 1000;

	DateTime LastSpawn = DateTime.Now;
	Random rng = new Random();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		if (LastSpawn.AddMilliseconds(SpawnRate- rng.Next(200)) < DateTime.Now)
		{
			var c = new Character
			{
				Position = new Vector2(230, 150),
				Texture = ResourceLoader.Load<Texture>("res://character.png"),
				Scale = new Vector2(0.5f, 0.5f)
				
			};

			var area = new CollisionShape2D
			{
				Shape = new CircleShape2D
				{
					Radius = 10
				}
			};

			var col = new Area2D();
			col.AddChild(area);

			c.AddChild(col);

			AddChild(c);
			c.MoveTo(new Vector2(500, 450));
			// 500, 450
			LastSpawn = DateTime.Now;
		}
	}
}