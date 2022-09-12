using Godot;
using System;

public class ruined_shrine : Node2D
{
	[Export] float SpawnRate = 1000;

	DateTime LastSpawn = DateTime.Now;

	PackedScene GameOverScene = ResourceLoader.Load<PackedScene>("res://scenes/gui/game_over.tscn");

	bool GameOver;
	Vector2 StartPos;
	Vector2 EndPos;

	TextureProgress HealthBar;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		StartPos = GetNode<Node2D>("Start Point").GlobalPosition;
		EndPos = GetNode<Node2D>("End Point").GlobalPosition;
		HealthBar = GetNode<TextureProgress>("../UI/Top Bar/Bars/HP/HPBar");
	}

	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{

		if (!GameOver && LastSpawn.AddMilliseconds(GD.RandRange(SpawnRate, SpawnRate + (SpawnRate * 0.25))) < DateTime.Now)
		{
			var creepBase = ResourceLoader.Load<PackedScene>("res://scenes/entities/creep.tscn");
			var newCreep = creepBase.Instance<creep>();
			newCreep.Connect("navigation_finished", this, "CreepLeaked");

			AddChild(newCreep);

			newCreep.GlobalPosition = StartPos;
			newCreep.SetNavigationPosition(EndPos);

			// 500, 450
			LastSpawn = DateTime.Now;
		}
	}

	public void CreepLeaked()
	{
		HealthBar.Value -= 10;

		if (HealthBar.Value <= 0)
		{
			if (!GameOver)
			{
				GameOver = true;
				var go = GameOverScene.Instance<Label>();
				go.SetPosition((GetViewportRect().Size/2)-(go.RectSize/2));
				AddChild(go);
			}
		}
	}
}
