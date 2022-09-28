using Godot;
using System;


public partial class ruined_shrine : Node2D
{
	[Export] float SpawnRate = 100;

	DateTime LastSpawn = DateTime.Now;

	PackedScene GameOverScene = ResourceLoader.Load<PackedScene>("res://scenes/gui/game_over.tscn");
	PackedScene CreepBase = ResourceLoader.Load<PackedScene>("res://scenes/entities/creep.tscn");
	PackedScene TowerBase = ResourceLoader.Load<PackedScene>("res://scenes/entities/turret.tscn");
	bool GameOver;
	Vector2 StartPos;
	Vector2 EndPos;

	TextureProgressBar? HealthBar;
	TextureButton? BasicTowerButton;
	Label? FPSLabel;

	Label? GoldLabel;
	System.Numerics.BigInteger Gold = 5;

	bool ActivelyPlacingTower = false;
	Turret? PlacementTower;

	RandomNumberGenerator RNG = new();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		StartPos = GetNode<Node2D>("Start Point").GlobalPosition;
		EndPos = GetNode<Node2D>("End Point").GlobalPosition;
		HealthBar = GetNode<TextureProgressBar>("../UI/HPBar");
		FPSLabel = GetNode<Label>("../UI/FPS");
		GoldLabel = GetNode<Label>("../UI/Gold");
		BasicTowerButton = GetNode<TextureButton>("../UI/BasicTower");
		BasicTowerButton.Connect("pressed",new Callable(this,"TowerButtonPressed"));
	}

	public void TowerButtonPressed()
	{
		ActivelyPlacingTower = true;
		PlacementTower = TowerBase.Instantiate<Turret>();
		PlacementTower.Position = GetGlobalMousePosition();
		PlacementTower.Active = false;
		AddChild(PlacementTower);
	}

	public override void _Input(InputEvent @event)
	{
		if (ActivelyPlacingTower)
		{
			if (@event is InputEventMouseButton eventMouseButton){
				PlacementTower!.Position = eventMouseButton.Position;
				PlacementTower.QueueRedraw();
				PlacementTower.Active = true;
				PlacementTower = null;
				ActivelyPlacingTower = false;
			}
			else if (@event is InputEventMouseMotion eventMouseMotion){
				PlacementTower!.Position = eventMouseMotion.Position;
				PlacementTower.QueueRedraw();
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		FPSLabel!.Text = $"{Engine.GetFramesPerSecond()} FPS";
		if (!GameOver && LastSpawn.AddMilliseconds(GD.RandRange(SpawnRate, SpawnRate + (SpawnRate * 0.25))) < DateTime.Now)
		{
			var creepBase = ResourceLoader.Load<PackedScene>("res://scenes/entities/creep.tscn");
			var newCreep = CreepBase.Instantiate<creep>();
			newCreep.Connect("navigation_finished",new Callable(this,"CreepLeaked"));
			newCreep.Connect("died",new Callable(this,"CreepKilled"));

			AddChild(newCreep);

			newCreep.GlobalPosition = StartPos;
			newCreep.SetNavigationPosition(EndPos);

			// 500, 450
			LastSpawn = DateTime.Now;
		}
	}

	public void CreepKilled(creep creep)
	{
		Gold *= (int)((creep.MaxHealth / 10) * RNG.RandfRange(1.1f, 1.3f));
		GoldLabel!.Text = $"{Gold} Gold";
		
		GoldLabel!.QueueRedraw();
	}

	public void CreepLeaked()
	{
		HealthBar!.Value -= 10;

		if (HealthBar?.Value <= 0)
		{
			if (!GameOver)
			{
				GameOver = true;
				var go = GameOverScene.Instantiate<Label>();
				go.SetPosition((GetViewportRect().Size / 2) - (go.Size / 2));
				AddChild(go);
			}
		}
	}
}
