using Godot;
using System;

public class ruined_shrine : Node2D
{
	[Export] float SpawnRate = 1000;

	DateTime LastSpawn = DateTime.Now;

	Vector2 StartPos;
	Vector2 EndPos;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		StartPos = GetNode<Node2D>("Start Point").GlobalPosition;
		EndPos = GetNode<Node2D>("End Point").GlobalPosition;
	}

	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		if (LastSpawn.AddMilliseconds(GD.RandRange(SpawnRate, SpawnRate+(SpawnRate*0.25))) < DateTime.Now)
		{
			var creepBase = ResourceLoader.Load<PackedScene>("res://creep.tscn");
			var newCreep = creepBase.Instance<creep>();

			AddChild(newCreep);
			
			newCreep.GlobalPosition = StartPos;
			newCreep.SetNavigationPosition(EndPos);

			// 500, 450
			LastSpawn = DateTime.Now;
		}
	}
}
