using Godot;
using System;

public class damage_number : Position2D
{
	Label label;
	Tween tween;

	[Export] public float Ammount = 0;

	Vector2 velocity = Vector2.Zero;

	public override void _Ready()
	{
		GD.Randomize();

		label = GetNode<Label>("Label");
		tween = GetNode<Tween>("Tween");

		label.Text = Ammount.ToString();

		velocity -= new Vector2((float)GD.RandRange(0, 50), 50);

		// Scale up to full size in 0.2 seconds, then after 100ms scale back down to 0.1
		tween.InterpolateProperty(this, "scale", Scale, new Vector2(1, 1), 0.2f, Tween.TransitionType.Linear, Tween.EaseType.Out);
		tween.InterpolateProperty(this, "scale", new Vector2(1, 1),new Vector2(0.1f, 0.1f), 0.7f, Tween.TransitionType.Linear, Tween.EaseType.Out, 0.3f);
		tween.Start();
	}

	public void TweenCompleted()
	{
		QueueFree();
	}

	public override void _Process(float delta)
	{
		Position -= velocity * delta;
	}
}
