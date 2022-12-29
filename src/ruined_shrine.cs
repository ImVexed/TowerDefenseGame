using Godot;
using System;


public partial class ruined_shrine : Node2D
{
	bool GameOver;
	Vector2 StartPos;
	Vector2 EndPos;

	TextureProgressBar? HealthBar;
	TextureButton? BasicTowerButton;
	TextureButton? PauseButton;
	Label? FPSLabel;
	Label? WaveLabel;
	Label? GoldLabel;
	Label? CountdownLabel;

	bool ActivelyPlacingTower = false;
	turret? PlacementTower;

	WaveManager? WaveManager;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		StartPos = GetNode<Node2D>("Start Point").GlobalPosition;
		EndPos = GetNode<Node2D>("End Point").GlobalPosition;
		HealthBar = GetNode<TextureProgressBar>("../UI/HPBar");
		FPSLabel = GetNode<Label>("../UI/FPS");
		WaveLabel = GetNode<Label>("../UI/Wave");
		GoldLabel = GetNode<Label>("../UI/Gold");
		CountdownLabel = GetNode<Label>("../UI/Countdown");
		BasicTowerButton = GetNode<TextureButton>("../UI/BasicTower");
		BasicTowerButton.Pressed += TowerButtonPressed;
		PauseButton = GetNode<TextureButton>("../UI/Pause");
		PauseButton.Pressed += PauseButtonPressed;
		WaveManager = new WaveManager(Diffuculty.Medium, this, StartPos, EndPos);
		HealthBar!.MaxValue = WaveManager!.Health;
	}

	public void TowerButtonPressed()
	{
		if (WaveManager!.Gold < 100)
			return;

		WaveManager.Gold -= 100;

		ActivelyPlacingTower = true;
		PlacementTower = ResourceManager.NewTower();
		PlacementTower.Position = GetGlobalMousePosition();

		AddChild(PlacementTower);
	}

	public void PauseButtonPressed()
	{
		if (WaveManager!.State == WaveState.NotStarted)
		{
			WaveManager.Start();
			PauseButton!.TextureNormal = ResourceManager.PauseTexture;

		}
		else if (WaveManager.State == WaveState.Started)
		{
			WaveManager.Pause();
			PauseButton!.TextureNormal = ResourceManager.PlayTexture;
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (ActivelyPlacingTower)
		{
			if (@event is InputEventMouseButton eventMouseButton && PlacementTower!.PlacementValid)
			{
				PlacementTower!.Position = eventMouseButton.Position;
				PlacementTower.Placed = true;
				PlacementTower.QueueRedraw();
				PlacementTower = null;
				ActivelyPlacingTower = false;
			}
			else if (@event is InputEventMouseMotion eventMouseMotion)
			{
				PlacementTower!.Position = eventMouseMotion.Position;
				PlacementTower.QueueRedraw();
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		FPSLabel!.Text = $"{Engine.GetFramesPerSecond()} FPS";
		WaveManager?.Update();
		GoldLabel!.Text = $"{WaveManager?.Gold.ToString(0)} Gold";
		WaveLabel!.Text = $"Wave {WaveManager?.WaveNumber}";
		if (WaveManager?.NextWaveTime != default(DateTime))
			CountdownLabel!.Text = $"{(WaveManager?.NextWaveTime - DateTime.Now)}";
		
		HealthBar!.Value = WaveManager!.Health;
		
		if (HealthBar?.Value <= 0)
		{
			if (!GameOver)
			{
				GameOver = true;
				WaveManager!.Died();
				var go = ResourceManager.NewGameOverLabel();
				go.SetPosition((GetViewportRect().Size / 2) - (go.Size / 2));
				AddChild(go);
			}
		}
	}
}
