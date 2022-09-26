using Godot;
using System;

public class Inventory : Control
{
	[Export] public int Gold = 0;

	Label GoldLabel;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GoldLabel = GetNode<Label>("Label");

		GoldLabel.Text = Gold.ToString();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		GoldLabel.Text = Gold.ToString();
	}
}
